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
        /// Provides an observable stream for game updates. Automatically sets the program to run every game tick while there are subscribers
        /// to the <see cref="Updates"/> stream, and sets the program to not run every game tick when there are no subscribers to <see cref="Updates"/>.
        /// </summary>
        public sealed class UpdateComponent
        {
            private readonly RxRefcountSubject<UpdateEvent> _Updates = new RxRefcountSubject<UpdateEvent>();
            private readonly RxSubject<UpdateEvent> _WeakUpdates = new RxSubject<UpdateEvent>();
            private readonly RxSubject<UpdateEvent> _Commands = new RxSubject<UpdateEvent>();

            public UpdateComponent(Program program)
            {
                _Updates.RefcountChanged.Subscribe(count => program.Runtime.UpdateFrequency = count == 0 ? UpdateFrequency.Once : UpdateFrequency.Update1);
            }

            public IRxObservable<UpdateEvent> Updates => _Updates;

            public IRxObservable<UpdateEvent> WeakUpdates => _WeakUpdates;

            public IRxObservable<UpdateEvent> Commands => _Commands;

            public IRxObservable<int> Refcount => _Updates.RefcountChanged;

            public void Update(string argument, UpdateType updateSource)
            {
                if ((updateSource & (UpdateType.Update1 | UpdateType.Update10 | UpdateType.Update100 | UpdateType.Once)) != 0)
                {
                    var e = new UpdateEvent(argument, updateSource);
                    _Updates.OnNext(e);
                    _WeakUpdates.OnNext(e);
                }


                if ((updateSource & (UpdateType.Terminal | UpdateType.Script | UpdateType.Trigger)) != 0)
                    _Commands.OnNext(new UpdateEvent(argument, updateSource));
            }
        }

        public struct UpdateEvent
        {
            public UpdateEvent(string argument, UpdateType updateSource)
            {
                Argument = argument;
                UpdateSource = updateSource;
            }

            public string Argument { get; }
            public UpdateType UpdateSource { get; }
        }
    }
}
