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
    static class ObservableExtensions
    {
        public  static Program.IRxObservable<float> SlidingAverage(this Program.IRxObservable<float> @this, int windowSize)
        {
            float[] ringBuffer = new float[windowSize];
            float total = 0f;
            int currentIndex = 0;

            return @this.Select(latestValue =>
            {
                total -= ringBuffer[currentIndex];
                total += latestValue;
                ringBuffer[currentIndex] = latestValue;
                currentIndex = (currentIndex + 1) % windowSize;

                return total / windowSize;
            });
        }
    }
}
