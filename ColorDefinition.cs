using System.Text.Json.Serialization;
using SFML.Graphics;

namespace KeyOverlayEnhanced
{
    public class ColorDefinition
    {
        public byte R { get; set; } = 255;
        public byte G { get; set; } = 255;
        public byte B { get; set; } = 255;
        public byte A { get; set; } = 255;

        public ColorDefinition() { }

        public ColorDefinition(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        [JsonIgnore]
        public Color SfmlColor
        {
            get => new Color(R, G, B, A);
            set
            {
                R = value.R;
                G = value.G;
                B = value.B;
                A = value.A;
            }
        }

        // Helper to create a default ColorDefinition from an SFML Color
        public static ColorDefinition FromSfmlColor(Color color)
        {
            return new ColorDefinition(color.R, color.G, color.B, color.A);
        }
    }
}
