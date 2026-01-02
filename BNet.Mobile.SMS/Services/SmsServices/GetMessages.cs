using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BNet.Mobile.SMS.Services.SmsServices.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BNet.Mobile.SMS.Services.SmsServices
{
    internal class GetMessages
    {
        public static List<Messages> RetriveBy(string message, string contact)
        {

            List<Messages> retrives = new List<Messages>();
            ////var currentTime = DateTime.Now;
            ////const int timeLimitMinutes = 5;

            string SortOrder = "date DESC LIMIT 1"; // Sort by date in descending order (latest first)  
            string selection = "body = ? AND address = ? "; // Replace "specific_id" with the actual ID you want to search for
            string[] selectionArgs = new string[] { message, contact };
            var inboxUri = Android.Net.Uri.Parse("content://sms/");
            var cursor = Android.App.Application.Context.ContentResolver.Query(inboxUri, null, selection, selectionArgs, SortOrder);
            if (cursor != null)
            {
                while (cursor.MoveToNext())
                {
                    var address = cursor.GetString(cursor.GetColumnIndex("address"));
                    var body = cursor.GetString(cursor.GetColumnIndex("body"));
                    var type = cursor.GetString(cursor.GetColumnIndex("type"));
                    var date = cursor.GetString(cursor.GetColumnIndex("date"));
                    var _id = cursor.GetString(cursor.GetColumnIndex("_id"));
                    retrives.Add(new Messages
                    {
                        address = address,
                        body = body,
                        type = type,
                        date = date,
                        id = _id,
                    });
                    //// CHECK THE TIME IF LESS THAN 5 Minute
                    //if (DateTime.TryParse(date, out DateTime messageDate))
                    //{
                    //    var timeDifference = currentTime - messageDate;
                    //    if (timeDifference.TotalMinutes < timeLimitMinutes)
                    //    {
                    //    }
                    // }


                }
                cursor.Close();
            }
            return retrives;
        }
    }
}