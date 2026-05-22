using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using MonoGame.Extended.Screens;
using MonoGameGum;
using System.Diagnostics;
using ToroidalWorld.Components.Controls;
using ToroidalWorld.GameEngine;
using ToroidalWorld.GameLogic.Progress;
using ToroidalWorld.GameLogic.Ui;
using ToroidalWorld.Screens;

namespace ToroidalWorld.GameLogic.Scenes
{
    public class MainMenuScreen : GameScreen
    {
        private const string BackgroundTextureKey = "background";

        private GameMain _game => (GameMain)Game;

        private SpriteFont _font;

        private Texture2D _backgroundTexture;

        private TitleScreenView _titleScreenView;
        private OptionsScreenView _optionsScreenView;
        private LeaderboardScreenView _leaderboardScreenView;
        private SelectPlayerScreenView _selectPlayerScreenView;

        private LeaderboardScreenPresenter _leaderboardPresenter;
        private SelectPlayerScreenPresenter _selectPlayerPresenter;

        private GumRootViewSwitcher _viewSwitcher;

        public MainMenuScreen(GameMain game) : base(game)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            GumService.Default.Root.Children.Clear();

            _titleScreenView = new TitleScreenView();
            _optionsScreenView = new OptionsScreenView();
            _leaderboardScreenView = new LeaderboardScreenView();
            _selectPlayerScreenView = new SelectPlayerScreenView();

            ForceOptionsButtonsEnabled();

            _viewSwitcher = new GumRootViewSwitcher();
            _viewSwitcher.Register(ScreenViewId.MainMenuTitle, _titleScreenView);
            _viewSwitcher.Register(ScreenViewId.MainMenuOptions, _optionsScreenView);
            _viewSwitcher.Register(ScreenViewId.MainMenuLeaderboard, _leaderboardScreenView);
            _viewSwitcher.Register(ScreenViewId.MainMenuSelectPlayer, _selectPlayerScreenView);

            _leaderboardPresenter = new LeaderboardScreenPresenter(
                _leaderboardScreenView,
                exit: () => _viewSwitcher.Show(ScreenViewId.MainMenuTitle));

            _selectPlayerPresenter = new SelectPlayerScreenPresenter(
                _selectPlayerScreenView,
                selectBoat: boatName => ScreenManager.ShowScreen(new GameplayScreen(Game, boatName)));

            _optionsScreenView.ExitOptionsButton.Click += (_, _) =>
            {
                AudioManager.TryPlaySoundEffect("click");
                _viewSwitcher.Show(ScreenViewId.MainMenuTitle);
            };

            _optionsScreenView.ToggleFullscreenButton.Click += (_, _) =>
            {
                AudioManager.TryPlaySoundEffect("click");
                _game.ToggleFullscreen();
            };

            _optionsScreenView.DeleteProgressButton.Click += (_, _) =>
            {
                AudioManager.TryPlaySoundEffect("click");
                PlayerProgressStore.Default.DeleteAll();
            };

            _titleScreenView.StartButton.Click += (_, _) =>
            {
                AudioManager.TryPlaySoundEffect("click");
                _selectPlayerPresenter.Refresh();
                _viewSwitcher.Show(ScreenViewId.MainMenuSelectPlayer);
            };

            _titleScreenView.LeaderboardButton.Click += (_, _) =>
            {
                AudioManager.TryPlaySoundEffect("click");
                _leaderboardPresenter.Refresh();
                _viewSwitcher.Show(ScreenViewId.MainMenuLeaderboard);
            };

            _titleScreenView.OptionsButton.Click += (_, _) =>
            {
                AudioManager.TryPlaySoundEffect("click");
                ForceOptionsButtonsEnabled();
                _viewSwitcher.Show(ScreenViewId.MainMenuOptions);
            };

            _titleScreenView.ExitButton.Click += (_, _) =>
            {
                AudioManager.TryPlaySoundEffect("click");
                _game.Exit();
            };

            _viewSwitcher.Show(ScreenViewId.MainMenuTitle);
        }

        public override void LoadContent()
        {
            base.LoadContent();

            _font = Content.Load<SpriteFont>("Fonts/Pixelbasel");
            Debug.WriteLine($"Fuente cargada: {_font}");

            try
            {
                _backgroundTexture = ResourceManager.GetTexture(BackgroundTextureKey);
            }
            catch
            {
                _backgroundTexture = null;
            }
        }

        public override void Update(GameTime gameTime)
        {
            var keyboard = KeyboardExtended.GetState();

            if (keyboard.IsKeyDown(Keys.Enter))
            {
                _selectPlayerPresenter.Refresh();
                _viewSwitcher.Show(ScreenViewId.MainMenuSelectPlayer);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            if (_backgroundTexture == null)
                return;

            var camera = _game.Camera;
            var bounds = camera.BoundingRectangle;

            var dst = new Rectangle(
                (int)bounds.X,
                (int)bounds.Y,
                (int)bounds.Width,
                (int)bounds.Height);

            _game.SpriteBatch.Begin(
                samplerState: SamplerState.PointClamp,
                transformMatrix: camera.GetViewMatrix());

            _game.SpriteBatch.Draw(_backgroundTexture, dst, Color.White);

            _game.SpriteBatch.End();
        }

        private void ForceOptionsButtonsEnabled()
        {
            SetButtonEnabled(_optionsScreenView?.ToggleFullscreenButton, enabled: true);
            SetButtonEnabled(_optionsScreenView?.DeleteProgressButton, enabled: true);
            SetButtonEnabled(_optionsScreenView?.ExitOptionsButton, enabled: true);
        }

        private static void SetButtonEnabled(ButtonStandardCustom button, bool enabled)
        {
            if (button == null)
                return;

            button.IsEnabled = enabled;
            button.ButtonCategoryState = enabled
                ? ButtonStandardCustom.ButtonCategory.Enabled
                : ButtonStandardCustom.ButtonCategory.Disabled;
        }
    }
}