using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BNet.Mobile.SMS.Services.HttpListenerServices.Models;
using BNet.Mobile.SMS.Services.NetworkServices;
using BNet.Mobile.SMS.Services.SmsServices;
using BNet.Mobile.SMS.Services.TempDataServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BNet.Mobile.SMS.Services.HttpListenerServices
{
    public class WebServer
    {
        static HttpListener listener = new HttpListener();
        public static void Start()
        {
            Task.Run(async () =>
            {
                try
                {
                    int port = NetworkChecker.PortNumber();
                    listener.Prefixes.Remove($"http://*:{port}/");
                    listener.Prefixes.Add($"http://*:{port}/");
                    listener.Start();

                    while (true)
                    {
                        var context = await listener.GetContextAsync();
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
                            await WriteResponse(response, "{\"status\":\"alive\"}");
                        }
                        else if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/send")
                        {
                            using var reader = new StreamReader(request.InputStream, Encoding.UTF8);
                            string body = await reader.ReadToEndAsync();
                            await SendQueue.SetQueueAsync(body);
                            response.StatusCode = 200;
                            await WriteResponse(response, "{\"status\":\"sent\"}");
                            HtmlElement.Refresh();  // Make sure this method is safe to use in an async context
                        }
                        else
                        {
                            response.StatusCode = 404;
                            await WriteResponse(response, "{\"error\":\"Not Found\"}");
                        }

                        response.Close();
                        await Task.Delay(100); // yield to OS
                    }
                }
                catch (Exception ex)
                {
                    // Log the error to debug if needed
                    Console.WriteLine($"Error: {ex.Message}");
                }
            });
        }

        private static async Task WriteResponse(HttpListenerResponse response, string json)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
    }
}
