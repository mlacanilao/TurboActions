using BepInEx.Configuration;
using UnityEngine;

namespace TurboActions;

internal static class TurboActionsConfig
{
    internal static ConfigEntry<bool> EnableTurboMode = null!;
    internal static ConfigEntry<int> TurboModeSpeedMultiplier = null!;
    internal static ConfigEntry<KeyCode> ToggleTurboKey = null!;
    internal static ConfigEntry<bool> EnableTurboMove = null!;
    internal static ConfigEntry<KeyboardShortcut> GameSpeedTurbo1Shortcut = null!;
    internal static ConfigEntry<int> GameSpeedTurbo1Multiplier = null!;
    internal static ConfigEntry<KeyboardShortcut> GameSpeedTurbo2Shortcut = null!;
    internal static ConfigEntry<int> GameSpeedTurbo2Multiplier = null!;
    internal static ConfigEntry<KeyboardShortcut> GameSpeedTurbo3Shortcut = null!;
    internal static ConfigEntry<int> GameSpeedTurbo3Multiplier = null!;

    internal static void LoadConfig(ConfigFile config)
    {
        EnableTurboMode = config.Bind(
            section: ModInfo.Name,
            key: "Enable Turbo Actions Mod",
            defaultValue: true,
            description: "Turn Turbo Actions on or off. When off, Elin uses its current game speed and normal turbo behavior.\n" +
                         "Turbo Actionsをオンまたはオフにします。オフの場合、Elin本体のゲームスピードと通常のターボ動作が使われます。\n" +
                         "开启或关闭 Turbo Actions。关闭时，Elin 会使用当前选择的游戏速度和原版加速效果。");

        TurboModeSpeedMultiplier = config.Bind(
            section: ModInfo.Name,
            key: "Turbo Actions Speed Multiplier",
            defaultValue: 2,
            description: "Set the Turbo Actions base multiplier used while Turbo Actions is on.\n" +
                         "Enter a whole number greater than 0. Values of 0 or less use x1; Elin's turbo effects can multiply it further.\n" +
                         "Turbo Actionsがオンのときに使うTurbo Actions基本倍率を設定します。\n" +
                         "0より大きい整数を入力してください。0以下の値は1倍速として扱われ、Elin本体のターボ効果が有効な場合はさらに倍率がかかります。\n" +
                         "设置 Turbo Actions 开启时使用的 Turbo Actions 基础倍率。\n" +
                         "请输入大于 0 的整数。0 或负数会按 1 倍速处理；Elin 原版的加速效果还会在此基础上继续相乘。");

        EnableTurboMove = config.Bind(
            section: ModInfo.Name,
            key: "Enable Turbo Movement",
            defaultValue: false,
            description: "Choose whether the Turbo Actions base multiplier also applies while moving.\n" +
                         "When off, movement uses Elin's selected base speed; Shift, autorun, and long-distance movement turbo still work normally.\n" +
                         "移動中にもTurbo Actions基本倍率を適用するか選びます。\n" +
                         "オフの場合、移動にはElin本体で選択中の基本速度が使われ、Shiftキー、自動走行、長距離移動によるターボは通常どおり動作します。\n" +
                         "选择移动时是否也应用 Turbo Actions 基础倍率。\n" +
                         "关闭时，移动会使用 Elin 当前选择的基础速度；Shift、自动奔跑和长距离移动加速仍会正常生效。"
        );

        ToggleTurboKey = config.Bind(
            section: ModInfo.Name,
            key: "Turbo Actions Toggle Key",
            defaultValue: KeyCode.T,
            description: "Choose the key used to turn Turbo Actions on or off during gameplay.\n" +
                         "ゲームプレイ中にTurbo Actionsをオンまたはオフにするキーを設定します。\n" +
                         "设置在游戏过程中开启或关闭 Turbo Actions 的按键。");

        GameSpeedTurbo1Shortcut = config.Bind(
            section: ModInfo.Name,
            key: "Game Speed Turbo 1 Shortcut",
            defaultValue: new KeyboardShortcut(mainKey: KeyCode.Alpha1, modifiers: KeyCode.LeftControl),
            description: GetPresetShortcutDescription(
                presetNumber: 1,
                defaultMultiplier: 1));

        GameSpeedTurbo1Multiplier = config.Bind(
            section: ModInfo.Name,
            key: "Game Speed Turbo 1 Multiplier",
            defaultValue: 1,
            description: GetPresetMultiplierDescription(
                presetNumber: 1,
                defaultMultiplier: 1));

        GameSpeedTurbo2Shortcut = config.Bind(
            section: ModInfo.Name,
            key: "Game Speed Turbo 2 Shortcut",
            defaultValue: new KeyboardShortcut(mainKey: KeyCode.Alpha2, modifiers: KeyCode.LeftControl),
            description: GetPresetShortcutDescription(
                presetNumber: 2,
                defaultMultiplier: 2));

        GameSpeedTurbo2Multiplier = config.Bind(
            section: ModInfo.Name,
            key: "Game Speed Turbo 2 Multiplier",
            defaultValue: 2,
            description: GetPresetMultiplierDescription(
                presetNumber: 2,
                defaultMultiplier: 2));

        GameSpeedTurbo3Shortcut = config.Bind(
            section: ModInfo.Name,
            key: "Game Speed Turbo 3 Shortcut",
            defaultValue: new KeyboardShortcut(mainKey: KeyCode.Alpha3, modifiers: KeyCode.LeftControl),
            description: GetPresetShortcutDescription(
                presetNumber: 3,
                defaultMultiplier: 3));

        GameSpeedTurbo3Multiplier = config.Bind(
            section: ModInfo.Name,
            key: "Game Speed Turbo 3 Multiplier",
            defaultValue: 3,
            description: GetPresetMultiplierDescription(
                presetNumber: 3,
                defaultMultiplier: 3));
    }

