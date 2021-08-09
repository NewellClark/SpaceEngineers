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
        public sealed class AssemblerBuildCommand : Command<IMyAssembler>
        {
            private readonly System.Text.RegularExpressions.Regex _splitArgumentRegex = new System.Text.RegularExpressions.Regex(
                @"\s+");
            private readonly System.Text.RegularExpressions.Regex _buildItemRegex = new System.Text.RegularExpressions.Regex(
                @"([^:,]+?):([0-9]+)");
            private readonly Action<string> _echo;

            public AssemblerBuildCommand(Action<string> echo)
            {
                _echo = echo;
            }

            protected override bool Execute(IMyAssembler block, string value)
            {
                var subArguments = _splitArgumentRegex.Split(value);

                var buildItems = new List<BuildItem>();

                foreach (var subArgument in subArguments)
                {
                    BuildItem buildItem;
                    if (!TryParseBuildItem(subArgument.Trim(), out buildItem))
                        return false;
                    buildItems.Add(buildItem);
                }

                foreach (var item in buildItems)
                {
                    try
                    {
                        block.AddQueueItem(item.Component, (MyFixedPoint)item.Quantity);
                    }
                    catch (Exception)
                    {
                        _echo?.Invoke($"Failed to add item {item.Component}");
                    }
                }

                return true;
            }

            private struct BuildItem
            {
                public BuildItem(MyDefinitionId component, int quantity)
                {
                    Component = component;
                    Quantity = quantity;
                }

                public MyDefinitionId Component { get; }
                public int Quantity { get; }
            }

            private bool TryParseBuildItem(string text, out BuildItem result)
            {
                result = default(BuildItem);

                var match = _buildItemRegex.Match(text);
                if (!match.Success)
                    return false;
                
                MyDefinitionId component;
                if (!MyDefinitionId.TryParse($"MyObjectBuilder_BlueprintDefinition", match.Groups[1].Value, out component))
                    return false;

                int quantity;
                if (!int.TryParse(match.Groups[2].Value, out quantity))
                    return false;

                result = new BuildItem(component, quantity);
                return true;
            }
        }
    }
}
