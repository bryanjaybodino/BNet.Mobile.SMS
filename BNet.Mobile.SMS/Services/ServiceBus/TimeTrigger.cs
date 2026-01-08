using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BNet.Mobile.SMS.Services.SmsServices;
using BNet.Mobile.SMS.Services.TempDataServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BNet.Mobile.SMS.Services.ServiceBus
{
    internal class TimeTrigger
    {

        public static async void ProcessSendingMessages()
        {
            int countSent = await SendQueue.CountAsync();
            if (countSent > 0)
            {
                string Message = await SendQueue.PeekAsync();
                SendMessage.Send(Message);
            }
        }
        public static async void ProcessSavingMessages()
        {
            int countSave = await SaveQueue.CountAsync();
            if (countSave > 0)
            {
                SaveMessage.Saved();
            }
        }

    }
}