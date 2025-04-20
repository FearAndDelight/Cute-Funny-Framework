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


namespace Cunny
{
    //For use in waiting for things to exist in Unity. Good for when you want something done ASAP
    public class CoroutineUtils
    {
        //This is only to be for when you NEED something to be done instantly such as for initalization
        public static IEnumerator WaitForObjectByFrame(string path, System.Action<GameObject> callback)
        {
            GameObject obj = null;
            while (obj == null)
            {
                obj = GameObject.Find(path);
                yield return null;
            }
            callback?.Invoke(obj);
        }

        //This is the bog-standard "waitfor" function that will wait politely for an object times out at 30 seconds.
        public static IEnumerator WaitForObject(string path, System.Action<GameObject> callback, float timeout = 30f)
        {
            GameObject obj = null;
            float timer = 0f;

            while (obj == null && timer < timeout)
            {
                obj = GameObject.Find(path);
                if (obj != null) break;

                yield return new WaitForSeconds(0.1f);
                timer += 0.1f;
            }

            callback?.Invoke(obj); // obj will be null if not found within the timeout
        }

        public static IEnumerator Wait(float delay, System.Action callback)
        {
            yield return new WaitForSeconds(delay);
            callback?.Invoke();
        }

    }

}
