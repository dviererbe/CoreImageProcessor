namespace CoreImageProcessor.Processing
{
    public class U8MinimumRankOrderStateMachine : IRankOrderStateMachine<byte>
    {
        private byte _Minimum = byte.MaxValue;

        public bool FinishedEarly => _Minimum == byte.MinValue;

        public void AddValue(byte value)
        {
            if (value < _Minimum)
                _Minimum = value;
        }

        public byte GetResult(byte value)
        {
            return _Minimum;
        }

        public void Reset()
        {
            _Minimum = byte.MaxValue;
        }
    }
}
