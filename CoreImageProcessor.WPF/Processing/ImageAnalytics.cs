using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CoreImageProcessor.Processing
{
    internal partial class ImageAnalytics
    {
        private PixelFormat _PixelFormat;
        private Dictionary<ColorChannel, ColorChannelAnalytics> _ColorChannels;

        /// <summary>
        /// Initializes a new Instance of ImageAnalytics.
        /// </summary>
        /// <param name="width">The width of the image in pixels.</param>
        /// <param name="height">The height of the image in pixels.</param>
        /// <param name="pixelFormat">The pixel format of the image.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Throws if the <paramref name="height"/> or the <paramref name="width"/> is negative.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Throws if the <paramref name="pixelFormat"/> is not supported yet.
        /// </exception>
        private ImageAnalytics(int width, int height, PixelFormat pixelFormat, byte[] pixelData)
        {
            if (width < 0)
                throw new ArgumentOutOfRangeException(nameof(width), width, "Width of an image can't be negative.");

            if (height < 0)
                throw new ArgumentOutOfRangeException(nameof(height), height, "Height of an image can't be negative.");

            Width = width;
            Height = height;
            PixelCount = Width * Height;
            PixelFormat = pixelFormat;
            PixelData = pixelData;
            
            if (pixelFormat == PixelFormats.Rgb24)
            {
                if (PixelData.Length != PixelCount * 3)
                    throw new ArgumentException("Size of pixel data array does not match the specified width, height and pixel format!", nameof(pixelData));
                
                _ColorChannels = new Dictionary<ColorChannel, ColorChannelAnalytics>(3);
            }
            else if (pixelFormat == PixelFormats.Gray8)
            {
                if (PixelData.Length != PixelCount)
                    throw new ArgumentException("Size of pixel data array does not match the specified width, height and pixel format!", nameof(pixelData));
                
                _ColorChannels = new Dictionary<ColorChannel, ColorChannelAnalytics>(1);
            }
            else
            {
                throw new NotSupportedException($"Pixel format {pixelFormat} is not supported yet.");
            }
        }

        /// <summary>
        /// Gets the name of the source image file on the file system.
        /// </summary>
        public string? FileName { get; private set; } = null;

        /// <summary>
        /// Gets the location where the source image file is located on the file system.
        /// </summary>
        public string? FilePath { get; private set; } = null;

        /// <summary>
        /// Gets the amaount of bytes the image occupies on the file system.
        /// </summary>
        public long? FileSize { get; private set; } = null;

        /// <summary>
        /// Gets or sets the Pixel Format of the Immage
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// Throws is the Pixel Format is set to a format that is not supported yet.
        /// </exception>
        public PixelFormat PixelFormat 
        {
            get => _PixelFormat;
            private set
            {
                if (PixelFormats.Gray8.Equals(value) ||
                    PixelFormats.Rgb24.Equals(value))
                {
                    BitsPerChannel = 8;
                    ColorCountPerChannel = 256; //2^8 

                    _PixelFormat = value;
                }
                else
                {
                    throw new NotSupportedException(message: $"Pixel Format '{value.GetType().Name}' is not supported yet.");
                }
            }
        }

        /// <summary>
        /// Gets or sets the width of the image in pixels.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Gets or sets the height of the image in pixels.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Gets or sets the amount how many pixels an image consists of.
        /// </summary>
        public int PixelCount { get; private set; }

        /// <summary>
        /// Gets or sets how many bits a color channel uses tp represent one pixel.
        /// </summary>
        public int BitsPerChannel { get; private set; } 

        /// <summary>
        /// Gets or sets how many discrete colors a channel can represent.
        /// </summary>
        public uint ColorCountPerChannel { get; private set; }

        /// <summary>
        /// Gets or sets the analytics for a specific color channel.
        /// </summary>
        /// <param name="channel">
        /// The color channel to get the analytics for.
        /// </param>
        /// <returns>
        /// Analytics of the specified color channel.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Throws an exception if you try to access a color channel that this image does not contain.
        /// </exception>
        public ColorChannelAnalytics this[ColorChannel channel]
        {
            get
            {
                if (_ColorChannels.TryGetValue(channel, out var analytics))
                    return analytics;
                else
                    throw new InvalidOperationException($"This image does not contain the color channel '{channel.ToString()}'.");
            }
            private set => _ColorChannels[channel] = value;
        }

        /// <summary>
        /// Gets the Color Channels Analytics of this image as a read only dictionary associated by color channel type.
        /// </summary>
        public IReadOnlyDictionary<ColorChannel, ColorChannelAnalytics> ColorChannels => _ColorChannels;

        /// <summary>
        /// Gets the raw pixel data of the image.
        /// </summary>
        public byte[] PixelData { get; private set; }

        public static async Task<ImageAnalytics> AnalyseImageAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            FileInfo imageFile = new FileInfo(path);

            if (!imageFile.Exists)
                throw new FileNotFoundException("Unable to find image under specified path.", path);

            ImageAnalytics analytics;
            
            using (Image<Rgb24> image = await Task.Run(() => Image.Load<Rgb24>(path)))
            {
                analytics = new ImageAnalytics(image.Width, image.Height, PixelFormats.Rgb24, await image.GetPixelDataAsync())
                {
                    FileName = imageFile.Name,
                    FilePath = imageFile.FullName,
                    FileSize = imageFile.Length
                };
            }
            
            await ColorChannelAnalytics.AnalyzeColorChannelsAsync(analytics, cancellationToken).ConfigureAwait(false);

            return analytics;
        }

        public static async Task<ImageAnalytics> AnalyseImageAsync(
            byte[] pixelData, 
            PixelFormat pixelFormat, 
            int width,
            int height, 
            string? fileName = null,
            string? filePath = null,
            long?   fileSize = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ImageAnalytics analytics = new ImageAnalytics(width, height, pixelFormat, pixelData)
            {
                FileName = fileName,
                FilePath = filePath,
                FileSize = fileSize
            };

            if (pixelFormat == PixelFormats.Rgb24)
            {
                if (pixelData.Length != analytics.PixelCount * 3)
                    throw new ArgumentException("Size of pixel data array does not match the specified width, height and pixel format!", nameof(pixelData));
            }
            else if (pixelFormat == PixelFormats.Gray8)
            {
                if (pixelData.Length != analytics.PixelCount)
                    throw new ArgumentException("Size of pixel data array does not match the specified width, height and pixel format!", nameof(pixelData));
            }
            else
            {
                throw new NotSupportedException($"Pixel format {pixelFormat} is not supported yet.");
            }

            await ColorChannelAnalytics.AnalyzeColorChannelsAsync(analytics, cancellationToken).ConfigureAwait(false);

            return analytics;
        }
    }
}
