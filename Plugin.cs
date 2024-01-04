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
            LeanTween.cancelAll();
            if (__instance.loadsceneindex == 0)
            {
                GlobalVariables.scene_destination = "levelselect";
                SceneManager.LoadScene("levelselect");
            }
            else if (__instance.loadsceneindex == 1)
            {
                GlobalVariables.scene_destination = "levelselect";
                SceneManager.LoadScene("charselect_mp");
            }
            else if (__instance.loadsceneindex == 3)
            {
                GlobalVariables.tootscene = "treble";
                SceneManager.LoadScene("tootvessel");
            }
            else if (__instance.loadsceneindex == 4)
            {
                GlobalVariables.scene_destination = "freeplay";
                GlobalVariables.chosen_track = "freeplay";
                SceneManager.LoadScene("gameplay");
            }
            else if (__instance.loadsceneindex == 6)
                SceneManager.LoadScene("cards");
            else
                SceneManager.LoadScene(__instance.loadsceneindex);
            return false;
        }
    }

    [HarmonyPatch(typeof(LevelSelectController))]
    public static class LevelSelectControllerPatches
    {
        [HarmonyPatch(nameof(LevelSelectController.clickBack))]
        public static bool Prefix(LevelSelectController __instance)
        {
            if (__instance.back_clicked || __instance.btntimer > 0.0)
                return false;
            __instance.clipPlayer.cancelCrossfades();
            GlobalVariables.levelselect_index = __instance.songindex;
            GlobalVariables.practicemode = 1f;
            GlobalVariables.turbomode = false;
            __instance.back_clicked = true;
            __instance.bgmus.Stop();
            __instance.doSfx(__instance.sfx_slidedown);
            if (GlobalVariables.numplayers == 1)
            {
                __instance.fadeOut("home", 0.35f);
            }
            else if (GlobalVariables.numplayers > 1) {
                __instance.fadeOut("charselect_mp", 0.35f);
            }
            return false;
        }
    }
}
