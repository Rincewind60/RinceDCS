using Microsoft.UI.Xaml.Data;
using RinceDCS.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
    class BindingGroup
    {
        public GroupName Name { get; set; }
        public Dictionary<AttachedJoystick, JoystickButtons> Sticks { get; set; } = new();
        public Dictionary<BindingID, Binding> Bindings { get; set; } = new();
    }

    class JoystickButtons
    {
        public AttachedJoystick Joystick { get; set; }
        public List<GameAssignedButton> Buttons { get; set; } = new();
    }

    class Binding
    {
        public BindingID Id { get; set; }
        public string CommandName { get; set; }
        public Dictionary<AircraftName, BindingAircraft> BoundAircraft { get; set; } = new();
    }

    class BindingAircraft
    {
        public AircraftName Name { get; set; }
        public bool IsActive { get; set; }
        public string CommandName { get; set; }
        public string CategoryName { get; set; }
    }

    private Dictionary<GroupName, BindingGroup> AllGroups = new();
    private Dictionary<BindingID, Binding> AllBindings { get; set; } = new();

    private List<GameJoystick> Joysticks { get; }
    private DCSData Data { get; }

    public BindingGroupsVMHelper(List<GameJoystick> sticks, DCSData data)
    {
        Joysticks = sticks;
        Data = data;
    }

    public GameBindingGroups GetUpdatedGroups(GameBindingGroups groups)
    {
        CacheExistingBindingGroups(groups);

        List<DCSBinding> bindingsToAdd = GetNewBindingsToAdd(groups);
        List<BindingGroup> updatedGroups = AddNewBindingsToGroups(bindingsToAdd);

        UpdateBindingGroups();

        return CreateUpdatedBindingGroups();
    }

    /// <summary>
    /// As there are over 1000 bindings in DCS we only want to automaticlly add those bindings that the user
    /// might be interested in, otherwise the sheer volumne will be unmanageable.
    /// 
    /// 1. Find all bindings not a member of a current group.
    /// 2. That either have a button binding OR have the same command name as an existing group.
    /// 
    /// </summary>
    /// <param name="groups"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private List<DCSBinding> GetNewBindingsToAdd(GameBindingGroups groups)
    {
        var query = from binding in Data.Bindings.Values
                    from aj in binding.AircraftJoystickBindings.Values
                    where !AllBindings.ContainsKey(binding.Key.Id) && (aj.AssignedButtons.Count > 0 || AllGroups.ContainsKey(binding.CommandName))
                    select binding;

        return query.ToList();
    }

    private List<BindingGroup> AddNewBindingsToGroups(List<DCSBinding> bindingsToAdd)
    {
        List<BindingGroup> updatedGroups = new();

        foreach(DCSBinding dcsBinding in bindingsToAdd)
        {
            BindingGroup group;
            Binding newBinding = new Binding() { Id = dcsBinding.Key.Id, CommandName = dcsBinding.CommandName };

            AllBindings[newBinding.Id] = newBinding;

            if (AllGroups.ContainsKey(dcsBinding.Key.Id))
            {
                group = AllGroups[dcsBinding.Key.Id];
            }
            else
            {
                group = new BindingGroup() { Name = dcsBinding.CommandName };
            }

            group.Bindings[newBinding.Id] = newBinding;
            updatedGroups.Add(group);
        }
        
        return updatedGroups;
    }

    public void UpdateBindingGroups()
    {
    }

    private void CacheExistingBindingGroups(GameBindingGroups groups)
    {
        if(groups == null) return;

        foreach(GameBindingGroup gameGroup in groups.Groups)
        {
            BindingGroup bindingGroup = new();

            foreach(GameJoystick stick in Joysticks)
            {
                bindingGroup.Sticks[stick.AttachedJoystick] = new JoystickButtons();
            }

            foreach(GameBindingJoystick gameStick in gameGroup.JoystickButtons)
            {
                bindingGroup.Sticks[gameStick.Joystick].Buttons = gameStick.Buttons;
            }

            //  Find all Aircraft that have bindings for any binding in this group
            Dictionary<AircraftName, AircraftName> aircraft = new();

            foreach (GameBinding gameBinding in gameGroup.GameBindings)
            {
                DCSBindingKey bindKey = new(gameBinding.Id);
                foreach (DCSAircraftKey aircraftKey in Data.Bindings[bindKey].AircraftWithBinding.Keys)
                {
                    if(!aircraft.ContainsKey(aircraftKey.Name))
                    {
                        aircraft[aircraftKey.Name] = aircraftKey.Name;
                    }
                }
            }

            foreach (GameBinding gameBinding in gameGroup.GameBindings)
            {
                Binding binding = new() { CommandName = gameBinding.CommandName };

                //  Each binding has entries for all aircraft with any bindings in the group
                foreach (AircraftName aircraftName in aircraft.Keys)
                {
                    binding.BoundAircraft[aircraftName] = new BindingAircraft() { IsActive = false };
                }

                foreach (GameBindingAircraft boundAircraft in gameBinding.BoundAircraft)
                {
                    BindingAircraft bindingAircraft = binding.BoundAircraft[boundAircraft.Aircraft.Name];
                    bindingAircraft.CommandName = boundAircraft.CommandName;
                    bindingAircraft.CategoryName = boundAircraft.CategoryName;
                    bindingAircraft.IsActive = true;
                }

                bindingGroup.Bindings[gameBinding.Id] = binding;
                AllBindings[gameBinding.Id] = binding;
            }

            AllGroups[gameGroup.Name] = bindingGroup;
        }
    }

    private void AddNewBindings()
    {
        var newBindingKeys = Data.Bindings.Keys.ExceptBy(AllBindings.Select(a => a.Key), b => b.Id);

        foreach(var newBindingKey in newBindingKeys)
        {
            DCSBinding newBinding = Data.Bindings[newBindingKey];

            BindingGroup group;
            if (AllGroups.ContainsKey(newBinding.CommandName))
            {
                group = AllGroups[newBinding.CommandName];
            }
            else
            {
                group = new();
                AllGroups[newBinding.CommandName] = group;
            }
            AddNewBindingToGroup(group, newBinding);
        }
    }

    private void AddNewBindingToGroup(BindingGroup group, DCSBinding newBinding)
    {
        Binding binding = new() { CommandName = newBinding.CommandName };
        group.Bindings[newBinding.Key.Id] = binding;
        AllBindings[newBinding.Key.Id] = binding;
    }



    private Dictionary<AircraftName, AircraftName> FindAllAircraftForGroup(BindingGroup group)
    {
        //  Find all Aircraft that have bindings for any binding in this group
        Dictionary<AircraftName, AircraftName> aircraft = new();

        foreach(Binding binding in group.Bindings.Values)
        {
            DCSBindingKey bindKey = new(binding.Id);
            foreach (DCSAircraftKey aircraftKey in Data.Bindings[bindKey].AircraftWithBinding.Keys)
            {
                if (!aircraft.ContainsKey(aircraftKey.Name))
                {
                    aircraft[aircraftKey.Name] = aircraftKey.Name;
                }
            }
        }
        return aircraft;
    }

    private GameBindingGroups CreateUpdatedBindingGroups()
    {
        return null;
    }

}
