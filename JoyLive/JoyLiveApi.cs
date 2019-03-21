﻿using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace JoyLive
{
    internal class JoyLiveApi
    {
        public static readonly string BaseMobileUrl = "http://m.joylive.tv";
        public static readonly string BaseAppUrl = "http://app.joylive.tv";
        public static readonly string UserAgentGogoLive = "Gogo.Live 2.7.6";
        public static readonly string UserAgentBrowser = "Mozilla/5.0 (Linux; Android 9; Pixel 2 XL) " +
            "AppleWebKit/537.36 (KHTML, like Gecko) " +
            "Chrome/71.0.3578.98 Mobile Safari/537.36";

        private static int nextPage = 1;
        public string errorMessage;
        public bool isError;

        public JoyLiveApi()
        {
        }

        private void ResetApi()
        {
            isError = false;
            errorMessage = string.Empty;
        }

        public async Task<bool> isLoggedIn()
        {
            try
            {
                ResetApi();
                var client = new RestClient(BaseAppUrl);
                client.CookieContainer = ReadCookies();
                var request = new RestRequest($"user/GetMyInfo");
                request.AddHeader("Host", BaseAppUrl.Replace("http://", ""));
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("User-Agent", UserAgentGogoLive);
                request.AddParameter("androidVersion", "9");
                request.AddParameter("packageId", "3");
                request.AddParameter("channel", "developer-default");
                request.AddParameter("version", "2.7.6");
                request.AddParameter("deviceName", "Google Pixel 2 XL");
                request.AddParameter("platform", "android");
                request.AlwaysMultipartFormData = true;
                request.Method = Method.POST;

                var cts = new CancellationTokenSource();
                var response = await client.ExecuteTaskAsync(request, cts.Token);
                var content = response.Content;

                WriteCookies(client.CookieContainer);
                cts.Dispose();

                Match regex = Regex.Match(content, @"({[\s\S]+})");
                if (regex.Success)
                {
                    content = regex.Groups[1].Value;
                }

                var json = JsonConvert.DeserializeObject<JoyLiveUserInfo>(content);

                return (json.errno != 100);
            }
            catch (Exception ex)
            {
                isError = true;
                Console.WriteLine(ex);
                errorMessage = ex.Message;
                return false;
            }
        }

        public async Task<bool> Login()
        {
            try
            {
                ResetApi();
                var client = new RestClient(BaseAppUrl);
                var request = new RestRequest($"user/login");
                client.CookieContainer = ReadCookies();
                request.AddHeader("Host", BaseAppUrl.Replace("http://", ""));
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("User-Agent", UserAgentGogoLive);
                request.AddParameter("androidVersion", "9");
                request.AddParameter("packageId", "3");
                request.AddParameter("channel", "developer-default");
                request.AddParameter("version", "2.7.6");
                request.AddParameter("deviceName", "Google Pixel 2 XL");
                request.AddParameter("platform", "android");
                request.AddParameter("remember", "true");
                request.AddParameter("username", "62895377348858");
                request.AddParameter("password", "d1jJ8nMd50wzneu3MA2q6JpQuJj5UrbA2RSwCsbtRrEZY1ER2oe/" +
                    "ZckJMLtwLCKs7YyzY/IEOO4+Xa1NORn8HLZTtQfgHxK4I5ZNOGJsU6aWxuW7Zqr57/" +
                    "aNZ9epluxpkUu+o3rFtYJWkABUB8rGz70Xzs3J4LB3SnCT2zqKccc=");
                request.AlwaysMultipartFormData = true;
                request.Method = Method.POST;

                var cts = new CancellationTokenSource();
                var response = await client.ExecuteTaskAsync(request, cts.Token);
                var content = response.Content;

                WriteCookies(client.CookieContainer);
                cts.Dispose();

                Match regex = Regex.Match(content, @"({[\s\S]+})");
                if (regex.Success)
                {
                    content = regex.Groups[1].Value;
                }

                var json = JsonConvert.DeserializeObject<JoyLiveLogin>(content);

                return (json.errno == 0) && !string.IsNullOrWhiteSpace(json.data.uid);
            }
            catch (Exception ex)
            {
                isError = true;
                Console.WriteLine(ex);
                errorMessage = ex.Message;
                return false;
            }
        }

        public async Task<RoomInfo> GetRoomInfo(string id)
        {
            try
            {
                ResetApi();
                if (string.IsNullOrWhiteSpace(id)) throw new Exception("ID can't be null");

                var client = new RestClient(BaseAppUrl);
                client.CookieContainer = ReadCookies();
                var request = new RestRequest($"room/GetInfo?rid={id}");
                request.AddHeader("Host", BaseAppUrl.Replace("http://", ""));
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("User-Agent", UserAgentGogoLive);
                request.AddParameter("androidVersion", "9");
                request.AddParameter("packageId", "3");
                request.AddParameter("channel", "developer-default");
                request.AddParameter("version", "2.7.6");
                request.AddParameter("deviceName", "Google Pixel 2 XL");
                request.AddParameter("platform", "android");
                request.AlwaysMultipartFormData = true;
                request.Method = Method.POST;

                var cts = new CancellationTokenSource();
                var response = await client.ExecuteTaskAsync(request, cts.Token);
                var content = response.Content;

                WriteCookies(client.CookieContainer);
                cts.Dispose();

                Match regex = Regex.Match(content, @"({[\s\S]+})");
                if (regex.Success)
                {
                    content = regex.Groups[1].Value;
                }

                try { File.WriteAllText("JoyRoom.json", content); }
                catch (Exception ex) { Console.WriteLine(ex.Message); }

                var json = JsonConvert.DeserializeObject<JoyLiveRoomInfo>(content);

                return json.data;
            }
            catch (Exception ex)
            {
                isError = true;
                Console.WriteLine(ex);
                errorMessage = ex.Message;
                return new RoomInfo();
            }
        }

        public async Task<UserInfo> GetUser(string id)
        {
            try
            {
                ResetApi();
                if (string.IsNullOrWhiteSpace(id)) throw new Exception("ID can't be null");

                var client = new RestClient(BaseAppUrl);
                client.CookieContainer = ReadCookies();
                var request = new RestRequest($"user/GetUserInfo?uid={id}");
                request.AddHeader("Host", BaseAppUrl.Replace("http://",""));
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("User-Agent", UserAgentGogoLive);
                request.AddParameter("androidVersion", "9");
                request.AddParameter("packageId", "3");
                request.AddParameter("channel", "developer-default");
                request.AddParameter("version", "2.7.6");
                request.AddParameter("deviceName", "Google Pixel 2 XL");
                request.AddParameter("platform", "android");
                request.AlwaysMultipartFormData = true;
                request.Method = Method.POST;

                var cts = new CancellationTokenSource();
                var response = await client.ExecuteTaskAsync(request, cts.Token);
                var content = response.Content;

                WriteCookies(client.CookieContainer);
                cts.Dispose();

                Match regex = Regex.Match(content, @"({[\s\S]+})");
                if (regex.Success)
                {
                    content = regex.Groups[1].Value;
                }

                try { File.WriteAllText("JoyUser.json", content); }
                catch (Exception ex) { Console.WriteLine(ex.Message); }

                var json = JsonConvert.DeserializeObject<JoyLiveUserInfo>(content);

                return json.data;
            }
            catch (Exception ex)
            {
                isError = true;
                Console.WriteLine(ex);
                errorMessage = ex.Message;
                return new UserInfo();
            }
        }

        public async Task<User[]> GetRoomInfo()
        {
            try
            {
                ResetApi();
                var client = new RestClient(BaseMobileUrl);
                client.CookieContainer = ReadCookies();
                var request = new RestRequest("index/getRoomInfo");
                request.AddParameter("page", nextPage);
                request.AddHeader("Host", BaseMobileUrl.Replace("http://",""));
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Origin", BaseMobileUrl);
                request.AddHeader("Referer", $"{BaseMobileUrl}/");
                request.AddHeader("User-Agent", UserAgentBrowser);
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddHeader("Accept-Encoding", "gzip, deflate");
                request.AddHeader("Accept-Language", "en,id;q=0.9");
                request.AddHeader("X-Requested-With", "XMLHttpRequest");
                request.Method = Method.GET;

                var cts = new CancellationTokenSource();
                var response = await client.ExecuteTaskAsync(request, cts.Token);
                var content = response.Content;

                WriteCookies(client.CookieContainer);
                cts.Dispose();

                try { File.WriteAllText("JoyLive.json", content); }
                catch (Exception ex) { Console.WriteLine(ex.Message); }

                var json = JsonConvert.DeserializeObject<JoyLiveRooms>(content);

                nextPage++;

                return json.data.rooms;
            }
            catch (Exception ex)
            {
                isError = true;
                errorMessage = ex.Message;
                return new List<User>().ToArray();
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

        private CookieContainer ReadCookies()
        {
            if (!File.Exists(App.CookiesFile)) return new CookieContainer();

            try
            {
                using (Stream stream = File.Open(App.CookiesFile, FileMode.Open))
                {
                    var formatter = new BinaryFormatter();
                    return formatter.Deserialize(stream) as CookieContainer;
                }
            }
            catch (Exception)
            {
                return new CookieContainer();
            }
        }

        private static void WriteCookies(CookieContainer cookies)
        {
            try
            {
                using (Stream stream = File.Create(App.CookiesFile))
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, cookies);
                }
            }
            catch (Exception) { }
        }
    }
}