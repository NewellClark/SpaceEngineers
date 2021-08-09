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
        public static class RxObservable
        {
            public static IRxObservable<T> Create<T>(Func<IRxObserver<T>, IDisposable> subscribe) => new AnonymousObservable<T>(subscribe);
            public static IRxObservable<T> Create<T>(Func<IRxObserver<T>, Action> subscribe) => new AnonymousObservable<T>(o => RxDisposable.Create(subscribe(o)));

            private sealed class AnonymousObservable<T> : IRxObservable<T>
            {
                public AnonymousObservable(Func<IRxObserver<T>, IDisposable> subscribe)
                {
                    _Subscribe = subscribe;
                }

                private readonly Func<IRxObserver<T>, IDisposable> _Subscribe;
                public IDisposable Subscribe(IRxObserver<T> observer) => _Subscribe(observer);
            }
        }
    }
}
