using System;
using System.Collections.Generic;
using System.Text;

namespace CoreImageProcessor
{
    internal class AppSettings
    {
        public static readonly int MaxThreadLimit = Environment.ProcessorCount;
        public static readonly AppSettings Default = new AppSettings(false, Environment.ProcessorCount);
        
        public AppSettings(bool useCuda, int threadLimit)
        {
            if (threadLimit < 1)
                throw new ArgumentOutOfRangeException(nameof(threadLimit), threadLimit, "Thread limit can't be smaller than one!");

            UseCUDA = useCuda;
            ThreadLimit = threadLimit > MaxThreadLimit ? MaxThreadLimit : threadLimit;
        }

        public bool UseCUDA
        {
            get;
        }

        public int ThreadLimit
        {
            get;
        }
    }
}
