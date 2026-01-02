using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Telephony;
using Android.Views;
using Android.Widget;
using BNet.Mobile.SMS.Services.APIService.Models;
using BNet.Mobile.SMS.Services.TempData;
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
        static List<string> MessagesHistory = new List<string>();
        static List<string> MessageFailed = new List<string>();
        static string tempJson = "";//Stored current message for failed message
        public static void Send(string jsonMessage)
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
                                     // SENT intent



                            IList<string> messages = SmsSender.DivideMessage(message);
                            IList<PendingIntent> sentIntents = new List<PendingIntent>();
                            IList<PendingIntent> deliveredIntents = new List<PendingIntent>();


                            Intent sentIntent = new Intent("SMS_SENT");
                            sentIntent.PutExtra("receiver", receiver);
                            sentIntent.PutExtra("message", message);
                            PendingIntent sentPending = PendingIntent.GetBroadcast(
                                Android.App.Application.Context,
                                i, // unique per part
                                sentIntent,
                                PendingIntentFlags.Immutable
                            );
                            sentIntents.Add(sentPending);
                            // Send the SMS
                            SmsSender.SendMultipartTextMessage(receiver, null, messages, sentIntents, null);
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
        public static async void Remove()
        {
            try
            {
                tempJson = await SendQueue.PeekAsync();
                await SendQueue.DequeueAsync();
            }
            catch { }
        }
        public static void Failed()
        {
            try
            {

                MessageFailed.Add(tempJson);
            }
            catch { }
        }
        public static async void Sent()
        {
            try
            {
                MessagesHistory.Add(await SendQueue.PeekAsync());
            }
            catch { }
        }

        public static int CountSent()
        {
            return MessagesHistory.Count;
        }
        public static int CountFailed()
        {
            return MessageFailed.Count;
        }

        static string RemovePHCountryCode(string phone)
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