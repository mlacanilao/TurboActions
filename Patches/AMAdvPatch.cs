using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace TurboActions.Patches;

internal static class AMAdvPatch
{
    private enum ResolutionBranch
    {
        IndexZeroVanilla,
        ModDisabledVanilla,
        MovementExcludedVanilla,
        CustomConfigured,
        CustomInvalidFallback
    }

    private enum MovementKind
    {
        NotInspected,
        NonMovement,
        GoalManualMove,
        AIGoto
    }

    private static bool hasLastSignature;
    private static ResolutionBranch lastBranch;
    private static bool lastActiveRegion;
    private static int lastGameSpeedIndex;
    private static float lastVanillaBase;
    private static float lastSelectedBase;
    private static bool lastConfiguredMultiplierRead;
    private static int lastRawConfiguredMultiplier;
    private static float lastRawTurbo;
    private static MovementKind lastMovementKind;

    internal static IEnumerable<CodeInstruction> GameSpeedTranspiler(
        IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> instructionList = new List<CodeInstruction>(collection: instructions);

        FieldInfo? gameSpeedsField = AccessTools.Field(
            type: typeof(ActionMode),
            name: nameof(ActionMode.GameSpeeds));
        MethodInfo? gameGetter = AccessTools.PropertyGetter(
            type: typeof(EClass),
            name: nameof(EClass.game));
        FieldInfo? gameSpeedIndexField = AccessTools.Field(
            type: typeof(Game),
            name: nameof(Game.gameSpeedIndex));
        FieldInfo? turboField = AccessTools.Field(
            type: typeof(AM_Adv),
            name: nameof(AM_Adv.turbo));
        MethodInfo? resolveBaseSpeed = AccessTools.Method(
            type: typeof(AMAdvPatch),
            name: nameof(ResolveBaseSpeed),
            parameters: new[] { typeof(float[]), typeof(int) });

        if (gameSpeedsField == null ||
            gameGetter == null ||
            gameSpeedIndexField == null ||
            turboField == null ||
            resolveBaseSpeed == null)
        {
            TurboActions.LogError(
                message: "AM_Adv.gameSpeed transpiler could not resolve its required members. " +
                    "No Turbo Actions replacement was applied; the incoming instruction stream is unchanged.");
            return instructionList;
        }

        int matchCount = CountBaseSpeedLookups(
            instructions: instructionList,
            gameSpeedsField: gameSpeedsField,
            gameGetter: gameGetter,
            gameSpeedIndexField: gameSpeedIndexField,
            turboField: turboField);

        if (matchCount != 1)
        {
            TurboActions.LogError(
                message: "AM_Adv.gameSpeed transpiler expected 1 base-speed lookup but found " +
                    matchCount.ToString() +
                    ". No Turbo Actions replacement was applied; the incoming instruction stream is unchanged.");
            return instructionList;
        }

        CodeMatcher codeMatcher = new CodeMatcher(instructions: instructionList);
        codeMatcher.MatchStartForward(matches: new[]
        {
            new CodeMatch(opcode: OpCodes.Ldsfld, operand: gameSpeedsField),
            new CodeMatch(opcode: OpCodes.Call, operand: gameGetter),
            new CodeMatch(opcode: OpCodes.Ldfld, operand: gameSpeedIndexField),
            new CodeMatch(opcode: OpCodes.Ldelem_R4),
            new CodeMatch(opcode: OpCodes.Ldsfld, operand: turboField)
        });

        if (codeMatcher.IsValid == false)
        {
            TurboActions.LogError(
                message: "AM_Adv.gameSpeed transpiler could not replace the matched base-speed lookup. " +
                    "No Turbo Actions replacement was applied; the incoming instruction stream is unchanged.");
            return codeMatcher.Instructions();
        }

        codeMatcher.Advance(offset: 3);
        codeMatcher.Set(opcode: OpCodes.Call, operand: resolveBaseSpeed);

        FeatureTestLog.Log(
            feature: "PatchDiscovery",
            detail: "target=AM_Adv.gameSpeed, matchCount=" + matchCount.ToString() +
                ", replaced=ldelem.r4 -> AMAdvPatch.ResolveBaseSpeed");

        return codeMatcher.Instructions();
    }

