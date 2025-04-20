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
using System.Xml.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using Il2CppSystem.IO;
using Il2CppFishNet.Connection;
using Il2CppFishNet.Managing;
using Il2CppFishNet.Managing.Object;
using Il2CppFishNet.Object;
using FishNet.Managing;
using System.Reflection;
using Il2CppScheduleOne.Clothing;

namespace Cunny
{
    //I HATE USING .FIND()!!! This helps simplify finding gameobjects as you no longer need to memorize the exact path!
    public class LocatorUtils
    {
        //Player & NPC Scanning Variables
        private static System.Collections.Generic.List<GameObject> cachedNPCs;
        private static System.Collections.Generic.List<GameObject> cachedPlayers;
        private static float lastUpdateTime;

        public static GameObject GetAppIconByName(string Name, int? Index) //Fetches an appicon through its name (THIS IS THE NAME AS IS ON THE PHONE IN-GAME)
        {
            int index = Index ?? 0; //Index is only going to be used to try and get newly duplicated apps!
            GameObject gameObject = GameObject.Find("Player_Local/CameraContainer/Camera/OverlayCamera/GameplayMenu/Phone/phone/HomeScreen/Viewport/Content/");
            GameObject[] array = (
                from t in gameObject.GetComponentsInChildren<Transform>(true)
                let labelTransform = t.gameObject.transform.Find("Label")
                let textComponent = labelTransform != null ? labelTransform.GetComponent<Text>() : null
                where textComponent != null && textComponent.text != null && textComponent.text.StartsWith(Name)
                select t.gameObject
            ).ToArray();
            return array[index];
        }

        public static GameObject GetAppCanvasByName(string Name) //Just here to avoid having to use the big name every time.
        {
            GameObject gameObject = GameObject.Find("Player_Local/CameraContainer/Camera/OverlayCamera/GameplayMenu/Phone/phone/AppsCanvas/");
            return gameObject.transform.Find(Name).gameObject;
        }

        public static void PrintAllPrefabs()
        {
            var spawnablePrefabs = Core.Instance.networkManager.SpawnablePrefabs;
            System.Collections.Generic.List<string> prefabNames = new System.Collections.Generic.List<string>();

            for (int i = 0; i < spawnablePrefabs.GetObjectCount(); i++)
            {
                var obj = spawnablePrefabs.GetObject(true, i);
                prefabNames.Add(obj.gameObject.name);
            }

            prefabNames.Sort();
            Core.Instance.LoggerInstance.Msg("---PRINTING ALL PREFABS FROM NETWORK MANAGER---");
            foreach (var name in prefabNames)
            {
                Core.Instance.LoggerInstance.Msg(name);
            }
        }

        public static NetworkObject GetPrefabByName(string PrefabName)
        {
            NetworkObject prefab = null;
            var spawnablePrefabs = Core.Instance.networkManager.SpawnablePrefabs;

            for (int i = 0; i < spawnablePrefabs.GetObjectCount(); i++)
            {
                var obj = spawnablePrefabs.GetObject(true, i);
                if (obj.gameObject.name == PrefabName)
                {
                    return obj;
                }
            }
            Core.Instance.LoggerInstance.Error("Could not find prefab named: " + PrefabName);
            return null;
        }
        public static void PrintStreamingAssets()
        {
            Core.Instance.LoggerInstance.Msg("---PRINTING ALL ASSETS FROM STREAMING ASSETS---");
            string rootPath = Application.streamingAssetsPath;

            if (!System.IO.Directory.Exists(rootPath))
            {
                Core.Instance.LoggerInstance.Msg("StreamingAssets folder does not exist at: " + rootPath);
                return;
            }

            PrintDirectoryTree(rootPath, rootPath, 0);
        }

        private static void PrintDirectoryTree(string rootPath, string currentPath, int indent)
        {
            string indentStr = new string(' ', indent * 2);

            // Print directories
            foreach (string dir in System.IO.Directory.GetDirectories(currentPath))
            {
                string folderName = System.IO.Path.GetFileName(dir);
                Core.Instance.LoggerInstance.Msg($"{indentStr}[Folder] {folderName}");
                PrintDirectoryTree(rootPath, dir, indent + 1);
            }

            // Print files
            foreach (string file in System.IO.Directory.GetFiles(currentPath))
            {
                string fileName = System.IO.Path.GetFileName(file);
                Core.Instance.LoggerInstance.Msg($"{indentStr}[File] {fileName}");
            }
        }

