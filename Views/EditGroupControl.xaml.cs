using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using RinceDCS.Models;
using RinceDCS.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

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
