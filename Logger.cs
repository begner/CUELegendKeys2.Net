using System;
using System.Collections.Generic;
using System.Text;
using NLog;

namespace CueLegendKey2
{
    public sealed class Logger
    {
        private static Logger instance = null;
        private static readonly object padlock = new object();

        Logger()
        {
            ConfigLogger();
        }

        public static Logger Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new Logger();
                    }
                    return instance;
                }
            }
        }

        private static readonly NLog.Logger NLogger = NLog.LogManager.GetCurrentClassLogger();

        private void ConfigLogger()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "file.txt" };
            var logconsole = new NLog.Targets.OutputDebugStringTarget("logconsole");

            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logconsole);

            // Apply config           
            NLog.LogManager.Configuration = config;
        }
      

        public void Info(string message)
        {
            NLogger.Info(message);
        }

        public void Debug(string message)
        {
            NLogger.Debug(message);
        }


    }

}

