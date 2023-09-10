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

    public GameBindingGroups UpdatedGroups()
    {
        AddNewBindingsToGroups();
        AddNewJoysticksToGroups();
        AddNewAircraftToGroups();

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

            GameBindingGroup group;
            GameBinding newBinding = new() {  Id = dcsBinding.Key.Id, CommandName = dcsBinding.CommandName };
            Groups.AllBindings[newBinding.Id] = newBinding;

            if (Groups.AllGroups.ContainsKey(dcsBinding.CommandName) && Groups.AllGroups[dcsBinding.CommandName].IsAxisBinding == dcsBinding.IsAxisBinding)
            {
                group = Groups.AllGroups[dcsBinding.CommandName];
            }
            else
            {
                group = new GameBindingGroup() { Name = dcsBinding.CommandName, IsAxisBinding = dcsBinding.IsAxisBinding };
                Groups.AllGroups[dcsBinding.CommandName] = group;
                Groups.Groups.Add(group);
            }
            group.Bindings.Add(newBinding);
        }
    }

    private void AddNewJoysticksToGroups()
    {
        foreach (GameJoystick stick in Joysticks)
        {
            var query = from grp in Groups.Groups
                        where stick != (from stk in grp.Joysticks where stk.Joystick == stick.AttachedJoystick select stk.Joystick)
                        select grp;
            foreach(GameBindingGroup group in query)
            {
                GameBindingJoystick bindingStick = new() { Joystick = stick.AttachedJoystick };
                group.Joysticks.Add(bindingStick);
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
                          !grp.Aircraft.Contains(new GameAircraft(aircraft.Key.Name))
                    select Tuple.Create(grp, grpBinding, aircraft);

        foreach(var newGroupAircraft in query)
        {
            GameBindingGroup group = newGroupAircraft.Item1;
            GameBinding grpBinding = newGroupAircraft.Item2;
            DCSAircraftBinding dCSAircraftBinding = newGroupAircraft.Item3;
            GameAircraft aircraft = new(dCSAircraftBinding.Key.Name);
            GameBoundAircraft boundAircraft = new()
            {
                AircraftName = aircraft.Name,
                BindingId = grpBinding.Id,
                CategoryName = dCSAircraftBinding.CategoryName,
                CommandName = dCSAircraftBinding.CommandName,
                IsActive = true
            };

            group.Aircraft.Add(aircraft);
            group.BoundAircraft.Add(boundAircraft);
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
