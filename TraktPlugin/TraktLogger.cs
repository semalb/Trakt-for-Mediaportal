﻿using System;
using System.IO;
using System.Threading;
using MediaPortal.Configuration;
using TraktPlugin.TraktAPI.DataStructures;
using TraktPlugin.TraktAPI.Extensions;

namespace TraktPlugin
{
    static class TraktLogger
    {
        private static Object lockObject = new object();
        private static string logFilename = Config.GetFile(Config.Dir.Log,"TraktPlugin.log");
        private static string logFilePattern = Config.GetFile(Config.Dir.Log, "TraktPlugin.{0}.log");

        internal delegate void OnLogReceivedDelegate(string message, bool error);
        internal static event OnLogReceivedDelegate OnLogReceived;

        static TraktLogger()
        {
            // default logging before we load settings
            TraktSettings.LogLevel = 2;

            #region Log Rollover

            // get max number of log files to create
            int maxLogFiles = 5;

            // delete legacy backup
            DeleteFile(Config.GetFile(Config.Dir.Log, "TraktPlugin.bak"));

            // delete oldest log file if it exists
            string newLogFile = string.Empty;
            string oldLogFile = string.Format(logFilePattern, maxLogFiles);
            DeleteFile(oldLogFile);

            // move the other older log files up one slot
            for (int i = maxLogFiles - 1; i > 0; i--)
            {
                oldLogFile = string.Format(logFilePattern, i);
                newLogFile = string.Format(logFilePattern, i + 1);
                MoveFile(oldLogFile, newLogFile);
            }

            // move the most recent log file up into the backup slots
            newLogFile = string.Format(logFilePattern, 1);
            MoveFile(logFilename, newLogFile);

            #endregion

            // listen to webclient events from the TraktAPI so we can provide useful logging            
            TraktAPI.TraktAPI.OnDataSend += new TraktAPI.TraktAPI.OnDataSendDelegate(TraktAPI_OnDataSend);
            TraktAPI.TraktAPI.OnDataError += new TraktAPI.TraktAPI.OnDataErrorDelegate(TraktAPI_OnDataError);
            TraktAPI.TraktAPI.OnDataReceived += new TraktAPI.TraktAPI.OnDataReceivedDelegate(TraktAPI_OnDataReceived);
        }

        internal static void Info(String log)
        {
            // log to configuration window
            if (TraktSettings.IsConfiguration == true)
                OnLogReceived(log, false);

            if(TraktSettings.LogLevel >= 2)
                writeToFile(String.Format(createPrefix(), "INFO", log));
        }

        internal static void Info(String format, params Object[] args)
        {
            Info(String.Format(format, args));
        }

        internal static void Debug(String log)
        {
            if(TraktSettings.LogLevel >= 3)
                writeToFile(String.Format(createPrefix(), "DEBG", log));
        }

        internal static void Debug(String format, params Object[] args)
        {
            Debug(String.Format(format, args));
        }

        internal static void Error(String log)
        {
            // log to configuration window
            if (TraktSettings.IsConfiguration == true)
                OnLogReceived(log, true);

            if(TraktSettings.LogLevel >= 0)
                writeToFile(String.Format(createPrefix(), "ERR ", log));
        }

        internal static void Error(String format, params Object[] args)
        {
            Error(String.Format(format, args));
        }

        internal static void Warning(String log)
        {
            if(TraktSettings.LogLevel >= 1)
                writeToFile(String.Format(createPrefix(), "WARN", log));
        }

        internal static void Warning(String format, params Object[] args)
        {
            Warning(String.Format(format, args));
        }

        private static String createPrefix()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " [{0}] " + String.Format("[{0}][{1}]", Thread.CurrentThread.Name, Thread.CurrentThread.ManagedThreadId.ToString().PadLeft(2,'0')) +  ": {1}";
        }

        private static void DeleteFile(String log)
        {
            if (File.Exists(log))
            {
                try
                { File.Delete(log); }
                catch { }
            }
        }

        private static void MoveFile(String oldlog, String newLog)
        {
            if (File.Exists(oldlog))
            {
                // Move to next backup slot
                try { File.Move(oldlog, newLog); }
                catch { }
            }
        }

        private static void writeToFile(String log)
        {
            try
            {
                lock (lockObject)
                {
                    StreamWriter sw = File.AppendText(logFilename);
                    sw.WriteLine(log);
                    sw.Close();
                }
            }
            catch { }
        }

        private static void TraktAPI_OnDataSend(string address, string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                TraktLogger.Debug("Address: {0}, Post: {1}", address, data);
            }
            else
            {
                TraktLogger.Debug("Address: {0}", address);
            }
        }

        private static void TraktAPI_OnDataReceived(string response)
        {
            TraktLogger.Debug("Response: {0}", response ?? "null");
        }

        private static void TraktAPI_OnDataError(string error)
        {
            TraktLogger.Error(error);
        }

        /// <summary>
        /// Logs the result of Trakt api call
        /// </summary>
        /// <typeparam name="T">Response Type of message</typeparam>
        /// <param name="response">The response object holding the message to log</param>
        internal static bool LogTraktResponse<T>(T response)
        {
            if (response == null)
            {
                // we already log errors which would normally not be able to be deserialised
                // currently the return value is only being used in livetv/recordings
                return true;
            }

            try
            {
                // only log the response if we don't have debug logging enabled
                // we already log all responses in debug level
                if (TraktSettings.LogLevel < 3)
                {
                    if ((response is TraktSyncResponse))
                    {
                        TraktLogger.Info("Sync Response: {0}", (response as TraktSyncResponse).ToJSON());
                    }
                    else if ((response is TraktScrobbleResponse))
                    {
                        TraktLogger.Info("Scrobble Response: {0}", (response as TraktScrobbleResponse).ToJSON());
                    }
                }

                return true;
            }
            catch (Exception)
            {
                TraktLogger.Info("Response: Failed to interpret response from server");
                return false;
            }
        }
    }
}
