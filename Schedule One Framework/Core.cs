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
using UnityEngine.PlayerLoop;


namespace Cunny
{
    public class Core : MelonMod
    {
        public static Core Instance;
        //UI Elements
        ScrollRect HomeScreenScrollRect;
        GameObject IconsPageTemplate;
        GameObject CompatabilityIconsPage; //This is here soley to catch any apps trying to edit icons.
        GameObject HomeScreenScrollRectContent;
        HorizontalLayoutGroup HomeScreenHorizontalLayoutGroup;

        //Lists For Tracking Apps & UIs
        public System.Collections.Generic.List<GameObject> UIAppPages = new System.Collections.Generic.List<GameObject>();
        public System.Collections.Generic.List<System.Collections.Generic.List<GameObject>> AllApps = new System.Collections.Generic.List<System.Collections.Generic.List<GameObject>>();
        
        //Homepage Scrolling
        private int currentAppPage = 0;
        bool IsHomeScreenSnapped = false;
        float snapSpeed;
        float snapForce = 50f;
        private float snapOffset = -30f; // Now positive for left offset
        private float snapDuration = 0.15f;
        private float snapVelocity;
        private float snapVelocityThreshold = 200f;
        private bool isInitialSnapDone;
        private RectTransform contentRect;
        private bool isDraggingBlocked;
        private float mouseDownTime;
        private Vector2 mouseDownPosition;
        private float minHoldDuration = 0.15f;
        private float dragThreshold = 15f;

        //Core Values
        public bool IsCunnyLoaded = false;
        public override void OnInitializeMelon()
        {
            Instance = this;
        }
        public override void OnUpdate()
        {
            if (!IsCunnyLoaded) return;

            // Initialize references
            if (contentRect == null)
            {
                contentRect = HomeScreenScrollRectContent.GetComponent<RectTransform>();
                if (contentRect == null) return;
            }

            // Toggle scrolling capability
            HomeScreenScrollRect.enabled = UIAppPages.Count > 1;

            if (!HomeScreenScrollRect.enabled)
            {
                if (!isInitialSnapDone)
                {
                    SnapImmediately(0);
                    isInitialSnapDone = true;
                }
                return;
            }

            // Drag initialization logic
            if (Input.GetMouseButtonDown(0))
            {
                mouseDownTime = Time.time;
                mouseDownPosition = Input.mousePosition;
                isDraggingBlocked = true;
            }

            // Check if we should block dragging
            if (Input.GetMouseButton(0) && isDraggingBlocked)
            {
                var mouseDelta = Vector2.Distance(Input.mousePosition, mouseDownPosition);
                var holdTime = Time.time - mouseDownTime;

                if (holdTime >= minHoldDuration || mouseDelta >= dragThreshold)
                {
                    isDraggingBlocked = false;
                }
            }

            // Cancel accidental drags
            if (isDraggingBlocked && HomeScreenScrollRect.m_Dragging)
            {
                HomeScreenScrollRect.velocity = Vector2.zero;
                SnapImmediately(currentAppPage);
                HomeScreenScrollRect.StopMovement();
            }

            // Initial snap logic
            if (!isInitialSnapDone)
            {
                SnapImmediately(0);
                isInitialSnapDone = true;
                return;
            }

            // Main scrolling logic
            int totalPages = UIAppPages.Count;
            if (totalPages == 0) return;

            float pageWidth = UIAppPages[0].GetComponent<RectTransform>().rect.width +
                            HomeScreenHorizontalLayoutGroup.spacing;
            float currentPos = contentRect.anchoredPosition.x;

            currentAppPage = Mathf.Clamp(
                Mathf.RoundToInt((-currentPos - snapOffset) / pageWidth),
                0,
                totalPages - 1
            );

            float targetPos = -currentAppPage * pageWidth - snapOffset;
            float minPos = -((totalPages - 1) * pageWidth + snapOffset);
            float maxPos = -snapOffset;

            if (currentPos < minPos || currentPos > maxPos ||
                (!HomeScreenScrollRect.m_Dragging && Mathf.Abs(currentPos - targetPos) < 1f))
            {
                SnapImmediately(currentAppPage);
                return;
            }

            if (!HomeScreenScrollRect.m_Dragging && !isDraggingBlocked)
            {
                contentRect.anchoredPosition = new Vector2(
                    Mathf.SmoothDamp(currentPos, targetPos, ref snapVelocity, snapDuration),
                    contentRect.anchoredPosition.y
                );
            }

            if ((HomeScreenScrollRect.m_Dragging ||
                HomeScreenScrollRect.velocity.magnitude > snapVelocityThreshold) && !isDraggingBlocked)
            {
                HomeScreenScrollRect.velocity = Vector2.zero;
                snapVelocity = 0;
            }
        }

