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
        private readonly CommandEngine _commands;

        public Program()
        {
            _commands = new CommandEngine(GridTerminalSystem)
            {
                //  Assembler
                { "Enqueue", new AssemblerBuildCommand(Echo) },

                //  Battery
                { nameof(IMyBatteryBlock.ChargeMode), Command.Enum<IMyBatteryBlock, ChargeMode>((block, value) => block.ChargeMode = value) },

                //  Conveyor sorter
                { nameof(IMyConveyorSorter.DrainAll), Command.OnOff<IMyConveyorSorter>((block, value) => block.DrainAll = value) },

                //  Landing gear
                { nameof(IMyLandingGear.AutoLock), Command.OnOff<IMyLandingGear>((block, value) => block.AutoLock = value) },

                //  Piston
                { nameof(IMyPistonBase.Velocity), Command.Float<IMyPistonBase>((block, value) => block.Velocity = value) },
                { nameof(IMyPistonBase.MinLimit), Command.Float<IMyPistonBase>((block, value) => block.MinLimit = value) },
                { nameof(IMyPistonBase.MaxLimit), Command.Float<IMyPistonBase>((block, value) => block.MaxLimit = value) },

                //  Rotor
                { nameof(IMyMotorStator.Torque), Command.Float<IMyMotorStator>((block, value) => block.Torque = value) },
                { nameof(IMyMotorStator.BrakingTorque), Command.Float<IMyMotorStator>((block, value) => block.BrakingTorque = value) },
                { "Velocity", Command.Float<IMyMotorStator>((block, value) => block.TargetVelocityRPM = value) },
                { "LowerLimit", Command.Float<IMyMotorStator>((block, value) => block.LowerLimitDeg = value) },
                { "UpperLimit", Command.Float<IMyMotorStator>((block, value) => block.UpperLimitDeg = value) },
                { nameof(IMyMotorStator.Displacement), Command.Float<IMyMotorStator>((block, value) => block.Displacement = value) },
                { nameof(IMyMotorStator.RotorLock), Command.OnOff<IMyMotorStator>((block, value) => block.RotorLock = value) },

                //  Ship connector
                { nameof(IMyShipConnector.ThrowOut), Command.OnOff<IMyShipConnector>((block, value) => block.ThrowOut = value) },
                { nameof(IMyShipConnector.CollectAll), Command.OnOff<IMyShipConnector>((block, value) => block.CollectAll = value) },
                { nameof(IMyShipConnector.PullStrength), Command.Float<IMyShipConnector>((block, value) => block.PullStrength = value / 100) },

                //  Ship controller
                { nameof(IMyShipController.ShowHorizonIndicator), Command.OnOff<IMyShipController>((block, value) => block.ShowHorizonIndicator = value) },
                { nameof(IMyShipController.DampenersOverride), Command.OnOff<IMyShipController>((block, value) => block.DampenersOverride = value) },
                { nameof(IMyShipController.HandBrake), Command.OnOff<IMyShipController>((block, value) => block.HandBrake = value) },
                { nameof(IMyShipController.ControlThrusters), Command.OnOff<IMyShipController>((block, value) => block.ControlThrusters = value) },
                { nameof(IMyShipController.ControlWheels), Command.OnOff<IMyShipController>((block, value) => block.ControlWheels = value) },
                { nameof(IMyShipController.IsMainCockpit), Command.OnOff<IMyShipController>((block, value) => block.IsMainCockpit = value) },

                //  Thruster
                { nameof(IMyThrust.ThrustOverride), Command.Float<IMyThrust>((block, value) => block.ThrustOverride = value) },
                { nameof(IMyThrust.ThrustOverridePercentage), Command.Float<IMyThrust>((block, value) => block.ThrustOverridePercentage = value) }
            };


        }

        public void Save()
        {

        }

        public void Main(string argument, UpdateType updateSource)
        {
            if ((updateSource & (UpdateType.IGC | UpdateType.Mod | UpdateType.Script | UpdateType.Terminal | UpdateType.Trigger)) != 0)
            {
                var result = _commands.Execute(argument);
                //Echo(result.ToString());
            }
        }
    }
}
