using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RinceDCS.Models;
using RinceDCS.Services;
using RinceDCS.ViewModels.Messages;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace RinceDCS.ViewModels;

public partial class InstanceData : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNameEditable))]
    private string name;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNameEditable))]
    private string gameExePath;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNameEditable))]
    private string savedGameFolderPath;

    [ObservableProperty]
    private bool isHeading;

    public bool IsNameEditable
    {
        get { return !string.IsNullOrWhiteSpace(GameExePath); }
    }


    public bool IsValid
    {
        get { return !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(GameExePath) && !string.IsNullOrWhiteSpace(SavedGameFolderPath); }
    }
}

public partial class GameInstancesViewModel : ObservableObject
{
    public ObservableCollection<InstanceData> Instances;

    [ObservableProperty]
    public bool isValid;

    private readonly int NumberOfHeadingInstances = 1;

    public GameInstancesViewModel(List<RinceDCSInstance> instances)
    {
        Instances = new();
        Instances.Add(new InstanceData() { Name = "Name", GameExePath = "Game Executable Path", SavedGameFolderPath = "Saved Games Folder Path", IsHeading = true });
        foreach(RinceDCSInstance instance in instances)
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
        ValidateInstances();
    }

    public void Save()
    {
        WeakReferenceMessenger.Default.Send(new GameInstancesUpdatedMessage(Instances.ToList()));
    }

    public void UpdateGameExePath(InstanceData instance, string newPath)
    {
        if (string.IsNullOrEmpty(newPath)) return;

        instance.GameExePath = newPath;

        string gameFolderPath = Path.GetDirectoryName(Path.GetDirectoryName(newPath));

        if (string.IsNullOrEmpty(instance.Name))
        {
            instance.Name = gameFolderPath.Split('\\').Last();
        }

        instance.SavedGameFolderPath = DCSService.Default.GetDCSSavedGamesPath(gameFolderPath, instance.SavedGameFolderPath);

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
        if(Instances.Count == NumberOfHeadingInstances)
        {
            IsValid = false;
            return;
        }

        bool valid = true;

        for(var i = NumberOfHeadingInstances; i < Instances.Count; i++)
        {
            if(Instances[i].IsValid == false) {  valid = false; break; }
        }

        IsValid = valid;
    }
}
