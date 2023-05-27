using RinceDCS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RinceDCS.ServiceModels;

public interface IJoystickService
{
    public List<AttachedJoystick> GetAttachedJoysticks();

    public JoystickInfo GetJoystickInfo(AttachedJoystick attachedJoystick);
}

public class JoystickInfo
{
    public List<string> Buttons { get; set; }
    public List<string> POVs { get; set; }
    public List<string> SupportedAxes { get; set; }
}

