using System.ComponentModel;

namespace CoreImageProcessor.Processing
{
    public enum KernelSize : ushort
    {
        [Description("1 x 3")]
        KERNEL_SIZE_1_X_3 = 0x00_01,

        [Description("1 x 5")]
        KERNEL_SIZE_1_X_5 = 0x00_02,

        [Description("3 x 1")]
        KERNEL_SIZE_3_X_1 = 0x00_04,

        [Description("5 x 1")]
        KERNEL_SIZE_5_X_1 = 0x00_08,

        [Description("3 x 3")]
        KERNEL_SIZE_3_X_3 = 0x00_10,

        [Description("5 x 5")]
        KERNEL_SIZE_5_X_5 = 0x00_20,

        [Description("7 x 7")]
        KERNEL_SIZE_7_X_7 = 0x00_40,

        [Description("9 x 9")]
        KERNEL_SIZE_9_X_9 = 0x00_80,

        [Description("11 x 11")]
        KERNEL_SIZE_11_X_11 = 0x01_00,

        [Description("13 x 13")]
        KERNEL_SIZE_13_X_13 = 0x02_00,

        [Description("15 x 15")]
        KERNEL_SIZE_15_X_15 = 0x04_00
    }
}
