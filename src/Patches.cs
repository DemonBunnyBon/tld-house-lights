using System;
using Il2Cpp;
using HarmonyLib;
using UnityEngine;
using Il2CppTLD.ModularElectrolizer;
using MelonLoader;

namespace HouseLights
{
    class Patches
    {
        [HarmonyPatch(typeof(GameManager), "InstantiatePlayerObject")]
        internal class GameManager_InstantiatePlayerObject
        {
            public static void Prefix()
            {
                if (!InterfaceManager.IsMainMenuEnabled() && (!GameManager.IsOutDoorsScene(GameManager.m_ActiveScene) || Settings.options.enableOutside || HouseLights.notReallyOutdoors.Contains(GameManager.m_ActiveScene)))
                {
                    if (Settings.options.Debug)
                    {
                        MelonLogger.Msg("Scene Init");
                    }

                    HouseLights.InstantiateCustomSwitches(GameManager.m_ActiveScene);
                    HouseLights.Init();
                    HouseLights.GetSwitches();


                }
            }
        }

        [HarmonyPatch(typeof(AuroraModularElectrolizer), "Initialize")]
        internal class AuroraElectrolizer_Initialize
        {
            private static void Postfix(AuroraModularElectrolizer __instance)
            {
                if (InterfaceManager.IsMainMenuEnabled() || (GameManager.IsOutDoorsScene(GameManager.m_ActiveScene) && !HouseLights.notReallyOutdoors.Contains(GameManager.m_ActiveScene) && !Settings.options.enableOutside))
                {
                    return;
                    
                }

                AuroraActivatedToggle[] radios = __instance.gameObject.GetComponentsInParent<AuroraActivatedToggle>();
                AuroraScreenDisplay[] screens = __instance.gameObject.GetComponentsInChildren<AuroraScreenDisplay>();

                if (radios.Length == 0 && screens.Length == 0)
                {
                    HouseLights.AddElectrolizer(__instance);
                }

                __instance.m_HasFlickerSet = !Settings.options.disableAuroraFlicker;


            }
        }

        [HarmonyPatch(typeof(AuroraManager), "RegisterAuroraLightSimple", new Type[] { typeof(AuroraLightingSimple) })]
        internal class AuroraManager_RegisterLightSimple
        {
            private static void Postfix(AuroraManager __instance, AuroraLightingSimple auroraLightSimple)
            {
                if (InterfaceManager.IsMainMenuEnabled() || (GameManager.IsOutDoorsScene(GameManager.m_ActiveScene) && !HouseLights.notReallyOutdoors.Contains(GameManager.m_ActiveScene) && !Settings.options.enableOutside))
                {
                    return;
                }

                HouseLights.AddElectrolizerLight(auroraLightSimple);
            }
        }

        [HarmonyPatch(typeof(AuroraManager), "UpdateForceAurora")]
        internal class AuroraManager_UpdateForceAurora
        {
            private static void Postfix(AuroraManager __instance)
            {
                if (InterfaceManager.IsMainMenuEnabled() || (GameManager.IsOutDoorsScene(GameManager.m_ActiveScene) && !HouseLights.notReallyOutdoors.Contains(GameManager.m_ActiveScene) && !Settings.options.enableOutside))
                {
                    return;
                }

                if (HouseLights.electroSources.Count > 0)
                {
                    HouseLights.UpdateElectroLights(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.UpdateHUDText), new Type[] { typeof(Panel_HUD) })]
        internal class PlayerManage_UpdateHUDText
        {
            private static void Postfix(PlayerManager __instance, Panel_HUD hud)
            {
                if (GameManager.GetMainCamera() == null) return;
                
                GameObject interactiveObject = __instance.GetInteractiveObjectUnderCrosshairs(Settings.options.InteractDistance);
                string hoverText;

                if (interactiveObject != null && interactiveObject.name == "MOD_HouseLightSwitch")
                {
                    if (HouseLights.lightsOn)
                    {
                        hoverText = "Turn Lights Off";
                    }
                    else 
                    {
                        hoverText = "Turn Lights On";
                        
                    }

                    hud.SetHoverText(hoverText, interactiveObject, HoverTextState.CanInteract);
                }
            }
        }

        [HarmonyPatch(typeof(PlayerManager), "InteractiveObjectsProcessInteraction")]
        internal class PlayerManager_InteractiveObjectsProcessInteraction
        {
            private static void Postfix(PlayerManager __instance, ref bool __result)
            {
                GameObject interactiveObject = __instance.GetInteractiveObjectUnderCrosshairs(Settings.options.InteractDistance);

                if (interactiveObject != null && interactiveObject.name == "MOD_HouseLightSwitch")
                {
                    HouseLights.ToggleLightsState();
                    GameAudioManager.PlaySound("Stop_RadioAurora", __instance.gameObject);
                    float curScaleX = interactiveObject.transform.localScale.x;
                    float curScaleY = interactiveObject.transform.localScale.y;
                    float curScaleZ = interactiveObject.transform.localScale.z;
                    interactiveObject.transform.localScale = new(curScaleX, curScaleY * -1, curScaleZ);
                    __result = true;
                }
            }
        }


        [HarmonyPatch(typeof(Weather), "IsTooDarkForAction", new Type[] { typeof(ActionsToBlock) })]
        internal class Weather_IsTooDarkForAction
        {
            private static void Postfix(Weather __instance, ref bool __result)
            {
                if (__result && GameManager.GetWeatherComponent().IsIndoorScene() && HouseLights.lightsOn)
                {
                    __result = false;
                }
            }
        }
    }
}
