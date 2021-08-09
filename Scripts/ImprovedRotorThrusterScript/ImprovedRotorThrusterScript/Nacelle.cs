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
        public sealed class Nacelle
        {
            private readonly HashSet<ThrustGroup> _thrustGroups = new HashSet<ThrustGroup>();
            private readonly IMyMotorStator _rotor;

            public Nacelle(IMyMotorStator rotor)
            {
                _rotor = rotor;
            }

            public void RotateTowards(Vector3D worldDirection)
            {
                if (_thrustGroups.Count == 0)
                    return;

                var thrustGroup = GetBestThrustGroup();
                double maxRpm = _rotor.GetMaximum<float>("Velocity");
                double errorScale = Math.PI * maxRpm;
                var angle = Vector3D.Cross(Vector3D.Normalize(worldDirection), Vector3D.Normalize(thrustGroup.MaxThrust));
                double error = angle.Dot(_rotor.WorldMatrix.Up);

                _rotor.TargetVelocityRPM = (float)(error * errorScale).Clamp(-maxRpm, maxRpm);
            }

            public void AddThrustGroup(ThrustGroup thrustGroup)
            {
                _thrustGroups.Add(thrustGroup);
            }

            public void RemoveThrustGroup(ThrustGroup thrustGroup)
            {
                _thrustGroups.Remove(thrustGroup);
            }

            public float TargetVelocityRpm
            {
                get { return _rotor.TargetVelocityRPM; }
                set { _rotor.TargetVelocityRPM = value; }
            }

            public bool IsEmpty => _thrustGroups.Count == 0;

            private ThrustGroup GetBestThrustGroup()
            {
                double max = double.NegativeInfinity;
                ThrustGroup best = null;

                foreach (var group in _thrustGroups)
                {
                    double lengthSquared = group.MaxThrust.LengthSquared();
                    if (lengthSquared > max)
                    {
                        max = lengthSquared;
                        best = group;
                    }
                }

                return best;
            }
        }
    }
}
