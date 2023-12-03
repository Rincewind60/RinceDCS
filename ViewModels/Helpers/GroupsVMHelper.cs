using HtmlAgilityPack;
using Microsoft.UI.Xaml.Data;
using RinceDCS.Models;
using SharpDX.DirectInput;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace RinceDCS.ViewModels.Helpers;

using GroupName = System.String;
using BindingID = System.String;
using AircraftName = System.String;

/// <summary>
/// TODO: Attached Joysticks , pass in those in RinceDCSFile
/// TODO: Update all groups attached joysticks, add new, delete removed
/// TODO: New Joysticks, copy buttons from DCSData
/// TODO: Add new Groups
/// TODO: Add/Delete Bindings to groups
/// TODO: For each group calc new Aircraft
/// TODO: For each binding, set IsActive false for new bindings/aircraft
/// </summary>


public class GroupsVMHelper
{
    private List<RinceDCSJoystick> Joysticks { get; }
    private DCSData Data { get; }
    private RinceDCSGroups Groups { get; }

    public GroupsVMHelper(List<RinceDCSJoystick> joysticks, DCSData data, RinceDCSGroups groups)
    {
        Joysticks = joysticks;
        Data = data;
        Groups = groups ?? new();
    }

    public RinceDCSGroups UpdatedGroups()
    {
        AddNewBindingsToGroups();
        AddNewAircraftToGroups();
        AddNewJoysticksToGroups();

        DeleteOldBindingsFromGroups();
        DeleteOldJoysticksFromGroups();
        DeleteOldAircraftFromGroups();
        DeleteOldBoundAircraft();

        return Groups;
    }

     /// <summary>
    /// As there are over 1000 bindings in DCS we only want to automaticlly add those bindings that the user
    /// might be interested in, otherwise the sheer volumne will be unmanageable.
    /// 
    /// 1. Find all bindings not a member of a current group.
    /// 2. That either have a button binding OR have the same command name as an existing group.
    /// 
    /// </summary>
    /// <returns></returns>
    private void AddNewBindingsToGroups()
    {
        var bindingsToAdd = from binding in Data.Bindings.Values
                            from aj in binding.AircraftJoystickBindings.Values
                            where !Groups.AllBindings.ContainsKey(binding.Key.Id) &&
                                  (aj.AssignedButtons.Count > 0 || Groups.AllGroups.ContainsKey(binding.CommandName))
                            select binding;

        foreach (DCSBinding dcsBinding in bindingsToAdd)
        {
            //  We may have found multiple copies of the same binding to add, one for each new Aircraft using binding. So skip if already added.
            if (Groups.AllBindings.ContainsKey(dcsBinding.Key.Id))
                continue;

            RinceDCSGroup group;
            RinceDCSGroupBinding newBinding = new() {  Id = dcsBinding.Key.Id, CommandName = dcsBinding.CommandName };
            Groups.AllBindings[newBinding.Id] = newBinding;

            if (Groups.AllGroups.ContainsKey(dcsBinding.CommandName) && Groups.AllGroups[dcsBinding.CommandName].IsAxisBinding == dcsBinding.IsAxisBinding)
            {
                group = Groups.AllGroups[dcsBinding.CommandName];
            }
            else
            {
                group = new RinceDCSGroup() { Name = dcsBinding.CommandName, IsAxisBinding = dcsBinding.IsAxisBinding };
                Groups.AllGroups[dcsBinding.CommandName] = group;
                Groups.Groups.Add(group);
            }
            group.Bindings.Add(newBinding);
        }
    }

    private void AddNewJoysticksToGroups()
    {
        foreach (RinceDCSJoystick stick in Joysticks)
        {
            var query = from grp in Groups.Groups
                        where stick != (from stk in grp.JoystickBindings where stk.Joystick == stick.AttachedJoystick select stk.Joystick)
                        select grp;
            foreach(RinceDCSGroup group in query)
            {
                RinceDCSGroupJoystick bindingStick = new() { Joystick = stick.AttachedJoystick };
                group.JoystickBindings.Add(bindingStick);
                AddButtonsToNewJoystick(group.Bindings, bindingStick);
            }
        }
    }

