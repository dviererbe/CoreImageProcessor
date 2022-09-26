using System;
using System.Collections.Generic;
using System.Text;

namespace CoreImageProcessor.Processing
{
    internal class U8C3Mapper2D : Mapper2D<byte>
    {
        public U8C3Mapper2D(byte[] source, int width, int height, EdgeHandling edgeHandling, byte? constant = null)
            : base(source, width, height, edgeHandling, constant)
        {
        }

        public override int Channels => 3;

        protected override byte[] GetValue(int x, int y)
        {
            int index = (x + y * Width) * 3;

            return new byte[] { Source[index], Source[index + 1], Source[index + 2] };
        }

        protected override void SetValue(int x, int y, byte[] value)
        {
            if (value.Length != 3)
                throw new ArgumentException($"Value has to contain exactly 3 channels! {value.Length} Channels found.", nameof(value));

            int index = (x + y * Width) * 3;

            Source[index++] = value[0];
            Source[index++] = value[1];
            Source[index] = value[2];
        }
    }
}
