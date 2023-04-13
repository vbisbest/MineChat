using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V7.App;
using Android.Util;

namespace MineChat.Droid
{
    [Activity(Theme = "@style/MyTheme.Splash", MainLauncher = false, NoHistory = true)]
    public class SplashActivity : Activity
    {
        static readonly string TAG = "X:" + typeof(SplashActivity).Name;


        public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
        {
            base.OnCreate(savedInstanceState, persistentState);

            Drawable replace = (Drawable)Resource.Drawable.splash_lite;

            LayerDrawable image = (LayerDrawable)Resource.Id.splashImage;
            image.SetDrawableByLayerId(Resource.Id.splashImage, replace);

            //var imageOne = this.FindViewById(Resource.Id.splashImage);
            //Drawable drawable = new BitmapDrawable(bitmap);
            //drawable.
            //layerDrawable.setDrawableByLayerId(R.id.dynamicItem, drawable);

            Log.Debug(TAG, "SplashActivity.OnCreate");
        }
        protected override void OnResume()
        {
            Drawable replace = (Drawable)Resource.Drawable.splash_lite;

            LayerDrawable image = (LayerDrawable)Resource.Id.splashImage;
            image.SetDrawableByLayerId(Resource.Id.splashImage, replace);

            base.OnResume();
            StartActivity(new Intent(Application.Context, typeof(MainActivity)));
        }
    }
}