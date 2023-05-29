using RinceDCS.Models;
using System.Collections.Generic;

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

