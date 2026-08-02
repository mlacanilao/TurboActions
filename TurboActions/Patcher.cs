using System.Collections.Generic;
using HarmonyLib;
using TurboActions.Patches;

namespace TurboActions;

internal static class Patcher
{
    [HarmonyTranspiler]
    [HarmonyPatch(
        declaringType: typeof(AM_Adv),
        methodName: nameof(AM_Adv.gameSpeed),
        methodType: MethodType.Getter)]
    internal static IEnumerable<CodeInstruction> AMAdvGameSpeedTranspiler(
        IEnumerable<CodeInstruction> instructions)
    {
        return AMAdvPatch.GameSpeedTranspiler(instructions: instructions);
    }
}