    private static float ResolveBaseSpeed(float[] speeds, int index)
    {
        float vanillaBase = speeds[index];
        float selectedBase = vanillaBase;
        bool configuredMultiplierRead = false;
        int rawConfiguredMultiplier = 0;
        MovementKind movementKind = MovementKind.NotInspected;
        ResolutionBranch branch;

        if (index == 0)
        {
            branch = ResolutionBranch.IndexZeroVanilla;
        }
        else
        {
            bool enableTurboMode = TurboActionsConfig.EnableTurboMode?.Value ?? false;
            if (enableTurboMode == false)
            {
                branch = ResolutionBranch.ModDisabledVanilla;
            }
            else
            {
                bool movementExcluded = false;
                bool enableTurboMove = TurboActionsConfig.EnableTurboMove?.Value ?? false;
                if (enableTurboMove == false)
                {
                    AIAct? currentAction = EClass.pc?.ai?.Current;
                    movementKind = GetMovementKind(currentAction: currentAction);
                    movementExcluded = movementKind == MovementKind.GoalManualMove ||
                        movementKind == MovementKind.AIGoto;
                }

                if (movementExcluded)
                {
                    branch = ResolutionBranch.MovementExcludedVanilla;
                }
                else
                {
                    configuredMultiplierRead = true;
                    int effectiveMultiplier = TurboActions.GetEffectiveTurboModeSpeedMultiplier(
                        configuredMultiplier: out rawConfiguredMultiplier);
                    selectedBase = (float)effectiveMultiplier;

                    if (rawConfiguredMultiplier > 0)
                    {
                        branch = ResolutionBranch.CustomConfigured;
                    }
                    else
                    {
                        branch = ResolutionBranch.CustomInvalidFallback;
                    }
                }
            }
        }

        LogResolutionIfChanged(
            branch: branch,
            index: index,
            vanillaBase: vanillaBase,
            selectedBase: selectedBase,
            configuredMultiplierRead: configuredMultiplierRead,
            rawConfiguredMultiplier: rawConfiguredMultiplier,
            movementKind: movementKind);

        return selectedBase;
    }

    private static MovementKind GetMovementKind(AIAct? currentAction)
    {
        if (currentAction is GoalManualMove)
        {
            return MovementKind.GoalManualMove;
        }

        if (currentAction is AI_Goto)
        {
            return MovementKind.AIGoto;
        }

        return MovementKind.NonMovement;
    }

    private static void LogResolutionIfChanged(
        ResolutionBranch branch,
        int index,
        float vanillaBase,
        float selectedBase,
        bool configuredMultiplierRead,
        int rawConfiguredMultiplier,
        MovementKind movementKind)
    {
        bool activeRegion = EClass.scene?.actionMode is AM_Region;
        float rawTurbo = AM_Adv.turbo;

        if (hasLastSignature &&
            lastBranch == branch &&
            lastActiveRegion == activeRegion &&
            lastGameSpeedIndex == index &&
            lastVanillaBase.Equals(obj: vanillaBase) &&
            lastSelectedBase.Equals(obj: selectedBase) &&
            lastConfiguredMultiplierRead == configuredMultiplierRead &&
            lastRawConfiguredMultiplier == rawConfiguredMultiplier &&
            lastRawTurbo.Equals(obj: rawTurbo) &&
            lastMovementKind == movementKind)
        {
            return;
        }

        hasLastSignature = true;
        lastBranch = branch;
        lastActiveRegion = activeRegion;
        lastGameSpeedIndex = index;
        lastVanillaBase = vanillaBase;
        lastSelectedBase = selectedBase;
        lastConfiguredMultiplierRead = configuredMultiplierRead;
        lastRawConfiguredMultiplier = rawConfiguredMultiplier;
        lastRawTurbo = rawTurbo;
        lastMovementKind = movementKind;

        float normalizedTurbo = rawTurbo;
        if (normalizedTurbo == 0f)
        {
            normalizedTurbo = 1f;
        }

        float expectedFinalProduct = selectedBase * normalizedTurbo;
        FeatureTestLog.Log(
            feature: "GameSpeedResolution",
            detail: "branch=" + GetBranchName(branch: branch) +
                ", activeRegion=" + activeRegion.ToString() +
                ", index=" + index.ToString() +
                ", vanillaBase=" + FeatureTestLog.FormatFloat(value: vanillaBase) +
                ", selectedBase=" + FeatureTestLog.FormatFloat(value: selectedBase) +
                ", configuredMultiplierRead=" + configuredMultiplierRead.ToString() +
                ", rawConfiguredMultiplier=" + rawConfiguredMultiplier.ToString() +
                ", rawTurbo=" + FeatureTestLog.FormatFloat(value: rawTurbo) +
                ", normalizedTurbo=" + FeatureTestLog.FormatFloat(value: normalizedTurbo) +
                ", expectedFinalProduct=" + FeatureTestLog.FormatFloat(value: expectedFinalProduct) +
                ", movementKind=" + GetMovementKindName(movementKind: movementKind));
    }

