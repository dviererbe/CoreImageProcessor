namespace CoreImageProcessor.Processing
{
    public class U8MaximumRankOrderStateMachine : IRankOrderStateMachine<byte>
    {
        private byte _Maximum = byte.MinValue;

        public bool FinishedEarly => _Maximum == byte.MaxValue;

        public void AddValue(byte value)
        {
            if (value > _Maximum)
                _Maximum = value;
        }

        public byte GetResult(byte value)
        {
            return _Maximum;
        }

        public void Reset()
        {
            _Maximum = byte.MinValue;
        }
    }
}
