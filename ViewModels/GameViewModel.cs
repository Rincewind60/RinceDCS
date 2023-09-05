using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RinceDCS.Models;
using RinceDCS.ServiceModels;
using RinceDCS.ViewModels.Helpers;
using RinceDCS.ViewModels.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

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
    private Game currentGame;

    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    private GameInstance currentInstance;

    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    private DCSData currentInstanceBindingsData;

    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    private GameBindingGroups currentInstanceBindingGroups;

    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    private GameAircraft currentAircraft;

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

        string savedPath = Ioc.Default.GetRequiredService<ISettingsService>().GetSetting(RinceDCSSettings.LastSavePath);
        if(!string.IsNullOrWhiteSpace(savedPath))
        {
            DoOpen(savedPath);
        }
        if (CurrentGame == null)
        {
            New();
        }
    }

    [RelayCommand]
    private void New()
    {
        Ioc.Default.GetRequiredService<ISettingsService>().SetSetting(RinceDCSSettings.LastSavePath, null);

        Game newGame = new();
        LoadJoysticks(newGame);

        SetCurrentGame(newGame);
    }

    [RelayCommand]
    private async void Open()
    {
        if(CurrentGame != null)
        {
            bool? result = await Ioc.Default.GetRequiredService<IDialogService>().OpenConfirmationDialog("Save Game", "Do you want to save the existing Game file first?");
            if(result.HasValue && result.Value)
            {
                await Ioc.Default.GetRequiredService<IFileService>().SaveGame(CurrentGame);
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
        Game openedGame = Task.Run(() => Ioc.Default.GetRequiredService<IFileService>().OpenGame(path)).GetAwaiter().GetResult();
        if (openedGame != null)
        {
            CheckForNewJoysticks(openedGame);
            SetCurrentGame(openedGame);
        }
    }

    private void CheckForNewJoysticks(Game openedGame)
    {
        foreach (AttachedJoystick stick in AttachedJoysticks)
        {
            bool existingStick = false;
            foreach (GameJoystick gameStick in openedGame.Joysticks)
            {
                if (stick == gameStick.AttachedJoystick)
                {
                    existingStick = true;
                    break;
                }
            }
            if (existingStick == false)
            {
                GameJoystick newJoystick = new() { AttachedJoystick = stick };

                AddJoystickButtons(newJoystick);

                openedGame.Joysticks.Add(newJoystick);

            }
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

    [RelayCommand]
    private async void ExportImages()
    {
        string exportFolder = await Ioc.Default.GetRequiredService<IDialogService>().OpenPickFolder();
        JoystickVMHelper helper = new(CurrentInstanceBindingsData);
        foreach(GameJoystick stick in CurrentGame.Joysticks)
        {
            Dictionary<GameAssignedButtonKey, GameJoystickButton> buttonsOnLayout = helper.GetJoystickButtonsOnLayout(stick);
            foreach(GameAircraft aircraft in CurrentInstance.Aircraft)
            {
                List<GameAssignedButton> assignedButtons = helper.GetAssignedButtons(stick, buttonsOnLayout, CurrentInstance.Name, aircraft.Name);
                string saveFilePath = exportFolder + "\\" + aircraft.Name + "_" + stick.AttachedJoystick.Name + ".png";
                WeakReferenceMessenger.Default.Send(new ExportAssignedButtonsImageMessage(stick, assignedButtons, saveFilePath));
            }
        }
    }

    [RelayCommand]
    private void ExportKneeboards()
    {
        JoystickVMHelper helper = new(CurrentInstanceBindingsData);
        foreach (GameJoystick stick in CurrentGame.Joysticks)
        {
            Dictionary<GameAssignedButtonKey, GameJoystickButton> buttonsOnLayout = helper.GetJoystickButtonsOnLayout(stick);
            foreach (GameAircraft aircraft in CurrentInstance.Aircraft)
            {
                List<GameAssignedButton> assignedButtons = helper.GetAssignedButtons(stick, buttonsOnLayout, CurrentInstance.Name, aircraft.Name);
                WeakReferenceMessenger.Default.Send(new ExportKneeboardMessage(stick, assignedButtons, aircraft.Name));
            }
        }
    }

    public void CurrentInstanceChanged()
    {
        if(CurrentInstance == null)
        {
            CurrentInstanceBindingsData = null;
            CurrentAircraft = null;
        }
        else
        {
            LoadBindingDataForInstance(CurrentInstance);
            BindingGroupsVMHelper bindHelper = new(CurrentGame.Joysticks.ToList(), CurrentInstance.BindingsData, CurrentInstance.BindingGroups);
            CurrentInstance.BindingGroups = bindHelper.UpdatedGroups();
            CurrentInstanceBindingsData = CurrentInstance.BindingsData;
            CurrentInstanceBindingGroups = CurrentInstance.BindingGroups;
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

    private void LoadJoysticks(Game game)
    {
        foreach (AttachedJoystick stick in AttachedJoysticks)
        {
            GameJoystick newJoystick = new() { AttachedJoystick = stick };

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

    private GameJoystickButton NewJoystickButton(string name, GameJoystick stick, bool isKey = true, bool isModifier = false)
    {
        return new GameJoystickButton()
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
//            LoadBindingDataForInstance(newInstance);
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
                BindingGroupsVMHelper bindHelper = new(CurrentGame.Joysticks.ToList(), updated.instance.BindingsData, updated.instance.BindingGroups);
                CurrentInstance.BindingGroups = bindHelper.UpdatedGroups();
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
            instance.Aircraft.Add(new GameAircraft(aircraft.Key.Name));
        }
    }
}