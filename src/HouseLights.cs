using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Il2Cpp;
using MelonLoader;
using Il2CppTLD.ModularElectrolizer;
using UnityEngine.UI.Collections;
using UnityEngine.UI;
using Il2CppAK;
using static Il2Cppgw.gql.Interpreter;
using SevenZip.CommandLineParser;

namespace HouseLights
{


    public class ElectrolizerConfig : MelonMod
    {
        public AuroraModularElectrolizer electrolizer = null;
        public float[] ranges = null;
        public Color[] colors = null;
    }

    public class ElectrolizerLightConfig : MelonMod
    {
        public AuroraLightingSimple electrolizer = null;
        public float[] ranges = null;
        public Color[] colors = null;
    }

    class HouseLights : MelonMod
    {

        private static AssetBundle? assetBundle;

        internal static AssetBundle HLbundle
        {
            get => assetBundle ?? throw new System.NullReferenceException(nameof(assetBundle));
        }


        public static bool lightsOn = false;
        public static List<ElectrolizerConfig> electroSources = new List<ElectrolizerConfig>();
        public static List<ElectrolizerLightConfig> electroLightSources = new List<ElectrolizerLightConfig>();
        public static List<GameObject> orgObj = new List<GameObject>();
        public static List<GameObject> result = new List<GameObject>();
        public static List<GameObject> lightSwitches = new List<GameObject>();

        public static List<string> notReallyOutdoors = new List<string>
        {
            "DamTransitionZone"
        };

        public override void OnInitializeMelon()
        {
            Settings.OnLoad();
            assetBundle = LoadAssetBundle("HouseLights.hlbundle");
            RegisterCommands();
        }
        private static AssetBundle LoadAssetBundle(string path)
        {
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
            MemoryStream memoryStream = new MemoryStream((int)stream.Length);
            stream.CopyTo(memoryStream);

            return memoryStream.Length != 0
                ? AssetBundle.LoadFromMemory(memoryStream.ToArray())
                : throw new System.Exception("No data loaded!");
        }

        internal static void Init()
        {
            electroSources.Clear();
            lightSwitches.Clear();
            lightsOn = false;
        }

        internal static void AddElectrolizer(AuroraModularElectrolizer light)
        {
            
            ElectrolizerConfig newLight = new ElectrolizerConfig
            {
                electrolizer = light,
                ranges = new float[light.m_LocalLights._size],
                colors = new Color[light.m_LocalLights._size]
            };

            for (int i = 0; i < light.m_LocalLights._size; i++)
            {
                float curRange = light.m_LocalLights[i].range;
                Color curColor = light.m_LocalLights[i].color;
                newLight.ranges[i] = curRange;
                newLight.colors[i] = curColor;
            }

            electroSources.Add(newLight);
        }

        internal static void AddElectrolizerLight(AuroraLightingSimple light)
        {
            ElectrolizerLightConfig newLight = new ElectrolizerLightConfig
            {
                electrolizer = light,
                ranges = new float[light.m_LocalLights.Length],
                colors = new Color[light.m_LocalLights.Length]
            };

            for (int i = 0; i < light.m_LocalLights.Length; i++)
            {
                float curRange = light.m_LocalLights[i].range;
                Color curColor = light.m_LocalLights[i].color;
                newLight.ranges[i] = curRange;
                newLight.colors[i] = curColor;
            }

            electroLightSources.Add(newLight);
        }

