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
        public class ThrustGroupProvider : IDisposable
        {
            private const float MinEffectiveThrustRatio = 0.01f;

            private readonly List<IMyThrust> _thrustBlocks = new List<IMyThrust>();
            private readonly HashSet<IMyThrust> _activeThrusters = new HashSet<IMyThrust>();
            private readonly HashSet<IMyThrust> _existingThrusters = new HashSet<IMyThrust>();
            private readonly HashSet<IMyThrust> _removedThrusters = new HashSet<IMyThrust>();

            private readonly Dictionary<SubgridAndBase6Direction, ThrustGroup> _thrustGroups = new Dictionary<SubgridAndBase6Direction, ThrustGroup>();

            private readonly List<IMyMotorStator> _rotorBlocks = new List<IMyMotorStator>();
            private readonly Dictionary<IMyCubeGrid, IMyMotorStator> _rotorLookup = new Dictionary<IMyCubeGrid, IMyMotorStator>();
            private readonly Dictionary<IMyCubeGrid, Nacelle> _nacelleLookup = new Dictionary<IMyCubeGrid, Nacelle>();

            private readonly MyGridProgram _program;
            private readonly IRxObservable<UpdateEvent> _updates;
            private readonly IDisposable _subscription;

            public ThrustGroupProvider(MyGridProgram program, IRxObservable<UpdateEvent> updates)
            {
                _program = program;
                _updates = updates;
                _subscription = _updates.Subscribe(OnUpdate);
            }

            public IReadOnlyCollection<ThrustGroup> GetThrustGroups() => _thrustGroups.Values;

            public IReadOnlyCollection<Nacelle> GetNacelles() => _nacelleLookup.Values;

            public void Dispose()
            {
                _subscription.Dispose();

                foreach (var thrustGroup in _thrustGroups.Values)
                    thrustGroup.Dispose();
                foreach (var nacelle in _nacelleLookup.Values)
                    nacelle.Dispose();
            }

            private void OnUpdate(UpdateEvent e)
            {
                _program.GridTerminalSystem.GetBlocksOfType(_rotorBlocks, x => x.IsSameConstructAs(_program.Me));

                _rotorLookup.Clear();
                foreach (var rotor in _rotorBlocks)
                {
                    if (rotor.TopGrid != null)
                        _rotorLookup.Add(rotor.TopGrid, rotor);
                }

                UpdateThrustGroups();
            }

            private void UpdateThrustGroups()
            {
                _program.GridTerminalSystem.GetBlocksOfType(_thrustBlocks, x => x.IsSameConstructAs(_program.Me));

                _existingThrusters.Clear();

                foreach (var thruster in _thrustBlocks)
                {
                    _existingThrusters.Add(thruster);

                    if (IsElligibleThruster(thruster))
                        AddThruster(thruster);
                    else
                        RemoveThruster(thruster);
                }

                _removedThrusters.Clear();
                foreach (var thruster in _activeThrusters)
                {
                    if (!_existingThrusters.Contains(thruster))
                        _removedThrusters.Add(thruster);
                }

                foreach (var thruster in _removedThrusters)
                    RemoveThruster(thruster);
            }

            private static bool IsElligibleThruster(IMyThrust thruster)
            {
                return thruster.IsWorking && thruster.MaxEffectiveThrust / thruster.MaxThrust >= MinEffectiveThrustRatio;
            }

            private void AddThruster(IMyThrust thruster)
            {
                if (_activeThrusters.Add(thruster))
                {
                    var key = new SubgridAndBase6Direction(thruster.CubeGrid, thruster.Orientation.Forward);

                    if (!_thrustGroups.ContainsKey(key))
                    {
                        _thrustGroups.Add(key, new ThrustGroup(key.Grid, _updates));

                        IMyMotorStator rotor;
                        if (_rotorLookup.TryGetValue(thruster.CubeGrid, out rotor))
                        {
                            Nacelle nacelle;
                            if (!_nacelleLookup.TryGetValue(thruster.CubeGrid, out nacelle))
                            {
                                nacelle = new Nacelle(rotor, _updates);
                                _nacelleLookup.Add(thruster.CubeGrid, nacelle);
                            }

                            nacelle.AddThrustGroup(_thrustGroups[key]);
                        }
                    }

                    _thrustGroups[key].Add(thruster);
                }
            }

            private void RemoveThruster(IMyThrust thruster)
            {
                if (_activeThrusters.Remove(thruster))
                {
                    var key = new SubgridAndBase6Direction(thruster.CubeGrid, thruster.Orientation.Forward);
                    ThrustGroup group;

                    if (_thrustGroups.TryGetValue(key, out group))
                    {
                        group.Remove(thruster);

                        if (group.IsEmpty)
                        {
                            Nacelle nacelle;
                            if (_nacelleLookup.TryGetValue(group.Grid, out nacelle))
                            {
                                nacelle.RemoveThrustGroup(group);

                                if (nacelle.IsEmpty)
                                {
                                    _nacelleLookup.Remove(group.Grid);
                                    nacelle.Dispose();
                                }
                            }

                            _thrustGroups.Remove(key);
                            group.Dispose();
                        }
                    }
                }
            }
        }
    }
}
