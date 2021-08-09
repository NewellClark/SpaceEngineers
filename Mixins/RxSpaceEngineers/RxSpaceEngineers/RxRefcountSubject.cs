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
        public sealed class RxRefcountSubject<T> : IRxObservable<T>, IRxObserver<T>
        {
            private readonly RxSubject<T> _subject = new RxSubject<T>();
            private readonly RxBehaviorSubject<int> _Refcount = new RxBehaviorSubject<int>(0);

            public int Refcount => _Refcount.Current;
            public IRxObservable<int> RefcountChanged => _Refcount;

            public SubjectStatus Status => _subject.Status;

            public void OnNext(T value) => _subject.OnNext(value);
            public void OnCompleted() => _subject.OnCompleted();
            public void OnError(Exception error) => _subject.OnError(error);

            public IDisposable Subscribe(IRxObserver<T> observer)
            {
                var subscription = _subject.Subscribe(observer);
                _Refcount.OnNext(_Refcount.Current + 1);

                return RxDisposable.Create(() =>
                {
                    subscription.Dispose();
                    _Refcount.OnNext(_Refcount.Current - 1);
                });
            }
        }
    }
}
