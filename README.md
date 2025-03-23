
# NostaleDPSMeter

A simple DMGMeter for the MMORPG Nostale that decrypts the packet received from the server and calculates DMG for mobs based on the information provided to the player.

Project written using Tauri framework for the frontend and C# for the backend with [TauriDotNetBridge](https://github.com/plainionist/TauriDotNetBridge) 

## Getting Started
The initial setup is actually simple.

After downloading the project, install all dependencies with [pnpm](https://pnpm.io/installation/)
```
pnpm i
```

and that's all, now you're able to run app by command
```
pnpm run tauri dev
```

or just build it yourself
```
pnpm run tauri build
```

## Configuration
Most important configuration file can be found in `src-dotnet/NostaleDPSMeter.TauriPlugIn/Config.cs`

At this moment there's 2 variables:
* ServerIP - IP which program will sniff packets and decode them to receive proper information
```cs
public static string ServerIP = "51.210.194.102";
```
* MobsNameList, which is map of Mobs ID and their names. Based on this list, program will decide if DMG should be add to DMGMeter or no. At this moment name is optional, in future will be mostly used to save data.

```cs
public static Dictionary<string, string> MobsNameList = new Dictionary<string, string>
{  
	{ "24", "BabyDander" },
	{ "333", "Chicken" }
};


```

## Extra Information

* For program to work properly user need to remember about add executable to firewall, and also run it as administrator.

* Currently, DMGMeter only count Player dmg, if dmg comes from pets or dots, it doesnt count at all.