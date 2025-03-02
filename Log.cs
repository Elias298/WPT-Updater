using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPT_Updater;


public static class Log
{
    private static readonly string logFilePath = "log.txt"; // You can change the path

    public static void WriteLine(string message)
    {
        string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
        File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
    }
}

