﻿using System;
using System.Net;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Net.Http;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MineChatAPI
{
    public class MojangAPIV2
    {
        private TimeSpan clientTimeout = TimeSpan.FromMilliseconds(10000);
        HttpClient httpPlayerInfoClient = new HttpClient();
        HttpClient httpGUIDClient = new HttpClient();
        HttpClient httpSkinClient = new HttpClient();

        public MojangAPIV2()
        {
            httpGUIDClient.Timeout = clientTimeout;
            httpPlayerInfoClient.Timeout = clientTimeout;
            httpSkinClient.Timeout = clientTimeout;
        }

        public static async Task<AuthResponse> Authenticate(string userName, string password, string clientToken)
        {
            AuthRequest auth = new AuthRequest();
            auth.username = userName;
            auth.password = password;

            if (clientToken == string.Empty)
            {
                clientToken = Guid.NewGuid().ToString();
                clientToken = clientToken.Replace("-", string.Empty);
            }
            auth.clienttoken = clientToken;

            string authString = JsonConvert.SerializeObject(auth);

            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromMilliseconds(10000);

            HttpContent content = new StringContent(authString, Encoding.UTF8, "application/json");

            Uri uri = new Uri("https://authserver.mojang.com/authenticate");

            AuthResponse authResponse = new AuthResponse();

            try
            {
                var postTask = client.PostAsync(uri, content);
                string result = postTask.Result.Content.ReadAsStringAsync().Result;

                if (postTask.Result.IsSuccessStatusCode)
                {
                    authResponse = JsonConvert.DeserializeObject<AuthResponse>(result);
                    authResponse.Result = RequestResult.Success;
                }
                else if(postTask.Result.StatusCode == HttpStatusCode.Forbidden)
                {
                    authResponse = JsonConvert.DeserializeObject<AuthResponse>(result);
                    authResponse.Result = RequestResult.Fail;
                    authResponse.Message = authResponse.errorMessage;
                }
                else
                {
                    authResponse.Result = RequestResult.Fail;
                    authResponse.Message = result;
                }
            }
            catch (TaskCanceledException)
            {
                throw new Exception("Connection timed out connecting to server");
            }

            catch (Exception ex)
            {
                throw ex;
            }

            return authResponse;

        }

        public static async Task<AuthResponse> Validate(string accessToken, string clientToken)
        {
            ValidateObject v = new ValidateObject();
            v.accessToken = accessToken;
            v.clientToken = clientToken;

            string authString = JsonConvert.SerializeObject(v);

            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromMilliseconds(10000);

            HttpContent content = new StringContent(authString, Encoding.UTF8, "application/json");
            Uri uri = new Uri("https://authserver.mojang.com/validate");

            AuthResponse authResponse = new AuthResponse();
            string result = string.Empty;

            try
            {
                var postTask = client.PostAsync(uri, content);
                result = postTask.Result.Content.ReadAsStringAsync().Result;

                if (postTask.Result.IsSuccessStatusCode)
                {
                    authResponse.Result = RequestResult.Success;
                }
                else
                {
                    authResponse.Result = RequestResult.Fail;
                    authResponse.Message = result;
                }
            }
            catch(TaskCanceledException)
            {
                throw new Exception("Connection timed out connecting to server");
            }
            catch(Exception ex)
            {
                throw ex;
            }

            return authResponse;
        }

        public static async Task<AuthResponse> Refresh(string accessToken, string clientToken, string id, string name)
        {

            RefreshRequest refresh = new RefreshRequest();
            refresh.accessToken = accessToken;
            refresh.clientToken = clientToken;

            string authString = JsonConvert.SerializeObject(refresh);

            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromMilliseconds(10000);

            HttpContent content = new StringContent(authString, Encoding.UTF8, "application/json");
            Uri uri = new Uri("https://authserver.mojang.com/refresh");
            AuthResponse authResponse = new AuthResponse();

            try
            {
                var postTask = client.PostAsync(uri, content);
                string result = postTask.Result.Content.ReadAsStringAsync().Result;

                if (postTask.Result.IsSuccessStatusCode)
                {
                    authResponse = JsonConvert.DeserializeObject<AuthResponse>(result);
                    authResponse.Result = RequestResult.Success;
                }
                else
                {
                    authResponse.Result = RequestResult.Fail;
                    authResponse.Message = result;
                }
            }
            catch (TaskCanceledException)
            {
                throw new Exception("Connection timed out connecting to server");
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return authResponse;            
        }

        private static HttpClient GetRealmsClient(string accessToken, string uuid, string ign)
        {
            var baseAddress = new Uri("https://pc.realms.minecraft.net");
            var cookieContainer = new CookieContainer();
            string token = String.Format("token:{0}:{1}", accessToken, uuid);
            cookieContainer.Add(baseAddress, new Cookie("sid", token));
            cookieContainer.Add(baseAddress, new Cookie("user", ign));
            cookieContainer.Add(baseAddress, new Cookie("version", "1.7.9"));

            var x = cookieContainer.GetCookies(baseAddress);

            var handler = new HttpClientHandler();
            handler.CookieContainer = cookieContainer;

            HttpClient client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromMilliseconds(10000);

            return client;
        }

        public static async Task<bool> GetRealmsAvailable(string accessToken, string uuid, string ign)
        {
            HttpClient client = GetRealmsClient(accessToken, uuid, ign);
            Uri uri = new Uri(@"https://pc.realms.minecraft.net/mco/available");

            try
            {
                string body = await client.GetStringAsync(uri);

                AuthResponse authResponse = new AuthResponse();

                //string result = getTask.Result.ToString();
                if (body.Trim() == string.Empty)
                {
                    return false;
                }

                return true;
            }
            catch (TaskCanceledException)
            {
                throw new Exception("Connection timed out connecting to server");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<string> GetRealmsServerAddress(string accessToken, string uuid, string ign, int serverId)
        {
            HttpClient client = GetRealmsClient(accessToken, uuid, ign);

            string url = String.Format("https://pc.realms.minecraft.net/worlds/v1/{0}/join/pc", serverId);
            Uri uri = new Uri(url);
            RealmsServerAddress address = null;
            string finalAddress = string.Empty;

            try
            {
                await client.GetAsync(uri).ContinueWith(getTask =>
                {
                    string body = getTask.Result.Content.ReadAsStringAsync().Result;
                    if (getTask.Result.IsSuccessStatusCode)
                    {
                        address = JsonConvert.DeserializeObject<RealmsServerAddress>(body);
                        finalAddress = address.address;
                    }
                    else
                    {
                        if (getTask.Result.StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            Task.Delay(3000).ContinueWith(_ =>
                            {
                                finalAddress = GetRealmsServerAddress(accessToken, uuid, ign, serverId).Result;
                            }).Wait();
                        }
                        else if (IsValidateJSON(body))
                        {
                            RequestError error = JsonConvert.DeserializeObject<RequestError>(body);
                            throw new Exception(error.errorMsg);
                        }
                        else
                        {
                            throw new Exception(getTask.Result.ReasonPhrase);
                        }
                    }

                });

                return finalAddress;

            }
            catch (Exception ex)
            {
                throw ex.GetBaseException();
            }
        }

        public static async Task<RealmsServers> GetRealmsWorlds(string accessToken, string uuid, string ign)
        {
            HttpClient client = GetRealmsClient(accessToken, uuid, ign);
            Uri uri = new Uri(@"https://pc.realms.minecraft.net/worlds");

            try
            {
                string result = await client.GetStringAsync(uri);
                RealmsServers realmsServers = new RealmsServers();

                realmsServers = JsonConvert.DeserializeObject<RealmsServers>(result);

                return realmsServers;
            }
            catch (TaskCanceledException)
            {
                throw new Exception("Connection timed out connecting to server");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<PlayerInfo> GetPlayerInfo(string uuid, string name)
        {
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromMilliseconds(10000);

            Uri uri = new Uri("https://sessionserver.mojang.com/session/minecraft/profile/" + uuid);


            var result = await client.GetStringAsync(uri);

            PlayerInfo playerInfo = null;

            if (string.IsNullOrEmpty(result))
            {
                playerInfo = new PlayerInfo();
                playerInfo.id = uuid;
                playerInfo.name = name;
            }
            else
            {
                playerInfo = JsonConvert.DeserializeObject<PlayerInfo>(result);

                string textures = playerInfo.properties.Find(p => p.name == "textures").value;
                byte[] textureBytes = System.Convert.FromBase64String(textures);
                textures = System.Text.Encoding.UTF8.GetString(textureBytes, 0, textureBytes.Length);
                playerInfo.textureObject = JsonConvert.DeserializeObject<TexturesObject>(textures);

                if (playerInfo.textureObject.Textures.Skin == null)
                {
                    playerInfo.textureObject.Textures.Skin = new Skin();
                }
            }

            return playerInfo;
        }

        public static bool IsValidateJSON(string s)
        {
            try
            {
                JToken.Parse(s);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }


    public class RealmsServers
    {
        public List<RealmsServer> servers { get; set; }
    }

    public class RealmsServerAddress
    {
        public string address { get; set; }
        public string resourcePackUrl { get; set; }
        public string resourcePackHash { get; set; }
    }

    public class RealmsServer
    {
        public int id { get; set; }
        public string remoteSubscriptionId { get; set; }
        public string owner { get; set; }
        public string ownerUUID { get; set; }
        public string name { get; set; }
        public string motd { get; set; }
        public string state { get; set; }
        public int daysLeft { get; set; }
        public bool expired { get; set; }
        public bool expiredTrial { get; set; }
        public string worldType { get; set; }
        public Players players { get; set; }
        public int maxPlayers { get; set; }
        public string minigameName { get; set; }
        public string minigameId { get; set; }
        public string minigameImage { get; set; }
        public int activeSlot { get; set; }
        public string slots { get; set; }
        public bool member { get; set; }
    }

    public class RefreshRequest
    {
        public string clientToken { get; set; }
        public string accessToken { get; set; }
        public bool requestUser { get; set; } = true;
    }

    public class RequestError
    {
        public string errorCode {get; set;}
        public string errorMsg { get; set; }
    }

    public class Players
    {
        public List<string> players { get; set; }
    }
}