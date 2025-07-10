using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;
using SFML.Window; // Required for Event

namespace KeyOverlayEnhanced
{
    // Basic Control base class (can be expanded later)
    public abstract class Control
    {
        public FloatRect Bounds { get; protected set; }
        public abstract void Draw(RenderWindow window);
        public abstract void HandleEvent(Event e, Vector2f mousePos);
        public bool IsMouseOver(Vector2f mousePos) => Bounds.Contains(mousePos.X, mousePos.Y);
    }

    public class ButtonControl : Control
    {
        private RectangleShape shape;
        private Text label;
        private Action onClick;

        public ButtonControl(string text, Vector2f position, Vector2f size, Font font, Action onClick)
        {
            this.onClick = onClick;
            Bounds = new FloatRect(position, size);

            shape = new RectangleShape(size)
            {
                Position = position,
                FillColor = new Color(70, 70, 70),
                OutlineColor = Color.White,
                OutlineThickness = 1
            };

            label = new Text(text, font, 16)
            {
                Position = new Vector2f(position.X + 5, position.Y + 5),
                FillColor = Color.White
            };
        }

        public override void Draw(RenderWindow window)
        {
            window.Draw(shape);
            window.Draw(label);
        }

        public override void HandleEvent(Event e, Vector2f mousePos)
        {
            if (e.Type == EventType.MouseButtonPressed && e.MouseButton.Button == Mouse.Button.Left)
            {
                if (IsMouseOver(mousePos))
                {
                    onClick?.Invoke();
                }
            }
        }
        public void SetText(string text)
        {
            label.DisplayedString = text;
        }
    }

    public class ToggleControl : Control
    {
        private RectangleShape box;
        private Text label;
        private Action onToggle;
        private Func<bool> getter;
        private bool isChecked;

        public ToggleControl(string text, Vector2f position, Font font, Action onToggle, Func<bool> getter)
        {
            this.onToggle = onToggle;
            this.getter = getter;
            this.isChecked = getter();

            label = new Text(text, font, 16)
            {
                Position = new Vector2f(position.X + 25, position.Y),
                FillColor = Color.White
            };

            Bounds = new FloatRect(position, new Vector2f(20 + label.GetGlobalBounds().Width + 5, 20));


            box = new RectangleShape(new Vector2f(20, 20))
            {
                Position = position,
                FillColor = isChecked ? Color.Green : new Color(70, 70, 70),
                OutlineColor = Color.White,
                OutlineThickness = 1
            };
        }

        public void UpdateState()
        {
            isChecked = getter();
            box.FillColor = isChecked ? Color.Green : new Color(70, 70, 70);
        }


        public override void Draw(RenderWindow window)
        {
            window.Draw(box);
            window.Draw(label);
        }

        public override void HandleEvent(Event e, Vector2f mousePos)
        {
            if (e.Type == EventType.MouseButtonPressed && e.MouseButton.Button == Mouse.Button.Left)
            {
                if (IsMouseOver(mousePos))
                {
                    onToggle?.Invoke();
                    UpdateState();
                }
            }
        }
    }

    public class SliderControl : Control
    {
        private Text label;
        private RectangleShape track;
        private RectangleShape thumb;
        private float minValue, maxValue, currentValue;
        private Action<int> onValueChanged;
        private Func<int> getter;
        private Text valueText;
        private Font font;

        public SliderControl(string text, Vector2f position, float min, float max, Font font, Func<int> getter, Action<int> onValueChanged)
        {
            this.label = new Text(text, font, 16) { Position = position, FillColor = Color.White };
            this.minValue = min;
            this.maxValue = max;
            this.getter = getter;
            this.currentValue = getter();
            this.onValueChanged = onValueChanged;
            this.font = font;

            float trackWidth = 150;
            float trackHeight = 10;
            float thumbSize = 20;

            track = new RectangleShape(new Vector2f(trackWidth, trackHeight))
            {
                Position = new Vector2f(position.X, position.Y + 25),
                FillColor = new Color(50, 50, 50)
            };

            thumb = new RectangleShape(new Vector2f(thumbSize, thumbSize))
            {
                FillColor = Color.White,
                Origin = new Vector2f(thumbSize / 2f, thumbSize / 2f)
            };

            valueText = new Text(currentValue.ToString("F0"), font, 14) { FillColor = Color.White };

            Bounds = new FloatRect(position, new Vector2f(Math.Max(trackWidth, label.GetGlobalBounds().Width), 25 + trackHeight + 5));

            UpdateThumbPosition();
            UpdateValueText();
        }

