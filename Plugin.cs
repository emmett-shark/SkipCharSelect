using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using BepInEx.Configuration;

namespace SkipCharSelect;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public static ConfigEntry<bool> enableButtons;

    private void Awake()
    {
        Instance = this;
        Log = Logger;
        enableButtons = Config.Bind("Default", "EnableButtons", true, "Enable buttons to access the char select screen.");
        new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();
    }

    public static Plugin Instance;
    public static ManualLogSource Log;
    public static bool loadCharScene;
    public static GameObject btnPrefab;

    [HarmonyPatch(typeof(HomeController))]
    public static class HomeControllerPatches
    {
        [HarmonyPatch(nameof(HomeController.loadscene))]
        public static bool Prefix(HomeController __instance)
        {
            if (__instance.loadsceneindex == 0 && !loadCharScene)
            {
                LeanTween.cancelAll();
                GlobalVariables.scene_destination = "levelselect";
                SceneManager.LoadScene("levelselect");
                return false;
            }
            loadCharScene = false;
            return true;
        }

        [HarmonyPatch(nameof(HomeController.Start))]
        public static void Postfix(HomeController __instance)
        {
            if (btnPrefab == null)
            {
                GameObject settingBtn = __instance.fullsettingspanel.transform.Find("Settings/GRAPHICS/btn_opengraphicspanel").gameObject;
                btnPrefab = Instantiate(settingBtn);
                btnPrefab.GetComponentInChildren<Text>().text = "CharSelect";
                btnPrefab.GetComponent<Button>().colors = new ColorBlock()
                {
                    fadeDuration = .2f,
                    colorMultiplier = 1f,
                    disabledColor = Color.gray,
                    normalColor = Color.black,
                    highlightedColor = Color.gray,
                    pressedColor = Color.white,
                    selectedColor = Color.black
                };
                var rect = btnPrefab.GetComponent<RectTransform>();
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(164, 42);
                rect.anchorMin = rect.anchorMax = new Vector2(.49f, .93f);
                Object.DontDestroyOnLoad(btnPrefab);
            }

            if (!enableButtons.Value) return;

            var btnGameObject = GameObject.Instantiate(btnPrefab, __instance.fullcanvas.transform);
            var btn = btnGameObject.GetComponent<Button>();
            btn.onClick.AddListener(delegate
            {
                loadCharScene = true;
                __instance.btnclick1();
            });
        }
    }

    [HarmonyPatch(typeof(LevelSelectController))]
    public static class LevelSelectControllerPatches
    {
        [HarmonyPatch(nameof(LevelSelectController.fadeOut))]
        public static void Postfix(string scene_name, LevelSelectController __instance)
        {
            if (scene_name == "charselect" && !loadCharScene)
                __instance.nextscene = "home";
            loadCharScene = false;
        }

        //Button in the level select screen kinda bad so I removed it
        /*[HarmonyPatch(nameof(LevelSelectController.Start))]
        public static void Postfix(LevelSelectController __instance)
        {
            if (!enableButtons.Value) return;

            var btnGameObject = GameObject.Instantiate(btnPrefab, __instance.fullpanel.transform);
            var btn = btnGameObject.GetComponent<Button>();
            btn.transform.localScale = Vector2.one * .42f;
            var rect = btn.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.14f, .885f);
            rect.sizeDelta = new Vector2(180, 42);
            btn.onClick.AddListener(delegate
            {
                loadCharScene = true;
                __instance.clickBack();
            });
        }*/
    }
}
