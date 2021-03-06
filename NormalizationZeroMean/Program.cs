﻿using System;
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
            string targetNormalizedPath = @"App_Data\targetNormalized.jpg";

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
            double mu = CalculateMean(greyscaleValues);

            //Determine squared sum
            double sigmaSquared = CalculateDeviation(greyscaleValues, mu);

            Console.WriteLine($"Source mu={mu}, sigma²={sigmaSquared}");

            //Calculate transformed values
            double[,] transformedValues = new double[image.Width, image.Height];

            //Save min, max for future calculations
            double min = double.MaxValue;
            double max = double.MinValue;
            for (int x = 0; x < transformedValues.GetLength(0); x++)
            {
                for (int y = 0; y < transformedValues.GetLength(1); y++)
                {
                    double normalizedGreyscaleValue = (greyscaleValues[x, y] - mu) / Math.Sqrt(sigmaSquared);
                    min = Math.Min(normalizedGreyscaleValue, min);
                    max = Math.Max(normalizedGreyscaleValue, max);
                    transformedValues[x, y] = normalizedGreyscaleValue;
                }
            }

            //Sanity check
            double muNormalized = CalculateMean(transformedValues);
            double sigmaSquaredNormalized = CalculateDeviation(transformedValues, muNormalized);
            Console.WriteLine($"Normalized mu={Math.Round(muNormalized,2)}, sigma²={Math.Round(sigmaSquaredNormalized,2)}");


            //Produce target bitmap file (greyscale)
            var targetFileNormalized = new Bitmap(greyscaleValues.GetLength(0), greyscaleValues.GetLength(1));
            for (int x = 0; x < greyscaleValues.GetLength(0); x++)
            {
                for (int y = 0; y < greyscaleValues.GetLength(1); y++)
                {
                    //Transform each real-valued brightness value onto an interval of 0-255
                    int normalizedTranslatedGreyscaleValue = (int)(255*(transformedValues[x, y]-min)/(max-min));
                    Color grey = Color.FromArgb(255, normalizedTranslatedGreyscaleValue, normalizedTranslatedGreyscaleValue, normalizedTranslatedGreyscaleValue);
                    targetFileNormalized.SetPixel(x, y, grey);
                }
            }

            //Store normalized target bitmap file
            if (File.Exists(targetNormalizedPath))
            {
                File.Delete(targetNormalizedPath);
            }
            targetFileNormalized.Save(targetNormalizedPath, ImageFormat.Jpeg);
        }

        private static double CalculateMean(int[,] values)
        {
            double pixelAmount = values.GetLength(0) * values.GetLength(1);
            double pixelSum = 0;
            for (int x = 0; x < values.GetLength(0); x++)
            {
                for (int y = 0; y < values.GetLength(1); y++)
                {
                    pixelSum += values[x, y];
                }
            }

            return (1.0 / pixelAmount) * pixelSum;
        }

        private static double CalculateMean(double[,] values)
        {
            double pixelAmount = values.GetLength(0) * values.GetLength(1);
            double pixelSum = 0;
            for (int x = 0; x < values.GetLength(0); x++)
            {
                for (int y = 0; y < values.GetLength(1); y++)
                {
                    pixelSum += values[x, y];
                }
            }

            return (1.0 / pixelAmount) * pixelSum;
        }

        private static double CalculateDeviation(double[,] values, double mean)
        {
            double squaredSum = 0;
            double pixelAmount = values.GetLength(0) * values.GetLength(1);
            for (int x = 0; x < values.GetLength(0); x++)
            {
                for (int y = 0; y < values.GetLength(1); y++)
                {
                    squaredSum += Math.Pow(values[x, y] - mean, 2);
                }
            }
            return (1.0 / pixelAmount) * squaredSum;
        }

        private static double CalculateDeviation(int[,] values, double mean)
        {
            double squaredSum = 0;
            double pixelAmount = values.GetLength(0) * values.GetLength(1);
            for (int x = 0; x < values.GetLength(0); x++)
            {
                for (int y = 0; y < values.GetLength(1); y++)
                {
                    squaredSum += Math.Pow(values[x, y] - mean, 2);
                }
            }
            return (1.0 / pixelAmount) * squaredSum;
        }
    }
}
