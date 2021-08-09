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
        public sealed class RxSubject<T> : IRxObservable<T>, IRxObserver<T>
        {
            private Action<T> _OnNext;
            private Action<Exception> _OnError;
            private Action _OnCompleted;

            public SubjectStatus Status { get; private set; } = SubjectStatus.Active;

            public void OnNext(T value)
            {
                if (Status == SubjectStatus.Active)
                {
                    _OnNext?.Invoke(value);
                }
            }

            public void OnCompleted()
            {
                if (Status == SubjectStatus.Active)
                {
                    Status = SubjectStatus.Completed;
                    _OnCompleted?.Invoke();
                }
            }

            public void OnError(Exception error)
            {
                if (Status == SubjectStatus.Active)
                {
                    Status = SubjectStatus.Faulted;
                    _OnError?.Invoke(error);
                }
            }

            public IDisposable Subscribe(IRxObserver<T> observer)
            {
                if (Status == SubjectStatus.Completed)
                    observer.OnCompleted();

                _OnNext += observer.OnNext;
                _OnError += observer.OnError;
                _OnCompleted += observer.OnCompleted;

                return RxDisposable.Create(() =>
                {
                    _OnNext -= observer.OnNext;
                    _OnError -= observer.OnError;
                    _OnCompleted -= observer.OnCompleted;
                });
            }
        }
    }
}
