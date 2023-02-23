using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Turing.Tools
{
    public class ScreenshotTool
    {
        private const string tools = "Screenshot";
        
        [MenuItem(tools + "/Screenshot 1X", false, 500)]
        public static void TakeScreenshot1X() => TakeScreenshot(1);

        [MenuItem(tools + "/Screenshot 2X", false, 501)]
        public static void TakeScreenshot2X() => TakeScreenshot(2);

        private static void TakeScreenshot(int superSize)
        {
            var dir = Application.dataPath + "/Screenshots/";
            Directory.CreateDirectory(dir);
            ScreenCapture.CaptureScreenshot(
                dir + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-") + superSize + "X.png",
                superSize);
            AssetDatabase.Refresh();
        }
    }
}