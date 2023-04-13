using System;
using MineChatAPI;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;

namespace MineChat
{
    public delegate void OnQueryInventoryDelegate();
    public delegate void OnRestoreProductsDelegate();
    public delegate void OnQueryInventoryErrorDelegate(int responseCode, IDictionary<string, object> skuDetails);
    public delegate void OnPurchaseProductErrorDelegate(int responseCode, string sku);
    public delegate void OnRestoreProductsErrorDelegate(int responseCode, IDictionary<string, object> skuDetails);
    public delegate void OnUserCanceledDelegate();
    public delegate void OnInAppBillingProcessingErrorDelegate(string message);
    public delegate void OnInvalidOwnedItemsBundleReturnedDelegate(IDictionary<string, object> ownedItems);
    public delegate void OnServiceConnectedDelegate();
    public delegate void OnServiceDisconnectedDelegate();
    public delegate void OnDNSLookupCompleteDelegate(OnDNSLookupCompleteEventArgs args);

    public enum DeviceType
    {
        Phone,
        Tablet
    }

    public interface IAnalytics
    {
        void FailedLogin(string description);
        void LogError(string error);
        void PageVideosOpened();
        void VideoPlayed(string videoId);
        void ConnectToServer(MineChatAPI.Server server);
    }

    public interface IPurchases
    {
        /// <summary>
        /// Starts the setup of this Android application by connection to the Google Play Service
        /// to handle In-App purchases.
        /// </summary>
        void Initialize();
        void QueryInventory();
        void PurchaseProduct(string productId, string videoId);
        void ConsumeProduct(string purchaseToken);
        void RestoreProducts();
        void RefundProduct();

        event OnQueryInventoryDelegate OnQueryInventory;
        //event OnPurchaseProductDelegate OnPurchaseProduct;
        event OnRestoreProductsDelegate OnRestoreProducts;
        event OnQueryInventoryErrorDelegate OnQueryInventoryError;
        event OnPurchaseProductErrorDelegate OnPurchaseProductError;
        event OnRestoreProductsErrorDelegate OnRestoreProductsError;
        event OnUserCanceledDelegate OnUserCanceled;
        event OnInAppBillingProcessingErrorDelegate OnInAppBillingProcesingError;
        event OnInvalidOwnedItemsBundleReturnedDelegate OnInvalidOwnedItemsBundleReturned;
        //event OnPurchaseFailedValidationDelegate OnPurchaseFailedValidation;
        event OnServiceConnectedDelegate OnServiceConnected;
        event OnServiceDisconnectedDelegate OnServicedisconnected;
    }
    
    public interface IRate
    {
        void DoRate();
    }

    public interface INotification
    {
        void ShowNotification(string message);
    }

    public interface IDevice
    {
        void Sound();
        void Vibrate();
        string Model();
        string AppVersionName();
        void OpenFacebook();
        void OpenLink(string url);
        void SendEmail(string[] to, string subject, string body);
        void CopyToClipboard(string text);
        void Log(string message);
        bool IsLite();
        string AppCenterId();
        string ProductName();
    }

    public interface IBannerAds
    {
        void Start(ContentPage page);
        void Pause();
    }

    public interface IAds
    {
        void Initialize(ContentPage contentPage);
        void Initialize(Xamarin.Forms.View spacer, ContentPage page, DeviceType deviceType);
        void StartAds();
        void Pause();
        void Resume();
    }

    public interface IGraphics
    {
        byte[] ScaleImage(byte[] imageSource, float width, float height);
        byte[] CropImage(byte[] imageSource, int crop_x, int crop_y, int width, int height);
    }

    public interface IStorage
    {
        void SaveFile(string fileName, string data);
        string LoadFile(string fileName);
    }

    public interface INetworkHelper
    {
        List<IPEndPoint> GetDnsServers();        
    }

    public interface IKeyboardNotifications
    {
        bool IsShown { get; }
        event EventHandler OnKeyboardShown;
        event EventHandler OnKeyboardHidden;
        void HookKeyboard(StackLayout shiftableStack, ContentPage topStack);
        void HideKeyboard();
    }

    /*
    public class OnDNSLookupCompleteEventArgs : EventArgs
    {
        public bool Success { get; set; } 
        public Server Server { get; set; }
    }
    */

}
