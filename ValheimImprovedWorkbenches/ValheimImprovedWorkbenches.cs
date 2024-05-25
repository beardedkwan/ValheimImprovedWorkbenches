using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ValheimImprovedWorkbenches
{
    public class PluginInfo
    {
        public const string Name = "ValheimImprovedWorkbenches";
        public const string Guid = "beardedkwan.ValheimImprovedWorkbenches";
        public const string Version = "1.0.0";
    }
    public class ValheimImprovedWorkbenchesConfig
    {
        public static ConfigEntry<bool> RequireRoof { get; set; }
        public static ConfigEntry<float> WorkbenchRange { get; set; }
        public static ConfigEntry<float> StationExtensionDistance { get; set; }
    }

    [BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
    [BepInProcess("valheim.exe")]
    public class ValheimImprovedWorkbenches : BaseUnityPlugin
    {
        void Awake()
        {
            // Initialize config
            ValheimImprovedWorkbenchesConfig.RequireRoof = Config.Bind("General", "RemoveRoofRequirement", false, "Remove the roof requirement for workbenches.");
            ValheimImprovedWorkbenchesConfig.WorkbenchRange = Config.Bind("General", "WorkbenchRange", 10f, "Range of the workbench.");
            ValheimImprovedWorkbenchesConfig.StationExtensionDistance = Config.Bind("General", "StationExtensionDistance", 5f, "How far away you can place workbench upgrades.");

            Harmony harmony = new Harmony(PluginInfo.Guid);
            harmony.PatchAll();
        }

        // remove roof requirement
        [HarmonyPatch(typeof(CraftingStation), "CheckUsable")]
        public static class WorkbenchRemoveRestrictions
        {
            private static void Prefix(ref CraftingStation __instance)
            {
                __instance.m_craftRequireRoof = ValheimImprovedWorkbenchesConfig.RequireRoof.Value;
            }
        }

        // increase upgrades distance
        [HarmonyPatch(typeof(StationExtension), "Awake")]
        public static class StationExtension_Awake_Patch
        {
            [HarmonyPrefix]
            public static void Prefix(ref float ___m_maxStationDistance)
            {
                ___m_maxStationDistance = ValheimImprovedWorkbenchesConfig.StationExtensionDistance.Value;
            }
        }

        // workbench range
        [HarmonyPatch(typeof(CraftingStation), "Start")]
        public static class WorkbenchRangeIncrease
        {
            private static void Prefix(ref CraftingStation __instance, ref float ___m_rangeBuild, GameObject ___m_areaMarker)
            {
                try
                {
                    float RANGE = ValheimImprovedWorkbenchesConfig.WorkbenchRange.Value;

                    ___m_rangeBuild = RANGE;
                    ___m_areaMarker.GetComponent<CircleProjector>().m_radius = ___m_rangeBuild;
                    float scaleIncrease = (RANGE - 20f) / 20f * 100f;
                    ___m_areaMarker.gameObject.transform.localScale = new Vector3(scaleIncrease / 100, 1f, scaleIncrease / 100);

                    EffectArea effectArea = __instance.GetComponentInChildren<EffectArea>();
                    if (effectArea != null && (effectArea.m_type & EffectArea.Type.PlayerBase) != 0)
                    {
                        SphereCollider collider = __instance.GetComponentInChildren<SphereCollider>();
                        if (collider != null)
                        {
                            collider.transform.localScale = Vector3.one * RANGE * 2f;
                        }
                    }
                }
                catch { }
            }
        }
    }
}
