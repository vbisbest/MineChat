using System;
using System.Net;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Net.Http;
using System.Collections.Generic;
using System.Diagnostics;

namespace MineChatAPI
{
    public class MojangAPI
    {
        public delegate void GetUUIDCompleteHandler(UUIDResponse uuidResponse);
        public event GetUUIDCompleteHandler GetUUIDCompleted;

        public delegate void AuthenticateCompleteHandler(AuthResponse authResponse);
        public event AuthenticateCompleteHandler AuthenticateCompleted;

        public delegate void ValidateCompleteHandler(AuthResponse authResponse);
        public event ValidateCompleteHandler ValidateCompleted;

        public delegate void RefreshCompleteHandler(AuthResponse authResponse);
        public event RefreshCompleteHandler RefreshCompleted;

        public delegate void PlayerInfoCompleteHandler(PlayerInfo playerInfo);
        public event PlayerInfoCompleteHandler PlayerInfoCompleted;

        public delegate void PlayerSkinCompleteHandler(PlayerInfo playerInfo);
        public event PlayerSkinCompleteHandler PlayerSkinCompleted;

        public delegate void MojangAPIErrorHandler(Exception e);
        public event MojangAPIErrorHandler MojangAPIError;

        public delegate void CreateSessionCompleteHandler(AuthResponse authResponse);
        public event CreateSessionCompleteHandler CreateSessionComplete;

        private TimeSpan clientTimeout = TimeSpan.FromMilliseconds(10000);
        HttpClient httpPlayerInfoClient = new HttpClient();
        HttpClient httpGUIDClient = new HttpClient();
        HttpClient httpSkinClient = new HttpClient();
        
        public MojangAPI()
        {
            httpGUIDClient.Timeout = clientTimeout;
            httpPlayerInfoClient.Timeout = clientTimeout;
            httpSkinClient.Timeout = clientTimeout;
        }

        public void GetUUID(string ign)
        {
            try
            {
                Uri uri = new Uri("https://api.mojang.com/users/profiles/minecraft/" + ign);
                httpGUIDClient.GetStringAsync(uri).ContinueWith(responseTask =>
                {
                    try
                    {
                        UUIDResponse result = JsonConvert.DeserializeObject<UUIDResponse>(responseTask.Result);

                        if (result == null)
                        {
                            result = new UUIDResponse();
                            result.Result = RequestResult.Fail;
                            result.Message = "Minecraft player not found.  Make sure the player name is correct.";
                        }
                        else
                        {
                            result.Result = RequestResult.Success;
                        }

                        if (GetUUIDCompleted != null)
                        {
                            GetUUIDCompleted(result);
                        }
                    }
                    catch(AggregateException e)
                    {
                        Debug.WriteLine("Error gettig GUID " + e.InnerException.Message);
                        ThrowError(e.InnerException);
                    }
                    catch(Exception e)
                    {
                        Debug.WriteLine("Error gettig GUID " + e.Message);
                        ThrowError(e);
                    }
                });
            }
            catch(Exception ex)
            {
                ThrowError(ex);
            }
        }

        public void GetPlayerInfo(string uuid, string name)
        {
            try
            {
                Uri uri = new Uri("https://sessionserver.mojang.com/session/minecraft/profile/" + uuid);

                //Debug.WriteLine(uri);

                httpPlayerInfoClient.GetStringAsync(uri).ContinueWith(getTask =>
                {
                    PlayerInfo playerInfo = null;

                    try
                    {
                        string result = getTask.Result.ToString();

                        if (string.IsNullOrEmpty(result))
                        {
                            playerInfo = new PlayerInfo();
                            playerInfo.id = uuid;
                            playerInfo.name = name;

                            if (PlayerInfoCompleted != null)
                            {
                                PlayerInfoCompleted(playerInfo);
                            }                            
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

                            GetPlayerSkin(playerInfo);
                        }
                    }
                    catch (Exception e)
                    {
                        // ignore
                    }
                });
            }
            catch (Exception ex)
            {
                // ignore;
                Debug.WriteLine("Error gettig PlayerInfo " + ex.Message);
                ThrowError(ex);
            }
        }

