using System;
using ToroidalWorld.Components.Controls;
using ToroidalWorld.GameEngine;
using ToroidalWorld.GameLogic.Progress;
using ToroidalWorld.Screens;

namespace ToroidalWorld.GameLogic.Ui
{
    internal sealed class LeaderboardScreenPresenter
    {
        private const int MaxEntries = 10;

        private readonly LeaderboardScreenView _view;
        private readonly Action _exit;

        public LeaderboardScreenPresenter(LeaderboardScreenView view, Action exit)
        {
            _view = view;
            _exit = exit;

            if (_view?.ButtonExit != null)
            {
                _view.ButtonExit.Click += (_, _) =>
                {
                    AudioManager.TryPlaySoundEffect("click");
                    _exit?.Invoke();
                };
            }
        }

        public void Refresh()
        {
            if (_view == null)
                return;

            var progress = PlayerProgressStore.Default.Load();

            if (_view.LabelInstance != null)
                _view.LabelInstance.Text = $"Total points: {progress.TotalPoints}";

            var stack = _view.StackPanelInstance;
            if (stack == null)
                return;

            ClearChildren(stack);

            for (int i = 0; i < MaxEntries; i++)
            {
                var row = new RecordLabel();

                var record = i < progress.BestRuns.Count ? progress.BestRuns[i] : null;

                if (row.NumAttemptLabel != null)
                    row.NumAttemptLabel.Text = (i + 1).ToString("D2");

                if (record == null)
                {
                    if (row.DateValue != null) row.DateValue.Text = "-";
                    if (row.TimeValue != null) row.TimeValue.Text = "--:--";
                    if (row.KillsValue != null) row.KillsValue.Text = "-";
                }
                else
                {
                    if (row.DateValue != null) row.DateValue.Text = record.UtcTimestamp.ToLocalTime().ToString("dd/MM/yy HH:mm");
                    if (row.TimeValue != null) row.TimeValue.Text = FormatTime(record.TimeSeconds);
                    if (row.KillsValue != null) row.KillsValue.Text = record.Kills.ToString("D3");
                }

                stack.AddChild(row);
            }
        }

        private static void ClearChildren(StackPanel stack)
        {
            while (stack.Children.Count > 0)
            {
                var child = stack.Children[0];
                stack.RemoveChild(child);
            }
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