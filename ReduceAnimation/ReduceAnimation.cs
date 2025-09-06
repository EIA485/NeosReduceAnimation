using BepInEx;
using BepInEx.NET.Common;
using BepInExResoniteShim;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using System.Reflection;

namespace ReduceAnimation
{
    [ResonitePlugin(PluginMetadata.GUID, PluginMetadata.NAME, PluginMetadata.VERSION, PluginMetadata.AUTHORS, PluginMetadata.REPOSITORY_URL)]
    [BepInDependency(BepInExResoniteShim.PluginMetadata.GUID)]
    public class ReduceAnimation : BasePlugin
    {
        static MethodInfo PositionScreen = AccessTools.Method(typeof(RadiantDash), "PositionScreen");
        public override void Load() => HarmonyInstance.PatchAll();

        [HarmonyPatch]
        class ReduceAnimationPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(RadiantDash), "OnDashSettingsUpdated")]
            static void RadiantDashOnDashSettingsUpdatedPostfix(RadiantDash __instance) => __instance.RunSynchronously(()=>__instance.AnimationSpeed.Value = float.MaxValue);

            [HarmonyPrefix]
            [HarmonyPatch(typeof(SlideSwapRegion), nameof(SlideSwapRegion.Swap))]
            static void SlideSwapRegionSwapPrefix(ref float duration) => duration = 0;

            [HarmonyPrefix]
            [HarmonyPatch(typeof(LegacySwapCanvasPanel), nameof(LegacySwapCanvasPanel.SwapPanel))]
            static void NeosSwapCanvasPanelSwapPanelPrefix(ref float duration) => duration = 0;

            [HarmonyPostfix]
            [HarmonyPatch(typeof(RadiantDash), "OnCommonUpdate")]
            static void ModalOverlayOnAwakePostfix(RadiantDash __instance, ref float ____screenLerp, ref RadiantDashScreen ____previousScreen, RadiantDashScreen ____currentScreen)
            {
                if (____previousScreen != null)
                {
                    ____screenLerp = 0f;
                    ____previousScreen.Hide();
                    ____previousScreen = null;
                }
                if (____currentScreen != null)
                {   //saves 1 frame of time
                    PositionScreen.Invoke(__instance, new object[] { ____currentScreen, 0f, 0f });
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ModalOverlay), "OnAwake")]
            static void ModalOverlayOnAwakePostfix(ModalOverlay __instance) => __instance.AnimationTime.Value = 0f;

            [HarmonyPostfix]
            [HarmonyPatch(typeof(LegacyWorldDetail), "OnChanges")]
            static void WorldDetailOnChangesPostfix(LegacyWorldDetail __instance, ref float ____expandLerp, ref float ____compactDetailLerp)
            {
                ____expandLerp = (__instance.Expanded.Value ? 1f : 0f);
                ____compactDetailLerp = (__instance.CompactDetailExpanded.Value ? 1f : 0f);
            }
        }
    }
}