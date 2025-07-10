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
        private float maxLifetime = 0.5f; // seconds, can be overridden by profile
        private Color startColor;
        private Color endColor;
        private float initialScale = 1.0f; // Used for animation based on profile setting

        // Constructor now takes SkinProfile for color/shape and Profile for general settings like KeySize
        public TapEffect(Vector2f center, string shapeTypeFromSkin, Profile generalProfile, SkinProfile skinProfile)
        {
            lifetime = new Clock();
            // Size of the tap effect could be based on KeySize or a new SkinProfile property
            float effectVisualSize = generalProfile.KeySize / 2f; // Example size, could be skin-dependent

            this.startColor = skinProfile.TapEffectColor.SfmlColor;
            this.endColor = new Color(startColor.R, startColor.G, startColor.B, 0); // Fade to transparent

            if (shapeTypeFromSkin.Equals("Circle", StringComparison.OrdinalIgnoreCase))
            {
                var circle = new CircleShape(effectVisualSize / 2) // Radius
                {
                    Origin = new Vector2f(effectVisualSize/2, effectVisualSize/2),
                    Position = center,
                    FillColor = this.startColor
                };
                Shape = circle;
            }
            else // Default to Rectangle / Square
            {
                var rect = new RectangleShape(new Vector2f(effectVisualSize, effectVisualSize))
                {
                    Origin = new Vector2f(effectVisualSize/2, effectVisualSize/2),
                    Position = center,
                    FillColor = this.startColor
                };
                Shape = rect;
            }
            // SetScale and SetDuration will be called externally using generalProfile settings
        }

        public void SetDuration(float duration)
        {
            maxLifetime = duration;
        }

        public void SetScale(float newInitialScale) // This scale is the target scale for animation
        {
            this.initialScale = newInitialScale; // Store the profile's desired max scale for the pop effect
            if(Shape != null) Shape.Scale = new Vector2f(1f,1f); // Start at normal size
        }

        // SetColor is now handled by constructor using SkinProfile.TapEffectColor
        // public void SetColor(Color color) { ... }

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

            // Animate scale: grow to 'initialScale' then shrink back.
            // This is a simple grow then hold, then fade. A bounce would be more complex.
            // For a pop effect: scale up quickly, then fade out.
            // Let's try scaling up to 'initialScale' over the first half of life, then just fade.
            float scaleRatio = Math.Min(1f, (elapsed / (maxLifetime / 2f))); // Reach max scale at half life
            float currentAnimatedScale = 1.0f + scaleRatio * (initialScale - 1.0f);

            // If we want it to shrink after reaching peak:
            // float peakTime = maxLifetime * 0.3f; // Time to reach peak scale
            // if (elapsed < peakTime) {
            //     currentAnimatedScale = 1.0f + (elapsed / peakTime) * (initialScale - 1.0f);
            // } else {
            //     currentAnimatedScale = initialScale - ((elapsed - peakTime) / (maxLifetime - peakTime)) * (initialScale - 1.0f);
            //     currentAnimatedScale = Math.Max(0.1f, currentAnimatedScale); // Don't shrink to zero scale
            // }

            Shape.Scale = new Vector2f(currentAnimatedScale, currentAnimatedScale);

            return true;
        }
    }


    public class KeyDefinition
    {
        public Keyboard.Key SfmlKey { get; }
        public Mouse.Button MouseButton { get; }
        public bool IsMouseKey { get; }
        public RectangleShape VisualKeyShape { get; set; }
        public Text KeyLabel { get; set; }
        // Colors are now derived from SkinProfile
        private Color currentDefaultColor;
        private Color currentPressedColor;
        public bool IsPressed { get; private set; }
        public int PressCount { get; private set; }
        private SkinProfile skin; // Keep a reference to apply styles

        // For standard keys
        public KeyDefinition(Keyboard.Key key, Vector2f position, Vector2f size, string label, Font font, Profile generalProfile, SkinProfile skinProfile)
        {
            SfmlKey = key;
            IsMouseKey = false;
            this.skin = skinProfile;

            currentDefaultColor = skin.KeyDefaultColor.SfmlColor;
            currentPressedColor = skin.KeyPressedColor.SfmlColor;

            VisualKeyShape = new RectangleShape(size) {
                Position = position,
                FillColor = currentDefaultColor,
                OutlineColor = skin.KeyOutlineColor.SfmlColor,
                OutlineThickness = generalProfile.OutlineThickness // Outline thickness from general profile
            };
            KeyLabel = new Text(label, font, (uint)(size.Y * 0.6f)) { FillColor = skin.KeyLabelColor.SfmlColor };
            KeyLabel.Position = new Vector2f(
                position.X + (size.X - KeyLabel.GetGlobalBounds().Width) / 2,
                position.Y + (size.Y - KeyLabel.GetGlobalBounds().Height) / 2 - KeyLabel.GetLocalBounds().Top /2 // Center label
            );
        }

        // For mouse buttons
        public KeyDefinition(Mouse.Button button, Vector2f position, Vector2f size, string label, Font font, Profile generalProfile, SkinProfile skinProfile)
        {
            MouseButton = button;
            IsMouseKey = true;
            this.skin = skinProfile;

            currentDefaultColor = skin.KeyDefaultColor.SfmlColor;
            currentPressedColor = skin.KeyPressedColor.SfmlColor;

            VisualKeyShape = new RectangleShape(size) {
                Position = position,
                FillColor = currentDefaultColor,
                OutlineColor = skin.KeyOutlineColor.SfmlColor,
                OutlineThickness = generalProfile.OutlineThickness // Outline thickness from general profile
            };
            KeyLabel = new Text(label, font, (uint)(size.Y * 0.6f)) { FillColor = skin.KeyLabelColor.SfmlColor };
            KeyLabel.Position = new Vector2f(
                position.X + (size.X - KeyLabel.GetGlobalBounds().Width) / 2,
                position.Y + (size.Y - KeyLabel.GetGlobalBounds().Height) / 2 - KeyLabel.GetLocalBounds().Top /2 // Center label
            );
        }

        public void UpdateSkin(SkinProfile newSkin, Font newFont, Profile generalProfile)
        {
            this.skin = newSkin;
            currentDefaultColor = skin.KeyDefaultColor.SfmlColor;
            currentPressedColor = skin.KeyPressedColor.SfmlColor;

            VisualKeyShape.OutlineColor = skin.KeyOutlineColor.SfmlColor;
            VisualKeyShape.OutlineThickness = generalProfile.OutlineThickness; // Update if this can change too

            KeyLabel.Font = newFont;
            KeyLabel.FillColor = skin.KeyLabelColor.SfmlColor;
            // Recalculate label position if size or font changed significantly
            KeyLabel.Position = new Vector2f(
                VisualKeyShape.Position.X + (VisualKeyShape.Size.X - KeyLabel.GetGlobalBounds().Width) / 2,
                VisualKeyShape.Position.Y + (VisualKeyShape.Size.Y - KeyLabel.GetGlobalBounds().Height) / 2 - KeyLabel.GetLocalBounds().Top /2
            );

            // Update fill color based on current state
            VisualKeyShape.FillColor = IsPressed ? currentPressedColor : currentDefaultColor;
        }


        public void UpdateState(bool justPressed)
        {
            bool currentlyPressed = IsMouseKey ? Mouse.IsButtonPressed(MouseButton) : Keyboard.IsKeyPressed(SfmlKey);
            if (currentlyPressed)
            {
                VisualKeyShape.FillColor = currentPressedColor;
                if (justPressed && !IsPressed)
                {
                    PressCount++;
                }
            }
            else
            {
                VisualKeyShape.FillColor = currentDefaultColor;
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
                Text countText = new Text(PressCount.ToString(), KeyLabel.Font, 12) // Font size for counter could be skinable
                {
                    FillColor = skin.CounterColor.SfmlColor, // Use CounterColor from SkinProfile
                    Position = new Vector2f(VisualKeyShape.Position.X + 5, VisualKeyShape.Position.Y + 5) // Positioning could be more dynamic/skinnable
                };
                window.Draw(countText);
            }
        }
    }


    public class CustomUI
    {
        private Profile generalProfile; // General settings like FPS, global effect toggles
        private SkinProfile currentSkin;  // Skin-specific settings like colors, fonts
        private RenderWindow window;
        private AudioAnalyzer audioAnalyzer;
        private Font uiFont;

        private Clock glitchClock = new Clock();
        private List<GlitchBar> glitchBars = new List<GlitchBar>();
        private List<PixelationEffect> pixelEffects = new List<PixelationEffect>();
        private List<TapEffect> tapEffects = new List<TapEffect>();

        private Dictionary<string, KeyDefinition> keyMap = new Dictionary<string, KeyDefinition>();

        public CustomUI(Profile generalProfile, SkinProfile initialSkin, RenderWindow window, AudioAnalyzer audioAnalyzer, Font font)
        {
            this.generalProfile = generalProfile;
            this.currentSkin = initialSkin;
            this.window = window;
            this.audioAnalyzer = audioAnalyzer;
            this.uiFont = font; // Initial font
            InitializeKeys();
        }

        public void UpdateSkin(SkinProfile newSkin, Font newFont)
        {
            this.currentSkin = newSkin;
            this.uiFont = newFont; // Update font
            // Re-initialize keys or update their visual properties based on the new skin
            RecalculateLayout(); // This already clears and re-initializes keys
            // Update other skin-dependent elements if any
            Console.WriteLine($"CustomUI skin updated to: {newSkin.SkinName}");
        }

        private void InitializeKeys()
        {
            if (uiFont == null)
            {
                Console.WriteLine("Cannot initialize keys in CustomUI: uiFont is null.");
                return;
            }
            keyMap.Clear(); // Clear existing keys before re-initializing

            // Key layout and sizing now primarily from generalProfile, colors from currentSkin.
            // TODO: Consider moving KeySize, Margin to SkinProfile if they should be skin-dependent. For now, they are global.
            float currentX = generalProfile.Margin;
            // Ensure window reference is valid and window has non-zero size for Y calculation.
            float keyY = (window != null && window.Size.Y > 0) ? window.Size.Y - generalProfile.KeySize - generalProfile.Margin : 100; // Fallback Y
            keyY = Math.Max(0, keyY); // Ensure keyY is not negative if window is too small

            float keySize = generalProfile.KeySize;

            // This is a very basic layout. A more robust system would read keybinds from generalProfile/config
            // The specific colors like Key1Color, Key2Color from generalProfile are now OBSOLETE.
            // KeyDefinition will use currentSkin.KeyDefaultColor, currentSkin.KeyPressedColor etc.
            AddKey("D", Keyboard.Key.D, currentX, keyY, keySize);
            currentX += keySize + generalProfile.Margin;
            AddKey("F", Keyboard.Key.F, currentX, keyY, keySize);
            currentX += keySize + generalProfile.Margin;
            AddKey("J", Keyboard.Key.J, currentX, keyY, keySize);
            currentX += keySize + generalProfile.Margin;
            AddKey("K", Keyboard.Key.K, currentX, keyY, keySize);
            currentX += keySize + generalProfile.Margin;

            // Example Mouse Buttons
            AddKey("M1", Mouse.Button.Left, currentX, keyY, keySize);
            currentX += keySize + generalProfile.Margin;
            AddKey("M2", Mouse.Button.Right, currentX, keyY, keySize);
        }

        // Removed Color parameter, as KeyDefinition will get its colors from the SkinProfile
        private void AddKey(string name, Keyboard.Key sfmlKey, float x, float y, float size)
        {
            if (uiFont == null) return; // Guard against null font
            var keyDef = new KeyDefinition(sfmlKey, new Vector2f(x,y), new Vector2f(size,size), name, uiFont, generalProfile, currentSkin);
            // Specific key coloring (e.g. Key1Color) from the old profile is not directly used here anymore.
            // The skin provides KeyDefaultColor, KeyPressedColor, etc.
            // If per-key custom colors within a skin are needed later, SkinProfile and KeyDefinition would need expansion.
            keyMap[name] = keyDef;
        }

        private void AddKey(string name, Mouse.Button mouseButton, float x, float y, float size)
        {
            if (uiFont == null) return; // Guard against null font
            var keyDef = new KeyDefinition(mouseButton, new Vector2f(x,y), new Vector2f(size,size), name, uiFont, generalProfile, currentSkin);
            keyMap[name] = keyDef;
        }


        public void Update()
        {
            // Use generalProfile for global effect toggles and skin for effect details
            bool beatDetected = generalProfile.AudioReactive ? audioAnalyzer.OnBeat() : true;

            foreach (var kvp in keyMap)
            {
                var keyDef = kvp.Value;
                bool isJustPressed = keyDef.CheckJustPressed();
                keyDef.UpdateState(isJustPressed);

                if (isJustPressed && (beatDetected || !generalProfile.AudioReactive))
                {
                    if (generalProfile.EnableTapEffects)
                    {
                        var center = new Vector2f(
                            keyDef.VisualKeyShape.Position.X + keyDef.VisualKeyShape.Size.X / 2,
                            keyDef.VisualKeyShape.Position.Y + keyDef.VisualKeyShape.Size.Y / 2
                        );
                        // TapEffect now takes SkinProfile to get its color/shape
                        var tapEffect = new TapEffect(center, currentSkin.TapEffectShape, generalProfile, currentSkin);
                        tapEffect.SetDuration(generalProfile.TapEffectDuration); // Duration from general profile
                        tapEffect.SetScale(generalProfile.TapEffectScale);       // Scale from general profile
                        // Color is set within TapEffect constructor from currentSkin
                        tapEffects.Add(tapEffect);
                    }

                    if (generalProfile.EnableGlitch && glitchClock.ElapsedTime.AsSeconds() >= 1f / Math.Max(1, generalProfile.GlitchFrequency))
                    {
                        float glitchY = (float)new Random().NextDouble() * window.Size.Y;
                        // GlitchBar now takes SkinProfile for color
                        glitchBars.Add(new GlitchBar((uint)window.Size.X, glitchY, generalProfile, currentSkin));
                        glitchClock.Restart();
                    }
                    if (generalProfile.EnablePixelation)
                    {
                        // PixelationEffect now takes SkinProfile (though not directly used for color yet)
                        pixelEffects.Add(new PixelationEffect(window, generalProfile.PixelSize, generalProfile, currentSkin));
                    }
                }
            }

            tapEffects.RemoveAll(e => !e.Update());
            glitchBars.RemoveAll(g => !g.Update());
            pixelEffects.RemoveAll(p => !p.IsAlive());
        }

        public void Render(RenderWindow targetWindow)
        {
            foreach (var kvp in keyMap)
            {
                kvp.Value.Draw(targetWindow, generalProfile.Counter); // Counter toggle from general profile
            }

            foreach (var e in tapEffects) targetWindow.Draw(e.Shape);

            if (generalProfile.EnableGlitch)
            {
                foreach (var g in glitchBars) g.Draw(targetWindow);
            }

            if (generalProfile.EnablePixelation)
            {
                foreach (var p in pixelEffects) p.Apply(targetWindow);
            }
        }

        public void RecalculateLayout()
        {
            // This will use the latest generalProfile, currentSkin, and uiFont
            InitializeKeys();
        }
    }
}
