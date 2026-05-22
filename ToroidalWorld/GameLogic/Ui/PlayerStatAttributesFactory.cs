using System;
using System.Collections.Generic;
using System.Globalization;
using ToroidalWorld.GameLogic.Entities.Components;

namespace ToroidalWorld.GameLogic.Ui
{
    internal static class PlayerStatAttributesFactory
    {
        private const float DefaultStep = 0.05f;
        private const float MaxEpsilon = 0.0001f;

        private const int HealthStep = 10;
        private const int HealthMax = 5000;

        private const float HealthRegenStep = 0.1f;
        private const float HealthRegenMax = 10.0f;

        private const float DamageMax = 3.00f;
        private const float AreaDamageMax = 3.00f;
        private const float AreaRadiusMax = 4.00f;
        private const float MoveSpeedMax = 2.00f;
        private const float IcebreakerRateMax = 3.00f;
        private const float IcebreakerStrengthMax = 3.00f;
        private const float ExperienceGainMax = 3.00f;
        private const float ExperienceMagnetRangeMax = 3.00f;

        private const float TurretAimSpeedMax = 3.00f;
        private const float TurretCooldownRateMax = 3.00f;
        private const float TurretRangeMax = 3.00f;

        public static IReadOnlyList<PlayerStatAttributeEntry> Create(
            StatMultipliersComponent multipliers,
            PlayerLevelComponent playerLevel,
            HealthComponent health,
            HealthRegenComponent healthRegen)
        {
            if (multipliers == null)
                return Array.Empty<PlayerStatAttributeEntry>();

            bool HasPoints()
            {
                return (playerLevel?.PendingLevelUps ?? 0) > 0;
            }

            bool TrySpendPoint()
            {
                if (!HasPoints() || playerLevel == null)
                    return false;

                playerLevel.PendingLevelUps--;
                if (playerLevel.PendingLevelUps < 0)
                    playerLevel.PendingLevelUps = 0;

                return true;
            }

            var list = new List<PlayerStatAttributeEntry>(capacity: 16);

            if (health != null)
            {
                list.Add(new PlayerStatAttributeEntry(
                    "Health",
                    getValueText: () => $"{health.CurrentHealth}/{health.MaxHealth}",
                    canUpgrade: () => HasPoints() && health.MaxHealth < HealthMax,
                    upgrade: () =>
                    {
                        if (!TrySpendPoint())
                            return;

                        int oldMax = health.MaxHealth;
                        int newMax = oldMax + HealthStep;
                        if (newMax > HealthMax)
                            newMax = HealthMax;

                        int delta = newMax - oldMax;

                        health.MaxHealth = newMax;
                        health.CurrentHealth += delta;

                        if (health.CurrentHealth > health.MaxHealth)
                            health.CurrentHealth = health.MaxHealth;
                    }));
            }

            if (healthRegen != null)
            {
                list.Add(new PlayerStatAttributeEntry(
                    "Health Regen",
                    getValueText: () => FormatAsHpPerSecond(healthRegen.RegenPerSecond),
                    canUpgrade: () => HasPoints() && !IsMaxed(healthRegen.RegenPerSecond, HealthRegenMax),
                    upgrade: () =>
                    {
                        if (!TrySpendPoint())
                            return;

                        healthRegen.RegenPerSecond = ClampRange(healthRegen.RegenPerSecond + HealthRegenStep, max: HealthRegenMax);
                    }));
            }

            list.Add(CreateMultiplier(
                "Damage",
                () => multipliers.DamageMultiplier,
                v => multipliers.DamageMultiplier = v,
                step: DefaultStep,
                max: DamageMax,
                format: FormatAsPercentBonus,
                hasPoints: HasPoints,
                trySpendPoint: TrySpendPoint));

            list.Add(CreateMultiplier(
                "Area Damage",
                () => multipliers.AreaDamageMultiplier,
                v => multipliers.AreaDamageMultiplier = v,
                step: DefaultStep,
                max: AreaDamageMax,
                format: FormatAsPercentBonus,
                hasPoints: HasPoints,
                trySpendPoint: TrySpendPoint));

            list.Add(CreateMultiplier(
                "Area Radius",
                () => multipliers.AreaRadiusMultiplier,
                v => multipliers.AreaRadiusMultiplier = v,
                step: DefaultStep,
                max: AreaRadiusMax,
                format: FormatAsPercentBonus,
                hasPoints: HasPoints,
                trySpendPoint: TrySpendPoint));

            list.Add(CreateMultiplier(
                "Move Speed",
                () => multipliers.MoveSpeedMultiplier,
                v => multipliers.MoveSpeedMultiplier = v,
                step: DefaultStep,
                max: MoveSpeedMax,
                format: FormatAsPercentBonus,
                hasPoints: HasPoints,
                trySpendPoint: TrySpendPoint));

            // Combined Turret: aim speed + fire rate (1 point upgrades both)
            list.Add(new PlayerStatAttributeEntry(
                "Fire Rate",
                getValueText: () =>
                {
                    var aim = multipliers.TurretAimSpeedMultiplier;
                    var fireRate = multipliers.TurretCooldownRateMultiplier;
                    return $"{FormatAsPercentBonus(fireRate)}";
                },
                canUpgrade: () =>
                {
                    if (!HasPoints())
                        return false;

                    return !IsMaxed(multipliers.TurretAimSpeedMultiplier, TurretAimSpeedMax)
                        && !IsMaxed(multipliers.TurretCooldownRateMultiplier, TurretCooldownRateMax);
                },
                upgrade: () =>
                {
                    if (!TrySpendPoint())
                        return;

                    multipliers.TurretAimSpeedMultiplier =
                        ClampRange(multipliers.TurretAimSpeedMultiplier + DefaultStep, max: TurretAimSpeedMax);

                    multipliers.TurretCooldownRateMultiplier =
                        ClampRange(multipliers.TurretCooldownRateMultiplier + DefaultStep, max: TurretCooldownRateMax);
                }));

            list.Add(CreateMultiplier(
                "Turret Range",
                () => multipliers.TurretRangeMultiplier,
                v => multipliers.TurretRangeMultiplier = v,
                step: DefaultStep,
                max: TurretRangeMax,
                format: FormatAsPercentBonus,
                hasPoints: HasPoints,
                trySpendPoint: TrySpendPoint));

            // Combined Icebreaker: rate + strength (1 point upgrades both)
            list.Add(new PlayerStatAttributeEntry(
                "Icebreaker",
                getValueText: () =>
                {
                    var rate = multipliers.IcebreakerRateMultiplier;
                    var strength = multipliers.IcebreakerStrengthMultiplier;
                    return $"{FormatAsPercentBonus(strength)}";
                },
                canUpgrade: () =>
                {
                    if (!HasPoints())
                        return false;

                    return !IsMaxed(multipliers.IcebreakerRateMultiplier, IcebreakerRateMax)
                        && !IsMaxed(multipliers.IcebreakerStrengthMultiplier, IcebreakerStrengthMax);
                },
                upgrade: () =>
                {
                    if (!TrySpendPoint())
                        return;

                    multipliers.IcebreakerRateMultiplier =
                        ClampRange(multipliers.IcebreakerRateMultiplier + DefaultStep, max: IcebreakerRateMax);

                    multipliers.IcebreakerStrengthMultiplier =
                        ClampRange(multipliers.IcebreakerStrengthMultiplier + DefaultStep, max: IcebreakerStrengthMax);
                }));

            list.Add(CreateMultiplier(
                "XP Gain",
                () => multipliers.ExperienceGainMultiplier,
                v => multipliers.ExperienceGainMultiplier = v,
                step: DefaultStep,
                max: ExperienceGainMax,
                format: FormatAsPercentBonus,
                hasPoints: HasPoints,
                trySpendPoint: TrySpendPoint));

            list.Add(CreateMultiplier(
                "XP Magnet",
                () => multipliers.ExperienceMagnetRangeMultiplier,
                v => multipliers.ExperienceMagnetRangeMultiplier = v,
                step: DefaultStep,
                max: ExperienceMagnetRangeMax,
                format: FormatAsPercentBonus,
                hasPoints: HasPoints,
                trySpendPoint: TrySpendPoint));

            return list;
        }

