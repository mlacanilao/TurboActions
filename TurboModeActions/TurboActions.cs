﻿using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace TurboActions
{
    internal static class ModInfo
    {
        internal const string Guid = "omegaplatinum.elin.turboactions";
        internal const string Name = "Turbo Actions";
        internal const string Version = "1.1.4.3";
    }

    [BepInPlugin(GUID: ModInfo.Guid, Name: ModInfo.Name, Version: ModInfo.Version)]
    internal partial class TurboActions : BaseUnityPlugin
    {
        internal static TurboActions Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            TurboActionsConfig.LoadConfig(config: Config);
            var harmony = new Harmony(id: ModInfo.Guid);
            harmony.PatchAll();
        }

        private void Update()
        {
            if (Input.GetKeyDown(key: TurboActionsConfig.ToggleTurboKey.Value))
            {
                if (EClass.core?.IsGameStarted == false)
                {
                    return;
                }
                
                TurboActionsConfig.EnableTurboMode.Value = !TurboActionsConfig.EnableTurboMode.Value;

                if (TurboActionsConfig.EnableTurboMode?.Value == false)
                {
                    ActionMode.Adv.SetTurbo(mtp: -1);
                    EClass._map?.charas?.ForEach(action: chara => chara.roundTimer = 0f);
                }

                string status = TurboActionsConfig.EnableTurboMode != null && TurboActionsConfig.EnableTurboMode.Value
                    ? __(ja: "有効", en: "enabled", cn: "启用")
                    : __(ja: "無効", en: "disabled", cn: "禁用");

                ELayer.pc.TalkRaw(
                    text: __(ja: $"Turbo Actions {status}。",
                        en: $"Turbo Actions {status}.",
                        cn: $"Turbo Actions {status}。"),
                    ref1: null,
                    ref2: null,
                    forceSync: false);
            }
        }
        
        private static string __(string ja = "", string en = "", string cn = "")
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
        
        public static void Log(object payload)
        {
            Instance.Logger.LogInfo(data: payload);
        }
    }

    [HarmonyPatch(declaringType: typeof(AIAct))]
    internal static class TurboActionsPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(methodName: nameof(AIAct.Start))]
        public static void StartPostfix(AIAct __instance)
        {
            if (TurboActionsConfig.EnableTurboMode?.Value == false) return;
            if (__instance?.owner?.IsPC == false) return;

            if (TurboActionsConfig.EnableTurboMove?.Value == false &&
                (__instance is GoalManualMove || __instance is AI_Goto)) return;

            if (AM_Adv.turbo != TurboActionsConfig.TurboModeSpeedMultiplier.Value)
            {
                ActionMode.Adv.SetTurbo(mtp: TurboActionsConfig.TurboModeSpeedMultiplier.Value);
            }
        }
    }
}