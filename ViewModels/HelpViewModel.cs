﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using RinceDCS.ServiceModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        string path = Windows.ApplicationModel.Package.Current.InstalledPath + "\\Help\\" + fileName;
        string tocPath = Windows.ApplicationModel.Package.Current.InstalledPath + "\\Help\\TOC.md";
        MarkDownText = File.ReadAllText(path) + File.ReadAllText(tocPath);
    }
}