        private static PlayerStatAttributeEntry CreateMultiplier(
            string name,
            Func<float> getter,
            Action<float> setter,
            float step,
            float? max,
            Func<float, string> format,
            Func<bool> hasPoints,
            Func<bool> trySpendPoint)
        {
            return new PlayerStatAttributeEntry(
                name,
                getValueText: () => format(getter()),
                canUpgrade: () => hasPoints() && !IsMaxed(getter(), max),
                upgrade: () =>
                {
                    if (!trySpendPoint())
                        return;

                    var next = getter() + step;
                    setter(ClampRange(next, max));
                });
        }

        private static bool IsMaxed(float current, float? max)
        {
            if (max is null)
                return false;

            return current >= max.Value - MaxEpsilon;
        }

        private static float ClampRange(float value, float? max)
        {
            if (value < 0f)
                value = 0f;

            if (max is not null && value > max.Value)
                value = max.Value;

            return value;
        }

        private static string FormatAsMultiplier(float value)
        {
            return "x" + value.ToString("0.00", CultureInfo.InvariantCulture);
        }

        private static string FormatAsPercentBonus(float multiplier)
        {
            var bonus = multiplier - 1f;
            return bonus.ToString("+0.##%;-0.##%;0%", CultureInfo.InvariantCulture);
        }

        private static string FormatAsHpPerSecond(float value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture) + " hp/s";
        }
    }
}