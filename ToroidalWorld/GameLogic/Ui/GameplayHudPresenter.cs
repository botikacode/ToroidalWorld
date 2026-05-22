using System;

using ToroidalWorld.GameLogic.Session;
using ToroidalWorld.Screens;

namespace ToroidalWorld.GameLogic.Ui
{
    internal sealed class GameplayHudPresenter
    {
        private readonly GameplayScreenView _view;

        private int _lastHpCurrent = int.MinValue;
        private int _lastHpMax = int.MinValue;

        private int _lastXp = int.MinValue;
        private int _lastXpToNext = int.MinValue;
        private int _lastLevel = int.MinValue;
        private int _lastPendingLevelUps = int.MinValue;

        private int _lastKills = int.MinValue;
        private int _lastTimeDisplaySeconds = int.MinValue;

        public GameplayHudPresenter(GameplayScreenView view)
        {
            _view = view;
        }

        public void Update(GameSession session)
        {
            if (session == null || _view == null)
                return;

            var hpBar = _view.PercentBarCustom_HPInstance;
            var xpBar = _view.PercentBarCustom_XPInstance;

            var killsLabel = _view.KillsLabel;
            var timeLabel = _view.TimeLabel;

            if (hpBar == null && xpBar == null && killsLabel == null && timeLabel == null)
                return;

            if (hpBar != null)
            {
                if (!session.TryGetPlayerHealth(out int hpCurrent, out int hpMax))
                {
                    hpCurrent = 0;
                    hpMax = 0;
                }

                SetHp(hpCurrent, hpMax);
            }

            if (xpBar != null)
            {
                if (!session.TryGetPlayerLevel(out int level, out int xp, out int xpToNext, out int pendingLevelUps))
                {
                    level = 0;
                    xp = 0;
                    xpToNext = 0;
                    pendingLevelUps = 0;
                }

                SetXp(level, xp, xpToNext, pendingLevelUps);
            }

            if (killsLabel != null)
            {
                var kills = session.GetEnemiesKilled();
                SetKills(kills);
            }

            if (timeLabel != null)
            {
                var timeSeconds = session.Stats?.Stage?.TimeSeconds ?? 0f;
                SetTime(timeSeconds);
            }
        }

        private void SetKills(int kills)
        {
            if (kills == _lastKills)
                return;

            _lastKills = kills;

            var label = _view.KillsLabel;
            if (label == null)
                return;

            label.AtributeValueText = kills.ToString();
        }

        private void SetTime(float timeSeconds)
        {
            if (timeSeconds < 0f)
                timeSeconds = 0f;

            var displaySeconds = (int)MathF.Floor(timeSeconds);
            if (displaySeconds == _lastTimeDisplaySeconds)
                return;

            _lastTimeDisplaySeconds = displaySeconds;

            var label = _view.TimeLabel;
            if (label == null)
                return;

            label.AtributeValueText = FormatTime(displaySeconds);
        }

        private static string FormatTime(int timeSeconds)
        {
            if (timeSeconds < 0)
                timeSeconds = 0;

            var ts = TimeSpan.FromSeconds(timeSeconds);

            if (ts.TotalHours >= 1)
                return $"{(int)ts.TotalHours:0}:{ts.Minutes:00}:{ts.Seconds:00}";

            return $"{ts.Minutes:00}:{ts.Seconds:00}";
        }

        private void SetHp(int hpCurrent, int hpMax)
        {
            if (hpCurrent == _lastHpCurrent && hpMax == _lastHpMax)
                return;

            _lastHpCurrent = hpCurrent;
            _lastHpMax = hpMax;

            var bar = _view.PercentBarCustom_HPInstance;
            if (bar == null)
                return;

            float percent = 0f;

            if (hpMax > 0)
                percent = hpCurrent * 100f / hpMax;

            percent = ClampPercent(percent);

            bar.BarPercent = percent;
            bar.Value = $"{hpCurrent}/{hpMax}";
        }

        private void SetXp(int level, int xp, int xpToNext, int pendingLevelUps)
        {
            if (level == _lastLevel
                && xp == _lastXp
                && xpToNext == _lastXpToNext
                && pendingLevelUps == _lastPendingLevelUps)
            {
                return;
            }

            _lastLevel = level;
            _lastXp = xp;
            _lastXpToNext = xpToNext;
            _lastPendingLevelUps = pendingLevelUps;

            var bar = _view.PercentBarCustom_XPInstance;
            if (bar == null)
                return;

            float percent = 0f;

            if (xpToNext > 0)
                percent = xp * 100f / xpToNext;

            percent = ClampPercent(percent);

            bar.BarPercent = percent;

            if (pendingLevelUps <= 0)
                bar.Value = $"Level {level}";
            else
                bar.Value = $"{pendingLevelUps}!";
        }

        private static float ClampPercent(float value)
        {
            if (value < 0f)
                return 0f;

            if (value > 100f)
                return 100f;

            return value;
        }
    }
}