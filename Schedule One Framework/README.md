# Cute & Funny Framework
Cunny, a portmanteau of "Cute" and "Funny" is a framework for Schedule 1. This framework allows for more involved features to be easily automated and standardized.

## Why use the Cute & Funny Framework?
At the time of writing the framework contains
* Debugging and logging tools for Schedule 1
```
//Prints all clothing types able to be sold, and gives a read out of all their public variables.
Cunny.LocatorUtils.PrintAllVariablesOfTypeAll<ClothingDefinition>();
//Prints all the Unity Prefabs 
Cunny.LocatorUtils.PrintAllPrefabs();
```
* Streamlined app creation & Support for more than 12 apps (multi-page).
```
MelonCoroutines.Start(Cunny.Core.Instance.CreateApp("ID","APPNAME"))
GameObject canvas = Cunny.LocatorUtils.GetAppCanvasByName("APPNAME");
GameObject icon = Cunny.LocatorUtils.GetAppIconByName("APPNAME");
```
* (Experimental, introduced 0.2.0) Creation of custom NPCS, schedules, and application of cosmetics
!!!THIS IS HIGHLY SUBJECT TO CHANGE AS IT IS STILL IN DEVELOPMENT, USE WITH CAUTION!!!
```
var goober = Cunny.Advanced.NPCHandler.SpawnCivPrefab();
MelonCoroutines.Start(CoroutineUtils.Wait(5f,
() => { Cunny.Advanced.NPCHandler.InitNPC(goober); }));

var task = Advanced.NPCHandler.AddToNPCSchedule<NPCEvent_LocationBasedAction>(goober, comp => {
    comp.SetDestination(new Vector3(-22.43f, 0.7412f, 95.6903f));
    comp.SetStartTime(3);
    comp.Duration = 60;
    comp.ApplyDuration();
    comp.ApplyEndTime();
});
```
## How do you use this framework?
To use this framework simply add the dll as a reference in your C# project, and add the following to the top of your code
```
using Cunny;
```
You can then access its methods this way
```
Cunny.Core.Instance.CreateApp() //Note, this will need to be run with MelonCoroutines.Start
Cunny.LocatorUtils.GetAppCanvasByName()
```
This framework make some invasive changes on the phone's UI, and while it tries to do them as fast as possible it would be best check the bool IsCunnyLoaded (It will return true when everything has initialized)
