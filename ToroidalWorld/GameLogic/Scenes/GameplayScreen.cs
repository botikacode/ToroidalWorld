using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using MonoGameGum;
using ToroidalWorld.GameEngine;
using ToroidalWorld.GameLogic.Progress;
using ToroidalWorld.GameLogic.Session;
using ToroidalWorld.GameLogic.Ui;
using ToroidalWorld.Screens;

namespace ToroidalWorld.GameLogic.Scenes
{
    public class GameplayScreen : GameScreen
    {
        private readonly GameMain _game;

        private readonly System.Random _random = new System.Random();

        private GameSession _currentSession;

        private GameplayScreenView _gameplayView;
        private PauseScreenView _pauseView;
        private GameOverScreenView _gameOverView;

        private NewTurretScreenView _newTurretView;
        private ChangeTurretScreenView _changeTurretView;

        private GameplayHudPresenter _hudPresenter;
        private PauseScreenPresenter _pausePresenter;
        private GameOverScreenPresenter _gameOverPresenter;

        private NewTurretScreenPresenter _newTurretPresenter;
        private ChangeTurretScreenPresenter _changeTurretPresenter;

        private GumRootViewSwitcher _viewSwitcher;

        private ScreenViewId _currentViewId = unchecked((ScreenViewId)(-1));

        private bool _progressSavedForCurrentSession;

        private readonly string _playerBoatName;

        public GameplayScreen(Game game, string playerBoatName = null) : base(game)
        {
            _game = (GameMain)game;
            _playerBoatName = playerBoatName;
            DrawWhenInactive = true;
        }

        public override void Initialize()
        {
            base.Initialize();

            GumService.Default.Root.Children.Clear();

            _gameplayView = new GameplayScreenView();
            _pauseView = new PauseScreenView();
            _gameOverView = new GameOverScreenView();

            _newTurretView = new NewTurretScreenView();
            _changeTurretView = new ChangeTurretScreenView();

            _hudPresenter = new GameplayHudPresenter(_gameplayView);
            _pausePresenter = new PauseScreenPresenter(_pauseView);
            _gameOverPresenter = new GameOverScreenPresenter(
                _gameOverView,
                restart: RestartSession,
                mainMenu: GoToMainMenu,
                exit: _game.Exit);

            _newTurretPresenter = new NewTurretScreenPresenter(
                _newTurretView,
                deny: DenyTurretPickup,
                openChange: OpenChangeTurret);

            _changeTurretPresenter = new ChangeTurretScreenPresenter(
                _changeTurretView,
                selectMountIndex: ConfirmTurretPickupToMount,
                cancel: CancelChangeTurret);

            _viewSwitcher = new GumRootViewSwitcher();
            _viewSwitcher.Register(ScreenViewId.GameplayHud, _gameplayView);
            _viewSwitcher.Register(ScreenViewId.GameplayPause, _pauseView);
            _viewSwitcher.Register(ScreenViewId.GameplayGameOver, _gameOverView);
            _viewSwitcher.Register(ScreenViewId.GameplayNewTurret, _newTurretView);
            _viewSwitcher.Register(ScreenViewId.GameplayChangeTurret, _changeTurretView);

            ShowView(ScreenViewId.GameplayHud);
        }

        public override void LoadContent()
        {
            base.LoadContent();
            StartNewSession();
        }

