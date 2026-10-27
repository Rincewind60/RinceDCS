// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using CommunityToolkit.Mvvm.ComponentModel;
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
    public partial string Name { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNameEditable))]
    public partial string GameExePath { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNameEditable))]
    public partial string SavedGamesPath { get; set; }

    [ObservableProperty]
    public partial bool IsHeading { get; set; }

    public bool IsNameEditable
    {
        get { return !string.IsNullOrWhiteSpace(GameExePath); }
    }

    public bool IsValid
    {
        get { return !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(GameExePath) && !string.IsNullOrWhiteSpace(SavedGamesPath); }
    }
}

public partial class GameInstancesVM : ObservableObject
{
    public ObservableCollection<InstanceData> Instances;

    [ObservableProperty]
    public partial bool IsValid { get; set; }

    private readonly int NumberOfHeadingInstances = 1;

    public GameInstancesVM(List<RinceDCSInstance> instances)
    {
        Instances = new();
        Instances.Add(new InstanceData() { Name = "Name", GameExePath = "Game Executable Path", SavedGamesPath = "Saved Games Folder Path", IsHeading = true });
        foreach (RinceDCSInstance instance in instances)
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
        if (Instances.Count == NumberOfHeadingInstances)
        {
            IsValid = false;
            return;
        }

        bool valid = true;

        for (var i = NumberOfHeadingInstances; i < Instances.Count; i++)
        {
            if (Instances[i].IsValid == false) { valid = false; break; }
        }

        IsValid = valid;
    }
}
