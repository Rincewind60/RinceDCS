﻿using CommunityToolkit.Mvvm.DependencyInjection;
using HtmlAgilityPack;
using Microsoft.VisualBasic;
using MoonSharp.Interpreter;
using RinceDCS.Models;
using RinceDCS.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Windows.Media.AppBroadcasting;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RinceDCS.Services;

public record DCSHtmlFileRecord(string Name, string Category, string Id);

public class DCSService
{
    private static DCSService defaultInstance = new DCSService();

    public static DCSService Default
    {
        get { return defaultInstance; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gameName"></param>
    /// <param name="gameExePath"></param>
    /// <param name="savedGamesPath"></param>
    /// <param name="sticks"></param>
    /// <returns></returns>
    public DCSData GetControlsData(string gameName, string gameExePath, string savedGamesPath, List<AttachedJoystick> sticks)
    {
        ///TODO: If no diff.lua file exists for an aircraft/joystick combination then assume there should be (the default)
        DCSData data = new();

        BuildListOfJoysticks(data, sticks);

        ReadDefaultModifiersLua(data, gameExePath);

        string htmlFilesFolder = savedGamesPath + "\\InputLayoutsTxt";
        if (Directory.Exists(htmlFilesFolder))
        {
            BuildListOfAircraftFromHTMLFiles(data, htmlFilesFolder);
            BuildActionsFromHTMLFiles(data, htmlFilesFolder);
        }

        string savedGamesAircraftPath = savedGamesPath + "\\Config\\Input";
        if (Directory.Exists(savedGamesAircraftPath))
        {
            ReadAircraftModifiersLua(data, savedGamesAircraftPath);
            BuildButtonActionsFromSavedGame(data, savedGamesAircraftPath);
        }

        return data;
    }

    private void ReadDefaultModifiersLua(DCSData data, string gameExePath)
    {
        string modifierPath = Path.GetDirectoryName(Path.GetDirectoryName(gameExePath)) + "\\Config\\Input\\Aircrafts\\modifiers.lua";

        Table table = Script.RunFile(modifierPath).Table;

        for(int i = 0; i < table.Keys.Count(); i++)
        {
            DCSModifier newModifier = new();
            newModifier.Name = table.Keys.ElementAt(i).String;
            Table modiferTable = table.Values.ElementAt(i).Table;
            for(int j = 0; j < modiferTable.Keys.Count(); j++)
            {
                string modifierPropertyName = modiferTable.Keys.ElementAt(j).String;
                if(modifierPropertyName == "device")
                {
                    newModifier.Device = modiferTable.Values.ElementAt(j).String;
                }
                else if(modifierPropertyName == "key")
                {
                    newModifier.Key = modiferTable.Values.ElementAt(j).String;
                }
            }
            data.Modifiers[newModifier.Name] = newModifier;
        }
    }

    private void ReadAircraftModifiersLua(DCSData data, string savedGamesAircraftPath)
    {
        foreach (DCSAircraft aircraft in data.Aircraft.Values)
        {
            string modifierPath = savedGamesAircraftPath + "\\" + aircraft.Key.Name + "\\modifiers.lua";
            if(File.Exists(modifierPath))
            {
                Table table = Script.RunFile(modifierPath).Table;
                for(int i = 0; i < table.Keys.Count(); i++)
                {
                    string name = table.Keys.ElementAt(i).String;
                    if(!data.Modifiers.ContainsKey(name))
                    {
                        DCSModifier newModifier = new();
                        newModifier.Name = name;
                        Table modiferTable = table.Values.ElementAt(i).Table;
                        for(int j = 0; j < modiferTable.Keys.Count(); j++)
                        {
                            string modifierPropertyName = modiferTable.Keys.ElementAt(j).String;
                            if(modifierPropertyName == "device")
                            {
                                newModifier.Device = modiferTable.Values.ElementAt(j).String;
                            }
                            else if(modifierPropertyName == "key")
                            {
                                newModifier.Key = modiferTable.Values.ElementAt(j).String;
                            }
                        }
                        data.Modifiers[newModifier.Name] = newModifier;
                    }
                }
            }
        }
    }

    public string GetDCSSavedGamesPath(string gameFolderPath, string currentSavedGamesFolder)
    {
        string savedGamesFolder = FileService.Default.GetSavedGamesFolderPath();
        string variantFilePath = gameFolderPath + "\\dcs_variant.txt";
        string variantName = null;

        if (File.Exists(variantFilePath))
        {
            variantName = File.ReadAllText(variantFilePath);
        }

        string newSavedGamesFolder = savedGamesFolder + "\\DCS";
        if (!string.IsNullOrWhiteSpace(variantName))
        {
            newSavedGamesFolder += "." + variantName;
        }

        if (Directory.Exists(newSavedGamesFolder))
        {
            return newSavedGamesFolder;
        }

        return currentSavedGamesFolder;
    }

    public List<DCSHtmlFileRecord> ReadAircraftStickHtmlFile(string aircraftStickHtmlPath)
    {
        List<DCSHtmlFileRecord> actions = new();
        HtmlDocument document = new();

        document.Load(aircraftStickHtmlPath);
        var rows = document.DocumentNode.SelectNodes("//tr");
        for (int i = 1; i < rows.Count; i++)
        {
            HtmlNode row = rows[i];
            actions.Add(new DCSHtmlFileRecord(row.ChildNodes[3].GetDirectInnerText().Trim(),
                                              row.ChildNodes[5].GetDirectInnerText().Trim().Split(";").First(),
                                              row.ChildNodes[7].GetDirectInnerText().Trim()));
        }
        return actions;
    }

    public void UpdateDCSConfigFiles(string savedGamesPath, RinceDCSGroups groups, DCSData data, List<string> aircraftNames)
    {
        BackupDCSFiles(savedGamesPath, aircraftNames);

        BuildModifiersFiles(savedGamesPath, groups, aircraftNames);

        //  Find all RinceDCS buttons to be added
        var rinceButtons = from grp in groups.Groups
                           from aircraft in grp.Aircraft
                           from gj in grp.Joysticks
                           from button in gj.Buttons
                           where aircraft.IsActive == true &&
                                 aircraftNames.Contains(aircraft.AircraftName)
                           select new GameUpdateButton()
                           {
                               AircraftName = aircraft.AircraftName,
                               Joystick = gj.Joystick,
                               IsAxis = grp.IsAxis,
                               ActionId = aircraft.ActionId,
                               Action = aircraft.Action,
                               AddButton = new DCSButton() { Name = button.Name, AxisFilter = button.AxisFilter, Modifiers = button.Modifiers }
                           };

        //  Find all DCS removed buttons that are not part of the RinceDCS buttons to Add, these still need to be removed
        var dcsRemoveButtons = (from dcsAction in data.Actions.Values
                                from ajb in dcsAction.AircraftJoysticks.Values
                                from button in ajb.ButtonChanges.Removed
                                select new GameUpdateButton()
                                {
                                    AircraftName = ajb.AircraftKey.Name,
                                    Joystick = data.Joysticks[ajb.JoystickKey].Joystick,
                                    IsAxis = dcsAction.IsAxis,
                                    ActionId = dcsAction.Key.Id,
                                    Action = dcsAction.Name,
                                    RemoveButton = button
                                }).Except(rinceButtons, new GameUpdateButtonComparer());

        //  Find all DCS add buttons that are not part of the RinceDCS buttons to Add, these now need to be removed
        var dcsAddedButtons = (from dcsAction in data.Actions.Values
                               from ajb in dcsAction.AircraftJoysticks.Values
                               from button in ajb.ButtonChanges.Added
                               select new GameUpdateButton()
                               {
                                   AircraftName = ajb.AircraftKey.Name,
                                   Joystick = data.Joysticks[ajb.JoystickKey].Joystick,
                                   IsAxis = dcsAction.IsAxis,
                                   ActionId = dcsAction.Key.Id,
                                   Action = dcsAction.Name,
                                   RemoveButton = button
                               }).Except(rinceButtons, new GameUpdateButtonComparer());

        var updates = from update in rinceButtons.Concat(dcsRemoveButtons).Concat(dcsAddedButtons) select update;
        BuildLuaFiles(savedGamesPath, updates, aircraftNames);
    }

    private void BuildModifiersFiles(string savedGamesPath, RinceDCSGroups groups, List<string> aircraftNames)
    {
        StringBuilder sb = new();
        sb.AppendLine("local modifiers = {");
        foreach(RinceDCSGroupModifier modifier in groups.Modifiers)
        {
            sb.AppendLine("\t[\"" + modifier.Name + "\"] = {");
            sb.AppendLine("\t\t[\"device\"] = \"" + modifier.Device + "\",");
            sb.AppendLine("\t\t[\"key\"] = \"" + modifier.Key + "\",");
            sb.AppendLine("\t\t[\"switch\"] = false,");
            sb.AppendLine("\t},");
        }
        sb.AppendLine("}");
        sb.AppendLine("return modifiers");
        string luaString = sb.ToString();
        string configFolder = savedGamesPath + "\\Config\\Input";

        string TestFolder = "S:\\RinceConfigBackup\\Input\\";

        foreach (string aircraft in aircraftNames)
        {
            string modifierPath = TestFolder + "\\" + aircraft + "\\modifiers.lua";
            File.WriteAllText(modifierPath, luaString);
        }
    }

    private void BuildLuaFiles(string savedGamesPath, IEnumerable<GameUpdateButton> updates, List<string> aircraftNames)
    {
        var ordedUpdates = updates.OrderBy(row => row.AircraftName)
                                  .ThenBy(row => row.Joystick.Name)
                                  .ThenByDescending(row => row.IsAxis)
                                  .ThenBy(row => row.ActionId)
                                  .ThenBy(row => row.AddButton != null);

        DCSLuaFileBuilder luaBuilder = null;
        string prevAircraft = "";
        AttachedJoystick prevJoystick = null;
        GameUpdateButton prevUpdate = null;
        int buttonIndex = 0;
        foreach (GameUpdateButton update in ordedUpdates)
        {
            if (update.AircraftName != prevAircraft || update.Joystick != prevJoystick)
            {
                if (luaBuilder != null)
                {
                    FinalizePreviousFile(luaBuilder, prevUpdate);
                    luaBuilder.WriteFile();
                }

                string TestFolder = "S:/RinceConfigBackup/Input/" + update.AircraftName + "/joystick/";
                string luaFileName = TestFolder + update.Joystick.DCSName + ".diff.lua";
                if (!Directory.Exists(TestFolder))
                {
                    Directory.CreateDirectory(TestFolder);
                }
                luaBuilder = new(luaFileName);
                luaBuilder.AppendHeader();

                prevAircraft = update.AircraftName;
                prevJoystick = update.Joystick;
                prevUpdate = null;
                buttonIndex = 0;
            }

            if (prevUpdate == null)
            {
                if (update.IsAxis)
                {
                    luaBuilder.AppendAxisHeader();
                }
                else
                {
                    luaBuilder.AppendKeyHeader();
                }
                luaBuilder.AppendActionHeader(update);
                if (update.AddButton != null)
                {
                    luaBuilder.AppendAddHeader();
                }
                else
                {
                    luaBuilder.AppendActionName(update);
                    luaBuilder.AppendRemoveHeader();
                }
                buttonIndex = 0;
            }
            else
            {
                if (prevUpdate.IsAxis != update.IsAxis)
                {
                    if (prevUpdate.AddButton != null)
                    {
                        luaBuilder.AppendAddFooter();
                        luaBuilder.AppendActionName(prevUpdate);
                    }
                    if (prevUpdate.RemoveButton != null)
                    {
                        luaBuilder.AppendRemoveFooter();
                    }
                    luaBuilder.AppendActionFooter();
                    luaBuilder.AppendAxisFooter();

                    if (update.IsAxis)
                    {
                        luaBuilder.AppendAxisHeader();
                    }
                    else
                    {
                        luaBuilder.AppendKeyHeader();
                    }
                    luaBuilder.AppendActionHeader(update);
                    if (update.AddButton != null)
                    {
                        luaBuilder.AppendAddHeader();
                    }
                    else
                    {
                        luaBuilder.AppendRemoveHeader();
                    }
                    buttonIndex = 0;
                }
                else if (prevUpdate.ActionId != update.ActionId)
                {
                    if (prevUpdate.AddButton != null)
                    {
                        luaBuilder.AppendAddFooter();
                        luaBuilder.AppendActionName(prevUpdate);
                    }
                    if (prevUpdate.RemoveButton != null)
                    {
                        luaBuilder.AppendRemoveFooter();
                    }
                    luaBuilder.AppendActionFooter();

                    luaBuilder.AppendActionHeader(update);
                    if (update.AddButton != null)
                    {
                        luaBuilder.AppendAddHeader();
                    }
                    else
                    {
                        luaBuilder.AppendActionName(update);
                        luaBuilder.AppendRemoveHeader();
                    }
                    buttonIndex = 0;
                }
                else if (prevUpdate.AddButton != null && update.RemoveButton != null)
                {
                    luaBuilder.AppendAddFooter();
                    luaBuilder.AppendRemoveHeader();
                    buttonIndex = 0;
                }
            }

            luaBuilder.AppendButton(update, buttonIndex);
            prevUpdate = update;
            buttonIndex += 1;
        }
        if (luaBuilder != null)
        {
            FinalizePreviousFile(luaBuilder, prevUpdate);
            luaBuilder.WriteFile();
        }
    }

    private static void FinalizePreviousFile(DCSLuaFileBuilder luaBuilder, GameUpdateButton prevUpdate)
    {
        if (prevUpdate.AddButton != null)
        {
            luaBuilder.AppendAddFooter();
            luaBuilder.AppendActionName(prevUpdate);
        }
        else
        {
            luaBuilder.AppendRemoveFooter();
        }
        luaBuilder.AppendActionFooter();
        if (prevUpdate.IsAxis)
        {
            luaBuilder.AppendAxisFooter();
        }
        else
        {
            luaBuilder.AppendKeyFooter();
        }
        luaBuilder.AppendFooter();
    }

    private static void BackupDCSFiles(string savedGamesPath, List<string> aircraftNames)
    {
        string backupFolder = savedGamesPath + "\\RinceDCS\\Backups\\Config_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + "\\Input";

        Directory.CreateDirectory(backupFolder);

        //  Copy existing Aircraft config files to backup, only for Aircraft being updated
        string fromConfigFolder = savedGamesPath + "\\Config\\Input";
        foreach (string aircraft in aircraftNames)
        {
            string fromAircraftFolder = fromConfigFolder + "\\" + aircraft;
            string toAircraftFolder = backupFolder + "\\" + aircraft;
            Directory.CreateDirectory(toAircraftFolder);

            string fromModifersFileName = fromAircraftFolder + "\\modifiers.lua";
            string toModifiersFileName = toAircraftFolder + "\\modifiers.lua";
            if (File.Exists(fromModifersFileName))
            {
                File.Copy(fromModifersFileName, toModifiersFileName);
            }

            string fromJoystickFolder = fromAircraftFolder + "\\joystick";
            string toJoystickFolder = toAircraftFolder + "\\joystick";

            Directory.CreateDirectory(toJoystickFolder);

            foreach (string filePath in Directory.GetFiles(fromJoystickFolder))
            {
                string fileName = Path.GetFileName(filePath);
                File.Copy(fromJoystickFolder + "\\" + fileName, toJoystickFolder + "\\" + fileName, true);
            }
        }
    }

    private static void BuildListOfJoysticks(DCSData data, List<AttachedJoystick> sticks)
    {
        foreach (AttachedJoystick joystick in sticks)
        {
            DCSJoystickKey key = new(joystick.JoystickGuid);
            data.Joysticks[key] = new DCSJoystick() { Key = key, Joystick = joystick };
        }
    }

    private void BuildListOfAircraftFromHTMLFiles(DCSData data, string htmlFolderPath)
    {
        foreach (string aircraftFolder in Directory.GetDirectories(htmlFolderPath))
        {
            DCSAircraftKey key = new(aircraftFolder.Split("\\").Last());
            data.Aircraft[key] = new DCSAircraft() { Key = key };
        }
    }

    private void BuildActionsFromHTMLFiles(DCSData data, string htmlFolderPath)
    {
        foreach (DCSAircraft aircraft in data.Aircraft.Values)
        {
            foreach (DCSJoystick stick in data.Joysticks.Values)
            {
                string aircraftStickHtmlPath = htmlFolderPath + "\\" + aircraft.Key.Name + "\\" + stick.Joystick.DCSName + ".html";
                if (File.Exists(aircraftStickHtmlPath))
                {
                    List<DCSHtmlFileRecord>  htmlActions = ReadAircraftStickHtmlFile(aircraftStickHtmlPath);
                    BuildHTMLActions(data, aircraft, stick, htmlActions);
                }
            }
        }
    }

    private void BuildHTMLActions(DCSData data, DCSAircraft aircraft, DCSJoystick stck, List<DCSHtmlFileRecord> htmlActionss)
    {
        foreach(DCSHtmlFileRecord row in htmlActionss)
        {
            DCSActionKey actionKey = new(row.Id);
            DCSAction action;
            if (data.Actions.ContainsKey(actionKey))
            {
                action = data.Actions[actionKey];
            }
            else
            {
                action = new DCSAction()
                {
                    Key = new(row.Id),
                    Name = row.Name,
                    IsAxis = row.Id.StartsWith("a")
                };
                data.Actions[actionKey] = action;
            }

            if (!action.Aircraft.ContainsKey(aircraft.Key))
            {
                action.Aircraft[aircraft.Key] = new DCSAircraftAction() { Key = aircraft.Key, Action = row.Name, Category = row.Category };
            }
            if (!action.Joysticks.ContainsKey(stck.Key))
            {
                action.Joysticks[stck.Key] = stck;
            }
            if (!data.Aircraft[aircraft.Key].Actions.ContainsKey(actionKey))
            {
                data.Aircraft[aircraft.Key].Actions.Add(actionKey, action);
            }
        }
    }

    private void BuildButtonActionsFromSavedGame(DCSData data, string savedGamesAircraftPath)
    {
        foreach (DCSAircraft aircraft in data.Aircraft.Values)
        {
            string aircraftJoystickFolderPath = savedGamesAircraftPath + "\\" + aircraft.Key.Name + "\\joystick";
            foreach (DCSJoystick stick in data.Joysticks.Values)
            {
                string aircraftStickPath = aircraftJoystickFolderPath + "\\" + stick.Joystick.DCSName + ".diff.lua";
                if (File.Exists(aircraftStickPath))
                {
                    ReadAircraftStickLuaFile(data, aircraft, stick, aircraftStickPath);
                }
            }
        }
    }

    private void ReadAircraftStickLuaFile(DCSData data, DCSAircraft aircraft, DCSJoystick stick, string aircraftStickPath)
    {
        Table table = Script.RunFile(aircraftStickPath).Table;

        for (int i = 0; i < table.Keys.Count(); i++)
        {
            string key = table.Keys.ElementAt(i).String;
            if (key == "axisDiffs")
            {
                ReadAxisDiffsLua(data, aircraft, stick, table.Values.ElementAt(i).Table);
            }
            else if (key == "keyDiffs")
            {
                ReadKeyDiffsLua(data, aircraft, stick, table.Values.ElementAt(i).Table);
            }
        }
    }

    private void ReadAxisDiffsLua(DCSData data, DCSAircraft aircraft, DCSJoystick stick, Table axisDiffsTable)
    {
        for (int i = 0; i < axisDiffsTable.Keys.Count(); i++)
        {
            DCSActionKey actionKey = new(axisDiffsTable.Keys.ElementAt(i).String);
            DCSAction action = data.Actions[actionKey];

            DCSAircraftJoystickAction actionData = CreateActionData(aircraft, stick, action);

            Table actionsTable = axisDiffsTable.Values.ElementAt(i).Table;

            for (int j = 0; j < actionsTable.Keys.Count(); j++)
            {
                string sectionName = actionsTable.Keys.ElementAt(j).String;
                if (sectionName == "added")
                {
                    ReadAddedAxisLua(actionData, actionsTable.Values.ElementAt(j).Table, actionData.ButtonChanges);
                }
                else if (sectionName == "changed")
                {
                    ReadChangedAxisLua(actionData, actionsTable.Values.ElementAt(j).Table, actionData.ButtonChanges);
                }
                else if (sectionName == "removed")
                {
                    ReadRemovedAxisLua(actionData, actionsTable.Values.ElementAt(j).Table, actionData.ButtonChanges);
                }
                else if (sectionName == "name")
                {
                    string _name = actionsTable.Values.ElementAt(j).String;
                }
            }
        }
    }

    private void ReadAddedAxisLua(DCSAircraftJoystickAction actionData, Table addedTable, DCSButtonChanges changes)
    {
        for (int i = 0; i < addedTable.Keys.Count(); i++)
        {
            Table table = addedTable.Values.ElementAt(i).Table;

            DCSButton axisButton = new();
            changes.Added.Add(axisButton);

            for (int j = 0; j < table.Keys.Count(); j++)
            {
                string key = table.Keys.ElementAt(j).String;
                if (key == "filter")
                {
                    ReadAxisFilterLua(axisButton, table.Values.ElementAt(j).Table);
                }
                else if (key == "key")
                {
                    axisButton.Name = table.Values.ElementAt(j).String;
                }
            }

            actionData.Buttons[axisButton.Name] = axisButton;
        }
    }

    private void ReadChangedAxisLua(DCSAircraftJoystickAction actionData, Table changedTable, DCSButtonChanges changes)
    {
        for (int i = 0; i < changedTable.Keys.Count(); i++)
        {
            Table table = changedTable.Values.ElementAt(i).Table;

            DCSButton axisButton = new();
            changes.Changed.Add(axisButton);

            for (int j = 0; j < table.Keys.Count(); j++)
            {
                string key = table.Keys.ElementAt(j).String;
                if (key == "filter")
                {
                    ReadAxisFilterLua(axisButton, table.Values.ElementAt(j).Table);
                }
                else if (key == "key")
                {
                    axisButton.Name = table.Values.ElementAt(j).String;
                }
            }

            actionData.Buttons[axisButton.Name] = axisButton;
        }
    }

    private void ReadAxisFilterLua(DCSButton axisButton, Table filterTable)
    {
        axisButton.AxisFilter = new();

        for (int j = 0; j < filterTable.Keys.Count(); j++)
        {
            string sectionName = filterTable.Keys.ElementAt(j).String;

            if (sectionName == "curvature")
            {
                Table curvatureTable = filterTable.Values.ElementAt(j).Table;
                for (int k = 0; k < curvatureTable.Keys.Count(); k++)
                {
                    axisButton.AxisFilter.Curvature.Add(curvatureTable.Values.ElementAt(k).Number);
                }
            }
            else if (sectionName == "deadzone")
            {
                axisButton.AxisFilter.Deadzone = filterTable.Values.ElementAt(j).Number;
            }
            else if (sectionName == "hardwareDetent")
            {
                axisButton.AxisFilter.HardwareDetent = filterTable.Values.ElementAt(j).Boolean;
            }
            else if (sectionName == "hardwareDetentAB")
            {
                axisButton.AxisFilter.HardwareDetentAB = filterTable.Values.ElementAt(j).Number;
            }
            else if (sectionName == "hardwareDetentMax")
            {
                axisButton.AxisFilter.HardwareDetentMax = filterTable.Values.ElementAt(j).Number;
            }
            else if (sectionName == "invert")
            {
                axisButton.AxisFilter.Invert = filterTable.Values.ElementAt(j).Boolean;
            }
            else if (sectionName == "saturationX")
            {
                axisButton.AxisFilter.SaturationX = filterTable.Values.ElementAt(j).Number;
            }
            else if (sectionName == "saturationY")
            {
                axisButton.AxisFilter.SaturationY = filterTable.Values.ElementAt(j).Number;
            }
            else if (sectionName == "slider")
            {
                axisButton.AxisFilter.Slider = filterTable.Values.ElementAt(j).Boolean;
            }
        }
    }

    private void ReadRemovedAxisLua(DCSAircraftJoystickAction actionData, Table removedTable, DCSButtonChanges changes)
    {
        for (int i = 0; i < removedTable.Values.Count(); i++)
        {
            Table table = removedTable.Values.ElementAt(i).Table;
            DCSButton removedButton = new() { Name = table.Values.ElementAt(i).String };
            changes.Removed.Add(removedButton);

            if (actionData.Buttons.ContainsKey(removedButton.Name))
            {
                actionData.Buttons.Remove(removedButton.Name);
            }
        }
    }

    private void ReadKeyDiffsLua(DCSData data, DCSAircraft aircraft, DCSJoystick stick, Table keyDiffsTable)
    {
        for (int i = 0; i < keyDiffsTable.Keys.Count(); i++)
        {
            DCSActionKey actionKey = new(keyDiffsTable.Keys.ElementAt(i).String);

            if (!data.Actions.ContainsKey(actionKey))
            {
                RinceLogger.Log.Warn("Error in LUA file: Aircraft-" + aircraft.Key.Name + " action-" + actionKey);
                continue;
            }

            DCSAction action = data.Actions[actionKey];

            DCSAircraftJoystickAction actionData = CreateActionData(aircraft, stick, action);

            Table actionsTable = keyDiffsTable.Values.ElementAt(i).Table;

            for (int j = 0; j < actionsTable.Keys.Count(); j++)
            {
                string sectionName = actionsTable.Keys.ElementAt(j).String;
                if (sectionName == "added")
                {
                    ReadAddedKeyLua(actionData, actionsTable.Values.ElementAt(j).Table, actionData.ButtonChanges);
                }
                else if (sectionName == "removed")
                {
                    ReadRemovedKeyLua(actionData, actionsTable.Values.ElementAt(j).Table, actionData.ButtonChanges);
                }
                else if (sectionName == "name")
                {
                    string _name = actionsTable.Values.ElementAt(j).String;
                }
            }
        }
    }

    private void ReadAddedKeyLua(DCSAircraftJoystickAction actionData, Table addedTable, DCSButtonChanges changes)
    {
        for (int i = 0; i < addedTable.Values.Count(); i++)
        {
            Table table = addedTable.Values.ElementAt(i).Table;
            DCSButton newButton = new();

            for (int j = 0; j < table.Keys.Count(); j++)
            {
                string sectionName = table.Keys.ElementAt(j).String;
                if (sectionName == "key")
                {
                    newButton.Name = table.Values.ElementAt(j).String;
                }
                else if (sectionName == "reformers")
                {
                    ReadKeyModifersLua(newButton, table.Values.ElementAt(j).Table);
                }
            }
            changes.Added.Add(newButton);

            actionData.Buttons[newButton.Name] = newButton;
        }
    }

    private void ReadKeyModifersLua(DCSButton button, Table modifiersTable)
    {
        for (int i = 0; i < modifiersTable.Values.Count(); i++)
        {
            button.Modifiers.Add(modifiersTable.Values.ElementAt(i).String);
        }
    }

    private void ReadRemovedKeyLua(DCSAircraftJoystickAction actionData, Table removedTable, DCSButtonChanges changes)
    {
        for (int i = 0; i < removedTable.Values.Count(); i++)
        {
            Table table = removedTable.Values.ElementAt(i).Table;
            for (int j = 0; j < table.Values.Count(); j++)
            {
                DCSButton removedButton = new() { Name = table.Values.ElementAt(j).String };
                changes.Removed.Add(removedButton);

                if (actionData.Buttons.ContainsKey(removedButton.Name))
                {
                    actionData.Buttons.Remove(removedButton.Name);
                }
            }
        }
    }

    private DCSAircraftJoystickAction CreateActionData(DCSAircraft aircraft, DCSJoystick stick, DCSAction action)
    {
        DCSAircraftJoystickAction actionData;
        DCSAircraftJoystickKey actionDataKey = new(aircraft.Key.Name, stick.Key.Id);

        if (action.AircraftJoysticks.ContainsKey(actionDataKey))
        {
            actionData = action.AircraftJoysticks[actionDataKey];
        }
        else
        {
            actionData = new DCSAircraftJoystickAction
            {
                AircraftKey = aircraft.Key,
                JoystickKey = stick.Key
            };
            action.AircraftJoysticks[actionDataKey] = actionData;
        }

        return actionData;
    }

    private class GameUpdateButton
    {
        public string AircraftName;
        public AttachedJoystick Joystick;
        public bool IsAxis;
        public string ActionId;
        public string Action;
        public DCSButton AddButton;
        public DCSButton RemoveButton;
    }

    private class GameUpdateButtonComparer : IEqualityComparer<GameUpdateButton>
    {
        public bool Equals(GameUpdateButton x, GameUpdateButton y)
        {
            if (x == null || y == null) return false;

            if (ReferenceEquals(x, y)) return true;

            string xButtonName = x.AddButton == null ? x.RemoveButton.Name : x.AddButton.Name;
            string yButtonName = y.AddButton == null ? y.RemoveButton.Name : y.AddButton.Name;

            return x.AircraftName == y.AircraftName &&
                   x.Joystick == y.Joystick &&
                   x.ActionId == y.ActionId &&
                   xButtonName == yButtonName;
        }

        public int GetHashCode([DisallowNull] GameUpdateButton obj)
        {
            if (obj == null)
                return 0;

            return (obj.AircraftName + obj.Joystick.Name + obj.ActionId + (obj.AddButton == null ? obj.RemoveButton.Name : obj.AddButton.Name)).GetHashCode();
        }
    }


    private class DCSLuaFileBuilder
    {
        private readonly string luaFileName;
        private StringBuilder sb = new();

        internal DCSLuaFileBuilder(string fileName)
        {
            luaFileName = fileName;
        }

        internal void AppendHeader()
        {
            sb.AppendLine("local diff = {");
        }

        internal void AppendFooter()
        {
            sb.AppendLine("}");
            sb.AppendLine("return diff");
        }

        internal void AppendAxisHeader()
        {
            sb.AppendLine("\t[\"axisDiffs\"] = {");
        }

        internal void AppendAxisFooter()
        {
            sb.AppendLine("\t},");
        }

        internal void AppendKeyHeader()
        {
            sb.AppendLine("\t[\"keyDiffs\"] = {");
        }

        internal void AppendKeyFooter()
        {
            sb.AppendLine("\t},");
        }

        internal void AppendActionHeader(GameUpdateButton button)
        {
            sb.AppendLine("\t\t[\"" + button.ActionId + "\"] = {");
        }

        internal void AppendActionName(GameUpdateButton button)
        {
            sb.AppendLine("\t\t\t[\"name\"] = \"" + button.Action + "\",");
        }

        internal void AppendActionFooter()
        {
            sb.AppendLine("\t\t},");
        }

        internal void AppendAddHeader()
        {
            sb.AppendLine("\t\t\t[\"added\"] = {");
        }

        internal void AppendAddFooter()
        {
            sb.AppendLine("\t\t\t},");
        }

        internal void AppendRemoveHeader()
        {
            sb.AppendLine("\t\t\t[\"removed\"] = {");
        }

        internal void AppendRemoveFooter()
        {
            sb.AppendLine("\t\t\t},");
        }

        internal void AppendButton(GameUpdateButton button, int buttonIndex)
        {
            string buttonName = button.AddButton != null ? button.AddButton.Name : button.RemoveButton.Name;
            AxisFilter filter = button.AddButton != null ? button.AddButton.AxisFilter : button.RemoveButton.AxisFilter;
            
            sb.AppendLine("\t\t\t\t[" + (buttonIndex + 1).ToString() + "] = {");
            if (filter != null)
            {
                sb.AppendLine("\t\t\t\t\t[\"filter\"] = {");
                sb.AppendLine("\t\t\t\t\t\t[\"curvature\"] = {");
                for (int curveIndex = 0; curveIndex < filter.Curvature.Count(); curveIndex++)
                {
                    sb.AppendLine("\t\t\t\t\t\t\t[" + (curveIndex + 1).ToString() + "] = " + filter.Curvature[curveIndex].ToString() + ",");
                }
                sb.AppendLine("\t\t\t\t\t\t},");
                sb.AppendLine("\t\t\t\t\t\t[\"deadzone\"] = " + filter.Deadzone.ToString() + ",");
                sb.AppendLine("\t\t\t\t\t\t[\"hardwareDetent\"] = " + filter.HardwareDetent.ToString() + ",");
                sb.AppendLine("\t\t\t\t\t\t[\"hardwareDetentAB\"] = " + filter.HardwareDetentAB.ToString() + ",");
                sb.AppendLine("\t\t\t\t\t\t[\"hardwareDetentMax\"] = " + filter.HardwareDetentMax.ToString() + ",");
                sb.AppendLine("\t\t\t\t\t\t[\"invert\"] = " + filter.Invert.ToString().ToLower() + ",");
                sb.AppendLine("\t\t\t\t\t\t[\"saturationX\"] = " + filter.SaturationX.ToString() + ",");
                sb.AppendLine("\t\t\t\t\t\t[\"saturationY\"] = " + filter.SaturationY.ToString() + ",");
                sb.AppendLine("\t\t\t\t\t\t[\"slider\"] = " + filter.Slider.ToString().ToLower() + ",");
                sb.AppendLine("\t\t\t\t\t},");
            }
            sb.AppendLine("\t\t\t\t\t[\"key\"] = \"" + buttonName + "\",");
            if(button.AddButton != null && button.AddButton.IsModifier)
            {
                sb.AppendLine("\t\t\t\t\t[\"reformers\"] = {");
                for (int index = 0; index < button.AddButton.Modifiers.Count; index++)
                {
                    sb.AppendLine("\t\t\t\t\t\t[" + (index + 1) + "] = \"" + button.AddButton.Modifiers[index] + "\",");
                }
                sb.AppendLine("\t\t\t\t\t},");
            }

            sb.AppendLine("\t\t\t\t},");
        }

        internal void WriteFile()
        {
            File.WriteAllText(luaFileName, sb.ToString());
        }
    }
}
