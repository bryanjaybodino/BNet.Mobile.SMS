using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BNet.Mobile.SMS.Services.MyDatabase;
using BNet.Mobile.SMS.Services.MyNetwork;
using BNet.Mobile.SMS.Services.SmsService.Models;
using BNet.Mobile.SMS.Services.TempData;
using Java.Lang;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BNet.Mobile.SMS.Services.SmsService
{
    internal class SaveMessage
    {
        static List<string> MessagesHistory = new List<string>();
        public static async void Saved()
        {
            try
            {
                if (Properties.IsMySQLEnabled())
                {
                    if (NetworkChecker.HasInternet())
                    {
                        var jsonString = await SaveQueue.PeekAsync();
                        var messages = JsonConvert.DeserializeObject<Messages>(jsonString);
                        List<string> data = new List<string>();
                        string query = InsertData(messages.id, messages.body, messages.date, messages.address, messages.type);
                        if (query != "")
                        {
                            data.Add(query);
                        }
                        ExecuteInsertQuery(data);
                    }
                }
            }
            catch { }
        }
        public static int CountSave()
        {
            return MessagesHistory.Count;
        }
        private static async void ExecuteInsertQuery(List<string> value)
        {
            DBQuery_IDU dBQuery_IDU = new DBQuery_IDU();
            string data = string.Join(",", value);
            string Query = data.TrimStart(',', ' ').TrimEnd(',', ' ');
            dBQuery_IDU.Command("INSERT INTO sms_table (sms_id,message,created_date,created_time,address,type) VALUES " + Query, "");
            if (dBQuery_IDU.isSuccess)
            {
                MessagesHistory.Add(await SaveQueue.PeekAsync());
                await SaveQueue.DequeueAsync();
            }
            HtmlElement.Refresh();
        }
        private static string InsertData(string id, string message, string date, string address, string type)
        {
            string data = "";
            try
            {
                long timestamp = Long.ParseLong(date.Substring(0, date.Length - 3)); // NEED TO REMOVE LAST 3 CHARACTER BECAUSE ITS NOT A VALID TIMESTAMP
                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(timestamp);
                DateTime localDateTime = dateTimeOffset.LocalDateTime;
                data = "";
                data = "(";
                data += "'" + id.Replace("'", "ˈ") + "',";//1
                data += "'" + message.Replace("'", "ˈ") + "',";//2
                data += "'" + localDateTime.ToString("yyyy-MM-dd").Replace("'", "ˈ") + "',";//3
                data += "'" + localDateTime.ToString("HH:mm").Replace("'", "ˈ") + "',";//4
                data += "'" + address.Replace("'", "ˈ").Replace(" ", "").Replace(" ", "").Replace("+63", "0").Replace("63", "0") + "',";//5
                data += "'" + type.Replace("'", "ˈ").Replace("1", "Received").Replace("2", "Sent") + "'";//6
                data += ")";
            }
            catch { }
            return data;
        }
    }
}