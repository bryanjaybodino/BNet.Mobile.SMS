using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BNet.Mobile.SMS.Services.MyNetwork;
using BNet.Mobile.SMS.Services.NotificationService;
using BNet.Mobile.SMS.Services.SmsService;
using BNet.Mobile.SMS.Services.TempData;
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
                scriptContext.UpdateInnerText(Label_Connection, NetworkChecker.LocalConnection());
                scriptContext.UpdateInnerText(Label_SentQueue, (await SendQueue.CountAsync()).ToString());
                scriptContext.UpdateInnerText(Label_SentSuccess, (SendMessage.CountSent()).ToString());
                scriptContext.UpdateInnerText(Label_SentFailed, (SendMessage.CountFailed()).ToString());
                scriptContext.UpdateInnerText(Label_ReceivedQueue, (await SaveQueue.CountAsync()).ToString());
                scriptContext.UpdateInnerText(Label_ReceivedSuccess, (SaveMessage.CountSave()).ToString());
                scriptContext.UpdateValue(Textbox_Connection, Properties.DatabaseConnection());
                isRefresh = true;
            }
        }

    }
}