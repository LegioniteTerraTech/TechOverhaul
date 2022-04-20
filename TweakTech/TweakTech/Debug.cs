using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TweakTech
{
    internal class Debug
    {
        internal static bool LogAll = false;
        internal static bool ShouldLog = true;

        internal static void Info(string message)
        {
            if (!ShouldLog || !LogAll)
                return;
            if (!message.StartsWith("TweakTech:"))
                message = "TweakTech: " + message;
            UnityEngine.Debug.Log(message);
        }
        internal static void Log(string message)
        {
            if (!ShouldLog)
                return;
            if (!message.StartsWith("TweakTech:"))
                message = "TweakTech: " + message;
            UnityEngine.Debug.Log(message);
        }
        internal static void Log(Exception e)
        {
            if (!ShouldLog)
                return;
            UnityEngine.Debug.Log("TweakTech: " + e);
        }
        internal static void Assert(bool shouldAssert, string message)
        {
            if (!ShouldLog || !shouldAssert)
                return;
            if (!message.StartsWith("TweakTech:"))
                message = "TweakTech: " + message;
            UnityEngine.Debug.Log(message + "\n" + StackTraceUtility.ExtractStackTrace().ToString());
        }
        internal static void LogError(string message)
        {
            if (!ShouldLog)
                return;
            if (!message.StartsWith("TweakTech:"))
                message = "TweakTech: " + message;
            UnityEngine.Debug.Log(message);
        }
        internal static void LogAutoStackTrace(string message)
        {
            if (!ShouldLog)
                return;
            if (!message.StartsWith("TweakTech:"))
                message = "TweakTech: " + message;
            UnityEngine.Debug.Log(message + "\n" + StackTraceUtility.ExtractStackTrace().ToString());
        }
        internal static void FatalError(Exception e)
        {
            ManUI.inst.ShowErrorPopup("TweakTech: ENCOUNTERED CRITICAL ERROR: " + e);
            UnityEngine.Debug.Log("TweakTech: ENCOUNTERED CRITICAL ERROR");
            UnityEngine.Debug.Log("TweakTech: MAY NOT WORK PROPERLY AFTER THIS ERROR, PLEASE REPORT!");
        }
    }
}
