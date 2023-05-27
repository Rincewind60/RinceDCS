using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using RinceDCS.Models;
using RinceDCS.ServiceModels;
using RinceDCS.ViewModels.Messages;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;

namespace RinceDCS.ViewModels;

public enum DisplayMode
{
    Bindings,
    View,
    Manage,
    Edit
}

public partial class GameViewModel : ObservableRecipient
{
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    private Game currentGame;

    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    private GameInstance currentInstance;

    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    private DCSData currentInstanceBindingsData;

    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    private GameAircraft currentAircraft;

    [ObservableProperty]
    private List<AttachedJoystick> attachedJoysticks;

    [ObservableProperty]
    private bool isGameLoaded = false;

    [ObservableProperty]
    private DisplayMode? joystickMode;

    public GameViewModel()
    {
        IsActive = true;

        JoystickMode = DisplayMode.View;

        AttachedJoysticks = Ioc.Default.GetRequiredService<IJoystickService>().GetAttachedJoysticks();

        WeakReferenceMessenger.Default.Register<GameInstancesUpdatedMessage>(this, (r, m) =>
        {
            DeleteInstancesNoLongerRequired(m.Value);
            UpdateExistingInstances(m.Value);
            AddNewInstances(m.Value);
        });

        Open();
        if(CurrentGame == null)
        {
            New();
        }
    }

    [RelayCommand]
    private void New()
    {
        Ioc.Default.GetRequiredService<ISettingsService>().SetSetting(RinceDCSSettings.LastSavePath, null);

        Game newGame = new Game();
        LoadJoysticks(newGame);

        SetCurrentGame(newGame);
    }

    [RelayCommand]
    private void Open()
    {
        Game openedGame = Task.Run(() => Ioc.Default.GetRequiredService<IFileService>().OpenGame()).GetAwaiter().GetResult();
        if (openedGame != null)
        {
            /// TODO: Check for new attached joysticks and add their buttons
            foreach(GameInstance instance in openedGame.Instances)
            {
                LoadBindingDataForInstance(instance);
            }
            SetCurrentGame(openedGame);
        }
    }

    [RelayCommand]
    private void Save()
    {
        ApplyChangesToModels();
        Task.Run(() => Ioc.Default.GetRequiredService<IFileService>().SaveGame(CurrentGame)).Wait();
    }

     [RelayCommand]
    private void SaveAs()
    {
        ApplyChangesToModels();
        Task.Run(() => Ioc.Default.GetRequiredService<IFileService>().SaveAsGame(CurrentGame)).Wait();
    }

    [RelayCommand]
    private void Exit()
    {

    }

    public void CurrentInstanceChanged()
    {
        CurrentInstanceBindingsData = CurrentInstance == null ? null : CurrentInstance.BindingsData;
        SetCurrentAircraftForCurrentInstance();
    }

    public void CurrentAircraftChanged()
    {
        CurrentInstance.CurrentAircraftName = CurrentAircraft == null ? null : CurrentAircraft.Name;
    }

    partial void OnJoystickModeChanged(DisplayMode? oldValue, DisplayMode? newValue)
    {
        if (newValue == null)
        {
#pragma warning disable MVVMTK0034
            joystickMode = oldValue;
#pragma warning restore MVVMTK0034
        }
    }

    private void LoadJoysticks(Game game)
    {
        foreach (AttachedJoystick stick in AttachedJoysticks)
        {
            GameJoystick newJoystick = new() { AttachedJoystick = stick };
            newJoystick.ImagePath = "D:\\OneDrive\\Documents\\Gaming\\JoyStick.png"; /// TODO: Replace default image path

            AddJoystickButtons(newJoystick);

            game.Joysticks.Add(newJoystick);
        }
    }

