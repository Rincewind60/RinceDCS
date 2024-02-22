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
    private string savedGamesPath;

    [ObservableProperty]
    private bool isHeading;

    public bool IsNameEditable
    {
        get { return !string.IsNullOrWhiteSpace(GameExePath); }
    }


    public bool IsValid
    {
        get { return !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(GameExePath) && !string.IsNullOrWhiteSpace(SavedGamesPath); }
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
        Instances.Add(new InstanceData() { Name = "Name", GameExePath = "Game Executable Path", SavedGamesPath = "Saved Games Folder Path", IsHeading = true });
        foreach(RinceDCSInstance instance in instances)
        {
            InstanceData instanceData = new InstanceData() { Name = instance.Name, GameExePath = instance.GameExePath, SavedGamesPath = instance.SavedGamesPath, IsHeading = false };
            Instances.Add(instanceData);
        }

        ValidateInstances();
    }

    [RelayCommand]
    public void AddInstance()
    {
        Instances.Add(new InstanceData() { Name = "", GameExePath = "", SavedGamesPath = "", IsHeading = false });
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

        instance.SavedGamesPath = DCSService.Default.GetDCSSavedGamesPath(gameFolderPath, instance.SavedGamesPath);

        ValidateInstances();
    }

    public void UpdateSavedGamesPathh(InstanceData instance, string newPath)
    {
        if (string.IsNullOrEmpty(newPath)) return;

        instance.SavedGamesPath = newPath;

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
