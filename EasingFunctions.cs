using System;

namespace KeyOverlayEnhanced
{
    public static class EasingFunctions
    {
        // Linear (no easing)
        public static float Linear(float t)
        {
            t = Math.Max(0f, Math.Min(1f, t));
            return t;
        }

        public static float EaseOutCubic(float t)
        {
            // Clamp t to the [0, 1] range
            t = Math.Max(0f, Math.Min(1f, t));
            return 1f - (float)Math.Pow(1f - t, 3);
        }

        public static float EaseOutExpo(float t)
        {
            // Clamp t to the [0, 1] range
            t = Math.Max(0f, Math.Min(1f, t));
            return t == 1f ? 1f : 1f - (float)Math.Pow(2, -10 * t);
        }

        // Add more easing functions here if needed:
        // e.g. EaseInQuad, EaseOutQuad, EaseInOutQuad, etc.

        public static float ApplyEasing(string easingType, float t)
        {
            switch (easingType)
            {
                case "EaseOutCubic":
                    return EaseOutCubic(t);
                case "EaseOutExpo":
                    return EaseOutExpo(t);
                case "Linear":
                default:
                    return Linear(t);
            }
        }
    }
}
