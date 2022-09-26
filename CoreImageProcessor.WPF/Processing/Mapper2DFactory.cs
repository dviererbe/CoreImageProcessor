using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows.Media;

namespace CoreImageProcessor.Processing
{
    internal class Mapper2DFactory
    {
        public static Mapper2D<byte> CreateU8Instance(byte[] source, int width, int height, int channels, EdgeHandling edgeHandling, byte? constant = null)
        {
            switch (channels)
            {
                case 1:
                    return new U8C1Mapper2D(source, width, height, edgeHandling, constant);
                case 3:
                    return new U8C3Mapper2D(source, width, height, edgeHandling, constant);
            }

            throw new NotSupportedException();
        }

        public static Mapper2D<byte> CreateU8Instance(ImageAnalytics analytics, EdgeHandling edgeHandling, byte? constant = null)
        {

            if (analytics.PixelFormat == PixelFormats.Rgb24)
            {
                return new U8C3Mapper2D(analytics.PixelData, analytics.Width, analytics.Height, edgeHandling, constant);
            }
            else if (analytics.PixelFormat == PixelFormats.Gray8)
            {
                return new U8C1Mapper2D(analytics.PixelData, analytics.Width, analytics.Height, edgeHandling, constant);
            }

            throw new NotSupportedException();
        }
    }
}
