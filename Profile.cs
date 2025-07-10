#nullable enable
using System.IO;
using System.Text.Json;
using SFML.Graphics; // Required for Color

namespace KeyOverlayEnhanced
{
    public enum OsuMode { Standard, Taiko, Catch, Mania } // Assuming this might be used later

    public class Profile
    {
        // original settings...
        public bool Fading { get; set; } = true;
        public bool Counter { get; set; } = true;
        public int BarSpeed { get; set; } = 600;
        public int KeySize { get; set; } = 70;
        public int Margin { get; set; } = 25;
        public int OutlineThickness { get; set; } = 5;
        public int FPS { get; set; } = 60;

        // Superseded by SkinProfile:
        // public byte BackgroundColorR { get; set; } = 0;
        // public byte BackgroundColorG { get; set; } = 0;
        // public byte BackgroundColorB { get; set; } = 0;
        // public byte BackgroundColorA { get; set; } = 255;
        // [System.Text.Json.Serialization.JsonIgnore]
        // public Color BackgroundColor { ... }

        // Superseded by SkinProfile.FontFileName:
        // public string FontPath { get; set; } = "Resources/consolab.ttf";
        // Superseded by SkinProfile.TapEffectShape:
        // public string TapShape { get; set; } = "Circle";

        // Effect toggles and general parameters remain in Profile.cs
        public bool EnableGlitch { get; set; } = true;
        public bool EnablePixelation { get; set; } = true;
        public int GlitchFrequency { get; set; } = 5; // bars per second
        public int PixelSize { get; set; } = 8;

        // new trigger mode
        public bool AudioReactive { get; set; } = false;

        // Superseded by SkinProfile:
        // Individual key colors (Key1Color to Key7Color) are no longer here.
        // SkinProfile defines KeyDefaultColor, KeyPressedColor, etc. which apply to all keys.
        // If per-key customization within a skin is needed, SkinProfile would need a dictionary or list.

        // Superseded by SkinProfile.GlitchColor:
        // public byte GlitchColorR { get; set; } = 255;
        // public byte GlitchColorG { get; set; } = 0;
        // public byte GlitchColorB { get; set; } = 0;
        // public byte GlitchColorA { get; set; } = 150;

        // Superseded by SkinProfile.TapEffectColor:
        // public byte TapEffectColorR { get; set; } = 255;
        // public byte TapEffectColorG { get; set; } = 255;
        // public byte TapEffectColorB { get; set; } = 255;
        // public byte TapEffectColorA { get; set; } = 255;

        // General effect parameters remain:
        public float TapEffectDuration { get; set; } = 0.5f;
        public float TapEffectScale { get; set; } = 1.5f;
        public bool EnableTapEffects { get; set; } = true;
        public bool EnableBarEffects { get; set; } = true;
        public int BarHeight { get; set; } = 600;
        public float BarSpeedMultiplier { get; set; } = 1.0f;
        public bool EnableKeyGlow { get; set; } = true;
        public int GlowIntensity { get; set; } = 50;

        // Stores the directory name of the currently selected skin (e.g., "DefaultDark")
        public string CurrentSkinDirectoryName { get; set; } = "Default (Built-in)"; // Default to the built-in skin

        // Legacy properties for backward compatibility (these are now handled by SkinProfile)
        [System.Text.Json.Serialization.JsonIgnore]
        public Color BackgroundColor { get; set; } = new Color(0, 0, 0, 255);
        
        [System.Text.Json.Serialization.JsonIgnore]
        public Color GlitchColor { get; set; } = new Color(255, 0, 0, 150);
        
        [System.Text.Json.Serialization.JsonIgnore]
        public Color TapEffectColor { get; set; } = new Color(255, 255, 255, 255);
        
        [System.Text.Json.Serialization.JsonIgnore]
        public string TapShape { get; set; } = "Circle";

        // Helper methods for colors are no longer needed here as these properties are removed.
        // e.g. [System.Text.Json.Serialization.JsonIgnore] public Color Key1Color { ... }

        public static Profile Load(string file)
        {
            if (!File.Exists(file)) return new Profile();
            var json = File.ReadAllText(file);
            return JsonSerializer.Deserialize<Profile>(json) ?? new Profile();
        }
        public void Save(string file)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(file, json);
        }
    }
}
