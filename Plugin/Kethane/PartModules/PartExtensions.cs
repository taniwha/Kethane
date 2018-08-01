using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kethane.PartModules
{
    internal static class PartExtensions
    {
        public static void GetConnectedResourceTotals(this Part part, String resourceName, out double amount, out double maxAmount, bool pulling = true)
        {
            var resourceDef = PartResourceLibrary.Instance.GetDefinition(resourceName);
            part.GetConnectedResourceTotals(resourceDef.id, resourceDef.resourceFlowMode, out amount, out maxAmount, pulling);
        }

        public static AnimationState[] SetUpAnimation(this Part part, string animationName)
        {
            var states = new List<AnimationState>();
            foreach (var animation in part.FindModelAnimators(animationName))
            {
                var animationState = animation[animationName];
                animationState.speed = 0;
                animationState.enabled = true;
                animationState.wrapMode = WrapMode.ClampForever;
                animation.Blend(animationName);
                states.Add(animationState);
            }
            return states.ToArray();
        }
    }
}
