using System;
using System.Collections.Generic;
using System.Text;

namespace CoreImageProcessor.Processing
{
    internal class U8C1Mapper2D : Mapper2D<byte>
    {
        public U8C1Mapper2D(byte[] source, int width, int height, EdgeHandling edgeHandling, byte? constant = null)
            : base(source, width, height, edgeHandling, constant)
        {
        }

        public override int Channels => 1;

        protected override byte[] GetValue(int x, int y) => new byte[] { Source[x + y * Width] };

        protected override void SetValue(int x, int y, byte[] value)
        {
            if (value.Length != 1)
                throw new ArgumentException($"Value has to contain exactly 1 channel! {value.Length} Channels found.", nameof(value));

            Source[x + y * Width] = value[0];
        }
    }
}
