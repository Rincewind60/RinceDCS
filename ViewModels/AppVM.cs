// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using RinceDCS.Models;
using RinceDCS.Properties;
using RinceDCS.Services;
using RinceDCS.Utilities;
using RinceDCS.ViewModels.Helpers;
using RinceDCS.ViewModels.Messages;
using RinceDCS.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace RinceDCS.ViewModels;

public enum DetailsDisplayMode
{
    None,
    ViewActions,
    ViewSticks,
    EditSticks,
    EditGroups,
    EditLayouts
}

public partial class AppVM : ObservableRecipient
{
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    private RinceDCSFile currentFile;

    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    private RinceDCSInstance currentInstance;

    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    private DCSData currentInstanceDCSData;

    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    private RinceDCSGroups currentInstanceGroups;

    [ObservableProperty]
    private List<AttachedJoystick> attachedJoysticks;

    [ObservableProperty]
    private bool isRinceDCSFileLoaded = false;

    [ObservableProperty]
    private DetailsDisplayMode? joystickMode;

    public AppVM()
    {
        IsActive = true;

        JoystickMode = DetailsDisplayMode.None;

        AttachedJoysticks = JoystickService.Default.GetAttachedJoysticks();

        WeakReferenceMessenger.Default.Register<GameInstancesUpdatedMessage>(this, (r, m) =>
        {
            DeleteInstancesNoLongerRequired(m.Value);
            UpdateExistingInstances(m.Value);
            AddNewInstances(m.Value);
        });

        string savedPath = Settings.Default.LastSavePath;
        if (!string.IsNullOrWhiteSpace(savedPath))
        {
            DoOpen(savedPath);
        }
        if (CurrentFile == null)
        {
            New();
        }
    }

    [RelayCommand]
    private void New()
    {
        Settings.Default.LastSavePath = null;
        Settings.Default.Save();

        RinceDCSFile newFile = new();
        LoadJoysticks(newFile);

        SetCurrentRinceDCSFile(newFile);
    }

    [RelayCommand]
    private async Task Open()
    {
        if (CurrentFile != null)
        {
            bool? result = await DialogService.Default.OpenConfirmationDialog("Save RinceDCS File", "Do you want to save the existing file first?");
            if (result.HasValue && result.Value)
            {
                await FileService.Default.SaveRinceDCSFile(CurrentFile);
            }
        }

        string path = await DialogService.Default.OpenPickFile(".json");
        if (!string.IsNullOrWhiteSpace(path))
        {
            DoOpen(path);
        }
    }

    private void DoOpen(string path)
    {
        RinceDCSFile openedRinceDCSFile = Task.Run(() => FileService.Default.OpenRinceDCSFile(path)).GetAwaiter().GetResult();
        if (openedRinceDCSFile != null)
        {
            CheckForNewJoysticks(openedRinceDCSFile);
            SetCurrentRinceDCSFile(openedRinceDCSFile);
        }
    }

    private void CheckForNewJoysticks(RinceDCSFile openedRinceDCSFile)
    {
        foreach (AttachedJoystick stick in AttachedJoysticks)
        {
            bool existingStick = false;
            foreach (RinceDCSJoystick gameStick in openedRinceDCSFile.Joysticks)
            {
                if (stick == gameStick.AttachedJoystick)
                {
                    existingStick = true;
                    break;
                }
            }
            if (existingStick == false)
            {
                RinceDCSJoystick newJoystick = new() { AttachedJoystick = stick };

                AddJoystickButtons(newJoystick);

                openedRinceDCSFile.Joysticks.Add(newJoystick);

            }
        }

        openedRinceDCSFile.Joysticks.Sort();
    }

    [RelayCommand]
    private void Save()
    {
        ApplyChangesToModels();
        Task.Run(() => FileService.Default.SaveRinceDCSFile(CurrentFile)).Wait();
    }

    [RelayCommand]
    private void SaveAs()
    {
        ApplyChangesToModels();
        Task.Run(() => FileService.Default.SaveAsRinceDCSFile(CurrentFile)).Wait();
    }

    [RelayCommand]
    private void Exit()
    {

    }

    [RelayCommand]
    private async Task ExportImages()
    {
        string exportFolder = await DialogService.Default.OpenPickFolder();
        JoystickVMHelper helper = new(CurrentInstanceDCSData);
        foreach (RinceDCSJoystick stick in CurrentFile.Joysticks)
        {
            Dictionary<AssignedButtonKey, RinceDCSJoystickButton> buttonsOnLayout = helper.GetJoystickButtonsOnLayout(stick);
            foreach (RinceDCSAircraft aircraft in CurrentInstance.Aircraft)
            {
                List<AssignedButton> assignedButtons = helper.GetAssignedButtons(stick, buttonsOnLayout, CurrentInstance.Name, aircraft.Name);
                string saveFilePath = exportFolder + "\\" + aircraft.Name + "_" + stick.AttachedJoystick.Name + ".png";
                WeakReferenceMessenger.Default.Send(new ExportAssignedButtonsImageMessage(stick, assignedButtons, saveFilePath));
            }
        }
    }

