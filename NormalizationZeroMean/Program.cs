using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace NormalizationZeroMean
{
    class Program
    {
        static void Main(string[] args)
        {
            //Define file paths
            string sourcePath = @"App_Data\source.jpg";
            string targetPath = @"App_Data\target.jpg";
            string targetConvertedPath = @"App_Data\targetConverted.jpg";

            //Load source image, convert to int[,] of greyscale values (0-255)
            Bitmap image = (Bitmap)Bitmap.FromFile(sourcePath);
            int[,] greyscaleValues = new int[image.Width, image.Height];
            for (int x = 0; x < greyscaleValues.GetLength(0); x++)
            {
                for (int y = 0; y < greyscaleValues.GetLength(1); y++)
                {
                    Color cl = image.GetPixel(x, y);
                    //https://stackoverflow.com/q/38464912/2725902
                    int greyValue = (int)((cl.R * 0.3) + (cl.G * 0.59) + (cl.B * 0.11));
                    greyscaleValues[x, y] = greyValue;
                }
            }

            //Produce target bitmap file (greyscale)
            var targetFile = new Bitmap(greyscaleValues.GetLength(0), greyscaleValues.GetLength(1));
            for (int x = 0; x < greyscaleValues.GetLength(0); x++)
            {
                for (int y = 0; y < greyscaleValues.GetLength(1); y++)
                {
                    Color grey = Color.FromArgb(255, greyscaleValues[x, y], greyscaleValues[x, y], greyscaleValues[x, y]);
                    targetFile.SetPixel(x, y, grey);
                }
            }

            //Store target bitmap file
            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }
            targetFile.Save(targetPath, ImageFormat.Jpeg);


            //Determine amount of pixels and the sum of their values
            double pixelAmount = greyscaleValues.GetLength(0) * greyscaleValues.GetLength(1);
            double pixelSum = 0;
            for (int x = 0; x < greyscaleValues.GetLength(0); x++)
            {
                for (int y = 0; y < greyscaleValues.GetLength(1); y++)
                {
                    pixelSum += greyscaleValues[x, y];
                }
            }

            double mu = (1.0 / pixelAmount) * pixelSum;

            //Determine squared sum
            double squaredSum = 0;
            for (int x = 0; x < greyscaleValues.GetLength(0); x++)
            {
                for (int y = 0; y < greyscaleValues.GetLength(1); y++)
                {
                    squaredSum += Math.Pow(greyscaleValues[x, y] - mu, 2);
                }
            }
            double sigmaSquared = (1.0 / pixelAmount) * squaredSum;


            //Produce target bitmap file (greyscale)
            var targetFileConverted = new Bitmap(greyscaleValues.GetLength(0), greyscaleValues.GetLength(1));
            for (int x = 0; x < greyscaleValues.GetLength(0); x++)
            {
                for (int y = 0; y < greyscaleValues.GetLength(1); y++)
                {
                    double normalizedGreyscaleValue = (greyscaleValues[x, y] - mu) / sigmaSquared;
                    Color grey = Color.FromArgb(255, (int)normalizedGreyscaleValue, (int)normalizedGreyscaleValue, (int)normalizedGreyscaleValue);
                    targetFileConverted.SetPixel(x, y, grey);
                }
            }

            //Store converted target bitmap file
            if (File.Exists(targetConvertedPath))
            {
                File.Delete(targetConvertedPath);
            }
            targetFile.Save(targetConvertedPath, ImageFormat.Jpeg);
        }
    }
}
