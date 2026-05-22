using Gum.Wireframe;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Input;
using MonoGame.Extended.Screens;
using MonoGameGum;
using ToroidalWorld.GameEngine;
using ToroidalWorld.GameLogic.Scenes;

namespace ToroidalWorld
{
    public class GameMain : GameBase
    {
        public SpriteBatch SpriteBatch { get; private set; }

        private readonly ScreenManager _screenManager;

        private int _lastUiWidth;
        private int _lastUiHeight;

        private Texture2D _cursorTexture;

        private GumService GumUI => GumService.Default;

        public GameMain()
        {
            _screenManager = new ScreenManager();
            Components.Add(_screenManager);
        }

        protected override void Initialize()
        {
            GumUI.Initialize(this, "GumProject/GumProject.gumx");

            IsMouseVisible = false;

            base.Initialize();

            SyncGumToViewport();
            _screenManager.ShowScreen(new MainMenuScreen(this));
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            ResourceManager.LoadTextures(GraphicsDevice);
            ResourceManager.LoadSprites(GraphicsDevice);

            ResourceManager.LoadBoatDefinitions();
            ResourceManager.LoadEnemyDefinitions();
            ResourceManager.LoadEnemyBaseDefinitions();
            ResourceManager.LoadStageDefinitions();
            ResourceManager.LoadProjectileDefinitions();
            ResourceManager.LoadTurretDefinitions();
            ResourceManager.LoadExperienceOrbDefinitions();

            ResourceManager.LoadSoundEffects();
            ResourceManager.LoadMusic();

            AudioManager.Initialize(sfxVolume: 1f, musicVolume: 0.5f);

            _cursorTexture = ResourceManager.GetTexture("cursor");
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardExtended.Update();
            MouseExtended.Update();

            SyncGumToViewport();

            base.Update(gameTime);

            SyncGumToViewport();

            GumUI.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);

            GumUI.Draw();
            DrawCursor();
        }

        private void DrawCursor()
        {
            if (_cursorTexture == null)
                return;

            var mouse = MouseExtended.GetState();
            var pos = mouse.Position;

            SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
            SpriteBatch.Draw(
                _cursorTexture,
                position: new Vector2(pos.X, pos.Y),
                sourceRectangle: null,
                color: Color.White,
                rotation: 0f,
                origin: Vector2.Zero,
                scale: 2f,
                effects: SpriteEffects.None,
                layerDepth: 0f);
            SpriteBatch.End();
        }

        private void SyncGumToViewport()
        {
            int width = GraphicsDevice.Viewport.Width;
            int height = GraphicsDevice.Viewport.Height;

            if (width == _lastUiWidth && height == _lastUiHeight)
                return;

            _lastUiWidth = width;
            _lastUiHeight = height;

            GraphicalUiElement.CanvasWidth = width;
            GraphicalUiElement.CanvasHeight = height;
        }
    }
}