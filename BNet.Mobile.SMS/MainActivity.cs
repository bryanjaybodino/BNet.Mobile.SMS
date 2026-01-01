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
using BNet.Mobile.SMS.Services.BroadCastReceiver;
using BNet.Mobile.SMS.Services.MyNetwork;
using BNet.Mobile.SMS.Services.ServiceBus;
using BNet.Mobile.SMS.Services.SmsService;
using BNet.Mobile.SMS.Services.TempData;
using BNet.Mobile.SMS.Services.Websocket;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using static Google.Android.Material.Tabs.TabLayout;

namespace BNet.Mobile.SMS
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {

        // AD HOC PASSWORD : 123456

        const int RequestSmsPermissionsId = 101;

        readonly string[] SmsPermissions =
        {
            Manifest.Permission.ReadSms,
            Manifest.Permission.ReceiveSms,
            Manifest.Permission.SendSms,
            "android.permission.POST_NOTIFICATIONS" // Optional: For Android 13+ notifications
        };



        NetworkChecker networkChecker = new NetworkChecker();
        ServerSocketConnection serverSocketConnection = new ServerSocketConnection();
        SendQueue ISendQueue = new SendQueue();
        SendMessage ISendMessage = new SendMessage();
        TimeTrigger timeTrigger = new TimeTrigger();

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            // Copy the HTML file from assets to internal storage
            CopyAssetsToInternalStorage();
            FullScreen();
            var webView = FindViewById<WebView>(Resource.Id.webview);
            // Enable JavaScript in WebView
            webView.Settings.JavaScriptEnabled = true;
            webView.Settings.DomStorageEnabled = true;  // Enable local storage if used
            webView.Settings.AllowUniversalAccessFromFileURLs = true;  // Allow access to files from file URLs
            webView.Settings.AllowFileAccessFromFileURLs = true;  // Allow file access from file URLs
            webView.Settings.AllowContentAccess = true;  // Allow access to content
            var scriptContext = new ScriptContext(this, webView);
            webView.AddJavascriptInterface(scriptContext, "ScriptContext");
            WebView.SetWebContentsDebuggingEnabled(true);// Enable debugging (Logcat or Chrome DevTools)
            webView.LoadUrl($"file:///android_asset/BNet.Mobile.SMS.html");
            SupportActionBar?.Hide();
            await StartTimer(scriptContext);

            // ✅ Check and request permissions at runtime
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                if (!HasSmsPermissions())
                {
                    RequestPermissions(SmsPermissions, RequestSmsPermissionsId);
                }
            }



            SmsDeliveryReceiver receiver = new SmsDeliveryReceiver();
            IntentFilter filter = new IntentFilter("SMS_SENT");
            RegisterReceiver(receiver, filter);
        }


        private async Task StartTimer(ScriptContext scriptContext)
        {
            async Task Refresh()
            {
                serverSocketConnection.Create();
                scriptContext.UpdateInnerText(HtmlElement.Label_Connection, networkChecker.LocalConnection());
                scriptContext.UpdateInnerText(HtmlElement.Label_SentQueue, (await ISendQueue.CountAsync()).ToString());
                scriptContext.UpdateInnerText(HtmlElement.Label_SentSuccess, (ISendMessage.CountSent()).ToString());
                scriptContext.UpdateInnerText(HtmlElement.Label_SentFailed, (ISendMessage.CountFailed()).ToString());
                timeTrigger.ProcessSendingMessages();
                timeTrigger.ProcessSavingMessages();
            }
            await Refresh();
            // create a timer
            Timer timer = new Timer(1000); // 1000ms = 1 second
            timer.Elapsed += async (sender, e) =>
            {
                // Switch to UI thread
                RunOnUiThread(async () =>
                {
                    await Refresh();
                });
            };
            timer.Start();
        }


        // ✅ Helper: Check if permissions are already granted
        bool HasSmsPermissions()
        {
            foreach (var permission in SmsPermissions)
            {
                if (CheckSelfPermission(permission) != Permission.Granted)
                    return false;
            }
            return true;
        }

        // ✅ Handle the user's permission response
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == RequestSmsPermissionsId)
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
        }

    }
}