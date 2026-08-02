using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using EvilMask.Elin.ModOptions;
using EvilMask.Elin.ModOptions.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TurboActions.UI;

internal static class UIController
{
    private const float MinimumDescriptionLabelWidth = 540f;
    private const float DescriptionLabelHorizontalPadding = 40f;
    private const string XmlFileName = "TurboActionsConfig.xml";
    private const string TranslationFileName = "translations.xlsx";

    private static readonly List<KeyCode> MainKeyOptions = Enum.GetValues(enumType: typeof(KeyCode))
        .Cast<KeyCode>()
        .Distinct()
        .ToList();

    private static readonly List<KeyCode> DefaultModifierOptions = new List<KeyCode>
    {
        KeyCode.None,
        KeyCode.LeftControl,
        KeyCode.RightControl,
        KeyCode.LeftShift,
        KeyCode.RightShift,
        KeyCode.LeftAlt,
        KeyCode.RightAlt,
        KeyCode.LeftCommand,
        KeyCode.RightCommand
    };

    private static bool hasLoggedCompleteBinding;

    internal static void RegisterUI()
    {
        string assemblyDirectory = Path.GetDirectoryName(path: Assembly.GetExecutingAssembly().Location) ?? string.Empty;
        string xmlPath = Path.Combine(path1: assemblyDirectory, path2: XmlFileName);
        string translationPath = Path.Combine(path1: assemblyDirectory, path2: TranslationFileName);

        if (File.Exists(path: xmlPath) == false)
        {
            TurboActions.LogError(message: "Mod Options XML was not found: " + xmlPath);
            return;
        }

        if (File.Exists(path: translationPath) == false)
        {
            TurboActions.LogError(message: "Mod Options translations were not found: " + translationPath);
            return;
        }

        string xml;
        try
        {
            xml = File.ReadAllText(path: xmlPath);
        }
        catch (Exception ex)
        {
            TurboActions.LogError(message: "Mod Options XML could not be read: " + xmlPath + ". " + ex);
            return;
        }

        ModOptionController? controller = ModOptionController.Register(
            guid: ModInfo.Guid,
            tooptipId: "mod.tooltip");
        if (controller == null)
        {
            TurboActions.LogError(message: "Failed to register the Mod Options controller.");
            return;
        }

        try
        {
            controller.SetTranslationsFromXslx(path: translationPath);
            if (controller.Tr(contentId: ModInfo.Guid) == ModInfo.Guid)
            {
                TurboActions.LogError(message: "Mod Options translations did not load the required Turbo Actions title row: " + translationPath);
                return;
            }

            controller.SetPreBuildWithXml(xml: xml);
        }
        catch (Exception ex)
        {
            TurboActions.LogError(message: "Mod Options assets could not be loaded; Turbo Actions gameplay remains available. " + ex);
            return;
        }

        RegisterEvents(controller: controller);
    }

