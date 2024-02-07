using CommunityToolkit.Mvvm.DependencyInjection;
using HtmlAgilityPack;
using Microsoft.UI.Xaml.Controls;
using MoonSharp.Interpreter;
using NLog;
using RinceDCS.Models;
using RinceDCS.ServiceModels;
using RinceDCS.Utilities;
using SharpDX.DirectInput;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Windows.Devices.Geolocation;
using Windows.Media.AppBroadcasting;

namespace RinceDCS.Services;

public class DCSService : IDCSService
{
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

        if (Directory.Exists(newSavedGamesFolder))
        {
            return newSavedGamesFolder;
        }

        return currentSavedGamesFolder;
    }

    public void UpdateGameBindingData(string savedGameFolderPath, RinceDCSGroups bindingGroups, DCSData data)
    {
        //  For each Aircraft
        foreach(string aircraftName in bindingGroups.AllAircraftNames.Values)
        {
            //  For each Joystick
            foreach(var stick in data.Joysticks.Values)
            {
                List<BindingAddsRemoves> bindingsWithAdds = (from bindingGroup in bindingGroups.Groups
                                                          from joystick in bindingGroup.JoystickBindings
                                                          from boundAircraft in bindingGroup.AircraftBindings
                                                          from addButton in joystick.Buttons
                                                          where joystick.Buttons.Count() > 0 &&
                                                                joystick.Joystick == stick.Joystick &&
                                                                boundAircraft.AircraftName == aircraftName && boundAircraft.IsActive == true
                                                          select new BindingAddsRemoves()
                                                          {
                                                              AircraftName = boundAircraft.AircraftName,
                                                              Joystick = joystick.Joystick,
                                                              IsAxisBinding = bindingGroup.IsAxisBinding,
                                                              BindingId = boundAircraft.BindingId,
                                                              CommandName = boundAircraft.CommandName,
                                                              AddButton = addButton
                                                          }).ToList();

                var btn = from addButton in bindingsWithAdds
                          where addButton.AddButton.ButtonName == "JOY_BTN5"
                          select addButton;


                //  Add bindings to be removed from Aircraft
                //      - Include all exisintg Remove bindings in current DCS file that are not in the Bindings list to be added
                //      - Include all existing Add bindings in current DCS file that are not in the Bindings list to be added
                //              (need to test, can we ignore old adds, or do we need a remove in file to delete them?)
                //  This is done at the button level, i.e. a Binding could have both Add and Remove actions depending on changes
                //  to the bound buttons

                List<BindingAddsRemoves> bindingsWithRemoves = (from dcsBinding in data.Bindings.Values
                                                                from ajb in dcsBinding.AircraftJoystickBindings.Values
                                                                from removeButton in (new List<IDCSButton>(ajb.SavedGamesButtonChanges.RemovedAxisButtons).Concat(ajb.SavedGamesButtonChanges.RemovedKeyButtons))
                                                                where ajb.AircraftKey.Name == aircraftName &&
                                                                      ajb.JoystickKey == stick.Key &&
                                                                      (ajb.SavedGamesButtonChanges.RemovedAxisButtons.Count() > 0 ||
                                                                       ajb.SavedGamesButtonChanges.RemovedKeyButtons.Count() > 0)
                                                                select new BindingAddsRemoves()
                                                                {
                                                                    AircraftName = aircraftName,
                                                                    Joystick = stick.Joystick,
                                                                    IsAxisBinding = dcsBinding.IsAxisBinding,
                                                                    BindingId = dcsBinding.Key.Id,
                                                                    CommandName = dcsBinding.CommandName,
                                                                    RemoveButton = removeButton
                                                                }).ToList();


                List < BindingAddsRemoves > bindings = (from dcsBinding in data.Bindings.Values
                                                        from ajb in dcsBinding.AircraftJoystickBindings.Values
                                                        from removeButton in (new List<IDCSButton>(ajb.SavedGamesButtonChanges.RemovedAxisButtons).Concat(ajb.SavedGamesButtonChanges.RemovedKeyButtons))
                                                        where ajb.AircraftKey.Name == aircraftName &&
                                                                ajb.JoystickKey == stick.Key &&
                                                                (ajb.SavedGamesButtonChanges.RemovedAxisButtons.Count() > 0 ||
                                                                ajb.SavedGamesButtonChanges.RemovedKeyButtons.Count() > 0)
                                                        select new BindingAddsRemoves()
                                                        {
                                                            AircraftName = aircraftName,
                                                            Joystick = stick.Joystick,
                                                            IsAxisBinding = dcsBinding.IsAxisBinding,
                                                            BindingId = dcsBinding.Key.Id,
                                                            CommandName = dcsBinding.CommandName,
                                                            RemoveButton = removeButton
                                                        }).ToList();


                //List<BindingAddsRemoves> bindings = (from bindingGroup in bindingGroups.Groups
                //                                     from joystick in bindingGroup.JoystickBindings
                //                                     from boundAircraft in bindingGroup.AircraftBindings
                //                                     from addButton in joystick.Buttons
                //                                     from dcsBinding in data.Bindings.Values
                //                                     from ajb in dcsBinding.AircraftJoystickBindings.Values
                //                                     from removeButton in (new List<IDCSButton>(ajb.SavedGamesButtonChanges.RemovedAxisButtons).Concat(ajb.SavedGamesButtonChanges.RemovedKeyButtons))
                //                                     where joystick.Buttons.Count() > 0 &&
                //                                           joystick.Joystick == stick.Joystick &&
                //                                           boundAircraft.AircraftName == aircraftName &&
                //                                           boundAircraft.IsActive == true &&
                //                                           dcsBinding.Key.Id == boundAircraft.BindingId &&
                //                                           ajb.AircraftKey.Name == aircraftName &&
                //                                           ajb.JoystickKey == stick.Key &&
                //                                           (ajb.SavedGamesButtonChanges.RemovedAxisButtons.Count() > 0 ||
                //                                            ajb.SavedGamesButtonChanges.RemovedKeyButtons.Count() > 0)
                //                                     select new BindingAddsRemoves()
                //                                     {
                //                                         AircraftName = aircraftName,
                //                                         Joystick = stick.Joystick,
                //                                         IsAxisBinding = bindingGroup.IsAxisBinding,
                //                                         BindingId = boundAircraft.BindingId,
                //                                         CommandName = boundAircraft.CommandName,
                //                                         AddButton = addButton,
                //                                         RemoveButton = removeButton
                //                                     }).ToList();

                foreach (BindingAddsRemoves record in bindingsWithAdds)
                {
                    string modifiers = record.AddButton is RinceDCSGroupKeyButton ? (((RinceDCSGroupKeyButton)record.AddButton).Modifiers.Count() > 0).ToString() : "";
                    RinceLogger.Log.Info(record.AircraftName + ", " + record.Joystick.Name + ", " + record.IsAxisBinding + ", " + record.BindingId + ", " + record.CommandName + ", " + record.AddButton.ButtonName + ", " + modifiers);
                }

                BuildLuaFile(savedGameFolderPath, data, bindingsWithAdds);
            }
        }
    }

    private void BuildLuaFile(string savedGameFolderPath, DCSData data, List<BindingAddsRemoves> bindingsWithAdds)
    {
        string luaBackupFolder = "D:/RinceConfigBackup/Input/" + bindingsWithAdds[0].AircraftName + "/joystick/";
        string luaFileName = luaBackupFolder + bindingsWithAdds[0].Joystick.DCSName + ".diff.lua";

        if (!Directory.Exists(luaBackupFolder))
        {
            Directory.CreateDirectory(luaBackupFolder);
        }

        DCSLuaFileBuilder luaBuilder = new(luaFileName);

        //  Add bindings to be added to Aircraft
        foreach (var binding in bindingsWithAdds)
        {
            if (binding.IsAxisBinding)
            {
                luaBuilder.AddGroupAxisButtons(binding);
            }
            else
            {
                luaBuilder.AddGroupKeyButtons(binding);
            }
        }

        //  Add bindings to be removed from Aircraft
        //      - Include all exisintg Remove bindings in current DCS file that are not in the Bindings list to be added
        //      - Include all existing Add bindings in current DCS file that are not in the Bindings list to be added
        //  This is done at the button level, i.e. a Binding could have both Add and Remove actions depending on changes
        //  to the bound buttons

        /// TODO: Implement remove in lua files


        luaBuilder.WriteFile();
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
                    ReadAircraftStickLuaFile(data, aircraft, stick, aircraftStickPath);
                }
            }
        }
    }

    private void ReadAircraftStickLuaFile(DCSData data, DCSAircraft aircraft, DCSJoystick stick, string aircraftStickPath)
    {
        Table table = Script.RunFile(aircraftStickPath).Table;

        for(int i = 0; i < table.Keys.Count(); i++)
        {
            string key = table.Keys.ElementAt(i).String;
            if (key == "axisDiffs")
            {
                ReadAxisDiffsLua(data, aircraft, stick, table.Values.ElementAt(i).Table);
            }
            else if(key == "keyDiffs")
            {
                ReadKeyDiffsLua(data, aircraft, stick, table.Values.ElementAt(i).Table);
            }
        }
    }

    private void ReadAxisDiffsLua(DCSData data, DCSAircraft aircraft, DCSJoystick stick, Table axisDiffsTable)
    {
        for(int i = 0; i < axisDiffsTable.Keys.Count(); i++)
        {
            DCSBindingKey bindingKey = new(axisDiffsTable.Keys.ElementAt(i).String);
            DCSBinding binding = data.Bindings[bindingKey];

            DCSAircraftJoystickBinding bindingData = CreateBindingData(aircraft, stick, binding);

            Table bindingsTable = axisDiffsTable.Values.ElementAt(i).Table;

            for (int j = 0; j < bindingsTable.Keys.Count(); j++)
            {
                string sectionName = bindingsTable.Keys.ElementAt(j).String;
                if(sectionName == "added")
                {
                    ReadAddedAxisLua(bindingData, bindingsTable.Values.ElementAt(j).Table, bindingData.SavedGamesButtonChanges);
                }
                else if (sectionName == "changed")
                {
                    ReadChangedAxisLua(bindingData, bindingsTable.Values.ElementAt(j).Table, bindingData.SavedGamesButtonChanges);
                }
                else if (sectionName == "removed")
                {
                    ReadRemovedAxisLua(bindingData, bindingsTable.Values.ElementAt(j).Table, bindingData.SavedGamesButtonChanges);
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

            DCSAxisButton axisButton = new();
            changes.AddedAxisButtons.Add(axisButton);

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

            bindingData.AssignedButtons[axisButton.Name] = axisButton;
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
                    ReadAxisFilterLua(axisButton, table.Values.ElementAt(j).Table);
                }
                else if (key == "key")
                {
                    axisButton.Name = table.Values.ElementAt(j).String;
                }
            }

            bindingData.AssignedButtons[axisButton.Name] = axisButton;
        }
    }

    private void ReadAxisFilterLua(DCSAxisButton axisButton, Table filterTable)
    {
        axisButton.Filter = new();

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
            DCSAxisButton removedButton = new() { Name = table.Values.ElementAt(i).String };
            changes.RemovedAxisButtons.Add(removedButton);

            if(bindingData.AssignedButtons.ContainsKey(removedButton.Name))
            {
                bindingData.AssignedButtons.Remove(removedButton.Name);
            }
        }
    }

    private void ReadKeyDiffsLua(DCSData data, DCSAircraft aircraft, DCSJoystick stick, Table keyDiffsTable)
    {
        for (int i = 0; i < keyDiffsTable.Keys.Count(); i++)
        {
            DCSBindingKey bindingKey = new(keyDiffsTable.Keys.ElementAt(i).String);

            if(!data.Bindings.ContainsKey(bindingKey))
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
                    ReadAddedKeyLua(bindingData, bindingsTable.Values.ElementAt(j).Table, bindingData.SavedGamesButtonChanges);
                }
                else if (sectionName == "removed")
                {
                    ReadRemovedKeyLua(bindingData, bindingsTable.Values.ElementAt(j).Table, bindingData.SavedGamesButtonChanges);
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
            DCSKeyButton newButton = new();

            for (int j = 0; j < table.Keys.Count(); j++)
            {
                string sectionName = table.Keys.ElementAt(j).String;
                if(sectionName == "key")
                {
                    newButton.Name = table.Values.ElementAt(j).String;
                }
                else if(sectionName == "reformers")
                {
                    ReadKeyModifersLua(newButton, table.Values.ElementAt(j).Table);
                }
            }
            changes.AddedKeyButtons.Add(newButton);

            bindingData.AssignedButtons[newButton.Name] = newButton;
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
                DCSKeyButton removedButton = new() { Name = table.Values.ElementAt(j).String };
                changes.RemovedKeyButtons.Add(removedButton);

                if (bindingData.AssignedButtons.ContainsKey(removedButton.Name))
                {
                    bindingData.AssignedButtons.Remove(removedButton.Name);
                }
            }
        }
    }

    private DCSAircraftJoystickBinding CreateBindingData(DCSAircraft aircraft, DCSJoystick stick, DCSBinding binding)
    {
        DCSAircraftJoystickBinding bindingData;
        DCSAircraftJoystickKey bindingDataKey = new(aircraft.Key.Name, stick.Key.Id);

        if (binding.AircraftJoystickBindings.ContainsKey(bindingDataKey))
        {
            bindingData = binding.AircraftJoystickBindings[bindingDataKey];
        }
        else
        {
            bindingData = new DCSAircraftJoystickBinding
            {
                AircraftKey = aircraft.Key,
                JoystickKey = stick.Key
            };
            binding.AircraftJoystickBindings[bindingDataKey] = bindingData;
        }

        return bindingData;
    }
}