    private void AddButtonsToNewJoystick(List<RinceDCSGroupBinding> bindings, RinceDCSGroupJoystick bindingStick)
    {
        //var newButtons = from binding in bindings
        //                 from dcsBinding in Data.Bindings.Values
        //                 from dcsAircraftJoystickBinding in dcsBinding.AircraftJoystickBindings.Values
        //                 from button in dcsAircraftJoystickBinding.AssignedButtons.Values
        //                 where binding.Id == dcsBinding.Key.Id &&
        //                       dcsAircraftJoystickBinding.JoystickKey.Id == bindingStick.Joystick.JoystickGuid
        //                 select new
        //                 {
        //                     AircraftName = dcsAircraftJoystickBinding.AircraftKey.Name,
        //                     ButtonName = button.Key.Name,
        //                 };

        //  Find all buttons for each aircraft for current group bindings and joystick
        var newAircraftButtons = from binding in bindings
                                 from dcsBinding in Data.Bindings.Values
                                 from dcsAircraftJoystickBinding in dcsBinding.AircraftJoystickBindings.Values
                                 from button in dcsAircraftJoystickBinding.AssignedButtons.Values
                                 where binding.Id == dcsBinding.Key.Id &&
                                       dcsAircraftJoystickBinding.JoystickKey.Id == bindingStick.Joystick.JoystickGuid
                                 select new { dcsAircraftJoystickBinding, button };

        foreach(var aircraftButton in newAircraftButtons)
        {
            //  Find all groups this aircraft belongs to
            var aircraftGroups = from aircraftGroup in Groups.AllGroups.Values
                                 where aircraftGroup.AircraftNames.Contains(aircraftButton.dcsAircraftJoystickBinding.AircraftKey.Name)
                                 select aircraftGroup;

            if (aircraftGroups.Count() > 0)
            {
                //  Check to see if this button is already defined in one of these groups
                var buttonGroups = from aGroup in aircraftGroups
                                   from joystickBinding in aGroup.JoystickBindings
                                   from button in joystickBinding.Buttons
                                   where joystickBinding.Joystick.JoystickGuid == bindingStick.Joystick.JoystickGuid &&
                                         button.ButtonName == aircraftButton.button.Name
                                   select new { aGroup, joystickBinding, button };

                if(buttonGroups.Count() == 0)
                {
                    //  Find the first DCSDataBinding that has buttons for one of the groups bindings that is also for this joystick
                    Tuple<RinceDCSGroupBinding, DCSAircraftJoystickBinding> newBinding =
                        (from binding in bindings
                         from dcsBinding in Data.Bindings.Values
                         from dcsAircraftJoystickBinding in dcsBinding.AircraftJoystickBindings.Values
                         where binding.Id == dcsBinding.Key.Id &&
                               dcsAircraftJoystickBinding.JoystickKey.Id == bindingStick.Joystick.JoystickGuid &&
                               dcsAircraftJoystickBinding.AssignedButtons.Count > 0
                         select Tuple.Create(binding, dcsAircraftJoystickBinding)).FirstOrDefault();

                    if (newBinding != null)
                    {
                        foreach (IDCSButton button in newBinding.Item2.AssignedButtons.Values)
                        {
                            if (button is DCSAxisButton)
                            {
                                DCSAxisButton dcsAxisButton = (DCSAxisButton)button;
                                RinceDCSGroupAxisButton newAxisButton = new()
                                {
                                    ButtonName = dcsAxisButton.Name,
                                    Curvature = dcsAxisButton.Curvature.ToList(),
                                    Deadzone = dcsAxisButton.Deadzone,
                                    HardwareDetent = dcsAxisButton.HardwareDetent,
                                    HardwareDetentAB = dcsAxisButton.HardwareDetentAB,
                                    HardwareDetentMax = dcsAxisButton.HardwareDetentMax,
                                    Invert = dcsAxisButton.Invert,
                                    SaturationX = dcsAxisButton.SaturationX,
                                    SaturationY = dcsAxisButton.SaturationY,
                                    Slider = dcsAxisButton.Slider
                                };
                                bindingStick.Buttons.Add(newAxisButton);
                            }
                            else
                            {
                                DCSKeyButton dcsKeyButton = (DCSKeyButton)button;
                                RinceDCSGroupKeyButton newKeyButton = new()
                                {
                                    ButtonName = dcsKeyButton.Name,
                                    Modifiers = dcsKeyButton.Modifiers.ToList()
                                };
                                bindingStick.Buttons.Add(newKeyButton);
                            }
                        }
                    }
                }
            }
        }

     }

    private void AddNewAircraftToGroups()
    {
        var query = from grp in Groups.Groups
                    from grpBinding in grp.Bindings
                    from binding in Data.Bindings.Values
                    from aircraft in binding.AircraftWithBinding.Values
                    where binding.Key.Id == grpBinding.Id &&
                          !grp.AircraftNames.Contains(aircraft.Key.Name)
                    select Tuple.Create(grp, grpBinding, aircraft);

        foreach(var newGroupAircraft in query)
        {
            RinceDCSGroup group = newGroupAircraft.Item1;
            RinceDCSGroupBinding grpBinding = newGroupAircraft.Item2;
            DCSAircraftBinding dCSAircraftBinding = newGroupAircraft.Item3;
            RinceDCSGroupAircraft boundAircraft = new()
            {
                AircraftName = dCSAircraftBinding.Key.Name,
                BindingId = grpBinding.Id,
                CategoryName = dCSAircraftBinding.CategoryName,
                CommandName = dCSAircraftBinding.CommandName,
                IsActive = true
            };

            group.AircraftNames.Add(dCSAircraftBinding.Key.Name);
            group.AircraftBindings.Add(boundAircraft);
            if(!Groups.AllAircraftNames.ContainsKey(dCSAircraftBinding.Key.Name))
            {
                Groups.AllAircraftNames[dCSAircraftBinding.Key.Name] = dCSAircraftBinding.Key.Name;
            }
        }
    }

    private void DeleteOldBindingsFromGroups()
    {
//        throw new NotImplementedException();
    }

    private void DeleteOldJoysticksFromGroups()
    {
//        throw new NotImplementedException();
    }

    private void DeleteOldAircraftFromGroups()
    {
//        throw new NotImplementedException();
    }

    private void DeleteOldBoundAircraft()
    {
//        throw new NotImplementedException();
    }

    //private Dictionary<AircraftName, AircraftName> FindAllAircraftForGroup(BindingGroup group)
    //{
    //    //  Find all Aircraft that have bindings for any binding in this group
    //    Dictionary<AircraftName, AircraftName> aircraft = new();

    //    foreach(Binding binding in group.Bindings.Values)
    //    {
    //        DCSBindingKey bindKey = new(binding.Id);
    //        foreach (DCSAircraftKey aircraftKey in Data.Bindings[bindKey].AircraftWithBinding.Keys)
    //        {
    //            if (!aircraft.ContainsKey(aircraftKey.Name))
    //            {
    //                aircraft[aircraftKey.Name] = aircraftKey.Name;
    //            }
    //        }
    //    }
    //    return aircraft;
    //}
}
