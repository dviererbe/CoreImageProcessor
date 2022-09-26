using System;
using System.Collections.Generic;
using System.Text;

namespace CoreImageProcessor.Processing
{
    public interface IRankOrderStateMachine<T> where T : struct
    {
        bool FinishedEarly { get; }

        void AddValue(T value);

        void Reset();

        T GetResult(T value);
    }
}
