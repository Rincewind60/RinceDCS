using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Media.Imaging;
using RinceDCS.Models;
using RinceDCS.ServiceModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace RinceDCS.Views.Utilities;

public class JoystickUtil
{
    public static async Task<BitmapImage> GetImageSource(GameJoystick joystick)
    {
        if (joystick == null || joystick.Image == null)
        {
            return new BitmapImage(new Uri("ms-appx:///Assets/DefaultJoystickImage.png"));
        }

        using (MemoryStream stream = new(joystick.Image))
        {
            using (IRandomAccessStream random = stream.AsRandomAccessStream())
            {
                BitmapImage bitmap = new();
                await bitmap.SetSourceAsync(random);
                return bitmap;
            }
        }
    }

    public static async void ExportButtonsImage(byte[] imageBytes, List<GameJoystickButton> buttons, string fontName, int fontSiZe)
    {
        string savePath = await Ioc.Default.GetRequiredService<IDialogService>().OpenPickSaveFile("JoystickButtons.png", "PNG", ".png");
        if (string.IsNullOrWhiteSpace(savePath)) { return; }

        Image image = CreateJoystickButtonsImage(imageBytes, buttons, fontName, fontSiZe);
        image.Save(savePath, ImageFormat.Png);
    }

    public static async void PrintButtonsImage(byte[] imageBytes, List<GameJoystickButton> buttons, string fontName, int fontSiZe)
    {
        using (PrintDocument printDoc = new())
        {
            PrintPage pp = new(printDoc);
            Microsoft.UI.Xaml.Controls.ContentDialogResult result = await Ioc.Default.GetRequiredService<IDialogService>().OpenResponsePageDialog("Print Joystick", pp, "Print", null, null, "Cancel");

            if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                printDoc.PrinterSettings.PrinterName = pp.ViewModel.Printer;
                printDoc.PrintPage += (sender, args) =>
                {
                    Image img = CreateJoystickButtonsImage(imageBytes, buttons, fontName, fontSiZe);
                    PaperSize size = printDoc.DefaultPageSettings.PaperSize;
                    Rectangle rect = new(20, 20, size.Width - 40, size.Height - 40);
                    args.Graphics.DrawImage(img, rect);
                };
                printDoc.Print();
            }
        }
    }

    public static void ExportAssignedButtonsImage(byte[] imageBytes, List<GameAssignedButton> assignedButtons, string fontName, int fontSize, string saveFilePath)
    {
        if (string.IsNullOrWhiteSpace(saveFilePath)) { return; }

        Image image = CreateJoystickAssignedButtonsImage(imageBytes, assignedButtons, fontName, fontSize);
        image.Save(saveFilePath, ImageFormat.Png);
    }

    public static void ExportKneeboard(byte[] imageBytes, List<GameAssignedButton> assignedButtons, string aircraftName, string stickDCSName, string savedGamesFolder, string fontName, int fontSize)
    {
        string savePath = savedGamesFolder + "\\Kneeboard\\" + aircraftName + "\\00_" + aircraftName + "__" + stickDCSName + ".png";
        if (string.IsNullOrWhiteSpace(savePath)) { return; }

        Image image = CreateJoystickAssignedButtonsImage(imageBytes, assignedButtons, fontName, fontSize);
        image.Save(savePath, ImageFormat.Png);
    }

    public static async void PrintAssigedButtonsImage(byte[] imageBytes, List<GameAssignedButton> assignedButtons, string fontName, int fontSize)
    {
        using (PrintDocument printDoc = new())
        {
            PrintPage pp = new(printDoc);
            Microsoft.UI.Xaml.Controls.ContentDialogResult result = await Ioc.Default.GetRequiredService<IDialogService>().OpenResponsePageDialog("Print Assigned Buttons", pp, "Print", null, null, "Cancel");

            if(result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                printDoc.PrinterSettings.PrinterName = pp.ViewModel.Printer;
                printDoc.PrintPage += (sender, args) =>
                {
                    Image img = JoystickUtil.CreateJoystickAssignedButtonsImage(imageBytes, assignedButtons, fontName, fontSize);
                    PaperSize size = printDoc.DefaultPageSettings.PaperSize;
                    Rectangle rect = new(20, 20, size.Width-40, size.Height-40);
                    args.Graphics.DrawImage(img, rect);
                };
                printDoc.Print();
            }
        }
    }

    private static Image CreateJoystickButtonsImage(byte[] imageBytes, List<GameJoystickButton> buttons, string fontName, int fontSiZe)
    {
        using (var stream = new MemoryStream(imageBytes))
        {
            Image image = Image.FromStream(stream, false, false);

            Font font = new(fontName, fontSiZe, FontStyle.Regular, GraphicsUnit.Pixel);
            SolidBrush brush = new(Color.Black);
            Pen pen = new Pen(brush);

            using (Graphics gfx = Graphics.FromImage(image))
            {
                foreach (GameJoystickButton button in buttons)
                {
                    if (button.OnLayout)
                    {
                        gfx.DrawRectangle(pen, (int)button.TopX, (int)button.TopY, (int)button.Width, (int)button.Height);
                        RectangleF rect = new((float)(button.TopX + 2), (float)(button.TopY + 2), (float)(button.Width - 4), (float)(button.Height - 4));
                        gfx.DrawString(button.ButtonLabel, font, brush, rect);
                    }
                }
            }

            return image;
        }
    }

    private static Image CreateJoystickAssignedButtonsImage(byte[] imageBytes, List<GameAssignedButton> assignedButtons, string fontName, int fontSiZe)
    {
        using (var stream = new MemoryStream(imageBytes))
        {
            Image image = Image.FromStream(stream, false, false);

            Font font = new(fontName, fontSiZe, FontStyle.Regular, GraphicsUnit.Pixel);
            SolidBrush brush = new(Color.Black);

            using (Graphics gfx = Graphics.FromImage(image))
            {
                foreach (GameAssignedButton button in assignedButtons)
                {
                    RectangleF rect = new((float)(button.BoundButton.TopX + 2), (float)(button.BoundButton.TopY + 2), (float)(button.BoundButton.Width - 4), (float)(button.BoundButton.Height - 4));
                    gfx.DrawString(button.CommandName, font, brush, rect);
                }
            }

            return image;
        }
    }
}
