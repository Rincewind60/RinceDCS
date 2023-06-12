using CommunityToolkit.Mvvm.DependencyInjection;
using HtmlAgilityPack;
using MoonSharp.Interpreter;
using RinceDCS.Models;
using RinceDCS.ServiceModels;
using SharpDX.DirectInput;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Devices.Geolocation;

namespace RinceDCS.Services;

public class DCSService : IDCSService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="gameName"></param>
    /// <param name="gamePath"></param>
    /// <param name="sticks"></param>
    /// <returns></returns>
    public DCSData GetBindingData(string gameName, string gameExePath, string savedGameFolderPath, List<AttachedJoystick> sticks)
    {
        DCSData data = new();

        BuildListOfJoysticks(data, sticks);

        string htmlFilesFolder = savedGameFolderPath + "\\InputLayoutsTxt";
        if (Directory.Exists(htmlFilesFolder))
        {
            BuildListOfAircraftFromHTMLFiles(data, htmlFilesFolder);
            BuildBindingsFromHTMLFiles(data, htmlFilesFolder);
        }

        string coreGameAircraftPath = Path.GetDirectoryName(Path.GetDirectoryName(gameExePath)) + "\\Mods\\Aircraft";
        if (Directory.Exists(coreGameAircraftPath))
        {
            BuildButtonBindingsFromCoreGame(data, coreGameAircraftPath);
        }

        string savedGamesAircraftPath = savedGameFolderPath + "\\Config\\Input";
        if (Directory.Exists(savedGamesAircraftPath))
        {
            BuildButtonBindingsFromSavedGame(data, savedGamesAircraftPath);
        }

        return data;
    }

    public string GetSavedGamesPath(string gameFolderPath, string currentSavedGamesFolder)
    {
        string savedGamesFolder = Ioc.Default.GetRequiredService<ISettingsService>().GetSetting(RinceDCSSettings.SavedGamesPath);
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

        if(Directory.Exists(newSavedGamesFolder))
        {
            return newSavedGamesFolder;
        }

        return currentSavedGamesFolder;
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
                    ReadAircraftStickHtmlFile(data, aircraft, stick, aircraftStickHtmlPath);
                }
            }
        }
    }

    private void ReadAircraftStickHtmlFile(DCSData data, DCSAircraft aircraft, DCSJoystick stck, string aircraftStickHtmlPath)
    {
        HtmlDocument document = new();
        document.Load(aircraftStickHtmlPath);
        var rows = document.DocumentNode.SelectNodes("//tr");
        for (int i = 1; i < rows.Count; i++)
        {
            HtmlNode row = rows[i];
            string name = row.ChildNodes[3].GetDirectInnerText().Trim();
            string category = row.ChildNodes[5].GetDirectInnerText().Trim().Split(";").First();
            string id = row.ChildNodes[7].GetDirectInnerText().Trim();

            DCSBindingKey bindKey = new(id);
            DCSBinding binding;
            if (data.Bindings.ContainsKey(bindKey))
            {
                binding = data.Bindings[bindKey];
            }
            else
            {
                binding = new DCSBinding()
                {
                    Key = new(id),
                    CommandName = name,
                    IsAxisBinding = id.StartsWith("a")
                };
                data.Bindings[bindKey] = binding;
            }

            if (!binding.AircraftWithBinding.ContainsKey(aircraft.Key))
            {
                binding.AircraftWithBinding[aircraft.Key] = new DCSAircraftBinding() { Key = aircraft.Key, CommandName = name, CategoryName = category };
            }
            if (!binding.JoysticksWithBinding.ContainsKey(stck.Key))
            {
                binding.JoysticksWithBinding[stck.Key] = stck;
            }
            if (!data.Aircraft[aircraft.Key].Bindings.ContainsKey(bindKey))
            {
                data.Aircraft[aircraft.Key].Bindings.Add(bindKey, binding);
            }
        }
    }

    private void BuildButtonBindingsFromCoreGame(DCSData data, string coreGameAircraftPath)
    {
        foreach(string aircraftDir in Directory.GetDirectories(coreGameAircraftPath))
        {
            string inputPath = aircraftDir + "\\input";
            foreach (string aircraftInputDir in Directory.GetDirectories(inputPath))
            {
                string aircraftName = aircraftInputDir.Split("\\").Last();
                if(data.Aircraft.ContainsKey(new(aircraftName)))
                {
                    string aircraftJoystickFolderPath = aircraftInputDir + "\\joystick";
                    foreach (DCSJoystick stick in data.Joysticks.Values)
                    {
                        string aircraftStickPath = aircraftJoystickFolderPath + "\\" + stick.Joystick.Name + ".diff.lua";
                        if (File.Exists(aircraftStickPath))
                        {
                            ReadAircraftStickLuaFile(data, data.Aircraft[new(aircraftName)], stick, aircraftStickPath, true);
                        }
                    }
                }
            }
        }
    }

    private void BuildButtonBindingsFromSavedGame(DCSData data, string savedGamesAircraftPath)
    {
        foreach(DCSAircraft aircraft in data.Aircraft.Values)
        {
            string aircraftJoystickFolderPath = savedGamesAircraftPath + "\\" + aircraft.Key.Name + "\\joystick";
            foreach (DCSJoystick stick in data.Joysticks.Values)
            {
                string aircraftStickPath = aircraftJoystickFolderPath + "\\" + stick.Joystick.DCSName + ".diff.lua";
                if (File.Exists(aircraftStickPath))
                {
                    ReadAircraftStickLuaFile(data, aircraft, stick, aircraftStickPath, false);
                }
            }
        }
    }

    private void ReadAircraftStickLuaFile(DCSData data, DCSAircraft aircraft, DCSJoystick stick, string aircraftStickPath, bool isCore)
    {
        Table table = Script.RunFile(aircraftStickPath).Table;

        for(int i = 0; i < table.Keys.Count(); i++)
        {
            string key = table.Keys.ElementAt(i).String;
            if (key == "axisDiffs")
            {
                ReadAxisDiffsLua(data, aircraft, stick, table.Values.ElementAt(i).Table, isCore);
            }
            else if(key == "keyDiffs")
            {
                ReadKeyDiffsLua(data, aircraft, stick, table.Values.ElementAt(i).Table, isCore);
            }
        }
    }

    private void ReadAxisDiffsLua(DCSData data, DCSAircraft aircraft, DCSJoystick stick, Table axisDiffsTable, bool isCore)
    {
        for(int i = 0; i < axisDiffsTable.Keys.Count(); i++)
        {
            DCSBindingKey bindingKey = new(axisDiffsTable.Keys.ElementAt(i).String);
            DCSBinding binding = data.Bindings[bindingKey];

            DCSAircraftJoystickBinding bindingData = CreateBindingData(aircraft, stick, binding);
            DCSButtonChanges changes = isCore ? bindingData.CoreButtonChanges : bindingData.SavedGamesButtonChanges;

            Table bindingsTable = axisDiffsTable.Values.ElementAt(i).Table;

            for (int j = 0; j < bindingsTable.Keys.Count(); j++)
            {
                string sectionName = bindingsTable.Keys.ElementAt(j).String;
                if (sectionName == "changed")
                {
                    ReadChangedAxisLua(bindingData, bindingsTable.Values.ElementAt(j).Table, changes);
                }
                else if (sectionName == "removed")
                {
                    ReadRemovedAxisLua(bindingData, bindingsTable.Values.ElementAt(j).Table, changes);
                }
                else if (sectionName == "name")
                {
                    string name = bindingsTable.Values.ElementAt(j).String;
                }
            }
        }
    }
 
    private void ReadChangedAxisLua(DCSAircraftJoystickBinding bindingData, Table changedTable, DCSButtonChanges changes)
    {
        for (int i = 0; i < changedTable.Keys.Count(); i++)
        {
            Table table = changedTable.Values.ElementAt(i).Table;

            DCSAxisButton axisButton = new();
            changes.ChangedAxisButtons.Add(axisButton);

            for (int j = 0; j < table.Keys.Count(); j++)
            {
                string key = table.Keys.ElementAt(j).String;
                if (key == "filter")
                {
                    ReadChangedAxisFilterLua(axisButton, table.Values.ElementAt(j).Table);
                }
                else if (key == "key")
                {
                    axisButton.Key = new(table.Values.ElementAt(j).String);
                }
            }

            bindingData.AssignedButtons[axisButton.Key] = axisButton;
        }
    }

    private void ReadChangedAxisFilterLua(DCSAxisButton axisButton, Table filterTable)
    {
        for (int j = 0; j < filterTable.Keys.Count(); j++)
        {
            string sectionName = filterTable.Keys.ElementAt(j).String;

            if (sectionName == "curvature")
            {
                Table curvatureTable = filterTable.Values.ElementAt(j).Table;
                for (int k = 0; k < curvatureTable.Keys.Count(); k++)
                {
                    axisButton.Filter.Curvature.Add(curvatureTable.Values.ElementAt(k).Number);
                }
            }
            else if (sectionName == "deadzone")
            {
                axisButton.Filter.Deadzone = filterTable.Values.ElementAt(j).Number;
            }
            else if (sectionName == "hardwareDetent")
            {
                axisButton.Filter.HardwareDetent = filterTable.Values.ElementAt(j).Boolean;
            }
            else if (sectionName == "hardwareDetentAB")
            {
                axisButton.Filter.HardwareDetentAB = filterTable.Values.ElementAt(j).Number;
            }
            else if (sectionName == "hardwareDetentMax")
            {
                axisButton.Filter.HardwareDetentMax = filterTable.Values.ElementAt(j).Number;
            }
            else if (sectionName == "invert")
            {
                axisButton.Filter.Invert = filterTable.Values.ElementAt(j).Boolean;
            }
            else if (sectionName == "saturationX")
            {
                axisButton.Filter.SaturationX = filterTable.Values.ElementAt(j).Number;
            }
            else if (sectionName == "saturationY")
            {
                axisButton.Filter.SaturationY = filterTable.Values.ElementAt(j).Number;
            }
            else if (sectionName == "slider")
            {
                axisButton.Filter.Slider = filterTable.Values.ElementAt(j).Boolean;
            }
        }
    }

    private void ReadRemovedAxisLua(DCSAircraftJoystickBinding bindingData, Table removedTable, DCSButtonChanges changes)
    {
        for (int i = 0; i < removedTable.Values.Count(); i++)
        {
            Table table = removedTable.Values.ElementAt(i).Table;
            string button = table.Values.ElementAt(i).String;
            DCSButton removedButton = new() { Key = new(button) };
            changes.RemovedAxisButtons.Add(removedButton);

            if(bindingData.AssignedButtons.ContainsKey(removedButton.Key))
            {
                bindingData.AssignedButtons.Remove(removedButton.Key);
            }
        }
    }

    private void ReadKeyDiffsLua(DCSData data, DCSAircraft aircraft, DCSJoystick stick, Table keyDiffsTable, bool isCore)
    {
        for (int i = 0; i < keyDiffsTable.Keys.Count(); i++)
        {
            DCSBindingKey bindingKey = new(keyDiffsTable.Keys.ElementAt(i).String);
            DCSBinding binding = data.Bindings[bindingKey];

            DCSAircraftJoystickBinding bindingData = CreateBindingData(aircraft, stick, binding);
            DCSButtonChanges changes = isCore ? bindingData.CoreButtonChanges : bindingData.SavedGamesButtonChanges;

            Table bindingsTable = keyDiffsTable.Values.ElementAt(i).Table;

            for (int j = 0; j < bindingsTable.Keys.Count(); j++)
            {
                string sectionName = bindingsTable.Keys.ElementAt(j).String;
                if (sectionName == "added")
                {
                    ReadAddedKeyLua(bindingData, bindingsTable.Values.ElementAt(j).Table, changes);
                }
                else if (sectionName == "removed")
                {
                    ReadRemovedKeyLua(bindingData, bindingsTable.Values.ElementAt(j).Table, changes);
                }
                else if (sectionName == "name")
                {
                    string name = bindingsTable.Values.ElementAt(j).String;
                }
            }
        }
    }

    private void ReadAddedKeyLua(DCSAircraftJoystickBinding bindingData, Table addedTable, DCSButtonChanges changes)
    {
        for (int i = 0; i < addedTable.Values.Count(); i++)
        {
            Table table = addedTable.Values.ElementAt(i).Table;
            DCSKeyButton newButton = new();

            for (int j = 0; j < table.Keys.Count(); j++)
            {
                string sectionName = table.Keys.ElementAt(j).String;
                if(sectionName == "key")
                {
                    string button = table.Values.ElementAt(j).String;
                    newButton.Key = new(button);
                }
                else if(sectionName == "reformers")
                {
                    ReadKeyModifersLua(newButton, table.Values.ElementAt(j).Table);
                }
            }
            changes.AddedKeyButtons.Add(newButton);

            bindingData.AssignedButtons[newButton.Key] = newButton;
        }
    }

    private void ReadKeyModifersLua(DCSKeyButton button, Table modifiersTable)
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
            for(int j = 0; j < table.Values.Count(); j++)
            {
                string button = table.Values.ElementAt(j).String;
                DCSButton removedButton = new() { Key = new(button) };
                changes.RemovedKeyButtons.Add(removedButton);

                if (bindingData.AssignedButtons.ContainsKey(removedButton.Key))
                {
                    bindingData.AssignedButtons.Remove(removedButton.Key);
                }
            }
        }
    }

    private DCSAircraftJoystickBinding CreateBindingData(DCSAircraft aircraft, DCSJoystick stick, DCSBinding binding)
    {
        DCSAircraftJoystickBinding bindingData;
        DCSAircraftJoystickKey bindingDataKey = new DCSAircraftJoystickKey(aircraft.Key.Name, stick.Key.Id);

        if (binding.AircraftJoystickBindings.ContainsKey(bindingDataKey))
        {
            bindingData = binding.AircraftJoystickBindings[bindingDataKey];
        }
        else
        {
            bindingData = new DCSAircraftJoystickBinding();
            bindingData.AircraftKey = aircraft.Key;
            bindingData.JoystickKey = stick.Key;
            binding.AircraftJoystickBindings[bindingDataKey] = bindingData;
        }

        return bindingData;
    }
}
