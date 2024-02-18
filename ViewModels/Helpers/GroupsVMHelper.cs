using Csv;
using Microsoft.UI.Xaml.Controls;
using RinceDCS.Models;
using RinceDCS.Properties;
using RinceDCS.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RinceDCS.ViewModels.Helpers;
public class GroupsVMHelper
{
    private List<RinceDCSJoystick> Joysticks { get; }
    private DCSData Data { get; }
    private RinceDCSGroups Groups { get; }
    private string SavedGameFolderPath;

    CsvExport CsvDump = new CsvExport(",", true, true);

    public GroupsVMHelper(List<RinceDCSJoystick> joysticks, DCSData data, RinceDCSGroups groups, string savedGameFolderPath)
    {
        Joysticks = joysticks;
        Data = data;
//        Groups = groups ?? new();
        Groups = new();
        SavedGameFolderPath = savedGameFolderPath;

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
        IOrderedEnumerable<BindingWithButtons> allBindingsWithButtons = GetAllBindingsWithButtons();

        CreateNewGroups(allBindingsWithButtons);
        CreateNewGroupJoysticks();
        CreateNewGroupBindings(allBindingsWithButtons);
        CreateNewGroupAircraft(allBindingsWithButtons);
        CreateNewGroupJoystickButtons(allBindingsWithButtons);
        AddMissingAircraftFromHTMLFiles();

        if (Settings.Default.CreateGroupsCSVFile)
        {
            string path = Path.GetDirectoryName(Settings.Default.LastSavePath) + "\\RinceDCSGroups.csv";
            CsvDump.ExportToFile(path, Encoding.UTF8);
        }

        return Groups;
    }

    /// <summary>
    /// The DCSData built from the config files only contains data for bindings with assigned buttons.
    /// This means some diff.lua files dont even exist if no buttons have been assigned to them and
    /// some bindings for an aircraft dont appear as there are no buttons.
    /// 
    /// We want these aircraft attached to any groups that their bindings belong to, wether they have
    /// a diff.lua file or not as well as a button or not.
    /// </summary>
    private void AddMissingAircraftFromHTMLFiles()
    {
        string htmlFilesFolder = SavedGameFolderPath + "\\InputLayoutsTxt";

        foreach (DCSAircraft aircraft in Data.Aircraft.Values)
        {
            foreach(DCSJoystick stick in Data.Joysticks.Values)
            {
                string aircraftStickHtmlPath = htmlFilesFolder + "\\" + aircraft.Key.Name + "\\" + stick.Joystick.DCSName + ".html";
                if (File.Exists(aircraftStickHtmlPath))
                {
                    List<DCSHtmlFileRecord> htmlBindings = DCSService.Default.ReadAircraftStickHtmlFile(aircraftStickHtmlPath);

                    var query = from grp in Groups.Groups
                                from grpBinding in grp.Bindings
                                join htmlBinding in htmlBindings on grpBinding.Id equals htmlBinding.Id
                                where !grp.Aircraft.Any(a => a.AircraftName == aircraft.Key.Name)
                                select new { grp, grpBinding, htmlBinding };
                    foreach(var newAircraft in query)
                    {
                        RinceDCSGroupAircraft grpAircraft = new()
                        {

                            AircraftName = aircraft.Key.Name,
                            BindingId = newAircraft.grpBinding.Id,
                            Category = newAircraft.htmlBinding.Category,
                            Command = newAircraft.htmlBinding.Name,
                            IsActive = true

                        };
                        newAircraft.grp.Aircraft.Add(grpAircraft);
                        newAircraft.grp.AircraftNames.Add(grpAircraft.AircraftName);
                    }
                }
            }
        }
    }

    private void CreateNewGroupJoystickButtons(IOrderedEnumerable<BindingWithButtons> allBindingsWithButtons)
    {
        //  Add Buttons to Group Joystick
        var buttons = (from binding in allBindingsWithButtons
                       from grp in Groups.AllGroups.Values
                       from grpStick in grp.Joysticks
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

        CsvDump.AddRow();
        CsvDump["Group"] = "#### Add buttons to Group Joysticks ####";
        string prevGroup = "";
        Guid prevStickId = Guid.NewGuid();
        string prevButtonName = "";
        foreach (var a in buttons)
        {
            if (a.Group != prevGroup || a.StickId != prevStickId || a.ButtonName != prevButtonName)
            {
                bool updated = false;

                RinceDCSGroupButton grpButton = a.GrpStick.Buttons.Find(b => b.Name == a.ButtonName);
                if (grpButton == null)
                {
                    //  Create new Group Button
                    grpButton = new() { Name = a.ButtonName };
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
                    CsvDump.AddRow();
                    CsvDump["Group"] = a.Group;
                    CsvDump["Stick"] = a.StickId;
                    CsvDump["Button"] = a.ButtonName;
                    CsvDump["Modifiers"] = a.Modifiers.Count();
                    CsvDump["Filter"] = a.AxisFilter != null;
                }
            }

            prevGroup = a.Group;
            prevStickId = a.StickId;
            prevButtonName = a.ButtonName;
        }
    }

    private void CreateNewGroupAircraft(IOrderedEnumerable<BindingWithButtons> allBindingsWithButtons)
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

        CsvDump.AddRow();
        CsvDump["Group"] = "#### Aircraft to add to Groups ####";
        foreach (var a in aircraftWithNoGroup)
        {
            CsvDump.AddRow();
            CsvDump["Group"] = a.Group;
            CsvDump["Binding"] = a.BindingId;
            CsvDump["Aircraft"] = a.AircraftName;
            CsvDump["Aircraft Command"] = a.AircraftCommand;
            CsvDump["Aircraft Category"] = a.AircraftCategory;

            //  Create new Group Aircraft
            RinceDCSGroupAircraft grpAircraft = new() { AircraftName = a.AircraftName, BindingId = a.BindingId, Command = a.AircraftCommand, Category = a.AircraftCategory, IsActive = true };

            //  Add to group
            a.Grp.AircraftNames.Add(grpAircraft.AircraftName);
            a.Grp.Aircraft.Add(grpAircraft);

            //  Add to AllAircraftNames if not there aleady
            if (!Groups.AllAircraftNames.Contains(grpAircraft.AircraftName))
            {
                Groups.AllAircraftNames.Add(grpAircraft.AircraftName);
            }
        }
    }

