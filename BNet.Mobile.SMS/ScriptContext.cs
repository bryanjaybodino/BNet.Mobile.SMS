using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Java.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Essentials;

namespace BNet.Mobile.SMS
{
    public class ScriptContext : Java.Lang.Object
    {
        private readonly Context context;
        private readonly WebView webView;





        public ScriptContext(Context context, WebView webView)
        {
            this.context = context;
            this.webView = webView;
        }


        [JavascriptInterface]
        [Export("Submit")]
        public  void Submit(string data)
        {
           
        }
        public void UpdateValue(string id, string value)
        {
            // Ensure we're on UI thread
            webView.Post(() =>
            {
                value = value.Replace("'", "\\'");
                string js = $"document.getElementById('{id}').value = '{value}';";
                webView.EvaluateJavascript(js, null);
            });
        }
        public void UpdateInnerText(string id,string value)
        {
            // Ensure we're on UI thread
            webView.Post(() =>
            {
                value = value.Replace("'", "\\'");
                string js = $"document.getElementById('{id}').innerText = '{value}';";
                webView.EvaluateJavascript(js, null);
            });
        }
    }
}