using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace TurboActions
{
    internal static class ModInfo
    {
        internal const string Guid = "omegaplatinum.elin.turboactions";
        internal const string Name = "Turbo Actions";
        internal const string Version = "1.0.0.0";
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
                TurboActionsConfig.EnableTurboMode.Value = !TurboActionsConfig.EnableTurboMode.Value;
                string status = TurboActionsConfig.EnableTurboMode.Value ? "enabled" : "disabled";
                ELayer.pc.TalkRaw(text: $"Turbo Actions {status}.", ref1: null, ref2: null, forceSync: false);
            }
        }
    }

    [HarmonyPatch(declaringType: typeof(AIAct), methodName: nameof(AIAct.Start))]
    internal static class TurboActionsPatch
    {
        [HarmonyPostfix]
        public static void Postfix(AIAct __instance)
        {
            if (TurboActionsConfig.EnableTurboMode?.Value == true)
            {
                int turboSpeed = TurboActionsConfig.TurboModeSpeedMultiplier?.Value ?? 2;
                ActionMode.Adv.SetTurbo(mtp: turboSpeed);
            }
        }
    }
}