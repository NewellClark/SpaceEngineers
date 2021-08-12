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
        public struct SubgridAndBase6Direction : IEquatable<SubgridAndBase6Direction>
        {
            public SubgridAndBase6Direction(IMyCubeGrid grid, Base6Directions.Direction direction)
            {
                Grid = grid;
                Direction = direction;
            }

            public IMyCubeGrid Grid { get; }
            public Base6Directions.Direction Direction { get; }

            private static bool Equals(SubgridAndBase6Direction left, SubgridAndBase6Direction right) => left.Grid == right.Grid && left.Direction == right.Direction;

            public bool Equals(SubgridAndBase6Direction other) => Equals(this, other);

            public override bool Equals(object obj) => obj is SubgridAndBase6Direction && Equals(this, (SubgridAndBase6Direction)obj);

            public override int GetHashCode()
            {
                int hash = 17;
                hash = hash * 23 + Grid?.GetHashCode() ?? 0;
                hash = hash * 23 + Direction.GetHashCode();

                return hash;
            }
        }
    }
}
