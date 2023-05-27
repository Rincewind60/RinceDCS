using RinceDCS.Models;
using RinceDCS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RinceDCS.ServiceModels;

public interface IDCSService
{
    public DCSData GetBindingData(string gameName, string gameExePath, string savedGameFolderPath, List<AttachedJoystick> sticks);
}
