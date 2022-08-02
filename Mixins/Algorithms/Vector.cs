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
        public struct Vector
        {
            private readonly double[] _elements;

            public Vector(int size)
            {
                _elements = new double[size];
            }
            private Vector(double[] elements)
            {
                _elements = elements;
            }

            public int Size => _elements?.Length ?? 0;

            public double this[int index]
            {
                get { return _elements[index]; }
                set { _elements[index] = value; }
            }

            public Vector Create(params double[] values) => new Vector(values);

            public string ToString(string format)
            {
                var result = new StringBuilder();
                result.Append("[");

                for (int i = 0; i < Size; i++)
                {
                    result.Append(i > 0 ? $" {this[i].ToString(format)}" : this[i].ToString(format));
                }
                result.Append("]");

                return result.ToString();
            }

            public override string ToString() => ToString("G");

            public Element Min()
            {
                double value = double.PositiveInfinity;
                int index = -1;

                for (int i = 0; i < Size; i++)
                {
                    if (this[i] <= value)
                    {
                        value = this[i];
                        index = i;
                    }
                }

                return new Element(index, value);
            }

            public Element Max()
            {
                double value = double.NegativeInfinity;
                int index = -1;

                for (int i = 0; i < Size; i++)
                {
                    if (this[i] >= value)
                    {
                        value = this[i];
                        index = i;
                    }
                }

                return new Element(index, value);
            }
        }

        public struct Element
        {
            public Element(int index, double value)
            {
                Index = index;
                Value = value;
            }

            public int Index { get; }
            public double Value { get; }

            public string ToString(string format) => $"[{Index}]({Value.ToString(format)})";
            public override string ToString() => ToString("G");
        }
    }
}

