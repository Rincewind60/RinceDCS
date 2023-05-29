using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;

namespace RinceDCS.Views.Utilities;

public class TextBoxHandler
{
    /// <summary>
    /// Call this on Enter key down to save text box text to bind and move focus to next focus element.
    /// 
    /// Usage:
    ///  - XAML: Set an event handler for on KeyDown.
    ///  - C#: Call this function from your code behind event handler.
    ///  
    /// Restrictions:
    ///  - Text property of textBox must be bound using "Binding", method needs to be able to create
    ///    a Binding Expression which cannot be done when using x:Bind as that is resolved at compile time.
    ///     
    /// Note:
    ///  - FocusManager can throw errors if not used properly as it has limitations.
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public static void EnterKey_SaveBindAndUnfocus(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            TextBox tBox = (TextBox)sender;
            DependencyProperty prop = TextBox.TextProperty;

            //  XAML must use Binding, not x:Bind
            BindingExpression binding = tBox.GetBindingExpression(prop);
            if (binding != null) { binding.UpdateSource(); }
            FindNextElementOptions fneo = new() { SearchRoot = tBox.XamlRoot.Content };
            FocusManager.TryMoveFocus(FocusNavigationDirection.Next, fneo);
            e.Handled = true;
        }
    }
}
