using System;
using SFML.Graphics;
using SFML.System;

namespace KeyOverlayEnhanced
{
    public class GlitchBar
    {
        public RectangleShape Bar { get; private set; }
        private Clock lifetime;
        private float maxLifetime = 0.2f; // Short duration for a glitch
        private static Random random = new Random();

        public GlitchBar(uint windowWidth, float yPosition, Profile profile) // Added profile
        {
            lifetime = new Clock();
            float height = (float)(random.NextDouble() * 15 + 5); // Random height
            Bar = new RectangleShape(new Vector2f(windowWidth, height))
            {
                Position = new Vector2f(0, yPosition - height / 2),
                FillColor = new Color((byte)random.Next(255), (byte)random.Next(255), (byte)random.Next(255), (byte)random.Next(100, 200))
            };
        }

        public bool Update() // Returns false if dead
        {
            // Optional: could add movement or color change over time
            return lifetime.ElapsedTime.AsSeconds() < maxLifetime;
        }

        public void Draw(RenderWindow window)
        {
            window.Draw(Bar);
        }
    }

    public class PixelationEffect
    {
        private Sprite pixelSprite;
        private Texture pixelTexture; // Hold the texture for the sprite explicitly
        private Clock lifetime;
        private float maxLifetime = 0.3f; // Short duration
        private bool isAlive = true;
        // private int pixelSize; // Stored in constructor, not needed as a field if not used elsewhere
        // private RenderWindow sourceWindow; // Used in constructor, not needed as a field

        public PixelationEffect(RenderWindow window, int pSize, Profile profile) // Added profile
        {
            // this.sourceWindow = window; // Not stored if only used here
            int currentPixelSize = Math.Max(1, pSize); // Ensure pixelSize is at least 1
            lifetime = new Clock();

            Texture tempWindowTexture = null;
            RenderTexture smallRt = null;
            try
            {
                // Capture current window content
                tempWindowTexture = new Texture(window.Size.X, window.Size.Y);
                tempWindowTexture.Update(window);

                Sprite snapshotSprite = new Sprite(tempWindowTexture);

                // Let's try a basic "draw small, then draw big" approach for a pseudo-pixelation
                uint smallWidth = window.Size.X / (uint)currentPixelSize;
                uint smallHeight = window.Size.Y / (uint)currentPixelSize;

                // Ensure smallRt dimensions are at least 1x1
                if (smallWidth == 0) smallWidth = 1;
                if (smallHeight == 0) smallHeight = 1;

                smallRt = new RenderTexture(smallWidth, smallHeight);
                smallRt.Clear(Color.Transparent);
                snapshotSprite.Scale = new Vector2f(1f / currentPixelSize, 1f / currentPixelSize); // Scale down
                smallRt.Draw(snapshotSprite);
                smallRt.Display();

                // Create a persistent copy of the texture from smallRt for pixelSprite
                pixelTexture = new Texture(smallRt.Texture); // Important: This copies the texture data

                pixelSprite = new Sprite(pixelTexture);
                pixelSprite.Scale = new Vector2f(currentPixelSize, currentPixelSize); // Scale up
                pixelSprite.Texture.Smooth = false; // Crucial for blocky pixel look
            }
            finally
            {
                // Dispose of temporary SFML resources created in constructor
                tempWindowTexture?.Dispose();
                smallRt?.Dispose();
            }
        }

        public bool IsAlive()
        {
            if (lifetime.ElapsedTime.AsSeconds() >= maxLifetime)
            {
                if (isAlive) // Ensure dispose happens only once
                {
                    isAlive = false;
                    // Clean up SFML resources owned by this instance
                    pixelSprite?.Dispose(); // Sprite itself is disposable
                    pixelTexture?.Dispose(); // Dispose the texture we explicitly created
                }
            }
            return isAlive;
        }

        public void Apply(RenderWindow window)
        {
            if (isAlive && pixelSprite != null)
            {
                // The effect is drawn over the current frame.
                // A more advanced implementation might blend it or use shaders.
                window.Draw(pixelSprite);
            }
        }
    }
}
