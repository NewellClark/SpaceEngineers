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
        public abstract class Command
        {
            public abstract bool Execute(CommandArguments arguments);

            private sealed class AnonymousCommand<TBlock> : Command<TBlock>
                where TBlock : class, IMyTerminalBlock
            {
                private readonly Func<TBlock, string, bool> _Execute;

                public AnonymousCommand(Func<TBlock, string, bool> execute)
                {
                    _Execute = execute;
                }

                protected override bool Execute(TBlock block, string value) => _Execute(block, value);
            }

            public static Command<TBlock> Create<TBlock>(Func<TBlock, string, bool> execute)
                where TBlock : class, IMyTerminalBlock => new AnonymousCommand<TBlock>(execute);

            public static Command<TBlock> OnOff<TBlock>(Action<TBlock, bool> execute)
                where TBlock : class, IMyTerminalBlock
            {
                return new AnonymousCommand<TBlock>((block, value) =>
                {
                    switch (value.ToLowerInvariant())
                    {
                        case "true":
                        case "on":
                            execute(block, true);
                            return true;
                        case "false":
                        case "off":
                            execute(block, false);
                            return true;
                        default:
                            return false;
                    }
                });
            }

            public static Command<TBlock> Enum<TBlock, TEnum>(Action<TBlock, TEnum> execute)
                where TBlock : class, IMyTerminalBlock
                where TEnum : struct
            {
                return new AnonymousCommand<TBlock>((block, value) =>
                {
                    TEnum result;
                    if (!System.Enum.TryParse(value, true, out result))
                        return false;

                    execute(block, result);

                    return true;
                });
            }

            public static Command<TBlock> Float<TBlock>(Action<TBlock, float> execute)
                where TBlock : class, IMyTerminalBlock
            {
                return new AnonymousCommand<TBlock>((block, value) =>
                {
                    float number;
                    if (float.TryParse(value, out number))
                    {
                        execute(block, number);
                        return true;
                    }

                    return false;
                });
            }

            public static Command<TBlock> Double<TBlock>(Action<TBlock, double> execute)
                where TBlock : class, IMyTerminalBlock
            {
                return new AnonymousCommand<TBlock>((block, value) =>
                {
                    double number;
                    if (double.TryParse(value, out number))
                    {
                        execute(block, number);
                        return true;
                    }

                    return false;
                });
            }
        }

        public abstract class Command<TBlock> : Command
            where TBlock : class, IMyTerminalBlock
        {

            protected abstract bool Execute(TBlock block, string value);

            public sealed override bool Execute(CommandArguments arguments)
            {
                var cast = arguments.Block as TBlock;

                if (cast == null)
                    return false;

                return Execute(cast, arguments.Value);
            }
        }
    }
}
