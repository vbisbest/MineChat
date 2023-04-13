using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Acr.UserDialogs;
//using Android.Gms.Analytics;
//using Xamarin.Forms;
using Android.Views;
using Android.Content;
//using Firebase.Iid;
//using Firebase.Messaging;
using System.Threading.Tasks;
//using Firebase.Analytics;
using System.Diagnostics;

namespace MineChat.Droid
{
    [Activity(MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {        
        private const string GcmSenderId = "665611710815";
        //private FirebaseAnalytics firebaseAnalytics;

        //private static GoogleAnalytics GAInstance;
        //private static Tracker GATracker;        
        protected override void OnCreate(Bundle bundle)
        {

            try
            {

                var connectToServer = Intent.GetStringExtra("host");
                Console.WriteLine("*******************************" + connectToServer);


                //Firebase.FirebaseApp.InitializeApp(this);

                global::Xamarin.Forms.Forms.Init(this, bundle);

                try
                {
                    Window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#2787c8"));
                }
                catch
                {
                    //ignore 
                    Console.WriteLine("Cant set status bar color");
                }

                Window.SetSoftInputMode(Android.Views.SoftInput.AdjustResize);

                UserDialogs.Init(this);
                GlobalDroid.mainActivity = this;

                Task.Run(() =>
                {
                    try
                    {
                        //var instanceID = FirebaseInstanceId.Instance;
                        //instanceID.DeleteInstanceId();
                        //var iid1 = instanceID.Token;
                        //var token = instanceID.GetToken(GcmSenderId, FirebaseMessaging.InstanceIdScope);
                        //System.Diagnostics.Debug.WriteLine($"Iid1 : {iid1}");
                        //System.Diagnostics.Debug.WriteLine($"Iid2 : {token}");

                        //FirebaseMessaging.Instance.SubscribeToTopic("allDevices");

                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                });

                try
                {
                    base.Window.RequestFeature(WindowFeatures.ActionBar);
                    base.SetTheme(Resource.Style.MineChatTheme);
                }
                catch
                {

                }

                base.OnCreate(bundle);


                LoadApplication(new App());

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
        }

        protected override void OnRestart()
        {            
            base.OnRestart();
            if (Global.IsConnected)
            {
                //Appodeal.OnResume(this, Appodeal.BANNER);
            }
        }

        
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            //if (BillingClass._serviceConnection != null)
            //    BillingClass._serviceConnection.BillingHandler.HandleActivityResult(requestCode, resultCode, data);
            //Purchases p = (Purchases)GlobalDroid.InAppServices;
            //p.HandleActivityResult(requestCode, resultCode, data);
            base.OnActivityResult(requestCode, resultCode, data);
        }
        

        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override void OnStop()
        {
            System.Threading.Tasks.Task.Run(() => { base.OnStop(); }).Wait();
        }
    }
}

