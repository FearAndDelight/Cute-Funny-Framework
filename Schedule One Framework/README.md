# Cute & Funny Framework
Cunny, a portmanteau of "Cute" and "Funny" is a framework for Schedule 1. This framework allows for more involved features to be easily automated and standardized such as app creation.

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
This framework make some invasive changes, and while it tries to do them as fast as possible it would be best check the bool IsCunnyLoaded (It will return true when everything has initialized)