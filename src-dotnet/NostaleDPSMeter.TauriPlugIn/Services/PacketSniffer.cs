namespace NostaleDPSMeter.TauriPlugIn;

using TauriDotNetBridge.Contracts;


using System.Net.Sockets;
using System.Net;

using System.Text;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using System.Runtime.InteropServices;
using PcapDotNet.Packets.Http;
using PcapDotNet.Packets.Arp;
using DPSMeterData;

public class PacketSniffer(IEventPublisher publisher) : IHostedService
{
	IEventPublisher _publisher;

	public static Dictionary<string, PlayerInfo> PlayerList = new Dictionary<string, PlayerInfo>();
	static Dictionary<string, int> BossList = new Dictionary<string, int>();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
		await Task.Factory.StartNew(() => StartSniffing(publisher));
    }
	
	void StartSniffing(IEventPublisher publisher){
		_publisher = publisher;

		//listening to IPv4 interfaces
		var IPv4Addresses = Dns.GetHostEntry(Dns.GetHostName())
			.AddressList.Where(al => al.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
			.AsEnumerable();

		foreach (IPAddress ip in IPv4Addresses){
			Sniff(ip);
		}
			
	}
	void Sniff(IPAddress ip)
    {
		 Socket socket = new Socket(System.Net.Sockets.AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            socket.Bind(new IPEndPoint(ip, 0));
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);
            socket.IOControl(IOControlCode.ReceiveAll, new byte[4] { 1, 0, 0, 0 }, null);

            byte[] buffer = new byte[8192];
			
            Action<IAsyncResult> OnReceive = null;
            OnReceive = (ar) =>
            {

                ushort iphdrlen;
                IPV4_HDR ipHdr = ByteArrayToStructure<IPV4_HDR>(buffer);
                iphdrlen = (ushort)((ushort)(ipHdr.ip_header_len) * 4);

                int skipBytes = iphdrlen;

                byte[] newBuffer = buffer.Skip(skipBytes).ToArray();
                TcpHeader tcpheader = ByteArrayToStructure<TcpHeader>(newBuffer);

                if (new IPAddress(ipHdr.ip_srcaddr).ToString() == Config.ServerIP)
                {
                   ProcessData(buffer.Skip(iphdrlen+ tcpheader.data_offset * 4).ToArray(),
                       ( buffer.Length - tcpheader.data_offset * 4 - ipHdr.ip_header_len * 4));
                }


                buffer = new byte[8192]; 
                socket.BeginReceive(buffer, 0, 8192, SocketFlags.None,
                    new AsyncCallback(OnReceive), null); 
            };
            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None,
                    new AsyncCallback(OnReceive), null);
        }

        public  T ByteArrayToStructure<T>(byte[] byteArray) where T : struct
        {
            IntPtr ptr = Marshal.AllocHGlobal(byteArray.Length);

            try
            {
                Marshal.Copy(byteArray, 0, ptr, byteArray.Length);
                return Marshal.PtrToStructure<T>(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);

            }
        }

        public  void ProcessData(byte[] data, int Size)
        {
            foreach(string log in DecryptGamePacket(data, Size)) {
                HandlePacket(log);
            }

        }

        public  List<string> DecryptGamePacket(byte[] buf, int len)
        {
            List<string> output = new List<string>();
            StringBuilder currentPacket = new StringBuilder();
            char[] keys = { ' ', '-', '.', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'n' };
            int index = 0;
            byte currentByte, length, first, second;

            while (index < len)
            {
                currentByte = buf[index];
                index++;

                if (currentByte == 0xFF)
                {
                    output.Add(currentPacket.ToString());
                    currentPacket.Clear();
                    continue;
                }

                length = (byte)(currentByte & 0x7F);
                if ((currentByte & 0x80) != 0)
                {
                    while (length > 0)
                    {
                        if (index < len)
                        {
                            currentByte = buf[index];
                            index++;

                            first = (byte)(((currentByte & 0xF0) >> 4) - 1);
                            if (first < keys.Length && first != 0x6E)
                                currentPacket.Append(keys[first]);

                            if (length <= 1)
                                break;

                            second = (byte)((currentByte & 0x0F) - 1);
                            if (second < keys.Length && second != 0x6E)
                                currentPacket.Append(keys[second]);

                            length -= 2;
                        }
                        else
                        {
                            length--;
                        }
                    }
                }
                else
                {
                    while (length > 0)
                    {
                        if (index < len)
                        {
                            currentPacket.Append((char)(buf[index] ^ 0xFF));
                            index++;
                        }
                        length--;
                    }
                }
            }

            return output;
        }

        public void HandlePacket(string packet)
        {
            if (packet.StartsWith("su"))
            {
                HandlePacket_su(packet);
            }
            else if (packet.StartsWith("c_info"))
            {
                HandlePacket_c_info(packet);
            }
            else if (packet.StartsWith("in "))
            {
                HandlePacket_in(packet);
            }
        }

        public void HandlePacket_su(string packet)
        {
            string[] SplitPacket = packet.Split(' ');
			
			
            if (SplitPacket[1] == "1" && BossList.ContainsKey(SplitPacket[4]))
            {
                PlayerInfo playerInfo;
                if (!PlayerList.TryGetValue(SplitPacket[2], out playerInfo))
                {
                    //In future try to save data even if theres no nickname information about player
                }
                else
                {
                    playerInfo.AddDMG(Int32.Parse(SplitPacket[13]), (SplitPacket[14] == "3") ? true : false);
                    //Console.WriteLine("{0} {1} {2} {3}", playerInfo.Name,playerInfo.Dmg, playerInfo.Hits, playerInfo.CritHits);
					_publisher.Publish("update-playerinfo", playerInfo);
                }
                
            }
        }
        public void HandlePacket_c_info(string packet)
        {
            string[] SplitPacket = packet.Split(' ');
            if (!PlayerList.ContainsKey(SplitPacket[6]))
            {
                PlayerList.Add(SplitPacket[6], new PlayerInfo(SplitPacket[1], 0));
            }
        }
        public static void HandlePacket_in(string packet)
        {
            string[] SplitPacket = packet.Split(' ');
					
            //spawn player
            if (SplitPacket[1] == "1")
            {
                //!in 1 Splashed - 2762 80 66 2 0 1 0 9 2 221.119.266.85.224.276.-1.-1.-1.-1 100 100 0 -1 4 3 1 12 1 0 73 32 51.-1 DeusLoVult 15 0 0 0 0 70 3 0|0|0 0 0 10 0 0
                if (!PlayerList.ContainsKey(SplitPacket[4]))
                {
                    PlayerList.Add(SplitPacket[4], new PlayerInfo(SplitPacket[2], 0));
                }
            }
            // spawn mob
            else if (SplitPacket[1] == "3")
            {
                if (Config.MobsNameList.ContainsKey(SplitPacket[2])){
                    if(!BossList.ContainsKey(SplitPacket[3]))
                        BossList.Add(SplitPacket[3],0);
                }
            }
        }

        //Future plans, add auto save dps data after changing map or just try to detect when Mob is defeated
        public void HandlePacket_mapout(string packet)
        {

        }
}