    [RelayCommand]
    private void ExportKneeboards()
    {
        JoystickVMHelper helper = new(CurrentInstanceDCSData);
        foreach (RinceDCSJoystick stick in CurrentFile.Joysticks)
        {
            Dictionary<AssignedButtonKey, RinceDCSJoystickButton> buttonsOnLayout = helper.GetJoystickButtonsOnLayout(stick);
            foreach (RinceDCSAircraft aircraft in CurrentInstance.Aircraft)
            {
                List<AssignedButton> assignedButtons = helper.GetAssignedButtons(stick, buttonsOnLayout, CurrentInstance.Name, aircraft.Name);
                WeakReferenceMessenger.Default.Send(new ExportKneeboardMessage(stick, assignedButtons, aircraft.Name));
            }
        }
    }

    [RelayCommand]
    private async Task UpdateDCSAsync()
    {
        List<string> aircraftNames = (from aircraft in CurrentInstance.Aircraft select aircraft.Name).ToList();
        SelectAircraftDialog page = new(aircraftNames);
        ContentDialogResult result = await DialogService.Default.OpenResponsePageDialog("Update DCS", page, "Start Export", null, null, "Cancel");
        if (result == ContentDialogResult.Primary)
        {
            DCSService.Default.UpdateDCSConfigFiles(CurrentInstance.SavedGamesPath, CurrentInstanceGroups, CurrentInstanceDCSData, page.ViewModel.GetSelectedAircraft());
            await DialogService.Default.OpenInfoDialog("Update DCS", "DCS Joystick configuration changes applied");
        }
    }

    public void CurrentInstanceChanged()
    {
        if (CurrentInstance == null)
        {
            CurrentInstanceDCSData = null;
        }
        else
        {
            LoadBindingDataForInstance(CurrentInstance);
            GroupsVMHelper groupsHelper = new(CurrentFile.Joysticks.ToList(), CurrentInstance.ControlsData, CurrentInstance.Groups, CurrentInstance.SavedGamesPath);
            CurrentInstance.Groups = groupsHelper.UpdatedGroups();
            CurrentInstanceDCSData = CurrentInstance.ControlsData;
            CurrentInstanceGroups = CurrentInstance.Groups;
        }
        FilterToolbarVM.Default.ResetFilters(CurrentInstance);
    }

    partial void OnJoystickModeChanged(DetailsDisplayMode? oldValue, DetailsDisplayMode? newValue)
    {
        if (newValue == null)
        {
#pragma warning disable MVVMTK0034
            joystickMode = oldValue;
#pragma warning restore MVVMTK0034
        }
    }

    private void LoadJoysticks(RinceDCSFile rinceDCSFile)
    {
        foreach (AttachedJoystick stick in AttachedJoysticks)
        {
            RinceDCSJoystick newJoystick = new() { AttachedJoystick = stick };

            AddJoystickButtons(newJoystick);

            rinceDCSFile.Joysticks.Add(newJoystick);
        }
    }

    /// <summary>
    /// Remove any existing Joystick info and update with latest
    /// </summary>
    /// <param name="joystick"></param>
    private void AddJoystickButtons(RinceDCSJoystick joystick)
    {
        JoystickInfo info = JoystickService.Default.GetJoystickInfo(joystick.AttachedJoystick);

        joystick.Buttons = new ObservableCollection<RinceDCSJoystickButton>();

        joystick.Buttons.Add(NewJoystickButton("Game", joystick));
        joystick.Buttons.Add(NewJoystickButton("Plane", joystick));
        joystick.Buttons.Add(NewJoystickButton("Joystick", joystick));
        foreach (string item in info.SupportedAxes) joystick.Buttons.Add(NewJoystickButton(item, joystick, IsKeyButton(item)));
        foreach (string item in info.POVs) joystick.Buttons.Add(NewJoystickButton(item, joystick, IsKeyButton(item)));
        foreach (string item in info.Buttons) joystick.Buttons.Add(NewJoystickButton(item, joystick, IsKeyButton(item)));
        //  Now add copies of Buttons for when using Modifier
        foreach (string item in info.SupportedAxes) joystick.Buttons.Add(NewJoystickButton(item, joystick, IsKeyButton(item), true));
        foreach (string item in info.POVs) joystick.Buttons.Add(NewJoystickButton(item, joystick, IsKeyButton(item), true));
        foreach (string item in info.Buttons) joystick.Buttons.Add(NewJoystickButton(item, joystick, IsKeyButton(item), true));
    }

