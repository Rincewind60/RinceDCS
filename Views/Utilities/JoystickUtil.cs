// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using Microsoft.UI.Xaml.Media.Imaging;
using RinceDCS.Models;
using RinceDCS.Services;
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
    public static async Task<BitmapImage> GetImageSource(RinceDCSJoystick joystick)
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

    public static async void ExportButtonsImage(byte[] imageBytes, List<RinceDCSJoystickButton> buttons, int height, int width, string fontName, int fontSiZe)
    {
        string savePath = await DialogService.Default.OpenPickSaveFile("JoystickButtons.png", "PNG", ".png");
        if (string.IsNullOrWhiteSpace(savePath)) { return; }

        Image image = CreateJoystickButtonsImage(imageBytes, buttons, height, width, fontName, fontSiZe);
        image.Save(savePath, ImageFormat.Png);
    }

    public static async void PrintButtonsImage(byte[] imageBytes, List<RinceDCSJoystickButton> buttons, int height, int width, string fontName, int fontSiZe)
    {
        using (PrintDocument printDoc = new())
        {
            PrintDialog pp = new(printDoc);
            Microsoft.UI.Xaml.Controls.ContentDialogResult result = await DialogService.Default.OpenResponsePageDialog("Print Joystick", pp, "Print", null, null, "Cancel");

            if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                printDoc.PrinterSettings.PrinterName = pp.ViewModel.Printer;
                printDoc.DefaultPageSettings.Margins = new Margins(25, 25, 25, 25);
                printDoc.PrintPage += (sender, args) =>
                {
                    Image img = CreateJoystickButtonsImage(imageBytes, buttons, height, width, fontName, fontSiZe);
                    Rectangle margins = CalculateImageRectangle(sender, args, printDoc, img);
                    args.Graphics.DrawImage(img, margins);
                };
                printDoc.Print();
            }
        }
    }

    public static void ExportAssignedButtonsImage(byte[] imageBytes, List<AssignedButton> assignedButtons, int height, int width, string fontName, int fontSize, string saveFilePath)
    {
        if (string.IsNullOrWhiteSpace(saveFilePath)) { return; }

        Image image = CreateJoystickAssignedButtonsImage(imageBytes, assignedButtons, height, width, fontName, fontSize);
        image.Save(saveFilePath, ImageFormat.Png);
    }

    public static void ExportKneeboard(byte[] imageBytes, List<AssignedButton> assignedButtons, string aircraftName, string stickDCSName, string savedGamesFolder, int height, int width, string fontName, int fontSize)
    {
        string savePath = savedGamesFolder + "\\Kneeboard\\" + aircraftName + "\\00_" + aircraftName + "__" + stickDCSName + ".png";
        if (string.IsNullOrWhiteSpace(savePath)) { return; }

        Image image = CreateJoystickAssignedButtonsImage(imageBytes, assignedButtons, height, width, fontName, fontSize);
        image.Save(savePath, ImageFormat.Png);
    }

    public static async void PrintAssigedButtonsImage(byte[] imageBytes, List<AssignedButton> assignedButtons, int height, int width, string fontName, int fontSize)
    {
        using (PrintDocument printDoc = new())
        {
            PrintDialog pp = new(printDoc);
            Microsoft.UI.Xaml.Controls.ContentDialogResult result = await DialogService.Default.OpenResponsePageDialog("Print Assigned Buttons", pp, "Print", null, null, "Cancel");

            if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                printDoc.PrinterSettings.PrinterName = pp.ViewModel.Printer;
                printDoc.DefaultPageSettings.Margins = new Margins(25, 25, 25, 25);

                printDoc.PrintPage += (sender, args) =>
                {
                    Image img = JoystickUtil.CreateJoystickAssignedButtonsImage(imageBytes, assignedButtons, height, width,  fontName, fontSize);
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

    private static Image CreateJoystickButtonsImage(byte[] imageBytes, List<RinceDCSJoystickButton> buttons, int height, int width, string fontName, int fontSize)
    {
        using (var stream = new MemoryStream(imageBytes))
        {
            Image image = Image.FromStream(stream, false, false);

            Font font = new(fontName, fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
            SolidBrush blackBrush = new(Color.Black);
            SolidBrush whiteBrush = new(Color.White);
            Pen linePen = new Pen(blackBrush, 4);
            Pen rectPen = new Pen(blackBrush);

            using (Graphics gfx = Graphics.FromImage(image))
            {
                foreach (RinceDCSJoystickButton button in buttons)
                {
                    if (button.OnLayout)
                    {
                        DrawJoystickButton(button, button.ButtonLabel, height, width, font, whiteBrush, blackBrush, linePen, rectPen, gfx);
                    }
                }
            }

            return image;
        }
    }

    private static Image CreateJoystickAssignedButtonsImage(byte[] imageBytes, List<AssignedButton> assignedButtons, int height, int width, string fontName, int fontSize)
    {
        using (var stream = new MemoryStream(imageBytes))
        {
            Image image = Image.FromStream(stream, false, false);

            Font font = new(fontName, fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
            SolidBrush blackBrush = new(Color.Black);
            SolidBrush whiteBrush = new(Color.White);
            Pen linePen = new Pen(blackBrush, 4);
            Pen rectPen = new Pen(blackBrush);

            using (Graphics gfx = Graphics.FromImage(image))
            {
                foreach (AssignedButton button in assignedButtons)
                {
                    DrawJoystickButton(button.JoystickButton, button.Action, height, width, font, whiteBrush, blackBrush, linePen, rectPen, gfx);
                }
            }

            return image;
        }
    }

    private static void DrawJoystickButton(
        RinceDCSJoystickButton button, 
        string label, 
        int height, 
        int width, 
        Font font, 
        SolidBrush whiteBrush, 
        SolidBrush blackBrush, 
        Pen linePen, 
        Pen rectPen, 
        Graphics gfx)
    {
        if (button.DrawLine)
        {
            gfx.DrawLine(linePen, button.LineStartX, button.LineStartY, button.LineEndX, button.LineEndY);
        }

        gfx.FillRectangle(whiteBrush, (int)button.TopX, (int)button.TopY, width, height);
        gfx.DrawRectangle(rectPen, (int)button.TopX, (int)button.TopY, width, height);

        StringFormat format = StringFormat.GenericDefault;
        StringAlignment alignment;
        if (Enum.TryParse(button.Alignment, out alignment))
        {
            format.Alignment = alignment;
        }
        else
        {
            format.Alignment = StringAlignment.Center;
        }
        format.LineAlignment = StringAlignment.Center;
        format.Trimming = StringTrimming.Character;
        RectangleF rect = new((float)(button.TopX + 1), (float)(button.TopY + 1), (float)(width - 2), (float)(height - 2));
        gfx.DrawString(label, font, blackBrush, rect, format);
    }
}
