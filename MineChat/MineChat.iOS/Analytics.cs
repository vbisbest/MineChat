using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MineChatAPI;
using Foundation;
using UIKit;

//using Firebase.Analytics;

[assembly: Xamarin.Forms.Dependency(typeof(MineChat.iOS.Analytics))]
namespace MineChat.iOS
{
    public class Analytics : IAnalytics
    {
        public void FailedLogin(string description)
        {
            //try
            //{
            //    NSString[] keys = { new NSString(Global.ANALYTICS_FAILED_LOGIN) };
            //    NSObject[] values = { new NSString(description) };
            //    var parameters = NSDictionary<NSString, NSObject>.FromObjectsAndKeys(values, keys, keys.Length);
            //    Firebase.Analytics.Analytics.LogEvent(EventNamesConstants.Login, parameters);
            //}
            //catch
            //{
            //    // Ignore
            //}

           
        }

        public void ConnectToServer(Server server)
        {
    //        try
    //        {
    //            NSString[] keys = { new NSString(Global.ANALYTICS_CONNECTION_ESTABLISHED) };
    //            NSObject[] values = { new NSString(server.FullAddress) };
    //            var parameters = NSDictionary<NSString, NSObject>.FromObjectsAndKeys(values, keys, keys.Length);
				//Firebase.Analytics.Analytics.LogEvent(EventNamesConstants.Login, parameters);
    //        }
    //        catch
    //        {
    //            // Ignore
    //        }
        }

        public void PageVideosOpened()
        {
            //throw new NotImplementedException();
        }

        public void VideoPlayed(string videoId)
        {
            //try
            //{
            //    NSString[] keys = { new NSString(Global.ANALYTICS_VIDEO_PLAYED) };
            //    NSObject[] values = { new NSString(videoId) };
            //    var parameters = NSDictionary<NSString, NSObject>.FromObjectsAndKeys(values, keys, keys.Length);
            //    Firebase.Analytics.Analytics.LogEvent(EventNamesConstants.SelectContent, parameters);
            //}
            //catch
            //{
            //    // Ignore
            //}
        }

        public void LogError(string error)
        {
			//try
			//{
			//	NSString[] keys = { new NSString(Global.ANALYTICS_LOG_ERROR) };
			//	NSObject[] values = { new NSString(error) };
			//	var parameters = NSDictionary<NSString, NSObject>.FromObjectsAndKeys(values, keys, keys.Length);
			//	Firebase.Analytics.Analytics.LogEvent(EventNamesConstants.Login, parameters);
			//}
			//catch
			//{
			//	// Ignore
			//}
        }
    }
}