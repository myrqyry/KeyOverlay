using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;
using SFML.Window; // Required for Event
using System.Linq; // Added for LINQ extension methods

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
        private SkinProfile currentSkin; // Store to access colors
        private Font currentFont;
        private bool isHovered = false;

        public ButtonControl(string text, Vector2f position, Vector2f size, Font font, SkinProfile skin, Action onClick)
        {
            this.onClick = onClick;
            this.currentSkin = skin;
            this.currentFont = font;
            Bounds = new FloatRect(position, size);

            shape = new RectangleShape(size)
            {
                Position = position,
                FillColor = skin.ButtonBackgroundColor.SfmlColor,
                OutlineColor = skin.ControlOutlineColor.SfmlColor, // Use control outline
                OutlineThickness = 1 // Consistent outline thickness
            };

            label = new Text(text, font, 16) // Font size could be skinnable
            {
                FillColor = skin.ButtonTextColor.SfmlColor
            };
            // Center label on button
            label.Position = new Vector2f(
                position.X + (size.X - label.GetGlobalBounds().Width) / 2f,
                position.Y + (size.Y - label.GetGlobalBounds().Height) / 2f - label.GetLocalBounds().Top // Adjust for local bounds top
            );
        }

        public void UpdateSkin(SkinProfile newSkin, Font newFont)
        {
            this.currentSkin = newSkin;
            this.currentFont = newFont; // Update font if text elements need it
            shape.FillColor = isHovered ? newSkin.ButtonHoverBackgroundColor.SfmlColor : newSkin.ButtonBackgroundColor.SfmlColor;
            shape.OutlineColor = newSkin.ControlOutlineColor.SfmlColor;
            label.Font = newFont;
            label.FillColor = newSkin.ButtonTextColor.SfmlColor;
            // Recenter label
            label.Position = new Vector2f(
                shape.Position.X + (shape.Size.X - label.GetGlobalBounds().Width) / 2f,
                shape.Position.Y + (shape.Size.Y - label.GetGlobalBounds().Height) / 2f - label.GetLocalBounds().Top
            );
        }


        public override void Draw(RenderWindow window)
        {
            // Update fill color based on hover state (alternative to doing it in HandleEvent)
            // This requires mousePos to be passed to Draw or checked here.
            // For simplicity, HandleEvent will manage isHovered, and Draw uses it.
            shape.FillColor = isHovered ? currentSkin.ButtonHoverBackgroundColor.SfmlColor : currentSkin.ButtonBackgroundColor.SfmlColor;
            window.Draw(shape);
            window.Draw(label);
        }

        public override void HandleEvent(Event e, Vector2f mousePos)
        {
            bool previousHoverState = isHovered;
            isHovered = IsMouseOver(mousePos);

            if (isHovered && e.Type == EventType.MouseButtonPressed && e.MouseButton.Button == Mouse.Button.Left)
            {
                onClick?.Invoke();
            }
            // No need to directly change color here if Draw handles it based on isHovered
            // else if (previousHoverState != isHovered) {
            //    shape.FillColor = isHovered ? currentSkin.ButtonHoverBackgroundColor.SfmlColor : currentSkin.ButtonBackgroundColor.SfmlColor;
            // }
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
        private SkinProfile currentSkin;
        private Font currentFont;

        public ToggleControl(string text, Vector2f position, Font font, SkinProfile skin, Action onToggle, Func<bool> getter)
        {
            this.onToggle = onToggle;
            this.getter = getter;
            this.isChecked = getter();
            this.currentSkin = skin;
            this.currentFont = font;

            label = new Text(text, font, 16) // Font size skinnable?
            {
                Position = new Vector2f(position.X + 25, position.Y), // Label to the right of the box
                FillColor = skin.PanelTextColor.SfmlColor // Use panel text color for label
            };

            // Bounds need to accommodate the box and the label
            float boxSize = 20f;
            Bounds = new FloatRect(position, new Vector2f(boxSize + 5 + label.GetGlobalBounds().Width, boxSize));

            box = new RectangleShape(new Vector2f(boxSize, boxSize))
            {
                Position = position,
                FillColor = isChecked ? skin.ControlAccentColor.SfmlColor : skin.ControlBackgroundColor.SfmlColor,
                OutlineColor = skin.ControlOutlineColor.SfmlColor,
                OutlineThickness = 1
            };
        }

        public void UpdateSkin(SkinProfile newSkin, Font newFont)
        {
            this.currentSkin = newSkin;
            this.currentFont = newFont;

            label.Font = newFont;
            label.FillColor = newSkin.PanelTextColor.SfmlColor;
            // Reposition label if font size changed significantly (not handled here, assumes fixed font size for now)

            box.FillColor = isChecked ? newSkin.ControlAccentColor.SfmlColor : newSkin.ControlBackgroundColor.SfmlColor;
            box.OutlineColor = newSkin.ControlOutlineColor.SfmlColor;
        }

        public void UpdateState() // Call this if the underlying bool state changes externally
        {
            isChecked = getter();
            box.FillColor = isChecked ? currentSkin.ControlAccentColor.SfmlColor : currentSkin.ControlBackgroundColor.SfmlColor;
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
        private Font currentFont; // Changed from 'font'
        private SkinProfile currentSkin;

        public SliderControl(string text, Vector2f position, float min, float max, Font font, SkinProfile skin, Func<int> getter, Action<int> onValueChanged)
        {
            this.currentSkin = skin;
            this.currentFont = font;
            this.label = new Text(text, font, 16) { Position = position, FillColor = skin.PanelTextColor.SfmlColor };
            this.minValue = min;
            this.maxValue = max;
            this.getter = getter;
            this.currentValue = getter();
            this.onValueChanged = onValueChanged;

            float trackWidth = 150; // Could be skinnable
            float trackHeight = 8;  // Skinnable
            float thumbSize = 18;   // Skinnable

            track = new RectangleShape(new Vector2f(trackWidth, trackHeight))
            {
                Position = new Vector2f(position.X, position.Y + 25 + (thumbSize - trackHeight)/2), // Center track with thumb vertically
                FillColor = skin.ControlBackgroundColor.SfmlColor,
                OutlineColor = skin.ControlOutlineColor.SfmlColor,
                OutlineThickness = 1
            };

            thumb = new RectangleShape(new Vector2f(thumbSize, thumbSize))
            {
                FillColor = skin.ControlAccentColor.SfmlColor, // Use accent color for thumb
                OutlineColor = skin.ControlOutlineColor.SfmlColor,
                OutlineThickness = 1,
                Origin = new Vector2f(thumbSize / 2f, thumbSize / 2f)
            };

            valueText = new Text(currentValue.ToString("F0"), font, 14) { FillColor = skin.PanelTextColor.SfmlColor };

            Bounds = new FloatRect(position, new Vector2f(Math.Max(trackWidth, label.GetGlobalBounds().Width) + 40, 25 + thumbSize + 5)); // +40 for valueText

            UpdateThumbPosition();
            UpdateValueText();
        }

        public void UpdateSkin(SkinProfile newSkin, Font newFont)
        {
            this.currentSkin = newSkin;
            this.currentFont = newFont;

            label.Font = newFont;
            label.FillColor = newSkin.PanelTextColor.SfmlColor;

            track.FillColor = newSkin.ControlBackgroundColor.SfmlColor;
            track.OutlineColor = newSkin.ControlOutlineColor.SfmlColor;

            thumb.FillColor = newSkin.ControlAccentColor.SfmlColor;
            thumb.OutlineColor = newSkin.ControlOutlineColor.SfmlColor;

            valueText.Font = newFont;
            valueText.FillColor = newSkin.PanelTextColor.SfmlColor;

            // Recalculate positions/bounds if sizes changed due to skin (not currently the case)
            UpdateThumbPosition(); // Ensure thumb is correctly placed with new potential sizes
            UpdateValueText();
        }

        public void UpdateState() // Call if underlying value changes externally
        {
            currentValue = getter();
            UpdateThumbPosition();
            UpdateValueText();
        }

        private void UpdateThumbPosition()
        {
            float ratio = (maxValue - minValue == 0) ? 0 : (currentValue - minValue) / (maxValue - minValue); // Avoid div by zero
            thumb.Position = new Vector2f(track.Position.X + ratio * track.Size.X, track.Position.Y + track.Size.Y / 2);
        }

        private void UpdateValueText()
        {
            valueText.DisplayedString = currentValue.ToString("F0");
            // Position value text to the right of the track
            valueText.Position = new Vector2f(track.Position.X + track.Size.X + 10, track.Position.Y + (track.Size.Y - valueText.GetGlobalBounds().Height) / 2f - valueText.GetLocalBounds().Top);
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
        private Font currentFont; // Renamed from font
        private Action<string> onSkinSelected;
        private SkinManager skinManager;
        private Vector2f controlBasePosition; // To distinguish from shape's position field
        private Vector2f controlSize; // To distinguish from shape's size field
        private SkinProfile currentSkin;

        public SkinSelectorControl(string text, Vector2f pos, Vector2f sz, Font fnt, SkinProfile skin, SkinManager sm, Action<string> onSelected)
        {
            this.currentSkin = skin;
            this.currentFont = fnt;
            label = new Text(text, fnt, 16) { Position = pos, FillColor = skin.PanelTextColor.SfmlColor };
            this.controlBasePosition = new Vector2f(pos.X, pos.Y + 20);
            this.controlSize = sz;
            this.skinManager = sm;
            this.onSkinSelected = onSelected;

            mainBox = new RectangleShape(controlSize)
            {
                Position = controlBasePosition,
                FillColor = skin.ControlBackgroundColor.SfmlColor,
                OutlineColor = skin.ControlOutlineColor.SfmlColor,
                OutlineThickness = 1
            };

            selectedSkinText = new Text("", currentFont, 16) { FillColor = skin.ControlTextColor.SfmlColor };

            Bounds = new FloatRect(controlBasePosition, controlSize);

            RefreshSkinList(); // This calls UpdateItemButtons and UpdateSelectedText internally after skinNames is populated
        }

        public void UpdateSkin(SkinProfile newSkin, Font newFont)
        {
            this.currentSkin = newSkin;
            this.currentFont = newFont;

            label.Font = newFont;
            label.FillColor = newSkin.PanelTextColor.SfmlColor;

            mainBox.FillColor = newSkin.ControlBackgroundColor.SfmlColor;
            mainBox.OutlineColor = newSkin.ControlOutlineColor.SfmlColor;

            selectedSkinText.Font = newFont;
            selectedSkinText.FillColor = newSkin.ControlTextColor.SfmlColor;

            // Re-create item buttons as their skin/font also needs updating
            // This implicitly happens if RefreshSkinList -> UpdateItemButtons is called
            // Or UpdateItemButtons itself could pass the new skin/font to ButtonControl constructor
            RefreshSkinList(); // This will rebuild buttons with new skin context if needed
            UpdateSelectedText(); // Ensure text position is correct with new font
        }

        public void RefreshSkinList()
        {
            if (skinManager.AvailableSkinNames != null)
            {
                skinNames = skinManager.AvailableSkinNames.ToList(); // Get a copy
            }
            else
            {
                skinNames = new List<string>(); // Fallback to an empty list if AvailableSkinNames is somehow null
                Console.WriteLine("Warning: SkinManager.AvailableSkinNames was null. Initializing to empty list.");
            }

            if (!skinNames.Any()) skinNames.Add("Default (Built-in)");

            selectedIndex = skinNames.IndexOf(skinManager.CurrentSkinDirectoryName ?? string.Empty); // Add null check for CurrentSkinDirectoryName too
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
                    var buttonPos = new Vector2f(controlBasePosition.X, controlBasePosition.Y + controlSize.Y + i * (controlSize.Y + 2));
                    // Pass currentSkin and currentFont to the ButtonControl for dropdown items
                    var button = new ButtonControl(skinName, buttonPos, controlSize, currentFont, currentSkin, () => SelectSkin(skinName));
                    itemButtons.Add(button);
                }
                 // Update overall bounds if dropdown is open
                if (itemButtons.Any())
                {
                    var lastButton = itemButtons.Last();
                    Bounds = new FloatRect(controlBasePosition.X, controlBasePosition.Y, controlSize.X, lastButton.Bounds.Top + lastButton.Bounds.Height - controlBasePosition.Y);
                }
                else
                {
                     Bounds = new FloatRect(controlBasePosition, controlSize);
                }
            }
            else
            {
                 Bounds = new FloatRect(controlBasePosition, controlSize); // Reset to main box bounds
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
            // Center the text within the mainBox
            selectedSkinText.Position = new Vector2f(
                controlBasePosition.X + (controlSize.X - selectedSkinText.GetGlobalBounds().Width) / 2f,
                controlBasePosition.Y + (controlSize.Y - selectedSkinText.GetGlobalBounds().Height) / 2f - selectedSkinText.GetLocalBounds().Top
            );
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
                FillColor = skinManager.CurrentSkin.PanelBackgroundColor.SfmlColor,
                OutlineColor = skinManager.CurrentSkin.ControlOutlineColor.SfmlColor, // Use control outline for panel border
                OutlineThickness = 1 // Thinner outline for panel
            };

            dirtyIndicator = new Text("*", font, 20) { FillColor = skinManager.CurrentSkin.DirtyIndicatorColor.SfmlColor };

            BuildControls();
            LoadProfileToUI();
            UpdateSaveLabel();
        }

        public void UpdateSkin(SkinProfile newSkin, Font newFont)
        {
            this.font = newFont; // Update font for new controls

            // Update panel's own appearance
            background.FillColor = newSkin.PanelBackgroundColor.SfmlColor;
            background.OutlineColor = newSkin.ControlOutlineColor.SfmlColor;
            dirtyIndicator.Font = newFont; // Already done, but good to keep
            dirtyIndicator.FillColor = newSkin.DirtyIndicatorColor.SfmlColor;

            // Rebuild controls to apply new font and new skin colors to them
            // Individual controls will pick up their specific colors during their construction/UpdateSkin call
            BuildControls();
            LoadProfileToUI(); // Reload profile values into the newly styled controls

            // Update the skin selector specifically as it's a complex control
            skinSelector?.RefreshSkinList(); // Ensure it has the latest list of skins
            skinSelector?.SetSelectedSkin(skinManager.CurrentSkinDirectoryName); // Ensure selection is current
            // skinSelector's own UpdateSkin will be called during BuildControls if it's recreated,
            // or we could call it explicitly if BuildControls doesn't always recreate all types.
            // For now, assuming BuildControls handles it.

            Console.WriteLine($"SettingsPanel skin updated to: {newSkin.SkinName}. Font: {newFont.ToString()}"); // Check font object
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
            float outerMargin = 0.04f;
            float col1X = outerMargin;
            float columnGap = 0.04f;
            float colWidth = (1.0f - (2 * outerMargin) - columnGap) / 2f;
            float col2X = col1X + colWidth + columnGap;

            float currentY = outerMargin; // Start Y position
            float verticalRowSpacing = 0.075f; // Increased vertical spacing between rows of controls
            float controlItemHeight = 30f; // Standard height for most controls (buttons, dropdown box part)
            float sectionBottomMargin = verticalRowSpacing * 0.5f; // Extra space after a group of settings

            // Skin Selector - Spans both columns
            var skinSelectorLabelActualPos = GetRelativePos(col1X, currentY);
            // The SkinSelectorControl itself handles the label and the box below it.
            // We give it the top-left for its label, and it calculates its box position.
            // The size is for the main box of the selector.
            var skinSelectorBoxSize = new Vector2f(panelSize.X * (colWidth * 2f + columnGap), controlItemHeight);

            skinSelector = new SkinSelectorControl(
                "Current Skin:", // This is the label text for the SkinSelector
                skinSelectorLabelActualPos,
                skinSelectorBoxSize,
                font,
                skinManager.CurrentSkin, // Pass current skin for initial styling of the selector itself
                skinManager, // Pass the manager for listing skins
                (selectedSkinDirName) => {
                    if (skinManager.LoadSkin(selectedSkinDirName))
                    {
                        profile.CurrentSkinDirectoryName = selectedSkinDirName;
                        applySkinCallback?.Invoke(skinManager.CurrentSkin);
                        Console.WriteLine($"SettingsPanel: Skin selected '{selectedSkinDirName}'");
                    }
                }
            );
            controls.Add(skinSelector);
            // SkinSelector's label is at currentY, its box is below. So advance currentY past the whole control.
            // Approximate height of SkinSelector: Label_Height + Box_Height + small_gap.
            // Assuming label height is approx controlItemHeight/2 or similar.
            currentY += (controlItemHeight / panelSize.Y) * 2.0f + sectionBottomMargin;


            // Basic Profile Settings
            CreateToggle("Fading (Profile)", col1X, currentY, () => { profile.Fading = !profile.Fading; MarkDirty(); }, () => profile.Fading);
            CreateToggle("Counter", col2X, currentY, () => { profile.Counter = !profile.Counter; MarkDirty(); }, () => profile.Counter);
            currentY += verticalRowSpacing;

            CreateSlider("Bar Speed", col1X, currentY, 100, 2000, () => profile.BarSpeed, v => { profile.BarSpeed = v; MarkDirty(); });
            CreateSlider("Key Size", col2X, currentY, 20, 200, () => profile.KeySize, v => { profile.KeySize = v; MarkDirty(); });
            currentY += verticalRowSpacing;

            CreateSlider("Margin", col1X, currentY, 0, 100, () => profile.Margin, v => { profile.Margin = v; MarkDirty(); });
            CreateSlider("Outline", col2X, currentY, 0, 20, () => profile.OutlineThickness, v => { profile.OutlineThickness = v; MarkDirty(); });
            currentY += verticalRowSpacing;

            CreateSlider("FPS", col1X, currentY, 15, 240, () => profile.FPS, v => { profile.FPS = v; MarkDirty(); });
            CreateSlider("Bar Height", col2X, currentY, 100, 1000, () => profile.BarHeight, v => { profile.BarHeight = v; MarkDirty(); });
            currentY += sectionBottomMargin;

            // Effect Settings
            CreateToggle("Glitch Bars", col1X, currentY, () => { profile.EnableGlitch = !profile.EnableGlitch; MarkDirty(); }, () => profile.EnableGlitch);
            CreateSlider("Glitch Freq", col2X, currentY, 1, 20, () => profile.GlitchFrequency, v => { profile.GlitchFrequency = v; MarkDirty(); });
            currentY += verticalRowSpacing;

            CreateToggle("Pixelation", col1X, currentY, () => { profile.EnablePixelation = !profile.EnablePixelation; MarkDirty(); }, () => profile.EnablePixelation);
            CreateSlider("Pixel Size", col2X, currentY, 1, 32, () => profile.PixelSize, v => { profile.PixelSize = v; MarkDirty(); });
            currentY += verticalRowSpacing;

            CreateToggle("Tap Effects", col1X, currentY, () => { profile.EnableTapEffects = !profile.EnableTapEffects; MarkDirty(); }, () => profile.EnableTapEffects);
            CreateToggle("Bar Effects", col2X, currentY, () => { profile.EnableBarEffects = !profile.EnableBarEffects; MarkDirty(); }, () => profile.EnableBarEffects);
            currentY += verticalRowSpacing;

            CreateToggle("Key Glow", col1X, currentY, () => { profile.EnableKeyGlow = !profile.EnableKeyGlow; MarkDirty(); }, () => profile.EnableKeyGlow);
            CreateSlider("Glow Intensity", col2X, currentY, 0, 100, () => profile.GlowIntensity, v => { profile.GlowIntensity = v; MarkDirty(); });
            currentY += verticalRowSpacing;

            CreateToggle("Audio Reactive", col1X, currentY, () => { profile.AudioReactive = !profile.AudioReactive; MarkDirty(); }, () => profile.AudioReactive);
            CreateSlider("Bar Speed Mult", col2X, currentY, 0, 300, () => (int)(profile.BarSpeedMultiplier * 100), v => { profile.BarSpeedMultiplier = v / 100f; MarkDirty(); });
            currentY += sectionBottomMargin;

            // Tap Effect Settings
            CreateSlider("Tap Duration", col1X, currentY, 1, 20, () => (int)(profile.TapEffectDuration * 10), v => { profile.TapEffectDuration = v / 10f; MarkDirty(); });
            CreateSlider("Tap Scale", col2X, currentY, 5, 30, () => (int)(profile.TapEffectScale * 10), v => { profile.TapEffectScale = v / 10f; MarkDirty(); });
            currentY += sectionBottomMargin;

            // Placeholder Color Settings are removed as they are not functional and clutter the UI
            // If actual color pickers were implemented, they would go here.

            // Save Button for general profile settings
            // Position it explicitly at the bottom, spanning the width
            float saveButtonHeight = 35f;
            float saveButtonY = 1.0f - outerMargin - (saveButtonHeight / panelSize.Y);
            saveButton = CreateButton("Save Profile Settings", col1X, saveButtonY, new Vector2f(panelSize.X * (1.0f - 2 * outerMargin), saveButtonHeight), () => {
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
            controls.Add(new ToggleControl(label, GetRelativePos(xPercent, yPercent), font, skinManager.CurrentSkin, onToggle, getter));
        }

        private void CreateSlider(string label, float xPercent, float yPercent, int min, int max, Func<int> getter, Action<int> setter)
        {
            controls.Add(new SliderControl(label, GetRelativePos(xPercent, yPercent), min, max, font, skinManager.CurrentSkin, getter, setter));
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


        // Overload for specific size
        private ButtonControl CreateButton(string text, float xPercent, float yPercent, Vector2f size, Action onClick)
        {
            var button = new ButtonControl(text, GetRelativePos(xPercent, yPercent), size, font, skinManager.CurrentSkin, onClick);
            controls.Add(button);
            return button;
        }

        // Original overload (if still needed, though currently not used by BuildControls directly for save button)
        private ButtonControl CreateButton(string text, float xPercent, float yPercent, Action onClick)
        {
            var defaultSize = new Vector2f(panelSize.X * 0.4f, 30f); // Default size
            var button = new ButtonControl(text, GetRelativePos(xPercent, yPercent), defaultSize, font, skinManager.CurrentSkin, onClick);
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
