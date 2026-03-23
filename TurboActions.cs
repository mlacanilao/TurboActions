using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace TurboActions;

internal static class ModInfo
{
    internal const string Guid = "omegaplatinum.elin.turboactions";
    internal const string Name = "Turbo Actions";
    internal const string Version = "1.2.0";
}

[BepInPlugin(GUID: ModInfo.Guid, Name: ModInfo.Name, Version: ModInfo.Version)]
internal class TurboActions : BaseUnityPlugin
{
    private static int? lastLoggedInvalidTurboModeSpeedMultiplier;

    internal static TurboActions? Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        TurboActionsConfig.LoadConfig(config: Config);
        LogInvalidTurboModeSpeedMultiplierIfNeeded();
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), ModInfo.Guid);
    }

    private void Update()
    {
        if (Input.GetKeyDown(key: TurboActionsConfig.ToggleTurboKey.Value))
        {
            if (EClass.core?.IsGameStarted != true)
            {
                return;
            }

            if (EInput.isInputFieldActive)
            {
                return;
            }

            TurboActionsConfig.EnableTurboMode.Value = !TurboActionsConfig.EnableTurboMode.Value;

            if (TurboActionsConfig.EnableTurboMode?.Value == false)
            {
                ActionMode.Adv.SetTurbo(mtp: -1);
                // Flush accumulated turn timer backlog when disabling turbo.
                EClass._map?.charas?.ForEach(action: chara => chara.roundTimer = 0f);
            }

            string status = TurboActionsConfig.EnableTurboMode != null && TurboActionsConfig.EnableTurboMode.Value
                ? GetLocalizedText(ja: "有効", en: "enabled", cn: "启用")
                : GetLocalizedText(ja: "無効", en: "disabled", cn: "禁用");

            ELayer.pc.TalkRaw(
                text: GetLocalizedText(
                    ja: $"Turbo Actions {status}。",
                    en: $"Turbo Actions {status}.",
                    cn: $"Turbo Actions {status}。"),
                ref1: null,
                ref2: null,
                forceSync: false);
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

    internal static int GetEffectiveTurboModeSpeedMultiplier()
    {
        int configuredMultiplier = TurboActionsConfig.TurboModeSpeedMultiplier.Value;
        if (configuredMultiplier > 0)
        {
            lastLoggedInvalidTurboModeSpeedMultiplier = null;
            return configuredMultiplier;
        }

        LogInvalidTurboModeSpeedMultiplierIfNeeded();
        return 1;
    }

    private static void LogInvalidTurboModeSpeedMultiplierIfNeeded()
    {
        int configuredMultiplier = TurboActionsConfig.TurboModeSpeedMultiplier.Value;
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
        LogInfo(
            $"Turbo Actions Speed Multiplier is {configuredMultiplier}. " +
            "Using 1 instead because the multiplier must be greater than 0.");
    }
}

[HarmonyPatch(declaringType: typeof(AIAct))]
internal static class TurboActionsPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(methodName: nameof(AIAct.Start))]
    public static void StartPostfix(AIAct __instance)
    {
        if (TurboActionsConfig.EnableTurboMode?.Value == false)
        {
            return;
        }

        if (__instance?.owner?.IsPC != true)
        {
            return;
        }

        if (TurboActionsConfig.EnableTurboMove?.Value == false &&
            (__instance is GoalManualMove || __instance is AI_Goto))
        {
            return;
        }

        int turboModeSpeedMultiplier = TurboActions.GetEffectiveTurboModeSpeedMultiplier();
        if (AM_Adv.turbo != turboModeSpeedMultiplier)
        {
            ActionMode.Adv.SetTurbo(mtp: turboModeSpeedMultiplier);
        }
    }
}
