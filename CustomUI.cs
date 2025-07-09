using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;
using SFML.Window; // Required for Keyboard, Mouse

namespace KeyOverlayEnhanced
{
    // Assuming TapEffect class exists or will be created, e.g. in Effects.cs
    public class TapEffect
    {
        public Shape Shape { get; private set; }
        private Clock lifetime;
        private float maxLifetime = 0.5f; // seconds
        private Color startColor = Color.White;
        private Color endColor = new Color(Color.White.R, Color.White.G, Color.White.B, 0);

        public TapEffect(Vector2f center, string shapeType, Profile profile)
        {
            lifetime = new Clock();
            float keySize = profile.KeySize / 2f; // Example size

            if (shapeType.Equals("Circle", StringComparison.OrdinalIgnoreCase))
            {
                var circle = new CircleShape(keySize / 2) // Radius
                {
                    Origin = new Vector2f(keySize/2, keySize/2),
                    Position = center,
                    FillColor = startColor
                };
                Shape = circle;
            }
            else // Default to Rectangle / Square
            {
                var rect = new RectangleShape(new Vector2f(keySize, keySize))
                {
                    Origin = new Vector2f(keySize/2, keySize/2),
                    Position = center,
                    FillColor = startColor
                };
                Shape = rect;
            }
        }

        public bool Update() // Returns false if dead
        {
            float elapsed = lifetime.ElapsedTime.AsSeconds();
            if (elapsed >= maxLifetime) return false;

            float ratio = elapsed / maxLifetime;
            byte r = (byte)(startColor.R * (1 - ratio) + endColor.R * ratio);
            byte g = (byte)(startColor.G * (1 - ratio) + endColor.G * ratio);
            byte b = (byte)(startColor.B * (1 - ratio) + endColor.B * ratio);
            byte a = (byte)(startColor.A * (1 - ratio) + endColor.A * ratio);
            Shape.FillColor = new Color(r,g,b,a);

            // Optional: Add scaling or other animations
            float scale = 1.0f + ratio * 0.5f; // Grow slightly
            Shape.Scale = new Vector2f(scale,scale);

            return true;
        }
    }


    public class KeyDefinition
    {
        public Keyboard.Key SfmlKey { get; }
        public Mouse.Button MouseButton { get; }
        public bool IsMouseKey { get; }
        public RectangleShape VisualKeyShape { get; set; } // The visual representation of the key background
        public Text KeyLabel { get; set; }
        public Color DefaultColor { get; set; } = new Color(50,50,50);
        public Color PressedColor { get; set; } = new Color(150,150,150);
        public bool IsPressed { get; private set; }
        public int PressCount { get; private set; }

        // For standard keys
        public KeyDefinition(Keyboard.Key key, Vector2f position, Vector2f size, string label, Font font, Profile config)
        {
            SfmlKey = key;
            IsMouseKey = false;
            VisualKeyShape = new RectangleShape(size) {
                Position = position,
                FillColor = DefaultColor,
                OutlineColor = Color.White,
                OutlineThickness = config.OutlineThickness
            };
            KeyLabel = new Text(label, font, (uint)(size.Y * 0.6f)) { FillColor = Color.White };
            KeyLabel.Position = new Vector2f(
                position.X + (size.X - KeyLabel.GetGlobalBounds().Width) / 2,
                position.Y + (size.Y - KeyLabel.GetGlobalBounds().Height) / 2 - KeyLabel.GetLocalBounds().Top /2
            );
        }

        // For mouse buttons (simplified)
        public KeyDefinition(Mouse.Button button, Vector2f position, Vector2f size, string label, Font font, Profile config)
        {
            MouseButton = button;
            IsMouseKey = true;
            VisualKeyShape = new RectangleShape(size) {
                Position = position,
                FillColor = DefaultColor,
                OutlineColor = Color.White,
                OutlineThickness = config.OutlineThickness
            };
            KeyLabel = new Text(label, font, (uint)(size.Y * 0.6f)) { FillColor = Color.White };
             KeyLabel.Position = new Vector2f(
                position.X + (size.X - KeyLabel.GetGlobalBounds().Width) / 2,
                position.Y + (size.Y - KeyLabel.GetGlobalBounds().Height) / 2 - KeyLabel.GetLocalBounds().Top /2
            );
        }

        public void UpdateState(bool justPressed)
        {
             bool currentlyPressed = IsMouseKey ? Mouse.IsButtonPressed(MouseButton) : Keyboard.IsKeyPressed(SfmlKey);
            if (currentlyPressed)
            {
                VisualKeyShape.FillColor = PressedColor;
                if (justPressed && !IsPressed) // Check if it was a new press
                {
                    PressCount++;
                }
            }
            else
            {
                VisualKeyShape.FillColor = DefaultColor;
            }
            IsPressed = currentlyPressed;
        }

        public bool CheckJustPressed()
        {
            bool currentlyPressed = IsMouseKey ? Mouse.IsButtonPressed(MouseButton) : Keyboard.IsKeyPressed(SfmlKey);
            bool wasPressed = IsPressed;
            // Update IsPressed for next frame's "wasPressed"
            // IsPressed = currentlyPressed; // This will be handled in UpdateState
            return currentlyPressed && !wasPressed;
        }


        public void Draw(RenderWindow window, bool counterEnabled)
        {
            window.Draw(VisualKeyShape);
            window.Draw(KeyLabel);
            if(counterEnabled)
            {
                // Simplified counter display on the key itself
                Text countText = new Text(PressCount.ToString(), KeyLabel.Font, 12)
                {
                    FillColor = Color.Cyan,
                    Position = new Vector2f(VisualKeyShape.Position.X + 5, VisualKeyShape.Position.Y + 5)
                };
                window.Draw(countText);
            }
        }
    }