        private void SnapImmediately(int page)
        {
            if (UIAppPages.Count == 0) return;

            float pageWidth = UIAppPages[0].GetComponent<RectTransform>().rect.width +
                             HomeScreenHorizontalLayoutGroup.spacing;
            float targetPos = Mathf.Clamp(
                -page * pageWidth - snapOffset,
                -((UIAppPages.Count - 1) * pageWidth + snapOffset),
                -snapOffset
            );

            contentRect.anchoredPosition = new Vector2(targetPos, contentRect.anchoredPosition.y);
            snapVelocity = 0;
        }

        public override void OnLateUpdate()
        {
            //experimental backwards compatability layer
            bool toggle = false;
            if (!IsCunnyLoaded) return;
            if (CompatabilityIconsPage.GetComponentsInChildren<Transform>().Count > 1) {
                LoggerInstance.Msg("Missing app found, attempting to patch." + " Count: " + (CompatabilityIconsPage.GetComponentsInChildren<Transform>().Count-1));
                var children = CompatabilityIconsPage.GetComponentsInChildren<Transform>(true)
                                 .Select(t => t.gameObject)
                                 .Where(obj => obj.transform != CompatabilityIconsPage)
                                 .ToArray();
                foreach (var child in children)
                {
                    MelonCoroutines.Start(Patcher(child));
                }
            }

            IEnumerator Patcher(GameObject child)
            {
                while (!toggle)
                {
                    toggle = true;
                    yield return new WaitForSeconds(5f);
                }
                RegisterApp(child);
                toggle = false;
            }
        }
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            LoggerInstance.Msg(string.Format("Scene loaded: {0} ({1})", sceneName, buildIndex));
            bool flag = sceneName.Equals("Main", System.StringComparison.OrdinalIgnoreCase);
            if (flag)
            {
                this.FullCunnyReset(); //This is here incase you join someone else's game while in your own game
                LoggerInstance.Msg("Initializing Framework");
                MelonCoroutines.Start(this.InitCunny());
            }
            else
            {
                bool flag2 = sceneName.Equals("Menu", System.StringComparison.OrdinalIgnoreCase);
                if (flag2)
                {
                    this.FullCunnyReset();
                }
            }
        }

