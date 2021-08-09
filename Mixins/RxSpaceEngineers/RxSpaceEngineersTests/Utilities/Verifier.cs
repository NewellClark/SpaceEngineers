using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static IngameScript.Program;

namespace IngameScript.Utilities
{
    sealed class Verifier<T>
    {
        private IDisposable _subscription;
        private TestObserver<T> _observer;

        private IRxObservable<T> _Observable;
        public IRxObservable<T> Observable
        {
            get => _Observable;
            set
            {
                _Observable = value;
                _subscription?.Dispose();
                _observer = new();
                _subscription = value.Subscribe(_observer);
            }
        }

        public List<RxEvent> Expected { get; } = new();

        public void Unsubscribe() => _subscription?.Dispose();

        public void Verify() => Assert.Equal(Expected.AsEnumerable(), _observer?.Events);
    }
}
