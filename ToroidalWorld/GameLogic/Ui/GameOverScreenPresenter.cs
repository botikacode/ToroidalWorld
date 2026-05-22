using System;

using ToroidalWorld.GameEngine;
using ToroidalWorld.GameLogic.Session;
using ToroidalWorld.Screens;

namespace ToroidalWorld.GameLogic.Ui
{
    internal sealed class GameOverScreenPresenter
    {
        private readonly GameOverScreenView _view;

        private readonly Action _restart;
        private readonly Action _mainMenu;
        private readonly Action _exit;

        public GameOverScreenPresenter(
            GameOverScreenView view,
            Action restart,
            Action mainMenu,
            Action exit)
        {
            _view = view;

            _restart = restart;
            _mainMenu = mainMenu;
            _exit = exit;

            if (_view?.RestartButton != null)
            {
                _view.RestartButton.Click += (_, _) =>
                {
                    AudioManager.TryPlaySoundEffect("click");
                    _restart?.Invoke();
                };
            }

            if (_view?.MainMenuButton != null)
            {
                _view.MainMenuButton.Click += (_, _) =>
                {
                    AudioManager.TryPlaySoundEffect("click");
                    _mainMenu?.Invoke();
                };
            }

            if (_view?.ExitButon != null)
            {
                _view.ExitButon.Click += (_, _) =>
                {
                    AudioManager.TryPlaySoundEffect("click");
                    _exit?.Invoke();
                };
            }
        }

        public void Refresh(GameSession session)
        {
            if (session == null || _view == null)
                return;

            if (_view.KillsValueLabel != null)
                _view.KillsValueLabel.ValueText = session.GetEnemiesKilled().ToString();

            if (_view.TimeValueLabel != null)
                _view.TimeValueLabel.ValueText = FormatTime(session.Stats?.Stage?.TimeSeconds ?? 0f);
        }

        private static string FormatTime(float timeSeconds)
        {
            if (timeSeconds < 0f)
                timeSeconds = 0f;

            var ts = TimeSpan.FromSeconds(timeSeconds);

            if (ts.TotalHours >= 1)
                return $"{(int)ts.TotalHours:0}:{ts.Minutes:00}:{ts.Seconds:00}";

            return $"{ts.Minutes:00}:{ts.Seconds:00}";
        }
    }
}