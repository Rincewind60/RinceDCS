using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RinceDCS.Utilities;

public class RinceLogger
{
    public static NLog.Logger Log { get; private set; } = NLog.LogManager.GetCurrentClassLogger();
    public static void ShutDown()
    {
        NLog.LogManager.Shutdown();
    }
}
