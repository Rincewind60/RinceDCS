using Csv;
using Microsoft.UI.Xaml.Controls;
using RinceDCS.Models;
using RinceDCS.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RinceDCS.ViewModels.Helpers;
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

        BuildCache();
    }

    private record BindingWithButtons
        (
        string BindingId,
        string Group,
        bool IsAxis,
        Guid StickId,
        string AircraftName,
        string AircraftCommand,
        string AircraftCategory,
        string ButtonName,
        List<string> Modifiers,
        AxisFilter AxisFilter
        );

    private record NewGroup
        (
        string Group,
        string Category,
        bool IsAxis
        );

    private record NewGroupJoystick
        (
        string Group,
        Guid JoystickGuid,
        RinceDCSJoystick Stick,
        RinceDCSGroup Grp
        );

    private record NewGroupBinding
        (
        string Group,
        string BindingId,
        RinceDCSGroup Grp
        );

    private record NewGroupAircraft
        (
        RinceDCSGroup Grp,
        string BindingId,
        string Group,
        string AircraftName,
        string AircraftCommand,
        string AircraftCategory
        );

    private record NewGroupSitckButton
        (
        string Group,
        Guid StickId,
        string ButtonName,
        List<string> Modifiers,
        AxisFilter AxisFilter,
        RinceDCSGroupJoystick GrpStick
        );

    public RinceDCSGroups UpdatedGroups()
    {
        CsvExport csvDump = new CsvExport(",", true, true);

        IOrderedEnumerable<BindingWithButtons> allBindingsWithButtons = GetAllBindingsWithButtons(csvDump);

        CreateNewGroups(csvDump, allBindingsWithButtons);
        CreateNewGroupJoysticks(csvDump);
        CreateNewGroupBindings(csvDump, allBindingsWithButtons);
        CreateNewGroupAircraft(csvDump, allBindingsWithButtons);
        CreateNewGroupJoystickButtons(csvDump, allBindingsWithButtons);

        if (Settings.Default.CreateGroupsCSVFile)
        {
            string path = Path.GetDirectoryName(Settings.Default.LastSavePath) + "\\RinceDCSGroups.csv";
            csvDump.ExportToFile(path, Encoding.UTF8);
        }

        return Groups;
    }

    private void CreateNewGroupJoystickButtons(CsvExport csvDump, IOrderedEnumerable<BindingWithButtons> allBindingsWithButtons)
    {
        //  Add Buttons to Group Joystick
        var buttons = (from binding in allBindingsWithButtons
                       from grp in Groups.AllGroups.Values
                       from grpStick in grp.JoystickBindings
                       where binding.Group == grp.Name &&
                             binding.StickId == grpStick.Joystick.JoystickGuid
                       select new NewGroupSitckButton
                       (
                           binding.Group,
                           binding.StickId,
                           binding.ButtonName,
                           binding.Modifiers,
                           binding.AxisFilter,
                           grpStick
                       )).OrderBy(row => row.Group)
                         .ThenBy(row => row.StickId)
                         .ThenBy(row => row.ButtonName);

        csvDump.AddRow();
        csvDump["Group"] = "#### Add buttons to Group Joysticks ####";
        string prevGroup = "";
        Guid prevStickId = Guid.NewGuid();
        string prevButtonName = "";
        foreach (var a in buttons)
        {
            if (a.Group != prevGroup || a.StickId != prevStickId || a.ButtonName != prevButtonName)
            {
                bool updated = false;

                RinceDCSGroupButton grpButton = a.GrpStick.Buttons.Find(b => b.ButtonName == a.ButtonName);
                if (grpButton == null)
                {
                    //  Create new Group Button
                    grpButton = new() { ButtonName = a.ButtonName };
                    grpButton.Modifiers.AddRange(a.Modifiers);
                    a.GrpStick.Buttons.Add(grpButton);
                    updated = true;
                }

                if (grpButton.AxisFilter == null && a.AxisFilter != null)
                {
                    grpButton.AxisFilter = new(a.AxisFilter);
                    updated = true;
                }

                if(updated)
                {
                    csvDump.AddRow();
                    csvDump["Group"] = a.Group;
                    csvDump["Stick"] = a.StickId;
                    csvDump["Button"] = a.ButtonName;
                    csvDump["Modifiers"] = a.Modifiers.Count();
                    csvDump["Filter"] = a.AxisFilter != null;
                }
            }

            prevGroup = a.Group;
            prevStickId = a.StickId;
            prevButtonName = a.ButtonName;
        }
    }

    private void CreateNewGroupAircraft(CsvExport csvDump, IOrderedEnumerable<BindingWithButtons> allBindingsWithButtons)
    {
        //  Find all Aircraft not part of a group that are a part of a binding in the group and add them to group
        var aircraftWithNoGroup = (from binding in allBindingsWithButtons
                                   join grp in Groups.AllGroups.Values on binding.Group equals grp.Name
                                   where !grp.AircraftNames.Contains(binding.AircraftName)
                                   select new NewGroupAircraft
                                   (
                                       grp,
                                       binding.BindingId,
                                       binding.Group,
                                       binding.AircraftName,
                                       binding.AircraftCommand,
                                       binding.AircraftCategory
                                   )).Distinct();

        csvDump.AddRow();
        csvDump["Group"] = "#### Aircraft to add to Groups ####";
        foreach (var a in aircraftWithNoGroup)
        {
            csvDump.AddRow();
            csvDump["Group"] = a.Group;
            csvDump["Binding"] = a.BindingId;
            csvDump["Aircraft"] = a.AircraftName;
            csvDump["Aircraft Command"] = a.AircraftCommand;
            csvDump["Aircraft Category"] = a.AircraftCategory;

            //  Create new Group Aircraft
            RinceDCSGroupAircraft grpAircraft = new() { AircraftName = a.AircraftName, BindingId = a.BindingId, Command = a.AircraftCommand, Category = a.AircraftCategory, IsActive = true };

            //  Add to group
            a.Grp.AircraftNames.Add(grpAircraft.AircraftName);
            a.Grp.AircraftBindings.Add(grpAircraft);

            //  Add to AllAircraftNames if not there aleady
            if (!Groups.AllAircraftNames.Contains(grpAircraft.AircraftName))
            {
                Groups.AllAircraftNames.Add(grpAircraft.AircraftName);
            }
        }
    }

    private void CreateNewGroupBindings(CsvExport csvDump, IOrderedEnumerable<BindingWithButtons> allBindingsWithButtons)
    {
        //  Find all bindings not part of a group and add to group 
        var bindingsWithNoGroup = (from binding in allBindingsWithButtons
                                   join grp in Groups.AllGroups.Values on binding.Group equals grp.Name
                                   //where !Groups.AllBindings.ContainsKey(binding.BindingId)
                                   where !Groups.AllBindings.Contains(binding.BindingId)
                                   select new NewGroupBinding
                                   (
                                       binding.Group,
                                       binding.BindingId,
                                       grp
                                   )).Distinct();

        csvDump.AddRow();
        csvDump["Group"] = "#### Bindings to add to Groups ####";
        foreach (var a in bindingsWithNoGroup)
        {
            csvDump.AddRow();
            csvDump["Group"] = a.Group;
            csvDump["Binding"] = a.BindingId;

            //  Create New Binding
            RinceDCSGroupBinding newBinding = new() { Id = a.BindingId, Command = a.Group };
            //Groups.AllBindings[newBinding.Id] = newBinding;
            Groups.AllBindings.Add(newBinding.Id);
            a.Grp.Bindings.Add(newBinding);
        }
    }

    private void CreateNewGroupJoysticks(CsvExport csvDump)
    {
        //  Find all Groups that are missing Joysticks add Add sticks to them
        var groupsMissingSticks = (from stick in Joysticks
                                   from grp in Groups.AllGroups.Values
                                   where !grp.JoystickBindings.Any(a => a.Joystick == stick.AttachedJoystick)
                                   select new NewGroupJoystick
                                   (
                                       grp.Name,
                                       stick.AttachedJoystick.JoystickGuid,
                                       stick,
                                       grp
                                   )).OrderBy(row => row.Group)
                                     .ThenBy(row => row.JoystickGuid);

        csvDump.AddRow();
        csvDump["Group"] = "#### Joysticks to add to Groups ####";
        foreach (var s in groupsMissingSticks)
        {
            csvDump.AddRow();
            csvDump["Group"] = s.Group;
            csvDump["Stick"] = s.JoystickGuid;

            //  Create New Joystick
            RinceDCSGroupJoystick newStick = new() { Joystick = s.Stick.AttachedJoystick };

            //  Add to group
            s.Grp.JoystickBindings.Add(newStick);
        }
    }

    private void CreateNewGroups(CsvExport csvDump, IOrderedEnumerable<BindingWithButtons> allBindingsWithButtons)
    {
        //  Find all bindings not part of a group and for which no group exists with their Command Name
        var groupsToCreate = (from bindingWithNoGroup in allBindingsWithButtons
                              where !Groups.AllGroups.ContainsKey(bindingWithNoGroup.Group) &&
                                    //!Groups.AllBindings.ContainsKey(bindingWithNoGroup.BindingId)
                                    !Groups.AllBindings.Contains(bindingWithNoGroup.BindingId)
                              select new NewGroup
                              (
                                  bindingWithNoGroup.Group, 
                                  bindingWithNoGroup.AircraftCategory, 
                                  bindingWithNoGroup.IsAxis
                              )).Distinct()
                                .OrderBy(row => row.Group)
                                .ThenByDescending(row => row.Category);

        csvDump.AddRow();
        csvDump["Group"] = "#### Groups To Create ####";
        string prevGroup = "";
        foreach (var a in groupsToCreate)
        {
            if(a.Group != prevGroup)
            {
                csvDump.AddRow();
                csvDump["Group"] = a.Group;
                csvDump["Category"] = a.Category;
                csvDump["Is Axis"] = a.IsAxis;

                RinceDCSGroup group = new RinceDCSGroup() { Name = a.Group, Category = a.Category, IsAxis = a.IsAxis };
                Groups.AllGroups[a.Group] = group;
                Groups.Groups.Add(group);
            }

            prevGroup = a.Group;
        }
    }

    private IOrderedEnumerable<BindingWithButtons> GetAllBindingsWithButtons(CsvExport csvDump)
    {
        //  Find all bindings in game that have a button assigned to them
        var allBindingsWithButtons = (from binding in Data.Bindings.Values
                                      from aircraftStickBinding in binding.AircraftJoystickBindings.Values
                                      from button in aircraftStickBinding.AssignedButtons.Values
                                      from aircraft in binding.AircraftWithBinding.Values
                                      where aircraftStickBinding.AircraftKey == aircraft.Key
                                      select new BindingWithButtons(
                                        binding.Key.Id,
                                        binding.Command,
                                        binding.IsAxis,
                                        aircraftStickBinding.JoystickKey.Id,
                                        aircraftStickBinding.AircraftKey.Name,
                                        aircraft.Command,
                                        aircraft.Category,
                                        button.Name,
                                        button.Modifiers,
                                        button.AxisFilter
                                      )).OrderBy(row => row.BindingId)
                                        .ThenBy(row => row.Group)
                                        .ThenByDescending(row => row.AircraftCategory)
                                        .ThenBy(row => row.StickId)
                                        .ThenBy(row => row.AircraftName)
                                        .ThenBy(row => row.ButtonName)
                                        .ThenBy(row => row.AxisFilter);

        csvDump.AddRow();
        csvDump["Group"] = "#### All Bindings with Buttons ####";
        foreach (var b in allBindingsWithButtons)
        {
            csvDump.AddRow();
            csvDump["Group"] = b.Group;
            csvDump["Binding"] = b.BindingId;
            csvDump["Is Axis"] = b.IsAxis;
            csvDump["Stick"] = b.StickId;
            csvDump["Aircraft"] = b.AircraftName;
            csvDump["Aircraft Command"] = b.AircraftCommand;
            csvDump["Aircraft Category"] = b.AircraftCategory;
            csvDump["Button"] = b.ButtonName;
            csvDump["Modifiers"] = b.Modifiers.Count();
            csvDump["Filter"] = b.AxisFilter != null;
        }

        return allBindingsWithButtons;
    }

    private void BuildCache()
    {
        Groups.AllGroups.Clear();
        Groups.AllBindings.Clear();
        Groups.AllAircraftNames.Clear();

        foreach (RinceDCSGroup grp in Groups.Groups)
        {
            Groups.AllGroups[grp.Name] = grp;
        }
        var bindings = (from grp in Groups.Groups
                        from binding in grp.Bindings
                        select binding.Id).Distinct();
        foreach (string bindingId in bindings)
        {
            Groups.AllBindings.Add(bindingId);
        }
        var aircraft = (from grp in Groups.Groups
                        from craft in grp.AircraftNames
                        select craft).Distinct();
        foreach (string craft in aircraft)
        {
            Groups.AllAircraftNames.Add(craft);
        }
    }
}
