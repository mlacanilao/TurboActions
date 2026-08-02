using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using TurboActions.UI;
using UnityEngine;

namespace TurboActions;

internal static class ModInfo
{
    internal const string Guid = "omegaplatinum.elin.turboactions";
    internal const string Name = "Turbo Actions";
    internal const string Version = "2.0.0";
    internal const string ModOptionsGuid = "evilmask.elinplugins.modoptions";
}

[BepInPlugin(GUID: ModInfo.Guid, Name: ModInfo.Name, Version: ModInfo.Version)]
[BepInDependency(ModInfo.ModOptionsGuid, BepInDependency.DependencyFlags.SoftDependency)]
internal class TurboActions : BaseUnityPlugin
{
    private static readonly KeyCode[] StandardModifierKeys =
    {
        KeyCode.LeftControl,
        KeyCode.RightControl,
        KeyCode.LeftShift,
        KeyCode.RightShift,
        KeyCode.LeftAlt,
        KeyCode.RightAlt,
        KeyCode.LeftCommand,
        KeyCode.RightCommand
    };

    private static int? lastLoggedInvalidTurboModeSpeedMultiplier;

    internal static TurboActions? Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        TurboActionsConfig.LoadConfig(config: Config);
        int configuredMultiplier = TurboActionsConfig.TurboModeSpeedMultiplier.Value;
        LogInvalidTurboModeSpeedMultiplierIfNeeded(configuredMultiplier: configuredMultiplier);
        Harmony.CreateAndPatchAll(type: typeof(Patcher), harmonyInstanceId: ModInfo.Guid);

        if (HasModOptionsPlugin() == false)
        {
            LogDebug(message: "Mod Options was not detected; optional configuration UI was not registered.");
            return;
        }

