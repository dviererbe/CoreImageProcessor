using ManagedCuda;
using ManagedCuda.NPP;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CoreImageProcessor.Processing
{
    internal static class ImageManipulation
    {
        private static Task Parallelize<TSource, TResult>(TSource source, TResult result, int pixelcount, Action<TSource, TResult, int, int, CancellationToken> action, CancellationToken cancellationToken)
        {
            var fragments = ParallelizationUtils.FragmentLength(pixelcount, maxDegreeOfFragmentation: App.Settings.ThreadLimit);
            Task[] tasks = new Task[fragments.Length];

            for (int i = 0; i < tasks.Length; ++i)
            {
                int iCopy = i;

                tasks[i] = Task.Run(() => action(source, result, fragments[iCopy].StartIndex, fragments[iCopy].EndIndex, cancellationToken));
            }

            return Task.WhenAll(tasks);
        }

        private static Task Parallelize<TSource, TResult, TParameter>(TSource source, TResult result, int pixelcount, Action<TSource, TResult, int, int, TParameter, CancellationToken> action, TParameter parameter, CancellationToken cancellationToken)
        {
            var fragments = ParallelizationUtils.FragmentLength(pixelcount, maxDegreeOfFragmentation: App.Settings.ThreadLimit);
            Task[] tasks = new Task[fragments.Length];

            for (int i = 0; i < tasks.Length; ++i)
            {
                int iCopy = i;

                tasks[i] = Task.Run(() => action(source, result, fragments[iCopy].StartIndex, fragments[iCopy].EndIndex, parameter, cancellationToken));
            }

            return Task.WhenAll(tasks);
        }

        public async static Task<byte[]> ConvertRgb24ToGray8Async(
            byte[] source,
            double redPortion = 0.2126, 
            double greenPortion = 0.7152, 
            double bluePortion = 0.0722,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            byte[] result = new byte[source.Length / 3];

            if (App.Settings.UseCUDA)
            {
                int deviceID = 0;
                CudaContext ctx = new CudaContext(deviceID);
                CudaKernel kernel = ctx.LoadKernel(@".\CUDA\ImageProcessingKernels.ptx", "ConvertRgb24ToGray8");
                kernel.GridDimensions = (result.Length + 255) / 256;
                kernel.BlockDimensions = 256;

                CudaDeviceVariable<byte> d_source = source;
                CudaDeviceVariable<byte> d_result = new CudaDeviceVariable<byte>(result.Length);

                kernel.Run(d_source.DevicePointer, d_result.DevicePointer, redPortion, greenPortion, bluePortion, result.Length);
                d_result.CopyToHost(result);
            }
            else
            {
                static void TaskKernel(byte[] source, byte[] result, int startIndex, int endIndex, (double redPortion, double greenPortion, double bluePortion) parameter, CancellationToken cancellationToken)
                {
                    for (int k = startIndex * 3, i = startIndex; i < endIndex; ++i, k += 3)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        double value = parameter.redPortion * source[k] + parameter.greenPortion * source[k + 1] + parameter.bluePortion * source[k + 2];

                        if (value >= byte.MaxValue)
                            result[i] = byte.MaxValue;
                        else if (value <= byte.MinValue)
                            result[i] = byte.MinValue;
                        else
                            result[i] = (byte)Math.Round(value);
                    }
                }

                await Parallelize<byte[], byte[], (double, double, double)>(source, result, result.Length, TaskKernel, (redPortion, greenPortion, bluePortion), cancellationToken).ConfigureAwait(false);
            }

            return result;
        }

        public async static Task<byte[]> ConvertGray8ToRgb24Async(byte[] source, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            byte[] result = new byte[source.Length * 3];

            if (App.Settings.UseCUDA)
            {
                int deviceID = 0;
                CudaContext ctx = new CudaContext(deviceID);
                CudaKernel kernel = ctx.LoadKernel(@".\CUDA\ImageProcessingKernels.ptx", "ConvertGray8ToRgb24");
                kernel.GridDimensions = (source.Length + 255) / 256;
                kernel.BlockDimensions = 256;

                CudaDeviceVariable<byte> d_source = source;
                CudaDeviceVariable<byte> d_result = new CudaDeviceVariable<byte>(result.Length);

                kernel.Run(d_source.DevicePointer, d_result.DevicePointer, source.Length);
                d_result.CopyToHost(result);
            }
            else
            {
                static void TaskKernel(byte[] source, byte[] result, int startIndex, int endIndex, CancellationToken cancellationToken)
                {
                    for (int k = startIndex, i = startIndex * 3; k < endIndex; ++k)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        result[i++] = source[k];
                        result[i++] = source[k];
                        result[i++] = source[k];
                    }
                }

                await Parallelize(source, result, source.Length, TaskKernel, cancellationToken).ConfigureAwait(false);
            }

            return result;
        }

        public async static Task<byte[]> InvertAsync(byte[] source, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            byte[] result = new byte[source.Length];

            if (App.Settings.UseCUDA)
            {
                int deviceID = 0;
                CudaContext ctx = new CudaContext(deviceID);
                CudaKernel kernel = ctx.LoadKernel(@".\CUDA\ImageProcessingKernels.ptx", "Invert");
                kernel.GridDimensions = (result.Length + 255) / 256;
                kernel.BlockDimensions = 256;

                CudaDeviceVariable<byte> d_source = source;
                CudaDeviceVariable<byte> d_result = new CudaDeviceVariable<byte>(result.Length);

                kernel.Run(d_source.DevicePointer, d_result.DevicePointer, source.Length);
                d_result.CopyToHost(result);
            }
            else
            {
                static void TaskKernel(byte[] source, byte[] result, int startIndex, int endIndex, CancellationToken cancellationToken)
                {
                    for (int i = startIndex; i < endIndex; ++i)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        result[i] = unchecked((byte)~source[i]);
                    }
                }

                await Parallelize(source, result, source.Length, TaskKernel, cancellationToken).ConfigureAwait(false);
            }

            return result;
        }

        public async static Task<byte[]> ApplyThresholdAsync(byte[] source, int threshold, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            byte[] result = new byte[source.Length];

            if (App.Settings.UseCUDA)
            {
                int deviceID = 0;
                CudaContext ctx = new CudaContext(deviceID);
                CudaKernel kernel = ctx.LoadKernel(@".\CUDA\ImageProcessingKernels.ptx", "ApplyThreshold");
                kernel.GridDimensions = (result.Length + 255) / 256;
                kernel.BlockDimensions = 256;

                CudaDeviceVariable<byte> d_source = source;
                CudaDeviceVariable<byte> d_result = new CudaDeviceVariable<byte>(result.Length);

                kernel.Run(d_source.DevicePointer, d_result.DevicePointer, threshold, source.Length);
                d_result.CopyToHost(result);
            }
            else
            {
                static void TaskKernel(byte[] source, byte[] result, int startIndex, int endIndex, int threshold, CancellationToken cancellationToken)
                {
                    for (int i = startIndex; i < endIndex; ++i)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        result[i] = source[i] < threshold ? byte.MinValue : byte.MaxValue;
                    }
                }
                
                await Parallelize(source, result, source.Length, TaskKernel, threshold, cancellationToken).ConfigureAwait(false);
            }

            return result;
        }

        public async static Task<byte[]> AdjustBrightnessAsync(byte[] source, int value, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            byte[] result = new byte[source.Length];

            if (App.Settings.UseCUDA)
            {
                int deviceID = 0;
                CudaContext ctx = new CudaContext(deviceID);
                CudaKernel kernel = ctx.LoadKernel(@".\CUDA\ImageProcessingKernels.ptx", "AdjustBrightness");
                kernel.GridDimensions = (result.Length + 255) / 256;
                kernel.BlockDimensions = 256;

                CudaDeviceVariable<byte> d_source = source;
                CudaDeviceVariable<byte> d_result = new CudaDeviceVariable<byte>(result.Length);

                kernel.Run(d_source.DevicePointer, d_result.DevicePointer, value, source.Length);
                d_result.CopyToHost(result);
            }
            else
            {
                static void TaskKernel(byte[] source, byte[] result, int startIndex, int endIndex, int value, CancellationToken cancellationToken)
                {
                    for (int i = startIndex; i < endIndex; ++i)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        int newValue = source[i] + value;

                        if (newValue > byte.MaxValue)
                            result[i] = byte.MaxValue;
                        else if (newValue < byte.MinValue)
                            result[i] = byte.MinValue;
                        else
                            result[i] = (byte)newValue;
                    }
                }

                await Parallelize(source, result, source.Length, TaskKernel, value, cancellationToken).ConfigureAwait(false);
            }

            return result;
        }

        public async static Task<byte[]> AdjustContrastAsync(byte[] source, int factor, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            byte[] result = new byte[source.Length];

            if (App.Settings.UseCUDA)
            {
                int deviceID = 0;
                CudaContext ctx = new CudaContext(deviceID);
                CudaKernel kernel = ctx.LoadKernel(@".\CUDA\ImageProcessingKernels.ptx", "AdjustContrast");
                kernel.GridDimensions = (result.Length + 255) / 256;
                kernel.BlockDimensions = 256;

                CudaDeviceVariable<byte> d_source = source;
                CudaDeviceVariable<byte> d_result = new CudaDeviceVariable<byte>(result.Length);

                kernel.Run(d_source.DevicePointer, d_result.DevicePointer, factor, source.Length);
                d_result.CopyToHost(result);
            }
            else
            {
                static void TaskKernel(byte[] source, byte[] result, int startIndex, int endIndex, float factor, CancellationToken cancellationToken)
                {
                    for (int i = startIndex; i < endIndex; ++i)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        float newValue = factor * (source[i] - 128f) + 128f;

                        if (newValue > byte.MaxValue)
                            result[i] = byte.MaxValue;
                        else if (newValue < byte.MinValue)
                            result[i] = byte.MinValue;
                        else
                            result[i] = (byte)MathF.Round(newValue);
                    }
                }

                float correctionFactor = 259f * (factor + 255f) / (255f * (259f - factor));

                await Parallelize(source, result, source.Length, TaskKernel, correctionFactor, cancellationToken).ConfigureAwait(false);
            }

            return result;
        }

        public async static Task<byte[]> ApplyU8LookupTablePerChannelAsync(byte[] source, byte[] lookupTable, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (lookupTable.Length != 256)
            {
                throw new ArgumentException("Lookup Table is too small or to big. Define a value for each 256 distict values.", nameof(lookupTable));
            }

            byte[] result = new byte[source.Length];

            if (App.Settings.UseCUDA)
            {
                int deviceID = 0;
                CudaContext ctx = new CudaContext(deviceID);
                CudaKernel kernel = ctx.LoadKernel(@".\CUDA\ImageProcessingKernels.ptx", "ApplyU8LookupTablePerChannel");
                kernel.GridDimensions = (result.Length + 255) / 256;
                kernel.BlockDimensions = 256;

                CudaDeviceVariable<byte> d_source = source;
                CudaDeviceVariable<byte> d_result = new CudaDeviceVariable<byte>(result.Length);
                CudaDeviceVariable<byte> d_lookupTable = lookupTable;

                kernel.Run(d_source.DevicePointer, d_result.DevicePointer, d_lookupTable.DevicePointer, source.Length);
                d_result.CopyToHost(result);
            }
            else
            {
                static void TaskKernel(byte[] source, byte[] result, int startIndex, int endIndex, byte[] lookupTable, CancellationToken cancellationToken)
                {
                    for (int i = startIndex; i < endIndex; ++i)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        result[i] = lookupTable[source[i]];
                    }
                }

                await Parallelize(source, result, source.Length, TaskKernel, lookupTable, cancellationToken).ConfigureAwait(false);
            }

            return result;
        }

        public async static Task<byte[]> ApplyU8LookupTablePerChannelAsync(byte[] source, byte[,] lookupTables, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (lookupTables.GetLength(1) != 256)
            {
                throw new ArgumentException("Lookup Table is too small or to big. Define a value for each 256 distict values.", nameof(lookupTables));
            }

            byte[] result = new byte[source.Length];

            if (App.Settings.UseCUDA)
            {
                throw new NotImplementedException("CUDA ist not supported for this Operation!");
            }
            else
            {
                static void TaskKernel(byte[] source, byte[] result, int startIndex, int endIndex, byte[,] lookupTables, CancellationToken cancellationToken)
                {
                    int start = startIndex * lookupTables.GetLength(0);
                    int end = endIndex * lookupTables.GetLength(0);

                    for (int i = start; i < end;)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        for (int k = 0; k < lookupTables.GetLength(0); ++k, ++i)
                        {
                            result[i] = lookupTables[k, source[i]];
                        }
                    }
                }

                await Parallelize(source, result, source.Length / lookupTables.GetLength(0), TaskKernel, lookupTables, cancellationToken).ConfigureAwait(false);

                return result;
            }
        }

        public static Task<byte[]> ApplyToneSeparationAsync(byte[] source, int steps, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<byte[]>(cancellationToken);

            if (steps < 1)
                return Task.FromException<byte[]>(new ArgumentOutOfRangeException(nameof(steps), steps, "Number of steps have to be equal or greater than 1."));
            if (steps > 256)
                return Task.FromException<byte[]>(new ArgumentOutOfRangeException(nameof(steps), steps, "Number of steps have to be equal or less than 265."));

            byte[] lookupTable = new byte[256];

            byte stepSize = (byte)(256 / steps);

            int maxLevel = stepSize;
            byte color = (byte)(stepSize / 2);
            
            for (int i = 0; i < lookupTable.Length; ++i)
            {
                if (i == maxLevel)
                    color += stepSize;

                lookupTable[i] = color;
            }

            return ApplyU8LookupTablePerChannelAsync(source, lookupTable, cancellationToken);
        }

        /*
        public async static Task<byte[]> ApplyFilterKernelU8Async(Mapper2D<byte> image, float[,] filter, CancellationToken cancellationToken = default)
        {
            static byte[] Sum(int sourceChannelCount, int resultChannelCount, float[,,] values)
            {
                float[] sum = new float[resultChannelCount];
                
                for (int x = 0; x < values.GetLength(0); ++x)
                {
                    for (int y = 0; y < values.GetLength(1); ++y)
                    {
                        for(int k = 0; k < sourceChannelCount; ++k)
                        {
                            sum[k] += values[x, y, k];
                        }
                    }
                }

                byte[] result = new byte[resultChannelCount];

                for (int k = 0; k < result.Length; ++k)
                {
                    if (sum[k] > byte.MaxValue)
                        result[k] = byte.MaxValue;
                    else if (sum[k] < byte.MinValue)
                        result[k] = byte.MinValue;
                    else
                        result[k] = (byte)MathF.Round(sum[k]);
                }

                return result;
            }

            Mapper2D<byte> result = Mapper2DFactory.CreateU8Instance(new byte[image.Source.Length], image.Width, image.Height, image.Channels, image.EdgeHandling, image.Constant);

            await MaskProcessU8Async(image, result, filter, Sum, cancellationToken);

            return result.Source;
        }
        */

        public async static Task<byte[]> ApplyFilterKernelU8Async(Mapper2D<byte> image, float[,] filter, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (filter.Length % 2 == 0)
            {
                throw new ArgumentException("Filter length has to be odd.", nameof(filter));
            }

            Mapper2D<byte> result = Mapper2DFactory.CreateU8Instance(new byte[image.Source.Length], image.Width, image.Height, image.Channels, image.EdgeHandling, image.Constant);

            if (App.Settings.UseCUDA)
            {
                throw new NotImplementedException("CUDA ist not supported for this Operation!");
            }
            else
            {
                static void TaskKernel(Mapper2D<byte> source, Mapper2D<byte> result, int startIndex, int endIndex, float[,] filter, CancellationToken cancellationToken)
                {
                    int x = startIndex % source.Width;
                    int y = startIndex / source.Width;

                    int filterSizeXHalf = filter.GetLength(0) / 2;
                    int filterSizeYHalf = filter.GetLength(1) / 2;

                    for (int i = startIndex; i < endIndex && y < source.Height; ++y, x = 0)
                    {
                        for (; i < endIndex && x < source.Width; ++x)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            if (source.EdgeHandling == EdgeHandling.None && 
                                (x - filterSizeXHalf < 0 ||
                                 x + filterSizeXHalf >= source.Width ||
                                 y - filterSizeYHalf < 0 ||
                                 y + filterSizeYHalf >= source.Height))
                            {
                                result[x,y] = source[x, y];
                            }
                            else
                            {
                                float[] values = new float[source.Channels];

                                for (int xFilter = 0, xOffset = -filterSizeXHalf; xOffset <= filterSizeXHalf; ++xFilter, ++xOffset)
                                {
                                    for (int yFilter = 0, yOffset = -filterSizeYHalf; yOffset <= filterSizeYHalf; ++yFilter, ++yOffset)
                                    {
                                        if (filter[xFilter, yFilter] != 0f)
                                        {
                                            if (source.TryGetValue(x + xOffset, y + yOffset, out byte[] value))
                                            {
                                                for (int k = 0; k < value.Length; ++k)
                                                {
                                                    values[k] += filter[xFilter, yFilter] * value[k];
                                                }
                                            }
                                        }
                                    }
                                }

                                byte[] pixel = new byte[source.Channels];

                                for (int k = 0; k < pixel.Length; ++k)
                                {
                                    if (values[k] > byte.MaxValue)
                                        pixel[k] = byte.MaxValue;
                                    else if (values[k] < byte.MinValue)
                                        pixel[k] = byte.MinValue;
                                    else
                                        pixel[k] = (byte)MathF.Round(values[k]);
                                }

                                result[x, y] = pixel;
                            }
                        }
                    }
                }

                await Parallelize(image, result, image.PixelCount, TaskKernel, filter, cancellationToken).ConfigureAwait(false);
            }

            return result.Source;
        }

        public static Task<byte[]> ApplyFilterKernelU8Async(Mapper2D<byte> source, KernelType kernelType, KernelSize kernelSize, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<byte[]>(cancellationToken);
            }

            if (ImageUtils.TryGetFilterKernel(kernelType, kernelSize, out float[,] kernel))
            {
                return ApplyFilterKernelU8Async(source, kernel, cancellationToken);
            }
            else
            {
                return Task.FromException<byte[]>(new NotSupportedException("The specified kernel size and/or kernel type is not supported!"));
            }
        }

        public async static Task<byte[]> ApplyMorphologicalFilterKernelU8C1Async(Mapper2D<byte> image, MorphologicalOperation operation, bool[,] filter, byte threshold, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (filter.Length % 2 == 0)
            {
                throw new ArgumentException("Filter length has to be odd.", nameof(filter));
            }

            if (image.Channels != 1)
            {
                throw new NotSupportedException("Morphological filters are just supported for grayscale images.");
            }

            Mapper2D<byte> result = Mapper2DFactory.CreateU8Instance(new byte[image.Source.Length], image.Width, image.Height, image.Channels, image.EdgeHandling, image.Constant);

            if (App.Settings.UseCUDA)
            {
                throw new NotImplementedException("CUDA ist not supported for this Operation!");
            }
            else
            {
                static void TaskKernel(Mapper2D<byte> source, Mapper2D<byte> result, int startIndex, int endIndex, (bool[,] filter, byte threshold, MorphologicalOperation operation) parameter, CancellationToken cancellationToken)
                {
                    bool[,] filter = parameter.filter;
                    byte threshold = parameter.threshold;

                    int x = startIndex % source.Width;
                    int y = startIndex / source.Width;

                    int filterSizeXHalf = filter.GetLength(0) / 2;
                    int filterSizeYHalf = filter.GetLength(1) / 2;

                    for (int i = startIndex; i < endIndex && y < source.Height; ++y, x = 0)
                    {
                        for (; i < endIndex && x < source.Width; ++x)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            if (source.EdgeHandling == EdgeHandling.None &&
                                (x - filterSizeXHalf < 0 ||
                                 x + filterSizeXHalf >= source.Width ||
                                 y - filterSizeYHalf < 0 ||
                                 y + filterSizeYHalf >= source.Height))
                            {
                                result[x, y] = source[x, y];
                            }
                            else
                            {
                                bool pixelFound = false;
                                bool missingPixelFound = false;

                                for (int xFilter = 0, xOffset = -filterSizeXHalf; !(pixelFound && missingPixelFound) && xOffset <= filterSizeXHalf; ++xFilter, ++xOffset)
                                {
                                    for (int yFilter = 0, yOffset = -filterSizeYHalf; !(pixelFound && missingPixelFound) && yOffset <= filterSizeYHalf; ++yFilter, ++yOffset)
                                    {
                                        if (filter[xFilter, yFilter])
                                        {
                                            if (source.TryGetValue(x + xOffset, y + yOffset, out byte[] value))
                                            {
                                                if (value[0] >= threshold)
                                                    pixelFound = true;
                                                else
                                                    missingPixelFound = true;
                                            }
                                        }
                                    }
                                }

                                if (parameter.operation == MorphologicalOperation.Dilation)
                                {
                                    if (pixelFound)
                                        result[x, y] = new byte[] { byte.MaxValue };
                                    else
                                        result[x, y] = new byte[] { byte.MinValue };
                                }
                                else if (parameter.operation == MorphologicalOperation.Erosion)
                                {
                                    if (missingPixelFound)
                                        result[x, y] = new byte[] { byte.MinValue };
                                    else
                                        result[x, y] = new byte[] { byte.MaxValue };
                                }
                            }
                        }
                    }
                }

                await Parallelize(image, result, image.PixelCount, TaskKernel, (filter, threshold, operation), cancellationToken).ConfigureAwait(false);
            }

            return result.Source;
        }

        public static Task<byte[]> ApplyMorphologicalFilterKernelU8C1Async(Mapper2D<byte> source, MorphologicalOperation morphologicalOperation, KernelShape kernelShape, KernelSize kernelSize, byte threshold, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<byte[]>(cancellationToken);
            }

            if (ImageUtils.TryGetMorphologicalFilterKernel(kernelShape, kernelSize, out bool[,] kernel))
            {
                return ApplyMorphologicalFilterKernelU8C1Async(source, morphologicalOperation, kernel, threshold, cancellationToken);
            }
            else
            {
                return Task.FromException<byte[]>(new NotSupportedException("The specified kernel size and/or kernel shape is not supported!"));
            }
        }


        public static byte[] CalculateSekelteton(Mapper2D<byte> image)
        {
            //DIESE FUNKTION IST ABSOLUT NICHT OPTIMIERT,
            //da ich diese funktion als letztes hinzugefügt habe um das angeforderte featureset zu vervollständigen.
            //
            //Diese implementierung ist der Zhang/Suen-Algorithmus von C++ übertragen in C#
            //Quelle: https://www.tutorials.de/threads/c-skelettierung-mit-zhang-suen-algorithmus.367139/
            //
            //VIELEN DANK AN DEN ORIGINAL AUTOR!!!

            if (image.Channels != 1)
                throw new NotSupportedException("Morphological filters are just supported for grayscale images.");

            byte[,] skelett = new byte[image.Width, image.Height];

            {
                for (int i = 0, y = 0; y < image.Height; ++y)
                {
                    for (int x = 0; x < image.Width; ++x, ++i)
                    {
                        skelett[x, y] = image.Source[i] >= 128 ? byte.MaxValue : byte.MinValue;
                    }
                }
            }

            {
                int x, y, i, Anzahl, Bedingung, Aenderung;

                //die 8 direkten Nachbarn
                int[] P = new int[11];

                bool Ende = false;

                do
                {
                    Aenderung = 0;

                    for (x = 1; x < skelett.GetLength(0) - 1; x++)
                    {
                        for (y = 1; y < skelett.GetLength(1) - 1; y++)
                        {
                            if (skelett[x, y] == 0)
                            {
                                /* Nachbarn ermitteln. Zur leichteren Berechnung */
                                P[2] = skelett[x, y - 1];
                                P[3] = skelett[x + 1, y - 1];
                                P[4] = skelett[x + 1, y];
                                P[5] = skelett[x + 1, y + 1];
                                P[6] = skelett[x, y + 1];
                                P[7] = skelett[x - 1, y + 1];
                                P[8] = skelett[x - 1, y];
                                P[9] = skelett[x - 1, y - 1];
                                P[10] = P[2]; /* Fuer 0-1-Uebergang von P[9]-P[2] */

                                Bedingung = 0;
                                Anzahl = 0; /* Anzahl der weissen Nachbarn */

                                for (i = 2; i <= 9; i++)
                                {
                                    if (P[i] == 255)
                                    {
                                        Anzahl++;
                                    }
                                }

                                /* diese erste Bedingung weicht von Zhang/Suen ab*/
                                if ((3 <= Anzahl) && (Anzahl <= 6)) /* 3<=S(P1)<=6 */
                                {
                                    Bedingung += 1;
                                }

                                if (!(P[2] > 0 && P[4] > 0 && P[6] > 0)) /* P2*P4*P6=0 */
                                {
                                    Bedingung += 2;
                                }

                                if (!(P[4] > 0 && P[6] > 0 && P[8] > 0)) /* P4*P6*P8=0 */
                                {
                                    Bedingung += 4;
                                }

                                Anzahl = 0;

                                for (i = 2; i < 10; i++)
                                {
                                    if (P[i] == 0 && P[i + 1] > 0)
                                    {
                                        Anzahl++; /* 01-Muster */
                                    }
                                }

                                if (Anzahl == 1)
                                {
                                    Bedingung += 8;
                                }

                                /* Falls alle Bedingungen a-d erfuellt, markieren*/
                                if (Bedingung == 15)
                                {
                                    skelett[x, y] = 128;/* Pixel mark.*/
                                    Aenderung++;
                                }
                            }
                        }
                    }

                    if (Aenderung == 0)
                    {
                        Ende = true;
                    }

                    /* Nach einem Durchlauf nun alle markierten Pixel loeschen */
                    for (x = 0; x < skelett.GetLength(0); x++)
                    {
                        for (y = 0; y < skelett.GetLength(1); y++)
                        {
                            if (skelett[x, y] == 128)
                            {
                                skelett[x, y] = 255;
                            }
                        }
                    }

                    // Nun von rechts unten nach links oben Skelett berechnen 
                    Aenderung = 0;

                    for (x = skelett.GetLength(0) - 2; x >= 1; x--)
                    {
                        for (y = skelett.GetLength(1) - 2; y >= 1; y--)
                        {
                            if (skelett[x, y] > 0)
                            {
                                // Nachbarn ermitteln. Zur leichteren Berechnung
                                P[2] = skelett[x, y - 1];
                                P[3] = skelett[x + 1, y - 1];
                                P[4] = skelett[x + 1, y];
                                P[5] = skelett[x + 1, y + 1];
                                P[6] = skelett[x, y + 1];
                                P[7] = skelett[x - 1, y + 1];
                                P[8] = skelett[x - 1, y];
                                P[9] = skelett[x - 1, y - 1];
                                P[10] = P[2]; // Fuer 0-1-Uebergang von P[9]-P[2]

                                Bedingung = 0;
                                Anzahl = 0; // Anzahl der weissen Nachbarn

                                for (i = 2; i <= 9; i++)
                                {
                                    if (P[i] == 255)
                                    {
                                        Anzahl++;
                                    }
                                }
                                // diese erste Bedingung weicht von Zhang/Suen ab
                                if ((3 <= Anzahl) && (Anzahl <= 6)) // 3<=S(P1)<=6
                                {
                                    Bedingung += 1;
                                }

                                if (!(P[2] > 0 && P[4] > 0 && P[8] > 0)) // P2*P4*P8=0
                                {
                                    Bedingung += 2;
                                }

                                if (!(P[2] > 0 && P[6] > 0 && P[8] > 0)) // P2*P6*P8=0
                                {
                                    Bedingung += 4;
                                }

                                Anzahl = 0;

                                for (i = 2; i < 10; i++)
                                {
                                    if (P[i] == 0 && P[i + 1] > 0)
                                    {
                                        Anzahl++; // 01-Muster 
                                    }
                                }

                                if (Anzahl == 1)
                                {
                                    Bedingung += 8;
                                }
                                // Falls alle Bedingungen a-d erfuellt, markieren
                                if (Bedingung == 15)
                                {
                                    skelett[x, y] = 128;// Pixel mark.
                                    Aenderung++;
                                }
                            }
                        }
                    }

                    if ((Aenderung == 0) && (Ende == true))
                    {
                        Ende = true;
                    }

                    // Nach einem Durchlauf nun alle markierten Pixel loeschen
                    for (x = 0; x < skelett.GetLength(0); x++)
                    {
                        for (y = 0; y < skelett.GetLength(1); y++)
                        {
                            if (skelett[x, y] == 128)
                            {
                                skelett[x, y] = 255;
                            }
                        }
                    }
                }
                while (!Ende); // Ende erst, falls keine Aenderung mehr 
            }

            byte[] result = new byte[image.PixelCount];

            for (int i = 0, y = 0; y < image.Height; ++y)
            {
                for (int x = 0; x < image.Width; ++x, ++i)
                {
                    result[i] = skelett[x, y];
                }
            }

            return result;
        }

        public static async Task<byte[]> ApplyRankOrderFilterU8Async<RankStateMachine>(
            Mapper2D<byte> image, bool[,] filter, CancellationToken cancellationToken = default) 
            where RankStateMachine : IRankOrderStateMachine<byte>, new()
        {
            if (filter.Length % 2 == 0)
            {
                throw new ArgumentException("Filter length has to be odd.", nameof(filter));
            }

            Mapper2D<byte> result = Mapper2DFactory.CreateU8Instance(new byte[image.Source.Length], image.Width, image.Height, image.Channels, image.EdgeHandling, image.Constant);

            if (App.Settings.UseCUDA)
            {
                throw new NotImplementedException("CUDA ist not supported for this Operation!");
            }
            else
            {
                static void TaskKernel(Mapper2D<byte> source, Mapper2D<byte> result, int startIndex, int endIndex, bool[,] filter, CancellationToken cancellationToken)
                {
                    int x = startIndex % source.Width;
                    int y = startIndex / source.Width;

                    int filterSizeXHalf = filter.GetLength(0) / 2;
                    int filterSizeYHalf = filter.GetLength(1) / 2;

                    RankStateMachine[] rankStateMachines = new RankStateMachine[source.Channels];
                    for (int k = 0; k < rankStateMachines.Length; ++k)
                    {
                        rankStateMachines[k] = new RankStateMachine();
                    }

                    for (int i = startIndex; i < endIndex && y < source.Height; ++y, x = 0)
                    {
                        for (; i < endIndex && x < source.Width; ++x)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            if (source.EdgeHandling == EdgeHandling.None &&
                                (x - filterSizeXHalf < 0 ||
                                 x + filterSizeXHalf >= source.Width ||
                                 y - filterSizeYHalf < 0 ||
                                 y + filterSizeYHalf >= source.Height))
                            {
                                result[x, y] = source[x, y];
                            }
                            else
                            {
                                bool done = false;

                                for (int xFilter = 0, xOffset = -filterSizeXHalf; !done && xOffset <= filterSizeXHalf; ++xFilter, ++xOffset)
                                {
                                    for (int yFilter = 0, yOffset = -filterSizeYHalf; !done && yOffset <= filterSizeYHalf; ++yFilter, ++yOffset)
                                    {
                                        if (filter[xFilter, yFilter])
                                        {
                                            if (source.TryGetValue(x + xOffset, y + yOffset, out byte[] value))
                                            {
                                                done = true;

                                                for (int k = 0; k < rankStateMachines.Length; ++k)
                                                {
                                                    rankStateMachines[k].AddValue(value[k]);
                                                    done = done && rankStateMachines[k].FinishedEarly;
                                                }

                                                if (done) goto AssignValue;
                                            }
                                        }
                                    }
                                }

                            AssignValue:
                                byte[] pixel = new byte[rankStateMachines.Length];
                                byte[] localValue = source[x, y];

                                for (int k = 0; k < rankStateMachines.Length; ++k)
                                {
                                    pixel[k] = rankStateMachines[k].GetResult(localValue[k]);
                                    rankStateMachines[k].Reset();
                                }

                                result[x, y] = pixel;
                            }
                        }
                    }
                }

                await Parallelize(image, result, image.PixelCount, TaskKernel, filter, cancellationToken).ConfigureAwait(false);
            }

            return result.Source;
        }

        public static Task<byte[]> ApplyRankOrderFilterU8Async(Mapper2D<byte> source, RankOrderFilter filterType, bool[,] mask, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<byte[]>(cancellationToken);

            if (App.Settings.UseCUDA)
            {
                return Task.FromException<byte[]>(new NotImplementedException("CUDA ist not supported for this Operation!"));
            }
            else
            {
                Mapper2D<byte> result = Mapper2DFactory.CreateU8Instance(new byte[source.Source.Length], source.Width, source.Height, source.Channels, source.EdgeHandling, source.Constant);

                if (filterType == RankOrderFilter.Maximum)
                {
                    return ApplyRankOrderFilterU8Async<U8MaximumRankOrderStateMachine>(source, mask, cancellationToken);
                }
                else if (filterType == RankOrderFilter.Minimum)
                {
                    return ApplyRankOrderFilterU8Async<U8MinimumRankOrderStateMachine>(source, mask, cancellationToken);
                }
                else if (filterType == RankOrderFilter.Median)
                {
                    return ApplyRankOrderFilterU8Async<U8MedianRankOrderStateMachine>(source, mask, cancellationToken);
                }
                else if (filterType == RankOrderFilter.LocalHistogramOperation)
                {
                    return ApplyRankOrderFilterU8Async<U8LocalHistogramOperationStateMachine>(source, mask, cancellationToken);
                }
                else
                {
                    return Task.FromException<byte[]>(new NotSupportedException("Specified RankOrderFilter is not supported."));
                }
            }
        }

        public static Task<byte[]> ApplyRankOrderFilterU8Async(Mapper2D<byte> source, RankOrderFilter filterType, KernelSize kernelSize, KernelShape kernelShape, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<byte[]>(cancellationToken);

            if (ImageUtils.TryGetMorphologicalFilterKernel(kernelShape, kernelSize, out bool[,] kernel))
            {
                return ApplyRankOrderFilterU8Async(source, filterType, kernel, cancellationToken);
            }
            else
            {
                return Task.FromException<byte[]>(new NotSupportedException("The specified kernel size and/or kernel shape is not supported!"));
            }
        }

        public async static Task<byte[]> Rotate90DegreesU8Async(byte[] source, int width, int height, int channels, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (App.Settings.UseCUDA)
            {
                throw new NotImplementedException("CUDA ist not supported for this Operation!");
            }
            else
            {
                (int, int) mapCoordinates(int x, int y)
                {
                    return (height - y - 1, x);
                }

                return await MapPixelsU8Async(source, width, height, newWidth: height, newHeight: width, channels, mapCoordinates, cancellationToken);
            }
        }

        public async static Task<byte[]> Rotate180DegreesU8Async(byte[] source, int width, int height, int channels, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (App.Settings.UseCUDA)
            {
                throw new NotImplementedException("CUDA ist not supported for this Operation!");
            }
            else
            {
                (int, int) mapCoordinates(int x, int y)
                {
                    return (width - x - 1, height - y - 1);
                }

                return await MapPixelsU8Async(source, width, height, newWidth: width, newHeight: height, channels, mapCoordinates, cancellationToken);
            }
        }

        public async static Task<byte[]> Rotate270DegreesU8Async(byte[] source, int width, int height, int channels, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (App.Settings.UseCUDA)
            {
                throw new NotImplementedException("CUDA ist not supported for this Operation!");
            }
            else
            {
                (int, int) mapCoordinates(int x, int y)
                {
                    return (y, width - x - 1);
                }

                return await MapPixelsU8Async(source, width, height, newWidth: height, newHeight: width, channels, mapCoordinates, cancellationToken);
            }
        }

        public async static Task<byte[]> MirrorHorizontalU8Async(byte[] source, int width, int height,  int channels, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (App.Settings.UseCUDA)
            {
                throw new NotImplementedException("CUDA ist not supported for this Operation!");
            }
            else
            {
                (int, int) mapCoordinates(int x, int y)
                {
                    return (width - x - 1, y);
                }

                return await MapPixelsU8Async(source, width, height, newWidth: width, newHeight: height, channels, mapCoordinates, cancellationToken);
            }
        }

        public async static Task<byte[]> MirrorVerticalU8Async(byte[] source, int width, int height, int channels, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (App.Settings.UseCUDA)
            {
                throw new NotImplementedException("CUDA ist not supported for this Operation!");
            }
            else
            {
                (int, int) mapCoordinates(int x, int y)
                {
                    return (x, height - y - 1);
                }

                return await MapPixelsU8Async(source, width, height, newWidth: width, newHeight: height, channels, mapCoordinates, cancellationToken);
            }
        }

        public async static Task<byte[]> MapPixelsU8Async(byte[] source, int width, int height, int newWidth, int newHeight, int channels, Func<int, int, (int newX, int newY)> mapFunction, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            byte[] result = new byte[source.Length];

            if (App.Settings.UseCUDA)
            {
                throw new NotImplementedException("CUDA ist not supported for this Operation!");
            }
            else
            {
                void TaskKernel(byte[] source, byte[] result, int startIndex, int endIndex, CancellationToken cancellationToken)
                {
                    int x = startIndex % width;
                    int y = startIndex / width;

                    int j = startIndex;
                    int i = (y * width + x) * channels;
                    
                    for (; j < endIndex; ++y)
                    {
                        for (; j < endIndex && x < width; ++x, ++j)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            var newCoordinates = mapFunction(x, y);
                            int index = (newCoordinates.newY * newWidth + newCoordinates.newX) * channels;

                            for (int k = 0; k < channels; ++k)
                            {
                                result[index++] = source[i++];
                            }
                        }

                        x = 0;
                    }
                }

                await Parallelize(source, result, width * height, TaskKernel, cancellationToken).ConfigureAwait(false);

                return result;
            }
        }

        public async static Task<byte[]> ScaleU8Async(byte[] source, int width, int height, int newWidth, int newHeight, int channels, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            byte[] result = new byte[newWidth * newHeight * channels];

            if (App.Settings.UseCUDA)
            {
                throw new NotImplementedException("CUDA ist not supported for this Operation!");
            }
            else
            {
                double xRatio = width / (double)newWidth;
                double yRatio = height / (double)newHeight;

                //Nearest Neighbor Interpolation
                void TaskKernel(byte[] source, byte[] result, int startIndex, int endIndex, CancellationToken cancellationToken)
                {
                    int x = startIndex % newWidth;
                    int y = startIndex / newWidth;

                    int i = startIndex * channels;

                    double xSource, ySource;
                    int index;

                    for (int j = startIndex; j < endIndex; ++y)
                    {
                        for (; j < endIndex && x < newWidth; ++x, ++j)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            xSource = Math.Floor(x * xRatio);
                            ySource = Math.Floor(y * yRatio);

                            //index = (y * width + x) * channels
                            index = (int)((ySource * width + xSource) * channels);

                            for (int k = 0; k < channels; ++k)
                            {
                                result[i++] = source[index++];
                            }
                        }

                        x = 0;
                    }
                }

                await Parallelize(source, result, newWidth * newHeight, TaskKernel, cancellationToken).ConfigureAwait(false);

                return result;
            }
        }
    }
}
