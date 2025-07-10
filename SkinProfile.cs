using System.Text.Json.Serialization;

namespace KeyOverlayEnhanced
{
    public class SkinProfile
    {
        public string SkinName { get; set; } = "Default";
        public string? Author { get; set; }
        public string? Description { get; set; }
        public string FontFileName { get; set; } = "Resources/consolab.ttf";
        public string KeyShape { get; set; } = "Rectangle";

        // Key Animation Settings
        public bool KeyEnableAnimation { get; set; } = true;
        public float KeyAnimationDurationPress { get; set; } = 0.15f;
        public float KeyAnimationDurationRelease { get; set; } = 0.25f;
        public float KeyTargetScalePress { get; set; } = 1.15f;
        public string KeyEasingFunctionPress { get; set; } = "EaseOutCubic";
        public string KeyEasingFunctionRelease { get; set; } = "EaseOutCubic"; // Default to same as press

        // Color Definitions
        public ColorDefinition BackgroundColor { get; set; } = new ColorDefinition(0, 0, 0, 255);
        public ColorDefinition KeyDefaultColor { get; set; } = new ColorDefinition(50, 50, 50, 255); // Default Dark Grey Key
        public ColorDefinition KeyPressedColor { get; set; } = new ColorDefinition(150, 150, 150, 255); // Default Light Grey Key Pressed
        public ColorDefinition KeyOutlineColor { get; set; } = new ColorDefinition(255, 255, 255, 255); // Default White Outline
        public ColorDefinition KeyLabelColor { get; set; } = new ColorDefinition(255, 255, 255, 255); // Default White Label
        public ColorDefinition CounterColor { get; set; } = new ColorDefinition(0, 255, 255, 255); // Default Cyan Counter

        // Tap Effect Specifics
        public ColorDefinition TapEffectColor { get; set; } = new ColorDefinition(255, 255, 255, 255); // Default White Tap Effect
        public string TapEffectShape { get; set; } = "Circle"; // Default Circle Tap Effect

        // Placeholder for future gradient/texture settings if needed
        // public GradientDefinition BackgroundGradient { get; set; }
        // public string BackgroundTexturePath { get; set; }

        public SkinProfile()
        {
            // Ensure default objects are created if not set by deserializer
            BackgroundColor ??= new ColorDefinition(0, 0, 0, 255);
            KeyDefaultColor ??= new ColorDefinition(50, 50, 50, 255);
            KeyPressedColor ??= new ColorDefinition(150, 150, 150, 255);
            KeyOutlineColor ??= new ColorDefinition(255, 255, 255, 255);
            KeyLabelColor ??= new ColorDefinition(255, 255, 255, 255);
            CounterColor ??= new ColorDefinition(0, 255, 255, 255);
            TapEffectColor ??= new ColorDefinition(255, 255, 255, 255);
        }
    }
}