        public override void Update(GameTime gameTime)
        {
            var keyboard = KeyboardExtended.GetState();

            if (_currentViewId == ScreenViewId.GameplayHud
                && _currentSession.IsWorldReady
                && (keyboard.WasKeyPressed(Keys.Escape) || keyboard.WasKeyPressed(Keys.P)))
            {
                ShowView(ScreenViewId.GameplayPause);
                return;
            }

            if (_currentViewId == ScreenViewId.GameplayPause
                && (keyboard.WasKeyPressed(Keys.Escape) || keyboard.WasKeyPressed(Keys.P)))
            {
                ShowView(ScreenViewId.GameplayHud);
                return;
            }

            if (_currentViewId == ScreenViewId.GameplayNewTurret)
            {
                if (keyboard.WasKeyPressed(Keys.Escape))
                    DenyTurretPickup();

                return;
            }

            if (_currentViewId == ScreenViewId.GameplayChangeTurret)
            {
                if (keyboard.WasKeyPressed(Keys.Escape))
                    CancelChangeTurret();

                return;
            }

            if (_currentViewId == ScreenViewId.GameplayPause)
                return;

            if (_currentViewId == ScreenViewId.GameplayGameOver)
            {
                if (keyboard.WasKeyPressed(Keys.Enter))
                {
                    RestartSession();
                    return;
                }

                if (keyboard.WasKeyPressed(Keys.Escape))
                {
                    GoToMainMenu();
                    return;
                }

                return;
            }

            if (_currentSession.HasPendingTurretPickup)
            {
                ShowNewTurretPickupUi();
                return;
            }

            _currentSession.Update(gameTime);

            if (_currentSession.HasPendingTurretPickup)
            {
                ShowNewTurretPickupUi();
                return;
            }

            _hudPresenter.Update(_currentSession);

            if (_currentSession.HasEnded)
            {
                ShowView(ScreenViewId.GameplayGameOver);
                return;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            _currentSession?.Draw(gameTime);
        }

        private void CancelChangeTurret()
        {
            ShowView(ScreenViewId.GameplayNewTurret);
        }

        private void ShowNewTurretPickupUi()
        {
            if (_currentSession?.PendingTurretPickup == null)
                return;

            ToroidalWorld.GameLogic.Entities.Definitions.TurretDefinition def = null;

            try
            {
                def = ToroidalWorld.GameEngine.ResourceManager.GetTurretDefinition(_currentSession.PendingTurretPickup.TurretName);
            }
            catch
            {
                def = null;
            }

            _newTurretPresenter.Refresh(def);
            ShowView(ScreenViewId.GameplayNewTurret);
        }

        private void DenyTurretPickup()
        {
            _currentSession?.CancelTurretPickupSelection(destroyPickup: true);
            ShowView(ScreenViewId.GameplayHud);
        }

        private void OpenChangeTurret()
        {
            if (_currentSession?.PendingTurretPickup == null)
                return;

            ToroidalWorld.GameLogic.Entities.Definitions.TurretDefinition def = null;

            try
            {
                def = ToroidalWorld.GameEngine.ResourceManager.GetTurretDefinition(_currentSession.PendingTurretPickup.TurretName);
            }
            catch
            {
                def = null;
            }

            _changeTurretPresenter.Refresh(_currentSession, def);
            ShowView(ScreenViewId.GameplayChangeTurret);
        }

        private void ConfirmTurretPickupToMount(int mountIndex)
        {
            bool ok = _currentSession != null && _currentSession.TryConfirmTurretPickupSelection(mountIndex);
            ShowView(ScreenViewId.GameplayHud);
        }

        private void ShowView(ScreenViewId viewId)
        {
            if (_currentViewId == viewId)
                return;

            if (viewId == ScreenViewId.GameplayPause)
            {
                AudioManager.StopManagedLoopingSoundEffects(disposeInstances: false);
                _pausePresenter.Refresh(_currentSession);
            }

            if (viewId == ScreenViewId.GameplayGameOver)
            {
                SaveProgressForCurrentSessionIfNeeded();
                _gameOverPresenter.Refresh(_currentSession);
            }

            _currentViewId = viewId;
            _viewSwitcher.Show(viewId);
        }

        private void SaveProgressForCurrentSessionIfNeeded()
        {
            if (_progressSavedForCurrentSession || _currentSession == null)
                return;

            _progressSavedForCurrentSession = true;

            var kills = _currentSession.GetEnemiesKilled();
            var timeSeconds = _currentSession.Stats?.Stage?.TimeSeconds ?? 0f;

            PlayerProgressStore.Default.RegisterGameResult(kills, timeSeconds);
        }

        private void StartNewSession()
        {
            _currentSession?.Dispose();
            _currentSession = null;

            _progressSavedForCurrentSession = false;

            _currentSession = new GameSession(
                "world",
                _random.Next(),
                _game.Camera,
                _game.GraphicsDevice,
                _game.SpriteBatch,
                Content,
                playerBoatName: _playerBoatName);
        }

        private void RestartSession()
        {
            StartNewSession();
            ShowView(ScreenViewId.GameplayHud);
        }

        private void GoToMainMenu()
        {
            _currentSession?.Dispose();
            _currentSession = null;

            ScreenManager.ShowScreen(
                new MainMenuScreen(_game),
                new FadeTransition(GraphicsDevice, Color.Black, 0.5f));
        }
    }
}