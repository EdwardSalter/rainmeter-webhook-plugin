using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using HttpMultipartParser;
using Microsoft.Owin;
using Newtonsoft.Json.Linq;
using Owin;
using Rainmeter;

[assembly: OwinStartup(typeof(PluginEmpty.Startup))]

namespace PluginEmpty
{
    public class Startup
    {
        //Used for reading and writing values from the rainmeter settings file
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(
            string section,
            string key,
            string defaultValue,
            [In, Out] char[] value,
            int size,
            string filePath
        );

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WritePrivateProfileString(
            string section,
            string key,
            string value,
            string filePath
        );

        public void Configuration(IAppBuilder app)
        {
            app.Run(context =>
            {
                Plugin._api.Log(API.LogType.Debug, "Received reqeust on webhook");

                string text = "";

                try
                {
                    if (context.Request.ContentType.Contains("multipart/form-data;"))
                    {
                        Plugin._api.Log(API.LogType.Debug, "Webhook has form data");
                        // TODO: CONFIGURABLE
                        var parser = MultipartFormDataParser.Parse(context.Request.Body);

                        text = "{\"data\":{";

                        foreach (var p in parser.Parameters)
                        {
                            text += $"\"{p.Name}\":{p.Data}";
                        }
                        text += "},\"files\":[";

                        // TODO: SKIP DOWNLOAD IF DATA SHOULD BE FILTERED OUT
                        foreach (var file in parser.Files)
                        {
                            string filename = Path.Combine(Plugin.measure.FilesPath, file.FileName);
                            Plugin._api.LogF(API.LogType.Debug, "Saving file to: {0}", filename);
                            using (var fileStream = File.Create(filename))
                            {
                                file.Data.CopyTo(fileStream);
                            }
                            text += $"\"{filename.Replace("\\", "\\\\")}\",";
                        }
                        text = text.TrimEnd(',');

                        text += "]}";
                    }
                    else
                    {
                        StreamReader reader = new StreamReader(context.Request.Body);
                        text = reader.ReadToEnd();
                    }

                    Plugin._api.LogF(API.LogType.Debug, "Received reqeust on webhook: {0}", text);

                    if (!string.IsNullOrWhiteSpace(Plugin.measure.FilterExpression))
                    {
                        Plugin._api.Log(API.LogType.Debug, $"Applying filter expression: {Plugin.measure.FilterExpression}");

                        JObject json = JObject.Parse(text);
                        if (json.SelectToken(Plugin.measure.FilterExpression) == null)
                        {
                            Plugin._api.Log(API.LogType.Debug, "Filter expression did not match, skipping");
                            return context.Response.WriteAsync("Hello, world. Skipped.");
                        }
                    }
                    else
                    {
                        Plugin._api.Log(API.LogType.Debug, "No filter expression configured");
                    }

                    Plugin.UpdateData(text);
                }
                catch (Exception e)
                {
                    Plugin._api.LogF(API.LogType.Error, "Error on webhook: {0}", e.ToString());
                }

                context.Response.ContentType = "text/plain";
                return context.Response.WriteAsync("Hello, world. Updated.");
            });
        }
    }
}