        public void UpdateState()
        {
            currentValue = getter();
            UpdateThumbPosition();
            UpdateValueText();
        }

        private void UpdateThumbPosition()
        {
            float ratio = (currentValue - minValue) / (maxValue - minValue);
            thumb.Position = new Vector2f(track.Position.X + ratio * track.Size.X, track.Position.Y + track.Size.Y / 2);
        }

        private void UpdateValueText()
        {
            valueText.DisplayedString = currentValue.ToString("F0");
            valueText.Position = new Vector2f(track.Position.X + track.Size.X + 10, track.Position.Y );
        }


        public override void Draw(RenderWindow window)
        {
            window.Draw(label);
            window.Draw(track);
            window.Draw(thumb);
            window.Draw(valueText);
        }

        public override void HandleEvent(Event e, Vector2f mousePos)
        {
            if (e.Type == EventType.MouseButtonPressed && e.MouseButton.Button == Mouse.Button.Left)
            {
                if (track.GetGlobalBounds().Contains(mousePos.X, mousePos.Y) || thumb.GetGlobalBounds().Contains(mousePos.X, mousePos.Y))
                {
                    UpdateValueFromMouse(mousePos.X);
                }
            }
            else if (e.Type == EventType.MouseMoved)
            {
                if (Mouse.IsButtonPressed(Mouse.Button.Left) && (track.GetGlobalBounds().Contains(mousePos.X, mousePos.Y) || thumb.GetGlobalBounds().Contains(mousePos.X,mousePos.Y))) // Check if dragging
                {
                     if (IsMouseOver(mousePos) || Bounds.Contains(mousePos.X, mousePos.Y-20)) // Allow some slack for dragging thumb
                     {
                         UpdateValueFromMouse(mousePos.X);
                     }
                }
            }
        }

        private void UpdateValueFromMouse(float mouseX)
        {
            float ratio = (mouseX - track.Position.X) / track.Size.X;
            currentValue = minValue + ratio * (maxValue - minValue);
            currentValue = Math.Max(minValue, Math.Min(maxValue, currentValue));
            onValueChanged?.Invoke((int)currentValue);
            UpdateThumbPosition();
            UpdateValueText();
        }
    }

    // Placeholder for ColorPickerControl and ShapePickerControl
    // These are more complex and might require dedicated UI or external libraries
    public class PlaceholderControl : Control
    {
        private Text label;
        public PlaceholderControl(string text, Vector2f position, Font font)
        {
            label = new Text(text + " (Placeholder)", font, 16)
            {
                Position = position,
                FillColor = Color.Yellow
            };
            Bounds = new FloatRect(position, new Vector2f(label.GetGlobalBounds().Width + 10 , label.GetGlobalBounds().Height + 10));
        }
        public override void Draw(RenderWindow window) { window.Draw(label); }
        public override void HandleEvent(Event e, Vector2f mousePos) { /* Do nothing */ }
    }


    public class SettingsPanel
    {
        private Profile profile;
        private Font font;
        private Vector2f panelPosition;
        private Vector2f panelSize;
        private RectangleShape background;
        private List<Control> controls = new List<Control>();
        private bool visible = false;
        private bool isDirty = false;
        private ButtonControl saveButton;
        private ButtonControl loadButton;
        private Text dirtyIndicator;


        // These were in the original snippet, but their direct use seems replaced by specific controls
        // private Action<bool> audioToggle;
        // private Func<bool> audioGetter;

