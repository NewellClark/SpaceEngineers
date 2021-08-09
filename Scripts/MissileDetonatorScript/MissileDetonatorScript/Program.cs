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
    partial class Program : MyGridProgram
    {
        public const bool StartArmed = true;
        public const double TriggerThreshhold = 500;

        private const string ArmCommand = "%arm";
        private const string DisarmCommand = "%disarm";

        private const double UpdatePeriod = 1D / 60D;
        private readonly UpdateComponent _updateComponent;
        private double _maxAcceleration = 0;
        private bool _armed = StartArmed;

        public Program()
        {

            bool storedArmedValue;
            if (bool.TryParse(Storage, out storedArmedValue))
                _armed = storedArmedValue;
            else
                _armed = StartArmed;

            _updateComponent = new UpdateComponent(this);

            _updateComponent.Commands.Subscribe(x =>
            {
                switch (x.Argument.ToLower())
                {
                    case ArmCommand:
                        _armed = true;
                        break;
                    case DisarmCommand:
                        _armed = false;
                        break;
                    default:
                        Echo($"Unknown command {x.Argument}");
                        break;
                }
            });

            var accelerationStream = _updateComponent.Updates.Select(x => Me.CubeGrid.GetPosition())
                .Derivative((current, previous) => (current - previous) / UpdatePeriod)
                .Derivative((current, previous) => (current - previous) / UpdatePeriod)
                .Select(x => x.Length());

            accelerationStream
                .Where(x => _armed && Math.Abs(x) >= TriggerThreshhold)
                .Subscribe(x => Detonate());

            accelerationStream.Subscribe(acc =>
            {
                if (Math.Abs(acc) >= Math.Abs(_maxAcceleration))
                {
                    _maxAcceleration = acc;
                }

                Echo($"Acceleration: {_maxAcceleration}");
            });
        }

        public void Main(string argument, UpdateType updateSource)
        {
            _updateComponent.Update(argument, updateSource);
        }

        private void Detonate()
        {
            var warheads = new List<IMyWarhead>();
            GridTerminalSystem.GetBlocksOfType(warheads, x => x.IsSameConstructAs(Me));
            
            foreach (var warhead in warheads)
            {
                warhead.Detonate();
            }
        }
    }
}
