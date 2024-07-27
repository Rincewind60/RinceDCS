// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using Microsoft.UI.Xaml.Controls;
using RinceDCS.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RinceDCS.Views
{
    public sealed partial class GroupsToolbar : UserControl
    {
        public GroupsToolbar()
        {
            this.InitializeComponent();
            DataContext = new GroupsToolbarVM();
        }

        public GroupsToolbarVM ViewModel => (GroupsToolbarVM)DataContext;
    }
}