        internal static void GetSwitches()
        {
            List<GameObject> rObjs = HouseLightsUtils.GetRootObjects();
            List<GameObject> result = new List<GameObject>();
            List<GameObject> customSwitches = new List<GameObject>();
            int wCount = 0;
            orgObj = new List<GameObject>();
            GameObject newSwitch;
            GameObject switchComponent;
            foreach (GameObject rootObj in rObjs)
            {
                HouseLightsUtils.GetChildrenWithName(rootObj, "houselightswitch", orgObj);
                HouseLightsUtils.GetChildrenWithName(rootObj, "lightswitcha", orgObj);
                HouseLightsUtils.GetChildrenWithName(rootObj, "lightswitchblack", orgObj);
                HouseLightsUtils.GetChildrenWithName(rootObj, "switch_a_black", orgObj);
                HouseLightsUtils.GetChildrenWithName(rootObj, "switch_a_white", orgObj);
                HouseLightsUtils.GetChildrenWithName(rootObj, "switch_a_purple", orgObj);
                HouseLightsUtils.GetChildrenWithName(rootObj, "switch_b_white", orgObj);
                int switchType;

                foreach (GameObject childObj in orgObj)
                {
                    if (childObj.active)
                    {
                        if (childObj.name.ToLowerInvariant().Contains("houselightswitch") || childObj.name.ToLowerInvariant().Contains("lightswitcha"))
                        {
                            switchType = 1;
                        }
                        else
                        {
                            switchType = 2;
                        }

                        childObj.active = false;
                        newSwitch = HouseLightsUtils.InstantiateSwitch(childObj.transform.position, childObj.transform.rotation.eulerAngles, switchType);
                        switchComponent = newSwitch.transform.FindChild("SM_LightSwitchBlack").gameObject;
                        result.Add(newSwitch);
                        switchComponent.layer = 19;
                        lightSwitches.Add(switchComponent);
                        switchComponent.name = "MOD_HouseLightSwitch";
                        wCount++;
                        if (!switchComponent.transform.GetComponent<Collider>())
                        {
                            BoxCollider col = switchComponent.AddComponent<BoxCollider>();
                            col.size = new(0.1f, 0.1f, 0.1f);
                        }

                    }


                }


            }
            if (Settings.options.Debug)
            {
                MelonLogger.Msg("Light switches found: " + wCount + ".");
                MelonLogger.Msg("Custom switches created: " + customSwitches.Count() + ".");
            }
        }

        internal static void ToggleLightsState()
        {
            lightsOn = !lightsOn;
        }

