using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using RinceDCS.ServiceModels;
using System;
using System.Drawing;
using System.IO;

namespace RinceDCS.ViewModels;

public partial class HelpViewModel : ObservableRecipient
{
    [ObservableProperty]
    private string markDownText;

    public HelpViewModel()
    {
        LinkToPage("Introduction.md");
    }

    public void LinkToPage(string fileName)
    {
        string path = GetAbsolutePath(fileName);
        string tocPath = GetAbsolutePath("TOC.md");
        MarkDownText = File.ReadAllText(path) + File.ReadAllText(tocPath);
    }

    public string GetAbsolutePath(string fileName)
    {
        return AppDomain.CurrentDomain.BaseDirectory + "\\Help\\" + fileName;
    }
}