public class BindingAddsRemoves
{
    public string AircraftName;
    public AttachedJoystick Joystick;
    public bool IsAxisBinding;
    public string BindingId;
    public string CommandName;
    public IRinceDCSGroupButton AddButton;
    public IDCSButton RemoveButton;
}

public class DCSLuaFileBuilder
{
    private readonly StringBuilder sbAxis = new();
    private readonly StringBuilder sbKeys = new();
    private readonly string luaFileName;

    public DCSLuaFileBuilder(string fileName)
    {
        luaFileName = fileName;
    }

    public void AddGroupAxisButtons(BindingAddsRemoves binding)
    {
        //sbAxis.AppendLine("\t\t[\"" + binding.BindingId + "\"] = {");
        //sbAxis.AppendLine("\t\t\t[\"name\"] = \"" + binding.CommandName + "\",");
        //sbAxis.AppendLine("\t\t\t[\"added\"] = {");
        //for (int buttonIndex = 0; buttonIndex < binding.Buttons.Count(); buttonIndex++)
        //{
        //    RinceDCSGroupAxisButton axisButton = (RinceDCSGroupAxisButton)binding.Buttons[buttonIndex];
        //    sbAxis.AppendLine("\t\t\t\t[" + (buttonIndex + 1).ToString() + "] = {");
        //    sbAxis.AppendLine("\t\t\t\t\t[\"key\"] = \"" + axisButton.ButtonName + "\",");
        //    sbAxis.AppendLine("\t\t\t\t\t[\"filter\"] = {");
        //    sbAxis.AppendLine("\t\t\t\t\t\t[\"curvature\"] = {");
        //    for (int curveIndex = 0; curveIndex < axisButton.Curvature.Count(); curveIndex++)
        //    {
        //        sbAxis.AppendLine("\t\t\t\t\t\t\t[" + (curveIndex + 1).ToString() + "] = " + axisButton.Curvature[curveIndex].ToString() + ",");
        //    }
        //    sbAxis.AppendLine("\t\t\t\t\t\t},");
        //    sbAxis.AppendLine("\t\t\t\t\t\t[\"deadzone\"] = " + axisButton.Deadzone.ToString() + ",");
        //    sbAxis.AppendLine("\t\t\t\t\t\t[\"invert\"] = " + axisButton.Invert.ToString().ToLower() + ",");
        //    sbAxis.AppendLine("\t\t\t\t\t\t[\"saturationX\"] = " + axisButton.SaturationX.ToString() + ",");
        //    sbAxis.AppendLine("\t\t\t\t\t\t[\"saturationY\"] = " + axisButton.SaturationY.ToString() + ",");
        //    sbAxis.AppendLine("\t\t\t\t\t\t[\"slider\"] = " + axisButton.Slider.ToString().ToLower() + ",");
        //    sbAxis.AppendLine("\t\t\t\t\t},");
        //    sbAxis.AppendLine("\t\t\t\t},");
        //}
        //sbAxis.AppendLine("\t\t\t},");
        //sbAxis.AppendLine("\t\t},");
    }