        internal static void UpdateElectroLights(AuroraManager mngr)
        {
            Vector3 playerPos = GameManager.GetVpFPSPlayer().gameObject.transform.position;

            for (int e = 0; e < electroSources.Count; e++)
            {
                if (electroSources[e].electrolizer != null && electroSources[e].electrolizer.m_LocalLights != null)
                {
                    float distance = Mathf.Abs(Vector3.Distance(electroSources[e].electrolizer.gameObject.transform.position, playerPos));

                    if (distance > Settings.options.cullDistance && !mngr.AuroraIsActive())
                    {
                        electroSources[e].electrolizer.UpdateIntensity(1f, 0f);
                        electroSources[e].electrolizer.UpdateLight(true);
                        electroSources[e].electrolizer.UpdateEmissiveObjects(true);
                        electroSources[e].electrolizer.UpdateAudio();
                        continue;
                    }
                    
                    for (int i = 0; i < electroSources[e].electrolizer.m_LocalLights._size; i++)
                    {
                        float cur_range = electroSources[e].ranges[i];

                        cur_range *= Settings.options.rangeMultiplier;
                        cur_range = Math.Min(cur_range, 20f);

                        electroSources[e].electrolizer.m_LocalLights[i].range = cur_range;
                        electroSources[e].electrolizer.m_HasFlickerSet = !Settings.options.disableAuroraFlicker;
                        ColorHSV curColor = electroSources[e].colors[i];

                        if (Settings.options.whiteLights)
                            curColor.s *= 0.15f;

                        electroSources[e].electrolizer.m_LocalLights[i].color = curColor;

                        if (Settings.options.castShadows)
                        {
                            electroSources[e].electrolizer.m_LocalLights[i].shadows = LightShadows.Soft;
                        }
                    }

                    if (lightsOn && !mngr.AuroraIsActive())
                    {
                        if (!electroSources[e].electrolizer.gameObject.name.Contains("Alarm") &&
                            !electroSources[e].electrolizer.gameObject.name.Contains("Headlight") &&
                            !electroSources[e].electrolizer.gameObject.name.Contains("Taillight") &&
                            !electroSources[e].electrolizer.gameObject.name.Contains("Television") &&
                            !electroSources[e].electrolizer.gameObject.name.Contains("Computer") &&
                            !electroSources[e].electrolizer.gameObject.name.Contains("Machine") &&
                            !electroSources[e].electrolizer.gameObject.name.Contains("ControlBox") &&
                            !electroSources[e].electrolizer.gameObject.name.Contains("Interiorlight"))
                        {
                            electroSources[e].electrolizer.UpdateIntensity(1f, Settings.options.intensityValue);
                            electroSources[e].electrolizer.UpdateLight(false);
                            electroSources[e].electrolizer.UpdateEmissiveObjects(false);
                            if(Settings.options.LightAudio)
                            {
                                electroSources[e].electrolizer.UpdateAudio();
                            }
                            else
                            {
                                electroSources[e].electrolizer.StopAudio();
                            }
                            

                        }
                    }
                    else if (!mngr.AuroraIsActive())
                    {
                        electroSources[e].electrolizer.UpdateIntensity(1f, 0f);
                        electroSources[e].electrolizer.UpdateLight(true);
                        electroSources[e].electrolizer.UpdateEmissiveObjects(true);
                        electroSources[e].electrolizer.UpdateAudio();
                    }
                    else
                    {
                        electroSources[e].electrolizer.UpdateIntensity(Time.deltaTime, mngr.m_NormalizedActive);
                    }
                }
            }

            for (int e = 0; e < electroLightSources.Count; e++)
            {
                if (electroLightSources[e].electrolizer != null && electroLightSources[e].electrolizer.m_LocalLights != null)
                {
                    float distance = Mathf.Abs(Vector3.Distance(electroLightSources[e].electrolizer.gameObject.transform.position, playerPos));

                    if (distance > Settings.options.cullDistance && !mngr.AuroraIsActive())
                    {
                        electroLightSources[e].electrolizer.m_CurIntensity = 0f;
                        electroLightSources[e].electrolizer.UpdateLight(true);
                        electroLightSources[e].electrolizer.UpdateEmissiveObjects(true);
                        electroLightSources[e].electrolizer.UpdateAudio();

                        continue;
                    }

                    for (int i = 0; i < electroLightSources[e].electrolizer.m_LocalLights.Length; i++)
                    {
                        float cur_range = electroLightSources[e].ranges[i];

                        cur_range *= Settings.options.rangeMultiplier;
                        cur_range = Math.Min(cur_range, 20f);

                        electroLightSources[e].electrolizer.m_LocalLights[i].range = cur_range;

                        ColorHSV curColor = electroLightSources[e].colors[i];

                        if (Settings.options.whiteLights)
                            curColor.s *= 0.15f;

                        electroLightSources[e].electrolizer.m_LocalLights[i].color = curColor;

                        if (Settings.options.castShadows)
                        {
                            electroLightSources[e].electrolizer.m_LocalLights[i].shadows = LightShadows.Soft;
                        }
                    }

                    if (lightsOn && !mngr.AuroraIsActive())
                    {
                        if (!electroLightSources[e].electrolizer.gameObject.name.Contains("Alarm") &&
                            !electroLightSources[e].electrolizer.gameObject.name.Contains("Headlight") &&
                            !electroLightSources[e].electrolizer.gameObject.name.Contains("Taillight") &&
                            !electroLightSources[e].electrolizer.gameObject.name.Contains("Television") &&
                            !electroLightSources[e].electrolizer.gameObject.name.Contains("Computer") &&
                            !electroLightSources[e].electrolizer.gameObject.name.Contains("Machine") &&
                            !electroLightSources[e].electrolizer.gameObject.name.Contains("ControlBox") &&
                            !electroLightSources[e].electrolizer.gameObject.name.Contains("Interiorlight"))
                        {
                            electroLightSources[e].electrolizer.m_CurIntensity = Settings.options.intensityValue;
                            electroLightSources[e].electrolizer.UpdateLight(false);
                            electroLightSources[e].electrolizer.UpdateEmissiveObjects(false);
                            if (Settings.options.LightAudio)
                            {
                                electroSources[e].electrolizer.UpdateAudio();
                            }
                            else
                            {
                                electroSources[e].electrolizer.StopAudio();
                            }
                        }
                    }
                    else if (!mngr.AuroraIsActive())
                    {
                        electroLightSources[e].electrolizer.m_CurIntensity = 0f;
                        electroLightSources[e].electrolizer.UpdateLight(true);
                        electroLightSources[e].electrolizer.UpdateEmissiveObjects(true);
                        electroLightSources[e].electrolizer.UpdateAudio();
                    }
                    else
                    {
                        electroLightSources[e].electrolizer.UpdateIntensity(Time.deltaTime);
                    }
                }
            }
        }

