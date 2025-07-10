using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using KeyOverlayEnhanced;

namespace KeyOverlay
{
    public class AppWindow : IDisposable
    {
        private RenderWindow _window;
        private Profile _profile;
        private SettingsPanel _settingsPanel;
        private CustomUI _customUI;
        private AudioAnalyzer _audioAnalyzer;
        private Font _appFont;
        private object _lock = new object();

        public AppWindow()
        {
            Console.WriteLine("Starting AppWindow initialization...");
            
            // Load profile first as it dictates window settings
            _profile = Profile.Load("profile.json");
            Console.WriteLine("Profile loaded successfully");

            // Use a default font, ensure "Resources/consolab.ttf" exists or handle error
            try
            {
                _appFont = new Font(_profile.FontPath);
                Console.WriteLine("Font loaded successfully");
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
            Console.WriteLine("Window created successfully");

            _audioAnalyzer = new AudioAnalyzer(); // Initialize audio analyzer
            Console.WriteLine("AudioAnalyzer created successfully");

            // Initialize UI components AFTER _window and _appFont are ready
            _customUI = new CustomUI(_profile, _window, _audioAnalyzer, _appFont);
            Console.WriteLine("CustomUI created successfully");
            
            _settingsPanel = new SettingsPanel(_appFont, new Vector2f(_window.Size.X, _window.Size.Y), _profile);
            Console.WriteLine("SettingsPanel created successfully");

            InitializeGraphics(); // New method to setup graphics based on profile
            Console.WriteLine("Graphics initialized successfully");
        }

        public void InitializeGraphics() // Was Initialize()
        {
            lock (_lock) // Keep lock if there's any chance of concurrent access on init
            {
                // Ensure FPS is positive before casting to uint
                _window.SetFramerateLimit((uint)Math.Max(1, _profile.FPS));

                // Initial layout calculation for CustomUI if needed
                _customUI.RecalculateLayout();
            }
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
        }

        private void OnMouseButtonPressed(object sender, MouseButtonEventArgs e)
        {
            if (_settingsPanel.IsVisible)
            {
                Event eventToPass = new Event
                {
                    Type = EventType.MouseButtonPressed,
                    MouseButton = new MouseButtonEvent { Button = e.Button, X = e.X, Y = e.Y }
                };
                _settingsPanel.HandleEvent(eventToPass, _window);
            }
            // Other potential game logic for mouse press if not consumed by panel
        }

        private void OnMouseButtonReleased(object sender, MouseButtonEventArgs e)
        {
            if (_settingsPanel.IsVisible)
            {
                Event eventToPass = new Event
                {
                    Type = EventType.MouseButtonReleased,
                    MouseButton = new MouseButtonEvent { Button = e.Button, X = e.X, Y = e.Y }
                };
                _settingsPanel.HandleEvent(eventToPass, _window);
            }
            // Other potential game logic for mouse release
        }

        private void OnMouseMoved(object sender, MouseMoveEventArgs e)
        {
            if (_settingsPanel.IsVisible)
            {
                Event eventToPass = new Event
                {
                    Type = EventType.MouseMoved,
                    MouseMove = new MouseMoveEvent { X = e.X, Y = e.Y }
                };
                _settingsPanel.HandleEvent(eventToPass, _window);
            }
            // Other potential game logic for mouse move
        }

        public void Run()
        {
            _window.Closed += OnClose;
            _window.KeyPressed += OnKeyPressed;
            _window.Resized += OnResized;
            _window.MouseButtonPressed += OnMouseButtonPressed;
            _window.MouseButtonReleased += OnMouseButtonReleased;
            _window.MouseMoved += OnMouseMoved;
            // _window.MouseWheelScrolled += OnMouseWheelScrolled; // If settings panel needs it

            // Ensure FPS is positive before casting to uint
            _window.SetFramerateLimit((uint)Math.Max(1, _profile.FPS)); // Set initial framerate from profile

            while (_window.IsOpen)
            {
                _window.DispatchEvents(); // Process all subscribed events

                // Update game logic
                _customUI.Update();

                // Drawing
                _window.Clear(_profile.BackgroundColor);

                _customUI.Render(_window);

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
