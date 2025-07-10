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
        private SkinManager _skinManager;
        private object _lock = new object();

        public AppWindow()
        {
            Console.WriteLine("Starting AppWindow initialization...");
            
            _profile = Profile.Load("profile.json");
            Console.WriteLine("Profile loaded successfully");

            _skinManager = new SkinManager(_profile.CurrentSkinDirectoryName);
            Console.WriteLine($"SkinManager initialized. Current skin: {_skinManager.CurrentSkin.SkinName}");

            LoadFontFromSkin(); // Load font based on current skin

            _window = new RenderWindow(new VideoMode(800, 600), "Key Overlay Enhanced", Styles.Default);
            _window.SetFramerateLimit((uint)Math.Max(1, _profile.FPS)); // FPS from general profile
            Console.WriteLine("Window created successfully");

            _audioAnalyzer = new AudioAnalyzer();
            Console.WriteLine("AudioAnalyzer created successfully");

            // Pass SkinManager's CurrentSkin to CustomUI and SettingsPanel
            _customUI = new CustomUI(_profile, _skinManager.CurrentSkin, _window, _audioAnalyzer, _appFont);
            Console.WriteLine("CustomUI created successfully");
            
            _settingsPanel = new SettingsPanel(_appFont, new Vector2f(_window.Size.X, _window.Size.Y), _profile, _skinManager, ApplySkin);
            Console.WriteLine("SettingsPanel created successfully");

            ApplySkin(_skinManager.CurrentSkin); // Apply initial skin settings (like background color)
            Console.WriteLine("Initial skin applied to window graphics.");
        }

        private void LoadFontFromSkin()
        {
            try
            {
                // Dispose existing font if any before loading a new one
                _appFont?.Dispose();
                _appFont = new Font(_skinManager.CurrentSkin.FontFileName);
                Console.WriteLine($"Font '{_skinManager.CurrentSkin.FontFileName}' loaded successfully from skin '{_skinManager.CurrentSkin.SkinName}'.");
            }
            catch (SFML.LoadingFailedException ex)
            {
                string fallbackFontPath = new SkinProfile().FontFileName; // Get default font from a new SkinProfile instance
                Console.WriteLine($"Failed to load font from skin: '{_skinManager.CurrentSkin.FontFileName}'. Error: {ex.Message}. Trying fallback default font '{fallbackFontPath}'.");
                try
                {
                    _appFont = new Font(fallbackFontPath);
                    Console.WriteLine($"Fallback font '{fallbackFontPath}' loaded successfully.");
                }
                catch (SFML.LoadingFailedException exFallback)
                {
                    Console.WriteLine($"CRITICAL: Failed to load fallback font: '{fallbackFontPath}'. Error: {exFallback.Message}. Application might not render text correctly.");
                    // Consider throwing here or using a built-in SFML font if possible, though SFML.Net doesn't ship one.
                    // For now, we'll proceed without a font, which will likely cause issues.
                    _appFont = null; // Signifies no font available
                }
            }
        }

        public void ApplySkin(SkinProfile newSkin)
        {
            lock (_lock)
            {
                _skinManager.CurrentSkin = newSkin; // Ensure SkinManager is also updated if called externally
                _profile.CurrentSkinDirectoryName = _skinManager.CurrentSkinDirectoryName; // Persist choice

                LoadFontFromSkin(); // Reload font in case it changed

                // Update window properties
                // Background color is handled in the main loop's Clear call.

                // Update UI components
                if (_appFont != null) // Only update components if font is available
                {
                    _customUI.UpdateSkin(_skinManager.CurrentSkin, _appFont);
                    _settingsPanel.UpdateSkin(_skinManager.CurrentSkin, _appFont); // Settings panel might also want skin updates
                }
                else
                {
                    Console.WriteLine("Cannot apply skin to UI components: AppFont is null.");
                }

                // Recalculate layout for CustomUI as key sizes/margins might change with skin (though not yet implemented in SkinProfile)
                _customUI.RecalculateLayout();

                Console.WriteLine($"Applied skin: {newSkin.SkinName}");
            }
        }


        // Graphics initialization is now part of ApplySkin or specific component updates
        // public void InitializeGraphics() // Was Initialize()
        // {
        //     lock (_lock)
        //     {
        //         _window.SetFramerateLimit((uint)Math.Max(1, _profile.FPS));
        //         _customUI.RecalculateLayout();
        //     }
        // }

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
                // Use background color from the currently loaded skin
                _window.Clear(_skinManager.CurrentSkin.BackgroundColor.SfmlColor);

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
