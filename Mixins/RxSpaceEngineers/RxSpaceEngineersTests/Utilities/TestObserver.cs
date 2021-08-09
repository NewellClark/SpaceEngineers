using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IngameScript.Program;

namespace IngameScript
{
    public sealed class TestObserver<T> : IRxObserver<T>
    {
        public int CompletionCount { get; private set; }

        private readonly List<T> _Values = new();
        public IReadOnlyList<T> Values => _Values;

        private readonly List<Exception> _Errors = new();
        public IReadOnlyList<Exception> Errors => _Errors;

        private readonly List<RxEvent> _Events = new();
        public IReadOnlyList<RxEvent> Events => _Events;

        public void OnNext(T value)
        {
            _Events.Add(RxEvent.OnNext(value));
            _Values.Add(value);
        }
        public void OnError(Exception error)
        {
            _Events.Add(RxEvent.OnError(error));
            _Errors.Add(error);
        }
        public void OnCompleted()
        {
            _Events.Add(RxEvent.OnCompleted);
            CompletionCount++;
        }
    }
}
