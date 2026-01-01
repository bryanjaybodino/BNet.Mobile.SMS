using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Telephony;
using Android.Views;
using Android.Widget;
using BNet.Mobile.SMS.Services.TempData;
using BNet.Mobile.SMS.Services.Websocket.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BNet.Mobile.SMS.Services.SmsService
{
    internal class SendMessage
    {
        SendQueue ISendQueue = new SendQueue();
        static List<string> MessagesHistory = new List<string>();
        static List<string> MessageFailed = new List<string>();
        static string tempJson = "";//Stored current message for failed message
        public void Send(string jsonMessage)
        {
            var SmsSender = SmsManager.Default;
            try
            {
                SendPayload sendPayload = JsonConvert.DeserializeObject<SendPayload>(jsonMessage);
                string phone = sendPayload.receiver;
                string message = sendPayload.message;


                if (message.Replace(" ", "") != "" && phone.Replace(" ", "") != "")
                {
                    string[] phoneNumbers = RemovePHCountryCode(phone).Split(new char[] { ',' });//set multiple receipients
                    string[] receivers = phoneNumbers.Distinct().ToArray();
                    for (int i = 0; i < receivers.Length; i++)
                    {
                        string receiver = receivers[i].Replace(" ", "");
                        if (receiver != "")
                        {
                            //NEED I REMOVE AGAD YUNG MESSAGE PARA HINDI NA MAG DUPLICATE 
                            //NASA SmsReceiver yung Next Action kung Failed() or Sent()
                            Remove();// NEED NA AGAD I REMOVE KASI PAG MAHABA ANG MESSAGE NAG KAKA ERROR NA

                            IList<string> messages = SmsSender.DivideMessage(message);
                            IList<PendingIntent> pendingIntents_deliver = new List<PendingIntent>();
                            Intent SmsDeliverAction = new Intent("SMS_SENT");
                            SmsDeliverAction.SetIdentifier(receiver + "¿" + message);
                            PendingIntent deliver = PendingIntent.GetBroadcast(Android.App.Application.Context, 0, SmsDeliverAction, PendingIntentFlags.Immutable);
                            pendingIntents_deliver.Add(deliver);
                            SmsSender.SendMultipartTextMessage(receiver, null, messages, pendingIntents_deliver, null);
                        }
                        else
                        {
                            Remove();
                            Failed();
                        }

                    }
                    SmsSender.Dispose();
                }
                else
                {
                    Remove();
                    Failed();
                }
                SmsSender.Dispose();
            }
            catch (Exception er)
            {
                Remove();
                Failed();
            }
            finally
            {
                SmsSender.Dispose();
            }
        }
        public async void Remove()
        {
            try
            {
                tempJson = await ISendQueue.PeekAsync();
                await ISendQueue.DequeueAsync();
            }
            catch { }
        }
        public void Failed()
        {
            try
            {

                MessageFailed.Add(tempJson);
            }
            catch { }
        }
        public async void Sent()
        {
            try
            {
                MessagesHistory.Add(await ISendQueue.PeekAsync());
            }
            catch { }
        }

        public int CountSent()
        {
            return MessagesHistory.Count;
        }
        public int CountFailed()
        {
            return MessageFailed.Count;
        }

        string RemovePHCountryCode(string phone)
        {
            phone = phone.Replace(" ", "").Trim();
            string pattern = @"^(?:\+63|63)?";
            string RegexResult = Regex.Replace(phone, pattern, match => match.Length > 0 ? "0" : "");
            if (RegexResult.Length == 10 && !RegexResult.Contains("+"))
            {
                RegexResult = "0" + RegexResult;
            }
            return RegexResult;
        }
    }
}