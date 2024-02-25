using Csv;
using Microsoft.UI.Xaml.Controls;
using MoonSharp.Interpreter;
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
    private string SavedGamesPath;

    CsvExport CsvDump = new CsvExport(",", true, true);

    public GroupsVMHelper(List<RinceDCSJoystick> joysticks, DCSData data, RinceDCSGroups groups, string savedGamesPath)
    {
        Joysticks = joysticks;
        Data = data;
//        Groups = groups ?? new();
        Groups = new();
        SavedGamesPath = savedGamesPath;

        BuildCache();
    }

    private record ActionWithButtons
        (
        string ActionId,
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

    private record NewGroupAction
        (
        string Group,
        string ActionId,
        RinceDCSGroup Grp
        );

    private record NewGroupAircraft
        (
        RinceDCSGroup Grp,
        string ActionId,
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
        IOrderedEnumerable<ActionWithButtons> allActionsWithButtons = GetAllActionsWithButtons();

        CreateNewModifiers();
        CreateNewGroups(allActionsWithButtons);
        CreateNewGroupJoysticks();
        CreateNewGroupActions(allActionsWithButtons);
        CreateNewGroupAircraft(allActionsWithButtons);
        CreateNewGroupJoystickButtons(allActionsWithButtons);
        AddMissingAircraftFromHTMLFiles();

        if (Settings.Default.CreateGroupsCSVFile)
        {
            string path = Path.GetDirectoryName(Settings.Default.LastSavePath) + "\\RinceDCSGroups.csv";
            CsvDump.ExportToFile(path, Encoding.UTF8);
        }

        return Groups;
    }

    private void CreateNewModifiers()
    {
        var query = from modifier in Data.Modifiers.Values
                    where !Groups.Modifiers.Any(row => row.Key == modifier.Key )
                    select modifier;
        foreach( var modifier in query )
        {
            RinceDCSGroupModifier newModifier = new()
            {
                Name = modifier.Name,
                Key = modifier.Key,
                Device = modifier.Device
            };
            Groups.Modifiers.Add(newModifier);
        }
        if(string.IsNullOrWhiteSpace(Groups.DefaultModifierName))
        {
            Groups.DefaultModifierName = Groups.Modifiers[Groups.Modifiers.Count-1].Name;
        }
    }

    /// <summary>
    /// The DCSData built from the config files only contains data for actions with assigned buttons.
    /// This means some diff.lua files dont even exist if no buttons have been assigned to them and
    /// some actions for an aircraft dont appear as there are no buttons.
    /// 
    /// We want these aircraft attached to any groups that their actions belong to, wether they have
    /// a diff.lua file or not as well as a button or not.
    /// </summary>
    private void AddMissingAircraftFromHTMLFiles()
    {
        string htmlFilesFolder = SavedGamesPath + "\\InputLayoutsTxt";

        foreach (DCSAircraft aircraft in Data.Aircraft.Values)
        {
            foreach(DCSJoystick stick in Data.Joysticks.Values)
            {
                string aircraftStickHtmlPath = htmlFilesFolder + "\\" + aircraft.Key.Name + "\\" + stick.Joystick.DCSName + ".html";
                if (File.Exists(aircraftStickHtmlPath))
                {
                    List<DCSHtmlFileRecord> htmlActions = DCSService.Default.ReadAircraftStickHtmlFile(aircraftStickHtmlPath);

                    var query = from grp in Groups.Groups
                                from grpAction in grp.Actions
                                join htmlAction in htmlActions on grpAction.Id equals htmlAction.Id
                                where !grp.Aircraft.Any(a => a.AircraftName == aircraft.Key.Name)
                                select new { grp, grpAction, htmlAction };
                    foreach(var newAircraft in query)
                    {
                        RinceDCSGroupAircraft grpAircraft = new()
                        {

                            AircraftName = aircraft.Key.Name,
                            ActionId = newAircraft.grpAction.Id,
                            Category = newAircraft.htmlAction.Category,
                            Action = newAircraft.htmlAction.Name,
                            IsActive = true

                        };
                        newAircraft.grp.Aircraft.Add(grpAircraft);
                        newAircraft.grp.AircraftNames.Add(grpAircraft.AircraftName);
                    }
                }
            }
        }
    }

    private void CreateNewGroupJoystickButtons(IOrderedEnumerable<ActionWithButtons> allActionsWithButtons)
    {
        //  Add Buttons to Group Joystick
        var buttons = (from action in allActionsWithButtons
                       from grp in Groups.AllGroups.Values
                       from grpStick in grp.Joysticks
                       where action.Group == grp.Name &&
                             action.StickId == grpStick.Joystick.JoystickGuid
                       select new NewGroupSitckButton
                       (
                           action.Group,
                           action.StickId,
                           action.ButtonName,
                           action.Modifiers,
                           action.AxisFilter,
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

    private void CreateNewGroupAircraft(IOrderedEnumerable<ActionWithButtons> allActionsWithButtons)
    {
        //  Find all Aircraft not part of a group that are a part of a action in the group and add them to group
        var aircraftWithNoGroup = (from action in allActionsWithButtons
                                   join grp in Groups.AllGroups.Values on action.Group equals grp.Name
                                   where !grp.AircraftNames.Contains(action.AircraftName)
                                   select new NewGroupAircraft
                                   (
                                       grp,
                                       action.ActionId,
                                       action.Group,
                                       action.AircraftName,
                                       action.AircraftCommand,
                                       action.AircraftCategory
                                   )).Distinct();

        CsvDump.AddRow();
        CsvDump["Group"] = "#### Aircraft to add to Groups ####";
        foreach (var a in aircraftWithNoGroup)
        {
            CsvDump.AddRow();
            CsvDump["Group"] = a.Group;
            CsvDump["Action Id"] = a.ActionId;
            CsvDump["Aircraft"] = a.AircraftName;
            CsvDump["Aircraft Command"] = a.AircraftCommand;
            CsvDump["Aircraft Category"] = a.AircraftCategory;

            //  Create new Group Aircraft
            RinceDCSGroupAircraft grpAircraft = new() { AircraftName = a.AircraftName, ActionId = a.ActionId, Action = a.AircraftCommand, Category = a.AircraftCategory, IsActive = true };

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

    private void CreateNewGroupActions(IOrderedEnumerable<ActionWithButtons> allActionsWithButtons)
    {
        //  Find all actions not part of a group and add to group 
        var actionsWithNoGroup = (from action in allActionsWithButtons
                                   join grp in Groups.AllGroups.Values on action.Group equals grp.Name
                                   where !Groups.AllActions.Contains(action.ActionId)
                                   select new NewGroupAction
                                   (
                                       action.Group,
                                       action.ActionId,
                                       grp
                                   )).Distinct();

        CsvDump.AddRow();
        CsvDump["Group"] = "#### Actions to add to Groups ####";
        foreach (var a in actionsWithNoGroup)
        {
            CsvDump.AddRow();
            CsvDump["Group"] = a.Group;
            CsvDump["Action"] = a.ActionId;

            //  Create New Action
            RinceDCSGroupAction newAction = new() { Id = a.ActionId, Action = a.Group };
            Groups.AllActions.Add(newAction.Id);
            a.Grp.Actions.Add(newAction);
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

    private void CreateNewGroups(IOrderedEnumerable<ActionWithButtons> allActionsWithButtons)
    {
        //  Find all actions not part of a group and for which no group exists with their Command Name
        var groupsToCreate = (from actionWithNoGroup in allActionsWithButtons
                              where !Groups.AllGroups.ContainsKey(actionWithNoGroup.Group) &&
                                    !Groups.AllActions.Contains(actionWithNoGroup.ActionId)
                              select new NewGroup
                              (
                                  actionWithNoGroup.Group, 
                                  actionWithNoGroup.AircraftCategory, 
                                  actionWithNoGroup.IsAxis
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

    private IOrderedEnumerable<ActionWithButtons> GetAllActionsWithButtons()
    {
        //  Find all actions in game that have a button assigned to them
        var allActionsWithButtons = (from action in Data.Actions.Values
                                      from aircraftStickAction in action.AircraftJoysticks.Values
                                      from button in aircraftStickAction.Buttons.Values
                                      from aircraft in action.Aircraft.Values
                                      where aircraftStickAction.AircraftKey == aircraft.Key
                                      select new ActionWithButtons(
                                        action.Key.Id,
                                        action.Name,
                                        action.IsAxis,
                                        aircraftStickAction.JoystickKey.Id,
                                        aircraftStickAction.AircraftKey.Name,
                                        aircraft.Action,
                                        aircraft.Category,
                                        button.Name,
                                        button.Modifiers,
                                        button.AxisFilter
                                      )).OrderBy(row => row.ActionId)
                                        .ThenBy(row => row.Group)
                                        .ThenByDescending(row => row.AircraftCategory)
                                        .ThenBy(row => row.StickId)
                                        .ThenBy(row => row.AircraftName)
                                        .ThenBy(row => row.ButtonName)
                                        .ThenBy(row => row.AxisFilter);

        CsvDump.AddRow();
        CsvDump["Group"] = "#### All Actions with Buttons ####";
        foreach (var b in allActionsWithButtons)
        {
            CsvDump.AddRow();
            CsvDump["Group"] = b.Group;
            CsvDump["Action Id"] = b.ActionId;
            CsvDump["Is Axis"] = b.IsAxis;
            CsvDump["Stick"] = b.StickId;
            CsvDump["Aircraft"] = b.AircraftName;
            CsvDump["Aircraft Command"] = b.AircraftCommand;
            CsvDump["Aircraft Category"] = b.AircraftCategory;
            CsvDump["Button"] = b.ButtonName;
            CsvDump["Modifiers"] = b.Modifiers.Count();
            CsvDump["Filter"] = b.AxisFilter != null;
        }

        return allActionsWithButtons;
    }

    private void BuildCache()
    {
        Groups.AllGroups.Clear();
        Groups.AllActions.Clear();
        Groups.AllAircraftNames.Clear();

        foreach (RinceDCSGroup grp in Groups.Groups)
        {
            Groups.AllGroups[grp.Name] = grp;
        }
        var actions = (from grp in Groups.Groups
                        from action in grp.Actions
                        select action.Id).Distinct();
        foreach (string actiongId in actions)
        {
            Groups.AllActions.Add(actiongId);
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
