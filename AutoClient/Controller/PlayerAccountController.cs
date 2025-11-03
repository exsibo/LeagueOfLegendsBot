using Newtonsoft.Json;
using System.IO;
using System.Net;
using Serilog;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PoniLCU;
using System;

namespace LeagueOfLegendsBot.AutoClient
{    
    /*
    *  /riotclient/kill-and-restart-ux // restart ux client side
    *  /process-control/v1/process/quit // kill client entirely
    *  /riotclient/ux-minimize // minimize client
    *  /riotclient/ux-crash-count // returns client crash state (0 not crashed, 1 crashed)
    */
    public class PlayerAccountController
    {
        // account login
        public string Username{ get; set; } 
        public string Password { get; set; }
        // account details 
        public long SummonerID { get; set; }
        public string SummonerName { get; set; } = "N/A";
        public string Server { get; set; } = "N/A";
        public string PUUID { get; set; } = "N/A";
        public int BlueEssence { get; set; } = 0;
        public int RiotPoints { get; set; } = 0;
        public PlayerClientController PlayerClientController { get; set; } 

        public PlayerAccountController(){
        }

        public async Task LoadAccountInfomations()
        {
            try {
                var Request = await PlayerClientController.LeagueClient.Request(LeagueClient.requestMethod.GET, "/lol-summoner/v1/current-summoner");
                var RequestJsonDeserialized = JObject.Parse(Request);
                SummonerID = (long)RequestJsonDeserialized["accountId"];
                SummonerName = (string)RequestJsonDeserialized["displayName"];
                Server = (string)RequestJsonDeserialized["tagLine"];
                PUUID = (string)RequestJsonDeserialized["puuid"];
                Log.Error($"SummonerID: {SummonerID} / SummonerName: {SummonerName} / Server: {Server} / PUUID: {PUUID}");
                //await StoreStuff();
            }
            catch(Exception ex) {
                Log.Error($"ERROR: {ex}");
               await LoadAccountInfomations();
            }
        }

        public async Task StoreStuff()
        {
            var StoreStatusRequest = await PlayerClientController.LeagueClient.Request(LeagueClient.requestMethod.GET, "/lol-store/v1/status");
            var StoreStatusRequestJson = JObject.Parse(StoreStatusRequest);
            Log.Error($"Store status: (on = true / off = false) {(bool)StoreStatusRequestJson[propertyName: "storefrontIsRunning"]}");

            var StoreCatalog = await PlayerClientController.LeagueClient.Request(LeagueClient.requestMethod.GET, $"/lol-store/v1/catalog");
            Log.Error($"Store Catalog: {StoreCatalog}");
        }
    }
}