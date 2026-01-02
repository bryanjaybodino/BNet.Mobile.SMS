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

namespace BNet.Mobile.SMS.Services.APIService.Models
{
    internal class SendPayload
    {
        public string receiver { get; set; }
        public string message { get; set; }
    }
}