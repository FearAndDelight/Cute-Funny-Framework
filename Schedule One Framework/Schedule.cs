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
    //This class is dedicated to calling functions within Schedule 1
    public class Schedule
    {
        public static void CreateNotification(string title, string subtitle, Sprite icon, float duration = 5f, bool playSound = true)
        {
            GameObject gameObject = GameObject.Find("UI/HUD/NotificationContainer");
            NotificationsManager component = gameObject.GetComponent<NotificationsManager>();
            component.SendNotification(title, subtitle, icon, duration, playSound);
        }
    }
}
