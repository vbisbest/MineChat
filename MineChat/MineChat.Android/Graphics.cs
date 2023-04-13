using System;
using Android.Graphics;
using System.Diagnostics;
using System.IO;

[assembly: Xamarin.Forms.Dependency (typeof (MineChat.Droid.Graphics))]
namespace MineChat.Droid
{
    public class Graphics : Java.Lang.Object, IGraphics
    {
        public Graphics () {}

        public byte[] ScaleImage (byte[] imageData, float width, float height)
        {
            try
            {
                BitmapFactory.Options o = new BitmapFactory.Options();
                o.InScaled = false;
                Bitmap bm = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length, o);

                bm = Bitmap.CreateScaledBitmap(bm, (int)width, (int)height, false);
                return ImageToByteArray(bm);
            }
            catch
            {
                return null;
            }
        }

        // crop the image, without resizing
        public byte[] CropImage(byte[] imageData, int crop_x, int crop_y, int width, int height)
        {
            try
            {
                Bitmap bm = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);
                Bitmap cropped = Bitmap.CreateBitmap(bm, crop_x,crop_y,width,height);
                return ImageToByteArray(cropped);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return null;
        }

        private byte[] ImageToByteArray(Bitmap bm)
        {
            MemoryStream stream = new MemoryStream();
            bm.Compress(Bitmap.CompressFormat.Png, 0, stream);
            return stream.ToArray();
        }

    }
}

