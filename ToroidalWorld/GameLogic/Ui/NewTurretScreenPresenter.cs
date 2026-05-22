using System;
using System.IO;
using ToroidalWorld.GameEngine;
using ToroidalWorld.GameLogic.Entities.Definitions;
using ToroidalWorld.Screens;

namespace ToroidalWorld.GameLogic.Ui
{
    internal sealed class NewTurretScreenPresenter
    {
        private readonly NewTurretScreenView _view;

        private readonly Action _deny;
        private readonly Action _openChange;

        public NewTurretScreenPresenter(NewTurretScreenView view, Action deny, Action openChange)
        {
            _view = view;
            _deny = deny;
            _openChange = openChange;

            if (_view?.ButtonDeny != null)
            {
                _view.ButtonDeny.Click += (_, _) =>
                {
                    AudioManager.TryPlaySoundEffect("click");
                    _deny?.Invoke();
                };
            }

            if (_view?.NewTurretButton != null)
            {
                _view.NewTurretButton.Click += (_, _) =>
                {
                    AudioManager.TryPlaySoundEffect("click");
                    _openChange?.Invoke();
                };
            }
        }

        public void Refresh(TurretDefinition turretDef)
        {
            if (_view == null)
                return;

            var button = _view.NewTurretButton;
            if (button == null)
                return;

            button.Text = turretDef?.Name ?? "Unknown";
            button.TurretDescriptionLabel.Text = turretDef?.Description ?? string.Empty;

            string spritePath = ResolveTurretSpritePath(turretDef);
            if (!string.IsNullOrWhiteSpace(spritePath))
                button.SpriteInstanceSourceFile = spritePath;
        }

        private static string ResolveTurretSpritePath(TurretDefinition def)
        {
            if (def == null)
                return null;

            string spriteKey = string.IsNullOrWhiteSpace(def.SpriteSheet) ? def.Name : def.SpriteSheet;
            if (string.IsNullOrWhiteSpace(spriteKey))
                return null;

            string pngPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Sprites", $"{spriteKey}.png");
            if (File.Exists(pngPath))
                return pngPath;

            string jpgPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Sprites", $"{spriteKey}.jpg");
            if (File.Exists(jpgPath))
                return jpgPath;

            string jpegPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Sprites", $"{spriteKey}.jpeg");
            if (File.Exists(jpegPath))
                return jpegPath;

            return null;
        }
    }
}