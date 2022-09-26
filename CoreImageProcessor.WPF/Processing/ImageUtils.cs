using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Threading;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;

using static CoreImageProcessor.Processing.ImageAnalytics;
using System.Runtime.CompilerServices;

namespace CoreImageProcessor.Processing
{
    internal static class ImageUtils
    {
        //Because of the C# Syntax the filter kernels had to be defined transposed. That way Dimension 0 represents the X-Axis and Dimension 1 represents the Y-Axis.
        //PLEASE MAKE SURE YOU UNDERSTAND WHAT THE X AND Y INDICES ARE, BEFORE YOU CHANGE OR FALSELY "Correct" ANYTHING!!!
        public static readonly IReadOnlyDictionary<KernelType, IReadOnlyDictionary<KernelSize, float[,]>> SupportedFilterKernels = new Dictionary<KernelType, IReadOnlyDictionary<KernelSize, float[,]>>
        {
            {
                KernelType.Mean, new Dictionary<KernelSize, float[,]>
                {
                    {
                        KernelSize.KERNEL_SIZE_3_X_3, new float[,]
                        {
                            { 1f / 9f, 1f / 9f, 1f / 9f },
                            { 1f / 9f, 1f / 9f, 1f / 9f },
                            { 1f / 9f, 1f / 9f, 1f / 9f }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_3_X_1, new float[,]
                        {
                            { 1f / 3f },
                            { 1f / 3f },
                            { 1f / 3f }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_1_X_3, new float[,]
                        {
                            { 1f / 3f, 1f / 3f, 1f / 3f }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_5_X_5, new float[,]
                        {
                            { 1f / 25f, 1f / 25f, 1f / 25f, 1f / 25f, 1f / 25f },
                            { 1f / 25f, 1f / 25f, 1f / 25f, 1f / 25f, 1f / 25f },
                            { 1f / 25f, 1f / 25f, 1f / 25f, 1f / 25f, 1f / 25f },
                            { 1f / 25f, 1f / 25f, 1f / 25f, 1f / 25f, 1f / 25f },
                            { 1f / 25f, 1f / 25f, 1f / 25f, 1f / 25f, 1f / 25f }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_5_X_1, new float[,]
                        {
                            { 1f / 5f },
                            { 1f / 5f },
                            { 1f / 5f },
                            { 1f / 5f },
                            { 1f / 5f }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_1_X_5, new float[,]
                        {
                            { 1f / 5f, 1f / 5f, 1f / 5f, 1f / 5f, 1f / 5f }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_7_X_7, new float[,]
                        {
                            { 1f / 49f, 1f / 49f, 1f / 49f, 1f / 49f, 1f / 49f, 1f / 49f, 1f / 49f },
                            { 1f / 49f, 1f / 49f, 1f / 49f, 1f / 49f, 1f / 49f, 1f / 49f, 1f / 49f },
                            { 1f / 49f, 1f / 49f, 1f / 49f, 1f / 49f, 1f / 49f, 1f / 49f, 1f / 49f },
                            { 1f / 49f, 1f / 49f, 1f / 49f, 1f / 49f, 1f / 49f, 1f / 49f, 1f / 49f },
                            { 1f / 49f, 1f / 49f, 1f / 49f, 1f / 49f, 1f / 49f, 1f / 49f, 1f / 49f },
                            { 1f / 49f, 1f / 49f, 1f / 49f, 1f / 49f, 1f / 49f, 1f / 49f, 1f / 49f },
                            { 1f / 49f, 1f / 49f, 1f / 49f, 1f / 49f, 1f / 49f, 1f / 49f, 1f / 49f }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_9_X_9, new float[,]
                        {
                            { 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f },
                            { 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f },
                            { 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f },
                            { 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f },
                            { 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f },
                            { 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f },
                            { 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f },
                            { 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f },
                            { 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f, 1f / 81f }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_11_X_11, new float[,]
                        {
                            { 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f },
                            { 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f },
                            { 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f },
                            { 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f },
                            { 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f },
                            { 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f },
                            { 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f },
                            { 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f },
                            { 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f },
                            { 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f },
                            { 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f, 1f / 121f }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_13_X_13, new float[,]
                        {
                            { 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f },
                            { 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f },
                            { 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f },
                            { 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f },
                            { 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f },
                            { 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f },
                            { 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f },
                            { 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f },
                            { 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f },
                            { 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f },
                            { 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f },
                            { 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f },
                            { 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f, 1f / 169f }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_15_X_15, new float[,]
                        {
                            { 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f  },
                            { 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f  },
                            { 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f  },
                            { 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f  },
                            { 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f  },
                            { 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f  },
                            { 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f  },
                            { 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f  },
                            { 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f  },
                            { 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f  },
                            { 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f  },
                            { 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f  },
                            { 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f  },
                            { 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f  },
                            { 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f, 1f / 225f  }
                        }
                    }
                }
            },
            {
                //source: http://dev.theomader.com/gaussian-kernel-calculator/ (sigma = 1)
                KernelType.Gaussian, new Dictionary<KernelSize, float[,]>
                {
                    {
                        KernelSize.KERNEL_SIZE_3_X_3, new float[,]
                        {
                            { 0.077847f, 0.123317f, 0.077847f },
                            { 0.123317f, 0.195346f, 0.123317f },
                            { 0.077847f, 0.123317f, 0.077847f }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_3_X_1, new float[,]
                        {
                            { 0.27901f },
                            { 0.44198f },
                            { 0.27901f }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_1_X_3, new float[,]
                        {
                            { 0.27901f, 0.44198f, 0.27901f }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_5_X_5, new float[,]
                        {
                            { 0.003765f, 0.015019f, 0.023792f, 0.015019f, 0.003765f },
                            { 0.015019f, 0.059912f, 0.094907f, 0.059912f, 0.015019f },
                            { 0.023792f, 0.094907f, 0.150342f, 0.094907f, 0.023792f },
                            { 0.015019f, 0.059912f, 0.094907f, 0.059912f, 0.015019f },
                            { 0.003765f, 0.015019f, 0.023792f, 0.015019f, 0.003765f },
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_5_X_1, new float[,]
                        {
                            { 0.06136f },
                            { 0.24477f },
                            { 0.38774f },
                            { 0.24477f },
                            { 0.06136f }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_1_X_5, new float[,]
                        {
                            { 0.06136f, 0.24477f, 0.38774f, 0.24477f, 0.06136f }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_7_X_7, new float[,]
                        {
                            { 0.000036f, 0.000363f, 0.001446f, 0.002291f, 0.001446f, 0.000363f, 0.000036f },
                            { 0.000363f, 0.003676f, 0.014662f, 0.023226f, 0.014662f, 0.003676f, 0.000363f },
                            { 0.001446f, 0.014662f, 0.058488f, 0.092651f, 0.058488f, 0.014662f, 0.001446f },
                            { 0.002291f, 0.023226f, 0.092651f, 0.146768f, 0.092651f, 0.023226f, 0.002291f },
                            { 0.001446f, 0.014662f, 0.058488f, 0.092651f, 0.058488f, 0.014662f, 0.001446f },
                            { 0.000363f, 0.003676f, 0.014662f, 0.023226f, 0.014662f, 0.003676f, 0.000363f },
                            { 0.000036f, 0.000363f, 0.001446f, 0.002291f, 0.001446f, 0.000363f, 0.000036f },
                        }
                    }
                }
            },
            {
                KernelType.Laplace, new Dictionary<KernelSize, float[,]>
                {
                    {
                        KernelSize.KERNEL_SIZE_3_X_3, new float[,]
                        {
                            { 0f, 1f, 0f },
                            { 1f,-4f, 1f },
                            { 0f, 1f, 0f }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_3_X_1, new float[,]
                        {
                            { 1f, -2f, 1f }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_1_X_3, new float[,]
                        {
                            {  1f },
                            { -2f },
                            {  1f }
                        }
                    }
                }
            },
            {
                KernelType.Sobel_Top, new Dictionary<KernelSize, float[,]>
                {
                    {
                        KernelSize.KERNEL_SIZE_3_X_3, new float[,]
                        {
                            { 1f, 0f,-1f },
                            { 2f, 0f,-2f },
                            { 1f, 0f,-1f }  
                        }
                    }
                }
            },
            {
                KernelType.Sobel_Bottom, new Dictionary<KernelSize, float[,]>
                {
                    {
                        KernelSize.KERNEL_SIZE_3_X_3, new float[,]
                        {
                            { -1f, 0f, 1f },
                            { -2f, 0f, 2f },
                            { -1f, 0f, 1f }
                        }
                    }
                }
            },
            {
                KernelType.Sobel_Left, new Dictionary<KernelSize, float[,]>
                {
                    {
                        KernelSize.KERNEL_SIZE_3_X_3, new float[,]
                        {
                            {  1f, 2f, 1f },
                            {  0f, 0f, 0f },
                            { -1f,-2f,-1f }
                        }
                    }
                }
            },
            {
                KernelType.Sobel_Right, new Dictionary<KernelSize, float[,]>
                {
                    {
                        KernelSize.KERNEL_SIZE_3_X_3, new float[,]
                        {
                            { -1f,-2f,-1f },
                            {  0f, 0f, 0f },
                            {  1f, 2f, 1f }
                        }
                    }
                }
            },
            {
                KernelType.Emboss, new Dictionary<KernelSize, float[,]>
                {
                    {
                        KernelSize.KERNEL_SIZE_3_X_3, new float[,]
                        {
                            { -2f,-1f, 0f },
                            { -1f, 1f, 1f },
                            {  0f, 1f, 2f }
                        }
                    }
                }
            },
            {
                KernelType.Outline, new Dictionary<KernelSize, float[,]>
                {
                    {
                        KernelSize.KERNEL_SIZE_3_X_3, new float[,]
                        {
                            { -1f,-1f,-1f },
                            { -1f, 8f,-1f },
                            { -1f,-1f,-1f }
                        }
                    }
                }
            },
            {
                KernelType.Sharpen, new Dictionary<KernelSize, float[,]>
                {
                    {
                        KernelSize.KERNEL_SIZE_3_X_3, new float[,]
                        {
                            {  0f,-1f, 0f },
                            { -1f, 5f,-1f },
                            {  0f,-1f, 0f }
                        }
                    }
                }
            }
        };

        public static readonly IReadOnlyDictionary<KernelShape, IReadOnlyDictionary<KernelSize, bool[,]>> SupportedMorphologicalFilterKernels = new Dictionary<KernelShape, IReadOnlyDictionary<KernelSize, bool[,]>>
        {
            {
                KernelShape.Square, new Dictionary<KernelSize, bool[,]>
                {
                    {
                        KernelSize.KERNEL_SIZE_3_X_3, new bool[,]
                        {
                            { true, true, true },
                            { true, true, true },
                            { true, true, true }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_5_X_5, new bool[,]
                        {
                            { true, true, true, true, true },
                            { true, true, true, true, true },
                            { true, true, true, true, true },
                            { true, true, true, true, true },
                            { true, true, true, true, true }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_7_X_7, new bool[,]
                        {
                            { true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_9_X_9, new bool[,]
                        {
                            { true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_11_X_11, new bool[,]
                        {
                            { true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_13_X_13, new bool[,]
                        {
                            { true, true, true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true, true, true }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_15_X_15, new bool[,]
                        {
                            { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true },
                            { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true }
                        }
                    }
                }
            },
            {
                KernelShape.Cross, new Dictionary<KernelSize, bool[,]>
                {
                    {
                        KernelSize.KERNEL_SIZE_3_X_3, new bool[,]
                        {
                            { false, true, false },
                            { true , true, true  },
                            { false, true, false }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_5_X_5, new bool[,]
                        {
                            { false, false, true, false, false },
                            { false, false, true, false, false },
                            { true , true , true, true , true  },
                            { false, false, true, false, false },
                            { false, false, true, false, false }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_7_X_7, new bool[,]
                        {
                            { false, false, false, true, false, false, false },
                            { false, false, false, true, false, false, false },
                            { false, false, false, true, false, false, false },
                            { true , true , true , true, true , true , true  },
                            { false, false, false, true, false, false, false },
                            { false, false, false, true, false, false, false },
                            { false, false, false, true, false, false, false }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_9_X_9, new bool[,]
                        {
                            { false, false, false, false, true, false, false, false, false },
                            { false, false, false, false, true, false, false, false, false },
                            { false, false, false, false, true, false, false, false, false },
                            { false, false, false, false, true, false, false, false, false },
                            { true , true , true , true , true, true , true , true , true  },
                            { false, false, false, false, true, false, false, false, false },
                            { false, false, false, false, true, false, false, false, false },
                            { false, false, false, false, true, false, false, false, false },
                            { false, false, false, false, true, false, false, false, false }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_11_X_11, new bool[,]
                        {
                            { false, false, false, false, false, true, false, false, false, false, false },
                            { false, false, false, false, false, true, false, false, false, false, false },
                            { false, false, false, false, false, true, false, false, false, false, false },
                            { false, false, false, false, false, true, false, false, false, false, false },
                            { false, false, false, false, false, true, false, false, false, false, false },
                            { true , true , true , true , true , true, true , true , true , true , true  },
                            { false, false, false, false, false, true, false, false, false, false, false },
                            { false, false, false, false, false, true, false, false, false, false, false },
                            { false, false, false, false, false, true, false, false, false, false, false },
                            { false, false, false, false, false, true, false, false, false, false, false },
                            { false, false, false, false, false, true, false, false, false, false, false }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_13_X_13, new bool[,]
                        {
                            { false, false, false, false, false, false, true, false, false, false, false, false, false },
                            { false, false, false, false, false, false, true, false, false, false, false, false, false },
                            { false, false, false, false, false, false, true, false, false, false, false, false, false },
                            { false, false, false, false, false, false, true, false, false, false, false, false, false },
                            { false, false, false, false, false, false, true, false, false, false, false, false, false },
                            { false, false, false, false, false, false, true, false, false, false, false, false, false },
                            { true , true , true , true , true , true , true, true , true , true , true , true , true  },
                            { false, false, false, false, false, false, true, false, false, false, false, false, false },
                            { false, false, false, false, false, false, true, false, false, false, false, false, false },
                            { false, false, false, false, false, false, true, false, false, false, false, false, false },
                            { false, false, false, false, false, false, true, false, false, false, false, false, false },
                            { false, false, false, false, false, false, true, false, false, false, false, false, false },
                            { false, false, false, false, false, false, true, false, false, false, false, false, false }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_15_X_15, new bool[,]
                        {
                            { false, false, false, false, false, false, false, true, false, false, false, false, false, false, false },
                            { false, false, false, false, false, false, false, true, false, false, false, false, false, false, false },
                            { false, false, false, false, false, false, false, true, false, false, false, false, false, false, false },
                            { false, false, false, false, false, false, false, true, false, false, false, false, false, false, false },
                            { false, false, false, false, false, false, false, true, false, false, false, false, false, false, false },
                            { false, false, false, false, false, false, false, true, false, false, false, false, false, false, false },
                            { false, false, false, false, false, false, false, true, false, false, false, false, false, false, false },
                            { true , true , true , true , true , true , true , true, true , true , true , true , true , true , true  },
                            { false, false, false, false, false, false, false, true, false, false, false, false, false, false, false },
                            { false, false, false, false, false, false, false, true, false, false, false, false, false, false, false },
                            { false, false, false, false, false, false, false, true, false, false, false, false, false, false, false },
                            { false, false, false, false, false, false, false, true, false, false, false, false, false, false, false },
                            { false, false, false, false, false, false, false, true, false, false, false, false, false, false, false },
                            { false, false, false, false, false, false, false, true, false, false, false, false, false, false, false },
                            { false, false, false, false, false, false, false, true, false, false, false, false, false, false, false }
                        }
                    }
                }
            },
            {
                KernelShape.Diamond, new Dictionary<KernelSize, bool[,]>
                {
                    {
                        KernelSize.KERNEL_SIZE_3_X_3, new bool[,]
                        {
                            { false, true, false },
                            { true , true, true  },
                            { false, true, false }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_5_X_5, new bool[,]
                        {
                            { false, false, true, false, false },
                            { false, true , true, true , false },
                            { true , true , true, true , true  },
                            { false, true , true, true , false },
                            { false, false, true, false, false }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_7_X_7, new bool[,]
                        {
                            { false, false, false, true, false, false, false },
                            { false, false, true , true, true , false, false },
                            { false, true , true , true, true , true , false },
                            { true , true , true , true, true , true , true  },
                            { false, true , true , true, true , true , false },
                            { false, false, true , true, true , false, false },
                            { false, false, false, true, false, false, false }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_9_X_9, new bool[,]
                        {
                            { false, false, false, false, true, false, false, false, false },
                            { false, false, false, true , true, true , false, false, false },
                            { false, false, true , true , true, true , true , false, false },
                            { false, true , true , true , true, true , true , true , false },
                            { true , true , true , true , true, true , true , true , true  },
                            { false, true , true , true , true, true , true , true , false },
                            { false, false, true , true , true, true , true , false, false },
                            { false, false, false, true , true, true , false, false, false },
                            { false, false, false, false, true, false, false, false, false }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_11_X_11, new bool[,]
                        {
                            { false, false, false, false, false, true, false, false, false, false, false },
                            { false, false, false, false, true , true, true , false, false, false, false },
                            { false, false, false, true , true , true, true , true , false, false, false },
                            { false, false, true , true , true , true, true , true , true , false, false },
                            { false, true , true , true , true , true, true , true , true , true , false },
                            { true , true , true , true , true , true, true , true , true , true , true  },
                            { false, true , true , true , true , true, true , true , true , true , false },
                            { false, false, true , true , true , true, true , true , true , false, false },
                            { false, false, false, true , true , true, true , true , false, false, false },
                            { false, false, false, false, true , true, true , false, false, false, false },
                            { false, false, false, false, false, true, false, false, false, false, false }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_13_X_13, new bool[,]
                        {
                            { false, false, false, false, false, false, true, false, false, false, false, false, false },
                            { false, false, false, false, false, true , true, true , false, false, false, false, false },
                            { false, false, false, false, true , true , true, true , true , false, false, false, false },
                            { false, false, false, true , true , true , true, true , true , true , false, false, false },
                            { false, false, true , true , true , true , true, true , true , true , true , false, false },
                            { false, true , true , true , true , true , true, true , true , true , true , true , false },
                            { true , true , true , true , true , true , true, true , true , true , true , true , true  },
                            { false, true , true , true , true , true , true, true , true , true , true , true , false },
                            { false, false, true , true , true , true , true, true , true , true , true , false, false },
                            { false, false, false, true , true , true , true, true , true , true , false, false, false },
                            { false, false, false, false, true , true , true, true , true , false, false, false, false },
                            { false, false, false, false, false, true , true, true , false, false, false, false, false },
                            { false, false, false, false, false, false, true, false, false, false, false, false, false }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_15_X_15, new bool[,]
                        {
                            { false, false, false, false, false, false, false, true, false, false, false, false, false, false, false },
                            { false, false, false, false, false, false, true , true, true , false, false, false, false, false, false },
                            { false, false, false, false, false, true , true , true, true , true , false, false, false, false, false },
                            { false, false, false, false, true , true , true , true, true , true , true , false, false, false, false },
                            { false, false, false, true , true , true , true , true, true , true , true , true , false, false, false },
                            { false, false, true , true , true , true , true , true, true , true , true , true , true , false, false },
                            { false, true , true , true , true , true , true , true, true , true , true , true , true , true , false },
                            { true , true , true , true , true , true , true , true, true , true , true , true , true , true , true  },
                            { false, true , true , true , true , true , true , true, true , true , true , true , true , true , false },
                            { false, false, true , true , true , true , true , true, true , true , true , true , true , false, false },
                            { false, false, false, true , true , true , true , true, true , true , true , true , false, false, false },
                            { false, false, false, false, true , true , true , true, true , true , true , false, false, false, false },
                            { false, false, false, false, false, true , true , true, true , true , false, false, false, false, false },
                            { false, false, false, false, false, false, true , true, true , false, false, false, false, false, false },
                            { false, false, false, false, false, false, false, true, false, false, false, false, false, false, false }
                        }
                    }
                }
            },
            {
                KernelShape.HorizontalLine, new Dictionary<KernelSize, bool[,]>
                {
                    //The kernels are smaller than the KERNEL_SIZE_<WIDTH>_X_<HEIGHT> inidcates to accelerate the image processing.
                    {
                        KernelSize.KERNEL_SIZE_3_X_3, new bool[,]
                        {
                            { true }, 
                            { true },
                            { true }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_5_X_5, new bool[,]
                        {
                            { true },
                            { true },
                            { true },
                            { true },
                            { true }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_7_X_7, new bool[,]
                        {
                            { true },
                            { true },
                            { true },
                            { true },
                            { true },
                            { true },
                            { true }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_9_X_9, new bool[,]
                        {
                            { true },
                            { true },
                            { true },
                            { true },
                            { true },
                            { true },
                            { true },
                            { true },
                            { true }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_11_X_11, new bool[,]
                        {
                            { true },
                            { true },
                            { true },
                            { true },
                            { true },
                            { true },
                            { true },
                            { true },
                            { true },
                            { true },
                            { true }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_13_X_13, new bool[,]
                        {
                            { true },
                            { true },
                            { true },
                            { true },
                            { true },
                            { true },
                            { true },
                            { true },
                            { true },
                            { true },
                            { true },
                            { true },
                            { true }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_15_X_15, new bool[,]
                        {
                            { true },
                            { true },
                            { true },
                            { true },
                            { true },
                            { true },
                            { true },
                            { true },
                            { true },
                            { true },
                            { true },
                            { true },
                            { true },
                            { true },
                            { true }
                        }
                    }
                }
            },
            {
                KernelShape.VerticalLine, new Dictionary<KernelSize, bool[,]>
                {
                    //The kernels are smaller than the KERNEL_SIZE_<WIDTH>_X_<HEIGHT> inidcates to accelerate the image processing.
                    {
                        KernelSize.KERNEL_SIZE_3_X_3, new bool[,]
                        {
                            { true, true, true }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_5_X_5, new bool[,]
                        {
                            { true, true, true, true, true }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_7_X_7, new bool[,]
                        {
                            { true, true, true, true, true, true, true }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_9_X_9, new bool[,]
                        {
                            { true, true, true, true, true, true, true, true, true }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_11_X_11, new bool[,]
                        {
                            { true, true, true, true, true, true, true, true, true, true, true }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_13_X_13, new bool[,]
                        {
                            { true, true, true, true, true, true, true, true, true, true, true, true, true }
                        }
                    },
                    {
                        KernelSize.KERNEL_SIZE_15_X_15, new bool[,]
                        {
                            { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true }
                        }
                    }
                }
            }
        };

        private static readonly float[,] EmptyFilterKernel = new float[0, 0];
        private static readonly bool[,] EmptyMorphologicalFilterKernel = new bool[0,0];

        public static bool TryGetFilterKernel(KernelType type, KernelSize size, out float[,] kernel)
        {
            if (SupportedFilterKernels.TryGetValue(type, out var kernels) && kernels.TryGetValue(size, out float[,]? value) && value != null)
            {
                kernel = value;
                return true;
            }

            kernel = EmptyFilterKernel;
            return false;
        }

        public static bool TryGetMorphologicalFilterKernel(KernelShape shape, KernelSize size, out bool[,] kernel)
        {
            if (SupportedMorphologicalFilterKernels.TryGetValue(shape, out var kernels) && kernels.TryGetValue(size, out bool[,]? value) && value != null)
            {
                kernel = value;
                return true;
            }

            kernel = EmptyMorphologicalFilterKernel;
            return false;
        }

        public static ImageSource DrawHistogram(ImageAnalytics analytics, ColorChannel colorChannels = ColorChannel.Red | ColorChannel.Green | ColorChannel.Blue | ColorChannel.Gray, CancellationToken cancellationToken = default)
        {
            if (analytics.BitsPerChannel != 8)
                throw new NotSupportedException("This method currently only supports drawing histograms for 8-Bit/Channel images.");

            const int widthMultiple = 8;

            const int height = 1024;
            const int width = 256 * widthMultiple;

            byte[] histogramImage = new byte[height * width * 4];

            int count = 0;
            double mean = 0d;

            foreach (ColorChannelAnalytics colorChannel in analytics.ColorChannels.Values)
            {
                if (((byte)colorChannels & (byte)colorChannel.Type) == 0)
                    continue;

                count += colorChannel.Histogram.Length;

                foreach (double value in colorChannel.Histogram)
                {
                    mean += value;
                }
            }

            mean /= count;

            double standardDeviation = 0d;

            foreach (ColorChannelAnalytics colorChannel in analytics.ColorChannels.Values)
            {
                if (((byte)colorChannels & (byte)colorChannel.Type) == 0)
                    continue;

                foreach (double value in colorChannel.Histogram)
                {
                    standardDeviation += Math.Pow(value - mean, 2);
                }
            }

            standardDeviation = Math.Sqrt(standardDeviation / (count - 1));

            foreach (ColorChannelAnalytics colorChannel in analytics.ColorChannels.Values)
            {
                if (((byte)colorChannels & (byte)colorChannel.Type) == 0)
                    continue;

                double[] histogram = colorChannel.Histogram;

                byte r = 0;
                byte g = 0;
                byte b = 0;

                if (colorChannel.Type == ColorChannel.Red)
                    r = byte.MaxValue;
                else if (colorChannel.Type == ColorChannel.Green)
                    g = byte.MaxValue;
                else if (colorChannel.Type == ColorChannel.Blue)
                    b = byte.MaxValue;
                else if (colorChannel.Type == ColorChannel.Gray)
                    r = g = b = 170;
                else
                    throw new NotSupportedException($"Color-Channel '{colorChannel.Type}' is not Supported.");

                for (int color = 0; color < histogram.Length; ++color)
                {
                    double histValue_d = histogram[color] / (mean + 2 * standardDeviation);
                    if (histValue_d > 1D)
                        histValue_d = 1D;

                    int histValue = (int)Math.Round(histValue_d * (height - 1)); 

                    int i = color * widthMultiple * 4 + (width * 4) * ((height - 1) - histValue);

                    for (int n = 0; n <= histValue; i += width * 4, ++n)
                    {
                        for (int x = i, k = 0; k < widthMultiple; ++k)
                        {
                            
                            histogramImage[x++] |= b;
                            histogramImage[x++] |= g;
                            histogramImage[x++] |= r;
                            histogramImage[x++] = byte.MaxValue; //alpha channel
                        }
                    }
                }
            }

            return BitmapSource.Create(width, height, 300d, 300d, PixelFormats.Bgra32, null, histogramImage, histogramImage.Length / height);
        }

        public static Task<ImageSource> DrawHistogramAsync(ImageAnalytics analytics, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => DrawHistogram(analytics, cancellationToken: cancellationToken));
        }

        public static async Task<byte[]> GetPixelDataAsync(this Image<Rgb24> image)
        {
            static void TaskKernel(Image<Rgb24> img, byte[] data, int startIndex, int endIndex)
            {
                int x = (startIndex / 3) % img.Width;
                int y = (startIndex / 3) / img.Width;

                int i = startIndex;

                for (; i < endIndex ; ++y)
                {
                    for (; i < endIndex && x < img.Width; ++x)
                    {
                        Rgb24 value = img[x, y];

                        data[i++] = value.R;
                        data[i++] = value.G;
                        data[i++] = value.B;
                    }

                    x = 0;
                }
            }
            
            int pixels = image.Width * image.Height;
            byte[] pixelData = new byte[pixels * 3];

            var fragments = ParallelizationUtils.FragmentLength(length: pixels, maxDegreeOfFragmentation: App.Settings.ThreadLimit);
            var tasks = new ConfiguredTaskAwaitable[fragments.Length];

            for (int i = 0; i < tasks.Length; ++i)
            {
                int iCopy = i;
                tasks[i] = Task.Run(() => TaskKernel(image, pixelData, fragments[iCopy].StartIndex * 3, fragments[iCopy].EndIndex * 3)).ConfigureAwait(false);
            }

            foreach (var task in tasks)
                await task;

            return pixelData;
        }

        public static ImageSource ToImageSource(this ImageAnalytics analytics, bool toUriSource = false)
        {
            if (analytics.FilePath != null && toUriSource)
            {
                try
                {
                    return new BitmapImage(new Uri(analytics.FilePath));
                }
                catch (FileNotFoundException)
                {
                    //Do Nothing
                }
            }

            int stride;

            if (analytics.PixelFormat == PixelFormats.Rgb24)
                stride = analytics.Width * 3;
            else if (analytics.PixelFormat == PixelFormats.Gray8)
                stride = analytics.Width;
            else
                throw new NotSupportedException($"Pixel format '{analytics.PixelFormat}' is not supported yet.");

            return BitmapSource.Create(analytics.Width, analytics.Height, 96d, 96d, analytics.PixelFormat, null, analytics.PixelData,  stride);
        }

        
    }
}
