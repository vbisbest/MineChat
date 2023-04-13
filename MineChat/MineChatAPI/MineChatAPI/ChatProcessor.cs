using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft;
using Newtonsoft.Json.Converters;
using System.Linq;
using Newtonsoft.Json.Linq;
using MineChat.Languages;
using System.Diagnostics;

namespace MineChatChat
{
    public class ChatProcessor
    {
        public ChatProcessor()
        {
        }

        public static string MinecraftStringToAttributedStringJson(string message)
        {
            //Debug.WriteLine(message);
            string returnMessage = string.Empty;
            string messageFormat = string.Empty;

            try
            {
                JToken jsonPayload = null;

                try
                {
                    jsonPayload = JToken.Parse(message);
                }
                catch
                {
                    //Not json so just return the message
                    return message;
                }

                if(jsonPayload.Type == JTokenType.String)
                {
                    return jsonPayload.ToString();
                }
                    
                List<object> list = new List<object>();

                if(jsonPayload.Type == JTokenType.Array)
                {
                    foreach(JToken t in jsonPayload)
                    {
                        //Console.WriteLine(t.Type.ToString());
                        returnMessage += ProcessChatValue(t);
                    }

                    //Console.WriteLine("After " + returnMessage);
                    return returnMessage;
                }
                    
                foreach(JProperty currentValue in jsonPayload)
                {

                    //Debug.WriteLine(currentValue.Name);

                    if(currentValue.Name == "color")
                    {
                        string colorValue = currentValue.ToString();
                        returnMessage = GetColorCode(colorValue) + returnMessage;
                    }
                    else if(currentValue.Name == "translate")
                    {
                        returnMessage += GetTranslation(currentValue.Value.ToString());
                    }
                    else if(currentValue.Name == "extra")
                    {
                        string extra = string.Empty;

                        if(currentValue.Value.Type == JTokenType.Array)
                        {
                            foreach(JToken newValue in currentValue.Value)
                            {
                                if(newValue.Type == JTokenType.String)
                                {
                                    returnMessage += "§r" + newValue;
                                }
                                else
                                {
                                    
                                    extra = ProcessChatValue(newValue);
                                    returnMessage += "§r" + extra;
                                }
                            }
                        }
                        else
                        {
                            JArray z = (JArray)currentValue.Value;
                            returnMessage += extra;
                        }
                    }
                    else if(currentValue.Name == "text")
                    { 
                        returnMessage = String.Format("{0}", (String)currentValue.Value) + returnMessage;
                        //Debug.WriteLine(returnMessage);                        
                    }
                    else if(currentValue.Name == "using" || currentValue.Name == "with")
                    {
                        foreach(JToken currentUsing in currentValue.Value)
                        {
                            if(currentUsing.Type == JTokenType.String)
                            {
                                list.Add((string)currentUsing);
                            }
                            else
                            {
                                list.Add(MinecraftStringToAttributedStringJson(currentUsing.ToString()));
                            }
                        }

                        returnMessage = String.Join(" ", list.ToArray());
                        //returnMessage = String.Format("{0}", list.ToArray());
                    }
                    else if(currentValue.Name == "bold")
                    {
                        returnMessage+="§l";
                    }
                    else if(currentValue.Name == "italic")
                    {
                        returnMessage+="§o";
                    }

                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            //Console.WriteLine("After " + returnMessage);

            return returnMessage;

        }

        private static string GetColorCode(string colorValue)
        {
            switch(colorValue)
            {
                case "green":
                    return "§a";
                case "cyan":
                    return "§b";
                case "aqua":
                    return "§b";
                case "red":
                    return "§c";
                case "pink":
                    return "§d";
                case "yellow":
                    return "§e";
                case "white":
                    return "§f";
                case "black":
                    return "§0";
                case "dark_blue":
                    return "§1";
                case "dark_green":
                    return "§2";
                case "dark_cyan":
                    return "§3";
                case "dark_aqua":
                    return "§3";
                case "dark_red":
                    return "§4";
                case "purple":
                    return "§5";
                case "dark_purple":
                    return "§5";
                case "gold":
                    return "§6";
                case "gray":
                    string s = "§7";
                    return s;
                case "dark_gray":
                    return "§8";
                case "blue":
                    return "§9";
                case "light_purple":
                    return "§d";
                default:
                    return "§r";
            }
        }

        public static string GetTranslation(string translate)
        {
            string lookup = translate.Replace(".", "_");
            string s =  AppResources.ResourceManager.GetString(lookup);
            if(s == null || s == string.Empty)
            {
                s = translate;
            }
            return s;

            //return string.Empty;
        }

        private static string ProcessChatValue(JToken jsonToken)
        {

            string returnMessage = string.Empty;
            string textValue = string.Empty;
            string colorValue = string.Empty;
            string boldValue = string.Empty;
            string italicValue = string.Empty;
            string extra = string.Empty;

            // grab the text
            try
            {
                if(jsonToken.Type == JTokenType.String)
                {
                    return jsonToken.ToString();
                }

                if(jsonToken["text"] != null)                    
                {
                    textValue = jsonToken["text"].ToString();
                }

                if(jsonToken["color"] != null)
                {
                    colorValue = GetColorCode(jsonToken["color"].ToString());
                    var utf8 = System.Text.Encoding.UTF8;
                    byte[] utfBytes = utf8.GetBytes(colorValue);
                }

                if (jsonToken["italic"] != null)
                {
                    if (jsonToken["italic"].ToString() == "true")
                    {
                        italicValue = "§o";
                    }
                }

                if (jsonToken["bold"] != null)
                {
                    if (jsonToken["bold"].ToString() == "true")
                    {
                        boldValue = "§l";
                    }
                }

                if (jsonToken["translate"] != null)
                {
                    textValue = GetTranslation(jsonToken["translate"].ToString());
                }

                if (jsonToken["extra"] != null)
                {
                    extra = MinecraftStringToAttributedStringJson(jsonToken.ToString());
                }

                returnMessage = colorValue + italicValue + boldValue + extra + textValue;

                return returnMessage;
            }
            catch(Exception ex)
            {
                return jsonToken.ToString();
            }

        }


    }
}