        try
        {
            UIController.RegisterUI();
        }
        catch (Exception ex)
        {
            LogError(message: "Mod Options registration failed; Turbo Actions gameplay remains available. " + ex);
        }
    }

    private void Update()
    {
        int selectedPreset = 0;
        int selectedModifierCount = -1;

        if (TryMatchPresetShortcut(
            shortcut: TurboActionsConfig.GameSpeedTurbo1Shortcut.Value,
            modifierCount: out int preset1ModifierCount))
        {
            selectedPreset = 1;
            selectedModifierCount = preset1ModifierCount;
        }

        if (TryMatchPresetShortcut(
            shortcut: TurboActionsConfig.GameSpeedTurbo2Shortcut.Value,
            modifierCount: out int preset2ModifierCount) &&
            preset2ModifierCount > selectedModifierCount)
        {
            selectedPreset = 2;
            selectedModifierCount = preset2ModifierCount;
        }

        if (TryMatchPresetShortcut(
            shortcut: TurboActionsConfig.GameSpeedTurbo3Shortcut.Value,
            modifierCount: out int preset3ModifierCount) &&
            preset3ModifierCount > selectedModifierCount)
        {
            selectedPreset = 3;
        }

        bool togglePressed = Input.GetKeyDown(key: TurboActionsConfig.ToggleTurboKey.Value);
        if (selectedPreset == 0 && togglePressed == false)
        {
            return;
        }

        if (EClass.core?.IsGameStarted != true)
        {
            return;
        }

        if (EInput.isInputFieldActive)
        {
            return;
        }

        if (EClass.ui?.BlockInput == true ||
            EClass.ui?.BlockActions == true)
        {
            return;
        }

        Chara? player = EClass.pc;
        if (player == null)
        {
            return;
        }

        if (selectedPreset != 0)
        {
            ApplyPreset(presetNumber: selectedPreset, player: player);
            return;
        }

        SetTurboActionsEnabled(
            enabled: TurboActionsConfig.EnableTurboMode.Value == false,
            showStatus: true);
    }

    private static bool TryMatchPresetShortcut(
        KeyboardShortcut shortcut,
        out int modifierCount)
    {
        modifierCount = 0;
        int configuredStandardModifiers = 0;
        KeyCode mainKey = shortcut.MainKey;
        if (mainKey == KeyCode.None || Input.GetKeyDown(key: mainKey) == false)
        {
            return false;
        }

        foreach (KeyCode modifier in shortcut.Modifiers)
        {
            modifierCount++;
            configuredStandardModifiers |= GetStandardModifierBit(keyCode: modifier);
            if (Input.GetKey(key: modifier) == false)
            {
                return false;
            }
        }

        foreach (KeyCode standardModifier in StandardModifierKeys)
        {
            if (standardModifier == mainKey)
            {
                continue;
            }

            int modifierBit = GetStandardModifierBit(keyCode: standardModifier);
            bool isConfigured = (configuredStandardModifiers & modifierBit) != 0;
            if (Input.GetKey(key: standardModifier) != isConfigured)
            {
                return false;
            }
        }

        return true;
    }

    private static int GetStandardModifierBit(KeyCode keyCode)
    {
        switch (keyCode)
        {
            case KeyCode.LeftControl:
                return 1 << 0;
            case KeyCode.RightControl:
                return 1 << 1;
            case KeyCode.LeftShift:
                return 1 << 2;
            case KeyCode.RightShift:
                return 1 << 3;
            case KeyCode.LeftAlt:
                return 1 << 4;
            case KeyCode.RightAlt:
                return 1 << 5;
            case KeyCode.LeftCommand:
                return 1 << 6;
            case KeyCode.RightCommand:
                return 1 << 7;
            default:
                return 0;
        }
    }

    private static void ApplyPreset(int presetNumber, Chara player)
    {
        ConfigEntry<int> presetMultiplier;
        switch (presetNumber)
        {
            case 1:
                presetMultiplier = TurboActionsConfig.GameSpeedTurbo1Multiplier;
                break;
            case 2:
                presetMultiplier = TurboActionsConfig.GameSpeedTurbo2Multiplier;
                break;
            case 3:
                presetMultiplier = TurboActionsConfig.GameSpeedTurbo3Multiplier;
                break;
            default:
                return;
        }

        int configuredMultiplier = presetMultiplier.Value;
        int previousMultiplier = TurboActionsConfig.TurboModeSpeedMultiplier.Value;
        TurboActionsConfig.TurboModeSpeedMultiplier.Value = configuredMultiplier;
        int effectiveMultiplier = GetEffectiveTurboModeSpeedMultiplier(
            configuredMultiplier: out int storedMultiplier);
        bool enabled = TurboActionsConfig.EnableTurboMode.Value;

        if (previousMultiplier != configuredMultiplier)
        {
            FeatureTestLog.Log(
                feature: "Preset",
                detail: "preset=" + presetNumber.ToString(provider: CultureInfo.InvariantCulture) +
                    ", configuredMultiplier=" + storedMultiplier.ToString(provider: CultureInfo.InvariantCulture) +
                    ", effectiveMultiplier=" + effectiveMultiplier.ToString(provider: CultureInfo.InvariantCulture) +
                    ", enabled=" + enabled.ToString());
        }

        ShowPresetStatus(
            player: player,
            effectiveMultiplier: effectiveMultiplier);
    }

    private static void ShowPresetStatus(
        Chara player,
        int effectiveMultiplier)
    {
        string effectiveText = effectiveMultiplier.ToString(provider: CultureInfo.InvariantCulture);

        player.TalkRaw(
            text: "Turbo Actions x" + effectiveText,
            ref1: null,
            ref2: null,
            forceSync: false);
    }

    internal static void SetTurboActionsEnabled(bool enabled, bool showStatus)
    {
        bool wasEnabled = TurboActionsConfig.EnableTurboMode.Value;
        if (wasEnabled == enabled)
        {
            return;
        }

        TurboActionsConfig.EnableTurboMode.Value = enabled;

        if (wasEnabled && enabled == false)
        {
            EClass._map?.charas?.ForEach(action: chara => chara.roundTimer = 0f);
        }

        FeatureTestLog.Log(
            feature: "Toggle",
            detail: "enabled=" + enabled.ToString());

        if (showStatus == false)
        {
            return;
        }

        Chara? player = EClass.pc;
        if (player == null)
        {
            return;
        }

        string status;
        if (enabled)
        {
            status = GetLocalizedText(ja: "有効", en: "enabled", cn: "启用");
        }
        else
        {
            status = GetLocalizedText(ja: "無効", en: "disabled", cn: "禁用");
        }

        player.TalkRaw(
            text: GetLocalizedText(
                ja: "Turbo Actions " + status + "。",
                en: "Turbo Actions " + status + ".",
                cn: "Turbo Actions " + status + "。"),
            ref1: null,
            ref2: null,
            forceSync: false);
    }

    private static bool HasModOptionsPlugin()
    {
        try
        {
            foreach (object pluginObject in ModManager.ListPluginObject)
            {
                if (pluginObject is not BaseUnityPlugin plugin)
                {
                    continue;
                }

                if (plugin.Info.Metadata.GUID == ModInfo.ModOptionsGuid)
                {
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            LogError(message: "Failed while checking for the optional Mod Options plugin. " + ex);
            return false;
        }
    }

    private static string GetLocalizedText(string ja = "", string en = "", string cn = "")
    {
        if (Lang.langCode == "JP")
        {
            return ja ?? en;
        }

        if (Lang.langCode == "CN")
        {
            return cn ?? en;
        }

        return en;
    }

    internal static void LogDebug(object message, [CallerMemberName] string caller = "")
    {
        Instance?.Logger.LogDebug($"[{caller}] {message}");
    }

    internal static void LogInfo(object message)
    {
        Instance?.Logger.LogInfo(message);
    }

    internal static void LogError(object message)
    {
        Instance?.Logger.LogError(message);
    }

    internal static int GetEffectiveTurboModeSpeedMultiplier(out int configuredMultiplier)
    {
        configuredMultiplier = TurboActionsConfig.TurboModeSpeedMultiplier.Value;
        if (configuredMultiplier > 0)
        {
            lastLoggedInvalidTurboModeSpeedMultiplier = null;
            return configuredMultiplier;
        }

        LogInvalidTurboModeSpeedMultiplierIfNeeded(configuredMultiplier: configuredMultiplier);
        return 1;
    }

    private static void LogInvalidTurboModeSpeedMultiplierIfNeeded(int configuredMultiplier)
    {
        if (configuredMultiplier > 0)
        {
            lastLoggedInvalidTurboModeSpeedMultiplier = null;
            return;
        }

        if (lastLoggedInvalidTurboModeSpeedMultiplier == configuredMultiplier)
        {
            return;
        }

        lastLoggedInvalidTurboModeSpeedMultiplier = configuredMultiplier;
        FeatureTestLog.Log(
            feature: "InvalidMultiplierFallback",
            detail: "configuredMultiplier=" + configuredMultiplier.ToString() +
                ", effectiveMultiplier=1");
    }
}
