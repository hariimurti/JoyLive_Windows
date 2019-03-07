using Newtonsoft.Json;
using RestSharp;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace JoyLive
{
    internal class JoyLiveApi
    {
        private static int nextPage = 1;
        public string errorMessage;
        public bool isError;

        public JoyLiveApi()
        {
        }

        public async Task<UserInfo> GetUser(string id)
        {
            try
            {
                var client = new RestClient("http://app.joylive.tv");
                var request = new RestRequest($"user/GetUserInfo?uid={id}");
                request.AddHeader("Host", "app.joylive.tv");
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("User-Agent", "Gogo.Live 2.7.6");
                request.AddParameter("androidVersion", "9");
                request.AddParameter("packageId", "3");
                request.AddParameter("channel", "developer-default");
                request.AddParameter("version", "2.7.6");
                request.AddParameter("deviceName", "Google Pixel XL");
                request.AddParameter("platform", "android");
                request.AlwaysMultipartFormData = true;
                request.Method = Method.POST;

                var cts = new CancellationTokenSource();
                var response = await client.ExecuteTaskAsync(request, cts.Token);
                var content = response.Content;

                Match regex = Regex.Match(content, @"({[\s\S]+})");
                if (regex.Success)
                {
                    content = regex.Groups[1].Value;
                }

                try { File.WriteAllText("JoyUser.json", content); }
                catch (Exception ex) { Console.WriteLine(ex.Message); }

                var json = JsonConvert.DeserializeObject<JoyLiveUserData>(content);

                return json.data;
            }
            catch (Exception ex)
            {
                isError = true;
                Console.WriteLine(ex);
                errorMessage = ex.Message;
                return null;
            }
        }

        public async Task<User[]> GetRoomInfo()
        {
            try
            {
                var client = new RestClient("http://m.cblive.tv");
                var request = new RestRequest("index/getRoomInfo");
                request.AddParameter("page", nextPage);
                request.AddHeader("Host", "m.cblive.tv");
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Origin", "http://m.cblive.tv");
                request.AddHeader("Referer", "http://m.cblive.tv/");
                request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddHeader("Accept-Encoding", "gzip, deflate");
                request.AddHeader("Accept-Language", "en,id;q=0.9");
                request.AddHeader("X-Requested-With", "XMLHttpRequest");
                request.Method = Method.GET;

                var cts = new CancellationTokenSource();
                var response = await client.ExecuteTaskAsync(request, cts.Token);
                var content = response.Content;

                try { File.WriteAllText("JoyLive.json", content); }
                catch (Exception ex) { Console.WriteLine(ex.Message); }

                var json = JsonConvert.DeserializeObject<JoyLiveData>(content);

                nextPage++;

                return json.data.rooms;
            }
            catch (Exception ex)
            {
                isError = true;
                errorMessage = ex.Message;
                return null;
            }
        }

        public string GetNextPage()
        {
            return nextPage.ToString();
        }

        public static string GetCurrentPage()
        {
            return (nextPage - 1).ToString();
        }

        public async Task<User[]> Reset()
        {
            nextPage = 1;
            return await GetRoomInfo();
        }
    }
}