    public class CustomUI
    {
        private Profile profile;
        private RenderWindow window; // For pixelation effect primarily
        private AudioAnalyzer audioAnalyzer; // Needs to be passed in or instantiated
        private Font uiFont;

        private Clock glitchClock = new Clock();
        private List<GlitchBar> glitchBars = new List<GlitchBar>();
        private List<PixelationEffect> pixelEffects = new List<PixelationEffect>();
        private List<TapEffect> tapEffects = new List<TapEffect>();

        // Key definitions will replace the old _keyList from AppWindow
        private Dictionary<string, KeyDefinition> keyMap = new Dictionary<string, KeyDefinition>();


        public CustomUI(Profile profile, RenderWindow window, AudioAnalyzer audioAnalyzer, Font font)
        {
            this.profile = profile;
            this.window = window;
            this.audioAnalyzer = audioAnalyzer;
            this.uiFont = font;
            InitializeKeys(); // Setup key definitions based on profile or defaults
        }

        private void InitializeKeys()
        {
            // Example key setup, this should be made configurable later, perhaps from Profile
            // For now, using some common osu! standard mode keys + mouse buttons
            // Positions and sizes are placeholders and need to be calculated based on profile.KeySize, profile.Margin etc.

            float currentX = profile.Margin;
            float keyY = window.Size.Y - profile.KeySize - profile.Margin; // Position keys at the bottom
            float keySize = profile.KeySize;

            // This is a very basic layout. A more robust system would read keybinds from profile/config
            AddKey("D", Keyboard.Key.D, currentX, keyY, keySize);
            currentX += keySize + profile.Margin;
            AddKey("F", Keyboard.Key.F, currentX, keyY, keySize);
            currentX += keySize + profile.Margin;
            AddKey("J", Keyboard.Key.J, currentX, keyY, keySize);
            currentX += keySize + profile.Margin;
            AddKey("K", Keyboard.Key.K, currentX, keyY, keySize);
            currentX += keySize + profile.Margin;

            // Example Mouse Buttons
            AddKey("M1", Mouse.Button.Left, currentX, keyY, keySize);
            currentX += keySize + profile.Margin;
            AddKey("M2", Mouse.Button.Right, currentX, keyY, keySize);
        }

        private void AddKey(string name, Keyboard.Key sfmlKey, float x, float y, float size)
        {
            keyMap[name] = new KeyDefinition(sfmlKey, new Vector2f(x,y), new Vector2f(size,size), name, uiFont, profile);
        }
        private void AddKey(string name, Mouse.Button mouseButton, float x, float y, float size)
        {
            keyMap[name] = new KeyDefinition(mouseButton, new Vector2f(x,y), new Vector2f(size,size), name, uiFont, profile);
        }


        public void Update()
        {
            bool beatDetected = profile.AudioReactive ? audioAnalyzer.OnBeat() : true;

            foreach (var kvp in keyMap)
            {
                var keyDef = kvp.Value;
                bool isJustPressed = keyDef.CheckJustPressed(); // Check before updating state
                keyDef.UpdateState(isJustPressed); // Update visual state (fill color)

                if (isJustPressed && (beatDetected || !profile.AudioReactive))
                {
                    var center = new Vector2f(
                        keyDef.VisualKeyShape.Position.X + keyDef.VisualKeyShape.Size.X / 2,
                        keyDef.VisualKeyShape.Position.Y + keyDef.VisualKeyShape.Size.Y / 2
                    );
                    tapEffects.Add(new TapEffect(center, profile.TapShape, profile));

                    if (profile.EnableGlitch && glitchClock.ElapsedTime.AsSeconds() >= 1f / Math.Max(1,profile.GlitchFrequency))
                    {
                        // GlitchBar Y position can be randomized or fixed
                        float glitchY = (float)new Random().NextDouble() * window.Size.Y;
                        glitchBars.Add(new GlitchBar((uint)window.Size.X, glitchY, profile));
                        glitchClock.Restart();
                    }
                    if (profile.EnablePixelation)
                    {
                        // Pixelation might be resource-intensive, consider limiting how often it's added
                        pixelEffects.Add(new PixelationEffect(window, profile.PixelSize, profile));
                    }
                }
            }

            // Update and remove dead effects
            tapEffects.RemoveAll(e => !e.Update());
            glitchBars.RemoveAll(g => !g.Update());
            pixelEffects.RemoveAll(p => !p.IsAlive());
        }

        public void Render(RenderWindow targetWindow)
        {
            // Draw keys first
            foreach (var kvp in keyMap)
            {
                kvp.Value.Draw(targetWindow, profile.Counter);
            }

            // Then draw effects
            foreach (var e in tapEffects) targetWindow.Draw(e.Shape);

            if (profile.EnableGlitch)
            {
                foreach (var g in glitchBars) g.Draw(targetWindow); // Assuming GlitchBar has a Draw method
            }

            // Pixelation is applied as a post-processing step or draws over everything
            // If PixelationEffect.Apply modifies the window content directly or uses shaders, its call might be different
            if (profile.EnablePixelation)
            {
                foreach (var p in pixelEffects) p.Apply(targetWindow);
            }
        }

        // Call this if window is resized or key layout settings change
        public void RecalculateLayout()
        {
            // Clear existing keys and re-initialize based on new profile settings (KeySize, Margin etc)
            keyMap.Clear();
            InitializeKeys();
        }
    }
}
