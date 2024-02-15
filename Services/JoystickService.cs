using RinceDCS.Models;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;

namespace RinceDCS.Services;

public class JoystickInfo
{
    public List<string> Buttons { get; set; }
    public List<string> POVs { get; set; }
    public List<string> SupportedAxes { get; set; }
}

/// <summary>
/// Returns list of attached flight joysticks.
/// 
/// Is designed as a Singleton service and chaches the list of joysticks on first call.
/// This allows and View, ViewModel, Model to get list without depnding on another class.
/// </summary>
public class JoystickService
{
    private static JoystickService defaultInstance = new JoystickService();

    private List<AttachedJoystick> sticks;

    //  Used to help build list of available joystick axes, an array of built in enum values, used to quiry DirectInput.
    private static List<JoystickOffset> JoystickAxisOffsets = new() { JoystickOffset.X, JoystickOffset.Y, JoystickOffset.Z, JoystickOffset.RotationX, JoystickOffset.RotationY, JoystickOffset.RotationZ, JoystickOffset.Sliders0, JoystickOffset.Sliders1 };
    private static string[] DCSjoystickAxisLabels = { "JOY_X", "JOY_Y", "JOY_Z", "JOY_RX", "JOY_RY", "JOY_RZ", "JOY_SLIDER1", "JOY_SLIDER2"};

    public static JoystickService Default
    {
        get { return defaultInstance; }
    }

    public List<AttachedJoystick> GetAttachedJoysticks()
    {
        if (sticks == null)
        {
            DirectInput input = new();
            sticks = new List<AttachedJoystick>();

            foreach (DeviceInstance joy in input.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly))
            {
                //  List of attached joy stick records
                sticks.Add(new AttachedJoystick(joy.InstanceGuid, joy.InstanceName));
            }
        }

        sticks.Sort((x, y) => String.Compare(x.Name, y.Name) );

        return sticks;
    }

    public JoystickInfo GetJoystickInfo(AttachedJoystick attachedJoystick)
    {
        DirectInput input = new();
        JoystickInfo info = new();
        DeviceInstance device = GetDevice(input, attachedJoystick.JoystickGuid);

        var joystick = new Joystick(input, device.InstanceGuid);
        joystick.Acquire();
        JoystickState state = joystick.GetCurrentState();

         info.Buttons = new List<string>();
        for (int i = 1; i < (joystick.Capabilities.ButtonCount + 1); i++)
        {
            info.Buttons.Add("JOY_BTN" + i.ToString());
        }

        info.POVs = new List<string>();
        for(int i = 1; i < (joystick.Capabilities.PovCount + 1); i++)
        {
            info.POVs.Add("JOY_BTN_POV" + i.ToString() + "_U");
            info.POVs.Add("JOY_BTN_POV" + i.ToString() + "_UR");
            info.POVs.Add("JOY_BTN_POV" + i.ToString() + "_R");
            info.POVs.Add("JOY_BTN_POV" + i.ToString() + "_DR");
            info.POVs.Add("JOY_BTN_POV" + i.ToString() + "_D");
            info.POVs.Add("JOY_BTN_POV" + i.ToString() + "_DL");
            info.POVs.Add("JOY_BTN_POV" + i.ToString() + "_L");
            info.POVs.Add("JOY_BTN_POV" + i.ToString() + "_UL");
        }

        info.SupportedAxes = new List<string>();
        for (int i = 0; i < JoystickAxisOffsets.Count; i++)
        {
            try
            {
                var mightGoBoom = joystick.GetObjectInfoByName(JoystickAxisOffsets[i].ToString());
                info.SupportedAxes.Add(DCSjoystickAxisLabels[i]);
            }
            catch { }
        }

        joystick.Unacquire();

        return info;
    }

    private DeviceInstance GetDevice(DirectInput input, Guid joystickGuid)
    {
        foreach(DeviceInstance device in input.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly))
        {
            if(device.InstanceGuid == joystickGuid )
                return device;
        }

        return null;
    }
}

