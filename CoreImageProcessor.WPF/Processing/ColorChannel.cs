using System;
using System.Collections.Generic;
using System.Text;

namespace CoreImageProcessor.Processing
{
    internal enum ColorChannel : byte
    {
        Red   = 0b0001,
        Green = 0b0010,
        Blue  = 0b0100,
        Gray  = 0b1000
    }
}
