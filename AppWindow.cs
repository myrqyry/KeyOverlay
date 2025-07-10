using System;
using System.Collections.Generic;
// using System.Globalization; // No longer directly needed here for parsing
// using System.IO; // No longer directly needed here for config
// using System.Linq; // May not be needed after refactor
// using System.Threading; // No longer directly needed for config watcher
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using KeyOverlayEnhanced; // Namespace for new classes

namespace KeyOverlay // Original namespace
{
    public class AppWindow : IDisposable
    {
        private RenderWindow _window;
        // private Vector2u _size; // Window size will be managed by SFML, UI adapts
        // Old fields to be replaced or managed by Profile/CustomUI
        // private List<Key> _keyList;
        // private List<RectangleShape> _squareList;
        // private float _barSpeed;
        // private float _ratioX;
        // private float _ratioY;
        // private int _outlineThickness;
        // private Color _backgroundColor;
        // private Color _fontColor;
        // private bool _fading;
        // private bool _counter;
        // private List<Drawable> _staticDrawables;
        // private uint _maxFPS;

        private Clock _gameClock = new Clock(); // Renamed from _clock for clarity
        // private Config _config; // Replaced by Profile
        private object _lock = new object(); // May still be useful if any async operations remain

        private Profile _profile;
        private SettingsPanel _settingsPanel;
        private CustomUI _customUI;
        private AudioAnalyzer _audioAnalyzer;
        private Font _appFont; // General purpose font

        // For fading effect, might need to be adapted or moved to CustomUI
        private RenderTexture _fadingTexture;
        private Sprite _fadingSprite;


        public AppWindow()
        {
            // Load profile first as it dictates window settings
            _profile = Profile.Load("profile.json");

            // Use a default font, ensure "Resources/consolab.ttf" exists or handle error
            try
            {
                _appFont = new Font(_profile.FontPath);
            }
            catch (SFML.LoadingFailedException ex)
            {
                Console.WriteLine($"Failed to load font: {ex.Message}. Using fallback or exiting.");
                // Handle fallback: try a system font or exit. For now, let it throw if critical.
                // For simplicity, we'll assume font exists. A real app needs robust error handling.
                throw;
            }

            // Initial window size can be from profile or a default
            _window = new RenderWindow(new VideoMode(800, 600), "Key Overlay Enhanced", Styles.Default);
            // _size = _window.Size; // Store initial size, though it can change

            _audioAnalyzer = new AudioAnalyzer(); // Initialize audio analyzer

            // Initialize UI components AFTER _window and _appFont are ready
            _customUI = new CustomUI(_profile, _window, _audioAnalyzer, _appFont);
            _settingsPanel = new SettingsPanel(_appFont, new Vector2f(_window.Size.X, _window.Size.Y), _profile);

            InitializeGraphics(); // New method to setup graphics based on profile
        }

        public void InitializeGraphics() // Was Initialize()
        {
            lock (_lock) // Keep lock if there's any chance of concurrent access on init
            {
                _window.SetFramerateLimit(_profile.FPS);

                // Fading effect setup (simplified, may need rework with CustomUI)
                if (_profile.Fading)
                {
                    SetupFadingEffect();
                }

                // Initial layout calculation for CustomUI if needed
                 _customUI.RecalculateLayout();
            }
        }

        private void SetupFadingEffect()
        {
            // This is a simplified version of the original fading logic.
            // It assumes Fading.GetBackgroundColorFadingTexture is available or reimplemented.
            // For now, let's create a dummy fading sprite.
            // The original `Fading` class and `CreateItems` are not in the provided snippets for KeyOverlayEnhanced.
            // This part will need the definitions of those helper classes or be integrated into CustomUI.

            // Placeholder:
            if (_fadingTexture != null) _fadingTexture.Dispose();
            if (_fadingSprite != null) _fadingSprite.Dispose();

            // We need a way to get the fading texture. Assuming a helper or adapting original.
            // For now, let's make a simple gradient if Fading class is not available.
            _fadingTexture = new RenderTexture(_window.Size.X, (uint)(255 * 2)); // Approximation
            _fadingTexture.Clear(Color.Transparent);

            if (_profile.Fading) // Check profile setting
            {
                 // Simple vertical gradient from background color to transparent (simulating old effect)
                Vertex[] gradient = new Vertex[4 * 255];
                for (uint i = 0; i < 255; i++)
                {
                    Color color = new Color(_profile.BackgroundColor.R, _profile.BackgroundColor.G, _profile.BackgroundColor.B, (byte)(255-i));
                    gradient[i*4 + 0] = new Vertex(new Vector2f(0, i * 2), color);
                    gradient[i*4 + 1] = new Vertex(new Vector2f(_window.Size.X, i * 2), color);
                    gradient[i*4 + 2] = new Vertex(new Vector2f(_window.Size.X, (i+1) * 2), color);
                    gradient[i*4 + 3] = new Vertex(new Vector2f(0, (i+1) * 2), color);

                }
                 _fadingTexture.Draw(gradient, PrimitiveType.Quads);
            }
            _fadingTexture.Display();
            _fadingSprite = new Sprite(_fadingTexture.Texture);
            _fadingSprite.Position = new Vector2f(0, _window.Size.Y - _fadingTexture.Size.Y); // Position at bottom
        }


