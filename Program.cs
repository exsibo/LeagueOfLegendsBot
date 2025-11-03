using LeagueOfLegendsBot.AutoClient;
using PoniLCU;
using Serilog;
using System;
using static PoniLCU.LeagueClient;

namespace LeagueOfLegendsBot
{
    public class Program
    {
        static void Main()
        {
            try {
                // creating logger
                Log.Logger = new LoggerConfiguration()
                    .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level:}] - {Message:lj}{NewLine}{Exception}")
                    .MinimumLevel.Information()
                .CreateLogger();
                PlayerClientController PlayerClientController = new PlayerClientController();
                PlayerClientController.Initialize();
                PlayerAccountController PlayerAccountController = new PlayerAccountController { 
                    PlayerClientController = PlayerClientController, 
                    Username = "Philipcoco38",
                    Password = "Hemligt66" 
                };
                PlayerClientController.PlayerAccountController = PlayerAccountController;
                PlayerClientController.PlayerSessionController.Login();
                Log.Error($"Account successfully logged in.");
                PlayerClientController.WaitForProcessToStart("LeagueClient").Wait();
                PlayerClientController.LeagueClient = new LeagueClient(credentials.cmd);
                PlayerClientController.WaitForChatToConnect().Wait();
                Log.Error($"Initialization done. ready for input...");
                PlayerAccountController.LoadAccountInfomations().Wait();
                Log.Error($"Account Informations loaded.");
            }
            catch (Exception ex) {
                Log.Fatal(ex, "Fatal Error Detected:");
            }
            Console.ReadKey();
        }

    }
}

/*
 
working normal acc: Philipcoco38 / Hemligt66 
banned acc: Ffyalleemaja / Xhx1cmByxeeFVG18

data = await LeagueClient.Request(requestMethod.GET, "/lol-summoner/v1/current-summoner");
var DataButJsonDeserialized = JObject.Parse(data);
var Request = await LeagueClient.Request(requestMethod.GET, $"/lol-store/v1/giftablefriends"); // full list with friends you can gift to
Thread.Sleep(5000);
 var Request = await LeagueClient.Request(requestMethod.GET, $"/lol-store/v1/catalog"); // full shop with all infos 
var response = await LeagueClient.Request(requestMethod.GET, $"/lol-banners/v1/current-summoner/flags");



                    Log.Error($"Sending friend request...");
                    var body = "{\"name\":\"" + username + "\"}";
                    var Request = LeagueClient.Request(requestMethod.POST, $"/lol-chat/v1/friend-requests", body).Result;
                    Log.Error(messageTemplate: $"Response: {Request}");
*/
