using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RinceDCS.Models;

public record ManagedButtonKey(string ButtonName, bool IsModifier);

public class ManagedButton
{
    public RinceDCSGroup Group { get; set; }
    public RinceDCSJoystickButton JoystickButton { get; }
    public List<RinceDCSGroup> Groups { get; set; }

    public bool IsValid { get { return true; } }

    public ManagedButton(RinceDCSJoystickButton joyButton, List<RinceDCSGroup> groups)
    {
        JoystickButton = joyButton;
        Groups = groups;
    }
}