    private static void RegisterEvents(ModOptionController controller)
    {
        controller.OnBuildUI += builder =>
        {
            OptLabel? turboActionsDescription = GetRequiredPreBuild<OptLabel>(builder: builder, id: "TurboActionsDescription");
            OptToggle? enableTurboModeToggle = GetRequiredPreBuild<OptToggle>(builder: builder, id: "EnableTurboModeToggle");
            OptLabel? turboModeSpeedMultiplierDescription = GetRequiredPreBuild<OptLabel>(builder: builder, id: "TurboModeSpeedMultiplierDescription");
            OptInput? turboModeSpeedMultiplierInput = GetRequiredPreBuild<OptInput>(builder: builder, id: "TurboModeSpeedMultiplierInput");
            OptToggle? enableTurboMoveToggle = GetRequiredPreBuild<OptToggle>(builder: builder, id: "EnableTurboMoveToggle");
            OptLabel? toggleTurboKeyDescription = GetRequiredPreBuild<OptLabel>(builder: builder, id: "ToggleTurboKeyDescription");
            OptDropdown? toggleTurboKeyDropdown = GetRequiredPreBuild<OptDropdown>(builder: builder, id: "ToggleTurboKeyDropdown");

            OptLabel? preset1Description = GetRequiredPreBuild<OptLabel>(builder: builder, id: "GameSpeedTurbo1Description");
            OptLabel? preset1HotkeyDescription = GetRequiredPreBuild<OptLabel>(builder: builder, id: "GameSpeedTurbo1HotkeyDescription");
            OptInput? preset1MultiplierInput = GetRequiredPreBuild<OptInput>(builder: builder, id: "GameSpeedTurbo1MultiplierInput");
            OptLabel? preset1ModifierLabel = GetRequiredPreBuild<OptLabel>(builder: builder, id: "GameSpeedTurbo1ModifierLabel");
            OptDropdown? preset1ModifierDropdown = GetRequiredPreBuild<OptDropdown>(builder: builder, id: "GameSpeedTurbo1ModifierDropdown");
            OptLabel? preset1MainKeyLabel = GetRequiredPreBuild<OptLabel>(builder: builder, id: "GameSpeedTurbo1MainKeyLabel");
            OptDropdown? preset1MainKeyDropdown = GetRequiredPreBuild<OptDropdown>(builder: builder, id: "GameSpeedTurbo1MainKeyDropdown");

            OptLabel? preset2Description = GetRequiredPreBuild<OptLabel>(builder: builder, id: "GameSpeedTurbo2Description");
            OptLabel? preset2HotkeyDescription = GetRequiredPreBuild<OptLabel>(builder: builder, id: "GameSpeedTurbo2HotkeyDescription");
            OptInput? preset2MultiplierInput = GetRequiredPreBuild<OptInput>(builder: builder, id: "GameSpeedTurbo2MultiplierInput");
            OptLabel? preset2ModifierLabel = GetRequiredPreBuild<OptLabel>(builder: builder, id: "GameSpeedTurbo2ModifierLabel");
            OptDropdown? preset2ModifierDropdown = GetRequiredPreBuild<OptDropdown>(builder: builder, id: "GameSpeedTurbo2ModifierDropdown");
            OptLabel? preset2MainKeyLabel = GetRequiredPreBuild<OptLabel>(builder: builder, id: "GameSpeedTurbo2MainKeyLabel");
            OptDropdown? preset2MainKeyDropdown = GetRequiredPreBuild<OptDropdown>(builder: builder, id: "GameSpeedTurbo2MainKeyDropdown");

            OptLabel? preset3Description = GetRequiredPreBuild<OptLabel>(builder: builder, id: "GameSpeedTurbo3Description");
            OptLabel? preset3HotkeyDescription = GetRequiredPreBuild<OptLabel>(builder: builder, id: "GameSpeedTurbo3HotkeyDescription");
            OptInput? preset3MultiplierInput = GetRequiredPreBuild<OptInput>(builder: builder, id: "GameSpeedTurbo3MultiplierInput");
            OptLabel? preset3ModifierLabel = GetRequiredPreBuild<OptLabel>(builder: builder, id: "GameSpeedTurbo3ModifierLabel");
            OptDropdown? preset3ModifierDropdown = GetRequiredPreBuild<OptDropdown>(builder: builder, id: "GameSpeedTurbo3ModifierDropdown");
            OptLabel? preset3MainKeyLabel = GetRequiredPreBuild<OptLabel>(builder: builder, id: "GameSpeedTurbo3MainKeyLabel");
            OptDropdown? preset3MainKeyDropdown = GetRequiredPreBuild<OptDropdown>(builder: builder, id: "GameSpeedTurbo3MainKeyDropdown");

            if (turboActionsDescription == null ||
                enableTurboModeToggle == null ||
                turboModeSpeedMultiplierDescription == null ||
                turboModeSpeedMultiplierInput == null ||
                enableTurboMoveToggle == null ||
                toggleTurboKeyDescription == null ||
                toggleTurboKeyDropdown == null ||
                preset1Description == null ||
                preset1HotkeyDescription == null ||
                preset1MultiplierInput == null ||
                preset1ModifierLabel == null ||
                preset1ModifierDropdown == null ||
                preset1MainKeyLabel == null ||
                preset1MainKeyDropdown == null ||
                preset2Description == null ||
                preset2HotkeyDescription == null ||
                preset2MultiplierInput == null ||
                preset2ModifierLabel == null ||
                preset2ModifierDropdown == null ||
                preset2MainKeyLabel == null ||
                preset2MainKeyDropdown == null ||
                preset3Description == null ||
                preset3HotkeyDescription == null ||
                preset3MultiplierInput == null ||
                preset3ModifierLabel == null ||
                preset3ModifierDropdown == null ||
                preset3MainKeyLabel == null ||
                preset3MainKeyDropdown == null)
            {
                return;
            }

            if (turboActionsDescription.Base == null ||
                enableTurboModeToggle.Base == null ||
                turboModeSpeedMultiplierDescription.Base == null ||
                turboModeSpeedMultiplierInput.Base == null ||
                enableTurboMoveToggle.Base == null ||
                toggleTurboKeyDescription.Base == null ||
                toggleTurboKeyDropdown.Base == null ||
                preset1Description.Base == null ||
                preset1HotkeyDescription.Base == null ||
                preset1MultiplierInput.Base == null ||
                preset1ModifierLabel.Base == null ||
                preset1ModifierDropdown.Base == null ||
                preset1MainKeyLabel.Base == null ||
                preset1MainKeyDropdown.Base == null ||
                preset2Description.Base == null ||
                preset2HotkeyDescription.Base == null ||
                preset2MultiplierInput.Base == null ||
                preset2ModifierLabel.Base == null ||
                preset2ModifierDropdown.Base == null ||
                preset2MainKeyLabel.Base == null ||
                preset2MainKeyDropdown.Base == null ||
                preset3Description.Base == null ||
                preset3HotkeyDescription.Base == null ||
                preset3MultiplierInput.Base == null ||
                preset3ModifierLabel.Base == null ||
                preset3ModifierDropdown.Base == null ||
                preset3MainKeyLabel.Base == null ||
                preset3MainKeyDropdown.Base == null)
            {
                TurboActions.LogError(message: "One or more required Mod Options controls did not provide a live UI component.");
                return;
            }

            ApplyDescriptionLabelLayout(label: turboActionsDescription);
            ApplyDescriptionLabelLayout(label: turboModeSpeedMultiplierDescription);
            ApplyDescriptionLabelLayout(label: toggleTurboKeyDescription);
            ApplyDescriptionLabelLayout(label: preset1Description);
            ApplyDescriptionLabelLayout(label: preset1HotkeyDescription);
            preset1ModifierLabel.Align = TextAnchor.UpperLeft;
            preset1MainKeyLabel.Align = TextAnchor.UpperLeft;
            ApplyDescriptionLabelLayout(label: preset2Description);
            ApplyDescriptionLabelLayout(label: preset2HotkeyDescription);
            preset2ModifierLabel.Align = TextAnchor.UpperLeft;
            preset2MainKeyLabel.Align = TextAnchor.UpperLeft;
            ApplyDescriptionLabelLayout(label: preset3Description);
            ApplyDescriptionLabelLayout(label: preset3HotkeyDescription);
            preset3ModifierLabel.Align = TextAnchor.UpperLeft;
            preset3MainKeyLabel.Align = TextAnchor.UpperLeft;

            string integerPlaceholder = controller.Tr(contentId: "config.integer.placeholder");
            turboModeSpeedMultiplierInput.Placeholder = integerPlaceholder;
            preset1MultiplierInput.Placeholder = integerPlaceholder;
            preset2MultiplierInput.Placeholder = integerPlaceholder;
            preset3MultiplierInput.Placeholder = integerPlaceholder;

            BindEnableToggle(toggle: enableTurboModeToggle);
            BindConfigToggle(toggle: enableTurboMoveToggle, configEntry: TurboActionsConfig.EnableTurboMove);
            BindIntInput(input: turboModeSpeedMultiplierInput, configEntry: TurboActionsConfig.TurboModeSpeedMultiplier);
            BindKeyCodeDropdown(dropdown: toggleTurboKeyDropdown, configEntry: TurboActionsConfig.ToggleTurboKey);

            BindIntInput(input: preset1MultiplierInput, configEntry: TurboActionsConfig.GameSpeedTurbo1Multiplier);
            BindShortcutDropdowns(
                modifierDropdown: preset1ModifierDropdown,
                mainKeyDropdown: preset1MainKeyDropdown,
                configEntry: TurboActionsConfig.GameSpeedTurbo1Shortcut);

            BindIntInput(input: preset2MultiplierInput, configEntry: TurboActionsConfig.GameSpeedTurbo2Multiplier);
            BindShortcutDropdowns(
                modifierDropdown: preset2ModifierDropdown,
                mainKeyDropdown: preset2MainKeyDropdown,
                configEntry: TurboActionsConfig.GameSpeedTurbo2Shortcut);

            BindIntInput(input: preset3MultiplierInput, configEntry: TurboActionsConfig.GameSpeedTurbo3Multiplier);
            BindShortcutDropdowns(
                modifierDropdown: preset3ModifierDropdown,
                mainKeyDropdown: preset3MainKeyDropdown,
                configEntry: TurboActionsConfig.GameSpeedTurbo3Shortcut);

            if (hasLoggedCompleteBinding == false)
            {
                hasLoggedCompleteBinding = true;
                FeatureTestLog.Log(
                    feature: "ModOptionsBinding",
                    detail: "allRequiredControlsBound=true");
            }
        };
    }

