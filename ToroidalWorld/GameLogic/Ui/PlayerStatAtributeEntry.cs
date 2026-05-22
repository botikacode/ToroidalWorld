using System;

namespace ToroidalWorld.GameLogic.Ui
{
    internal sealed class PlayerStatAttributeEntry
    {
        private readonly Func<string> _getValueText;
        private readonly Func<bool> _canUpgrade;
        private readonly Action _upgrade;

        public string Name { get; }

        public PlayerStatAttributeEntry(
            string name,
            Func<string> getValueText,
            Func<bool> canUpgrade,
            Action upgrade)
        {
            Name = name;
            _getValueText = getValueText;
            _canUpgrade = canUpgrade;
            _upgrade = upgrade;
        }

        public string GetValueText()
        {
            return _getValueText();
        }

        public bool CanUpgrade()
        {
            return _canUpgrade();
        }

        public void Upgrade()
        {
            if (!CanUpgrade())
                return;

            _upgrade();
        }
    }
}