using Il2CppScheduleOne.NPCs.CharacterClasses;
using Il2CppScheduleOne.NPCs;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Il2CppFishNet.Connection;
using Il2CppFishNet.Managing;
using Il2CppFishNet.Managing.Object;
using Il2CppFishNet.Object;
using FishNet.Managing;
using Il2CppVLB;
using Il2CppScheduleOne.NPCs.Schedules;
using UnityEngine.Playables;
using UnityEngine.Events;
using Il2CppScheduleOne.AvatarFramework;
using static MelonLoader.MelonLaunchOptions;
using static Il2CppScheduleOne.AvatarFramework.AvatarSettings;
using Il2CppScheduleOne.Dialogue;
using Il2CppScheduleOne.Economy;
using Il2CppScheduleOne.NPCs.Behaviour;

namespace Cunny.Advanced
{
    //this is a MASSIVE undertaking, hence the advanced folder (for incredibly advanced stuff)
    //THIS CLASS IS HIGHLY SUBJECT TO CHANGE!!! AS IT IS MAINLY BEING USED FOR TESTING
    public  class NPCHandler
    {
        //Credit to Surrealnirvana, I looked at their code to understand this one.

        //Notable prefabs that might be what im looking for are as follows
        //PoliceNPC
        //Player | PlayerPrefab???
        //CivilianNPC
        //Dealer
        //BaseNPC
        //BaseEmployee
        //SUV_Police
        //For schedule, all schedule comps are in Il2CppScheduleOne.NPCs.Schedules.NPCEvent_NAME or Il2CppScheduleOne.NPCs.Schedules.NPCSignal_NAME