    private void CreateNewGroupBindings(IOrderedEnumerable<BindingWithButtons> allBindingsWithButtons)
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

        CsvDump.AddRow();
        CsvDump["Group"] = "#### Bindings to add to Groups ####";
        foreach (var a in bindingsWithNoGroup)
        {
            CsvDump.AddRow();
            CsvDump["Group"] = a.Group;
            CsvDump["Binding"] = a.BindingId;

            //  Create New Binding
            RinceDCSGroupBinding newBinding = new() { Id = a.BindingId, Command = a.Group };
            //Groups.AllBindings[newBinding.Id] = newBinding;
            Groups.AllBindings.Add(newBinding.Id);
            a.Grp.Bindings.Add(newBinding);
        }
    }

    private void CreateNewGroupJoysticks()
    {
        //  Find all Groups that are missing Joysticks add Add sticks to them
        var groupsMissingSticks = (from stick in Joysticks
                                   from grp in Groups.AllGroups.Values
                                   where !grp.Joysticks.Any(a => a.Joystick == stick.AttachedJoystick)
                                   select new NewGroupJoystick
                                   (
                                       grp.Name,
                                       stick.AttachedJoystick.JoystickGuid,
                                       stick,
                                       grp
                                   )).OrderBy(row => row.Group)
                                     .ThenBy(row => row.JoystickGuid);

        CsvDump.AddRow();
        CsvDump["Group"] = "#### Joysticks to add to Groups ####";
        foreach (var s in groupsMissingSticks)
        {
            CsvDump.AddRow();
            CsvDump["Group"] = s.Group;
            CsvDump["Stick"] = s.JoystickGuid;

            //  Create New Joystick
            RinceDCSGroupJoystick newStick = new() { Joystick = s.Stick.AttachedJoystick };

            //  Add to group
            s.Grp.Joysticks.Add(newStick);
        }
    }

    private void CreateNewGroups(IOrderedEnumerable<BindingWithButtons> allBindingsWithButtons)
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

        CsvDump.AddRow();
        CsvDump["Group"] = "#### Groups To Create ####";
        string prevGroup = "";
        foreach (var a in groupsToCreate)
        {
            if(a.Group != prevGroup)
            {
                CsvDump.AddRow();
                CsvDump["Group"] = a.Group;
                CsvDump["Category"] = a.Category;
                CsvDump["Is Axis"] = a.IsAxis;

                RinceDCSGroup group = new RinceDCSGroup() { Name = a.Group, Category = a.Category, IsAxis = a.IsAxis };
                Groups.AllGroups[a.Group] = group;
                Groups.Groups.Add(group);
            }

            prevGroup = a.Group;
        }
    }

    private IOrderedEnumerable<BindingWithButtons> GetAllBindingsWithButtons()
    {
        //  Find all bindings in game that have a button assigned to them
        var allBindingsWithButtons = (from binding in Data.Bindings.Values
                                      from aircraftStickBinding in binding.AircraftJoysticks.Values
                                      from button in aircraftStickBinding.Buttons.Values
                                      from aircraft in binding.Aircraft.Values
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

        CsvDump.AddRow();
        CsvDump["Group"] = "#### All Bindings with Buttons ####";
        foreach (var b in allBindingsWithButtons)
        {
            CsvDump.AddRow();
            CsvDump["Group"] = b.Group;
            CsvDump["Binding"] = b.BindingId;
            CsvDump["Is Axis"] = b.IsAxis;
            CsvDump["Stick"] = b.StickId;
            CsvDump["Aircraft"] = b.AircraftName;
            CsvDump["Aircraft Command"] = b.AircraftCommand;
            CsvDump["Aircraft Category"] = b.AircraftCategory;
            CsvDump["Button"] = b.ButtonName;
            CsvDump["Modifiers"] = b.Modifiers.Count();
            CsvDump["Filter"] = b.AxisFilter != null;
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
