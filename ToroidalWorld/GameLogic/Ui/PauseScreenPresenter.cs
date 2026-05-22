using ToroidalWorld.Components.Controls;
using ToroidalWorld.GameEngine;
using ToroidalWorld.GameLogic.Entities.Components;
using ToroidalWorld.GameLogic.Session;
using ToroidalWorld.Screens;

namespace ToroidalWorld.GameLogic.Ui
{
    internal sealed class PauseScreenPresenter
    {
        private readonly PauseScreenView _view;

        public PauseScreenPresenter(PauseScreenView view)
        {
            _view = view;
        }

        public void Refresh(GameSession session)
        {
            if (session == null)
                return;

            var stack = _view?.StackPanelInstance;
            if (stack == null)
                return;

            ClearChildren(stack);

            if (!session.TryGetPlayerEntity(out var player) || player == null)
                return;

            StatMultipliersComponent multipliers = null;
            PlayerLevelComponent level = null;
            HealthComponent health = null;
            HealthRegenComponent healthRegen = null;

            try
            {
                multipliers = player.Get<StatMultipliersComponent>();
                level = player.Get<PlayerLevelComponent>();
                health = player.Get<HealthComponent>();
                healthRegen = player.Get<HealthRegenComponent>();
            }
            catch
            {
                multipliers = null;
                level = null;
                health = null;
                healthRegen = null;
            }

            if (multipliers == null)
                return;

            var entries = PlayerStatAttributesFactory.Create(multipliers, level, health, healthRegen);

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];

                var row = new AtributeLabelButton();
                row.AtributeNameText = entry.Name;
                row.AtributeValueText = entry.GetValueText();

                var button = row.ButtonStandardCustomInstance;
                if (button != null)
                {
                    var canUpgrade = entry.CanUpgrade();

                    button.Text = "+";
                    button.IsEnabled = canUpgrade;
                    button.ButtonCategoryState = canUpgrade
                        ? ButtonStandardCustom.ButtonCategory.Enabled
                        : ButtonStandardCustom.ButtonCategory.Disabled;

                    button.Click += (_, _) =>
                    {
                        if (!entry.CanUpgrade())
                            return;

                        AudioManager.TryPlaySoundEffect("click");

                        entry.Upgrade();

                        Refresh(session);
                    };
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
    }
}