        public SettingsPanel(Font font, Vector2f windowSize, Profile profile)
        {
            this.font = font;
            this.profile = profile;

            panelSize = new Vector2f(windowSize.X * 0.8f, windowSize.Y * 0.9f);
            panelPosition = new Vector2f((windowSize.X - panelSize.X) / 2, (windowSize.Y - panelSize.Y) / 2);

            background = new RectangleShape(panelSize)
            {
                Position = panelPosition,
                FillColor = new Color(30, 30, 30, 220),
                OutlineColor = Color.White,
                OutlineThickness = 2
            };

            dirtyIndicator = new Text("*", font, 20) { FillColor = Color.Red };

            BuildControls();
            LoadProfileToUI();
            UpdateSaveLabel(); // Initial state
        }

        public bool IsVisible => visible;
        public void ToggleVisibility() => visible = !visible;


        private Vector2f GetRelativePos(float xPercent, float yPercent)
        {
            return new Vector2f(
                panelPosition.X + panelSize.X * xPercent,
                panelPosition.Y + panelSize.Y * yPercent
            );
        }

        private void MarkDirty()
        {
            isDirty = true;
            UpdateSaveLabel();
        }

        private void UpdateSaveLabel()
        {
            if (saveButton != null)
            {
                saveButton.SetText(isDirty ? "Save*" : "Save");
            }
             if (dirtyIndicator != null && saveButton != null) // Position dirty indicator next to save button
            {
                dirtyIndicator.DisplayedString = isDirty ? "*" : "";
                // This positioning might need adjustment depending on your button's implementation
                dirtyIndicator.Position = new Vector2f(
                    saveButton.Bounds.Left + saveButton.Bounds.Width -15, // Approx position
                    saveButton.Bounds.Top
                );
            }
        }


        private void BuildControls()
        {
            controls.Clear();
            float col1X = 0.05f;
            float col2X = 0.50f; // Adjusted for potentially wider sliders
            float currentY = 0.05f;
            float spacingY = 0.07f; // Increased spacing for sliders

            // Original controls
            CreateToggle("Fading", col1X, currentY, () => { profile.Fading = !profile.Fading; MarkDirty(); }, () => profile.Fading);
            CreateToggle("Counter", col2X, currentY, () => { profile.Counter = !profile.Counter; MarkDirty(); }, () => profile.Counter);
            currentY += spacingY;

            CreateSlider("Bar Speed", col1X, currentY, 100, 2000, () => profile.BarSpeed, v => { profile.BarSpeed = v; MarkDirty(); });
            CreateSlider("Key Size", col2X, currentY, 20, 200, () => profile.KeySize, v => { profile.KeySize = v; MarkDirty(); });
            currentY += spacingY;

            CreateSlider("Margin", col1X, currentY, 0, 100, () => profile.Margin, v => { profile.Margin = v; MarkDirty(); });
            CreateSlider("Outline", col2X, currentY, 0, 20, () => profile.OutlineThickness, v => { profile.OutlineThickness = v; MarkDirty(); });
            currentY += spacingY;

            CreateSlider("FPS", col1X, currentY, 15, 240, () => profile.FPS, v => { profile.FPS = v; MarkDirty(); });
            // Placeholder for ColorPicker
            // The setter lambda 'c => { ... }' takes an object 'c'. It needs to be cast if assigned to a strongly-typed property.
            CreatePlaceHolder("BG Color", col2X, currentY, () => profile.BackgroundColor, c => { if(c is SFML.Graphics.Color) profile.BackgroundColor = (SFML.Graphics.Color)c; MarkDirty(); });
            currentY += spacingY;

            // Placeholder for ShapePicker
            // The setter lambda 's => { ... }' takes an object 's'. It needs to be cast.
            CreatePlaceHolder("Tap Shape", col1X, currentY, () => profile.TapShape, s => { if(s is string) profile.TapShape = (string)s; MarkDirty(); });
            CreateToggle("Audio Reactive", col2X, currentY, () => { profile.AudioReactive = !profile.AudioReactive; MarkDirty(); }, () => profile.AudioReactive);
            currentY += spacingY;

            // New effect options
            CreateToggle("Glitch Bars", col1X, currentY, () => { profile.EnableGlitch = !profile.EnableGlitch; MarkDirty(); }, () => profile.EnableGlitch);
            CreateSlider("Glitch Freq", col2X, currentY, 1, 20, () => profile.GlitchFrequency, v => { profile.GlitchFrequency = v; MarkDirty(); });
            currentY += spacingY;

            CreateToggle("Pixelation", col1X, currentY, () => { profile.EnablePixelation = !profile.EnablePixelation; MarkDirty(); }, () => profile.EnablePixelation);
            CreateSlider("Pixel Size", col2X, currentY, 1, 32, () => profile.PixelSize, v => { profile.PixelSize = v; MarkDirty(); });
            currentY += spacingY * 1.5f; // Extra space before buttons

            // Save/Load
            saveButton = CreateButton("Save", col1X, currentY, () => { profile.Save("profile.json"); isDirty = false; UpdateSaveLabel(); });
            loadButton = CreateButton("Load", col2X, currentY, () => {
                profile = Profile.Load("profile.json");
                LoadProfileToUI();
                isDirty = false;
                UpdateSaveLabel();
            });
        }

