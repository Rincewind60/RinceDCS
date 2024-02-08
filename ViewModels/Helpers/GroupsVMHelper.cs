using HtmlAgilityPack;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Shapes;
using RinceDCS.Models;
using RinceDCS.Utilities;
using SharpDX.DirectInput;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace RinceDCS.ViewModels.Helpers;

using GroupName = System.String;
using BindingID = System.String;
using AircraftName = System.String;
using static System.Net.Mime.MediaTypeNames;

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
//        string filePath = "RinceDCSGroupsBuildTrace.csv";
//        File.WriteAllText(filePath, "GroupName,BindingId,Command,IsAxisBinding,StickId,AircraftName,AircraftCommand,AircraftCategory,Button,Modifiers,Filter\n");

        //  Find all bindings in game that have a button assigned to them
        var allBindingsWithButtons = (from binding in Data.Bindings.Values
                                      from aircraftStickBinding in binding.AircraftJoystickBindings.Values
                                      from button in aircraftStickBinding.AssignedButtons.Values
                                      from aircraft in binding.AircraftWithBinding.Values
                                      select new {
                                        BindingId = binding.Key.Id,
                                        Command = binding.Command,
                                        binding.IsAxisBinding,
                                        StickId = aircraftStickBinding.JoystickKey.Id,
                                        AircraftName = aircraftStickBinding.AircraftKey.Name,
                                        AircraftCommand = aircraft.Command,
                                        AircraftCategory = aircraft.Category,
                                        ButtonName = button.Name,
                                        button.Modifiers,
                                        button.AxisFilter
                                      }).OrderBy(row => row.BindingId)
                                        .ThenBy(row => row.Command)
                                        .ThenBy(row => row.StickId)
                                        .ThenBy(row => row.AircraftName)
                                        .ThenBy(row => row.ButtonName);

//        File.AppendAllText(filePath, "All Bindings with Buttons\n");
//        foreach (var b in allBindingsWithButtons)
//        {
//            File.AppendAllText(filePath, "," + b.BindingId + "," + b.Command + "," + b.IsAxisBinding + "," + b.StickId + "," + b.AircraftName + "," + b.AircraftCommand + "," + b.AircraftCategory + "," + b.ButtonName + "," + b.Modifiers.Count() + "," + (b.AxisFilter != null) + "\n");
//        }

        //  Find all bindings not part of a group and for which no group exists with their Command Name
        var groupsToCreate = (from bindingWithNoGroup in allBindingsWithButtons
                                   where !Groups.AllGroups.ContainsKey(bindingWithNoGroup.Command) &&
                                         !Groups.AllBindings.ContainsKey(bindingWithNoGroup.BindingId)
                                   select new {
                                       Command = bindingWithNoGroup.Command,
                                       bindingWithNoGroup.IsAxisBinding
                                   }).Distinct();

//        File.AppendAllText(filePath, "Groups To Create\n");
        foreach (var a in groupsToCreate)
        {
//            File.AppendAllText(filePath, a.Command + ",,," + a.IsAxisBinding + "\n");

            //  Create New Group
            RinceDCSGroup group = new RinceDCSGroup() { Name = a.Command, IsAxisBinding = a.IsAxisBinding };
            Groups.AllGroups[a.Command] = group;
            Groups.Groups.Add(group);
        }

        //  Find all Groups that are missing Joysticks add Add sticks to them
        var groupsMissingSticks = (from stick in Joysticks
                                   from grp in Groups.AllGroups.Values
                                   where !grp.JoystickBindings.Any(a => a.Joystick == stick.AttachedJoystick)
                                   select new
                                   {
                                       grp.Name,
                                       stick.AttachedJoystick.JoystickGuid,
                                       stick,
                                       grp
                                   }).OrderBy(row => row.Name)
                                     .ThenBy(row => row.JoystickGuid);

//        File.AppendAllText(filePath, "Joysticks to add to Groups\n");
        foreach (var s in groupsMissingSticks)
        {
//            File.AppendAllText(filePath, s.grp.Name + ",,,," + s.stick.AttachedJoystick.JoystickGuid + "\n");

            //  Create New Joystick
            RinceDCSGroupJoystick newStick = new() { Joystick = s.stick.AttachedJoystick };

            //  Add to group
            s.grp.JoystickBindings.Add(newStick);
        }

        //  Find all bindings not part of a group and add to group 
        var bindingsWithNoGroup = (from binding in allBindingsWithButtons
                                   join grp in Groups.AllGroups.Values on binding.Command equals grp.Name
                                   where !Groups.AllBindings.ContainsKey(binding.BindingId)
                                   select new {
                                        grp,
                                        binding.BindingId,
                                        binding.Command
                                  }).Distinct();

