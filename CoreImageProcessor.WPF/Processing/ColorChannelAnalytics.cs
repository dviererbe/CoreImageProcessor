using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

using static CoreImageProcessor.Processing.ParallelizationUtils;

namespace CoreImageProcessor.Processing
{
    internal partial class ImageAnalytics
    {
        public class ColorChannelAnalytics
        {
            private ColorChannelAnalytics(ColorChannel colorChannel, uint colorCount)
            {
                Type = colorChannel;
                
                Sum = 0;
                Min = uint.MaxValue;
                Max = uint.MinValue;
                Median = uint.MinValue;
                Mean = 0D;
                Variance = 0D;
                StandardDeviation = 0D;
                Entropy = 0D;
                
                PixelCountPerColor = new int[colorCount];
                Histogram = new double[colorCount];
            }

            /// <summary>
            /// 
            /// </summary>
            public ColorChannel Type { get; private set; }

            /// <summary>
            /// 
            /// </summary>
            public long Sum { get; private set; }

            /// <summary>
            /// 
            /// </summary>
            public uint Min { get; private set; }

            /// <summary>
            /// 
            /// </summary>
            public uint Max { get; private set; }

            /// <summary>
            /// 
            /// </summary>
            public double Mean { get; private set; }

            /// <summary>
            /// 
            /// </summary>
            public uint Median { get; private set; }

            /// <summary>
            /// 
            /// </summary>
            public double Variance { get; private set; }

            /// <summary>
            /// /
            /// </summary>
            public double StandardDeviation { get; private set; }

            /// <summary>
            /// 
            /// </summary>
            public double Entropy { get; private set; }

            /// <summary>
            /// 
            /// </summary>
            public int[] PixelCountPerColor { get; private set; }

            /// <summary>
            /// 
            /// </summary>
            public double[] Histogram { get; private set; }

            public static async Task AnalyzeColorChannelsAsync(ImageAnalytics image, CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (image.PixelFormat == PixelFormats.Rgb24)
                {   
                    image[ColorChannel.Red] = await AnalyzeColorChannelAsync(image, ColorChannel.Red, cancellationToken); 
                    image[ColorChannel.Green] = await AnalyzeColorChannelAsync(image, ColorChannel.Green, cancellationToken);
                    image[ColorChannel.Blue] = await AnalyzeColorChannelAsync(image, ColorChannel.Blue, cancellationToken);
                }
                else if (image.PixelFormat == PixelFormats.Gray8)
                {
                    image[ColorChannel.Gray] = await AnalyzeColorChannelAsync(image, ColorChannel.Gray, cancellationToken);
                }
            }

            public static async Task<ColorChannelAnalytics> AnalyzeColorChannelAsync(ImageAnalytics image, ColorChannel colorChannel, CancellationToken cancellationToken = default)
            {
                static async Task AnalyzeAsync(ColorChannelAnalytics analytics, byte[] pixelData, int offset, int bytesPerPixel, int pixelCount, CancellationToken cancellationToken = default)
                {
                    #region Local Functions

                    //helper method to square a double value for the variance calculations
                    static double Square(double x) => x * x;
                    
                    static void TaskKernel(ColorChannelAnalytics analytics, byte[] pixelData, int startIndex, int endIndex, int offset, int bytesPerPixel, CancellationToken cancellationToken = default)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        
                        long sum = 0;
                        int[] pixelCountPerColor = new int[256];

                        byte value;
                        
                        for (int i = startIndex; i < endIndex ; i += bytesPerPixel)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            
                            value = pixelData[i + offset];

                            sum += value;
                            ++pixelCountPerColor[value];
                        }
                        
                        lock (analytics)
                        {
                            analytics.Sum += sum;

                            for (int i = 0; i < pixelCountPerColor.Length; ++i)
                            {
                                analytics.PixelCountPerColor[i] += pixelCountPerColor[i];
                            }
                        }
                    }

                    #endregion

                    cancellationToken.ThrowIfCancellationRequested();
                    
