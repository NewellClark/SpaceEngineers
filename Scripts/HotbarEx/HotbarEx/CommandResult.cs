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
        public struct CommandResult
        {
            private CommandResult(IReadOnlyCollection<IMyTerminalBlock> successfulBlocks, IReadOnlyCollection<IMyTerminalBlock> failedBlocks, CommandResultStatus status)
            {
                SuccessfulBlocks = successfulBlocks;
                FailedBlocks = failedBlocks;
                Status = status;
            }

            public static CommandResult Executed(IReadOnlyCollection<IMyTerminalBlock> successfulBlocks, IReadOnlyCollection<IMyTerminalBlock> failedBlocks) =>
                new CommandResult(successfulBlocks, failedBlocks, CommandResultStatus.Executed);
            public static CommandResult InvalidCommand() => new CommandResult(Array.Empty<IMyTerminalBlock>(), Array.Empty<IMyTerminalBlock>(), CommandResultStatus.InvalidCommand);
            public static CommandResult UnknownProperty() => new CommandResult(Array.Empty<IMyTerminalBlock>(), Array.Empty<IMyTerminalBlock>(), CommandResultStatus.UnknownProperty);

            public IReadOnlyCollection<IMyTerminalBlock> SuccessfulBlocks { get; }
            public IReadOnlyCollection<IMyTerminalBlock> FailedBlocks { get; }
            public CommandResultStatus Status { get; }
            public int TotalBlocks => SuccessfulBlocks.Count + FailedBlocks.Count;

            public override string ToString()
            {
                switch (Status)
                {
                    case CommandResultStatus.Executed:
                        return $"Successfully executed command for {SuccessfulBlocks.Count}/{TotalBlocks} blocks.";
                    case CommandResultStatus.InvalidCommand:
                        return "Invalid command";
                    case CommandResultStatus.UnknownProperty:
                        return "Unknown property";
                    default:
                        return "Unexpected enum value.";
                }
            }
        }

        public enum CommandResultStatus
        {
            InvalidCommand,
            UnknownProperty,
            Executed
        }
    }
}
