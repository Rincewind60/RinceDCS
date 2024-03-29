﻿using Microsoft.UI.Xaml;

namespace RinceDCS.Utilities;

public class GlobalExceptionHandler
{
    public static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Logger.Log.Fatal(e.Exception, "Unhandled Fatal Error");
        Logger.ShutDown();
    }
}
