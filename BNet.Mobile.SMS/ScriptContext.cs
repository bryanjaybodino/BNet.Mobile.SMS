using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using BNet.Mobile.SMS.Services.DatabaseServices;
using BNet.Mobile.SMS.Services.ExitAppServices;
using BNet.Mobile.SMS.Services.FloatingServices;
using BNet.Mobile.SMS.Services.ForegroundServices;
using BNet.Mobile.SMS.Services.TempDataServices;
using Java.Interop;
using System.Threading.Tasks;
using static Android.Renderscripts.Sampler;


namespace BNet.Mobile.SMS
{
    public class ScriptContext : Java.Lang.Object
    {
        MySQLService IMySQLService = new MySQLService();
        CreateDatabase ICreateDatabase = new CreateDatabase();



        private readonly Context context;
        private readonly WebView webView;
        public ScriptContext(Context context, WebView webView)
        {
            this.context = context;
            this.webView = webView;
        }


        [JavascriptInterface]
        [Export("Service")]
        public void Service(string IconClass)
        {

            bool canOverlay = Android.Provider.Settings.CanDrawOverlays(Application.Context);
            if (canOverlay)
            {
                if (IconClass == "bi bi-stop-circle fs-3 text-danger")
                {
                    UpdateElementAttribute(HtmlElement.Icon_RunService, "class", "bi bi-play-circle fs-3 text-success");
                    UpdateInnerHtml(HtmlElement.Label_RunService, "Start Service");
                    //ForegroundTasks.StopService();
                    FloatingIcon.StopService();
                }
                else
                {
                    UpdateElementAttribute(HtmlElement.Icon_RunService, "class", "bi bi-stop-circle fs-3 text-danger");
                    UpdateInnerHtml(HtmlElement.Label_RunService, "Service is running");
                    //ForegroundTasks.StartService();
                    FloatingIcon.StartService();

                    AlertDialog.Builder alert = new AlertDialog.Builder(context);
                    alert.SetCancelable(false);
                    alert.SetTitle("Message");
                    alert.SetMessage("The app is closing in order to run in the background.");
                    alert.SetPositiveButton("Ok", (senderAlert, args) =>
                    {
                        RecentTasks.RemoveAppFromRecentTasks();
                    });
                    alert.Show();
                }
            }
            else
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(context);
                alert.SetTitle("Permission Required");
                alert.SetMessage(
                     "This app needs permission to display over other apps.\n\n" +
                     "Please go to Advanced settings and enable \"Display over other apps\"."
                    );
                alert.SetPositiveButton("Open Settings", (senderAlert, args) =>
                {
                    var intent = new Android.Content.Intent(Android.Provider.Settings.ActionApplicationDetailsSettings);
                    intent.SetData(Android.Net.Uri.Parse("package:" + Application.Context.PackageName));
                    intent.AddFlags(ActivityFlags.NewTask);
                    Application.Context.StartActivity(intent);
                });
                alert.Show();


            }
            HtmlElement.Refresh();
        }







        [JavascriptInterface]
        [Export("CreateDatabase")]
        public void CreateDatabase(string value)
        {

            // Run the database creation task in the background
            Task.Run(async () =>
            {
                Properties.SetDatabaseConnection(value);
                if (IMySQLService.isConnected())
                {
                    await ICreateDatabase.Create();
                    ToastShort("Database has been saved!");
                    Properties.SetIsMySQLEnabled(true);
                }
                else
                {
                    ToastShort("Database is not found!");
                    Properties.SetIsMySQLEnabled(false);
                }


                RemoveAttribute(HtmlElement.Button_SaveSettings, "disabled");
                UpdateInnerHtml(HtmlElement.Button_SaveSettings, "Save changes");
                HtmlElement.Refresh();
            });
        }























































        public void UpdateValue(string id, string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            // Offload heavy processing to a background task
            Task.Run(() =>
            {
                // Escape single quotes in the value
                string escapedValue = value.Replace("'", "\\'");
                string js = $"document.getElementById('{id}').value = '{escapedValue}';";

                // Only switch to UI thread for JS evaluation
                webView.Post(() =>
                {
                    webView.EvaluateJavascript(js, null);
                });
            });
        }
        public void UpdateInnerHtml(string id, string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            // Offload heavy processing to a background task
            Task.Run(() =>
            {
                // Escape single quotes in the value
                string escapedValue = value.Replace("'", "\\'");
                string js = $"document.getElementById('{id}').innerHTML = '{escapedValue}';";

                // Only switch to UI thread for JS evaluation
                webView.Post(() =>
                {
                    webView.EvaluateJavascript(js, null);
                });
            });
        }

        public void UpdateElementAttribute(string id, string attribute, string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            // Offload heavy processing to a background task
            Task.Run(() =>
            {
                // Escape single quotes in the value
                string escapedValue = value.Replace("'", "\\'");
                string js = $"document.getElementById('{id}').setAttribute('{attribute}', '{escapedValue}');";
                // Only switch to UI thread for JS evaluation
                webView.Post(() =>
                {
                    webView.EvaluateJavascript(js, null);
                });
            });
        }


        public void RemoveAttribute(string id, string attribute)
        {
            // Offload heavy processing to a background task
            Task.Run(() =>
            {
                string js = $"document.getElementById('{id}').removeAttribute('{attribute}');";
                // Only switch to UI thread for JS evaluation
                webView.Post(() =>
                {
                    webView.EvaluateJavascript(js, null);
                });
            });
        }

        public void ToastShort(string message)
        {
            Android.App.Application.SynchronizationContext.Post(_ =>
            {
                Toast.MakeText(context, message, ToastLength.Short).Show();
            },
            null);
        }
    }
}