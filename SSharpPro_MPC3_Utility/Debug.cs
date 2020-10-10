using System;
using Crestron.SimplSharp;

namespace SSharpPro_MPC3_Utility
{
    public static class Debug
    {
        public enum ErrorLevel { Notice, Warning, Error, None }

        public static void Log(string msg, ErrorLevel errLevel, bool printToConsole)
        {
            if(printToConsole)
                CrestronConsole.PrintLine(msg);

            if (errLevel != ErrorLevel.None)
            {
                switch (errLevel)
                {
                    case ErrorLevel.Notice:
                        ErrorLog.Notice(msg);
                        break;
                    case ErrorLevel.Warning:
                        ErrorLog.Warn(msg);
                        break;
                    case ErrorLevel.Error:
                        ErrorLog.Error(msg);
                        break;
                }
            }
        }
    }
}