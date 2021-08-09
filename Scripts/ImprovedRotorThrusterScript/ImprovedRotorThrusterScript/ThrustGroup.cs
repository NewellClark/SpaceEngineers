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
        public sealed class ThrustGroup : IDisposable
        {
            private const int SlidingAverageBufferSize = 20;
            private readonly HashSet<IMyThrust> _thrusters = new HashSet<IMyThrust>();
            private bool _dirty;
            private readonly IDisposable _subscription;

            /// <summary>
            /// The minimum value for <see cref="IMyThrust.ThrustOverridePercentage"/> that will prevent the cockpit from controlling the thruster.
            /// </summary>
            private const float MinThrustOverridePercentage = 1e-32f;

            public ThrustGroup(IMyCubeGrid grid, IRxObservable<UpdateEvent> updates)
            {
                Grid = grid;
                _subscription = SlidingAverage(updates.Select(x => ThrustOverridePercentage), SlidingAverageBufferSize)
                    .Subscribe(x =>
                    {
                        UpdateThrustOverridePercentage(x);
                        _dirty = true;
                    });
            }

            public IMyCubeGrid Grid { get; }

            private Vector3D _MaxThrust;
            public Vector3D MaxThrust
            {
                get
                {
                    if (_dirty)
                        UpdateMaxThrust();

                    return _MaxThrust;
                }
            }

            private void UpdateMaxThrust()
            {
                _MaxThrust = Vector3D.Zero;

                foreach (var thruster in _thrusters)
                    _MaxThrust += thruster.MaxEffectiveThrust * thruster.ThrustMatrix().Forward;

                _dirty = false;
            }

            public float ThrustOverridePercentage { get; set; }

            private void UpdateThrustOverridePercentage(float value)
            {
                //  Prevent ship controller from controlling thrusters.
                float newThrustPercentage = value == 0 ? MinThrustOverridePercentage : value;

                foreach (var thruster in _thrusters)
                    thruster.ThrustOverridePercentage = newThrustPercentage;
            }

            public bool Add(IMyThrust thruster)
            {
                bool wasAdded = _thrusters.Add(thruster);

                if (wasAdded)
                    _dirty = true;

                return wasAdded;
            }

            public bool Remove(IMyThrust thruster)
            {
                bool wasRemoved = _thrusters.Remove(thruster);

                if (wasRemoved)
                {
                    //_gridMaxThrust -= GetMaxThrust(thruster) * thruster.ThrustMatrix().Forward.ToBodyDirection(Grid.WorldMatrix);
                    _dirty = true;
                    thruster.ThrustOverridePercentage = 0;
                }

                return wasRemoved;
            }

            public bool IsEmpty => _thrusters.Count == 0;

            public void Dispose()
            {
                _subscription.Dispose();

                foreach (var thruster in _thrusters)
                    thruster.ThrustOverride = 0;
            }

            private static IRxObservable<float> SlidingAverage(IRxObservable<float> source, int count)
            {
                float[] ringBuffer = new float[count];
                float total = 0f;
                int currentIndex = 0;

                return source.Select(latestValue =>
                {
                    total -= ringBuffer[currentIndex];
                    total += latestValue;
                    ringBuffer[currentIndex] = latestValue;
                    currentIndex = (currentIndex + 1) % count;

                    return total / count;
                });
            }
        }
    }
}
