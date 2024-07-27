// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.IO;

namespace RinceDCS.ViewModels;

public partial class HelpVM : ObservableRecipient
{
    [ObservableProperty]
    private string markDownText;

    public HelpVM()
    {
        LinkToPage("Introduction.md");
    }

    public void LinkToPage(string fileName)
    {
        string path = GetAbsolutePath(fileName);
        MarkDownText = File.ReadAllText(path);
    }

    public string GetAbsolutePath(string fileName)
    {
        return AppDomain.CurrentDomain.BaseDirectory + "\\Help\\" + fileName;
    }
}
