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
        public sealed class ThrustControllerComponent
        {
            private const double UpdatePeriod = 1D / 60D;

            private readonly IRxObservable<UpdateEvent> _updates;
            private readonly Func<IMyShipController> _shipControllerProvider;
            private readonly Func<ThrustGroupProvider> _thrustGroupProviderFactory;

            private IDisposable _updateSubscription;
            private ThrustGroupProvider _thrustGroupProvider;
            private int _counter;

            private const double Tolerance = 1e-8;
            private const double BaseSpeedVelocity = 100D;

            public ThrustControllerComponent(
                IRxObservable<UpdateEvent> updates, 
                Func<IMyShipController> shipControllerProvider, 
                Func<ThrustGroupProvider> thrustGroupProviderFactory)
            {
                _updates = updates;
                _shipControllerProvider = shipControllerProvider;
                _thrustGroupProviderFactory = thrustGroupProviderFactory;
                _updateSubscription = _updates.Subscribe(OnUpdate);
                _thrustGroupProvider = _thrustGroupProviderFactory();
            }

            private FlightMode _Mode = FlightMode.Hover;
            public FlightMode Mode
            {
                get { return _Mode; }
                set
                {
                    _Mode = value;

                    if (value == FlightMode.Park)
                    {
                        _thrustGroupProvider?.Dispose();
                        _thrustGroupProvider = null;

                        _updateSubscription?.Dispose();
                        _updateSubscription = null;
                    }
                    else
                    {
                        if (_thrustGroupProvider == null)
                            _thrustGroupProvider = _thrustGroupProviderFactory();

                        _updateSubscription?.Dispose();
                        _updateSubscription = _updates.Subscribe(OnUpdate);
                    }
                }
            }

            private void OnUpdate(UpdateEvent updateEvent)
            {
                _counter = (_counter + 1) % 60;

                var shipController = _shipControllerProvider();
                var dampVelocity = GetDampVelocity(shipController);
                var thrustGroups = _thrustGroupProvider.GetThrustGroups();
                var nacelles = _thrustGroupProvider.GetNacelles();
                
                float mass = shipController.CalculateShipMass().TotalMass;

                var mainGridThrustGroups = thrustGroups.Where(x => x.Grid == shipController.CubeGrid);
                var subgridThrustGroups = thrustGroups.Where(x => x.Grid != shipController.CubeGrid).ToArray();

                //  First try and hit damp velocity using only main grid thrusters.

                var mainThrustByDirection = mainGridThrustGroups.ToLookup(x => x.Direction);
                foreach (var group in mainGridThrustGroups)
                    group.ThrustOverridePercentage = 1e-30f;
                    
                var fixedGroupsByAxis = GetFixedThrustGroupsByAxis(mainThrustByDirection, dampVelocity.Value);
                var thrustLimit = Vector3D.Abs(mass * dampVelocity.Value);
                var mainGridThrust = Vector3D.Zero;
                bool xMet = false;
                bool yMet = false;
                bool zMet = false;

                if (fixedGroupsByAxis[0] != null)
                {
                    var xGroup = fixedGroupsByAxis[0];
                    var maxThrust = xGroup.MaxThrust.Length();
                    fixedGroupsByAxis[0].ThrustOverridePercentage = MyMath.Clamp((float)(thrustLimit.X / xGroup.MaxThrust.Length()), 0, 1);
                    mainGridThrust += xGroup.MaxThrust * xGroup.ThrustOverridePercentage;

                    if (maxThrust >= Math.Abs(dampVelocity.Value.X) * mass)
                        xMet = true;
                }
                if (fixedGroupsByAxis[1] != null)
                {
                    var yGroup = fixedGroupsByAxis[1];
                    var maxThrust = yGroup.MaxThrust.Length();
                    yGroup.ThrustOverridePercentage = MyMath.Clamp((float)(thrustLimit.Y / maxThrust), 0, 1);
                    mainGridThrust.Y = maxThrust * yGroup.ThrustOverridePercentage;

                    if (maxThrust >= Math.Abs(dampVelocity.Value.Y) * mass)
                        yMet = true;
                }
                if (fixedGroupsByAxis[2] != null)
                {
                    var zGroup = fixedGroupsByAxis[2];
                    var maxThrust = zGroup.MaxThrust.Length();
                    zGroup.ThrustOverridePercentage = MyMath.Clamp((float)(thrustLimit.Z / maxThrust), 0, 1);
                    mainGridThrust.Z = maxThrust * zGroup.ThrustOverridePercentage;

                    if (maxThrust >= Math.Abs(dampVelocity.Value.Z) * mass)
                        zMet = true;
                }

                var remainingDampVelocity = dampVelocity.Value - mainGridThrust / mass;

                //  If main-grid thrusters are insufficient, then use subgrid thrusters.
                if (!xMet || !yMet || !zMet)
                {
                    var worldDampVelocity = Vector3D.TransformNormal(remainingDampVelocity, shipController.WorldMatrix);

                    foreach (var nacelle in nacelles)
                        nacelle.RotateTowards(worldDampVelocity);

                    ApplyThrust(shipController, subgridThrustGroups, remainingDampVelocity, mass);
                }
                else
                {
                    foreach (var thrustGroup in subgridThrustGroups)
                        thrustGroup.ThrustOverridePercentage = 0;

                    foreach (var nacelle in nacelles)
                        nacelle.TargetVelocityRpm = 0;
                }
            }
            
            private ThrustGroup[] GetFixedThrustGroupsByAxis(ILookup<Base6Directions.Direction, ThrustGroup> thrustGroups, Vector3D dampVelocity)
            {
                var empty = Enumerable.Empty<ThrustGroup>();
                var xGroup = dampVelocity.X < 0 ? (thrustGroups.Contains(Base6Directions.Direction.Left) ? thrustGroups[Base6Directions.Direction.Left] : empty) :
                    (thrustGroups.Contains(Base6Directions.Direction.Right) ? thrustGroups[Base6Directions.Direction.Right] : empty);
                var yGroup = dampVelocity.Y < 0 ? (thrustGroups.Contains(Base6Directions.Direction.Down) ? thrustGroups[Base6Directions.Direction.Down] : empty) :
                    (thrustGroups.Contains(Base6Directions.Direction.Up) ? thrustGroups[Base6Directions.Direction.Up] : empty);
                var zGroup = dampVelocity.Z < 0 ? (thrustGroups.Contains(Base6Directions.Direction.Forward) ? thrustGroups[Base6Directions.Direction.Forward] : empty) :
                    (thrustGroups.Contains(Base6Directions.Direction.Backward) ? thrustGroups[Base6Directions.Direction.Backward] : empty);

                return new[] { xGroup.SingleOrDefault(), yGroup.SingleOrDefault(), zGroup.SingleOrDefault() };
            }

            private DampVelocity GetDampVelocity(IMyShipController shipController)
            {
                Vector3D gravity = shipController.GetNaturalGravity().ToBodyDirection(shipController.WorldMatrix);
                Vector3D velocity = GetShipVelocityToCancel(shipController);
                Vector3D moveIndicator = RoundDownAlmostZeroComponents(shipController.MoveIndicator, Tolerance);

                return new DampVelocity(-gravity, -velocity, moveIndicator * BaseSpeedVelocity);
            }

            private struct DampVelocity
            {
                public DampVelocity(Vector3D gravityComponent, Vector3D shipVelocity, Vector3D moveComponent)
                {
                    GravityComponent = gravityComponent;
                    ShipVelocity = shipVelocity;
                    MoveComponent = moveComponent;
                }

                public Vector3D GravityComponent { get; }
                public Vector3D ShipVelocity { get; }
                public Vector3D MoveComponent { get; }

                public Vector3D Value => MoveComponent + ShipVelocity + GravityComponent;
            }

            private Vector3D GetShipVelocityToCancel(IMyShipController shipController)
            {
                Vector3D velocity = shipController.GetShipVelocities().LinearVelocity;

                switch (Mode)
                {
                    case FlightMode.Hover:
                        break;
                    case FlightMode.Cruise:
                        if (velocity.Comp(shipController.WorldMatrix.Forward) > 0)
                        {
                            var forward = shipController.WorldMatrix.Forward;
                            velocity -= Vector3D.ProjectOnVector(ref velocity, ref forward);
                        }

                        break;
                    case FlightMode.Drift:
                        velocity = Vector3D.Zero;
                        break;
                    default:
                        throw new InvalidOperationException("Invalid enum value");
                }

                return velocity.ToBodyDirection(shipController.WorldMatrix);
            }

            private void ApplyThrust(IMyShipController shipController, ThrustGroup[] thrustGroups, Vector3D dampVelocity, float mass)
            {
                var simplex = new Simplex();
                const int m = 3;
                simplex.Reset(thrustGroups.Length + m, thrustGroups.Length);
                var thrustLimit = Vector3D.Abs(mass * dampVelocity);

                simplex.SetConstraintConstant(0, thrustLimit.X);
                simplex.SetConstraintConstant(1, thrustLimit.Y);
                simplex.SetConstraintConstant(2, thrustLimit.Z);

                for (int j = 0; j < thrustGroups.Length; j++)
                {
                    var maxThrust = thrustGroups[j].MaxThrust.ToBodyDirection(shipController.WorldMatrix);
                    var relativeSignImpulse = ToRelativeSigns(maxThrust, dampVelocity);

                    simplex.SetObjectiveCoefficient(j, maxThrust.Comp(dampVelocity));

                    simplex.SetConstraintCoefficient(0, j, relativeSignImpulse.X);
                    simplex.SetConstraintCoefficient(1, j, relativeSignImpulse.Y);
                    simplex.SetConstraintCoefficient(2, j, relativeSignImpulse.Z);

                    simplex.SetConstraintConstant(j + m, 1D);
                    simplex.SetConstraintCoefficient(j + m, j, 1D);
                }

                var result = simplex.Solve();

                if (result.Validity == Validity.Valid)
                {
                    for (int i = 0; i < thrustGroups.Length; i++)
                    {
                        thrustGroups[i].ThrustOverridePercentage = (float)result.Solution[i];
                    }
                }
            }

            /// <summary>
            /// For each component, if its sign is in the same direction as relativeTo's component, it's positive. If it's the opposite direction, 
            /// it's negative. If relativeTo's component is zero, it is zero.
            /// </summary>
            private static Vector3D ToRelativeSigns(Vector3D vector, Vector3D relativeTo)
            {
                return new Vector3D(
                    vector.X * Math.Sign(relativeTo.X),
                    vector.Y * Math.Sign(relativeTo.Y),
                    vector.Z * Math.Sign(relativeTo.Z));
            }

            private static Vector3D RoundDownAlmostZeroComponents(Vector3D vector, double tolerance)
            {
                Vector3D result = vector;
                if (Math.Abs(vector.X) <= tolerance)
                    result.X = 0;
                if (Math.Abs(vector.Y) <= tolerance)
                    result.Y = 0;
                if (Math.Abs(vector.Z) <= tolerance)
                    result.Z = 0;

                return result;
            }
        }

        public enum FlightMode
        {
            Hover,
            Cruise,
            Drift,
            Park
        }
    }
}
