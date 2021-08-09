using System;
using System.Collections.Generic;

namespace IngameScript
{
    public readonly struct RxEvent : IEquatable<RxEvent>
    {
        private RxEvent(RxEventKind kind, object value, Exception error)
        {
            Kind = kind;
            Value = value;
            Error = error;
        }

        public static RxEvent OnNext(object value) => new RxEvent(RxEventKind.OnNext, value, null);
        public static RxEvent OnError(Exception error) => new RxEvent(RxEventKind.OnError, null, error);
        public static RxEvent OnCompleted => new RxEvent(RxEventKind.OnCompleted, null, null);

        public RxEventKind Kind { get; }
        public object Value { get; }
        public Exception Error { get; }

        public static bool Equals(RxEvent left, RxEvent right)
        {
            return left.Kind == right.Kind &&
                EqualityComparer<object>.Default.Equals(left.Value, right.Value) &&
                EqualityComparer<Exception>.Default.Equals(left.Error, right.Error);
        }
        public static bool operator ==(RxEvent left, RxEvent right) => Equals(left, right);
        public static bool operator !=(RxEvent left, RxEvent right) => !Equals(left, right);
        public bool Equals(RxEvent other) => Equals(this, other);
        public override bool Equals(object obj) => obj is RxEvent other && Equals(this, other);
        public override int GetHashCode() => (Kind, Value, Error).GetHashCode();

        public override string ToString()
        {
            return Kind switch
            {
                RxEventKind.OnNext => $"{{{Value}}}",
                RxEventKind.OnError => $"!{Error}!",
                RxEventKind.OnCompleted => $"Completed",
                _ => $"Unknown event kind"
            };
        }
    }
}
