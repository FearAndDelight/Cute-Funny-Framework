using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Il2CppScheduleOne;
using Il2CppScheduleOne.NPCs.CharacterClasses;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.UI;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using UnityEngine.UI;
using Il2CppScheduleOne.NPCs.Behaviour;
using Il2CppInterop.Runtime;
using HarmonyLib;
using Il2CppScheduleOne.NPCs;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;
using System.Xml.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace Cunny
{
    public class Utils
    {
        public static void ClearChildren(Transform parent, System.Func<GameObject, bool> keepFilter = null)
        {
            var children = parent.GetComponentsInChildren<Transform>(true)
                                 .Select(t => t.gameObject)
                                 .Where(obj => obj.transform != parent)
                                 .ToArray();

            foreach (var child in children)
            {
                if (keepFilter == null || !keepFilter(child))
                {
                    child.transform.parent = null;
                    UnityEngine.Object.Destroy(child);
                }
            }
        }

        public static Texture2D LoadCustomImage(string path)
        {
            string finalPath = Path.Combine(MelonEnvironment.UserDataDirectory, path);
            bool flag = !File.Exists(finalPath);
            Texture2D result;
            if (flag)
            {
                result = null;
                Debug.LogError("[CUNNY] Specified path does not exist.");
            }
            else
            {
                byte[] array = File.ReadAllBytes(finalPath);
                Texture2D texture2D = new Texture2D(2, 2);
                ImageConversion.LoadImage(texture2D, array);
                result = texture2D;
            }
            return result;
        }
    }
}
