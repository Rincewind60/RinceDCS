using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using RinceDCS.Services;
using System;
using System.Drawing;
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