    public void AddGroupKeyButtons(BindingAddsRemoves binding)
    {
        //sbKeys.AppendLine("\t\t[\"" + binding.BindingId + "\"] = {");
        //sbKeys.AppendLine("\t\t\t[\"name\"] = \"" + binding.CommandName + "\",");
        //sbKeys.AppendLine("\t\t\t[\"added\"] = {");
        //for (int buttonIndex = 0; buttonIndex < binding.Buttons.Count(); buttonIndex++)
        //{
        //    RinceDCSGroupKeyButton axisButton = (RinceDCSGroupKeyButton)binding.Buttons[buttonIndex];
        //    sbKeys.AppendLine("\t\t\t\t[" + (buttonIndex + 1).ToString() + "] = {");
        //    sbKeys.AppendLine("\t\t\t\t\t[\"key\"] = \"" + axisButton.ButtonName + "\",");
        //    if (axisButton.Modifiers.Count() > 0)
        //    {
        //        sbKeys.AppendLine("\t\t\t\t\t[\"reformers\"] = {");
        //        for (int reformerIndex = 0; reformerIndex < axisButton.Modifiers.Count(); reformerIndex++)
        //        {
        //            sbKeys.AppendLine("\t\t\t\t\t\t[" + (reformerIndex + 1).ToString() + "] = \"" + axisButton.Modifiers[reformerIndex].ToString() + "\",");
        //        }
        //        sbKeys.AppendLine("\t\t\t\t\t},");
        //    }
        //    sbKeys.AppendLine("\t\t\t\t},");
        //}
        //sbKeys.AppendLine("\t\t\t},");
        //sbKeys.AppendLine("\t\t},");
    }

    public void WriteFile()
    {
        StringBuilder sb = new();

        sb.AppendLine("local diff = {");
        sb.AppendLine("\t[\"axisDiffs\"] = {");
        sb.Append(sbAxis);
        sb.AppendLine("\t},");
        sb.AppendLine("\t[\"keyDiffs\"] = {");
        sb.Append(sbKeys);
        sb.AppendLine("\t},");
        sb.AppendLine("}");
        sb.AppendLine("return diff");

        File.WriteAllText(luaFileName, sb.ToString());
    }
}
