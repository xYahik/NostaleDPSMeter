
namespace DPSMeterData
{
    public class PlayerInfo
    {
        public string Name;
        public int Dmg;
        public int Hits;
        public int CritHits;

        public PlayerInfo(string name, int dmg)
        {
            Name = name;
            Dmg = dmg;
            Hits = 0;
            CritHits = 0;
        }
        public void AddDMG(int dmg, bool wasCrit = false)
        {
            Dmg += dmg;
            Hits += 1;
            if(wasCrit)
            CritHits += 1;
        }
        public void ResetDMG(){
            Dmg = 0;
            Hits = 0;
            CritHits = 0;
        }
    }


    public struct IPV4_HDR
    {
       
        private byte _ipHeaderByte;
        // Property for ip_header_len (4 bits) // 4-bit header length (in 32-bit words) normally=5 (Means 20 Bytes may be 24 also)
        
        public byte ip_header_len
        {
            get => (byte)(_ipHeaderByte & 0x0F); // Mask the upper 4 bits and return the lower 4 bits
            set => _ipHeaderByte = (byte)((_ipHeaderByte & 0xF0) | (value & 0x0F)); // Clear the lower 4 bits and set the value
        }

        // Property for ip_version (4 bits)

        public byte ip_version
        {
            get => (byte)((_ipHeaderByte >> 4) & 0x0F); // Right shift by 4 and mask to get the upper 4 bits
            set => _ipHeaderByte = (byte)((_ipHeaderByte & 0x0F) | ((value & 0x0F) << 4)); // Clear the upper 4 bits and set the value
        }

       
        public byte ip_tos;  // IP type of service

       
        public ushort ip_total_length;  // Total length

        
        public ushort ip_id;  // Unique identifier
      
        private byte _ipFlagsByte;

        // Fragment Offset (5 bits)

        public byte ip_frag_offset
        {
            get => (byte)(_ipFlagsByte & 0x1F); // Mask the upper 3 bits and return the lower 5 bits
            set => _ipFlagsByte = (byte)((_ipFlagsByte & 0xE0) | (value & 0x1F)); // Clear the lower 5 bits and set the value
        }

        // More Fragment Flag (1 bit)

        public bool ip_more_fragment
        {
            get => (_ipFlagsByte & 0x20) != 0; // Check the 6th bit
            set => _ipFlagsByte = (byte)((_ipFlagsByte & 0xDF) | (value ? 0x20 : 0)); // Set or clear the 6th bit
        }

        // Don't Fragment Flag (1 bit)

        public bool ip_dont_fragment
        {
            get => (_ipFlagsByte & 0x40) != 0; // Check the 7th bit
            set => _ipFlagsByte = (byte)((_ipFlagsByte & 0xBF) | (value ? 0x40 : 0)); // Set or clear the 7th bit
        }

        // Reserved Zero Flag (1 bit)

        public bool ip_reserved_zero
        {
            get => (_ipFlagsByte & 0x80) != 0; // Check the 8th bit (most significant bit)
            set => _ipFlagsByte = (byte)((_ipFlagsByte & 0x7F) | (value ? 0x80 : 0)); // Set or clear the 8th bit
        }


       
        public byte ip_frag_offset1;  // Fragment offset (remaining part)


       
        public byte ip_ttl;  // Time to live


       
        public byte ip_protocol;  // Protocol (TCP, UDP, etc.)


       
        public ushort ip_checksum;  // IP checksum


       
        public uint ip_srcaddr;  // Source address


       
        public uint ip_destaddr;  // Destination address
    }



    public struct TcpHeader
    {
        public ushort source_port; // source port
        public ushort dest_port; // destination port
        public uint sequence; // sequence number - 32 bits
        public uint acknowledge; // acknowledgement number - 32 bits

        // Bit fields are represented by byte and we can manually manipulate them using bitwise operations
        //public byte ns_reserved_part1_data_offset; // combining ns, reserved_part1, and data_offset
        private byte ns_reserved_part1_data_offset; // 1 byte (8 bits) to hold all the flags and fields

        // Nonce Sum Flag (1 bit)
        public bool ns
        {
            get => (ns_reserved_part1_data_offset & 0x01) != 0; // Check the least significant bit (0x01)
            set => ns_reserved_part1_data_offset = (byte)((ns_reserved_part1_data_offset & 0xFE) | (value ? 0x01 : 0)); // Set or clear the bit
        }