    private static string GetPresetShortcutDescription(
        int presetNumber,
        int defaultMultiplier)
    {
        return "Choose the Game Speed Turbo " + presetNumber.ToString() + " hotkey. Default: Left Control + " + presetNumber.ToString() + ".\n" +
            "It replaces the live Turbo Actions base multiplier with this preset's value (default x" + defaultMultiplier.ToString() + ") without turning Turbo Actions on. All configured modifiers are required; extra Control, Shift, Alt, or Command keys prevent a match, while unrelated held keys are allowed. Values of 0 or less use effective x1; Elin's turbo can multiply it further. Set the main key to None to disable this preset. Do not use the standalone Turbo Actions toggle key as a preset modifier; it triggers before the main key is pressed.\n" +
            "Game Speed Turbo " + presetNumber.ToString() + "のホットキーを設定します。初期値：左Control + " + presetNumber.ToString() + "。\n" +
            "Turbo Actionsをオンにせず、使用中のTurbo Actions基本倍率をこのプリセット値（初期値：x" + defaultMultiplier.ToString() + "）に置き換えます。設定したすべての修飾キーが必要です。追加のControl、Shift、Alt、Commandキーを押していると一致しませんが、関係のないキーを押していても動作します。0以下の倍率は実効x1として扱われ、Elin本体のターボでさらに倍率がかかる場合があります。メインキーをNoneにするとこのプリセットは無効になります。Turbo Actionsの単独切り替えキーをプリセットの修飾キーに使わないでください。メインキーを押す前に切り替えが作動します。\n" +
            "设置 Game Speed Turbo " + presetNumber.ToString() + " 热键。默认：左 Control + " + presetNumber.ToString() + "。\n" +
            "此热键不会开启 Turbo Actions；它只会将当前 Turbo Actions 基础倍率替换为该预设值（默认：x" + defaultMultiplier.ToString() + "）。必须按住所有已配置的修饰键；额外按住 Control、Shift、Alt 或 Command 会导致不匹配，但无关按键可以同时按住。倍率为 0 或负数时实际按 x1 处理，Elin 原版加速还可能继续相乘。将主键设为 None 可禁用此预设。不要把 Turbo Actions 的独立开关键用作预设修饰键；它会在按下主键前先触发切换。";
    }

    private static string GetPresetMultiplierDescription(
        int presetNumber,
        int defaultMultiplier)
    {
        return "Set the stored base multiplier for Game Speed Turbo " + presetNumber.ToString() + ". Default: " + defaultMultiplier.ToString() + ". Hotkey default: Left Control + " + presetNumber.ToString() + ".\n" +
            "Pressing its hotkey replaces the live Turbo Actions base multiplier with this value without turning Turbo Actions on. Values of 0 or less use effective x1; Elin's turbo can multiply it further. Set the shortcut's main key to None to disable this preset. Do not use the standalone Turbo Actions toggle key as a preset modifier; it triggers before the main key is pressed.\n" +
            "Game Speed Turbo " + presetNumber.ToString() + "に保存する基本倍率を設定します。初期値：" + defaultMultiplier.ToString() + "。ホットキー初期値：左Control + " + presetNumber.ToString() + "。\n" +
            "ホットキーを押すとTurbo Actionsをオンにせず、使用中のTurbo Actions基本倍率をこの値に置き換えます。0以下の値は実効x1として扱われ、Elin本体のターボでさらに倍率がかかる場合があります。ショートカットのメインキーをNoneにするとこのプリセットは無効になります。Turbo Actionsの単独切り替えキーをプリセットの修飾キーに使わないでください。メインキーを押す前に切り替えが作動します。\n" +
            "设置 Game Speed Turbo " + presetNumber.ToString() + " 保存的基础倍率。默认：" + defaultMultiplier.ToString() + "。热键默认：左 Control + " + presetNumber.ToString() + "。\n" +
            "按下该热键只会将当前 Turbo Actions 基础倍率替换为此值，不会开启 Turbo Actions。值为 0 或负数时实际按 x1 处理，Elin 原版加速还可能继续相乘。将快捷键主键设为 None 可禁用此预设。不要把 Turbo Actions 的独立开关键用作预设修饰键；它会在按下主键前先触发切换。";
    }
}
