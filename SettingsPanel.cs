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


    // Simple Dropdown/Selector for Skins
    public class SkinSelectorControl : Control
    {
        private Text label;
        private RectangleShape mainBox;
        private Text selectedSkinText;
        private List<string> skinNames;
        private int selectedIndex = 0;
        private bool isOpen = false;
        private List<ButtonControl> itemButtons = new List<ButtonControl>();
        private Font font;
        private Action<string> onSkinSelected;
        private SkinManager skinManager; // To get available skins
        private Vector2f position;
        private Vector2f size;

        public SkinSelectorControl(string text, Vector2f pos, Vector2f sz, Font fnt, SkinManager sm, Action<string> onSelected)
        {
            label = new Text(text, fnt, 16) { Position = pos, FillColor = Color.White };
            position = new Vector2f(pos.X, pos.Y + 20); // Position box below label
            size = sz;
            font = fnt;
            skinManager = sm;
            this.onSkinSelected = onSelected;

            mainBox = new RectangleShape(size)
            {
                Position = position,
                FillColor = new Color(70, 70, 70),
                OutlineColor = Color.White,
                OutlineThickness = 1
            };

            selectedSkinText = new Text("", font, 16) { FillColor = Color.White };
            selectedSkinText.Position = new Vector2f(position.X + 5, position.Y + (size.Y - selectedSkinText.GetGlobalBounds().Height) / 2 - 5);

            Bounds = new FloatRect(position, size); // Initial bounds for the main box

            RefreshSkinList();
            UpdateSelectedText();
        }

        public void RefreshSkinList()
        {
            skinNames = skinManager.AvailableSkinNames.ToList(); // Get a copy
            if (!skinNames.Any()) skinNames.Add("Default (Built-in)");

            selectedIndex = skinNames.IndexOf(skinManager.CurrentSkinDirectoryName);
            if (selectedIndex < 0) selectedIndex = 0;

            UpdateItemButtons();
            UpdateSelectedText();
        }

        private void UpdateItemButtons()
        {
            itemButtons.Clear();
            if (isOpen)
            {
                for (int i = 0; i < skinNames.Count; i++)
                {
                    var skinName = skinNames[i];
                    var buttonPos = new Vector2f(position.X, position.Y + size.Y + i * (size.Y + 2));
                    var button = new ButtonControl(skinName, buttonPos, size, font, () => SelectSkin(skinName));
                    itemButtons.Add(button);
                }
                 // Update overall bounds if dropdown is open
                if (itemButtons.Any())
                {
                    var lastButton = itemButtons.Last();
                    Bounds = new FloatRect(position.X, position.Y, size.X, lastButton.Bounds.Top + lastButton.Bounds.Height - position.Y);
                }
                else
                {
                     Bounds = new FloatRect(position, size);
                }
            }
            else
            {
                 Bounds = new FloatRect(position, size); // Reset to main box bounds
            }
        }

        private void SelectSkin(string skinName)
        {
            selectedIndex = skinNames.IndexOf(skinName);
            onSkinSelected?.Invoke(skinName);
            isOpen = false; // Close dropdown after selection
            UpdateSelectedText();
            UpdateItemButtons(); // Rebuild buttons (effectively removes them)
        }

        private void UpdateSelectedText()
        {
            if (skinNames.Any() && selectedIndex >= 0 && selectedIndex < skinNames.Count)
            {
                selectedSkinText.DisplayedString = skinNames[selectedIndex];
            }
            else if (skinNames.Any())
            {
                 selectedSkinText.DisplayedString = skinNames[0]; // Fallback
            }
            else
            {
                selectedSkinText.DisplayedString = "No Skins";
            }
            selectedSkinText.Position = new Vector2f(position.X + 5, position.Y + (size.Y - selectedSkinText.GetGlobalBounds().Height) / 2 - selectedSkinText.GetLocalBounds().Top);
        }

        public void SetSelectedSkin(string skinDirectoryName)
        {
            int newIndex = skinNames.IndexOf(skinDirectoryName);
            if (newIndex != -1)
            {
                selectedIndex = newIndex;
                UpdateSelectedText();
            }
        }

        public override void Draw(RenderWindow window)
        {
            window.Draw(label);
            window.Draw(mainBox);
            window.Draw(selectedSkinText);
            if (isOpen)
            {
                foreach (var btn in itemButtons)
                {
                    btn.Draw(window);
                }
            }
        }

        public override void HandleEvent(Event e, Vector2f mousePos)
        {
            if (e.Type == EventType.MouseButtonPressed && e.MouseButton.Button == Mouse.Button.Left)
            {
                if (mainBox.GetGlobalBounds().Contains(mousePos.X, mousePos.Y))
                {
                    isOpen = !isOpen;
                    UpdateItemButtons(); // Rebuild buttons based on new state
                }
                else if (isOpen)
                {
                    bool clickedOnItem = false;
                    foreach (var btn in itemButtons)
                    {
                        if (btn.IsMouseOver(mousePos))
                        {
                            btn.HandleEvent(e, mousePos); // This will trigger SelectSkin via its action
                            clickedOnItem = true;
                            break;
                        }
                    }
                    if (!clickedOnItem) // Clicked outside the dropdown items
                    {
                        isOpen = false;
                        UpdateItemButtons();
                    }
                }
            }
        }
    }


    public class SettingsPanel
    {
        private Profile profile;
        private SkinManager skinManager;
        private Action<SkinProfile> applySkinCallback;
        private Font font;
        private Vector2f panelPosition;
        private Vector2f panelSize;
        private RectangleShape background;
        private List<Control> controls = new List<Control>();
        private bool visible = false;
        private bool isDirty = false; // For general profile settings, not skin changes
        private ButtonControl saveButton;
        // private ButtonControl loadButton; // Loading profile reloads everything, including skin
        private Text dirtyIndicator;
        private SkinSelectorControl skinSelector;


        public SettingsPanel(Font initialFont, Vector2f windowSize, Profile prof, SkinManager sm, Action<SkinProfile> onApplySkin)
        {
            this.font = initialFont; // May be updated by skin changes
            this.profile = prof;
            this.skinManager = sm;
            this.applySkinCallback = onApplySkin;

            panelSize = new Vector2f(windowSize.X * 0.8f, windowSize.Y * 0.9f);
            panelPosition = new Vector2f((windowSize.X - panelSize.X) / 2, (windowSize.Y - panelSize.Y) / 2);

            background = new RectangleShape(panelSize)
            {
                Position = panelPosition,
                // Background color could be part of the settings panel's own skin in the future
                FillColor = new Color(30, 30, 30, 220),
                OutlineColor = Color.White,
                OutlineThickness = 2
            };

            dirtyIndicator = new Text("*", font, 20) { FillColor = Color.Red };

            BuildControls(); // Builds general profile controls
            LoadProfileToUI(); // Loads general profile settings into controls
            UpdateSaveLabel();
        }

        public void UpdateSkin(SkinProfile newSkin, Font newFont)
        {
            this.font = newFont;
            // Potentially update colors/styles of the panel itself if it were skinnable
            // For now, just rebuild controls if font changed significantly, or update font for text elements
            // Re-create text elements or update their font
            dirtyIndicator.Font = newFont;
            // Controls themselves would need an UpdateFont method or be recreated
            // For simplicity, we might need to rebuild controls if font changes.
            // However, many controls get font passed at creation and might not update automatically.
            // This is a complex part of UI skinning.

            // If the skin selector is present, tell it to update its list and current selection display
            skinSelector?.RefreshSkinList();
            skinSelector?.SetSelectedSkin(skinManager.CurrentSkinDirectoryName);


            // Re-color background based on skin? For now, it's fixed.
            // background.FillColor = newSkin.SomePanelBackgroundColor;
            Console.WriteLine($"SettingsPanel skin updated. Font: {newFont}");
            BuildControls(); // Rebuild to ensure new font is used by controls.
            LoadProfileToUI(); // And reload values.
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
            float col2X = 0.50f;
            float currentY = 0.05f;
            float spacingY = 0.06f; // Slightly reduced for more controls
            float controlHeight = 30f; // Approximate height for positioning skin selector

            // Skin Selector - Placed at the top
            skinSelector = new SkinSelectorControl(
                "Current Skin:",
                GetRelativePos(col1X, currentY),
                new Vector2f(panelSize.X * 0.85f, controlHeight), // Make it wider
                font,
                skinManager,
                (selectedSkinDirName) => {
                    if (skinManager.LoadSkin(selectedSkinDirName))
                    {
                        profile.CurrentSkinDirectoryName = selectedSkinDirName; // Update profile
                        applySkinCallback?.Invoke(skinManager.CurrentSkin);
                        // No MarkDirty() here as skin changes are applied immediately and saved via CurrentSkinDirectoryName
                        // Rebuild controls might be needed if panel's own appearance depended on skin.
                        // For now, AppWindow.ApplySkin handles necessary updates.
                        Console.WriteLine($"SettingsPanel: Skin selected '{selectedSkinDirName}'");
                    }
                }
            );
            controls.Add(skinSelector);
            currentY += spacingY * 1.5f; // Extra space after skin selector

            // Basic Profile Settings (these are general, not skin-specific)
            CreateToggle("Fading (Profile)", col1X, currentY, () => { profile.Fading = !profile.Fading; MarkDirty(); }, () => profile.Fading);
            CreateToggle("Counter", col2X, currentY, () => { profile.Counter = !profile.Counter; MarkDirty(); }, () => profile.Counter);
            currentY += spacingY;

            CreateSlider("Bar Speed", col1X, currentY, 100, 2000, () => profile.BarSpeed, v => { profile.BarSpeed = v; MarkDirty(); });
            CreateSlider("Key Size", col2X, currentY, 20, 200, () => profile.KeySize, v => { profile.KeySize = v; MarkDirty(); });
            currentY += spacingY;

            CreateSlider("Margin", col1X, currentY, 0, 100, () => profile.Margin, v => { profile.Margin = v; MarkDirty(); });
            CreateSlider("Outline", col2X, currentY, 0, 20, () => profile.OutlineThickness, v => { profile.OutlineThickness = v; MarkDirty(); });
            currentY += spacingY;

            CreateSlider("FPS", col1X, currentY, 15, 240, () => profile.FPS, v => { profile.FPS = v; MarkDirty(); });
            CreateSlider("Bar Height", col2X, currentY, 100, 1000, () => profile.BarHeight, v => { profile.BarHeight = v; MarkDirty(); });
            currentY += spacingY;

            // Effect Settings
            CreateToggle("Glitch Bars", col1X, currentY, () => { profile.EnableGlitch = !profile.EnableGlitch; MarkDirty(); }, () => profile.EnableGlitch);
            CreateSlider("Glitch Freq", col2X, currentY, 1, 20, () => profile.GlitchFrequency, v => { profile.GlitchFrequency = v; MarkDirty(); });
            currentY += spacingY;

            CreateToggle("Pixelation", col1X, currentY, () => { profile.EnablePixelation = !profile.EnablePixelation; MarkDirty(); }, () => profile.EnablePixelation);
            CreateSlider("Pixel Size", col2X, currentY, 1, 32, () => profile.PixelSize, v => { profile.PixelSize = v; MarkDirty(); });
            currentY += spacingY;

            CreateToggle("Tap Effects", col1X, currentY, () => { profile.EnableTapEffects = !profile.EnableTapEffects; MarkDirty(); }, () => profile.EnableTapEffects);
            CreateToggle("Bar Effects", col2X, currentY, () => { profile.EnableBarEffects = !profile.EnableBarEffects; MarkDirty(); }, () => profile.EnableBarEffects);
            currentY += spacingY;

            CreateToggle("Key Glow", col1X, currentY, () => { profile.EnableKeyGlow = !profile.EnableKeyGlow; MarkDirty(); }, () => profile.EnableKeyGlow);
            CreateSlider("Glow Intensity", col2X, currentY, 0, 100, () => profile.GlowIntensity, v => { profile.GlowIntensity = v; MarkDirty(); });
            currentY += spacingY;

            CreateToggle("Audio Reactive", col1X, currentY, () => { profile.AudioReactive = !profile.AudioReactive; MarkDirty(); }, () => profile.AudioReactive);
            CreateSlider("Bar Speed Mult", col2X, currentY, 0, 3, () => (int)(profile.BarSpeedMultiplier * 100), v => { profile.BarSpeedMultiplier = v / 100f; MarkDirty(); });
            currentY += spacingY;

            // Tap Effect Settings
            CreateSlider("Tap Duration", col1X, currentY, 1, 20, () => (int)(profile.TapEffectDuration * 10), v => { profile.TapEffectDuration = v / 10f; MarkDirty(); });
            CreateSlider("Tap Scale", col2X, currentY, 5, 30, () => (int)(profile.TapEffectScale * 10), v => { profile.TapEffectScale = v / 10f; MarkDirty(); });
            currentY += spacingY;

            // Color Settings (simplified for now)
            CreatePlaceHolder("BG Color", col1X, currentY, () => profile.BackgroundColor, c => { if(c is SFML.Graphics.Color) profile.BackgroundColor = (SFML.Graphics.Color)c; MarkDirty(); });
            CreatePlaceHolder("Glitch Color", col2X, currentY, () => profile.GlitchColor, c => { if(c is SFML.Graphics.Color) profile.GlitchColor = (SFML.Graphics.Color)c; MarkDirty(); });
            currentY += spacingY;

            CreatePlaceHolder("Tap Effect Color", col1X, currentY, () => profile.TapEffectColor, c => { if(c is SFML.Graphics.Color) profile.TapEffectColor = (SFML.Graphics.Color)c; MarkDirty(); });
            CreatePlaceHolder("Tap Shape", col2X, currentY, () => profile.TapShape, s => { if(s is string) profile.TapShape = (string)s; MarkDirty(); });
            currentY += spacingY * 1.5f; // Extra space before buttons

            // Save Button for general profile settings
            saveButton = CreateButton("Save Profile Settings", col1X, currentY, () => {
                profile.Save("profile.json");
                isDirty = false;
                UpdateSaveLabel();
                Console.WriteLine("Profile settings saved.");
            });
            // The Load button functionality is implicitly handled:
            // 1. On app start, AppWindow loads profile.json, which includes the CurrentSkinDirectoryName.
            // 2. SkinManager initializes with this CurrentSkinDirectoryName.
            // 3. If a skin is changed via SkinSelector, that change is live and CurrentSkinDirectoryName is updated in 'profile' instance.
            // 4. Saving 'profile' then persists this chosen skin.
            // A manual "Load Profile" button here would need to coordinate with AppWindow to re-initialize many things.
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
