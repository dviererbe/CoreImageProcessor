using System.Collections.Generic;

namespace CoreImageProcessor.Processing
{
    public class U8MedianRankOrderStateMachine : IRankOrderStateMachine<byte>
    {
        private List<byte> _Values = new List<byte>();

        public bool FinishedEarly => false;

        public void AddValue(byte value)
        {
            _Values.Add(value);
        }

        public byte GetResult(byte value)
        {
            _Values.Sort();

            if (_Values.Count % 2 == 0)
            {
                return (byte)((_Values[_Values.Count / 2 - 1] + _Values[_Values.Count / 2]) / 2);
            }
            else
            {
                return _Values[_Values.Count / 2];
            }
        }

        public void Reset()
        {
            _Values.Clear();
        }
    }
}
