using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Telephony;
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
    [BroadcastReceiver(Enabled = true, Exported = true, Label = "SMS Service")]
    [IntentFilter(new[] { Telephony.Sms.Intents.SmsReceivedAction })]
    public class SmsReceiver : BroadcastReceiver
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
                string message = "";
                string sender = "";

                Bundle bundle = intent.Extras;
                if (bundle != null)
                {
                    SmsMessage[] msgs = Telephony.Sms.Intents.GetMessagesFromIntent(intent);
                    var smstext = new StringBuilder();

                    foreach (var msg in msgs)
                    {
                        smstext.Append(msg.DisplayMessageBody);
                    }

                    message = smstext.ToString();
                    sender = msgs.Length > 0 ? msgs[0].OriginatingAddress : string.Empty;
                }

                Android.App.Application.SynchronizationContext.Post(_ =>
                {
                    Toast.MakeText(context, "Received SMS!", ToastLength.Short).Show();
                }, null);
                var messages = IGetMessages.RetriveBy(message, sender);
                if (messages.Count > 0)
                {
                    string jsonString = JsonConvert.SerializeObject(messages[0]);
                    await ISaveQueue.SetQueueAsync(jsonString);
                }
            });
        } 
    }
}
