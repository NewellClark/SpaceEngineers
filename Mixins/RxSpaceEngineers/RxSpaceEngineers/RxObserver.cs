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
        public static class RxObserver
        {
            public static IRxObserver<T> Create<T>(Action<T> onNext, Action onCompleted = null, Action<Exception> onError = null) =>
                new AnonymousObserver<T>(onNext, onCompleted, onError);

            private sealed class AnonymousObserver<T> : IRxObserver<T>
            {
                public AnonymousObserver(Action<T> onNext, Action onCompleted, Action<Exception> onError)
                {
                    _OnNext = onNext;
                    _OnError = onError;
                    _OnCompleted = onCompleted;
                }

                private readonly Action<T> _OnNext;
                public void OnNext(T value) => _OnNext?.Invoke(value);

                private readonly Action _OnCompleted;
                public void OnCompleted() => _OnCompleted?.Invoke();

                private readonly Action<Exception> _OnError;
                public void OnError(Exception error) => _OnError?.Invoke(error);
            }
        }
    }
}
