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

        // Animation state properties
        private bool isAnimatingPress = false;
        private bool isAnimatingRelease = false;
        private Clock animationClock = new Clock(); // For timing animations

        // These will be configurable later via SkinProfile, using defaults for now
        private float currentConfiguredAnimationDurationPress = 0.15f;
        private float currentConfiguredAnimationDurationRelease = 0.25f;
        private float currentConfiguredTargetScalePress = 1.15f;
        // private string currentEasingFunctionType = "EaseOutCubic"; // Obsolete: now using skin.KeyEasingFunctionPress/Release

        private float currentScale = 1.0f;
        private float animationStartScale = 1.0f; // Stores the scale at the beginning of an animation
        private Vector2f keyOriginalTopLeftPosition; // Store the initial top-left position for label calculation


        // For standard keys
        public KeyDefinition(Keyboard.Key key, Vector2f topLeftPosition, Vector2f size, string label, Font font, Profile generalProfile, SkinProfile skinProfile)
        {
            SfmlKey = key;
            IsMouseKey = false;
            this.skin = skinProfile;
            this.keyOriginalTopLeftPosition = topLeftPosition;

            currentDefaultColor = skin.KeyDefaultColor.SfmlColor;
            currentPressedColor = skin.KeyPressedColor.SfmlColor;

            VisualKeyShape = new RectangleShape(size) {
                Origin = new Vector2f(size.X / 2f, size.Y / 2f),
                Position = new Vector2f(topLeftPosition.X + size.X / 2f, topLeftPosition.Y + size.Y / 2f),
                FillColor = currentDefaultColor,
                OutlineColor = skin.KeyOutlineColor.SfmlColor,
                OutlineThickness = generalProfile.OutlineThickness
            };
            KeyLabel = new Text(label, font, (uint)(size.Y * 0.6f)) { FillColor = skin.KeyLabelColor.SfmlColor };
            UpdateLabelPosition();
        }

        // For mouse buttons
        public KeyDefinition(Mouse.Button button, Vector2f topLeftPosition, Vector2f size, string label, Font font, Profile generalProfile, SkinProfile skinProfile)
        {
            MouseButton = button;
            IsMouseKey = true;
            this.skin = skinProfile;
            this.keyOriginalTopLeftPosition = topLeftPosition;

            currentDefaultColor = skin.KeyDefaultColor.SfmlColor;
            currentPressedColor = skin.KeyPressedColor.SfmlColor;

            VisualKeyShape = new RectangleShape(size) {
                Origin = new Vector2f(size.X / 2f, size.Y / 2f),
                Position = new Vector2f(topLeftPosition.X + size.X / 2f, topLeftPosition.Y + size.Y / 2f),
                FillColor = currentDefaultColor,
                OutlineColor = skin.KeyOutlineColor.SfmlColor,
                OutlineThickness = generalProfile.OutlineThickness
            };
            KeyLabel = new Text(label, font, (uint)(size.Y * 0.6f)) { FillColor = skin.KeyLabelColor.SfmlColor };
            UpdateLabelPosition();
        }

        private void UpdateLabelPosition()
        {
            // Recalculate apparent top-left based on original top-left, size, and current scale.
            // This is simpler than using GetGlobalBounds() if the parent doesn't transform.
            float visualWidth = VisualKeyShape.Size.X * currentScale;
            float visualHeight = VisualKeyShape.Size.Y * currentScale;
            float apparentTopLeftX = keyOriginalTopLeftPosition.X + (VisualKeyShape.Size.X - visualWidth) / 2f;
            float apparentTopLeftY = keyOriginalTopLeftPosition.Y + (VisualKeyShape.Size.Y - visualHeight) / 2f;

            KeyLabel.Position = new Vector2f(
                apparentTopLeftX + (visualWidth - KeyLabel.GetGlobalBounds().Width) / 2f,
                apparentTopLeftY + (visualHeight - KeyLabel.GetGlobalBounds().Height) / 2f - KeyLabel.GetLocalBounds().Top
            );
        }

        public void UpdateSkin(SkinProfile newSkin, Font newFont, Profile generalProfile)
        {
            this.skin = newSkin;
            currentDefaultColor = skin.KeyDefaultColor.SfmlColor;
            currentPressedColor = skin.KeyPressedColor.SfmlColor;

            VisualKeyShape.OutlineColor = skin.KeyOutlineColor.SfmlColor;
            VisualKeyShape.OutlineThickness = generalProfile.OutlineThickness;

            KeyLabel.Font = newFont;
            KeyLabel.FillColor = skin.KeyLabelColor.SfmlColor;
            // VisualKeyShape.Position needs to be reset to its centered position if not already
            VisualKeyShape.Position = new Vector2f(keyOriginalTopLeftPosition.X + VisualKeyShape.Size.X / 2f, keyOriginalTopLeftPosition.Y + VisualKeyShape.Size.Y / 2f);
            VisualKeyShape.Origin = new Vector2f(VisualKeyShape.Size.X / 2f, VisualKeyShape.Size.Y / 2f);
            UpdateLabelPosition(); // Recalculate label based on new font/skin

            VisualKeyShape.FillColor = IsPressed ? currentPressedColor : currentDefaultColor;

            // Update animation parameters from the new skin
            this.currentConfiguredAnimationDurationPress = newSkin.KeyAnimationDurationPress;
            this.currentConfiguredAnimationDurationRelease = newSkin.KeyAnimationDurationRelease;
            this.currentConfiguredTargetScalePress = newSkin.KeyTargetScalePress;
            // currentEasingFunctionType is now split into press and release, will be used directly from skin property.
        }

        private void UpdateAnimation()
        {
            if (!skin.KeyEnableAnimation) // Check if animation is enabled for the current skin
            {
                currentScale = IsPressed ? currentConfiguredTargetScalePress : 1.0f;
                VisualKeyShape.Scale = new Vector2f(currentScale, currentScale);
                UpdateLabelPosition();
                isAnimatingPress = false; // Ensure animation flags are false
                isAnimatingRelease = false;
                return;
            }

            float elapsed = animationClock.ElapsedTime.AsSeconds();
            float t = 0f;

            if (isAnimatingPress)
            {
                t = Math.Min(1f, elapsed / currentConfiguredAnimationDurationPress);
                float easedT = EasingFunctions.ApplyEasing(skin.KeyEasingFunctionPress, t); // Use press easing func
                currentScale = animationStartScale + (currentConfiguredTargetScalePress - animationStartScale) * easedT;

                if (t >= 1f)
                {
                    isAnimatingPress = false;
                    currentScale = currentConfiguredTargetScalePress;
                }
            }
            else if (isAnimatingRelease)
            {
                t = Math.Min(1f, elapsed / currentConfiguredAnimationDurationRelease);
                float easedT = EasingFunctions.ApplyEasing(skin.KeyEasingFunctionRelease, t); // Use release easing func
                currentScale = animationStartScale + (1.0f - animationStartScale) * easedT;

                if (t >= 1f)
                {
                    isAnimatingRelease = false;
                    currentScale = 1.0f; // Snap to final scale
                }
            }
            // Origin is already set to center in constructor. Position is also set to center.
            // Only scale needs to be applied here.
            VisualKeyShape.Scale = new Vector2f(currentScale, currentScale);
            UpdateLabelPosition(); // Update label position based on new scale
        }


        public void UpdateState(bool justPressed)
        {
            bool previouslyPressed = IsPressed;
            IsPressed = IsMouseKey ? Mouse.IsButtonPressed(MouseButton) : Keyboard.IsKeyPressed(SfmlKey);

            UpdateAnimation(); // Handle ongoing or finishing animations

            if (IsPressed && !previouslyPressed) // Key down event
            {
                VisualKeyShape.FillColor = currentPressedColor;
                PressCount++;

                animationStartScale = currentScale; // Capture current scale for animation base
                isAnimatingPress = true;
                isAnimatingRelease = false;
                animationClock.Restart();
            }
            else if (!IsPressed && previouslyPressed) // Key up event
            {
                VisualKeyShape.FillColor = currentDefaultColor;

                animationStartScale = currentScale; // Capture current scale for animation base
                isAnimatingRelease = true;
                isAnimatingPress = false;
                animationClock.Restart();
            }
            else if (!IsPressed && !isAnimatingRelease) // Not pressed, not animating release (steady state)
            {
                 VisualKeyShape.FillColor = currentDefaultColor;
                 currentScale = 1.0f; // Ensure it's reset if no animation is running
                 VisualKeyShape.Scale = new Vector2f(currentScale, currentScale);
            }
             else if (IsPressed && !isAnimatingPress) // Pressed, not animating press (steady state)
            {
                VisualKeyShape.FillColor = currentPressedColor;
                currentScale = currentConfiguredTargetScalePress; // Stay scaled while pressed
                VisualKeyShape.Scale = new Vector2f(currentScale, currentScale);
            }
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
