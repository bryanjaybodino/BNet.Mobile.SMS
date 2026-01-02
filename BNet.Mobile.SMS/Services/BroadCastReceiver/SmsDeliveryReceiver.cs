using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using BNet.Mobile.SMS.Services.SmsService;
using BNet.Mobile.SMS.Services.TempData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BNet.Mobile.SMS.Services.BroadCastReceiver
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter(new[] { "SMS_SENT" })]
    public class SmsDeliveryReceiver : BroadcastReceiver
    {
        SendMessage ISendMessage = new SendMessage();
        GetMessages IGetMessages = new GetMessages();
        ForegroundService IForegroundService = new ForegroundService();
        SaveQueue ISaveQueue = new SaveQueue();
        public override void OnReceive(Context context, Intent intent)
        {
            var pendingResult = GoAsync();
            Task.Run(async () =>
            {
                try
                {
                    // Wait 5 seconds to reflect on the SIM card database
                    await Task.Delay(10000);
                    string receiver = intent.GetStringExtra("receiver");
                    string message = intent.GetStringExtra("message");
                    var messages = IGetMessages.RetriveBy(message, receiver);
                    if (messages.Count > 0)
                    {
                        string jsonString = JsonConvert.SerializeObject(messages[0]);
                        await ISaveQueue.SetQueueAsync(jsonString);
                        ISendMessage.Sent();
                        Android.App.Application.SynchronizationContext.Post(_ =>
                        {
                            Toast.MakeText(context, "Sent SMS!", ToastLength.Short).Show();
                        }, null);
                    }
                    else
                    {
                        ISendMessage.Failed();
                    }

                    HtmlElement.Refresh();
                }
                finally
                {
                    pendingResult.Finish();
                }

            });

        }
    }
}
