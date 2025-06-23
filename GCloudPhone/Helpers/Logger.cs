using System;
using System.Diagnostics;

namespace GCloudPhone.Helpers
{
    public static class Logger
    {
        public static void LogInfo(string message)
        {
            Debug.WriteLine($"[INFO] {DateTime.Now}: {message}");
        }

        public static void LogError(string message)
        {
            Debug.WriteLine($"[ERROR] {DateTime.Now}: {message}");
        }

        public static void LogError(Exception ex)
        {
            LogError(ex.ToString());
        }
    }
}