    private static void BindEnableToggle(OptToggle toggle)
    {
        toggle.Checked = TurboActionsConfig.EnableTurboMode.Value;
        toggle.OnValueChanged += value =>
        {
            TurboActions.SetTurboActionsEnabled(enabled: value, showStatus: false);
        };
    }

    private static void BindConfigToggle(OptToggle toggle, ConfigEntry<bool> configEntry)
    {
        toggle.Checked = configEntry.Value;
        toggle.OnValueChanged += value =>
        {
            configEntry.Value = value;
        };
    }

    private static void BindIntInput(OptInput input, ConfigEntry<int> configEntry)
    {
        input.ContentType = InputField.ContentType.IntegerNumber;
        input.Text = configEntry.Value.ToString(provider: CultureInfo.InvariantCulture);
        input.OnValueChanged += value =>
        {
            if (int.TryParse(
                s: value,
                style: NumberStyles.Integer,
                provider: CultureInfo.InvariantCulture,
                result: out int parsedValue) == false)
            {
                return;
            }

            configEntry.Value = parsedValue;
        };
    }

    private static void BindKeyCodeDropdown(
        OptDropdown dropdown,
        ConfigEntry<KeyCode> configEntry)
    {
        SetupDropdown(
            dropdown: dropdown,
            keyCodes: MainKeyOptions,
            selectedKey: configEntry.Value);
        dropdown.OnValueChanged += _ =>
        {
            configEntry.Value = GetKeyCodeAtIndex(
                keyCodes: MainKeyOptions,
                index: dropdown.Value);
        };
    }

