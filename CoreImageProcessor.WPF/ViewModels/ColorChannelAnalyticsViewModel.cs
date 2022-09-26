using CoreImageProcessor.Processing;
using System;
using System.Collections.Generic;
using System.Text;
using static CoreImageProcessor.Processing.ImageAnalytics;

namespace CoreImageProcessor.ViewModels
{
    internal class ColorChannelAnalyticsViewModel
    {
        public ColorChannelAnalyticsViewModel(ColorChannelAnalytics analytics)
        {
            AnalyticsData = analytics;
        }

        public string ChannelType => AnalyticsData.Type.ToString();

        public long Sum => AnalyticsData.Sum;

        public uint Min => AnalyticsData.Min;

        public uint Max => AnalyticsData.Max;

        public uint Median => AnalyticsData.Median;

        public double Mean => AnalyticsData.Mean;

        public double Variance => AnalyticsData.Variance;

        public double StandardDeviation => AnalyticsData.StandardDeviation;

        public double Entropy => AnalyticsData.Entropy;

        public ColorChannelAnalytics AnalyticsData { get; }
    }
}
