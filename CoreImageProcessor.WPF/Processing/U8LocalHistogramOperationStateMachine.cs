using System;
using System.Collections.Generic;
using System.Text;

namespace CoreImageProcessor.Processing
{
    public class U8LocalHistogramOperationStateMachine : IRankOrderStateMachine<byte>
    {
        int _Sum = 0;
        private int _Count = 0;
        

        public bool FinishedEarly => false;

        public void AddValue(byte value)
        {
            ++_Count;
            _Sum += value;
        }

        public byte GetResult(byte value)
        {
            double result = _Sum / (double)_Count;
            result = value + ((127.5 - result) / 127.5) * 20d;

            if (result > byte.MaxValue)
                return byte.MaxValue;
            else if (result < byte.MinValue)
                return byte.MinValue;
            else
                return (byte)Math.Round(result);
        }

        public void Reset()
        {
            _Sum = 0;
            _Count = 0;
        }
    }
}
