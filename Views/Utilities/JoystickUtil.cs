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
                    Rectangle margins = CalculateImageRectangle(sender, args, printDoc, img);
                    args.Graphics.DrawImage(img, margins);
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
                printDoc.DefaultPageSettings.Margins = new Margins(50, 50, 50, 50);

                printDoc.PrintPage += (sender, args) =>
                {
                    Image img = JoystickUtil.CreateJoystickAssignedButtonsImage(imageBytes, assignedButtons, fontName, fontSize);
                    Rectangle margins = CalculateImageRectangle(sender, args, printDoc, img);

                    args.Graphics.DrawImage(img, margins);
                };
                printDoc.Print();
            }
        }
    }

    private static Rectangle CalculateImageRectangle(object sender, PrintPageEventArgs args, PrintDocument printDoc, Image img)
    {
        Rectangle margins = args.MarginBounds;

        if ((double)img.Width / (double)img.Height > (double)margins.Width / (double)margins.Height) // image is wider
        {
            margins.Height = (int)((double)img.Height / (double)img.Width * (double)margins.Width);
        }
        else
        {
            margins.Width = (int)((double)img.Width / (double)img.Height * (double)margins.Height);
        }
        //Calculating optimal orientation.
        printDoc.DefaultPageSettings.Landscape = margins.Width > margins.Height;
        //Putting image in center of page.
        margins.Y = (int)((((PrintDocument)(sender)).DefaultPageSettings.PaperSize.Height - margins.Height) / 2);
        margins.X = (int)((((PrintDocument)(sender)).DefaultPageSettings.PaperSize.Width - margins.Width) / 2);

        return margins;
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
                        StringFormat format = StringFormat.GenericDefault;
                        format.Trimming = StringTrimming.EllipsisCharacter;
                        RectangleF rect = new((float)(button.TopX + 1), (float)(button.TopY + 1), (float)(button.Width - 2), (float)(button.Height - 2));
                        gfx.DrawString(button.ButtonLabel, font, brush, rect, format);
                    }
                }
            }

            return image;
        }
    }

    private static Image CreateJoystickAssignedButtonsImage(byte[] imageBytes, List<GameAssignedButton> assignedButtons, string fontName, int fontSize)
    {
        using (var stream = new MemoryStream(imageBytes))
        {
            Image image = Image.FromStream(stream, false, false);

            Font font = new(fontName, fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
            SolidBrush brush = new(Color.Black);

            using (Graphics gfx = Graphics.FromImage(image))
            {
                foreach (GameAssignedButton button in assignedButtons)
                {
                    StringFormat format = StringFormat.GenericDefault;
                    format.Trimming = StringTrimming.EllipsisCharacter;
                    if (button.BoundButton.Alignment == "Left")
                    {
                        format.Alignment = StringAlignment.Near;
                    }
                    else if(button.BoundButton.Alignment == "Center")
                    {
                        format.Alignment = StringAlignment.Center;
                    }
                    else if(button.BoundButton.Alignment == "Right")
                    {
                        format.Alignment = StringAlignment.Far;
                    }
                    RectangleF rect = new((float)(button.BoundButton.TopX), (float)(button.BoundButton.TopY), (float)(button.BoundButton.Width), (float)(fontSize+4));
                    gfx.DrawString(button.CommandName, font, brush, rect, format);
                }
            }

            return image;
        }
    }
}
