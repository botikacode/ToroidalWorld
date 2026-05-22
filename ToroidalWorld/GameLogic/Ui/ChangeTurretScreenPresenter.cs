using System;
using System.IO;
using ToroidalWorld.Components.Controls;
using ToroidalWorld.GameEngine;
using ToroidalWorld.GameLogic.Entities.Components;
using ToroidalWorld.GameLogic.Entities.Definitions;
using ToroidalWorld.GameLogic.Session;
using ToroidalWorld.Screens;

namespace ToroidalWorld.GameLogic.Ui
{
    internal sealed class ChangeTurretScreenPresenter
    {
        private readonly ChangeTurretScreenView _view;
        private readonly Action<int> _selectMountIndex;
        private readonly Action _cancel;

        public ChangeTurretScreenPresenter(ChangeTurretScreenView view, Action<int> selectMountIndex, Action cancel)
        {
            _view = view;
            _selectMountIndex = selectMountIndex;
            _cancel = cancel;

            if (_view?.ButtonDeny != null)
            {
                _view.ButtonDeny.Click += (_, _) =>
                {
                    AudioManager.TryPlaySoundEffect("click");
                    _cancel?.Invoke();
                };
            }
        }

        public void Refresh(GameSession session, TurretDefinition newTurret)
        {
            if (_view == null || session == null)
                return;

            var stack = _view.StackPanelInstance;
            if (stack == null)
                return;

            ClearChildren(stack);

            if (!session.TryGetPlayerEntity(out var player) || player == null)
                return;

            TurretMountsComponent mounts = null;

            try
            {
                mounts = player.Get<TurretMountsComponent>();
            }
            catch
            {
                mounts = null;
            }

            if (mounts?.Mounts == null)
                return;

            for (int i = 0; i < mounts.Mounts.Count; i++)
            {
                int mountIndex = i;
                var mount = mounts.Mounts[mountIndex];
                if (mount == null)
                    continue;

                TurretDefinition currentDef = null;

                try
                {
                    if (!string.IsNullOrWhiteSpace(mount.TurretName))
                        currentDef = ResourceManager.GetTurretDefinition(mount.TurretName);
                }
                catch
                {
                    currentDef = null;
                }

                var button = new ButtonStandardIconCustom();

                try
                {
                    button.ButtonCategoryState = ButtonStandardIconCustom.ButtonCategory.Enabled;
                }
                catch
                {
                }

                string title = currentDef?.Name ?? mount.TurretName ?? $"Slot {mountIndex}";
                string description = currentDef?.Description ?? string.Empty;

                if (button.TurretNameLabel != null)
                    button.TurretNameLabel.Text = title;

                if (button.TurretDescriptionLabel != null)
                    button.TurretDescriptionLabel.Text = description;

                string spritePath = ResolveTurretSpritePath(currentDef);
                if (!string.IsNullOrWhiteSpace(spritePath) && button.SpriteInstance != null)
                    button.SpriteInstance.SourceFileName = spritePath;

                button.Click += (_, _) =>
                {
                    AudioManager.TryPlaySoundEffect("click");
                    _selectMountIndex?.Invoke(mountIndex);
                };

                stack.AddChild(button);
            }
        }

        private static string ResolveTurretSpritePath(TurretDefinition def)
        {
            if (def == null)
                return null;

            string spriteKey = string.IsNullOrWhiteSpace(def.SpriteSheet) ? def.Name : def.SpriteSheet;
            if (string.IsNullOrWhiteSpace(spriteKey))
                return null;

            string baseDir = Directory.GetCurrentDirectory();

            string pngPath = Path.Combine(baseDir, "Resources", "Sprites", $"{spriteKey}.png");
            if (File.Exists(pngPath))
                return pngPath;

            string jpgPath = Path.Combine(baseDir, "Resources", "Sprites", $"{spriteKey}.jpg");
            if (File.Exists(jpgPath))
                return jpgPath;

            string jpegPath = Path.Combine(baseDir, "Resources", "Sprites", $"{spriteKey}.jpeg");
            if (File.Exists(jpegPath))
                return jpegPath;

            return null;
        }

        private static void ClearChildren(StackPanel stack)
        {
            while (stack.Children.Count > 0)
            {
                var child = stack.Children[0];
                stack.RemoveChild(child);
            }
        }
    }
}