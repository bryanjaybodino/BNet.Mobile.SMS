using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BNet.Mobile.SMS.Services.APIService.Models;
using BNet.Mobile.SMS.Services.SmsService;
using BNet.Mobile.SMS.Services.TempData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BNet.Mobile.SMS.Services.APIService
{
    internal class APIServer
    {
        SendQueue ISendQueue = new SendQueue();
        SaveQueue ISaveQueue = new SaveQueue();

        public void Start()
        {
            Task.Run(async () =>
            {
                HttpListener listener = new HttpListener();
                listener.Prefixes.Add("http://*:8030/");
                listener.Start();

                while (true)
                {
                    var context = listener.GetContext();
                    var request = context.Request;
                    var response = context.Response;

                    response.ContentType = "application/json";

                    // CORS
                    response.Headers.Add("Access-Control-Allow-Origin", "*");
                    response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                    response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

                    // Preflight
                    if (request.HttpMethod == "OPTIONS")
                    {
                        response.StatusCode = 200;
                        response.Close();
                        continue;
                    }

                    if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/status")
                    {
                        response.StatusCode = 200;
                        WriteResponse(response, "{\"status\":\"alive\"}");
                    }
                    else if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/send")
                    {
                        using var reader = new StreamReader(request.InputStream, Encoding.UTF8);
                        string body = reader.ReadToEnd();
                        await ISendQueue.SetQueueAsync(body);
                        response.StatusCode = 200;
                        WriteResponse(response, "{\"status\":\"sent\"}");
                        HtmlElement.Refresh();
                    }
                    else
                    {
                        response.StatusCode = 404;
                        WriteResponse(response, "{\"error\":\"Not Found\"}");
                    }

                    response.Close();
                }
            });
        }
        private void WriteResponse(HttpListenerResponse response, string json)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }
    }
}