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
        public struct SimplexSolution
        {
            private SimplexSolution(Validity validity, Vector solution)
            {
                Validity = validity;
                Solution = solution;
            }

            public static SimplexSolution Valid(Vector solution) => new SimplexSolution(Validity.Valid, solution);
            public static SimplexSolution Unbounded => new SimplexSolution(Validity.Unbounded, default(Vector));
            public static SimplexSolution NoSolution => new SimplexSolution(Validity.NoSolution, default(Vector));

            public Validity Validity { get; }
            public Vector Solution { get; }
        }

    }
}

