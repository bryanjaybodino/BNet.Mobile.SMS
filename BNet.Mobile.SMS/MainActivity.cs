using Android.App;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using AndroidX.AppCompat.App;
using BNet.Mobile.SMS.Services.MyNetwork;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using static Google.Android.Material.Tabs.TabLayout;

namespace BNet.Mobile.SMS
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        NetworkChecker networkChecker = new NetworkChecker();
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            // Copy the HTML file from assets to internal storage
            CopyAssetsToInternalStorage();








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


            // Make the activity full screen by removing the status bar
            // Hide the status bar and make the activity full-screen
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                var decorView = Window.DecorView;
                int uiOptions = (int)SystemUiFlags.Fullscreen | (int)SystemUiFlags.HideNavigation | (int)SystemUiFlags.ImmersiveSticky;
                decorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;
            }
            else
            {
                // For lower versions, just hide the status bar
                Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            }
            SupportActionBar?.Hide();


            // create a timer
            Timer timer = new Timer(1000); // 1000ms = 1 second
            timer.Elapsed += async (sender, e) =>
            {
                // Switch to UI thread
                RunOnUiThread(async () =>
                {
                    scriptContext.UpdateInnerText(HtmlElement.Label_Connection, networkChecker.LocalConnection());
                    await Task.CompletedTask;
                });
            };
            timer.Start(); 
        }



        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

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


    }
}