    private static void BindShortcutDropdowns(
        OptDropdown modifierDropdown,
        OptDropdown mainKeyDropdown,
        ConfigEntry<KeyboardShortcut> configEntry)
    {
        List<KeyCode> modifierOptions = GetModifierOptions(shortcut: configEntry.Value);
        SetupDropdown(
            dropdown: modifierDropdown,
            keyCodes: modifierOptions,
            selectedKey: GetFirstModifier(shortcut: configEntry.Value));
        SetupDropdown(
            dropdown: mainKeyDropdown,
            keyCodes: MainKeyOptions,
            selectedKey: configEntry.Value.MainKey);

        bool isUpdating = false;
        Action updateShortcut = () =>
        {
            if (isUpdating)
            {
                return;
            }

            isUpdating = true;
            try
            {
                KeyCode mainKey = GetKeyCodeAtIndex(
                    keyCodes: MainKeyOptions,
                    index: mainKeyDropdown.Value);
                KeyCode modifier = GetKeyCodeAtIndex(
                    keyCodes: modifierOptions,
                    index: modifierDropdown.Value);

                if (mainKey == KeyCode.None)
                {
                    int noneIndex = modifierOptions.IndexOf(item: KeyCode.None);
                    if (modifierDropdown.Value != noneIndex)
                    {
                        modifierDropdown.Value = noneIndex;
                        modifierDropdown.Base.RefreshShownValue();
                    }

                    configEntry.Value = KeyboardShortcut.Empty;
                    return;
                }

                if (modifier == KeyCode.None || modifier == mainKey)
                {
                    configEntry.Value = new KeyboardShortcut(mainKey: mainKey);
                    return;
                }

                configEntry.Value = new KeyboardShortcut(
                    mainKey: mainKey,
                    modifiers: modifier);
            }
            finally
            {
                isUpdating = false;
            }
        };

        modifierDropdown.OnValueChanged += _ => updateShortcut();
        mainKeyDropdown.OnValueChanged += _ => updateShortcut();
    }