        // Reserved Part 1 (3 bits)
        public byte reserved_part1
        {
            get => (byte)((ns_reserved_part1_data_offset >> 1) & 0x07); // Shift right by 1 and mask the lower 3 bits (0x07)
            set => ns_reserved_part1_data_offset = (byte)((ns_reserved_part1_data_offset & 0xF8) | (value & 0x07)); // Mask the lower 3 bits and set the new value
        }

        // Data Offset (4 bits)
        public byte data_offset
        {
            get => (byte)((ns_reserved_part1_data_offset >> 4) & 0x0F); // Shift right by 4 and mask the lower 4 bits (0x0F)
            set => ns_reserved_part1_data_offset = (byte)((ns_reserved_part1_data_offset & 0x0F) | ((value & 0x0F) << 4)); // Mask the upper 4 bits and set the new value
        }


        // Flags as individual bits
        //public byte flags; // combining fin, syn, rst, psh, ack, urg, ecn, cwr
        private byte _flags; // 1 byte to store all the flags (8 bits)

        // Finish Flag (1 bit)
        public bool Fin
        {
            get => (_flags & 0x01) != 0; // Check if the least significant bit is 1
            set => _flags = (byte)((_flags & 0xFE) | (value ? 0x01 : 0)); // Set or clear the least significant bit
        }

        // Synchronize Flag (1 bit)
        public bool Syn
        {
            get => (_flags & 0x02) != 0; // Check if the second bit is 1
            set => _flags = (byte)((_flags & 0xFD) | (value ? 0x02 : 0)); // Set or clear the second bit
        }

        // Reset Flag (1 bit)
        public bool Rst
        {
            get => (_flags & 0x04) != 0; // Check if the third bit is 1
            set => _flags = (byte)((_flags & 0xFB) | (value ? 0x04 : 0)); // Set or clear the third bit
        }

        // Push Flag (1 bit)
        public bool Psh
        {
            get => (_flags & 0x08) != 0; // Check if the fourth bit is 1
            set => _flags = (byte)((_flags & 0xF7) | (value ? 0x08 : 0)); // Set or clear the fourth bit
        }

        // Acknowledgement Flag (1 bit)
        public bool Ack
        {
            get => (_flags & 0x10) != 0; // Check if the fifth bit is 1
            set => _flags = (byte)((_flags & 0xEF) | (value ? 0x10 : 0)); // Set or clear the fifth bit
        }

        // Urgent Flag (1 bit)
        public bool Urg
        {
            get => (_flags & 0x20) != 0; // Check if the sixth bit is 1
            set => _flags = (byte)((_flags & 0xDF) | (value ? 0x20 : 0)); // Set or clear the sixth bit
        }

        // ECN-Echo Flag (1 bit)
        public bool Ecn
        {
            get => (_flags & 0x40) != 0; // Check if the seventh bit is 1
            set => _flags = (byte)((_flags & 0xBF) | (value ? 0x40 : 0)); // Set or clear the seventh bit
        }

        // Congestion Window Reduced Flag (1 bit)
        public bool Cwr
        {
            get => (_flags & 0x80) != 0; // Check if the eighth bit is 1
            set => _flags = (byte)((_flags & 0x7F) | (value ? 0x80 : 0)); // Set or clear the eighth bit
        }

        public ushort window; // window
        public ushort checksum; // checksum
        public ushort urgent_pointer; // urgent pointer

        // Property to extract individual flag bits
        //public bool ns => (ns_reserved_part1_data_offset & 0x80) != 0;
        //public byte reserved_part1 => (byte)((ns_reserved_part1_data_offset >> 4) & 0x07);
       // public byte data_offset => (byte)(ns_reserved_part1_data_offset & 0x0F);

        // Flags extraction
       /* public bool fin => (flags & 0x01) != 0;
        public bool syn => (flags & 0x02) != 0;
        public bool rst => (flags & 0x04) != 0;
        public bool psh => (flags & 0x08) != 0;
        public bool ack => (flags & 0x10) != 0;
        public bool urg => (flags & 0x20) != 0;
        public bool ecn => (flags & 0x40) != 0;
        public bool cwr => (flags & 0x80) != 0;*/
    }


}
