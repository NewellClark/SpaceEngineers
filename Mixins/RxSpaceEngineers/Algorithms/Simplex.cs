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
        public sealed class Simplex
        {
            private SlackForm _slack;
            private SlackForm _pivot;
            private readonly HashSet<int> _basicIndexes = new HashSet<int>();
            private readonly HashSet<int> _nonBasicIndexes = new HashSet<int>();

            public SimplexSolution Solve()
            {
                if (!Initialize())
                    return SimplexSolution.NoSolution;

                Vector delta = new Vector(M + N);

                int entering;
                while (TryGetIndexWithPositiveNonZeroObjectiveValue(out entering))
                {
                    foreach (int i in _basicIndexes)
                    {
                        delta[i] = _slack.ConstraintCoefficients[i, entering] > 0 ?
                            _slack.ConstraintConstants[i] / _slack.ConstraintCoefficients[i, entering] :
                            double.PositiveInfinity;
                    }

                    int leaving = 0;
                    {
                        double minDelta = double.PositiveInfinity;
                        foreach (int i in _basicIndexes)
                        {
                            if (delta[i] <= minDelta)
                            {
                                leaving = i;
                                minDelta = delta[i];
                            }
                        }
                    }

                    if (double.IsPositiveInfinity(delta[leaving]))
                        return SimplexSolution.Unbounded;
                    else
                        Pivot(leaving, entering);
                }
                Vector solution = new Vector(N);

                for (int i = 0; i < N; i++)
                {
                    solution[i] = _basicIndexes.Contains(i) ? _slack.ConstraintConstants[i] : 0D;
                }

                return SimplexSolution.Valid(solution);
            }

            public Vector GetSolution()
            {
                Vector result = new Vector(N);

                for (int i = 0; i < N; i++)
                {
                    result[i] = _basicIndexes.Contains(i) ? _slack.ConstraintConstants[i] : 0D;
                }

                return result;
            }
            private void Pivot(int leaving, int entering)
            {
                _nonBasicIndexes.Remove(entering);
                _basicIndexes.Remove(leaving);

                //  Compute the coefficients of the equation for new basic entering variable.

                _pivot.ConstraintConstants[entering] = _slack.ConstraintConstants[leaving] / _slack.ConstraintCoefficients[leaving, entering];

                foreach (int j in _nonBasicIndexes)
                    _pivot.ConstraintCoefficients[entering, j] = _slack.ConstraintCoefficients[leaving, j] / _slack.ConstraintCoefficients[leaving, entering];

                _pivot.ConstraintCoefficients[entering, leaving] = 1D / _slack.ConstraintCoefficients[leaving, entering];

                //  Compute the coefficients of the remaining constraints.

                foreach (int i in _basicIndexes)
                {
                    _pivot.ConstraintConstants[i] = _slack.ConstraintConstants[i] - _slack.ConstraintCoefficients[i, entering] * _pivot.ConstraintConstants[entering];

                    foreach (int j in _nonBasicIndexes)
                        _pivot.ConstraintCoefficients[i, j] = _slack.ConstraintCoefficients[i, j] - _slack.ConstraintCoefficients[i, entering] * _pivot.ConstraintCoefficients[entering, j];

                    _pivot.ConstraintCoefficients[i, leaving] = -_slack.ConstraintCoefficients[i, entering] * _pivot.ConstraintCoefficients[entering, leaving];
                }

                //  Compute the objective function.

                _pivot.ObjectiveConstant = _slack.ObjectiveConstant + _slack.ObjectiveCoefficients[entering] * _pivot.ConstraintConstants[entering];

                foreach (int j in _nonBasicIndexes)
                    _pivot.ObjectiveCoefficients[j] = _slack.ObjectiveCoefficients[j] - _slack.ObjectiveCoefficients[entering] * _pivot.ConstraintCoefficients[entering, j];

                _pivot.ObjectiveCoefficients[leaving] = -_slack.ObjectiveCoefficients[entering] * _pivot.ConstraintCoefficients[entering, leaving];

                //  Compute new sets of basic and nonbasic variables

                _nonBasicIndexes.Add(leaving);
                _basicIndexes.Add(entering);

                Swap(ref _slack, ref _pivot);
            }

            private bool Initialize()
            {
                int leaving = -1;
                double minConstraintConstant = double.PositiveInfinity;
                foreach (int i in _basicIndexes)
                {
                    if (_slack.ConstraintConstants[i] <= minConstraintConstant)
                    {
                        minConstraintConstant = _slack.ConstraintConstants[i];
                        leaving = i;
                    }
                }

                if (minConstraintConstant >= 0)
                    return true;

                double[] originalObjectiveCoefficients = _slack.ObjectiveCoefficients;
                _slack.ObjectiveCoefficients = new double[M + N + 1];
                _slack.ObjectiveCoefficients[Aux] = -1;

                foreach (int i in _basicIndexes)
                    _slack.ConstraintCoefficients[i, Aux] = -1;

                _nonBasicIndexes.Add(Aux);
                Pivot(leaving, Aux);

                Vector delta = new Vector(M + N + 1);
                int entering;
                while (TryGetIndexWithPositiveNonZeroObjectiveValue(out entering))
                {
                    foreach (int i in _basicIndexes)
                    {
                        delta[i] = _slack.ConstraintCoefficients[i, entering] > 0 ?
                            _slack.ConstraintConstants[i] / _slack.ConstraintCoefficients[i, entering] :
                            double.PositiveInfinity;
                    }

                    {
                        double minDelta = double.PositiveInfinity;
                        foreach (int i in _basicIndexes)
                        {
                            if (delta[i] <= minDelta)
                            {
                                leaving = i;
                                minDelta = delta[i];
                            }
                        }
                    }

                    if (double.IsPositiveInfinity(delta[leaving]))
                        break;
                    else
                        Pivot(leaving, entering);
                }

                if (_slack.ConstraintConstants[Aux] == 0)
                {
                    if (_basicIndexes.Contains(Aux))
                        Pivot(Aux, _nonBasicIndexes.First());

                    _nonBasicIndexes.Remove(Aux);
                    _slack.ObjectiveCoefficients = originalObjectiveCoefficients;

                    foreach (int i in _basicIndexes)
                    {
                        _slack.ConstraintCoefficients[i, Aux] = 0;

                        foreach (int j in _nonBasicIndexes)
                            _slack.ObjectiveCoefficients[j] -= _slack.ObjectiveCoefficients[i] * _slack.ConstraintCoefficients[i, j];
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }

            public int M { get; private set; } = -1;
            public int N { get; private set; } = -1;

            private int Aux => M + N;

            public void Reset(int m, int n)
            {
                if (M != m || N != n)
                {
                    M = m;
                    N = n;
                    _slack = new SlackForm(0D, new double[m + n + 1], new double[m + n + 1], new double[m + n + 1, m + n + 1]);
                    _pivot = new SlackForm(0D, new double[m + n + 1], new double[m + n + 1], new double[m + n + 1, m + n + 1]);
                }

                _slack.ObjectiveConstant = 0D;
                _pivot.ObjectiveConstant = 0D;

                _nonBasicIndexes.Clear();
                for (int j = 0; j < n; j++)
                    _nonBasicIndexes.Add(j);
                _basicIndexes.Clear();
                for (int i = 0; i < m; i++)
                    _basicIndexes.Add(i + n);
            }

            public double GetObjectiveCoefficient(int j) => _slack.ObjectiveCoefficients[j];
            public void SetObjectiveCoefficient(int j, double value) => _slack.ObjectiveCoefficients[j] = value;
            public double GetConstraintCoefficient(int i, int j) => _slack.ConstraintCoefficients[i + N, j];
            public void SetConstraintCoefficient(int i, int j, double value) => _slack.ConstraintCoefficients[i + N, j] = value;
            public double GetConstraintConstant(int i) => _slack.ConstraintConstants[i + N];
            public void SetConstraintConstant(int i, double value) => _slack.ConstraintConstants[i + N] = value;

            private struct SlackForm
            {
                public SlackForm(double objectiveConstant, double[] objectiveCoefficients, double[] constraintConstants, double[,] constraintCoefficients)
                {
                    ObjectiveConstant = objectiveConstant;
                    ObjectiveCoefficients = objectiveCoefficients;
                    ConstraintConstants = constraintConstants;
                    ConstraintCoefficients = constraintCoefficients;
                }

                public double ObjectiveConstant { get; set; }
                public double[] ObjectiveCoefficients { get; set; }
                public double[] ConstraintConstants { get; }
                public double[,] ConstraintCoefficients { get; }
            }

            private static void Swap<T>(ref T left, ref T right)
            {
                T temp = left;
                left = right;
                right = temp;
            }

            private bool TryGetIndexWithPositiveNonZeroObjectiveValue(out int j)
            {
                foreach (int index in _nonBasicIndexes)
                {
                    if (_slack.ObjectiveCoefficients[index] > 0)
                    {
                        j = index;
                        return true;
                    }
                }

                j = default(int);
                return false;
            }

            public string ToString(string format)
            {
                var result = new StringBuilder();
                result.Append($"f(x) =");
                for (int j = 0; j < N; j++)
                {
                    result.Append($" {GetObjectiveCoefficient(j).ToString(format)} X{j + 1}");
                }
                result.AppendLine();

                for (int i = 0; i < M; i++)
                {
                    for (int j = 0; j < N; j++)
                    {
                        if (j > 0)
                            result.Append(" + ");
                        result.Append($"{GetConstraintCoefficient(i, j).ToString(format)} X{j + 1}");
                    }

                    result.AppendLine($" <= {GetConstraintConstant(i).ToString(format)}");
                }

                return result.ToString();
            }

            public override string ToString() => ToString("G");
        }

        public enum Validity
        {
            NoSolution,
            Unbounded,
            Valid
        }
    }
}
