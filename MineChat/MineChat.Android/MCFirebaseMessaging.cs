//using System;
//using Android.App;
//using Android.Content;
//using Android.Media;
//using Android.Support.V4.App;
//using Firebase.Messaging;
//using Firebase.Iid;
//using Android.Graphics;

//namespace MineChat.Droid
//{
//    [Service]
//    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
//    public class MyFirebaseIIDService : FirebaseInstanceIdService
//    {
//        /**
//         * Called if InstanceID token is updated. This may occur if the security of
//         * the previous token had been compromised. Note that this is called when the InstanceID token
//         * is initially generated so this is where you would retrieve the token.
//         */
//        public override void OnTokenRefresh()
//        {
//            // Get updated InstanceID token.
//            var refreshedToken = FirebaseInstanceId.Instance.Token;
//            Console.WriteLine("GCM Token: " + refreshedToken);

//            // TODO: Implement this method to send any registration to your app's servers.
//        }
//    }

//    [Service(Exported = true)]
//    [BroadcastReceiver]
//    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
//    [IntentFilter(new[] { Intent.ActionBootCompleted })]
//    public class MCFirebaseMessagingService : FirebaseMessagingService
//    {
//        public override void OnMessageReceived(RemoteMessage message)
//        {

//            try
//            {
//                Console.WriteLine("From: " + message.From);

//                // Check if message contains a data payload.
//                if (message.Data.Count > 0)
//                {
//                    Console.WriteLine("Message data payload: " + message.Data.ToString());
//                }

//                // Check if message contains a notification payload.
//                if (message.GetNotification() != null)
//                {
//                    Console.WriteLine("Message Notification Body: " + message.GetNotification().Body);
//                }
//                //The message which i send will have keys named [message, image, AnotherActivity] and corresponding values.
//                //You can change as per the requirement.

//                //message will contain the Push Message
//                string title = message.Data["title"];
//                string body = message.Data["body"];
//                string link = message.Data["link"];
//                bool sound = Convert.ToBoolean(message.Data["sound"]);
//                bool vibrate = Convert.ToBoolean(message.Data["vibrate"]);
//                //To get a Bitmap image from the URL received
//                Bitmap icon = BitmapFactory.DecodeResource(Resources, Resource.Drawable.MineChatLogo);

//                SendNotification(title, body, icon, link, sound, vibrate);
//            }
//            catch(Exception ex)
//            {
//                Console.WriteLine(ex.Message);
//            }
//        }

//        private void SendNotification(string title, string body, Bitmap image, string link, bool sound, bool vibrate)
//        {
//            Android.Net.Uri webpage = Android.Net.Uri.Parse(link);
//            Intent intent = new Intent(Intent.ActionView, webpage);

//            PendingIntent pendingIntent = PendingIntent.GetActivity(this, 0, intent, 0);
            
//            NotificationCompat.Builder notificationBuilder = new NotificationCompat.Builder(this)
//                .SetSmallIcon(Resource.Drawable.MineChatLogo)
//                .SetContentTitle(title)
//                .SetContentText(body)
//                .SetAutoCancel(true)
//                .SetContentIntent(pendingIntent);

//            if(sound)
//            {
//                //notificationBuilder.SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification));
//            }

//            int defaults = 0;
//            if(vibrate)
//            {
//                defaults = (int)NotificationDefaults.Vibrate;
//            }

//            if(sound)
//            {
//                defaults = defaults | (int)NotificationDefaults.Sound;
//            }

//            notificationBuilder.SetDefaults(defaults);

//            NotificationManager notificationManager = (NotificationManager)GetSystemService(Context.NotificationService);

//            notificationManager.Notify(100, notificationBuilder.Build());
//        }
//    }
//}
