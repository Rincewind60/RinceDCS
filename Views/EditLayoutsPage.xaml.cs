// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using RinceDCS.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RinceDCS.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditLayoutsPage : Page
    {
        public EditLayoutsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            RinceDCSFile rinceDCSFile = (RinceDCSFile)e.Parameter;

            foreach (RinceDCSJoystick joystick in rinceDCSFile.Joysticks)
            {
                TabViewItem newItem = new TabViewItem();
                newItem.Header = joystick.AttachedJoystick.Name;
                newItem.IsClosable = false;

                EditLayoutsControl ctrl = new(joystick);

                newItem.Content = ctrl;
                EditJoystickLayouts.TabItems.Add(newItem);
            }
        }
    }
}
