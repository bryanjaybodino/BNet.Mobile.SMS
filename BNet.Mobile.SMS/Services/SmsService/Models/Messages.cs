using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BNet.Mobile.SMS.Services.SmsService.Models
{
    internal class Messages
    {
        public string address { get; set; }
        public string body { get; set; }
        public string type { get; set; }
        public string date { get; set; }
        public string id { get; set; }
    }
}