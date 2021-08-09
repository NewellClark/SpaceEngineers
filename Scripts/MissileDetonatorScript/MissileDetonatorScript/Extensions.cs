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
    static class Extensions
    {
        public static Program.IRxObservable<TDerivative> Derivative<TSource, TDerivative>(this Program.IRxObservable<TSource> @this, Func<TSource, TSource, TDerivative> differenceFunction)
        {
            return Program.RxObservable.Create<TDerivative>(o =>
            {
                bool gotFirst = false;
                TSource previous = default(TSource);

                return @this.Subscribe(
                    current =>
                    {
                        if (gotFirst)
                            o.OnNext(differenceFunction(current, previous));

                        previous = current;
                        gotFirst = true;
                    },
                    () => o.OnCompleted(),
                    error => o.OnError(error));
            });
        }

        public static MatrixD ThrustMatrix(this IMyThrust @this)
        {
            var result = @this.WorldMatrix;
            result.Forward *= -1;
            return result;
        }
    }
}
