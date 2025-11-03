using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LeagueOfLegendsBot.AutoClient.Controller;
using Newtonsoft.Json.Linq;
using PoniLCU;
using Serilog;

namespace LeagueOfLegendsBot.AutoClient
{
    public class PlayerClientController
    {
        public static string ClientPath { get; set; } = @"D:\Riot Games\Riot Client\RiotClientServices.exe";
        public static string ClientParams { get; set; } = "--launch-product=league_of_legends --launch-patchline=live";
        public PlayerAccountController PlayerAccountController { get; set; }
        public PlayerSessionController PlayerSessionController { get; set; }
        public LeagueClient LeagueClient { get; set; }
        public PlayerClientController(){

        }
        public void Initialize()
        {
            // kills all riot processes for safety reasons
            Process[] LeagueClientProcess = Process.GetProcessesByName("LeagueClient");
            Process[] RiotClientUx = Process.GetProcessesByName("RiotClientUx");
            Process[] RiotClientServicesProcess = Process.GetProcessesByName("RiotClientServices");
            LeagueClientProcess.ToList().ForEach(x => { x.Kill(); });
            RiotClientUx.ToList().ForEach(x => { x.Kill(); });
            RiotClientServicesProcess.ToList().ForEach(x => { x.Kill(); });
            // starts process
            ProcessStartInfo startInfo = new ProcessStartInfo() {
                FileName = ClientPath,
                Arguments = ClientParams
            };
            Process.Start(startInfo);
            // waits for process to be acknowledged by the system
            WaitForProcessToStart("RiotClientUx").Wait();
            // creates and initializes session
            PlayerSessionController = new PlayerSessionController(this);
            PlayerSessionController.Initialize();
        }
        public async Task WaitForChatToConnect()
        {
            var Request = await LeagueClient.Request(LeagueClient.requestMethod.GET, "/lol-chat/v1/session");
            var RequestJsonDeserialized = JObject.Parse(Request);
            var ErrorCode = RequestJsonDeserialized["httpStatus"];
            if (ErrorCode != null) {
                await WaitForChatToConnect();
                return;
            }
        }
        public async Task WaitForProcessToStart(string processName)
        {
            bool ProcessRunning = false;
            while (!ProcessRunning) {
                Process[] processes = Process.GetProcessesByName(processName);
                if (processes.Length > 0) {
                    ProcessRunning = true;
                }
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
        }
    }
}