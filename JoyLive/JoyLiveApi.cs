using Newtonsoft.Json;
using RestSharp;
using System;
using System.IO;
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

        public async Task<User[]> GetRoomInfo()
        {
            return await Task.Run(() =>
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

                    var response = client.Post(request);
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
            });
        }

        public string GetNextPage()
        {
            return nextPage.ToString();
        }

        public async Task<User[]> Reset()
        {
            nextPage = 1;
            return await GetRoomInfo();
        }
    }
}