        public void GetPlayerSkin(string playerName)
        {
            try
            {

                string pn = System.Net.WebUtility.UrlEncode(playerName);

                Uri uri = new Uri("http://skins.minecraft.net/MinecraftSkins/" + pn + ".png");

                httpSkinClient.GetByteArrayAsync(uri).ContinueWith(getTask =>
                {
                    PlayerInfo playerInfo = new PlayerInfo();
                    playerInfo.textureObject = new TexturesObject();
                    playerInfo.textureObject.Textures = new Textures();
                    playerInfo.textureObject.Textures.Skin = new Skin();

                    try
                    {
                        if (!getTask.IsFaulted)
                        {
                            byte[] bytes = getTask.Result;
                            playerInfo.textureObject.Textures.Skin.Bytes = System.Convert.ToBase64String(bytes);
                        }
                        else
                        {
                            //throw getTask.Exception;                            
                            playerInfo.textureObject.Textures.Skin.Bytes = null;
                        }
                    }
                    catch (Exception e)
                    {
                        playerInfo.textureObject.Textures.Skin.Bytes = null;
                    }
                    finally
                    {
                        if (PlayerInfoCompleted != null)
                        {
                            PlayerInfoCompleted(playerInfo);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                // ignore;
                Debug.WriteLine("Error gettig Skin " + ex.Message);
                //ThrowError(ex);
            }
        }
        
        public void GetPlayerSkin(PlayerInfo playerInfo)
        {
            try
            {
                // Does the player have a skin
                if(playerInfo.textureObject.Textures.Skin.URL == null)
                {
                    // no skin
                    if (PlayerInfoCompleted != null)
                    {
                        PlayerInfoCompleted(playerInfo);
                        return;
                    }
                }

                Uri uri = new Uri(playerInfo.textureObject.Textures.Skin.URL);

                httpSkinClient.GetByteArrayAsync(uri).ContinueWith(getTask =>
                {
                    try
                    {
                        byte[] bytes = getTask.Result;
                        playerInfo.textureObject.Textures.Skin.Bytes = System.Convert.ToBase64String(bytes);
                    }
                    catch(AggregateException e)
                    {
                        throw e.InnerException;
                    }
                    catch(Exception e)
                    {
                        throw e;
                    }
                    finally
                    {
                        if (PlayerInfoCompleted != null)
                        {
                            PlayerInfoCompleted(playerInfo);
                        }
                    }
                });
            }              
            catch (Exception ex)
            {
                playerInfo.textureObject.Textures.Skin.Bytes = null;
                // ignore;
                if (PlayerInfoCompleted != null)
                {
                    PlayerInfoCompleted(playerInfo);
                }
            }
        }

        public void CreateSession(string accessToken, string selectedProfile, string hash)
        {
            try
            {
                //string escaptedUserName = Uri.EscapeUriString(userName);
                //string encodedSessionId = Uri.EscapeDataString(sessionId);
                string escapedHash = Uri.EscapeDataString(hash);

                
                HttpClient client = new HttpClient();
                //client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
                client.Timeout = clientTimeout;

                // string url = string.Format("http://session.minecraft.net/game/joinserver.jsp?user={0}&sessionId={1}&serverId={2}", escaptedUserName, encodedSessionId, escapedHash);

                Uri uri = new Uri(@"https://sessionserver.mojang.com/session/minecraft/join");
                Session session = new Session();
                session.accessToken = accessToken;
                session.selectedProfile = selectedProfile;
                session.serverId = escapedHash;

                string sessionString = JsonConvert.SerializeObject(session);
                HttpContent content = new StringContent(sessionString, Encoding.UTF8);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");


                client.PostAsync(uri, content).ContinueWith(postTask =>
                {
                    AuthResponse authResponse = new AuthResponse();

                    try
                    {
                        string result = postTask.Result.Content.ReadAsStringAsync().Result;

                        //string result = getTask.Result.ToString();
                        if(result.Trim() == string.Empty)
                        {
                            authResponse.Result = RequestResult.Success;
                        }
                        else
                        {
                            authResponse.Result = RequestResult.Fail;
                            authResponse.Message = result;
                        }

                        if (CreateSessionComplete != null)
                        {
                            CreateSessionComplete(authResponse);
                        }
                    }
                    catch (Exception e)
                    {
                        ThrowError(e);
                    }
                });
            }
            catch (Exception ex)
            {
                ThrowError(ex);
            }
        }
        

        private void ThrowError(Exception ex)
        {
            if(MojangAPIError != null)
            {
                MojangAPIError(ex);
            }
        }
        
    }

    public class PlayerInfo
    {
        public string id { get; set; }
        public string name { get; set; }
        public List<PlayerProperties> properties { get; set; } 
        public TexturesObject textureObject { get; set; }       
    }

    public class Skin
    {
        public string URL { get; set; }
        public string Bytes { get; set; }
    }

    public class Cape
    {
        public string URL { get; set; }
    }

    public class Textures
    {
        [JsonProperty(PropertyName = "SKIN")]
        public Skin Skin { get; set; }
        [JsonProperty(PropertyName = "CAPE")]
        public Cape Cape { get; set; }
    }

    public class TexturesObject
    {
        [JsonProperty(PropertyName = "timestamp")]
        public string Timestamp { get; set; }
        [JsonProperty(PropertyName = "profileId")]
        public string ProfileId  { get; set; }
        [JsonProperty(PropertyName = "profileName")]
        public string ProfileName  { get; set; }
        [JsonProperty(PropertyName = "isPublic")]
        public bool IsPublic  { get; set; }
        [JsonProperty(PropertyName = "textures")]
        public Textures Textures { get; set; }
    }

    public class PlayerProperties
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class AgentObject
    {
        public string name
        {
            get
            {
                return "Minecraft";
            }
        } 

        public string version
        {
            get
            {
                return "1";
            }
        } 
    }

    public class AuthRequest
    {
        public AgentObject agent
        {
            get
            {
                return new AgentObject();
            }
        }

        public string username { get; set; }
        public string password { get; set; }
        public string clienttoken { get; set; }
        public bool requestUser { get; set; } = true;
    }

    public class ValidateObject
    {
        public string accessToken { get; set; }
        public string clientToken { get; set; }
    }

    public enum RequestResult
    {
        Success,
        Fail
    }

    public class Session
    {
        public string accessToken { get; set;  }
        public string selectedProfile { get; set; }
        public string serverId { get; set; }

    }


    public class UUIDResponse
    {
        public RequestResult Result {get; set;}
        public string Message { get; set; }
        public string id { get; set; }
        public string name { get; set; }
    }

    public class AuthResponse
    {
        public string Message { get; set; }
        public RequestResult Result {get; set;}
        public string accessToken { get; set; }
        public string clientToken { get; set; }
        public string error { get; set; }
        public string errorMessage { get; set; }
        public SelectedProfile selectedProfile;
    }

    public class SelectedProfile
    {
        public string id { get; set; }
        public string name { get; set; }
    }
}