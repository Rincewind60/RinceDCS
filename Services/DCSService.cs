using CommunityToolkit.Mvvm.DependencyInjection;
using HtmlAgilityPack;
using MoonSharp.Interpreter;
using RinceDCS.Models;
using RinceDCS.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

namespace RinceDCS.Services;

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
    /// <param name="savedGameFolderPath"></param>
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

        string savedGamesAircraftPath = savedGameFolderPath + "\\Config\\Input";
        if (Directory.Exists(savedGamesAircraftPath))
        {
            BuildButtonBindingsFromSavedGame(data, savedGamesAircraftPath);
        }

        return data;
    }

    public string GetSavedGamesPath(string gameFolderPath, string currentSavedGamesFolder)
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

    public void UpdateGameBindingData(string savedGameFolderPath, RinceDCSGroups bindingGroups, DCSData data)
    {
        //  Find all RinceDCS buttons to be added
        var rinceButtons = from grp in bindingGroups.Groups
                           from aircraft in grp.Aircraft
                           from gj in grp.Joysticks
                           from button in gj.Buttons
                           where aircraft.IsActive == true
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

        BuildLuaFile(savedGameFolderPath, updates);


        //  For each Aircraft
        //foreach (string aircraftName in bindingGroups.AllAircraftNames)
        //{
        //    //  For each Joystick
        //    foreach(var joystick in data.Joysticks.Values)
        //    {




        //        //  All RinceDCS buttons include in the add list
        //        List<BindingAddsRemoves> rinceDCSAdds = (from bindingGroup in bindingGroups.Groups
        //                                                 from stick in bindingGroup.Joysticks
        //                                                 from boundAircraft in bindingGroup.Aircraft
        //                                                 where stick.Buttons.Count() > 0 &&
        //                                                       stick.Joystick == stick.Joystick &&
        //                                                       boundAircraft.AircraftName == aircraftName && boundAircraft.IsActive == true
        //                                                 select new BindingAddsRemoves()
        //                                                 {
        //                                                    AircraftName = boundAircraft.AircraftName,
        //                                                    Joystick = stick.Joystick,
        //                                                    IsAxis = bindingGroup.IsAxis,
        //                                                    BindingId = boundAircraft.BindingId,
        //                                                    Command = boundAircraft.Command,
        //                                                    AddButtons = stick.Buttons
        //                                                 }).ToList();

        //        //  Add bindings to be removed from Aircraft
        //        //      - Include all existing Remove bindings in current DCS file that are not in the Bindings list to be added
        //        //      - Include all existing Add bindings in current DCS file that are not in the Bindings list to be added
        //        //              (need to test, can we ignore old adds, or do we need a remove in file to delete them?)
        //        //  This is done at the button level, i.e. a Binding could have both Add and Remove actions depending on changes
        //        //  to the bound buttons

        //        //List<BindingAddsRemoves> dcsRemoves = (from dcsBinding in data.Bindings.Values
        //        //                                       from ajb in dcsBinding.AircraftJoystickBindings.Values
        //        //                                       where ajb.AircraftKey.Name == aircraftName &&
        //        //                                             ajb.JoystickKey == joystick.Key &&
        //        //                                             ajb.ButtonChanges.RemovedButtons.Count() > 0 &&
        //        //                                             !rinceDCSAdds.Any(r => r.BindingId == dcsBinding.Key.Id && r.AddButtons.ButtonName == removeButton.Name)
        //        //                                       select new BindingAddsRemoves()
        //        //                                       {
        //        //                                            AircraftName = aircraftName,
        //        //                                            Joystick = joystick.Joystick,
        //        //                                            IsAxis = dcsBinding.IsAxis,
        //        //                                            BindingId = dcsBinding.Key.Id,
        //        //                                            Command = dcsBinding.Command,
        //        //                                            RemoveButtons = ajb.ButtonChanges.RemovedButtons
        //        //                                       }).ToList();

        //        //List < BindingAddsRemoves > dcsAddsNoRinceAdds = (from dcsBinding in data.Bindings.Values
        //        //                                                  from ajb in dcsBinding.AircraftJoystickBindings.Values
        //        //                                                  where ajb.AircraftKey.Name == aircraftName &&
        //        //                                                        ajb.JoystickKey == joystick.Key &&
        //        //                                                        ajb.ButtonChanges.AddedButtons.Count() > 0 &&
        //        //                                                        !rinceDCSAdds.Any(r => r.BindingId == dcsBinding.Key.Id && r.AddButtons.ButtonName == addButtons.Name)
        //        //                                                  select new BindingAddsRemoves()
        //        //                                                  {
        //        //                                                    AircraftName = aircraftName,
        //        //                                                    Joystick = joystick.Joystick,
        //        //                                                    IsAxis = dcsBinding.IsAxis,
        //        //                                                    BindingId = dcsBinding.Key.Id,
        //        //                                                    Command = dcsBinding.Command,
        //        //                                                    RemoveButtons = ajb.ButtonChanges.AddedButtons
        //        //                                                  }).ToList();

        //        //var newBindings = rinceDCSAdds.Concat(dcsRemoves).Concat(dcsAddsNoRinceAdds).OrderBy(row => row.IsAxis).ThenBy(row => row.BindingId);
        //        //BuildLuaFile(savedGameFolderPath, aircraftName, joystick.Joystick.DCSName, newBindings);
        //    }
        //}
    }

    private void BuildLuaFile(string savedGameFolderPath, IEnumerable<GameUpdateButton> updates)
    {
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
                else if(prevUpdate.AddButton != null && update.RemoveButton != null)
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
                    Command = name,
                    IsAxis = id.StartsWith("a")
                };
                data.Bindings[bindKey] = binding;
            }

            if (!binding.Aircraft.ContainsKey(aircraft.Key))
            {
                binding.Aircraft[aircraft.Key] = new DCSAircraftBinding() { Key = aircraft.Key, Command = name, Category = category };
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
            List<string> modifiers = button.AddButton != null ? button.AddButton.Modifiers != null ? button.AddButton.Modifiers : null : null;
            if(modifiers != null && modifiers.Count > 0)
            {
                sb.AppendLine("\t\t\t\t\t[\"reformers\"] = {");
                for(int index = 0; index < modifiers.Count; index++)
                {
                    sb.AppendLine("\t\t\t\t\t\t[" + (index + 1) + "] = \"" + modifiers[index] + "\",");
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
