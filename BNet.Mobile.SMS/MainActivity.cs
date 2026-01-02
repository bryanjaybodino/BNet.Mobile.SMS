using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.LocalBroadcastManager.Content;
using BNet.Mobile.SMS.Services.APIService;
using BNet.Mobile.SMS.Services.BroadCastReceiver;
using BNet.Mobile.SMS.Services.MyNetwork;
using BNet.Mobile.SMS.Services.PermissionService;
using BNet.Mobile.SMS.Services.ServiceBus;
using BNet.Mobile.SMS.Services.SmsService;
using BNet.Mobile.SMS.Services.TempData;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using static Android.Bluetooth.BluetoothClass;
using static Google.Android.Material.Tabs.TabLayout;

namespace BNet.Mobile.SMS
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {

        // AD HOC PASSWORD : 123456

        TimeTrigger timeTrigger = new TimeTrigger();
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            //REGISTER CUSTOM BROADCAST RECEIVER
            SmsDeliveryReceiver receiver = new SmsDeliveryReceiver();
            IntentFilter filter = new IntentFilter();
            filter.AddAction("SMS_SENT");
            RegisterReceiver(receiver, filter);


            // Copy the HTML file from assets to internal storage
            CopyAssetsToInternalStorage();

            //Full Screen Application
            FullScreen();

            //API Server For Client Connection
            APIServer aPIServer = new APIServer();
            aPIServer.Start();

            //Check Permission
            MyPermission myPermission = new MyPermission(this);
            myPermission.EnsurePermissions();



            //Run Webvview
            var webView = FindViewById<WebView>(Resource.Id.webview);
            webView.Settings.JavaScriptEnabled = true;                      // Enable JavaScript in WebView
            webView.Settings.DomStorageEnabled = true;                      // Enable local storage if used
            webView.Settings.AllowUniversalAccessFromFileURLs = true;       // Allow access to files from file URLs
            webView.Settings.AllowFileAccessFromFileURLs = true;            // Allow file access from file URLs
            webView.Settings.AllowContentAccess = true;                     // Allow access to content
            var scriptContext = new ScriptContext(this, webView);
            webView.AddJavascriptInterface(scriptContext, "ScriptContext");
            WebView.SetWebContentsDebuggingEnabled(true);                   // Enable debugging (Logcat or Chrome DevTools)
            webView.LoadUrl($"file:///android_asset/BNet.Mobile.SMS.html"); // Default Landing Page


            StartTimer(scriptContext);
        }




        // ✅ Handle the user's permission response
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == 1001)
            {
                if (grantResults.All(result => result == Permission.Granted))
                {
                    Toast.MakeText(this, "All permissions granted.", ToastLength.Short).Show();
                }
                else
                {
                    Toast.MakeText(this, "Some permissions were denied. App may not function properly.", ToastLength.Long).Show();
                }
            }

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private async void StartTimer(ScriptContext scriptContext)
        {
            await Task.Run(() =>
            {    
                // create a timer
                Timer timer = new Timer(500); // 1000ms = 1 second
                timer.Elapsed += async (sender, e) =>
                {
                    // Switch to UI thread
                    RunOnUiThread(async () =>
                    {
                        await HtmlElement.Update(scriptContext);

                        //Service Bus Timer
                        timeTrigger.ProcessSendingMessages();
                        timeTrigger.ProcessSavingMessages();
                    });
                };
                timer.Start();

            });
        }


        // Function to copy file from Assets to internal storage
        private void CopyAssetsToInternalStorage()
        {
            try
            {
                void CopyRecursive(string assetPath, string internalPath)
                {
                    string[] items = Assets.List(assetPath);

                    foreach (string item in items)
                    {
                        string nextAssetPath = string.IsNullOrEmpty(assetPath)
                            ? item
                            : $"{assetPath}/{item}";

                        string nextInternalPath = Path.Combine(internalPath, item);

                        // Directory
                        if (Assets.List(nextAssetPath).Length > 0)
                        {
                            Directory.CreateDirectory(nextInternalPath);
                            CopyRecursive(nextAssetPath, nextInternalPath);
                        }
                        // File
                        else
                        {
                            using (var assetStream = Assets.Open(nextAssetPath))
                            using (var fileStream = new FileStream(nextInternalPath, FileMode.Create))
                            {
                                assetStream.CopyTo(fileStream);
                            }
                        }
                    }
                }

                // Start from asset root → internal storage root
                CopyRecursive("", Application.Context.FilesDir.AbsolutePath);
            }
            catch
            {

            }
        }
        private void FullScreen()
        {
            var decorView = Window.DecorView;
            int uiOptions = (int)SystemUiFlags.Fullscreen | (int)SystemUiFlags.HideNavigation | (int)SystemUiFlags.ImmersiveSticky;
            decorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;
            SupportActionBar?.Hide();
        }

    }
}