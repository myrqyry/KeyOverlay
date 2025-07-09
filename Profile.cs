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
