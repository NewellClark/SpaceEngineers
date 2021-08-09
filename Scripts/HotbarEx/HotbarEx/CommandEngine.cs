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
        public class CommandEngine : IEnumerable<Command>
        {
            private readonly System.Text.RegularExpressions.Regex _blockGroupRegex = new System.Text.RegularExpressions.Regex(
                @"^\*(.*?)\*\.([A-Za-z0-9_]*?)=(.*)$");
            private readonly System.Text.RegularExpressions.Regex _blockNameRegex = new System.Text.RegularExpressions.Regex(
                @"^([^\.""=]*?| "".*?"")\.([A-Za-z0-9_]*?)=(.*)$");

            private readonly IMyGridTerminalSystem _gridTerminalSystem;
            private readonly Dictionary<string, List<Command>> _commandLookup = new Dictionary<string, List<Command>>();
            private readonly List<IMyTerminalBlock> _blocks = new List<IMyTerminalBlock>();

            public CommandEngine(IMyGridTerminalSystem gridTerminalSystem)
            {
                _gridTerminalSystem = gridTerminalSystem;
            }

            public CommandResult Execute(string argument)
            {
                CommandSyntax syntax;

                if (!TryParseCommand(argument, out syntax))
                    return CommandResult.InvalidCommand();

                List<Command> commands;
                if (!_commandLookup.TryGetValue(syntax.PropertyName.ToLowerInvariant(), out commands))
                    return CommandResult.UnknownProperty();

                var blocks = GetBlocksFromParsedSyntax(syntax);
                var successful = new List<IMyTerminalBlock>();
                var failed = new List<IMyTerminalBlock>();

                foreach (var command in commands)
                {
                    foreach (var block in blocks)
                    {
                        if (command.Execute(new CommandArguments(block, syntax.PropertyName, syntax.PropertyValue)))
                            successful.Add(block);
                        else
                            failed.Add(block);
                    }

                    if (failed.Count == 0)
                        break;

                    blocks = failed;
                    failed = new List<IMyTerminalBlock>();
                }

                return CommandResult.Executed(successful, failed);
            }

            public void Add(string propertyName, Command command)
            {
                string key = propertyName.ToLowerInvariant();

                List<Command> commands;
                if (!_commandLookup.TryGetValue(key, out commands))
                    _commandLookup.Add(key, commands = new List<Command>());

                commands.Add(command);
            }

            public IEnumerator<Command> GetEnumerator()
            {
                foreach (var kvp in _commandLookup)
                {
                    foreach (var command in kvp.Value)
                        yield return command;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private IEnumerable<IMyTerminalBlock> GetBlocksFromParsedSyntax(CommandSyntax syntax)
            {
                if (syntax.IsBlockGroup)
                {
                    var blockGroup = _gridTerminalSystem.GetBlockGroupWithName(syntax.BlockName);
                    blockGroup?.GetBlocks(_blocks);

                    return blockGroup != null ? _blocks : Enumerable.Empty<IMyTerminalBlock>();
                }
                else
                {
                    _gridTerminalSystem.GetBlocksOfType(_blocks, x => string.Equals(x.CustomName.Trim(), syntax.BlockName.Trim(), StringComparison.CurrentCulture));
                    return _blocks;
                }
            }

            private bool TryParseCommand(string argument, out CommandSyntax syntax)
            {
                var match = _blockGroupRegex.Match(argument);
                if (match.Success)
                {
                    syntax = new CommandSyntax(match.Groups[1].Value, true, match.Groups[2].Value, match.Groups[3].Value);
                    return true;
                }

                match = _blockNameRegex.Match(argument);
                if (match.Success)
                {
                    syntax = new CommandSyntax(match.Groups[1].Value, false, match.Groups[2].Value, match.Groups[3].Value);
                    return true;
                }

                syntax = default(CommandSyntax);
                return false;
            }

            private struct CommandSyntax
            {
                public CommandSyntax(string blockName, bool isBlockGroup, string propertyName, string propertyValue)
                {
                    BlockName = blockName;
                    IsBlockGroup = isBlockGroup;
                    PropertyName = propertyName;
                    PropertyValue = propertyValue;
                }

                public string BlockName { get; }
                public bool IsBlockGroup { get; }
                public string PropertyName { get; }
                public string PropertyValue { get; }
            }
        }
    }
}
