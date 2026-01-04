using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BNet.Mobile.SMS.Services.NetworkServices;
using BNet.Mobile.SMS.Services.SmsServices;
using BNet.Mobile.SMS.Services.TempDataServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNet.Mobile.SMS
{
    internal class HtmlElement
    {
        public static string Label_Connection = "Label_Connection";
        public static string Label_SentQueue = "Label_SentQueue";
        public static string Label_SentSuccess = "Label_SentSuccess";
        public static string Label_SentFailed = "Label_SentFailed";
        public static string Label_DatabaseStatus = "Label_DatabaseStatus";
        public static string Label_ReceivedQueue = "Label_ReceivedQueue";
        public static string Label_ReceivedSuccess = "Label_ReceivedSuccess";
        public static string Icon_RunService = "Icon_RunService";
        public static string Label_RunService = "Label_RunService";
        public static string Button_SaveSettings = "Button_SaveSettings";
        public static string Textbox_Connection = "Textbox_Connection";

        static bool isRefresh = true;
        public static void Refresh()
        {
            isRefresh = true;
        }

        public static async Task Update(ScriptContext scriptContext)
        {
            if (isRefresh)
            {
                scriptContext.UpdateInnerHtml(Label_SentQueue, (await SendQueue.CountAsync()).ToString());
                scriptContext.UpdateInnerHtml(Label_SentSuccess, (SendMessage.CountSent()).ToString());
                scriptContext.UpdateInnerHtml(Label_SentFailed, (SendMessage.CountFailed()).ToString());
                scriptContext.UpdateInnerHtml(Label_ReceivedQueue, (await SaveQueue.CountAsync()).ToString());
                scriptContext.UpdateInnerHtml(Label_ReceivedSuccess, (SaveMessage.CountSave()).ToString());

                scriptContext.UpdateValue(Textbox_Connection, Properties.DatabaseConnection());
                if (Properties.IsMySQLEnabled())
                {
                    scriptContext.UpdateInnerHtml(Label_DatabaseStatus, "<i class='bi-check-lg'></i> Database is connected</span>");
                }
                else
                {
                    scriptContext.UpdateInnerHtml(Label_DatabaseStatus, "<i class='bi-x-lg'></i> No Database Found");
                }

                if (Properties.IsBackgroundService())
                {
                    scriptContext.UpdateElementAttribute(Icon_RunService, "class", "bi bi-stop-circle fs-3 text-danger");
                    scriptContext.UpdateInnerHtml(Label_RunService, "Service is running");
                }
                else
                {
                    scriptContext.UpdateElementAttribute(Icon_RunService, "class", "bi bi-play-circle fs-3 text-success");
                    scriptContext.UpdateInnerHtml(Label_RunService, "Start Service");
                }
                isRefresh = false;
            }
            else
            {
                scriptContext.UpdateInnerHtml(Label_Connection, NetworkChecker.WebServerConnection());
            }
        }

    }
}