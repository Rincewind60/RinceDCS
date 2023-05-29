using HtmlAgilityPack;
using MoonSharp.Interpreter;
using RinceDCS.Models;
using RinceDCS.ServiceModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        string savedGamesAircraftPath = savedGameFolderPath + "\\Config\\Input";
        if (Directory.Exists(savedGamesAircraftPath))
        {
            BuildButtonBindingsFromSavedGame(data, savedGamesAircraftPath);
        }

        //foreach(KeyValuePair<DCSBindingKey, DCSBinding> binding in data.Bindings)
        //{
        //    BuildAssignedButtons(binding.Value);
        //}

        return data;
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
        foreach (KeyValuePair<DCSAircraftKey, DCSAircraft> aircraft in data.Aircraft)
        {
            foreach (KeyValuePair<DCSJoystickKey, DCSJoystick> stick in data.Joysticks)
            {
                string aircraftStickHtmlPath = htmlFolderPath + "\\" + aircraft.Key.Name + "\\" + stick.Value.Joystick.DCSName + ".html";
                if (File.Exists(aircraftStickHtmlPath))
                {
                    ReadAircraftStickHtmlFile(data, aircraft.Value, stick.Value, aircraftStickHtmlPath);
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
            string category = row.ChildNodes[5].GetDirectInnerText().Trim();
            string id = row.ChildNodes[7].GetDirectInnerText().Trim();

            DCSBindingKey bindKey = new(id);
            if (!data.Bindings.ContainsKey(bindKey))
            {
                DCSBinding binding = new DCSBinding()
                {
                    Key = new(id),
                    CommandName = name
                };
                data.Bindings[bindKey] = binding;
            }

            if (!data.Bindings[bindKey].AircraftWithBinding.ContainsKey(aircraft.Key))
            {
                data.Bindings[bindKey].AircraftWithBinding[aircraft.Key] = new DCSAircraftBinding() { Key = aircraft.Key, CommandName = name, CategoryName = category };
            }
            if (!data.Bindings[bindKey].JoysticksWithBinding.ContainsKey(stck.Key))
            {
                data.Bindings[bindKey].JoysticksWithBinding[stck.Key] = stck;
            }
        }
    }

    private void BuildButtonBindingsFromSavedGame(DCSData data, string savedGamesAircraftPath)
    {
        foreach(KeyValuePair<DCSAircraftKey, DCSAircraft> aircraft in data.Aircraft)
        {
            string aircraftJoystickFolderPath = savedGamesAircraftPath + "\\" + aircraft.Key.Name + "\\joystick";
            foreach (KeyValuePair<DCSJoystickKey, DCSJoystick> stick in data.Joysticks)
            {
                string aircraftStickPath = aircraftJoystickFolderPath + "\\" + stick.Value.Joystick.DCSName + ".diff.lua";
                if (File.Exists(aircraftStickPath))
                {
                    ReadAircraftStickLuaFile(data, aircraft.Value, stick.Value, aircraftStickPath);
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
                if (sectionName == "changed")
                {
                    ReadChangedAxisLua(bindingData, bindingsTable.Values.ElementAt(j).Table);
                }
                else if (sectionName == "removed")
                {
                    ReadRemovedAxisLua(bindingData, bindingsTable.Values.ElementAt(j).Table);
                }
                else if (sectionName == "name")
                {
                    string name = bindingsTable.Values.ElementAt(j).String;
                }
            }
        }
    }
 
    private void ReadChangedAxisLua(DCSAircraftJoystickBinding bindingData, Table changedTable)
    {
        for (int i = 0; i < changedTable.Keys.Count(); i++)
        {
            Table table = changedTable.Values.ElementAt(i).Table;

            DCSAxisButton axisButton = new();
            bindingData.ChangedAxisButtons.Add(axisButton);

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

    private void ReadRemovedAxisLua(DCSAircraftJoystickBinding bindingData, Table removedTable)
    {
        for (int i = 0; i < removedTable.Values.Count(); i++)
        {
            Table table = removedTable.Values.ElementAt(i).Table;
            string button = table.Values.ElementAt(i).String;
            DCSButton removedButton = new() { Key = new(button) };
            bindingData.RemovedAxisButtons.Add(removedButton);

            if(bindingData.AssignedButtons.ContainsKey(removedButton.Key))
            {
                bindingData.AssignedButtons.Remove(removedButton.Key);
            }
        }
    }

    private void ReadKeyDiffsLua(DCSData data, DCSAircraft aircraft, DCSJoystick stick, Table keyDiffsTable)
    {
        for (int i = 0; i < keyDiffsTable.Keys.Count(); i++)
        {
            DCSBindingKey bindingKey = new(keyDiffsTable.Keys.ElementAt(i).String);
            DCSBinding binding = data.Bindings[bindingKey];

            DCSAircraftJoystickBinding bindingData = CreateBindingData(aircraft, stick, binding);


            Table bindingsTable = keyDiffsTable.Values.ElementAt(i).Table;

            for (int j = 0; j < bindingsTable.Keys.Count(); j++)
            {
                string sectionName = bindingsTable.Keys.ElementAt(j).String;
                if (sectionName == "added")
                {
                    ReadAddedKeyLua(bindingData, bindingsTable.Values.ElementAt(j).Table);
                }
                else if (sectionName == "removed")
                {
                    ReadRemovedKeyLua(bindingData, bindingsTable.Values.ElementAt(j).Table);
                }
                else if (sectionName == "name")
                {
                    string name = bindingsTable.Values.ElementAt(j).String;
                }
            }
        }
    }

    private void ReadAddedKeyLua(DCSAircraftJoystickBinding bindingData, Table addedTable)
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
            bindingData.AddedKeyButtons.Add(newButton);

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

    private void ReadRemovedKeyLua(DCSAircraftJoystickBinding bindingData, Table removedTable)
    {
        for (int i = 0; i < removedTable.Values.Count(); i++)
        {
            Table table = removedTable.Values.ElementAt(i).Table;
            string button = table.Values.ElementAt(i).String;
            DCSButton removedButton = new() { Key = new(button) };
            bindingData.RemovedKeyButtons.Add(removedButton);

            if (bindingData.AssignedButtons.ContainsKey(removedButton.Key))
            {
                bindingData.AssignedButtons.Remove(removedButton.Key);
            }
        }
    }

    private DCSAircraftJoystickBinding CreateBindingData(DCSAircraft aircraft, DCSJoystick stick, DCSBinding binding)
    {
        DCSAircraftJoystickBinding bindingData;
        Tuple<DCSAircraftKey, DCSJoystickKey> bindingDataKey = Tuple.Create(aircraft.Key, stick.Key);

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

    //private void BuildAssignedButtons(DCSBinding binding)
    //{
    //    foreach(KeyValuePair<Tuple<DCSAircraftKey, DCSJoystickKey>, DCSAircraftJoystickBinding> ajBinding in binding.AircraftJoystickBindings)
    //    {
    //        Dictionary<string, DCSButton> buttons = new();

    //        foreach(DCSAxisButton button in ajBinding.Value.ChangedAxisButtons)
    //        {
    //            buttons[button.Name] = button;
    //        }
    //        foreach(DCSButton button in ajBinding.Value.RemovedAxisButtons)
    //        {
    //            if(buttons.ContainsKey(button.Name))
    //            {
    //                buttons.Remove(button.Name);
    //            }
    //        }
    //        foreach (DCSKeyButton button in ajBinding.Value.AddedKeyButtons)
    //        {
    //            buttons[button.Name] = button;
    //        }
    //        foreach (DCSButton button in ajBinding.Value.RemovedKeyButtons)
    //        {
    //            if (buttons.ContainsKey(button.Name))
    //            {
    //                buttons.Remove(button.Name);
    //            }
    //        }

    //        if(buttons.Count > 0)
    //        {
    //            ajBinding.Value.AssignedButtons = buttons.Values.ToList();
    //        }
    //    }
    //}

//private void GetAircraftFromFolder(Dictionary<string, DCSAircraftBinding> aircraft, string aircraftPath)
//{
//    foreach(string aircraftDirectory in Directory.GetDirectories(aircraftPath))
//    {
//        if (AreAircraftInSubDirectory(aircraftDirectory))
//        {
//            foreach(string subAircraftDirectory in Directory.GetDirectories(aircraftDirectory + "\\Input"))
//            {
//                AddAircraft(aircraft, subAircraftDirectory, sticks);
//            }
//        }
//        else
//        {
//            AddAircraft(aircraft, aircraftDirectory, sticks);
//        }
//    }
//}

//private void AddAircraft(Dictionary<string, DCSAircraftBinding> aircraft, string aircraftDirectory)
//{
//    //string aircraftName = GetAircraftName(aircraftDirectory);
//    //if (IsAircraft(aircraftName))
//    //{
//    //    List<DCSJoystickBindings> joystickBindings = GetJoystickBindings(aircraftDirectory, sticks);
//    //    DCSAircraft newAircraft = new DCSAircraft()
//    //    {
//    //        Name = aircraftName,
//    //        JoystickBindings = joystickBindings
//    //    };

//    //    aircraft.Add(aircraftName, newAircraft);
//    //}
//}

//private List<DCSJoystickButtonBinding> GetJoystickBindings(string aircraftDirectory)
//{
//    List<DCSJoystickButtonBinding> joystickBindings = new(); 

//    //foreach (string file in Directory.GetFiles(aircraftDirectory + "\\joystick"))
//    //{
//    //    string fileName = Path.GetFileNameWithoutExtension(file);
//    //    if(sticks.Any(stick => stick.Name == fileName))
//    //    {
//    //        DCSJoystickButtonBinding bindings = new();

//    //        joystickBindings.Add(bindings);
//    //    }
//    //}

//    return joystickBindings;
//}

    //private bool AreAircraftInSubDirectory(string aircraftDirectory)
    //{
    //    return Directory.Exists(aircraftDirectory + "\\Input");
    //}

    //private static bool IsAircraft(string aircraftName)
    //{
    //    return  String.Compare(aircraftName, "Keyboard", true) != 0 &&
    //            String.Compare(aircraftName, "Joystick", true) != 0 &&
    //            String.Compare(aircraftName, "Mouse", true) != 0;
    //}

    //private static string GetAircraftName(string aircraftDirectory)
    //{
    //    return aircraftDirectory.Split('\\').Last();
    //}

    //private void GetAvailableAircracftBindings(string gameName)
    //{
    //    //string aircraftPath = Ioc.Default.GetRequiredService<ISettingsService>().GetSetting(RinceDCSSettings.SavedGamesPath) + "\\" + gameName + "\\InputLayoutsTxt";

    //    //foreach (string aircraftDirectory in Directory.GetDirectories(aircraftPath))
    //    //{
    //    //    foreach(AttachedJoystick stick in sticks)
    //    //    {
    //    //        string stickPath = aircraftDirectory + "\\" + stick.DCSName;
    //    //        if (File.Exists(stickPath))
    //    //        {
    //    //            var doc = new HtmlDocument();
    //    //            doc.Load(stickPath);
    //    //        }
    //    //    }
    //    //}

    //}
}
