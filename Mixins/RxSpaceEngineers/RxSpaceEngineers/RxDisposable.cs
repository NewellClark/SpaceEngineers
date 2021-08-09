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
        /// Contains methods for creating <see cref="IDisposable"/>s.
        /// </summary>
        public static class RxDisposable
        {
            public static IDisposable Create(Action dispose) => new AnonymousDisposable(dispose);

            private sealed class AnonymousDisposable : IDisposable
            {
                private Action _Dispose;

                public AnonymousDisposable(Action dispose)
                {
                    _Dispose = dispose;
                }

                public void Dispose()
                {
                    _Dispose?.Invoke();
                    _Dispose = null;
                }
            }
        }
    }
}
