// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using Microsoft.UI.Xaml;

namespace RinceDCS.Utilities;

public class GlobalExceptionHandler
{
    public static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        RinceLogger.Log.Fatal(e.Exception, "Unhandled Fatal Error");
        RinceLogger.ShutDown();
    }
}
