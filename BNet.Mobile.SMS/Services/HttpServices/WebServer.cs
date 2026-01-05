using Android.App;
using BNet.Mobile.SMS.Services.NetworkServices;
using BNet.Mobile.SMS.Services.TempDataServices;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BNet.Mobile.SMS.Services.HttpServices
{
    public class WebServer
    {
        static readonly HttpListener listener = new HttpListener();

        public static void Start()
        {
            Task.Run(async () =>
            {
                try
                {
                    int port = NetworkChecker.WebServerPortNumber();

                    listener.Prefixes.Clear();
                    listener.Prefixes.Add($"http://*:{port}/");
                    listener.Start();

                    Console.WriteLine($"WebServer started on port {port}");

                    while (listener.IsListening)
                    {
                        var context = await listener.GetContextAsync();
                        _ = HandleRequestAsync(context);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WebServer error: {ex.Message}");
                }
            });
        }

        private static async Task HandleRequestAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            try
            {
                // ===== CORS =====
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

                if (request.HttpMethod == "OPTIONS")
                {
                    response.StatusCode = 200;
                    response.Close();
                    return;
                }

                // ===== API ROUTES =====
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

                // ===== STATIC FILE ROUTING =====
                // Get the asset path
                string assetPath = request.Url.AbsolutePath.TrimStart('/');

                // SPA fallback: serve index.html for "/"
                if (string.IsNullOrEmpty(assetPath))
                    assetPath = "index.html";

                // Check if file exists in Assets
                bool exists = AssetExists(assetPath);

                if (exists)
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
                Console.WriteLine($"Request error: {ex.Message}");
                response.StatusCode = 500;
            }
            finally
            {
                response.Close();
            }
        }

        // ===== HELPER: Check if asset exists =====
        private static bool AssetExists(string assetPath)
        {
            string dir = Path.GetDirectoryName(assetPath) ?? "";
            string file = Path.GetFileName(assetPath);

            try
            {
                var files = Application.Context.Assets.List(dir);
                return files.Contains(file);
            }
            catch
            {
                return false;
            }
        }

        // ===== HELPER: Serve an asset file =====
        private static async Task ServeAssetFile(HttpListenerResponse response, string assetPath, string contentType)
        {
            try
            {
                using var assetStream = Application.Context.Assets.Open(assetPath);
                using var memoryStream = new MemoryStream();
                await assetStream.CopyToAsync(memoryStream);
                byte[] content = memoryStream.ToArray();

                response.ContentType = contentType;
                response.ContentLength64 = content.Length;
                await response.OutputStream.WriteAsync(content, 0, content.Length);
            }
            catch (FileNotFoundException)
            {
                response.StatusCode = 404;
            }
        }

        // ===== HELPER: Write JSON response =====
        private static async Task WriteJson(HttpListenerResponse response, int statusCode, string json)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            response.ContentType = "application/json";
            response.StatusCode = statusCode;
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }

        // ===== HELPER: Content type detection =====
        private static string GetContentType(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLower();
            return ext switch
            {
                ".html" => "text/html",
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".json" => "application/json",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".svg" => "image/svg+xml",
                ".ico" => "image/x-icon",
                _ => "application/octet-stream",
            };
        }
    }
}
