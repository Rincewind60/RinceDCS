using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RinceDCS.Models;
using RinceDCS.ServiceModels;
using RinceDCS.ViewModels.Messages;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Data.Text;
using Windows.Devices.Geolocation;
using Windows.System;

namespace RinceDCS.ViewModels;

public partial class InstanceData : ObservableObject
{
    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string gameExePath;

    [ObservableProperty]
    private string savedGameFolderPath;

    [ObservableProperty]
    private bool isHeading;

    public bool IsValid
    {
        get
        {
            return !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(GameExePath) && !string.IsNullOrWhiteSpace(SavedGameFolderPath);
        }
    }
}

public partial class GameInstancesViewModel : ObservableObject
{
     public ObservableCollection<InstanceData> Instances;

    [ObservableProperty]
    public bool isValid;

//    private GameViewModel GameVM { get; set; }

    public GameInstancesViewModel(List<GameInstance> instances)
    {
        Instances = new();
        Instances.Add(new InstanceData() { Name = "Name", GameExePath = "Game Executable Path", SavedGameFolderPath = "Saved Games Folder Path", IsHeading = true });
        foreach(GameInstance instance in instances)
        {
            InstanceData instanceData = new InstanceData() { Name = instance.Name, GameExePath = instance.GameExePath, SavedGameFolderPath = instance.SavedGameFolderPath, IsHeading = false };
            Instances.Add(instanceData);
        }

        ValidateInstances();
    }

    [RelayCommand]
    public void AddInstance()
    {
        Instances.Add(new InstanceData() { Name = "", GameExePath = "", SavedGameFolderPath = "", IsHeading = false });
    }

    public void Save()
    {
        WeakReferenceMessenger.Default.Send(new GameInstancesUpdatedMessage(Instances.ToList()));
    }

    public void UpdateGameExePath(InstanceData instance, string newPath)
    {
        if (string.IsNullOrEmpty(newPath)) return;

        instance.GameExePath = newPath;

        if(string.IsNullOrEmpty(instance.Name))
        {
            instance.Name = Path.GetDirectoryName(Path.GetDirectoryName(newPath)).Split('\\').Last();
        }

        ValidateInstances();
    }

    public void UpdateSavedGameFolderPathh(InstanceData instance, string newPath)
    {
        if (string.IsNullOrEmpty(newPath)) return;

        instance.SavedGameFolderPath = newPath;

        ValidateInstances();
    }

    public void DeleteInstance(InstanceData instance)
    {
        Instances.Remove(instance);

        ValidateInstances();
    }

    private void ValidateInstances()
    {
        if(Instances.Count == 0)
        {
            IsValid = false;
            return;
        }

        bool valid = true;

        for(var i = 1; i < Instances.Count; i++)
        {
            InstanceData instance = Instances[i];
            if (string.IsNullOrEmpty(instance.Name) || string.IsNullOrEmpty(instance.GameExePath) || string.IsNullOrEmpty(instance.SavedGameFolderPath))
            {
                valid = false; 
                break;
            }
        }

        IsValid = valid;
    }
}