        private void FullCunnyReset() {
            //Used to reset all variables back to normal (Used in scene transitions)

            // Reset UI Element References
            HomeScreenScrollRect = null;
            IconsPageTemplate = null;
            HomeScreenScrollRectContent = null;
            HomeScreenHorizontalLayoutGroup = null;
            CompatabilityIconsPage = null;

            // Clear Lists
            UIAppPages.Clear();
            AllApps.Clear();

            // Reset Homepage Scrolling Variables
            currentAppPage = 0;
            IsHomeScreenSnapped = false;
            snapSpeed = 0f;
            snapForce = 100f;
            snapOffset = -30f;
            snapDuration = 0.15f;
            snapVelocity = 0f;
            isInitialSnapDone = false;

            // Reset Core Value
            IsCunnyLoaded = false;
        }
        private IEnumerator InitCunny()
        {   
            GameObject HomeScreen = null;
            GameObject Icons = null;
            yield return MelonCoroutines.Start(CoroutineUtils.WaitForObjectByFrame("Player_Local/CameraContainer/Camera/OverlayCamera/GameplayMenu/Phone/phone/HomeScreen/AppIcons/",
                (obj) => { Icons = obj; }));
            
            LoggerInstance.Msg("Editing Homescreen");

            //Inject homescreen with ScrollRect & Create Viewport + Content + HorizontalLayoutGroup
            HomeScreen = GameObject.Find("Player_Local/CameraContainer/Camera/OverlayCamera/GameplayMenu/Phone/phone/HomeScreen");
            ScrollRect scrollRect = HomeScreen.AddComponent<ScrollRect>();
            scrollRect.horizontal = true;
            scrollRect.vertical = false;
            HomeScreenScrollRect = scrollRect;

            GameObject viewportObj = new GameObject("Viewport", Il2CppType.Of<RectTransform>(), Il2CppType.Of<Image>(), Il2CppType.Of<Mask>());
            RectTransform viewportRect = viewportObj.GetComponent<RectTransform>();
            viewportObj.transform.SetParent(HomeScreen.transform, false);
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            viewportObj.GetComponent<Mask>().showMaskGraphic = false;
            
            GameObject contentObj = new GameObject("Content", Il2CppType.Of<RectTransform>());
            HomeScreenScrollRectContent = contentObj;
            RectTransform contentRect = contentObj.GetComponent<RectTransform>();
            contentObj.transform.SetParent(viewportObj.transform, false);
            contentRect.anchorMin = new Vector2(0, 0.5f);
            contentRect.anchorMax = new Vector2(0, 0.5f);
            contentRect.pivot = new Vector2(0, 0.5f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(1000, 200);
            contentRect.localPosition = new Vector3(contentRect.localPosition.x, 300, contentRect.localPosition.z);
            var fitter = contentObj.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            scrollRect.m_Dragging = true;
            scrollRect.m_ScrollSensitivity = 0; //temporary

            var layout = contentObj.AddComponent<HorizontalLayoutGroup>();
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;
            layout.spacing = 100;
            HomeScreenHorizontalLayoutGroup = layout;
            Icons.transform.SetParent(contentObj.transform, true);

            UIAppPages.Add(Icons);

            AllApps.Add(new System.Collections.Generic.List<GameObject>());
            GameObject[] array2 = (from t in Icons.GetComponentsInChildren<Transform>(true)
                                   select t.gameObject into g
                                   where g.name.StartsWith("AppIcon(Clone)")
                                   select g).ToArray<GameObject>();

            for (int i = 0; i < array2.Length; i++)
            {
                AllApps[0].Add(array2[i]);
            }

            LoggerInstance.Msg("App & Page List Established");

            IconsPageTemplate = UnityEngine.Object.Instantiate(Icons, null);
            Utils.ClearChildren(IconsPageTemplate.transform);

            //Make a fake version of the icons page for compatability reasons.
            CompatabilityIconsPage = new GameObject("AppIcons", Il2CppType.Of<RectTransform>());
            CompatabilityIconsPage.transform.SetParent(HomeScreen.transform, true);

            LoggerInstance.Msg("[INIT COMPLETE] Cloned & Cleaned AppUI");
            IsCunnyLoaded = true;
        }

        public void ChangeAppIconImage(GameObject appIcon, string ImagePath)
        {
            if (ImagePath == null) { ImagePath = "CunnyFramework\\ExampleIco.png"; }
            Transform transform = appIcon.transform.Find("Mask/Image");
            GameObject gameObject = (transform != null) ? transform.gameObject : null;
            bool flag = gameObject == null;
            if (!flag)
            {
                Image component = gameObject.GetComponent<Image>();
                bool flag2 = component == null;
                if (!flag2)
                {
                    Texture2D texture2D = Utils.LoadCustomImage(ImagePath);
                    bool flag3 = texture2D == null;
                    if (!flag3)
                    {
                        var newSprite = Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), new Vector2(0.5f, 0.5f));
                        component.sprite = newSprite;
                    }
                    else
                    {
                        LoggerInstance.Msg("Custom image failed to load");
                    }
                }
            }
        }
        public void RegisterApp(GameObject App, string Title="Unknown App")
        {
            //if this app exceeds the app count then simply create a new page
            if (AllApps[AllApps.Count - 1].Count >= 12)
            {
                LoggerInstance.Msg("Exceeded Space For Page: " + (AllApps.Count - 1) + " Creating New Page");
                AllApps.Add(new System.Collections.Generic.List<GameObject>());
                //Clone the template, and put it in uiapppages
                UIAppPages.Add(UnityEngine.Object.Instantiate(IconsPageTemplate, HomeScreenScrollRectContent.transform));
                GridLayoutGroup Grid = UIAppPages[UIAppPages.Count - 1].GetComponent<GridLayoutGroup>();
                Grid.enabled = true;
                UIAppPages[UIAppPages.Count - 1].name = "AppIcons" + UIAppPages.Count;
            }

            AllApps[AllApps.Count - 1].Add(App);
            App.transform.SetParent(UIAppPages[UIAppPages.Count - 1].transform, false);
            LoggerInstance.Msg("Added " + Title + " to Page: " + (AllApps.Count));
        }
        public void UnregisterApp(GameObject App)
        {
            //Todo: Add the ability to unregister apps and move them to a "deleted" 
        }

        public IEnumerator CreateApp(string IDName, string Title,bool IsRotated = true, string IconPath = null)
        {

            GameObject CloningCandiate = null;
            string CloningName = null;
            GameObject icons = null;

            yield return MelonCoroutines.Start(CoroutineUtils.WaitForObject("Player_Local/CameraContainer/Camera/OverlayCamera/GameplayMenu/Phone/phone/HomeScreen/Viewport/Content/AppIcons/",
                (obj) => { icons = obj; }));
            if (IsRotated)
            {
                yield return MelonCoroutines.Start(CoroutineUtils.WaitForObject("Player_Local/CameraContainer/Camera/OverlayCamera/GameplayMenu/Phone/phone/AppsCanvas/ProductManagerApp",
                (obj) => { CloningCandiate = obj; CloningName = "Products"; }));
            }
            else
            {
                yield return MelonCoroutines.Start(CoroutineUtils.WaitForObject("Player_Local/CameraContainer/Camera/OverlayCamera/GameplayMenu/Phone/phone/AppsCanvas/Messages",
                (obj) => { CloningCandiate = obj; CloningName = "Messages"; }));
            }

            GameObject appCanvas = GameObject.Find("Player_Local/CameraContainer/Camera/OverlayCamera/GameplayMenu/Phone/phone/AppsCanvas/");
            GameObject newApp = UnityEngine.Object.Instantiate(CloningCandiate, appCanvas.transform);

            Utils.ClearChildren(newApp.transform.Find("Container"), go => go.name == "Background");

            newApp.transform.Find("Container/Background").GetComponent<Image>().color = new Color32(240, 240, 240, 255);
            newApp.name = IDName;

            GameObject Ico = LocatorUtils.GetAppIconByName(CloningName, 1); 
            Transform transform = Ico.transform.Find("Label");
            GameObject gameObject3 = (transform != null) ? transform.gameObject : null;
            bool flag3 = gameObject3 != null;
            if (flag3)
            {
                Text component = gameObject3.GetComponent<Text>();
                bool flag4 = component != null;
                if (flag4)
                {
                    component.text = Title;
                }
            }
            ChangeAppIconImage(Ico, IconPath);
            RegisterApp(Ico,Title);
        }
    }
}