        internal static void RegisterCommands()
        {
            uConsole.RegisterCommand("thl", new Action(ToggleLightsState));
        }

        public static void InstantiateCustomSwitches(string sceneName)
        {
            switch(sceneName.ToLowerInvariant())
            {
                case "lakeregion":
                    HouseLightsUtils.InstantiateSwitch(new(791.93f, 214.38f, 965.76f), new(0f, 265f, 0f), 0);
                    break;
                case "trailera":
                    HouseLightsUtils.InstantiateSwitch(new(-2.95f, 1.38f, 2.06f), new(0f, 180f, 0f),0);
                    break;
                case "communityhalla":
                    HouseLightsUtils.InstantiateSwitch(new(0.128f, 1.38f, 4.20f), new(0f, 0f, 0f), 1);
                    HouseLightsUtils.InstantiateSwitch(new(8.52f, 1.40f, 0.26f), new(0f, 0f, 0f), 1);
                    HouseLightsUtils.InstantiateSwitch(new(6.91f, 1.54f, -3.97f), new(0f, 90f, 0f), 1);

                    HouseLightsUtils.InstantiateSwitch(new(6.58f, 1.43f, 0.22f), new(0f, 270f, 0f), 3);
                    HouseLightsUtils.InstantiateSwitch(new(-9.2f, 2.24f, 3.16f), new(0f, 180f, 0f), 3);

                    HouseLightsUtils.InstantiateSwitch(new(9.05f, 1.41f, 0.16f), new(0f, 180f, 0f), 0);
                    break;
                case "trailersshape":
                    HouseLightsUtils.InstantiateSwitch(new(7.20f, 1.50f, -10.12f), new(0f, 0f, 0f), 1);
                    HouseLightsUtils.InstantiateSwitch(new(0.59f, 1.55f, -5.92f), new(0f, 0f, 0f), 1);
                    HouseLightsUtils.InstantiateSwitch(new(-6.53f, 1.52f, 3.76f), new(0f, 0f, 0f), 1);
                    break;
                case "trailerb":
                    HouseLightsUtils.InstantiateSwitch(new(3.88f, 1.34f, 2.07f), new(0f, 180f, 0f), 3);
                    break;
                case "trailerc":
                    HouseLightsUtils.InstantiateSwitch(new(-0.88f, 1.32f, 2.07f), new(0f, 180f, 0f), 0);
                    break;
                case "trailerd":
                    HouseLightsUtils.InstantiateSwitch(new(-3.00f, 1.37f, 2.07f), new(0f, 180f, 0f), 1);
                    break;
                case "trailere":
                    HouseLightsUtils.InstantiateSwitch(new(-2.94f, 1.34f, 2.07f), new(0f, 180f, 0f), 0);
                    break;
                case "tracksregion":
                    HouseLightsUtils.InstantiateSwitch(new(586.17f, 200.48f, 564.31f), new(0f, 270f, 0f), 3);
                    break;
                case "mountainpassburiedcabin":
                    HouseLightsUtils.InstantiateSwitch(new(3.89f, 1.26f, 0.82f), new(0f, 270f, 0f), 1);
                    HouseLightsUtils.InstantiateSwitch(new(-0.52f, 5.17f, 1.711f), new(0f, 180f, 0f), 1);
                    HouseLightsUtils.InstantiateSwitch(new(-0.54f, 5.13f, 1.78f), new(0f, 0f, 0f), 1);
                    break;
                case "miltontrailerb":
                    HouseLightsUtils.InstantiateSwitch(new(3.92f, 1.50f, 2.07f), new(0f, 180f, 0f), 3);
                    break;
                case "huntinglodgea":
                    HouseLightsUtils.InstantiateSwitch(new(7.27f, 1.18f, -1.58f), new(0f, 90f, 0f), 0);
                    HouseLightsUtils.InstantiateSwitch(new(-1.17f, 1.51f, -5.01f), new(0f, 0f, 0f), 3);
                    break;
                case "damtrailerb":
                    HouseLightsUtils.InstantiateSwitch(new(4.01f, 1.49f, 2.07f), new(0f, 180f, 0f), 0);
                    break;
                case "crashmountainregion":
                    HouseLightsUtils.InstantiateSwitch(new(889.93f, 162.08f, 346.07f), new(0f, 180f, 0f), 3);
                    break;
                case "coastalregion":
                    HouseLightsUtils.InstantiateSwitch(new(757.9f, 25.51f, 646.78f), new(0f, 50f, 0f), 0);
                    break;
                case "cannerytrailera":
                    HouseLightsUtils.InstantiateSwitch(new(-3.02f, 1.42f, 2.79f), new(0f, 180f, 0f), 3);
                    break;
                case "bunkerc":
                    HouseLightsUtils.InstantiateSwitch(new(1.09f, 1.73f, 3.54f), new(0f, 0f, 0f), 3);
                    HouseLightsUtils.InstantiateSwitch(new(-14.72f, 0.33f, 12.93f), new(0f, 0f, 0f), 3);
                    break;
                case "bunkerb":
                    HouseLightsUtils.InstantiateSwitch(new(1.13f, 1.67f, 3.54f), new(0f, 0f, 0f), 3);
                    HouseLightsUtils.InstantiateSwitch(new(2.94f, 1.54f, 7.68f), new(0f, 90f, 0f), 3);
                    HouseLightsUtils.InstantiateSwitch(new(-3.19f, 1.61f, 7.66f), new(0f, 270f, 0f), 3);
                    break;
                case "bunkera":
                    HouseLightsUtils.InstantiateSwitch(new(5.93f, 1.61f, 12.63f), new(0f, 180f, 0f), 3);
                    HouseLightsUtils.InstantiateSwitch(new(1.18f, 1.65f, 1.62f), new(0f, 0f, 0f), 3);
                    break;
                case "blackrocktrailerb":
                    HouseLightsUtils.InstantiateSwitch(new(3.9726f, 1.4155f, 2.0799f), new(0f, 180f, 0f), 1);
                    break;
                case "airfieldtrailerb":
                    HouseLightsUtils.InstantiateSwitch(new(4.01f, 1.49f, 2.07f), new(0f, 180f, 0f), 0);
                    break;
            }
        }

