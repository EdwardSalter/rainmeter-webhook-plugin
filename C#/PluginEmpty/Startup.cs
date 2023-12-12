using HttpMultipartParser;
using Microsoft.Owin;
using Owin;
using Rainmeter;
using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

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
                Plugin._api.Log(Rainmeter.API.LogType.Debug, "Received reqeust on webhook");

                string text = "";

                try
                {
                    if (context.Request.ContentType.Contains("multipart/form-data;"))
                    {
                        // TODO: CONFIGURABLE
                        var parser = MultipartFormDataParser.Parse(context.Request.Body);

                        text = "{\"data\":{";

                        foreach (var p in parser.Parameters)
                        {
                            text += $"\"{p.Name}\":{p.Data}";
                        }
                        text += "},\"files\":[";

                        foreach (var file in parser.Files)
                        {
                            string filename = Path.Combine(Plugin.measure.FilesPath, file.FileName);
                            Plugin._api.LogF(
                                API.LogType.Debug,
                                "Saving file to: {0}",
                                filename
                            );
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

                    Plugin.UpdateData(text);
                    Plugin._api.LogF(
                        Rainmeter.API.LogType.Debug,
                        "Received reqeust on webhook: {0}",
                        text
                    );
                }
                catch (Exception e)
                {
                    Plugin._api.LogF(
                        Rainmeter.API.LogType.Error,
                        "Error on webhook: {0}",
                        e.Message
                    );
                }

                context.Response.ContentType = "text/plain";
                return context.Response.WriteAsync("Hello, world. Updated.");
            });
        }
    }
}
