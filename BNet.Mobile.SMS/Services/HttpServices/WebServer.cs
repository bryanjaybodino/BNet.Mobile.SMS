using Android.App;
using BNet.Mobile.SMS.Services.NetworkServices;
using BNet.Mobile.SMS.Services.TempDataServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BNet.Mobile.SMS.Services.HttpServices
{
    public class WebServer
    {
        private static readonly HttpListener listener = new HttpListener();
        private static HashSet<string> _assetCache;

        public static void Start()
        {
            Task.Run(async () =>
            {
                try
                {
                    int port = NetworkChecker.WebServerPortNumber();
                    BuildAssetCache();
                    listener.Prefixes.Add($"http://*:{port}/");
                    listener.Start();
                    Console.WriteLine($"WebServer started on port {port}");
                    // IMPORTANT: Sequential handling (no fire-and-forget)
                    while (listener.IsListening)
                    {
                        var context = await listener.GetContextAsync();
                        await HandleRequestAsync(context);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WebServer error: {ex}");
                }
            });
        }

        private static async Task HandleRequestAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            response.KeepAlive = false;

            try
            {
                // ===== CORS =====
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

                if (request.HttpMethod == "OPTIONS")
                {
                    response.StatusCode = 200;
                    return;
                }

                // ===== API =====
                if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/status")
                {
                    await WriteJson(response, 200, "{\"status\":\"alive\"}");
                    return;
                }

                if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/send")
                {
                    using var reader = new StreamReader(request.InputStream, Encoding.UTF8);
                    string body = await reader.ReadToEndAsync();

                    await SendQueue.SetQueueAsync(body);
                    HtmlElement.Refresh();

                    await WriteJson(response, 200, "{\"status\":\"sent\"}");
                    return;
                }

                // ===== STATIC FILES =====
                string assetPath = request.Url.AbsolutePath.TrimStart('/');
                if (string.IsNullOrEmpty(assetPath))
                    assetPath = "index.html";

                if (_assetCache.Contains(assetPath))
                {
                    string contentType = GetContentType(assetPath);
                    await ServeAssetFile(response, assetPath, contentType);
                    return;
                }

                // ===== NOT FOUND =====
                await WriteJson(response, 404, "{\"error\":\"Not Found\"}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Request error: {ex}");
                response.StatusCode = 500;
            }
            finally
            {
                try { response.OutputStream.Close(); } catch { }
                try { response.Close(); } catch { }
            }
        }

        // ===== ASSET CACHE =====
        private static void BuildAssetCache()
        {
            _assetCache = new HashSet<string>();

            void ScanDir(string path)
            {
                foreach (var file in Application.Context.Assets.List(path))
                {
                    string full = string.IsNullOrEmpty(path) ? file : $"{path}/{file}";

                    try
                    {
                        Application.Context.Assets.Open(full).Close();
                        _assetCache.Add(full);
                    }
                    catch
                    {
                        ScanDir(full);
                    }
                }
            }

            ScanDir("");
        }

        // ===== STREAM FILE (NO MEMORY BUFFERING) =====
        private static async Task ServeAssetFile(HttpListenerResponse response, string assetPath, string contentType)
        {
            response.ContentType = contentType;

            using var assetStream = Application.Context.Assets.Open(assetPath);
            await assetStream.CopyToAsync(response.OutputStream);
        }

        // ===== JSON RESPONSE =====
        private static async Task WriteJson(HttpListenerResponse response, int statusCode, string json)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            response.ContentType = "application/json";
            response.StatusCode = statusCode;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }

        // ===== CONTENT TYPE =====
        private static string GetContentType(string filePath)
        {
            return Path.GetExtension(filePath).ToLower() switch
            {
                ".html" => "text/html",
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".json" => "application/json",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".svg" => "image/svg+xml",
                ".ico" => "image/x-icon",
                _ => "application/octet-stream"
            };
        }
    }
}
