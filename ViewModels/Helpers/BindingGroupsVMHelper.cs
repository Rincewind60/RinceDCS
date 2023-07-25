using Microsoft.UI.Xaml.Data;
using RinceDCS.Models;
using SharpDX.DirectInput;
using System;
using System.Collections;
using System.Collections.Generic;
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
/// TODO: Attached Joysticks , pass in those in game file
/// TODO: Update all groups attached joysticks, add new, delete removed
/// TODO: New Joysticks, copy buttons from DCSData
/// TODO: Add new Groups
/// TODO: Add/Delete Bindings to groups
/// TODO: For each group calc new Aircraft
/// TODO: For each binding, set IsActive false for new bindings/aircraft
/// </summary>


public class BindingGroupsVMHelper
{
    private List<GameJoystick> Joysticks { get; }
    private DCSData Data { get; }
    private GameBindingGroups Groups { get; }

    public BindingGroupsVMHelper(List<GameJoystick> joysticks, DCSData data, GameBindingGroups groups)
    {
        Joysticks = joysticks;
        Data = data;
        Groups = groups ?? new();
    }

    public void UpdatedGroups()
    {
        AddNewBindingsToGroups();
        AddNewJoysticksToGroups();
        AddNewAircraftToGroups();
        AddNewBoundAircraft();

        DeleteOldBindingsFromGroups();
        DeleteOldJoysticksFromGroups();
        DeleteOldAircraftFromGroups();
        DeleteOldBoundAircraft();
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
    private List<GameBindingGroup> AddNewBindingsToGroups()
    {
        Dictionary<string, GameBindingGroup> updatedGroups = new();

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

            GameBindingGroup group;
            GameBinding newBinding = new() {  Id = dcsBinding.Key.Id, CommandName = dcsBinding.CommandName };
            Groups.AllBindings[newBinding.Id] = newBinding;

            if (Groups.AllGroups.ContainsKey(dcsBinding.CommandName))
            {
                group = Groups.AllGroups[dcsBinding.CommandName];
            }
            else
            {
                group = new GameBindingGroup() { Name = dcsBinding.CommandName };
                Groups.AllGroups[dcsBinding.CommandName] = group;
                Groups.Groups.Add(group);
            }
            group.GameBindings.Add(newBinding);

            if(!updatedGroups.ContainsKey(group.Name))
            {
                updatedGroups[group.Name] = group;
            }
        }
        
        return updatedGroups.Values.ToList();
    }

    private void AddNewJoysticksToGroups()
    {
        foreach (GameJoystick stick in Joysticks)
        {
            var query = from grp in Groups.Groups
                        where stick != (from stk in grp.JoystickButtons where stk.Joystick == stick.AttachedJoystick select stk.Joystick)
                        select grp;
            foreach(GameBindingGroup group in query)
            {
                GameBindingJoystick bindingStick = new() { Joystick = stick.AttachedJoystick };
                group.JoystickButtons.Add(bindingStick);
            }
        }
    }

    private void AddNewAircraftToGroups()
    {
        throw new NotImplementedException();
    }

    private void AddNewBoundAircraft()
    {
        throw new NotImplementedException();
    }

    private void DeleteOldBindingsFromGroups()
    {
        throw new NotImplementedException();
    }

    private void DeleteOldJoysticksFromGroups()
    {
        throw new NotImplementedException();
    }

    private void DeleteOldAircraftFromGroups()
    {
        throw new NotImplementedException();
    }

    private void DeleteOldBoundAircraft()
    {
        throw new NotImplementedException();
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
