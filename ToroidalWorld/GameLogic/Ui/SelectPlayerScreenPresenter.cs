using System;
using System.Collections.Generic;
using System.Reflection;
using ToroidalWorld.GameEngine;
using ToroidalWorld.GameLogic.Entities.Definitions;
using ToroidalWorld.GameLogic.Progress;
using ToroidalWorld.Screens;

namespace ToroidalWorld.GameLogic.Ui
{
    internal sealed class SelectPlayerScreenPresenter
    {
        private readonly SelectPlayerScreenView _view;
        private readonly Action<string> _selectBoat;

        private readonly List<BoatDefinition> _boats;

        private int _index;

        public SelectPlayerScreenPresenter(SelectPlayerScreenView view, Action<string> selectBoat)
        {
            _view = view;
            _selectBoat = selectBoat;

            _boats = new List<BoatDefinition>(ResourceManager.GetBoatDefinitions());

            if (_view?.LeftButton != null)
            {
                _view.LeftButton.Click += (_, _) =>
                {
                    AudioManager.TryPlaySoundEffect("click");
                    Move(-1);
                };
            }

            if (_view?.RightButton != null)
            {
                _view.RightButton.Click += (_, _) =>
                {
                    AudioManager.TryPlaySoundEffect("click");
                    Move(+1);
                };
            }

            if (_view?.SelectButton != null)
            {
                _view.SelectButton.Click += (_, _) =>
                {
                    TrySelectCurrentBoat();
                };
            }
        }

        public void Refresh()
        {
            if (_view == null)
                return;

            if (_boats.Count == 0)
                return;

            if (_index < 0) _index = 0;
            if (_index >= _boats.Count) _index = _boats.Count - 1;

            var boat = _boats[_index];

            var progress = PlayerProgressStore.Default.Load();
            var totalPoints = progress?.TotalPoints ?? 0;

            UpdateRemainingPointsLabel(totalPoints, boat.RequiredPoints);
            TrySetPreviewSprite(boat);
        }

        private void Move(int delta)
        {
            if (_boats.Count == 0)
                return;

            _index += delta;

            if (_index < 0) _index = _boats.Count - 1;
            if (_index >= _boats.Count) _index = 0;

            Refresh();
        }

        private void TrySelectCurrentBoat()
        {
            if (_boats.Count == 0)
                return;

            var progress = PlayerProgressStore.Default.Load();
            var totalPoints = progress?.TotalPoints ?? 0;

            var boat = _boats[_index];
            if (totalPoints < boat.RequiredPoints)
                return;

            _selectBoat?.Invoke(boat.Name);
        }

        private void UpdateRemainingPointsLabel(long totalPoints, long requiredPoints)
        {
            var label = _view?.RemaingPointsLabel;
            if (label == null)
                return;

            long remaining = requiredPoints - totalPoints;
            if (remaining < 0)
                remaining = 0;

            label.Text = $"Remaining Points: {remaining}";

            TrySetVisible(label, remaining != 0);
        }

        private void TrySetPreviewSprite(BoatDefinition boat)
        {
            var sprite = _view?.SpriteInstance;
            if (sprite == null || boat == null)
                return;

            try
            {
                var spriteKey = string.IsNullOrWhiteSpace(boat.SpriteSheet) ? boat.Name : boat.SpriteSheet;
                var sheet = ResourceManager.GetSpriteSheet(spriteKey);

                sprite.Texture = sheet.TextureAtlas.Texture;
            }
            catch
            {
            }
        }

        private static void TrySetVisible(object element, bool visible)
        {
            if (element == null)
                return;

            try
            {
                var type = element.GetType();

                var directProp = type.GetProperty("IsVisible", BindingFlags.Instance | BindingFlags.Public)
                                 ?? type.GetProperty("Visible", BindingFlags.Instance | BindingFlags.Public);

                if (directProp != null && directProp.CanWrite && directProp.PropertyType == typeof(bool))
                {
                    directProp.SetValue(element, visible);
                    return;
                }

                var visualProp = type.GetProperty("Visual", BindingFlags.Instance | BindingFlags.Public);
                var visual = visualProp?.GetValue(element);
                if (visual == null)
                    return;

                var visualType = visual.GetType();
                var visualVisibleProp = visualType.GetProperty("Visible", BindingFlags.Instance | BindingFlags.Public);
                if (visualVisibleProp != null && visualVisibleProp.CanWrite && visualVisibleProp.PropertyType == typeof(bool))
                    visualVisibleProp.SetValue(visual, visible);
            }
            catch
            {
            }
        }
    }
}