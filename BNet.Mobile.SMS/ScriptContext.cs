using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using BNet.Mobile.SMS.Services.ExitAppService;
using BNet.Mobile.SMS.Services.MyDatabase;
using BNet.Mobile.SMS.Services.NotificationService;
using BNet.Mobile.SMS.Services.TempData;
using Java.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using static Android.Renderscripts.Sampler;
using static Android.Telephony.CarrierConfigManager;
using static Google.Android.Material.Tabs.TabLayout;

namespace BNet.Mobile.SMS
{
    public class ScriptContext : Java.Lang.Object
    {
        MySQLService IMySQLService = new MySQLService();
        Properties IProperties = new Properties();
        CreateDatabase ICreateDatabase = new CreateDatabase();
        ForegroundService IForegroundService = new ForegroundService();
        RecentTasksService IRecentTasksService = new RecentTasksService();


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
            if (IconClass == "bi bi-stop-circle fs-3 text-danger")
            {
                UpdateElementAttribute(HtmlElement.Icon_RunService, "class", "bi bi-play-circle fs-3 text-success");
                UpdateInnerText(HtmlElement.Label_RunService, "Start Service");
                IForegroundService.StopMyForeGroundService();
            }
            else
            {
                UpdateElementAttribute(HtmlElement.Icon_RunService, "class", "bi bi-stop-circle fs-3 text-danger");
                UpdateInnerText(HtmlElement.Label_RunService, "Service is running");
                IForegroundService.StartMyForeGroundService();


                AlertDialog.Builder alert = new AlertDialog.Builder(context);
                alert.SetCancelable(false);
                alert.SetTitle("Message");
                alert.SetMessage("The app is closing in order to run in the background.");
                alert.SetPositiveButton("Ok", (senderAlert, args) => {
                    IRecentTasksService.RemoveAppFromRecentTasks();
                });
                alert.Show();


            }
        }



        [JavascriptInterface]
        [Export("CreateDatabase")]
        public void CreateDatabase(string value)
        {

            // Run the database creation task in the background
            Task.Run(async () =>
            {
                IProperties.SetDatabaseConnection(value);
                if (IMySQLService.isConnected())
                {
                    await ICreateDatabase.Create();
                    ToastShort("Database has been saved!");
                    IProperties.SetIsMySQLEnabled(true);
                }
                else
                {
                    ToastShort("Database is not found!");
                    IProperties.SetIsMySQLEnabled(false);
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