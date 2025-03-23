using DPSMeterData;

namespace NostaleDPSMeter.TauriPlugIn;

public class BaseController
{
    public void Reset(){
        foreach(var Player in PacketSniffer.PlayerList){
            Player.Value.ResetDMG();
        }
    }
}
