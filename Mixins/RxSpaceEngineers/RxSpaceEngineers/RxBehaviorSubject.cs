using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        /// <summary>
        /// Forwards the most recent value to new subscribers. 
        /// </summary>
        public sealed class RxBehaviorSubject<T> : IRxObservable<T>, IRxObserver<T>
        {
            private readonly RxSubject<T> _subject;

            public RxBehaviorSubject()
                : this(default(T))
            { }
            public RxBehaviorSubject(T initialValue)
            {
                _subject = new RxSubject<T>();
                Current = initialValue;
            }

            /// <summary>
            /// The most-recently pushed value, or the default value provided at construction.
            /// </summary>
            public T Current { get; private set; }
            public SubjectStatus Status => _subject.Status;

            public void OnNext(T value)
            {
                if (Status == SubjectStatus.Active)
                    Current = value;
                _subject.OnNext(value);
            }

            public void OnCompleted() => _subject.OnCompleted();

            public void OnError(Exception error) => _subject.OnError(error);

            public IDisposable Subscribe(IRxObserver<T> observer)
            {
                if (Status == SubjectStatus.Active)
                    observer.OnNext(Current);

                return _subject.Subscribe(observer);
            }
        }
    }
}
