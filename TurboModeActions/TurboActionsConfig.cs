using BepInEx.Configuration;
using UnityEngine;

namespace TurboActions
{
    internal static class TurboActionsConfig
    {
        internal static ConfigEntry<bool> EnableTurboMode;
        internal static ConfigEntry<int> TurboModeSpeedMultiplier;
        internal static ConfigEntry<KeyCode> ToggleTurboKey;

        internal static void LoadConfig(ConfigFile config)
        {
            EnableTurboMode = config.Bind(
                section: ModInfo.Name,
                key: "Enable Turbo Actions Mod",
                defaultValue: true,
                description: "Enable or disable the Turbo Actions mod.\n" +
                             "Set to 'true' to activate the mod, or 'false' to keep the game unchanged.\n" +
                             "ターボアクションMODを有効または無効にします。\n" +
                             "'true' に設定するとMODが有効になり、'false' に設定するとゲームのデフォルトのままになります。");

            TurboModeSpeedMultiplier = config.Bind(
                section: ModInfo.Name,
                key: "Turbo Actions Speed Multiplier",
                defaultValue: 2,
                description: "Set the speed multiplier for Turbo Actions.\n" +
                             "Must be an integer value greater than 0 (e.g., 2 for double speed).\n" +
                             "ターボアクションのスピード倍率を設定します。\n" +
                             "0より大きい整数値である必要があります（例: 倍速の場合は2）。");

            ToggleTurboKey = config.Bind(
                section: ModInfo.Name,
                key: "Turbo Actions Toggle Key",
                defaultValue: KeyCode.T,
                description: "Key to toggle Turbo Actions on or off in-game.\n" +
                             "Press this key to enable or disable Turbo Actions during gameplay.\n" +
                             "ゲーム内でターボアクションをオンまたはオフに切り替えるキーを設定します。\n" +
                             "このキーを押して、ゲームプレイ中にターボアクションを有効または無効にします。");
        }
    }
}