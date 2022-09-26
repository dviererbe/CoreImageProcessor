using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CoreImageProcessor.Processing
{
    public enum KernelType : ushort
    {
        Mean = 0x00_01,

        Gaussian = 0x00_02,

        Laplace = 0x00_04,

        [Description("Bottom Sobel")]
        Sobel_Bottom = 0x00_08,

        [Description("Top Sobel")]
        Sobel_Top = 0x00_10,

        [Description("Left Sobel")]
        Sobel_Left = 0x00_20,

        [Description("Right Sobel")]
        Sobel_Right = 0x00_40,

        Emboss = 0x00_80,

        Outline = 0x01_00,

        Sharpen = 0x02_00,
    }
}