//        File.AppendAllText(filePath, "Bindings to add to Groups\n");
        foreach (var a in bindingsWithNoGroup)
        {
//            File.AppendAllText(filePath, a.grp.Name + "," + a.BindingId + "," + a.Command + "\n");

            //  Create New Binding
            RinceDCSGroupBinding newBinding = new() { Id = a.BindingId, Command = a.Command };
            Groups.AllBindings[newBinding.Id] = newBinding;

            //  Add to group
            a.grp.Bindings.Add(newBinding);
        }

        //  Find all Aircraft not part of a group that are a part of a binding in the group and add them to group
        var aircraftWithNoGroup = (from binding in allBindingsWithButtons
                                  join grp in Groups.AllGroups.Values on binding.Command equals grp.Name
                                  where !grp.AircraftNames.Contains(binding.AircraftName)
                                  select new
                                  {
                                      grp,
                                      binding.BindingId,
                                      binding.Command,
                                      binding.AircraftName,
                                      binding.AircraftCommand,
                                      binding.AircraftCategory
                                  }).Distinct();

//        File.AppendAllText(filePath, "Aircraft to add to Groups\n");
        foreach (var a in aircraftWithNoGroup)
        {
//            File.AppendAllText(filePath, a.grp.Name + "," + a.BindingId + "," + a.Command + ",,," + a.AircraftName + "," + a.AircraftCommand + "," + a.AircraftCategory + "\n");

            //  Create new Group Aircraft
            RinceDCSGroupAircraft grpAircraft = new() { AircraftName = a.AircraftName, BindingId = a.BindingId, Command = a.AircraftCommand, Category = a.AircraftCategory, IsActive = true };

            //  Add to group
            a.grp.AircraftNames.Add(grpAircraft.AircraftName);
            a.grp.AircraftBindings.Add(grpAircraft);

            //  Add to AllAircraftNames if not there aleady
            if(!Groups.AllAircraftNames.ContainsKey(grpAircraft.AircraftName))
            {
                Groups.AllAircraftNames[grpAircraft.AircraftName] = grpAircraft.AircraftName;
            }
        }

        //  Add Buttons to Group Joystick
        var buttons = from binding in allBindingsWithButtons
                      from grp in Groups.AllGroups.Values
                      from grpStick in grp.JoystickBindings
                      where binding.Command == grp.Name &&
                            binding.StickId == grpStick.Joystick.JoystickGuid
                      select new
                      {
                          binding.Command,
                          binding.StickId,
                          binding.ButtonName,
                          binding.Modifiers,
                          binding.AxisFilter,
                          grpStick
                      };

 //       File.AppendAllText(filePath, "Add buttons to Group Joysticks\n");
        foreach (var a in buttons)
        {
//            File.AppendAllText(filePath, a.Command + ",,,," + a.StickId + ",,," + a.ButtonName + "\n");

            RinceDCSGroupButton grpButton = a.grpStick.Buttons.Find(b => b.ButtonName == a.ButtonName);
            if(grpButton == null)
            {
                //  Create new Group Button
                grpButton = new() { ButtonName = a.ButtonName };
                grpButton.Modifiers.AddRange(a.Modifiers);
                a.grpStick.Buttons.Add(grpButton);
            }

            if (grpButton.Filter == null && a.AxisFilter != null)
            {
                grpButton.Filter = new(a.AxisFilter);
            }
        }

        //AddNewBindingsToGroups();
        //AddNewAircraftToGroups();
        //AddNewJoysticksToGroups();

        //DeleteOldBindingsFromGroups();
        //DeleteOldJoysticksFromGroups();
        //DeleteOldAircraftFromGroups();
        //DeleteOldBoundAircraft();

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
                                  (aj.AssignedButtons.Count > 0 || Groups.AllGroups.ContainsKey(binding.Command))
                            select binding;

        foreach (DCSBinding dcsBinding in bindingsToAdd)
        {
            //  We may have found multiple copies of the same binding to add, one for each new Aircraft using binding. So skip if already added.
            if (Groups.AllBindings.ContainsKey(dcsBinding.Key.Id))
                continue;

            RinceDCSGroup group;
            RinceDCSGroupBinding newBinding = new() {  Id = dcsBinding.Key.Id, Command = dcsBinding.Command };
            Groups.AllBindings[newBinding.Id] = newBinding;

            if (Groups.AllGroups.ContainsKey(dcsBinding.Command) && Groups.AllGroups[dcsBinding.Command].IsAxisBinding == dcsBinding.IsAxisBinding)
            {
                group = Groups.AllGroups[dcsBinding.Command];
            }
            else
            {
                group = new RinceDCSGroup() { Name = dcsBinding.Command, IsAxisBinding = dcsBinding.IsAxisBinding };
                Groups.AllGroups[dcsBinding.Command] = group;
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
                        foreach (DCSButton button in newBinding.Item2.AssignedButtons.Values)
                        {
                            RinceDCSGroupButton newButton = new()
                            {
                                ButtonName = button.Name,
                                Modifiers = button.Modifiers.ToList()
                            };
                            if(button.AxisFilter != null)
                            {
                                newButton.Filter = new(button.AxisFilter);
                            }
                            bindingStick.Buttons.Add(newButton);
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
                Category = dCSAircraftBinding.Category,
                Command = dCSAircraftBinding.Command,
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
