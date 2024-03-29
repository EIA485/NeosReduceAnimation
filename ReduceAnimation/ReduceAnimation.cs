﻿using NeosModLoader;
using HarmonyLib;
using FrooxEngine;
using FrooxEngine.UIX;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;

namespace ReduceAnimation
{
    public class ReduceAnimation : NeosMod
    {
        public override string Name => "ReduceAnimation";
        public override string Author => "eia485";
        public override string Version => "1.3.0";
        public override string Link => "https://github.com/EIA485/NeosReduceAnimation";
        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony("net.eia485.ReduceAnimation");
            harmony.PatchAll();

            MethodInfo OpenCloseSpeedOnAttach = null;

            var ins = PatchProcessor.GetOriginalInstructions(AccessTools.Method(typeof(SettingsDialog), "OnAttach"));
            for (int i = 0; i < ins.Count; i++)
            {
                if (ins[i].opcode == OpCodes.Ldstr && ins[i].operand == "Settings.Dash.OpenCloseSpeed")
                {
                    for (int si = i + 1; si < ins.Count; si++)
                    {
                        if (ins[si].opcode == OpCodes.Ldftn)
                        {
                            OpenCloseSpeedOnAttach = (MethodInfo)ins[si].operand;
                            break;
                        }
                    }
                    break;
                }
            }
            if (OpenCloseSpeedOnAttach != null)
            {
                harmony.Patch(OpenCloseSpeedOnAttach, transpiler: new HarmonyMethod(AccessTools.Method(typeof(ReduceAnimation), nameof(DashTranspiler))));
            }
        }

        static IEnumerable<CodeInstruction> DashTranspiler(IEnumerable<CodeInstruction> codes)
        {
            foreach (var code in codes)
            {
                if (code.opcode == OpCodes.Ldc_R4 && (float)code.operand == 10f) code.operand = float.PositiveInfinity;
                yield return code;
            }
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

            [HarmonyPostfix]
            [HarmonyPatch(typeof(WorldDetail), "OnChanges")]
            static void WorldDetailOnChangesPostfix(WorldDetail __instance, ref float ____expandLerp, ref float ____compactDetailLerp)
            {
                ____expandLerp = (__instance.Expanded.Value ? 1f : 0f);
                ____compactDetailLerp = (__instance.CompactDetailExpanded.Value ? 1f : 0f);
            }
        }
    }
}