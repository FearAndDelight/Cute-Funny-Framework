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
            GameObject gameObject = GameObject.Find("Player_Local/CameraContainer/Camera/OverlayCamera/GameplayMenu/Phone/phone/AppCanvas");
            return gameObject.transform.Find(Name).gameObject;
        }
    }
}
