using System.Runtime.CompilerServices;

namespace CoreImageProcessor.Processing
{
    internal static class ParallelizationUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int StartIndex, int EndIndex)[] FragmentLength(int length, int maxDegreeOfFragmentation, bool smartFragmentation = true)
        {
            int fragments;

            if (smartFragmentation)
            {
                fragments = length / 10000;

                if (fragments > maxDegreeOfFragmentation)
                    fragments = maxDegreeOfFragmentation;
                else if (fragments < 1)
                    fragments = 1;
            }
            else
            {
                fragments = maxDegreeOfFragmentation;
            }

            (int StartIndex, int EndIndex)[] result = new (int, int)[fragments];

            int fragmentSize = (length + fragments - 1 ) / fragments;

            for(int i = 0, index = 0; i < fragments; ++i)
            {
                result[i].StartIndex = index;
                result[i].EndIndex = (index += fragmentSize); 
            }

            result[^1].EndIndex = length;
                
            return result;
        }
    }
}