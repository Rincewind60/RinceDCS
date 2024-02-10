using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RinceDCS.Models;
using RinceDCS.ServiceModels;
using RinceDCS.ViewModels.Helpers;
using RinceDCS.ViewModels.Messages;
using RinceDCS.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using RinceDCS.Properties;

namespace RinceDCS.ViewModels;

public enum DetailsDisplayMode
{
    None,
    Bindings,
    View,
    Manage,
    EditGroups,
    EditSticks
}

public partial class GameViewModel : ObservableRecipient
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
    [NotifyPropertyChangedRecipients]
    private RinceDCSAircraft currentAircraft;

    [ObservableProperty]
    private List<AttachedJoystick> attachedJoysticks;

    [ObservableProperty]
    private bool isGameLoaded = false;

    [ObservableProperty]
    private DetailsDisplayMode? joystickMode;

    public GameViewModel()
    {
        IsActive = true;

        JoystickMode = DetailsDisplayMode.None;

        AttachedJoysticks = Ioc.Default.GetRequiredService<IJoystickService>().GetAttachedJoysticks();

        WeakReferenceMessenger.Default.Register<GameInstancesUpdatedMessage>(this, (r, m) =>
        {
            DeleteInstancesNoLongerRequired(m.Value);
            UpdateExistingInstances(m.Value);
            AddNewInstances(m.Value);
        });

        string savedPath = Settings.Default.LastSavePath;
        if(!string.IsNullOrWhiteSpace(savedPath))
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

        RinceDCSFile newGame = new();
        LoadJoysticks(newGame);

        SetCurrentGame(newGame);
    }

    [RelayCommand]
    private async void Open()
    {
        if(CurrentFile != null)
        {
            bool? result = await Ioc.Default.GetRequiredService<IDialogService>().OpenConfirmationDialog("Save RinceDCS File", "Do you want to save the existing file first?");
            if(result.HasValue && result.Value)
            {
                await Ioc.Default.GetRequiredService<IFileService>().SaveGame(CurrentFile);
            }
        }

        string path = await Ioc.Default.GetRequiredService<IDialogService>().OpenPickFile(".json");
        if(!string.IsNullOrWhiteSpace(path))
        {
            DoOpen(path);
        }
    }

    private void DoOpen(string path)
    {
        RinceDCSFile openedGame = Task.Run(() => Ioc.Default.GetRequiredService<IFileService>().OpenGame(path)).GetAwaiter().GetResult();
        if (openedGame != null)
        {
            CheckForNewJoysticks(openedGame);
            SetCurrentGame(openedGame);
        }
    }

    private void CheckForNewJoysticks(RinceDCSFile openedGame)
    {
        foreach (AttachedJoystick stick in AttachedJoysticks)
        {
            bool existingStick = false;
            foreach (RinceDCSJoystick gameStick in openedGame.Joysticks)
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

                openedGame.Joysticks.Add(newJoystick);

            }
        }

        openedGame.Joysticks.Sort();
    }

    [RelayCommand]
    private void Save()
    {
        ApplyChangesToModels();
        Task.Run(() => Ioc.Default.GetRequiredService<IFileService>().SaveGame(CurrentFile)).Wait();
    }

     [RelayCommand]
    private void SaveAs()
    {
        ApplyChangesToModels();
        Task.Run(() => Ioc.Default.GetRequiredService<IFileService>().SaveAsGame(CurrentFile)).Wait();
    }

    [RelayCommand]
    private void Exit()
    {

    }

    [RelayCommand]
    private async void ExportImages()
    {
        string exportFolder = await Ioc.Default.GetRequiredService<IDialogService>().OpenPickFolder();
        JoystickVMHelper helper = new(CurrentInstanceDCSData);
        foreach(RinceDCSJoystick stick in CurrentFile.Joysticks)
        {
            Dictionary<AssignedButtonKey, RinceDCSJoystickButton> buttonsOnLayout = helper.GetJoystickButtonsOnLayout(stick);
            foreach(RinceDCSAircraft aircraft in CurrentInstance.Aircraft)
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
    private void UpdateDCS()
    {
        Ioc.Default.GetRequiredService<IDCSService>().UpdateGameBindingData(CurrentInstance.SavedGameFolderPath, CurrentInstanceGroups, CurrentInstanceDCSData);
    }

    public void CurrentInstanceChanged()
    {
        if(CurrentInstance == null)
        {
            CurrentInstanceDCSData = null;
            CurrentAircraft = null;
        }
        else
        {
            LoadBindingDataForInstance(CurrentInstance);
            GroupsVMHelper groupsHelper = new(CurrentFile.Joysticks.ToList(), CurrentInstance.BindingsData, CurrentInstance.BindingGroups);
            CurrentInstance.BindingGroups = groupsHelper.UpdatedGroups();
            CurrentInstanceDCSData = CurrentInstance.BindingsData;
            CurrentInstanceGroups = CurrentInstance.BindingGroups;
            SetCurrentAircraftForCurrentInstance();
        }
    }

    public void CurrentAircraftChanged()
    {
        if(CurrentInstance != null)
        {
            CurrentInstance.CurrentAircraftName = CurrentAircraft != null ? CurrentAircraft.Name : null;
        }
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
        JoystickInfo info = Ioc.Default.GetRequiredService<IJoystickService>().GetJoystickInfo(joystick.AttachedJoystick);

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

        foreach (RinceDCSInstance instance in CurrentFile.Instances)
        {
            if (instance == CurrentInstance)
            {
                instance.CurrentAircraftName = CurrentAircraft == null ? null : CurrentAircraft.Name;
            }
            else
            {
                instance.CurrentAircraftName = null;
            }
        }
    }

    /// <summary>
    /// When a new RinceDCSFile is created the old EinceDCSFile must be replaced.
    /// 
    /// This means updating any ViewModel properties relating to the old RinceDCSFile object.
    /// </summary>
    /// <param name="newGame"></param>
    private void SetCurrentGame(RinceDCSFile newGame)
    {
        IsGameLoaded = false;

        CurrentFile = newGame;

        SetCurrentInstanceForGame();
        SetCurrentAircraftForCurrentInstance();

        IsGameLoaded = true;
    }

    private void SetCurrentInstanceForGame()
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

    private void SetCurrentAircraftForCurrentInstance()
    {
        if(CurrentInstance == null)
        {
            CurrentAircraft = null;
            return;
        }

        var aircraftQuery = from aircraft in CurrentInstance.Aircraft
                            where aircraft.Name == CurrentInstance.CurrentAircraftName
                            select aircraft;

        if (aircraftQuery.Count() == 0)
        {
            CurrentAircraft = null;
        }
        else
        {
            CurrentAircraft = aircraftQuery.First();
        }
    }

    private void AddNewInstances(List<InstanceData> instances)
    {
        foreach (InstanceData instance in instances.Skip(1).ExceptBy(CurrentFile.Instances.Select(i => i.GameExePath), j => j.GameExePath))
        {
            RinceDCSInstance newInstance = new() { Name = instance.Name, GameExePath = instance.GameExePath, SavedGameFolderPath = instance.SavedGameFolderPath };
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
                        savedGameFolderPath = instanceData.SavedGameFolderPath
                    };

        foreach (var updated in query)
        {
            updated.instance.Name = updated.newName;
            if (updated.instance.GameExePath != updated.gameExePath || updated.instance.SavedGameFolderPath != updated.savedGameFolderPath)
            {
                updated.instance.GameExePath = updated.gameExePath;
                updated.instance.SavedGameFolderPath = updated.savedGameFolderPath;
                LoadBindingDataForInstance(updated.instance);
                GroupsVMHelper groupsHelper = new(CurrentFile.Joysticks.ToList(), updated.instance.BindingsData, updated.instance.BindingGroups);
                CurrentInstance.BindingGroups = groupsHelper.UpdatedGroups();
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
                CurrentAircraft = null;
            }
            CurrentFile.Instances.Remove(instance);
        }
    }

    private void LoadBindingDataForInstance(RinceDCSInstance instance)
    {
        if(instance.BindingsData != null) { return; }

        DCSData data = Ioc.Default.GetRequiredService<IDCSService>().GetBindingData(
            instance.Name,
            instance.GameExePath,
            instance.SavedGameFolderPath,
            AttachedJoysticks);

        instance.BindingsData = data;

        instance.Aircraft.Clear();
        foreach (var aircraft in data.Aircraft)
        {
            instance.Aircraft.Add(new RinceDCSAircraft(aircraft.Key.Name));
        }
    }
}