        private void OnClose(object sender, EventArgs e)
        {
            _window.Close();
        }

        private void OnKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Code == Keyboard.Key.F1) // Toggle settings panel
            {
                _settingsPanel.ToggleVisibility();
            }
            // Other global key presses can be handled here if not consumed by UI
        }

        private void OnResized(object sender, SizeEventArgs e)
        {
            // Update view to match new window size
            _window.SetView(new View(new FloatRect(0, 0, e.Width, e.Height)));
            // Notify CustomUI and SettingsPanel if they need to adjust layout
            // _settingsPanel.UpdateWindowSize(new Vector2f(e.Width, e.Height)); // Requires method in SettingsPanel
            _customUI.RecalculateLayout(); // If CustomUI layout depends on window size
            if(_profile.Fading) SetupFadingEffect(); // Recreate fading effect for new size
        }


        public void Run()
        {
            _window.Closed += OnClose;
            _window.KeyPressed += OnKeyPressed;
            _window.Resized += OnResized; // Handle window resizing

            _window.SetFramerateLimit(_profile.FPS); // Set initial framerate from profile

            while (_window.IsOpen)
            {
                // Event processing loop
                Event e;
                while (_window.PollEvent(out e)) // Changed from DispatchEvents to PollEvent for explicit handling
                {
                    // Pass event to settings panel first if it's visible
                    if (_settingsPanel.IsVisible)
                    {
                        _settingsPanel.HandleEvent(e, _window);
                        // If settings panel consumes the event (e.g., a click inside it),
                        // we might not want CustomUI or AppWindow to process it further.
                        // For now, we'll let both process, but this could be refined.
                    }

                    // Global event handling
                    switch (e.Type)
                    {
                        case EventType.Closed:
                            OnClose(this, EventArgs.Empty);
                            break;
                        case EventType.KeyPressed:
                            OnKeyPressed(this, e.Key);
                            // If settings panel is not visible or didn't handle F1, AppWindow can.
                            // Note: OnKeyPressed in AppWindow already handles F1 for panel toggle.
                            break;
                        case EventType.Resized:
                            OnResized(this, e.Size);
                            break;
                        // Add other event types as needed (MouseButtonPressed, MouseMoved for CustomUI if not handled internally)
                    }
                }

                // Update game logic
                _customUI.Update();

                // Drawing
                _window.Clear(_profile.BackgroundColor);

                _customUI.Render(_window);

                if (_profile.Fading && _fadingSprite != null)
                {
                    _fadingSprite.Position = new Vector2f(0, _window.Size.Y - _fadingTexture.Size.Y - _profile.KeySize - _profile.Margin*2);
                    _window.Draw(_fadingSprite);
                }

                // SettingsPanel drawing is handled AFTER all game elements
                // Its HandleEvent is now part of the event loop above.
                _settingsPanel.Draw(_window);

                _window.Display();
            }
        }

        // MoveBars is now effectively part of CustomUI.Update() and CustomUI.Render()
        // private void MoveBars(List<Key> keyList, List<RectangleShape> squareList) { ... }

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    _appFont?.Dispose();
                    _fadingTexture?.Dispose();
                    _fadingSprite?.Dispose(); // Sprite itself is IDisposable
                    _settingsPanel = null; // Allow GC if panel holds resources indirectly
                    _customUI = null;      // Allow GC
                    _audioAnalyzer = null; // Allow GC
                }

                // Free unmanaged resources (unmanaged objects) and override a finalizer below.
                // Set large fields to null.
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
