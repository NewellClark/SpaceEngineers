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
    static class VectorExtensions
    {
        /// <summary>
        /// Transforms the current world direction to a body direction.
        /// </summary>
        /// <param name="referenceBlockWorldMatrix">World matrix of the reference body.</param>
        public static Vector3D ToBodyDirection(this Vector3D @this, MatrixD referenceBlockWorldMatrix)
        {
            return Vector3D.TransformNormal(@this, MatrixD.Transpose(referenceBlockWorldMatrix));
        }

        public static MatrixD ThrustMatrix(this IMyThrust thruster)
        {
            var result = thruster.WorldMatrix;
            result.Forward *= -1;
            return result;
        }

        public static double Clamp(this double @this, double min, double max)
        {
            if (@this < min)
                return min;
            if (@this > max)
                return max;

            return @this;
        }

        public static double Comp(this Vector3D @this, Vector3D direction) => @this.Dot(Vector3D.Normalize(direction));
    }
}
