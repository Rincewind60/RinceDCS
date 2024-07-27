// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RinceDCS.Models;
using RinceDCS.ViewModels;
using System.Collections.Generic;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RinceDCS.Views;

public sealed partial class EditGroupControl : UserControl
{
    public EditGroupControl(RinceDCSGroups groups, List<AttachedJoystick> sticks)
    {
        this.InitializeComponent();

        EditGroupVM vm = new(groups, sticks);

        DataContext = vm;
    }

    public EditGroupVM ViewModel => (EditGroupVM)DataContext;

    private void UpdateGroup_Click(object sender, RoutedEventArgs e)
    {

    }

    private void AddGroup_Click(object sender, RoutedEventArgs e)
    {

    }

    private void DeleteGroup_Click(object sender, RoutedEventArgs e)
    {

    }

    private void MergeGroup_Click(object sender, RoutedEventArgs e)
    {

    }
}