        private void CreateToggle(string label, float xPercent, float yPercent, Action onToggle, Func<bool> getter)
        {
            controls.Add(new ToggleControl(label, GetRelativePos(xPercent, yPercent), font, onToggle, getter));
        }

        private void CreateSlider(string label, float xPercent, float yPercent, int min, int max, Func<int> getter, Action<int> setter)
        {
            controls.Add(new SliderControl(label, GetRelativePos(xPercent, yPercent), min, max, font, getter, setter));
        }

        // Placeholder for actual ColorPicker and ShapePicker creation
        private void CreatePlaceHolder(string label, float xPercent, float yPercent, Func<object> getter, Action<object> setter)
        {
            // These would need more complex UI elements. For now, a simple placeholder.
            controls.Add(new PlaceholderControl(label, GetRelativePos(xPercent, yPercent), font));
        }
        // Unused private methods, CreatePlaceHolder is used instead for these.
        // private void CreateColorPicker(string label, float xPercent, float yPercent, Func<Color> getter, Action<Color> setter)
        // {
        //     // Actual implementation would be more complex
        //     controls.Add(new PlaceholderControl($"Color: {label}", GetRelativePos(xPercent, yPercent), font));
        // }

        // private void CreateShapePicker(string label, float xPercent, float yPercent, Func<string> getter, Action<string> setter)
        // {
        //     // Actual implementation would be more complex
        //     controls.Add(new PlaceholderControl($"Shape: {label}", GetRelativePos(xPercent, yPercent), font));
        // }


        private ButtonControl CreateButton(string text, float xPercent, float yPercent, Action onClick)
        {
            var button = new ButtonControl(text, GetRelativePos(xPercent, yPercent), new Vector2f(panelSize.X * 0.4f, 30), font, onClick);
            controls.Add(button);
            return button;
        }

        public void LoadProfileToUI()
        {
            foreach (var control in controls)
            {
                if (control is ToggleControl toggle) toggle.UpdateState();
                if (control is SliderControl slider) slider.UpdateState();
                // Add more types if needed, e.g., for ColorPicker, ShapePicker
            }
            isDirty = false; // Profile just loaded, so it's not dirty
            UpdateSaveLabel();
        }

        public void HandleEvent(Event e, RenderWindow window)
        {
            if (!visible) return;

            Vector2f mousePos = window.MapPixelToCoords(Mouse.GetPosition(window), window.GetView());

            foreach (var control in controls)
            {
                control.HandleEvent(e, mousePos);
            }
        }

        public void Draw(RenderWindow window)
        {
            if (!visible) return;

            // Ensure panel is drawn with its own view if the main window has a different view
            View panelView = new View(new FloatRect(0, 0, window.Size.X, window.Size.Y));
            View originalView = window.GetView();
            window.SetView(panelView);

            window.Draw(background);
            foreach (var c in controls) c.Draw(window);

            if (isDirty && dirtyIndicator != null)
            {
                // This was for the save button text, but an indicator is also good
                // We can position it near the save button or a fixed spot
                 dirtyIndicator.Position = new Vector2f(
                    saveButton.Bounds.Left + saveButton.Bounds.Width - 25,
                    saveButton.Bounds.Top + 5
                );
                window.Draw(dirtyIndicator);
            }


            window.SetView(originalView); // Restore original view
        }
    }
}
