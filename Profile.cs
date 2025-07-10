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

        // SFML Color doesn't serialize well by default with System.Text.Json.
        // We'll store them as components and provide a Color property.
        public byte BackgroundColorR { get; set; } = 0;
        public byte BackgroundColorG { get; set; } = 0;
        public byte BackgroundColorB { get; set; } = 0;
        public byte BackgroundColorA { get; set; } = 255;

        [System.Text.Json.Serialization.JsonIgnore]
        public Color BackgroundColor
        {
            get => new Color(BackgroundColorR, BackgroundColorG, BackgroundColorB, BackgroundColorA);
            set
            {
                BackgroundColorR = value.R;
                BackgroundColorG = value.G;
                BackgroundColorB = value.B;
                BackgroundColorA = value.A;
            }
        }

        public string FontPath { get; set; } = "Resources/consolab.ttf";
        public string TapShape { get; set; } = "Circle";

        // new effect settings
        public bool EnableGlitch { get; set; } = true;
        public bool EnablePixelation { get; set; } = true;
        public int GlitchFrequency { get; set; } = 5; // bars per second
        public int PixelSize { get; set; } = 8;

        // new trigger mode
        public bool AudioReactive { get; set; } = false;

        // NEW: Individual key colors
        public byte Key1ColorR { get; set; } = 255;
        public byte Key1ColorG { get; set; } = 255;
        public byte Key1ColorB { get; set; } = 255;
        public byte Key1ColorA { get; set; } = 255;

        public byte Key2ColorR { get; set; } = 0;
        public byte Key2ColorG { get; set; } = 255;
        public byte Key2ColorB { get; set; } = 255;
        public byte Key2ColorA { get; set; } = 255;

        public byte Key3ColorR { get; set; } = 255;
        public byte Key3ColorG { get; set; } = 255;
        public byte Key3ColorB { get; set; } = 255;
        public byte Key3ColorA { get; set; } = 255;

        public byte Key4ColorR { get; set; } = 255;
        public byte Key4ColorG { get; set; } = 255;
        public byte Key4ColorB { get; set; } = 0;
        public byte Key4ColorA { get; set; } = 255;

        public byte Key5ColorR { get; set; } = 255;
        public byte Key5ColorG { get; set; } = 255;
        public byte Key5ColorB { get; set; } = 255;
        public byte Key5ColorA { get; set; } = 255;

        public byte Key6ColorR { get; set; } = 0;
        public byte Key6ColorG { get; set; } = 255;
        public byte Key6ColorB { get; set; } = 255;
        public byte Key6ColorA { get; set; } = 255;

        public byte Key7ColorR { get; set; } = 255;
        public byte Key7ColorG { get; set; } = 255;
        public byte Key7ColorB { get; set; } = 255;
        public byte Key7ColorA { get; set; } = 255;

        // NEW: Effect colors
        public byte GlitchColorR { get; set; } = 255;
        public byte GlitchColorG { get; set; } = 0;
        public byte GlitchColorB { get; set; } = 0;
        public byte GlitchColorA { get; set; } = 150;

        public byte TapEffectColorR { get; set; } = 255;
        public byte TapEffectColorG { get; set; } = 255;
        public byte TapEffectColorB { get; set; } = 255;
        public byte TapEffectColorA { get; set; } = 255;

        // NEW: Additional settings
        public float TapEffectDuration { get; set; } = 0.5f;
        public float TapEffectScale { get; set; } = 1.5f;
        public bool EnableTapEffects { get; set; } = true;
        public bool EnableBarEffects { get; set; } = true;
        public int BarHeight { get; set; } = 600;
        public float BarSpeedMultiplier { get; set; } = 1.0f;
        public bool EnableKeyGlow { get; set; } = true;
        public int GlowIntensity { get; set; } = 50;

        // Helper methods for colors
        [System.Text.Json.Serialization.JsonIgnore]
        public Color Key1Color
        {
            get => new Color(Key1ColorR, Key1ColorG, Key1ColorB, Key1ColorA);
            set
            {
                Key1ColorR = value.R;
                Key1ColorG = value.G;
                Key1ColorB = value.B;
                Key1ColorA = value.A;
            }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        public Color Key2Color
        {
            get => new Color(Key2ColorR, Key2ColorG, Key2ColorB, Key2ColorA);
            set
            {
                Key2ColorR = value.R;
                Key2ColorG = value.G;
                Key2ColorB = value.B;
                Key2ColorA = value.A;
            }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        public Color Key3Color
        {
            get => new Color(Key3ColorR, Key3ColorG, Key3ColorB, Key3ColorA);
            set
            {
                Key3ColorR = value.R;
                Key3ColorG = value.G;
                Key3ColorB = value.B;
                Key3ColorA = value.A;
            }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        public Color Key4Color
        {
            get => new Color(Key4ColorR, Key4ColorG, Key4ColorB, Key4ColorA);
            set
            {
                Key4ColorR = value.R;
                Key4ColorG = value.G;
                Key4ColorB = value.B;
                Key4ColorA = value.A;
            }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        public Color Key5Color
        {
            get => new Color(Key5ColorR, Key5ColorG, Key5ColorB, Key5ColorA);
            set
            {
                Key5ColorR = value.R;
                Key5ColorG = value.G;
                Key5ColorB = value.B;
                Key5ColorA = value.A;
            }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        public Color Key6Color
        {
            get => new Color(Key6ColorR, Key6ColorG, Key6ColorB, Key6ColorA);
            set
            {
                Key6ColorR = value.R;
                Key6ColorG = value.G;
                Key6ColorB = value.B;
                Key6ColorA = value.A;
            }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        public Color Key7Color
        {
            get => new Color(Key7ColorR, Key7ColorG, Key7ColorB, Key7ColorA);
            set
            {
                Key7ColorR = value.R;
                Key7ColorG = value.G;
                Key7ColorB = value.B;
                Key7ColorA = value.A;
            }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        public Color GlitchColor
        {
            get => new Color(GlitchColorR, GlitchColorG, GlitchColorB, GlitchColorA);
            set
            {
                GlitchColorR = value.R;
                GlitchColorG = value.G;
                GlitchColorB = value.B;
                GlitchColorA = value.A;
            }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        public Color TapEffectColor
        {
            get => new Color(TapEffectColorR, TapEffectColorG, TapEffectColorB, TapEffectColorA);
            set
            {
                TapEffectColorR = value.R;
                TapEffectColorG = value.G;
                TapEffectColorB = value.B;
                TapEffectColorA = value.A;
            }
        }

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
