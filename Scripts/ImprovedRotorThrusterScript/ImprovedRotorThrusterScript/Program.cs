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
        private readonly UpdateComponent _updateComponent;
        private ThrustControllerComponent _thrustController;
        private FlightDeckProvider _flightDeckProvider;

        public Program()
        {
            _updateComponent = new UpdateComponent(this);
            DelayedInitStrategy();
        }

        public void Save()
        {
            Storage = _thrustController.Mode.ToString();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            _updateComponent.Update(argument, updateSource);
        }

        private void DelayedInitStrategy()
        {
            int initCounter = 0;
            IDisposable initSubscription = null;

            initSubscription = _updateComponent.Updates.Subscribe(x =>
            {
                if (++initCounter == 120)
                {
                    _flightDeckProvider = new FlightDeckProvider(GridTerminalSystem, Me);

                    _thrustController = new ThrustControllerComponent(
                        _updateComponent.Updates,
                        _flightDeckProvider.GetBestFlightDeck,
                        () => new ThrustGroupProvider(this, _updateComponent.Updates));

                    initSubscription.Dispose();

                    FlightMode mode;
                    if (Enum.TryParse(Storage, out mode))
                        _thrustController.Mode = mode;

                    _updateComponent.Commands.Subscribe(ExecuteCommands);

                    int charIndex = 0;
                    const string pinwheel = @"/-\|";
                    _updateComponent.WeakUpdates.Subscribe(
                        y =>
                        {
                            charIndex = (charIndex + 1) % pinwheel.Length;
                            Echo($"{pinwheel[charIndex]}\nFlight mode: {_thrustController.Mode}");
                        });
                }
            });

            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        private void ExecuteCommands(UpdateEvent e)
        {
            switch (e.Argument.ToLowerInvariant())
            {
                case CommandNames.Park:
                    _thrustController.Mode = FlightMode.Park;
                    break;
                case CommandNames.Hover:
                    _thrustController.Mode = FlightMode.Hover;
                    break;
                case CommandNames.Cruise:
                    _thrustController.Mode = FlightMode.Cruise;
                    break;
                case CommandNames.Drift:
                    _thrustController.Mode = FlightMode.Drift;
                    break;
                default:
                    Echo($"Invalid command '{e.Argument}'");
                    break;
            }
        }
    }
}
