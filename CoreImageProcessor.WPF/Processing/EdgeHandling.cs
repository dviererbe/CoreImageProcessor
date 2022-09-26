using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CoreImageProcessor.Processing
{
    public enum EdgeHandling : byte
    {
        /// <summary>
        /// The filter kernel will not process the edge pixels of the input image that require values from beyond the edge.
        /// </summary>
        [Description("The filter kernel will not process the edge pixels of the input image that require values from beyond the edge.")]
        None = 0x01,

        /// <summary>
        /// The filter kernel will use a specified constant for pixel values from beyond the edge.
        /// </summary>
        [Description("The filter kernel will use a specified constant for pixel values from beyond the edge.")]
        Constant = 0x02,

        /// <summary>
        /// The nearest border pixels are conceptually extended as far as necessary to provide values for the filter kernel. Corner pixels are extended in 90° wedges. Other edge pixels are extended in lines.
        /// </summary>
        [Description("The nearest border pixels are conceptually extended as far as necessary to provide values for the filter kernel. Corner pixels are extended in 90° wedges. Other edge pixels are extended in lines.")]
        Extend = 0x04,

        /// <summary>
        /// The image is conceptually wrapped (or tiled) and values are taken from the opposite edge or corner.
        /// </summary>
        [Description("The image is conceptually wrapped (or tiled) and values are taken from the opposite edge or corner.")]
        Wrap = 0x08,

        /// <summary>
        /// The image is conceptually mirrored at the edges. For example, attempting to read a pixel 3 units outside an edge reads one 3 units inside the edge instead.
        /// </summary>
        [Description("The image is conceptually mirrored at the edges. For example, attempting to read a pixel 3 units outside an edge reads one 3 units inside the edge instead.")]
        Mirror = 0x10,

        /// <summary>
        /// Any pixel in the kernel that extends past the input image isn't used.
        /// </summary>
        [Description("Any pixel in the kernel that extends past the input image isn't used.")]
        KernelCrop = 0x20
    }
}
