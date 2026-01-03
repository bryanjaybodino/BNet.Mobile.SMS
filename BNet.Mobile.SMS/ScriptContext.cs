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
                    UpdateInnerText(HtmlElement.Label_RunService, "Start Service");
                    //ForegroundTasks.StopService();
                    FloatingIcon.StopService();
                }
                else
                {
                    UpdateElementAttribute(HtmlElement.Icon_RunService, "class", "bi bi-stop-circle fs-3 text-danger");
                    UpdateInnerText(HtmlElement.Label_RunService, "Service is running");
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
                var intent = new Android.Content.Intent(Android.Provider.Settings.ActionManageOverlayPermission);
                intent.SetData(Android.Net.Uri.Parse("package:" + Application.Context.PackageName));
                intent.AddFlags(ActivityFlags.NewTask);
                Application.Context.StartActivity(intent);
            }
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
                UpdateInnerText(HtmlElement.Button_SaveSettings, "Save changes");
            });
        }























































        public void UpdateValue(string id, string value)
        {
            // Ensure we're on UI thread
            webView.Post(() =>
            {
                if (!string.IsNullOrEmpty(value))
                {
                    value = value.Replace("'", "\\'");
                    string js = $"document.getElementById('{id}').value = '{value}';";
                    webView.EvaluateJavascript(js, null);
                }

            });
        }
        public void UpdateInnerText(string id, string value)
        {
            // Ensure we're on UI thread
            webView.Post(() =>
            {
                if (!string.IsNullOrEmpty(value))
                {
                    value = value.Replace("'", "\\'");
                    string js = $"document.getElementById('{id}').innerText = '{value}';";
                    webView.EvaluateJavascript(js, null);
                }
            });
        }
        public void UpdateInnerHtml(string id, string value)
        {
            // Ensure we're on UI thread
            webView.Post(() =>
            {
                if (!string.IsNullOrEmpty(value))
                {
                    value = value.Replace("'", "\\'");
                    string js = $"document.getElementById('{id}').innerHtml = '{value}';";
                    webView.EvaluateJavascript(js, null);
                }
            });
        }

        public void UpdateElementAttribute(string id, string attribute, string value)
        {
            // Ensure we're on the UI thread
            webView.Post(() =>
            {
                if (!string.IsNullOrEmpty(value))
                {
                    value = value.Replace("'", "\\'"); // Escape single quotes in the value
                    string js = $"document.getElementById('{id}').setAttribute('{attribute}', '{value}');";
                    webView.EvaluateJavascript(js, null);
                }
            });
        }


        public void RemoveAttribute(string id, string attribute)
        {
            // Ensure we're on the UI thread
            webView.Post(() =>
            {
                if (!string.IsNullOrEmpty(attribute))
                {
                    string js = $"document.getElementById('{id}').removeAttribute('{attribute}');";
                    webView.EvaluateJavascript(js, null);
                }
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