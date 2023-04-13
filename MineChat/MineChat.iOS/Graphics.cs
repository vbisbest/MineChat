using System;
using UIKit;
using CoreGraphics;

[assembly: Xamarin.Forms.Dependency (typeof ( MineChat.iOS.Graphics))]
namespace MineChat.iOS
{
    public class Graphics : IGraphics
    {
        public Graphics()
        {
        
        }            

        public byte[] ScaleImage (byte[] imageData, float width, float height)
        {
            try
            {
                UIImage image = ImageFromByteArray(imageData);
                CGSize cgSize = new CGSize(width, height);
                UIGraphics.BeginImageContext (cgSize);

                CGContext context = UIGraphics.GetCurrentContext ();
                context.InterpolationQuality = CGInterpolationQuality.None;


                context.TranslateCTM (0, cgSize.Height);
                context.ScaleCTM (1f, -1f);

                context.DrawImage (new CGRect (0, 0, cgSize.Width, cgSize.Height), image.CGImage);

                var scaledImage = UIGraphics.GetImageFromCurrentImageContext();
                UIGraphics.EndImageContext();

                return ImageToByteArray(scaledImage);
            }
            catch(Exception ex) 
            {
                Console.WriteLine (ex.Message);
                return null;
            }
        }

        // crop the image, without resizing
        public byte[] CropImage(byte[] imageData, int crop_x, int crop_y, int width, int height)
        {
            try
            {
                UIImage img = ImageFromByteArray(imageData);
                var imgSize = img.Size;
                UIGraphics.BeginImageContext(new CGSize(width, height));
                var context = UIGraphics.GetCurrentContext();
                var clippedRect = new CGRect(0, 0, width, height);
                context.ClipToRect(clippedRect);
                var drawRect = new CGRect(-crop_x, -crop_y, imgSize.Width, imgSize.Height);
                img.Draw(drawRect);
                var modifiedImage = UIGraphics.GetImageFromCurrentImageContext();
                UIGraphics.EndImageContext();
                return ImageToByteArray(modifiedImage);
            }
            catch(Exception ex) 
            {
                Console.WriteLine (ex.Message);
                return imageData;
            }
        }

        private byte[] ImageToByteArray(UIImage source)
        {
            var data = source.AsPNG();
            var dataBytes = new byte[data.Length];
            System.Runtime.InteropServices.Marshal.Copy(data.Bytes, dataBytes, 0, Convert.ToInt32(data.Length));
            return dataBytes;
        }

        private UIKit.UIImage ImageFromByteArray(byte[] data)
        {
            if (data == null) 
            {
                return null;
            }

            UIKit.UIImage image;
            try 
            {
                image = new UIKit.UIImage(Foundation.NSData.FromArray(data));
            } 
            catch (Exception e) 
            {
                Console.WriteLine ("Image load failed: " + e.Message);
                return null;
            }
            return image;
        }

    }
}

