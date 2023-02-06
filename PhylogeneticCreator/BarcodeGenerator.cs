using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;
using ZXing.Rendering;

[SupportedOSPlatform("windows")]
public class BarcodeGenerator
{
    private int CharacterWidth { get; set; }
    private int Height { get; set; }
    private Color InverseColor { get; set; }
    private Color BackgroundColor { get; set; }

    // Constructor
    public BarcodeGenerator()
    {
        CharacterWidth = 5;
        Height = 100;
        InverseColor = Color.Black;
        BackgroundColor = Color.White;
    }
    public BarcodeGenerator(int characterWidth, int height, Color inverseColor, Color backgroundColor)
    {
        CharacterWidth = characterWidth;
        Height = height;
        InverseColor = inverseColor;
        BackgroundColor = backgroundColor;
    }

    public void CreateBarcode(string oGtext, char[] characterToInvertColor, string filePath)
    {
        var image = new Bitmap(oGtext.Length * CharacterWidth, Height, PixelFormat.Format32bppRgb);
        using (var g = Graphics.FromImage(image))
        {
            g.Clear(Color.White);
            for (int x = 0; x < image.Width; x++)
            {
                // We have to get the character reprented in this pixel
                // the width is CharacterWidth, so we can get the character index
                // by dividing the x coordinate by the CharacterWidth
                var character = oGtext[x / CharacterWidth];

                // We paint all the height of the image with black
                // if the character is in the array of characters to invert
                if (characterToInvertColor.Contains(character))
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        image.SetPixel(x, y, InverseColor);
                    }
                }
                // If the character is not the one we want to invert
                // we paint the pixel with the original background color
                else
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        image.SetPixel(x, y, BackgroundColor);
                    }
                }
            }
        }
        // Save the image to the specified file path
        image.Save(filePath);
    }
}