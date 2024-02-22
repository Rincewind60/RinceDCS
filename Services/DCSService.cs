using CommunityToolkit.Mvvm.DependencyInjection;
using HtmlAgilityPack;
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
    public DCSData GetBindingData(string gameName, string gameExePath, string savedGamesPath, List<AttachedJoystick> sticks)
    {
        ///TODO: If no diff.lua file exists for an aircraft/joystick combination then assume there should be (the default)
        DCSData data = new();

        BuildListOfJoysticks(data, sticks);

        ReadDefaultModifiersLua(data, gameExePath);

        string htmlFilesFolder = savedGamesPath + "\\InputLayoutsTxt";
        if (Directory.Exists(htmlFilesFolder))
        {
            BuildListOfAircraftFromHTMLFiles(data, htmlFilesFolder);
            BuildBindingsFromHTMLFiles(data, htmlFilesFolder);
        }

        string savedGamesAircraftPath = savedGamesPath + "\\Config\\Input";
        if (Directory.Exists(savedGamesAircraftPath))
        {
            ReadAircraftModifiersLua(data, savedGamesAircraftPath);
            BuildButtonBindingsFromSavedGame(data, savedGamesAircraftPath);
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
                    newModifier.Device = modiferTable.Values.ElementAt(j).ToString();
                }
                else if(modifierPropertyName == "key")
                {
                    newModifier.Key = modiferTable.Values.ElementAt(j).ToString();
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
                                newModifier.Device = modiferTable.Values.ElementAt(j).ToString();
                            }
                            else if(modifierPropertyName == "key")
                            {
                                newModifier.Key = modiferTable.Values.ElementAt(j).ToString();
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
        List<DCSHtmlFileRecord> bindings = new();
        HtmlDocument document = new();

        document.Load(aircraftStickHtmlPath);
        var rows = document.DocumentNode.SelectNodes("//tr");
        for (int i = 1; i < rows.Count; i++)
        {
            HtmlNode row = rows[i];
            bindings.Add(new DCSHtmlFileRecord(row.ChildNodes[3].GetDirectInnerText().Trim(),
                                               row.ChildNodes[5].GetDirectInnerText().Trim().Split(";").First(),
                                               row.ChildNodes[7].GetDirectInnerText().Trim()));
        }
        return bindings;
    }

    public void UpdateDCSConfigFiles(string SavedGamesPath, RinceDCSGroups Groups, DCSData data, List<string> aircraftNames)
    {
        //  Find all RinceDCS buttons to be added
        var rinceButtons = from grp in Groups.Groups
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
                               BindingId = aircraft.BindingId,
                               Command = aircraft.Command,
                               AddButton = new DCSButton() { Name = button.Name, AxisFilter = button.AxisFilter, Modifiers = button.Modifiers }
                           };

        //  Find all DCS removed buttons that are not part of the RinceDCS buttons to Add, these still need to be removed
        var dcsRemoveButtons = (from dcsBinding in data.Bindings.Values
                                from ajb in dcsBinding.AircraftJoysticks.Values
                                from button in ajb.ButtonChanges.Removed
                                select new GameUpdateButton()
                                {
                                    AircraftName = ajb.AircraftKey.Name,
                                    Joystick = data.Joysticks[ajb.JoystickKey].Joystick,
                                    IsAxis = dcsBinding.IsAxis,
                                    BindingId = dcsBinding.Key.Id,
                                    Command = dcsBinding.Command,
                                    RemoveButton = button
                                }).Except(rinceButtons, new GameUpdateButtonComparer());

        //  Find all DCS add buttons that are not part of the RinceDCS buttons to Add, these now need to be removed
        var dcsAddedButtons = (from dcsBinding in data.Bindings.Values
                               from ajb in dcsBinding.AircraftJoysticks.Values
                               from button in ajb.ButtonChanges.Added
                               select new GameUpdateButton()
                               {
                                   AircraftName = ajb.AircraftKey.Name,
                                   Joystick = data.Joysticks[ajb.JoystickKey].Joystick,
                                   IsAxis = dcsBinding.IsAxis,
                                   BindingId = dcsBinding.Key.Id,
                                   Command = dcsBinding.Command,
                                   RemoveButton = button
                               }).Except(rinceButtons, new GameUpdateButtonComparer());

        var updates = from update in rinceButtons.Concat(dcsRemoveButtons).Concat(dcsAddedButtons) select update;

        BuildLuaFile(SavedGamesPath, updates, aircraftNames);
    }

    private void BuildLuaFile(string SavedGamesPath, IEnumerable<GameUpdateButton> updates, List<string> aircraftNames)
    {
        string configFolder = SavedGamesPath + "\\Config\\Input";

        BackupAircraftConfigFiles(SavedGamesPath, aircraftNames, configFolder);

        var ordedUpdates = updates.OrderBy(row => row.AircraftName)
                                  .ThenBy(row => row.Joystick.Name)
                                  .ThenByDescending(row => row.IsAxis)
                                  .ThenBy(row => row.BindingId)
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

                string luaBackupFolder = "S:/RinceConfigBackup/Input/" + update.AircraftName + "/joystick/";
                string luaFileName = luaBackupFolder + update.Joystick.DCSName + ".diff.lua";
                if (!Directory.Exists(luaBackupFolder))
                {
                    Directory.CreateDirectory(luaBackupFolder);
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
                luaBuilder.AppendBindingHeader(update);
                if (update.AddButton != null)
                {
                    luaBuilder.AppendAddHeader();
                }
                else
                {
                    luaBuilder.AppendBindingName(update);
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
                        luaBuilder.AppendBindingName(prevUpdate);
                    }
                    if (prevUpdate.RemoveButton != null)
                    {
                        luaBuilder.AppendRemoveFooter();
                    }
                    luaBuilder.AppendBindingFooter();
                    luaBuilder.AppendAxisFooter();

                    if (update.IsAxis)
                    {
                        luaBuilder.AppendAxisHeader();
                    }
                    else
                    {
                        luaBuilder.AppendKeyHeader();
                    }
                    luaBuilder.AppendBindingHeader(update);
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
                else if (prevUpdate.BindingId != update.BindingId)
                {
                    if (prevUpdate.AddButton != null)
                    {
                        luaBuilder.AppendAddFooter();
                        luaBuilder.AppendBindingName(prevUpdate);
                    }
                    if (prevUpdate.RemoveButton != null)
                    {
                        luaBuilder.AppendRemoveFooter();
                    }
                    luaBuilder.AppendBindingFooter();

                    luaBuilder.AppendBindingHeader(update);
                    if (update.AddButton != null)
                    {
                        luaBuilder.AppendAddHeader();
                    }
                    else
                    {
                        luaBuilder.AppendBindingName(update);
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
    }

    private static void FinalizePreviousFile(DCSLuaFileBuilder luaBuilder, GameUpdateButton prevUpdate)
    {
        if (prevUpdate.AddButton != null)
        {
            luaBuilder.AppendAddFooter();
            luaBuilder.AppendBindingName(prevUpdate);
        }
        else
        {
            luaBuilder.AppendRemoveFooter();
        }
        luaBuilder.AppendBindingFooter();
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

    private static void BackupAircraftConfigFiles(string SavedGamesPath, List<string> aircraftNames, string configFolder)
    {
        string backupFolder = SavedGamesPath + "\\RinceDCS\\Backups\\Config_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + "\\Input";

        Directory.CreateDirectory(backupFolder);

        //  Copy existing Aircraft config files to backup, only for Aircraft being updated
        foreach (string aircraft in aircraftNames)
        {
            string fromFolder = configFolder + "\\" + aircraft + "\\joystick";
            string toFolder = backupFolder + "\\" + aircraft + "\\joystick";

            Directory.CreateDirectory(toFolder);

            foreach (string filePath in Directory.GetFiles(fromFolder))
            {
                string fileName = Path.GetFileName(filePath);
                File.Copy(fromFolder + "\\" + fileName, toFolder + "\\" + fileName, true);
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

    private void BuildBindingsFromHTMLFiles(DCSData data, string htmlFolderPath)
    {
        foreach (DCSAircraft aircraft in data.Aircraft.Values)
        {
            foreach (DCSJoystick stick in data.Joysticks.Values)
            {
                string aircraftStickHtmlPath = htmlFolderPath + "\\" + aircraft.Key.Name + "\\" + stick.Joystick.DCSName + ".html";
                if (File.Exists(aircraftStickHtmlPath))
                {
                    List<DCSHtmlFileRecord>  htmlBindings = ReadAircraftStickHtmlFile(aircraftStickHtmlPath);
                    BuildHTMLBindings(data, aircraft, stick, htmlBindings);
                }
            }
        }
    }

    private void BuildHTMLBindings(DCSData data, DCSAircraft aircraft, DCSJoystick stck, List<DCSHtmlFileRecord> htmlBindings)
    {
        foreach(DCSHtmlFileRecord row in htmlBindings)
        {
            DCSBindingKey bindKey = new(row.Id);
            DCSBinding binding;
            if (data.Bindings.ContainsKey(bindKey))
            {
                binding = data.Bindings[bindKey];
            }
            else
            {
                binding = new DCSBinding()
                {
                    Key = new(row.Id),
                    Command = row.Name,
                    IsAxis = row.Id.StartsWith("a")
                };
                data.Bindings[bindKey] = binding;
            }

            if (!binding.Aircraft.ContainsKey(aircraft.Key))
            {
                binding.Aircraft[aircraft.Key] = new DCSAircraftBinding() { Key = aircraft.Key, Command = row.Name, Category = row.Category };
            }
            if (!binding.Joysticks.ContainsKey(stck.Key))
            {
                binding.Joysticks[stck.Key] = stck;
            }
            if (!data.Aircraft[aircraft.Key].Bindings.ContainsKey(bindKey))
            {
                data.Aircraft[aircraft.Key].Bindings.Add(bindKey, binding);
            }
        }
    }

    private void BuildButtonBindingsFromSavedGame(DCSData data, string savedGamesAircraftPath)
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
            DCSBindingKey bindingKey = new(axisDiffsTable.Keys.ElementAt(i).String);
            DCSBinding binding = data.Bindings[bindingKey];

            DCSAircraftJoystickBinding bindingData = CreateBindingData(aircraft, stick, binding);

            Table bindingsTable = axisDiffsTable.Values.ElementAt(i).Table;

            for (int j = 0; j < bindingsTable.Keys.Count(); j++)
            {
                string sectionName = bindingsTable.Keys.ElementAt(j).String;
                if (sectionName == "added")
                {
                    ReadAddedAxisLua(bindingData, bindingsTable.Values.ElementAt(j).Table, bindingData.ButtonChanges);
                }
                else if (sectionName == "changed")
                {
                    ReadChangedAxisLua(bindingData, bindingsTable.Values.ElementAt(j).Table, bindingData.ButtonChanges);
                }
                else if (sectionName == "removed")
                {
                    ReadRemovedAxisLua(bindingData, bindingsTable.Values.ElementAt(j).Table, bindingData.ButtonChanges);
                }
                else if (sectionName == "name")
                {
                    string _name = bindingsTable.Values.ElementAt(j).String;
                }
            }
        }
    }

    private void ReadAddedAxisLua(DCSAircraftJoystickBinding bindingData, Table addedTable, DCSButtonChanges changes)
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

            bindingData.Buttons[axisButton.Name] = axisButton;
        }
    }

    private void ReadChangedAxisLua(DCSAircraftJoystickBinding bindingData, Table changedTable, DCSButtonChanges changes)
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

            bindingData.Buttons[axisButton.Name] = axisButton;
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

    private void ReadRemovedAxisLua(DCSAircraftJoystickBinding bindingData, Table removedTable, DCSButtonChanges changes)
    {
        for (int i = 0; i < removedTable.Values.Count(); i++)
        {
            Table table = removedTable.Values.ElementAt(i).Table;
            DCSButton removedButton = new() { Name = table.Values.ElementAt(i).String };
            changes.Removed.Add(removedButton);

            if (bindingData.Buttons.ContainsKey(removedButton.Name))
            {
                bindingData.Buttons.Remove(removedButton.Name);
            }
        }
    }

    private void ReadKeyDiffsLua(DCSData data, DCSAircraft aircraft, DCSJoystick stick, Table keyDiffsTable)
    {
        for (int i = 0; i < keyDiffsTable.Keys.Count(); i++)
        {
            DCSBindingKey bindingKey = new(keyDiffsTable.Keys.ElementAt(i).String);

            if (!data.Bindings.ContainsKey(bindingKey))
            {
                RinceLogger.Log.Warn("Error in LUA file: Aircraft-" + aircraft.Key.Name + " binding-" + bindingKey);
                continue;
            }

            DCSBinding binding = data.Bindings[bindingKey];

            DCSAircraftJoystickBinding bindingData = CreateBindingData(aircraft, stick, binding);

            Table bindingsTable = keyDiffsTable.Values.ElementAt(i).Table;

            for (int j = 0; j < bindingsTable.Keys.Count(); j++)
            {
                string sectionName = bindingsTable.Keys.ElementAt(j).String;
                if (sectionName == "added")
                {
                    ReadAddedKeyLua(bindingData, bindingsTable.Values.ElementAt(j).Table, bindingData.ButtonChanges);
                }
                else if (sectionName == "removed")
                {
                    ReadRemovedKeyLua(bindingData, bindingsTable.Values.ElementAt(j).Table, bindingData.ButtonChanges);
                }
                else if (sectionName == "name")
                {
                    string _name = bindingsTable.Values.ElementAt(j).String;
                }
            }
        }
    }

    private void ReadAddedKeyLua(DCSAircraftJoystickBinding bindingData, Table addedTable, DCSButtonChanges changes)
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

            bindingData.Buttons[newButton.Name] = newButton;
        }
    }

    private void ReadKeyModifersLua(DCSButton button, Table modifiersTable)
    {
        for (int i = 0; i < modifiersTable.Values.Count(); i++)
        {
            button.Modifiers.Add(modifiersTable.Values.ElementAt(i).String);
        }
    }

    private void ReadRemovedKeyLua(DCSAircraftJoystickBinding bindingData, Table removedTable, DCSButtonChanges changes)
    {
        for (int i = 0; i < removedTable.Values.Count(); i++)
        {
            Table table = removedTable.Values.ElementAt(i).Table;
            for (int j = 0; j < table.Values.Count(); j++)
            {
                DCSButton removedButton = new() { Name = table.Values.ElementAt(j).String };
                changes.Removed.Add(removedButton);

                if (bindingData.Buttons.ContainsKey(removedButton.Name))
                {
                    bindingData.Buttons.Remove(removedButton.Name);
                }
            }
        }
    }

    private DCSAircraftJoystickBinding CreateBindingData(DCSAircraft aircraft, DCSJoystick stick, DCSBinding binding)
    {
        DCSAircraftJoystickBinding bindingData;
        DCSAircraftJoystickKey bindingDataKey = new(aircraft.Key.Name, stick.Key.Id);

        if (binding.AircraftJoysticks.ContainsKey(bindingDataKey))
        {
            bindingData = binding.AircraftJoysticks[bindingDataKey];
        }
        else
        {
            bindingData = new DCSAircraftJoystickBinding
            {
                AircraftKey = aircraft.Key,
                JoystickKey = stick.Key
            };
            binding.AircraftJoysticks[bindingDataKey] = bindingData;
        }

        return bindingData;
    }

    private class GameUpdateButton
    {
        public string AircraftName;
        public AttachedJoystick Joystick;
        public bool IsAxis;
        public string BindingId;
        public string Command;
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
                   x.BindingId == y.BindingId &&
                   xButtonName == yButtonName;
        }

        public int GetHashCode([DisallowNull] GameUpdateButton obj)
        {
            if (obj == null)
                return 0;

            return (obj.AircraftName + obj.Joystick.Name + obj.BindingId + (obj.AddButton == null ? obj.RemoveButton.Name : obj.AddButton.Name)).GetHashCode();
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

        internal void AppendBindingHeader(GameUpdateButton button)
        {
            sb.AppendLine("\t\t[\"" + button.BindingId + "\"] = {");
        }

        internal void AppendBindingName(GameUpdateButton button)
        {
            sb.AppendLine("\t\t\t[\"name\"] = \"" + button.Command + "\",");
        }

        internal void AppendBindingFooter()
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
