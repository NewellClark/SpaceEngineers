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
        /// Object responsible for obtaining the "best" <see cref="IMyShipController"/> on a grid, using a variety of factors.
        /// </summary>
        public sealed class FlightDeckProvider
        {
            private readonly IMyGridTerminalSystem _gridTerminalSystem;
            private readonly IMyProgrammableBlock _programmableBlock;
            private readonly List<IMyShipController> _shipControllerBlocks = new List<IMyShipController>();
            private IMyShipController _lastSelectedShipController;

            public FlightDeckProvider(IMyGridTerminalSystem gridTerminalSystem, IMyProgrammableBlock programmableBlock)
            {
                _gridTerminalSystem = gridTerminalSystem;
                _programmableBlock = programmableBlock;
            }

            /// <summary>
            /// Gets the "best" ship controller on a grid, considering a variety of factors, including whether the block is under control,
            /// whether the block is a main cockpit/remote control, and whether it was the previously-selected "best" ship controller.
            /// </summary>
            public IMyShipController GetBestFlightDeck()
            {
                _gridTerminalSystem.GetBlocksOfType(_shipControllerBlocks, x => x.IsWorking && x.IsSameConstructAs(_programmableBlock));
                _shipControllerBlocks.Sort((x, y) => -RateShipController(x).CompareTo(RateShipController(y)));  //  Sort descending.

                return _lastSelectedShipController = _shipControllerBlocks.Count > 0 ? _shipControllerBlocks[0] : default(IMyShipController);
            }

            /// <summary>
            /// A non-main ship controller that is under control is better than a main controller that is not.
            /// A main controller that is not under control is better than a non-main controller that is not.
            /// A controller that was last selected is better than an otherwise equal controller that was not.
            /// </summary>
            private int RateShipController(IMyShipController shipController)
            {
                int isUnderControl = shipController.IsUnderControl ? 5 : 1;
                int isMain = shipController.IsMainCockpit ? 3 : 1;
                int wasLastSelected = shipController == _lastSelectedShipController ? 1 : 0;
                return isUnderControl * isMain + wasLastSelected;
            }
        }
    }
}
