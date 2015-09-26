using System;
using System.IO;
using Serilog;
using SerilogWeb.Classic.Enrichers;

namespace Quang.Auth.Api
{
    public class LogConfig
    {
        public static void SetupLog()
        {

            //ApplicationLifecycleModule.DebugLogPostedFormData = true;
            Log.Logger = new LoggerConfiguration().ReadFrom.AppSettings()            
              .WriteTo.RollingFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Logs\log-{Date}.txt"))
              .Enrich.With<HttpRequestIdEnricher>()
              .CreateLogger();

        }
    }
}