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
    // This template is intended for extension classes. For most purposes you're going to want a normal
    // utility class.
    // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods
    static class IRxObservableExtensions
    {
        public static IDisposable Subscribe<T>(this Program.IRxObservable<T> @this, Action<T> onNext, Action onCompleted = null, Action<Exception> onError = null) =>
            @this.Subscribe(Program.RxObserver.Create(onNext, onCompleted, onError));

        public static Program.IRxObservable<T> Where<T>(this Program.IRxObservable<T> @this, Func<T, bool> predicate)
        {
            return Program.RxObservable.Create<T>(o =>
            {
                return @this.Subscribe(value =>
                {
                    if (predicate(value))
                        o.OnNext(value);
                },
                o.OnCompleted,
                o.OnError);
            });
        }

        public static Program.IRxObservable<TResult> Select<TSource, TResult>(this Program.IRxObservable<TSource> @this, Func<TSource, TResult> selector) =>
            Program.RxObservable.Create<TResult>(o => @this.Subscribe(value => o.OnNext(selector(value)), o.OnCompleted, o.OnError));
    }
}