        public static void PrintResources()
        {
            Core.Instance.LoggerInstance.Msg("---PRINTING ALL RESOURCES FROM RESOURCES---");
            Il2CppSystem.Object[] allResources = Resources.LoadAll("", Il2CppType.Of<Il2CppSystem.Object>());

            if (allResources.Length == 0)
            {
                Core.Instance.LoggerInstance.Msg("No assets found in the Resources folder.");
                return;
            }

            System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<UnityEngine.Object>> pseudoFolders = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<UnityEngine.Object>>();

            foreach (UnityEngine.Object obj in allResources)
            {
                string[] nameParts = obj.name.Split('/');
                string folder = nameParts.Length > 1 ? string.Join("/", nameParts, 0, nameParts.Length - 1) : "(root)";

                if (!pseudoFolders.ContainsKey(folder))
                    pseudoFolders[folder] = new System.Collections.Generic.List<UnityEngine.Object>(); 

                pseudoFolders[folder].Add(obj);
            }

            foreach (var folder in pseudoFolders.OrderBy(f => f.Key))
            {
                Core.Instance.LoggerInstance.Msg($"[Psuedo-Folder] {folder.Key}");
                foreach (var obj in folder.Value)
                {
                    Core.Instance.LoggerInstance.Msg($"  [File] {obj.name} ({obj.GetType().Name})");
                }
            }
        }

        //I created PrintAllVariablesOfTypeAll strictly for definitions, but it should be able to print the PUBLIC variables of ANY class.
        //Ok, so this isn't perfect. It heavily relies on the assumption that the defintion classes store EVERY possible variation available in-game.
        //Refer to this page https://github.com/FearAndDelight/Schedule-1-Modder-Documentation/wiki/Definitions
        //This will get you pretty much everything for accessories for NPCS, BUT for hair specifically for some weird reason theres an exception.

        public static void PrintAllVariablesOfTypeAll<T>() where T : UnityEngine.Object
        {
            T[] allDefs = Resources.FindObjectsOfTypeAll<T>();

            if (allDefs == null || allDefs.Length == 0)
            {
                Core.Instance.LoggerInstance.Msg($"No instances of {typeof(T).Name} found.");
                return;
            }

            Core.Instance.LoggerInstance.Msg($"---Found {allDefs.Length} instances of {typeof(T).Name}: ---");

            foreach (T def in allDefs)
            {
                DumpObject(def);
            }
        }

        public static void DumpObject(object obj)
        {
            if (obj == null)
            {
                Core.Instance.LoggerInstance.Msg("Null object passed to DumpObject.");
                return;
            }

            System.Type type = obj.GetType();
            Core.Instance.LoggerInstance.Warning("      ");
            Core.Instance.LoggerInstance.Msg($"--- {type.Name}: {((obj is UnityEngine.Object uo) ? uo.name : "Unnamed")} ---");

            // Print public instance fields
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                object value = field.GetValue(obj);
                Core.Instance.LoggerInstance.Msg($"{field.Name}: {FormatValue(value)}");
            }

            // Print public instance properties with getters
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                if (property.GetIndexParameters().Length == 0 && property.CanRead)
                {
                    object value = property.GetValue(obj, null);
                    Core.Instance.LoggerInstance.Msg($"{property.Name}: {FormatValue(value)}");
                }
            }
        }

        private static string FormatValue(object value)
        {
            if (value == null) return "null";

            if (value is System.Collections.IEnumerable enumerable && !(value is string))
            {
                var list = new System.Text.StringBuilder();
                list.Append("[");
                foreach (var item in enumerable)
                {
                    list.Append(item?.ToString() + ", ");
                }
                if (list.Length > 1) list.Length -= 2; // Remove trailing comma
                list.Append("]");
                return list.ToString();
            }

            return value.ToString();
        }
        //Im sorry but I can't seem to find any dependable way to print the paths EXCEPT for the barbershop UI.
        //They all follow a predictable path, as in they all start with Avatar/Hair/NAME/NAME (Source: Trust me bro, it prints to console in the barber shop)
        public static void PrintAllHairStyles() {
            // To be implemented in future update, for now use UnityExplorer to see debug logs when you're in the barber shop.
        }
    }
}
