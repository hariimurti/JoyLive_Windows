using MangoLive.Json;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MangoLive
{
    internal class MangoApi
    {
        private static readonly string baseUrl = Configs.GetAppHost();
        private static readonly string cookiesFile = Path.Combine(App.WorkingDir, "MangoLive.dat");
        private static CookieContainer cookies = ReadCookies();

        private static CookieContainer ReadCookies()
        {
            try
            {
                using (Stream stream = File.Open(cookiesFile, FileMode.Open))
                {
                    var formatter = new BinaryFormatter();
                    return formatter.Deserialize(stream) as CookieContainer;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new CookieContainer();
            }
        }

        private static void WriteCookies(CookieContainer _cookies)
        {
            try
            {
                cookies = _cookies;
                using (Stream stream = File.Create(cookiesFile))
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, _cookies);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static async Task<string> ClientExecute(RestRequest request)
        {
            var client = new RestClient($"https://{baseUrl}")
            {
                CookieContainer = cookies,
                UserAgent = "okhttp/3.10.0",
                FollowRedirects = true
            };
            var cts = new CancellationTokenSource();
            var response = await client.ExecuteTaskAsync(request, cts.Token);

            WriteCookies(client.CookieContainer);
            cts.Dispose();

            return response.Content;
        }

        private static RestRequest RequestBuilder(string resource, Method method = Method.GET)
        {
            var request = new RestRequest(resource);
            request.AddHeader("androidVersion", "9");
            request.AddHeader("packageId", "41");
            request.AddHeader("channel", "developer-default");
            request.AddHeader("version", "3.0.0");
            request.AddHeader("deviceName", "Google Pixel 2 XL");
            request.AddHeader("platform", "android");
            request.AddHeader("Host", baseUrl);
            request.AddDecompressionMethod(DecompressionMethods.GZip);
            request.Method = method;
            return request;
        }

        private static bool isJsonFormat(string content)
        {
            return Regex.IsMatch(content, "{.*}");
        }

        public static async Task<GetMyInfo> GetMyInfo()
        {
            try
            {
                var request = RequestBuilder("user/GetMyInfo");
                var content = await ClientExecute(request);
                DumpFile.Write("GetMyInfo.json", content);

                if (!isJsonFormat(content))
                    throw new Exception("Response: not json format!");

                return JsonConvert.DeserializeObject<GetMyInfo>(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR : " + ex.Message);
                return new GetMyInfo()
                {
                    errno = -100,
                    msg = ex.Message
                };
            }
        }

        public static async Task<GetMyInfo> Login()
        {
            try
            {
                var _username = Configs.GetUsername();
                var _password = Configs.GetPassword();

                if (string.IsNullOrWhiteSpace(_username) || string.IsNullOrWhiteSpace(_password))
                    throw new Exception("Please fill the Username & Password!");

                var request = RequestBuilder("user/login", Method.POST);
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddParameter("application/x-www-form-urlencoded", $"remember=true&username={_username}&password={_password}", ParameterType.RequestBody);
                var content = await ClientExecute(request);
                DumpFile.Write("Login.json", content);

                if (!isJsonFormat(content))
                    throw new Exception("Response: not json format!");

                return JsonConvert.DeserializeObject<GetMyInfo>(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR : " + ex.Message);
                return new GetMyInfo()
                {
                    errno = -100,
                    msg = ex.Message
                };
            }
        }

        public static async Task<GetInfo> GetRoomInfo(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id)) throw new Exception("ID can't be null");

                var request = RequestBuilder($"room/GetInfo?rid={id}");
                var content = await ClientExecute(request);
                DumpFile.Write("GetInfo.json", content);

                if (!isJsonFormat(content))
                    throw new Exception("Response: not json format!");

                return JsonConvert.DeserializeObject<GetInfo>(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR : " + ex.Message);
                return new GetInfo()
                {
                    errno = -100,
                    msg = ex.Message
                };
            }
        }

        public static async Task<GetUserInfo> GetUser(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id)) throw new Exception("ID can't be null");

                var request = RequestBuilder($"user/GetUserInfo?uid={id}");
                var content = await ClientExecute(request);
                DumpFile.Write("GetUserInfo.json", content);

                if (!isJsonFormat(content))
                    throw new Exception("Response: not json format!");

                return JsonConvert.DeserializeObject<GetUserInfo>(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR : " + ex.Message);
                return new GetUserInfo()
                {
                    errno = -100,
                    msg = ex.Message
                };
            }
        }

        public static async Task<GetRooms> GetRooms(int page = 0, int status = 1)
        {
            try
            {
                var request = RequestBuilder($"room/GetRooms?page={page}&status={status}");
                var content = await ClientExecute(request);
                DumpFile.Write("GetRooms.json", content);

                if (!isJsonFormat(content))
                    throw new Exception("Response: not json format!");

                return JsonConvert.DeserializeObject<GetRooms>(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR : " + ex.Message);
                return new GetRooms()
                {
                    errno = -100,
                    msg = ex.Message
                };
            }
        }
    }
}