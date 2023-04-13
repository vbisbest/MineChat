using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
//using Firebase.Analytics;
using MineChat;
using Xamarin.Facebook.Ads;

[assembly: Xamarin.Forms.Dependency(typeof(MineChat.Droid.Analytics))]
namespace MineChat.Droid
{
    public class Analytics : Java.Lang.Object, IAnalytics, INativeAdListener
    {
        const string TAG = "FB_AUDIENCE_NETWORK";

        LinearLayout nativeAdContainer;
        LinearLayout adView;
        AdChoicesView adChoicesView;
        NativeAd nativeAd;


        public void ConnectToServer(MineChatAPI.Server server)
        {
            try
            {
                AdSettings.SetTestMode(true);

                //NativeAdListener listener = new NativeAdListener();
                
                //var placementId = "1802865323317020_2321026524834228";
                //NativeBannerAd nativeBannerAd = new NativeBannerAd(GlobalDroid.mainActivity.ApplicationContext, placementId);
                //nativeBannerAd.SetAdListener(listener);
                //nativeBannerAd.LoadAd();


                //var placementId = Application.Context.Resources.GetString(Resource.String.fb_placement_id);
                //nativeAd = new NativeAd(GlobalDroid.mainActivity.ApplicationContext, placementId);
                //nativeAd.SetAdListener(this);
                //nativeAd.LoadAd();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            var intent = new Intent(GlobalDroid.mainActivity, typeof(NativeActivity));
        }

        public void FailedLogin(string description)
        {
            //throw new NotImplementedException();
        }

        public void LogError(string error)
        {
            //throw new NotImplementedException();
        }






        public void OnAdClicked(IAd p0)
        {
            Console.WriteLine("OnAdClicked");
        }

        public void OnAdLoaded(IAd ad)
        {
            //Console.WriteLine("OnAdLoaded");
            //Android.Util.Log.Debug(TAG, "Native Ad Loaded");

            //if (ad != nativeAd)
            //{
            //    return;
            //}

            //LayoutInflater inflater = (LayoutInflater)GlobalDroid.mainActivity.GetSystemService(Context.LayoutInflaterService);
            try
            {
            //    //var nativeAdContainer = inflater.Inflate(Resource.Id.native_ad_container, null) as LinearLayout;

            //    //// Add ad into the ad container.
            //    //nativeAdContainer = FindViewById<LinearLayout>(Resource.Id.native_ad_container);

            //    //var inflater = LayoutInflater.From(this);
            //    nativeAdContainer = (LinearLayout)inflater.Inflate(Resource.Layout.NativeAdLayout, null, false);
            //    adView = (LinearLayout)inflater.Inflate(Resource.Layout.NativeAdView, nativeAdContainer, false);


            //    nativeAdContainer.AddView(adView);

            //    //// Create native UI using the ad metadata.
            //    var nativeAdIcon = adView.FindViewById<ImageView>(Resource.Id.native_ad_icon);
            //    var nativeAdTitle = adView.FindViewById<TextView>(Resource.Id.native_ad_title);
            //    var nativeAdBody = adView.FindViewById<TextView>(Resource.Id.native_ad_body);
            //    var nativeAdMedia = adView.FindViewById<MediaView>(Resource.Id.native_ad_media);
            //    var nativeAdSocialContext = adView.FindViewById<TextView>(Resource.Id.native_ad_social_context);
            //    var nativeAdCallToAction = adView.FindViewById<Button>(Resource.Id.native_ad_call_to_action);

            //    //// Setting the Text.
            //    nativeAdSocialContext.Text = nativeAd.AdSocialContext;
            //    nativeAdCallToAction.Text = nativeAd.AdCallToAction;
            //    nativeAdTitle.Text = nativeAd.AdHeadline;
            //    nativeAdBody.Text = nativeAd.AdBodyText;

            //    //// Downloading and setting the ad icon.
            //    var adIcon = nativeAd.AdIcon;
            //    //NativeAd.DownloadAndDisplayImage(adIcon, nativeAdIcon);

            //    //// Download and setting the cover image.
            //    var adCoverImage = nativeAd.AdCoverImage;
            //    //nativeAdMedia.SetNativeAd(nativeAd);

            //    //// Add adChoices icon
            //    if (adChoicesView == null)
            //    {
            //        adChoicesView = new AdChoicesView(GlobalDroid.mainActivity, nativeAd, true);
            //        adView.AddView(adChoicesView, 0);
            //    }

            //    //nativeAd.RegisterViewForInteraction(adView);
            //    GlobalDroid.mainActivity AddContentView(adView, null);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public void OnError(IAd p0, AdError p1)
        {
            Console.WriteLine(p1.ErrorMessage);
        }

        public void OnLoggingImpression(IAd p0)
        {
            Console.WriteLine("OnLoggingImpression");
        }

        public void OnMediaDownloaded(IAd p0)
        {
            Console.WriteLine("OnMediaDownloaded");
        }

        public void PageVideosOpened()
        {
            Console.WriteLine("PageVideosOpened");
        }

        public void VideoPlayed(string videoId)
        {
            Console.WriteLine("VideoPlayed");
        }
    }
}