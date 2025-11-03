using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LeagueOfLegendsBot.AutoClient.Controller
{
    public class PlayerSessionController
    {
        public PlayerClientController PlayerClientController { get; private set; }
        public string Port { get; set; }
        public string Token { get; set; }
        public PlayerSessionController(PlayerClientController playerClientController){
            PlayerClientController = playerClientController;
        }
        public void Initialize()
        {
            // authenticates and defines port / token
            string wmiQuery = string.Format("select CommandLine from Win32_Process where Name='{0}'", "RiotClientUx.exe");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(wmiQuery);
            ManagementObjectCollection retObjectCollection = searcher.Get();
            var data = retObjectCollection.Cast<ManagementBaseObject>().SingleOrDefault()["CommandLine"].ToString();
            var token = Regex.Match(data, "(?<=--remoting-auth-token=)([\\w-]+)").Value;
            Port = Regex.Match(data, "(?<=--app-port=)([\\w-]+)").Value;
            Token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"riot:{token}"));
        }
        public void Login()
        {
            // logs into acc
            PlayerClientController.PlayerSessionController.Reset();
            string auth = $"{{\"username\":\"{PlayerClientController.PlayerAccountController.Username}\"," +
                          $"\"password\": \"{PlayerClientController.PlayerAccountController.Password}\"," +
                          "\"persistLogin\": false" +
                          "}";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create($"https://127.0.0.1:{PlayerClientController.PlayerSessionController.Port}/rso-auth/v1/session/credentials");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "PUT";
            httpWebRequest.Headers.Add("Authorization", $"Basic {PlayerClientController.PlayerSessionController.Token}");
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream())) {
                streamWriter.Write(auth);
            }
            var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            switch (httpWebResponse.StatusCode) {
                case HttpStatusCode.Created:
                case HttpStatusCode.OK:
                    Debug.Write("PlayerSessionController: Account Login successfull.");
                    break;
                default:
                    Log.Error($"PlayerSessionController: Account Login error. {httpWebResponse.StatusCode}");
                    Login();
                    break;
            }
        }
        public void Reset()
        {
            // resets authentication
            ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            var httpWebRequest = (HttpWebRequest)WebRequest.Create($"https://127.0.0.1:{Port}/rso-auth/v2/authorizations");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.Headers.Add("Authorization", $"Basic {Token}");
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream())) {
                string json = "{\"" +
                              "clientId\":\"riot-client\"," +
                              "\"trustLevels\":[\"always_trusted\"]" +
                              "}";
                streamWriter.Write(json);
            }
            var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            switch (httpWebResponse.StatusCode) {
                case HttpStatusCode.Created:
                case HttpStatusCode.OK:
                    Debug.Write("PlayerSessionController: Authentication Reset successfull.");
                    break;
                default:
                    Log.Error($"PlayerSessionController: Authentication Reset error. {httpWebResponse.StatusCode}");
                    Reset();
                    break;
            }
        }
    }
}
