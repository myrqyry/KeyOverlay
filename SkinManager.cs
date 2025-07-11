using System;
using System.Collections.Generic;
#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace KeyOverlayEnhanced
{
    public class SkinManager
    {
        public const string SkinsDirectory = "Skins";
        public const string SkinFileName = "skin.json";

        public List<string> AvailableSkinNames { get; private set; } = new List<string>();
        public SkinProfile CurrentSkin { get; set; } // Made setter public
        public string CurrentSkinDirectoryName { get; private set; }

        private SkinProfile _defaultSkin;

        public SkinManager(string? initialSkinDirectoryName = null)
        {
            _defaultSkin = new SkinProfile(); // A fallback default skin
            CurrentSkin = _defaultSkin; // Start with the default skin

            DiscoverAvailableSkins();

            if (!string.IsNullOrEmpty(initialSkinDirectoryName))
            {
                LoadSkin(initialSkinDirectoryName);
            }
            else if (AvailableSkinNames.Any())
            {
                // If no specific skin is requested, try loading the first one found or a "Default" named one
                var preferredInitialSkin = AvailableSkinNames.FirstOrDefault(s => s.Equals("Default", StringComparison.OrdinalIgnoreCase))
                                           ?? AvailableSkinNames.First();
                LoadSkin(preferredInitialSkin);
            }
        }

        public void DiscoverAvailableSkins()
        {
            AvailableSkinNames.Clear();
            if (!Directory.Exists(SkinsDirectory))
            {
                try
                {
                    Directory.CreateDirectory(SkinsDirectory);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating Skins directory: {ex.Message}");
                    // Still add a "Default" option that uses the hardcoded _defaultSkin
                    AvailableSkinNames.Add("Default (Built-in)");
                    CurrentSkinDirectoryName = "Default (Built-in)";
                    CurrentSkin = _defaultSkin;
                    return;
                }
            }

            var skinDirectories = Directory.GetDirectories(SkinsDirectory);
            foreach (var dir in skinDirectories)
            {
                if (File.Exists(Path.Combine(dir, SkinFileName)))
                {
                    AvailableSkinNames.Add(Path.GetFileName(dir));
                }
            }

            if (!AvailableSkinNames.Any())
            {
                // If no skins are found in subdirectories, ensure the "Default (Built-in)" is an option
                AvailableSkinNames.Add("Default (Built-in)");
                 if (string.IsNullOrEmpty(CurrentSkinDirectoryName) || CurrentSkin == null)
                 {
                    CurrentSkinDirectoryName = "Default (Built-in)";
                    CurrentSkin = _defaultSkin;
                 }
            }
        }

        public bool LoadSkin(string skinDirectoryName)
        {
            if (skinDirectoryName == "Default (Built-in)")
            {
                CurrentSkin = _defaultSkin;
                CurrentSkinDirectoryName = skinDirectoryName;
                Console.WriteLine("Loaded built-in default skin.");
                return true;
            }

            string skinFilePath = Path.Combine(SkinsDirectory, skinDirectoryName, SkinFileName);
            if (!File.Exists(skinFilePath))
            {
                Console.WriteLine($"Skin file not found: {skinFilePath}. Falling back to current or default skin.");
                // Optionally, fall back to the _defaultSkin if CurrentSkin is not satisfactory
                if (CurrentSkin == null || CurrentSkin == _defaultSkin && skinDirectoryName != _defaultSkin.SkinName)
                {
                     CurrentSkin = _defaultSkin;
                     CurrentSkinDirectoryName = "Default (Built-in)";
                }
                return false;
            }

            try
            {
                var json = File.ReadAllText(skinFilePath);
                var loadedSkin = JsonSerializer.Deserialize<SkinProfile>(json);
                if (loadedSkin != null)
                {
                    CurrentSkin = loadedSkin;
                    CurrentSkinDirectoryName = skinDirectoryName;
                    Console.WriteLine($"Successfully loaded skin: {CurrentSkin.SkinName} from {skinDirectoryName}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Failed to deserialize skin: {skinFilePath}. Using current or default skin.");
                    if (CurrentSkin == null) { CurrentSkin = _defaultSkin; CurrentSkinDirectoryName = "Default (Built-in)";}
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading skin '{skinDirectoryName}': {ex.Message}. Using current or default skin.");
                if (CurrentSkin == null) { CurrentSkin = _defaultSkin; CurrentSkinDirectoryName = "Default (Built-in)";}
                return false;
            }
        }

        // Call this if skins are added/removed at runtime, or to refresh the list
        public void RefreshSkins()
        {
            DiscoverAvailableSkins();
            // Try to maintain current skin if still available, otherwise load default
            if (!string.IsNullOrEmpty(CurrentSkinDirectoryName) && AvailableSkinNames.Contains(CurrentSkinDirectoryName))
            {
                LoadSkin(CurrentSkinDirectoryName);
            }
            else if (AvailableSkinNames.Any())
            {
                LoadSkin(AvailableSkinNames.First());
            }
            else // Should fall back to built-in default if this happens
            {
                LoadSkin("Default (Built-in)");
            }
        }
    }
}
