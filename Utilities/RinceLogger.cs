// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

namespace RinceDCS.Utilities;

public class RinceLogger
{
    public static NLog.Logger Log { get; private set; } = NLog.LogManager.GetCurrentClassLogger();
    public static void ShutDown()
    {
        NLog.LogManager.Shutdown();
    }
}
