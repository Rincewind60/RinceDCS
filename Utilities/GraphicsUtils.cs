using RinceDCS.Models;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;

namespace RinceDCS.Utilities;

public class GraphicsUtils
{
    public static Image CreateJoystickButtonsImage(byte[] imageBytes, ObservableCollection<GameJoystickButton> buttons, string fontName, int fontSiZe)
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
                        RectangleF rect = new((float)(button.TopX+2), (float)(button.TopY+2), (float)(button.Width-4), (float)(button.Height-4));
                        gfx.DrawString(button.ButtonLabel, font, brush, rect);
                    }
                }
            }

            return image;
        }
    }
}
