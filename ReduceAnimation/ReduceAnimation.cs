using NeosModLoader;
using HarmonyLib;
using FrooxEngine;
using FrooxEngine.UIX;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ReduceAnimation
{
    public class ReduceAnimation : NeosMod
    {
        public override string Name => "ReduceAnimation";
        public override string Author => "eia485";
        public override string Version => "1.1.0";
        public override string Link => "https://github.com/EIA485/NeosReduceAnimation";
        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony("net.eia485.ReduceAnimation");
            harmony.PatchAll();
        }

        [HarmonyPatch]
        class ReduceAnimationPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(SlideSwapRegion), "Swap")]
            static void SlideSwapRegionSwapPrefix(ref float duration)
            {
                duration = 0;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(NeosSwapCanvasPanel), "SwapPanel")]
            static void NeosSwapCanvasPanelSwapPanelPrefix(ref float duration)
            {
                duration = 0;
            }

            [HarmonyTranspiler]
            [HarmonyPatch(typeof(RadiantDash), "OnCommonUpdate")]
            static IEnumerable<CodeInstruction> RadiantDash_OnCommonUpdateTranspiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count - 4; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldarg_0 & codes[i + 1].opcode == OpCodes.Ldc_R4 & codes[i + 2].opcode == OpCodes.Stfld & codes[i + 3].opcode == OpCodes.Br_S)
                    {
                        if ((float)codes[i + 1].operand == 1f)
                        {
                            codes[i + 1].operand = 0f;
                            break;
                        }
                    }
                }
                return codes;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ModalOverlay), "OnAwake")]
            static void ModalOverlayOnAwakePostfix(ModalOverlay __instance)
            {
                __instance.AnimationTime.Value = 0f;
            }

        }
    }
}