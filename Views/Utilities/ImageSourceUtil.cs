using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using RinceDCS.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace RinceDCS.Views.Utilities;

public class ImageSourceUtil
{
    public static async Task SetSourceFromGameJoystick(Image image, GameJoystick joystick)
    {
        if (joystick == null)
        {
            image.Source = new BitmapImage(new Uri("ms-appx:///Assets/DefaultJoystickImage.png"));
        }
        else
        {
            using (MemoryStream stream = new(joystick.Image))
            {
                using (IRandomAccessStream random = stream.AsRandomAccessStream())
                {
                    BitmapImage bitmap = new();
                    await bitmap.SetSourceAsync(random);
                    image.Source = bitmap;
                }
            }
        }
    }
}