    private static string GetBranchName(ResolutionBranch branch)
    {
        switch (branch)
        {
            case ResolutionBranch.IndexZeroVanilla:
                return "index-zero-vanilla";
            case ResolutionBranch.ModDisabledVanilla:
                return "mod-disabled-vanilla";
            case ResolutionBranch.MovementExcludedVanilla:
                return "movement-excluded-vanilla";
            case ResolutionBranch.CustomConfigured:
                return "custom-configured";
            case ResolutionBranch.CustomInvalidFallback:
                return "custom-invalid-fallback";
            default:
                return "unknown";
        }
    }

    private static string GetMovementKindName(MovementKind movementKind)
    {
        switch (movementKind)
        {
            case MovementKind.NotInspected:
                return "NotInspected";
            case MovementKind.NonMovement:
                return "NonMovement";
            case MovementKind.GoalManualMove:
                return "GoalManualMove";
            case MovementKind.AIGoto:
                return "AIGoto";
            default:
                return "Unknown";
        }
    }

    private static int CountBaseSpeedLookups(
        List<CodeInstruction> instructions,
        FieldInfo gameSpeedsField,
        MethodInfo gameGetter,
        FieldInfo gameSpeedIndexField,
        FieldInfo turboField)
    {
        int count = 0;

        for (int i = 0; i < instructions.Count; i++)
        {
            if (IsBaseSpeedLookupStart(
                instructions: instructions,
                startIndex: i,
                gameSpeedsField: gameSpeedsField,
                gameGetter: gameGetter,
                gameSpeedIndexField: gameSpeedIndexField,
                turboField: turboField))
            {
                count++;
            }
        }

        return count;
    }

    private static bool IsBaseSpeedLookupStart(
        List<CodeInstruction> instructions,
        int startIndex,
        FieldInfo gameSpeedsField,
        MethodInfo gameGetter,
        FieldInfo gameSpeedIndexField,
        FieldInfo turboField)
    {
        if (startIndex + 4 >= instructions.Count)
        {
            return false;
        }

        return instructions[index: startIndex].opcode == OpCodes.Ldsfld &&
            Equals(objA: instructions[index: startIndex].operand, objB: gameSpeedsField) &&
            instructions[index: startIndex + 1].opcode == OpCodes.Call &&
            Equals(objA: instructions[index: startIndex + 1].operand, objB: gameGetter) &&
            instructions[index: startIndex + 2].opcode == OpCodes.Ldfld &&
            Equals(objA: instructions[index: startIndex + 2].operand, objB: gameSpeedIndexField) &&
            instructions[index: startIndex + 3].opcode == OpCodes.Ldelem_R4 &&
            instructions[index: startIndex + 4].opcode == OpCodes.Ldsfld &&
            Equals(objA: instructions[index: startIndex + 4].operand, objB: turboField);
    }
}
