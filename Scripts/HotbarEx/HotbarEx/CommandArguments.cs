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
        public struct CommandArguments
        {
            public CommandArguments(IMyTerminalBlock block, string propertyName, string value)
            {
                Block = block;
                PropertyName = propertyName;
                Value = value;
            }

            public IMyTerminalBlock Block { get; }
            public string PropertyName { get; }
            public string Value { get; }
        }
    }
}
