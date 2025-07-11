#nullable enable
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
        public string KeyEasingFunctionRelease { get; set; } = "EaseOutCubic";

        // --- Main Overlay Colors ---
        public ColorDefinition BackgroundColor { get; set; } = new ColorDefinition(0, 0, 0, 255); // Main window background
        public ColorDefinition KeyDefaultColor { get; set; } = new ColorDefinition(50, 50, 50, 255);
        public ColorDefinition KeyPressedColor { get; set; } = new ColorDefinition(150, 150, 150, 255);
        public ColorDefinition KeyOutlineColor { get; set; } = new ColorDefinition(220, 220, 220, 255);
        public ColorDefinition KeyLabelColor { get; set; } = new ColorDefinition(255, 255, 255, 255);
        public ColorDefinition CounterColor { get; set; } = new ColorDefinition(0, 200, 200, 255);
        public ColorDefinition GlitchColor { get; set; } = new ColorDefinition(255, 0, 0, 150);


        // --- Tap Effect Specifics ---
        public ColorDefinition TapEffectColor { get; set; } = new ColorDefinition(255, 255, 255, 200);
        public string TapEffectShape { get; set; } = "Circle";

        // --- Settings Panel Theming ---
        public ColorDefinition PanelBackgroundColor { get; set; } = new ColorDefinition(30, 30, 46, 235); // Dark, slightly transparent
        public ColorDefinition PanelTextColor { get; set; } = new ColorDefinition(205, 214, 244, 255); // Light text
        public ColorDefinition PanelHeaderTextColor { get; set; } = new ColorDefinition(203, 166, 247, 255); // e.g., Mauve for headers

        public ColorDefinition ControlBackgroundColor { get; set; } = new ColorDefinition(49, 50, 68, 255);  // Darker control background
        public ColorDefinition ControlOutlineColor { get; set; } = new ColorDefinition(88, 91, 112, 255); // Subtle outline
        public ColorDefinition ControlTextColor { get; set; } = new ColorDefinition(186, 194, 222, 255); // Lighter than main panel text
        public ColorDefinition ControlAccentColor { get; set; } = new ColorDefinition(137, 180, 250, 255); // e.g., Blue for highlights

        public ColorDefinition ButtonBackgroundColor { get; set; } = new ColorDefinition(69, 71, 90, 255);
        public ColorDefinition ButtonTextColor { get; set; } = new ColorDefinition(205, 214, 244, 255);
        public ColorDefinition ButtonHoverBackgroundColor { get; set; } = new ColorDefinition(88, 91, 112, 255);

        public ColorDefinition DirtyIndicatorColor { get; set; } = new ColorDefinition(243, 139, 168, 255); // e.g., Red

        // public float ButtonCornerRadius { get; set; } = 4f; // For RoundedRectangleShape if used


        public SkinProfile()
        {
            // Ensure default ColorDefinition objects are created if not set by deserializer
            // This helps if a skin.json is missing some of these newer color properties
            BackgroundColor ??= new ColorDefinition(0, 0, 0, 255);
            KeyDefaultColor ??= new ColorDefinition(50, 50, 50, 255);
            KeyPressedColor ??= new ColorDefinition(150, 150, 150, 255);
            KeyOutlineColor ??= new ColorDefinition(220, 220, 220, 255);
            KeyLabelColor ??= new ColorDefinition(255, 255, 255, 255);
            CounterColor ??= new ColorDefinition(0, 200, 200, 255);
            GlitchColor ??= new ColorDefinition(255,0,0,150);
            TapEffectColor ??= new ColorDefinition(255, 255, 255, 200);

            PanelBackgroundColor ??= new ColorDefinition(30, 30, 46, 235);
            PanelTextColor ??= new ColorDefinition(205, 214, 244, 255);
            PanelHeaderTextColor ??= new ColorDefinition(203, 166, 247, 255);
            ControlBackgroundColor ??= new ColorDefinition(49, 50, 68, 255);
            ControlOutlineColor ??= new ColorDefinition(88, 91, 112, 255);
            ControlTextColor ??= new ColorDefinition(186, 194, 222, 255);
            ControlAccentColor ??= new ColorDefinition(137, 180, 250, 255);
            ButtonBackgroundColor ??= new ColorDefinition(69, 71, 90, 255);
            ButtonTextColor ??= new ColorDefinition(205, 214, 244, 255);
            ButtonHoverBackgroundColor ??= new ColorDefinition(88, 91, 112, 255);
            DirtyIndicatorColor ??= new ColorDefinition(243, 139, 168, 255);
        }
    }
}
