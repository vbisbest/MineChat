using System;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MineChatAPI
{
    public class PromotedVideos
    {
        private const string host = "http://www.minechatapp.com";

        public delegate void PromotedVideosReceivedHandler(PromotedVideoList videos, string message);
        public event PromotedVideosReceivedHandler PromotedVideosReceived;

        public delegate void PromotedVideoPurchasedHandler(string message);
        public event PromotedVideoPurchasedHandler PromotedVideoPurchased;

        public delegate void VideoViewedCompleteHandler(string error);
        public event VideoViewedCompleteHandler VideoViewed;

        public void GetVideos()
        {
            PromotedVideoList videos = null;
            string error = string.Empty;
            Uri uri = new Uri(host + "/api/GetPromotedVideos.php");
            HttpClient client = new HttpClient();

            try
            {
                client.Timeout = new TimeSpan(0, 0, 10);
                client.GetStringAsync(uri).ContinueWith(getTask =>
                {
                    try
                    {
                        string result = getTask.Result.ToString();
                        videos = JsonConvert.DeserializeObject<PromotedVideoList>(result);
                    }
                    catch (Exception e)
                    {
                        error = e.Message;
                    }
                    finally
                    {
                        if (PromotedVideosReceived != null)
                        {
                            PromotedVideosReceived(videos, "Could not load messages: " + error);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                if (PromotedVideosReceived != null)
                {
                    PromotedVideosReceived(null, "Could not load videos: " + ex.Message);
                }
            }
        }

        public void UpdateVideoViewed(int videoId)
        {
            string error = string.Empty;
            Uri uri = new Uri(host + "/api/UpdatePromotedVideo.php?id=" + videoId);
            HttpClient client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, 10);
            client.GetStringAsync(uri).ContinueWith(getTask =>
            {
                try
                {
                    string result = getTask.Result.ToString();

                    if (VideoViewed != null)
                    {
                        VideoViewed(string.Empty);
                    }
                }
                catch (Exception ex)
                {
                    if (VideoViewed != null)
                    {
                        VideoViewed(ex.Message);
                    }
                }
            });
        }

        public void PurchaseVideo(string videoId, string referenceNumber)
        {

            PromotedVideoList videos = null;
            string error = string.Empty;
            Uri uri = new Uri(host + "/api/InsertPromotedVideo.php?videoid=" + videoId + "&refnumber=" + referenceNumber);
            HttpClient client = new HttpClient();

            try
            {
                client.Timeout = new TimeSpan(0, 0, 10);
                client.GetStringAsync(uri).ContinueWith(getTask =>
                {
                    try
                    {
                        string result = getTask.Result.ToString();
                        if(result != "ok")
                        {
                            throw new Exception("Error sending video to MineChat: " + result.ToString());
                        }
                    }
                    catch (Exception e)
                    {
                        error = e.Message;
                    }
                    finally
                    {
                        if (PromotedVideoPurchased != null)
                        {
                            PromotedVideoPurchased(error);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                if (PromotedVideosReceived != null)
                {
                    PromotedVideoPurchased(error);
                }
            }
        }
    }

    public class PromotedVideoList
    {
        public List<PromotedVideo> PromotedVideos { get; set; }
    }

    public class PromotedVideo
    {
        public int PromotedVideoID {get; set;}
        public string VideoID { get; set; }
        public string ReferenceNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
