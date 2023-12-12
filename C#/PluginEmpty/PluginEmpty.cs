using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Rainmeter;

// Overview: This is a blank canvas on which to build your plugin.

// Note: GetString, ExecuteBang and an unnamed function for use as a section variable
// have been commented out. If you need GetString, ExecuteBang, and/or section variables
// and you have read what they are used for from the SDK docs, uncomment the function(s)
// and/or add a function name to use for the section variable function(s).
// Otherwise leave them commented out (or get rid of them)!

namespace PluginEmpty
{
    public class Measure
    {
        public static implicit operator Measure(IntPtr data)
        {
            return (Measure)GCHandle.FromIntPtr(data).Target;
        }

        public IntPtr buffer = IntPtr.Zero;
        public int previousPort { get; set; } = 0;
        public int port { get; set; } = 9000;
        public string Data { get; set; }
        public string FilesPath { get; set; } = Path.GetTempPath();
    }

    public class Plugin
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

        private static IDisposable _server;
        internal static API _api;
        private static string settingsFile;
        public static Measure measure;

        [DllExport]
        public static void Initialize(ref IntPtr data, IntPtr rm)
        {
            data = GCHandle.ToIntPtr(GCHandle.Alloc(new Measure()));
            _api = (API)rm;

            settingsFile = API.GetSettingsFile();

            measure = (Measure)data;
        }

        public static void UpdateData(string data)
        {
            measure.Data = data;
        }

        [DllExport]
        public static void Reload(IntPtr data, IntPtr rm, ref double maxValue)
        {
            Measure measure = (Measure)data;
            API api = (API)rm;

            char[] outString = new char[20480];
            GetPrivateProfileString("Webhook", "Data", "", outString, 20480, settingsFile);
            measure.Data = new string(outString).Trim();

            measure.FilesPath = api.ReadPath("FilesPath", Path.GetTempPath());
            api.Log(API.LogType.Notice, $"FilesPath: {measure.FilesPath}");
            Directory.CreateDirectory(measure.FilesPath);

            //Read measure for an Input string
            measure.port = api.ReadInt("Port", 9000);

            if (measure.port != measure.previousPort)
            {
                measure.previousPort = measure.port;

                if (_server != null)
                {
                    _server.Dispose();
                }

                try
                {
                    _server = Microsoft.Owin.Hosting.WebApp.Start<Startup>(
                        $"http://*:{measure.port}/"
                    );
                    _api.Log(
                        API.LogType.Notice,
                        $"Incoming webhook server is now running at http://*:{measure.port}/"
                    );
                }
                catch (Exception e)
                {
                    _api.Log(
                        API.LogType.Error,
                        $"Error starting incoming webhook server: {e.Message}"
                    );
                }
            }
        }

        [DllExport]
        public static void Finalize(IntPtr data)
        {
            if (_server != null)
            {
                _server.Dispose();
            }
            _server = null;
            _api.Log(API.LogType.Notice, "Incoming webhook server stopped.");

            Measure measure = (Measure)data;
            WritePrivateProfileString("Webhook", "Data", measure.Data, settingsFile);

            if (measure.buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(measure.buffer);
            }
            GCHandle.FromIntPtr(data).Free();
        }

        [DllExport]
        public static double Update(IntPtr data)
        {
            Measure measure = (Measure)data;

            return 0.0;
        }

        [DllExport]
        public static IntPtr GetString(IntPtr data)
        {
            Measure measure = (Measure)data;
            if (measure.buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(measure.buffer);
                measure.buffer = IntPtr.Zero;
            }

            measure.buffer = Marshal.StringToHGlobalUni(measure.Data);

            return measure.buffer;
        }

        //[DllExport]
        //public static void ExecuteBang(IntPtr data, [MarshalAs(UnmanagedType.LPWStr)]String args)
        //{
        //    Measure measure = (Measure)data;
        //}

        //[DllExport]
        //public static IntPtr (IntPtr data, int argc,
        //    [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 1)] string[] argv)
        //{
        //    Measure measure = (Measure)data;
        //    if (measure.buffer != IntPtr.Zero)
        //    {
        //        Marshal.FreeHGlobal(measure.buffer);
        //        measure.buffer = IntPtr.Zero;
        //    }
        //
        //    measure.buffer = Marshal.StringToHGlobalUni("");
        //
        //    return measure.buffer;
        //}
    }
}