        int type = 0;
        
        RaycastHit hit;
        public override void OnUpdate()
        {


            //Debugging purposes only for new lightswitch creation.
            if (Settings.options.Placer)
            {
                if (!GameManager.IsMainMenuActive() && InputManager.instance != null && InputManager.GetKeyDown(InputManager.m_CurrentContext, KeyCode.Equals))
                {

                    HUDMessage.AddMessage("Placed switch");
                    if (Physics.Raycast(GameManager.GetMainCamera().transform.position, GameManager.GetMainCamera().transform.TransformDirection(Vector3.forward), out hit, 5f))
                    {
                        Vector3 hitpos = hit.point;
                        Vector3 rot = hit.transform.rotation.eulerAngles;
                        HouseLightsUtils.InstantiateSwitch(hitpos, rot, type);
                        GameAudioManager.PlaySound("Play_FlashlightOn",GameManager.GetPlayerObject());
                    }
                }
                if (!GameManager.IsMainMenuActive() && InputManager.instance != null && InputManager.GetKeyDown(InputManager.m_CurrentContext, KeyCode.Minus))
                {
                    GameAudioManager.PlaySound("Play_FlashlightOff", GameManager.GetPlayerObject());
                    switch (type)
                    {
                        case 0:
                            {
                                HUDMessage.AddMessage("Switch Type: Industrial 02.");    
                                type = 3;
                                break;
                            }
                        case 1:
                            {
                                HUDMessage.AddMessage("Switch Type: Industrial 01.");
                                type = 0;
                                break;
                            }
                        case 3:
                            {
                                HUDMessage.AddMessage("Switch Type: House Switch.");
                                type = 1;
                                break;
                            }
                    }
                }
            }

        }
    }
}
