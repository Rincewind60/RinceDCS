// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using System.Collections.Generic;

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
