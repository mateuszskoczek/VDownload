using System;
using System.Collections.Generic;
using System.Diagnostics;
using VDownload.Core.Enums;

namespace VDownload.Core.Services
{
    public class Log
    {
        private static List<(DateTime? Time, string Message, LogMessageType MessageType)> MessageList = new List<(DateTime? Time, string Message, LogMessageType MessageType)>();

        public static void AddHeader(string message)
        {
            MessageList.Add((DateTime.Now, message, LogMessageType.Header));
            Debug.WriteLine(message);
        }

        public static void Add(string message)
        {
            MessageList.Add((DateTime.Now, message, LogMessageType.Normal));
            Debug.WriteLine(message);
        }

        public static void Break()
        {
            MessageList.Add((null, string.Empty, LogMessageType.Break));
        }


        public static void Clear()
        {
            MessageList.Clear();
        }
    }
}
