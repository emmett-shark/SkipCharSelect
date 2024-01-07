using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.SceneManagement;

namespace SkipCharSelect;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        Instance = this;
        Log = Logger;
        new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();
    }

    public static Plugin Instance;
    public static ManualLogSource Log;

    [HarmonyPatch(typeof(HomeController))]
    public static class HomeControllerPatches
    {
        [HarmonyPatch(nameof(HomeController.loadscene))]
        public static bool Prefix(HomeController __instance)
        {
            if (__instance.loadsceneindex == 0)
            {
                LeanTween.cancelAll();
                GlobalVariables.scene_destination = "levelselect";
                SceneManager.LoadScene("levelselect");
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(LevelSelectController))]
    public static class LevelSelectControllerPatches
    {
        [HarmonyPatch(nameof(LevelSelectController.fadeOut))]
        public static void Postfix(string scene_name, LevelSelectController __instance)
        {
            if (scene_name == "charselect")
                __instance.nextscene = "home";
        }
    }
}