    private RinceDCSJoystickButton NewJoystickButton(string name, RinceDCSJoystick stick, bool isKey = true, bool isModifier = false)
    {
        return new RinceDCSJoystickButton()
        {
            ButtonName = name,
            Font = stick.Font,
            FontSize = stick.FontSize,
            IsKeyButton = isKey,
            IsModifier = isModifier
        };
    }

    private bool IsKeyButton(string item)
    {
        return item.Contains("BTN");
    }

    private void ApplyChangesToModels()
    {
        if (CurrentInstance == null)
        {
            CurrentFile.CurrentInstanceName = null;
        }
        else
        {
            CurrentFile.CurrentInstanceName = CurrentInstance.Name;
        }
    }

    /// <summary>
    /// When a new RinceDCSFile is created the old EinceDCSFile must be replaced.
    /// 
    /// This means updating any ViewModel properties relating to the old RinceDCSFile object.
    /// </summary>
    /// <param name="newFile"></param>
    private void SetCurrentRinceDCSFile(RinceDCSFile newFile)
    {
        IsRinceDCSFileLoaded = false;
        CurrentFile = newFile;
        SetCurrentInstanceForRinceDCSFile();
        IsRinceDCSFileLoaded = true;
    }

    private void SetCurrentInstanceForRinceDCSFile()
    {
        var instanceQuery = from instance in CurrentFile.Instances
                            where instance.Name == CurrentFile.CurrentInstanceName
                            select instance;

        if (instanceQuery.Count() == 0)
        {
            CurrentInstance = null;
        }
        else
        {
            CurrentInstance = instanceQuery.First();
        }
    }

    private void AddNewInstances(List<InstanceData> instances)
    {
        foreach (InstanceData instance in instances.Skip(1).ExceptBy(CurrentFile.Instances.Select(i => i.GameExePath), j => j.GameExePath))
        {
            RinceDCSInstance newInstance = new() { Name = instance.Name, GameExePath = instance.GameExePath, SavedGamesPath = instance.SavedGamesPath };
            //            LoadBindingDataForInstance(newInstance);
            CurrentFile.Instances.Add(newInstance);
        }
    }

    private void UpdateExistingInstances(List<InstanceData> instances)
    {
        var query = from gameInstance in CurrentFile.Instances
                    join instanceData in instances
                    on gameInstance.GameExePath equals instanceData.GameExePath
                    select new
                    {
                        instance = gameInstance,
                        newName = instanceData.Name,
                        gameExePath = instanceData.GameExePath,
                        SavedGamesPath = instanceData.SavedGamesPath
                    };

        foreach (var updated in query)
        {
            updated.instance.Name = updated.newName;
            if (updated.instance.GameExePath != updated.gameExePath || updated.instance.SavedGamesPath != updated.SavedGamesPath)
            {
                updated.instance.GameExePath = updated.gameExePath;
                updated.instance.SavedGamesPath = updated.SavedGamesPath;
                LoadBindingDataForInstance(updated.instance);
                GroupsVMHelper groupsHelper = new(CurrentFile.Joysticks.ToList(), updated.instance.ControlsData, updated.instance.Groups, updated.instance.SavedGamesPath);
                CurrentInstance.Groups = groupsHelper.UpdatedGroups();
            }
        }
    }

    private void DeleteInstancesNoLongerRequired(List<InstanceData> instances)
    {
        var toDelete = CurrentFile.Instances.ExceptBy(instances.Select(i => i.GameExePath), j => j.GameExePath);
        foreach (RinceDCSInstance instance in toDelete)
        {
            if (instance == CurrentInstance)
            {
                CurrentInstance = null;
                CurrentInstanceDCSData = null;
            }
            CurrentFile.Instances.Remove(instance);
        }
    }

    private void LoadBindingDataForInstance(RinceDCSInstance instance)
    {
        if (instance.ControlsData != null) { return; }

        DCSData data = DCSService.Default.GetControlsData(
            instance.Name,
            instance.GameExePath,
            instance.SavedGamesPath,
            AttachedJoysticks);

        instance.ControlsData = data;

        instance.Aircraft.Clear();
        foreach (var aircraft in data.Aircraft)
        {
            instance.Aircraft.Add(new RinceDCSAircraft(aircraft.Key.Name));
        }
    }
}