        /*Usage:
            NPCEvent_LocationBasedAction Task = AddToNPCSchedule<NPCEvent_LocationBasedAction>(NPC, comp => {
                comp.SetDestination(new Vector3(-22.43f, 0.7412f, 95.6903f));
                comp.SetStartTime(3);
                comp.Duration = 60;
                comp.ApplyDuration();
                comp.ApplyEndTime();
            });
         */
        public static T AddToNPCSchedule<T>(GameObject NPC, Action<T> configure = null) where T : NPCAction
        {
            Transform ScheduleFolder = NPC.transform.Find("Schedule");
            GameObject Task = new GameObject("Custom Action");
            Task.transform.parent = ScheduleFolder;
            T component = Task.AddComponent<T>();

            configure?.Invoke(component); //Usage: comp => {//Multiline code here for configuration}
            return component;
        }
        public static void InitNPC(GameObject NPC)
        {
            Core.Instance.LoggerInstance.Warning("---INIT NPC---");
            NPC.SetActive(true);
            var avatarObj = NPC.transform.Find("Avatar").gameObject;
            avatarObj.SetActive(true);
            GameObject ScheduleFolder = NPC.transform.Find("Schedule").gameObject;
            var civNPC = NPC.gameObject.GetComponent<NPC>();
            civNPC.Start();
            civNPC.SetVisible(true);

            var civHP = NPC.gameObject.GetComponent<NPCHealth>();
            civHP.MaxHealth = 100;
            civHP.Health = 100;
            /* TESTING
            var civDialogue = NPC.transform.Find("Dialogue").gameObject.GetComponent<DialogueModule>();
            civDialogue.Entries.Add(new Entry 
            { 
                Key = "This is a KEY",
                Chains = new DialogueChain[]
                {
                    new DialogueChain
                    {
                        Lines = new string[]
                        {
                            "Hello there!",
                            "How can I help you?",
                            "Nice weather today, huh?"
                        }
                    },
                    new DialogueChain
                    {
                        Lines = new string[]
                        {
                            "Goodbye!",
                            "Come again soon."
                        }
                    }
                }
            });
            
            //
            Avatar civAvatar = NPC.GetComponentInChildren<Avatar>();
            AvatarSettings civAvatarSettings = ScriptableObject.CreateInstance<AvatarSettings>();

            civAvatarSettings.AccessorySettings.Add(new AvatarSettings.AccessorySetting
            {
                path = "Avatar/Accessories/Feet/CombatBoots/CombatBoots",
                color = Color.black
            });
            civAvatarSettings.AccessorySettings.Add(new AvatarSettings.AccessorySetting
            {
                path = "Avatar/Accessories/Head/Saucepan/Saucepan",
                color = Color.white
            });
            civAvatarSettings.BodyLayerSettings.Add(new AvatarSettings.LayerSetting
            {
                layerPath = "Avatar/Layers/Bottom/Jeans",
                layerTint = Color.black
            });
            civAvatarSettings.BodyLayerSettings.Add(new AvatarSettings.LayerSetting
            {
                layerPath = "Avatar/Layers/Top/ButtonUp",
                layerTint = Color.white
            });
            civAvatarSettings.AccessorySettings.Add(new AvatarSettings.AccessorySetting
            {
                path = "Avatar/Accessories/Chest/CollarJacket/CollarJacket",
                color = Color.gray
            });

            civAvatarSettings.HairPath = "Avatar/Hair/LongCurly/LongCurly";
            civAvatarSettings.SkinColor =  new Color32(144, 128, 115, 255);

            civAvatar.CurrentSettings = civAvatarSettings;
            civAvatar.SetHairVisible(true);
            civAvatar.ApplyHairSettings(civAvatarSettings);
            civAvatar.ApplyAccessorySettings(civAvatarSettings);
            civAvatar.ApplyBodyLayerSettings(civAvatarSettings);

            //DO NOT USE LoadAvatarSettings!!! Rn, it bugs out and renders the avatar invisible.

            //civAvatar.LoadAvatarSettings(civAvatarSettings);

            //Testing for schedules

            AddToNPCSchedule<NPCEvent_LocationBasedAction>(NPC, comp => {
                comp.SetDestination(new Vector3(-22.43f, 0.7412f, 95.6903f));
                comp.SetStartTime(3);
                comp.Duration = 60;
                comp.ApplyDuration();
                comp.ApplyEndTime();
            });

            AddToNPCSchedule<NPCEvent_LocationBasedAction>(NPC, comp => {
                comp.SetDestination(new Vector3(-45.1303f, -0.035f, 83.4865f));
                comp.SetStartTime(4);
                comp.Duration = 60;
                comp.ApplyDuration();
                comp.ApplyEndTime();
            });

            //schedule component NEEDS to be added AFTER it goes into the folder or it will run a bunch of errors.

            //Test adding things
            civNPC.gameObject.AddComponent<Customer>();
            //Customer, Dealer, Supplier

            var civBehaviors = NPC.transform.Find("Behaviour").GetComponent<NPCBehaviour>();
            var civGenDialogue = NPC.gameObject.GetComponentInChildren<GenericDialogueBehaviour>();
            //civGenDialogue.SetTargetPlayer(); civGenDialogue doesn't have player set I need to change that
            */
        }
        public static GameObject SpawnCivPrefab(string FullName = "First Last")
        {
            string firstName = FullName.Split(' ')[0];
            string lastName = FullName.Split(' ')[1];

            NetworkObject prefab = LocatorUtils.GetPrefabByName("CivilianNPC");
            Core.Instance.LoggerInstance.Msg("PREFAB SPAWNING IS: "+(prefab == null));
            var newCiv = GameObject.Instantiate(prefab);
            newCiv.transform.position = new Vector3(-61.3939f, 0.975f, 84.0066f); //By default it will ALWAYS spawn outside the Motel
            newCiv.gameObject.name = firstName;
            var civNPC = newCiv.gameObject.GetComponent<NPC>();
            civNPC.FirstName = firstName;
            civNPC.LastName = lastName;
            civNPC.ID = firstName + "_" + lastName;

            //It seems InitNPC Needs to be run a short duration AFTER the civ is first Instantiated. (I.E you cant just SetActive(True) right after spawning)
            return newCiv.gameObject;
        }

        //ok, so here I will experiment with changing appearance
        //Comp "Avatar" contains all the required avatar/npc settings in AvatarSettings
        //It appears that all NPC/Player clothings and hair is in a unity internal folder called Avatar
        // Avatar/Hair
        // Avatar/Layers/Face
        // Avatar/Layers/Bottom
        // Avatar/Accessories/Feet
        // Avatar/Accessories/Bottom

        //CharacterCreator line 136 & 137 IMPORTANT!!!

        //Now the question is, how can we print all of these folders or atleast figure out what is what???
    }
}
