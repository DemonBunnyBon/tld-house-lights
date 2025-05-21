using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using Il2Cpp;
using MelonLoader;
using System.Reflection.Metadata.Ecma335;

namespace HouseLights
{
    class HouseLightsUtils
    {
        internal static object InvokePrivMethod(object inst, string name, params object[] arguments)
        {
            MethodInfo method = inst.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
            if (!method.Equals(null))
            {
                return method.Invoke(inst, arguments);
            }
            return null;
        }

        internal static void SetPrivObj(object inst, string name, object value, Type type)
        {
            FieldInfo field = inst.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            if (!field.Equals(null) && field.FieldType.Equals(type))
            {
                field.SetValue(inst, value);
            }
        }

        internal static void SetPrivFloat(object inst, string name, float value)
        {
            SetPrivObj(inst, name, value, typeof(float));
        }

        internal static List<GameObject> GetRootObjects()
        {
            List<GameObject> rootObj = new List<GameObject>();

            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);

                GameObject[] sceneObj = scene.GetRootGameObjects();

                foreach (GameObject obj in sceneObj)
                {
                    rootObj.Add(obj);
                }
            }

            return rootObj;
        }

        internal static void GetChildrenWithName(GameObject obj, string name, List<GameObject> result)
        {
            if (obj.transform.childCount > 0)
            {

                for (int i = 0; i < obj.transform.childCount; i++)
                {
                    GameObject child = obj.transform.GetChild(i).gameObject;

                    if (child.name.ToLower().Contains(name))
                    {
                        result.Add(child);
                    }

                    GetChildrenWithName(child, name, result);
                }
            }
        }

        public static Shader vanillaShader = Shader.Find("Shader Forge/TLD_StandardDiffuse");

        public static GameObject InstantiateSwitch(Vector3 pos, Vector3 rot, int variant)
        {
            GameObject ls;
            if (variant == 1)
            {
                ls = GameManager.Instantiate(HouseLights.HLbundle.LoadAsset<GameObject>("OBJ_SwitchHLB"));
            }
            else if(variant == 2)
            {
                ls = GameManager.Instantiate(HouseLights.HLbundle.LoadAsset<GameObject>("OBJ_SwitchHLC"));
            }
            else if(variant == 3)
            {
                ls = GameManager.Instantiate(HouseLights.HLbundle.LoadAsset<GameObject>("OBJ_SwitchHLD"));
            }
            else
            {
                ls = GameManager.Instantiate(HouseLights.HLbundle.LoadAsset<GameObject>("OBJ_SwitchHL"));
            }

            MeshRenderer[] rs = ls.GetComponentsInChildren<MeshRenderer>();
            foreach(MeshRenderer r in rs)
            {
                Texture tex = r.material.mainTexture;
                Material mat = new(vanillaShader);
                mat.mainTexture = tex;
                r.material = mat;
            }

            ls.transform.eulerAngles = rot;
            ls.transform.position = pos;
            ls.transform.localScale = new(1f, 1f, 1f);

            if(Settings.options.Debug)
            {
                MelonLogger.Msg("Instantiate new House Lights switch.");
            }
            return ls;
        }

    }
}
