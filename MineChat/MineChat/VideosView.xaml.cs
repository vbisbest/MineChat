using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using MineChatAPI;
using Xamarin.Forms;
using MineChat.Languages;
using Acr.UserDialogs;

namespace MineChat
{
    public partial class VideosView : ContentPage
    {
        public VideosView()
        {

            InitializeComponent();


            this.BackgroundColor = Color.Black;

            ToolbarItem refreshVideos = new ToolbarItem();
            refreshVideos.Clicked += RefreshVideos_Clicked;
            refreshVideos.Text = AppResources.minechat_general_refresh;
            refreshVideos.Icon = "refresh.png";

            listVideos.HasUnevenRows = true;
            listVideos.ItemSelected += ListVideos_ItemSelected;

            this.ToolbarItems.Add(refreshVideos);

            GetPromotedVideos();
        }

        private void ShowMessage(string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                DisplayAlert("MineChat", message, AppResources.minechat_general_ok);
            });
        }

        private void RefreshVideos_Clicked(object sender, EventArgs e)
        {
            listVideos.ItemsSource = null;
            GetPromotedVideos();
        }

        private void ListVideos_ItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
            if (e.SelectedItem == null)
            {
                return;
            }

            Video vs = (Video)e.SelectedItem;
            listVideos.SelectedItem = null;

            MineChatAPI.PromotedVideos videos = new MineChatAPI.PromotedVideos();
            videos.UpdateVideoViewed(Convert.ToInt32(vs.Snippet.ChannelId));

            var analytics = DependencyService.Get<IAnalytics>();
            if(analytics != null)
            {
                analytics.VideoPlayed(vs.Snippet.ChannelId);
            }

            var device = DependencyService.Get<IDevice>();
            if (device != null)
            {
                device.OpenLink("http://www.youtube.com/watch?v=" + vs.Id);
            }
        }

        private void GetPromotedVideos()
        {
            MineChatAPI.PromotedVideos videos = new MineChatAPI.PromotedVideos();
            videos.PromotedVideosReceived += Videos_PromotedVideosReceived;
            UserDialogs.Instance.ShowLoading();

            try
            {
                videos.GetVideos();
            }
            catch(Exception ex)
            {
                ShowMessage("Could not get promoted videos: " + ex.Message);
            }
        }

        private void Videos_PromotedVideosReceived(MineChatAPI.PromotedVideoList videos, string message)
        {
            
            
            if(videos == null)
            {
                UserDialogs.Instance.HideLoading();
                ShowMessage("Could not get promoted videos: " + message);
            }

            GetYouTubeInfo(videos);
        }

        private void GetYouTubeInfo(MineChatAPI.PromotedVideoList videos)
        {
            try
            {
                string videoIds = string.Empty;

                foreach (PromotedVideo currentVideo in videos.PromotedVideos)
                {
                    videoIds += currentVideo.VideoID + ",";
                }

                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = "redacted",
                    ApplicationName = this.GetType().ToString()
                });

                var searchListRequest = youtubeService.Videos.List("snippet");
                searchListRequest.Id = videoIds;
                searchListRequest.MaxResults = 50;


                // Call the search.list method to retrieve results matching the specified query term.
                var searchListResponse = searchListRequest.Execute();

                foreach(Video item in searchListResponse.Items)
                {
                    if (item.Snippet.Description.Length > 300)
                    {
                        item.Snippet.Description = item.Snippet.Description.Substring(0, 200) + "...";
                        item.Snippet.Description = item.Snippet.Description.Replace("\r", " ").Trim();
                        item.Snippet.Description = item.Snippet.Description.Replace("\n", " ").Trim();
                    }

                    item.Snippet.ChannelId = videos.PromotedVideos.Find(c => c.VideoID == item.Id).PromotedVideoID.ToString();
                }

                Device.BeginInvokeOnMainThread(() =>
                {
                    listVideos.BeginRefresh();
                    listVideos.ItemsSource = searchListResponse.Items;
                    listVideos.EndRefresh();
                    UserDialogs.Instance.HideLoading();
                });
            }
            catch(Exception ex)
            {
                UserDialogs.Instance.HideLoading();
                ShowMessage("Could not get promoted videos: " + ex.Message);
            }
        }
    }
}
