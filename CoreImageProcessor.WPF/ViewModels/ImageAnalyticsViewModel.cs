using CoreImageProcessor.Processing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows.Media;

namespace CoreImageProcessor.ViewModels
{
    internal class ImageAnalyticsViewModel
    {
        public ImageAnalyticsViewModel(ImageAnalytics analytics, bool useUriSource = false)
        {
            AnalyticsData = analytics;
            ColorMode = $"{analytics.BitsPerChannel}-Bits/Channel ({analytics.ColorCountPerChannel} Colors/Channel)";

            ColorChannels = analytics.ColorChannels.Values.Select((colorChannel) => new ColorChannelAnalyticsViewModel(colorChannel));

            Image = analytics.ToImageSource(useUriSource);
            Histogram = ImageUtils.DrawHistogram(analytics);
        }

        public ImageAnalytics AnalyticsData { get; }

        public bool IsRgb24 => AnalyticsData.PixelFormat == System.Windows.Media.PixelFormats.Rgb24;

        public bool IsGray8 => AnalyticsData.PixelFormat == System.Windows.Media.PixelFormats.Gray8;

        public string? FilePath => AnalyticsData.FilePath;

        public long? FileSize => AnalyticsData.FileSize;

        public string PixelFormat => AnalyticsData.PixelFormat.ToString();

        public int Width => AnalyticsData.Width;

        public int Height => AnalyticsData.Height;

        public int PixelCount => AnalyticsData.PixelCount;

        public string ColorMode { get; }

        public IEnumerable<ColorChannelAnalyticsViewModel> ColorChannels { get; }

        public ImageSource Image { get; }

        public ImageSource Histogram { get; }

    }
}