    private static List<KeyCode> GetModifierOptions(KeyboardShortcut shortcut)
    {
        List<KeyCode> modifierOptions = new List<KeyCode>(collection: DefaultModifierOptions);
        KeyCode currentModifier = GetFirstModifier(shortcut: shortcut);
        if (modifierOptions.Contains(item: currentModifier) == false)
        {
            modifierOptions.Add(item: currentModifier);
        }

        return modifierOptions;
    }

    private static void SetupDropdown(
        OptDropdown dropdown,
        List<KeyCode> keyCodes,
        KeyCode selectedKey)
    {
        dropdown.Base.options.Clear();
        foreach (KeyCode keyCode in keyCodes)
        {
            dropdown.Base.options.Add(item: new Dropdown.OptionData(text: keyCode.ToString()));
        }

        int selectedIndex = keyCodes.IndexOf(item: selectedKey);
        if (selectedIndex < 0)
        {
            selectedIndex = 0;
        }

        dropdown.Value = selectedIndex;
        dropdown.Base.RefreshShownValue();
    }

    private static KeyCode GetKeyCodeAtIndex(List<KeyCode> keyCodes, int index)
    {
        if (index < 0 || index >= keyCodes.Count)
        {
            return KeyCode.None;
        }

        return keyCodes[index: index];
    }

    private static KeyCode GetFirstModifier(KeyboardShortcut shortcut)
    {
        foreach (KeyCode modifier in shortcut.Modifiers)
        {
            return modifier;
        }

        return KeyCode.None;
    }

    private static void ApplyDescriptionLabelLayout(OptLabel label)
    {
        label.Align = TextAnchor.UpperLeft;
        if (label.Base == null ||
            label.Base.text1 == null)
        {
            return;
        }

        float width = GetAvailableDescriptionLabelWidth(transform: label.Base.text1.transform);
        ApplyLayoutWidth(rectTransform: label.Base.GetComponent<RectTransform>(), width: width);
        ApplyLayoutWidth(rectTransform: label.Base.text1.rectTransform, width: width);
    }

    private static float GetAvailableDescriptionLabelWidth(Transform transform)
    {
        Transform? current = transform;
        for (int i = 0; i < 8 && current != null; i++)
        {
            if (current.name == "Content" ||
                current.name == "Viewport")
            {
                RectTransform? rectTransform = current as RectTransform;
                if (rectTransform != null &&
                    rectTransform.rect.width > MinimumDescriptionLabelWidth)
                {
                    return rectTransform.rect.width - DescriptionLabelHorizontalPadding;
                }
            }

            current = current.parent;
        }

        return MinimumDescriptionLabelWidth;
    }

    private static void ApplyLayoutWidth(RectTransform? rectTransform, float width)
    {
        if (rectTransform == null)
        {
            return;
        }

        LayoutElement? layoutElement = rectTransform.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
        }

        layoutElement.minWidth = width;
        layoutElement.preferredWidth = width;
        layoutElement.flexibleWidth = 0f;
        rectTransform.SetSizeWithCurrentAnchors(axis: RectTransform.Axis.Horizontal, size: width);
    }

    private static T? GetRequiredPreBuild<T>(OptionUIBuilder builder, string id)
        where T : OptUIElement
    {
        T? element = builder.GetPreBuild<T>(id: id);
        if (element == null)
        {
            TurboActions.LogError(message: "Missing or incorrectly typed Mod Options prebuilt element: " + id);
        }

        return element;
    }
}
