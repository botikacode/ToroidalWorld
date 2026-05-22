using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Input;
using MonoGame.Extended.ViewportAdapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace ToroidalWorld
{
    public abstract class GameBase : Game
    {
        public OrthographicCamera Camera { get; private set; }

        private readonly GraphicsDeviceManager _graphicsDeviceManager;

        private readonly int _initialWindowedWidth;
        private readonly int _initialWindowedHeight;

        protected GameBase(int width = 800, int height = 480)
        {
            _initialWindowedWidth = width;
            _initialWindowedHeight = height;

            _graphicsDeviceManager = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = width,
                PreferredBackBufferHeight = height
            };

            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Content.RootDirectory = "Content";

            //test V-sync
            _graphicsDeviceManager.SynchronizeWithVerticalRetrace = true;
            IsFixedTimeStep = true;

            //Unlock FPS
            //IsFixedTimeStep = false;
            //GraphicsDeviceManager.SynchronizeWithVerticalRetrace = false;   // Desactivar VSync
            _graphicsDeviceManager.ApplyChanges();
        }

        protected override void Initialize()
        {
            Camera = new OrthographicCamera(GraphicsDevice);

            base.Initialize();
        }

        public void ToggleFullscreen()
        {
            if (_graphicsDeviceManager.IsFullScreen)
            {
                _graphicsDeviceManager.IsFullScreen = false;
                _graphicsDeviceManager.PreferredBackBufferWidth = _initialWindowedWidth;
                _graphicsDeviceManager.PreferredBackBufferHeight = _initialWindowedHeight;
            }
            else
            {
                var displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;

                _graphicsDeviceManager.PreferredBackBufferWidth = displayMode.Width;
                _graphicsDeviceManager.PreferredBackBufferHeight = displayMode.Height;
                _graphicsDeviceManager.IsFullScreen = true;
            }

            _graphicsDeviceManager.ApplyChanges();
        }

        protected override void Update(GameTime gameTime)
        {
            if (KeyboardExtended.GetState().WasKeyPressed(Keys.F11))
                ToggleFullscreen();

            base.Update(gameTime);
        }
    }
}