                    #region Calculating Sum & PixelCount/Color via Multitasking

                    var fragments = ParallelizationUtils.FragmentLength(length: pixelCount, maxDegreeOfFragmentation: App.Settings.ThreadLimit);
                    var tasks = new ConfiguredTaskAwaitable[fragments.Length];

                    for (int i = 0; i < tasks.Length; ++i)
                    {
                        int iCopy = i;
                        tasks[i] = Task.Run(() => TaskKernel(analytics, pixelData, fragments[iCopy].StartIndex * bytesPerPixel, fragments[iCopy].EndIndex * bytesPerPixel, offset, bytesPerPixel, cancellationToken)).ConfigureAwait(false);
                    }

                    foreach (var task in tasks)
                        await task;

                    #endregion

                    cancellationToken.ThrowIfCancellationRequested();
                    
                    #region Calculating Variance, StandardDeviation, Entropy, Min & Max

                    //save the pixel count as double to avoid casting it to a double 256 more times
                    double pixelCountAsDouble = pixelCount;
                    analytics.Mean = analytics.Sum / pixelCountAsDouble;

                    //Variance = ∑((pixels[i] - mean)^2) / (n - 1)
                    //StandardDeviation = √(Variance)
                    //Entropy = - ∑(Histogram[i] * log2(Histogram[i]))
                    for (int i = 0; i < analytics.Histogram.Length; ++i)
                    {
                        //calculate the relative frequency for each color 
                        analytics.Histogram[i] = analytics.PixelCountPerColor[i] / pixelCountAsDouble;
                        analytics.Variance += Square(i - analytics.Mean) * analytics.PixelCountPerColor[i];

                        if (analytics.PixelCountPerColor[i] > 0)
                        {
                            analytics.Entropy += analytics.Histogram[i] * Math.Log(analytics.Histogram[i], 2d);

                            if (i < analytics.Min)
                                analytics.Min = (byte) i;

                            if (i > analytics.Max)
                                analytics.Max = (byte) i;
                        }
                    }

                    analytics.Entropy = Math.Abs(analytics.Entropy);
                    analytics.Variance /= (pixelCountAsDouble - 1d);
                    analytics.StandardDeviation = Math.Sqrt(analytics.Variance);

                    #endregion

                    cancellationToken.ThrowIfCancellationRequested();
                    
                    #region Calculating Median

                    analytics.Median = 0;
                    int position = 0;
                    int half = (pixelCount + 1) / 2;

                    while (true)
                    {
                        position += analytics.PixelCountPerColor[analytics.Median];

                        if (half <= position)
                            break;

                        ++analytics.Median;
                    }

                    #endregion
                }
                
                cancellationToken.ThrowIfCancellationRequested();

                int offset;
                int bytesPerPixel;
                
                if (image.PixelFormat == PixelFormats.Rgb24)
                {
                    bytesPerPixel = 3;
                    
                    if (colorChannel == ColorChannel.Red)
                        offset = 0;
                    else if (colorChannel == ColorChannel.Green)
                        offset = 1;
                    else if (colorChannel == ColorChannel.Blue)
                        offset = 2;
                    else
                        throw new InvalidOperationException(
                            $"Pixel format {image.PixelFormat.ToString()} has no color channel '{colorChannel}'.");
                }
                else if (image.PixelFormat == PixelFormats.Gray8)
                {
                    bytesPerPixel = 1;

                    if (colorChannel == ColorChannel.Gray)
                        offset = 0;
                    else
                        throw new InvalidOperationException(
                            $"Pixel format {image.PixelFormat.ToString()} has no color channel '{colorChannel}'.");
                }
                else
                {
                    throw new NotSupportedException($"Pixel format {image.PixelFormat.ToString()} is not supported yet.");
                }
                
                ColorChannelAnalytics analytics = new ColorChannelAnalytics(colorChannel, 256);
                await AnalyzeAsync(analytics, image.PixelData, offset, bytesPerPixel, image.PixelCount, cancellationToken);
                
                return analytics;
            }
        }
    }
}