    /// <summary>
    /// Remove any existing Joystick info and update with latest
    /// </summary>
    /// <param name="joystick"></param>
    private void AddJoystickButtons(GameJoystick joystick)
    {
        JoystickInfo info = Ioc.Default.GetRequiredService<IJoystickService>().GetJoystickInfo(joystick.AttachedJoystick);

        joystick.Buttons = new ObservableCollection<GameJoystickButton>();

        joystick.Buttons.Add(new GameJoystickButton() { ButtonName = "Game" });
        joystick.Buttons.Add(new GameJoystickButton() { ButtonName = "Plane" });
        joystick.Buttons.Add(new GameJoystickButton() { ButtonName = "Joystick" });
        foreach (string item in info.SupportedAxes) joystick.Buttons.Add(new GameJoystickButton() { ButtonName = item });
        foreach (string item in info.POVs) joystick.Buttons.Add(new GameJoystickButton() { ButtonName = item });
        foreach (string item in info.Buttons) joystick.Buttons.Add(new GameJoystickButton() { ButtonName = item });
        //  Now add copies of Buttons for when using Modifier
        foreach (string item in info.SupportedAxes) joystick.Buttons.Add(new GameJoystickButton() { ButtonName = item, IsModifier = true });
        foreach (string item in info.POVs) joystick.Buttons.Add(new GameJoystickButton() { ButtonName = item, IsModifier = true });
        foreach (string item in info.Buttons) joystick.Buttons.Add(new GameJoystickButton() { ButtonName = item, IsModifier = true });
    }

    private void ApplyChangesToModels()
    {
        if (CurrentInstance == null)
        {
            CurrentGame.CurrentInstanceName = null;
        }
        else
        {
            CurrentGame.CurrentInstanceName = CurrentInstance.Name;
        }

        foreach (GameInstance instance in CurrentGame.Instances)
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
    /// When a new game is created the old game must be replaced.
    /// 
    /// This means updating any ViewModel properties relating to the old Game object.
    /// </summary>
    /// <param name="newGame"></param>
    private void SetCurrentGame(Game newGame)
    {
        IsGameLoaded = false;

        CurrentGame = newGame;

        foreach (GameInstance instance in CurrentGame.Instances)
        {
            LoadBindingDataForInstance(instance);
        }

        SetCurrentInstanceForGame();

        SetCurrentAircraftForCurrentInstance();

        IsGameLoaded = true;
    }

    private void SetCurrentInstanceForGame()
    {
        var instanceQuery = from instance in CurrentGame.Instances
                            where instance.Name == CurrentGame.CurrentInstanceName
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
        foreach (InstanceData instance in instances.Skip(1).ExceptBy(CurrentGame.Instances.Select(i => i.GameExePath), j => j.GameExePath))
        {
            GameInstance newInstance = new() { Name = instance.Name, GameExePath = instance.GameExePath, SavedGameFolderPath = instance.SavedGameFolderPath };
            LoadBindingDataForInstance(newInstance);
            CurrentGame.Instances.Add(newInstance);
        }
    }
     
    private void UpdateExistingInstances(List<InstanceData> instances)
    {
        var query = from gameInstance in CurrentGame.Instances
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
            }
        }
    }

    private void DeleteInstancesNoLongerRequired(List<InstanceData> instances)
    {
        var toDelete = CurrentGame.Instances.ExceptBy(instances.Select(i => i.GameExePath), j => j.GameExePath);
        foreach (GameInstance instance in toDelete)
        {
            if (instance == CurrentInstance)
            {
                CurrentInstance = null;
                CurrentInstanceBindingsData = null;
                CurrentAircraft = null;
            }
            CurrentGame.Instances.Remove(instance);
        }
    }

    private void LoadBindingDataForInstance(GameInstance instance)
    {
        DCSData data = Ioc.Default.GetRequiredService<IDCSService>().GetBindingData(
            instance.Name,
            instance.GameExePath,
            instance.SavedGameFolderPath,
            AttachedJoysticks);

        instance.BindingsData = data;

        instance.Aircraft.Clear();
        foreach (var aircraft in data.Aircraft)
        {
            instance.Aircraft.Add(new GameAircraft() { Name = aircraft.Key.Name });
        }
    }
}