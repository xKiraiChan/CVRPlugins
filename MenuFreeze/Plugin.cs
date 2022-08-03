using ABI_RC.Core.InteractionSystem;
using ABI_RC.Systems.MovementSystem;
using BepInEx;
using HarmonyLib;
using System;
using System.Reflection;

namespace com.github.xKiraiChan.CVRPlugins.MenuFreeze
{
    [BepInPlugin(GUID, "MenuFreeze", "0.2.0")]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "com.github.xKiraiChan.CVRPlugins.MenuFreeze";

        static Plugin()
        {
            Harmony harmony = new(GUID);
            HarmonyMethod hook = new(typeof(Plugin).GetMethod(nameof(Plugin.HookMenuOpen), BindingFlags.NonPublic | BindingFlags.Static));
            Type[] types = new Type[1] { typeof(bool) };

            harmony.Patch(typeof(ViewManager).GetMethod(nameof(ViewManager.UiStateToggle), types), null, hook);
            harmony.Patch(typeof(CVR_MenuManager).GetMethod(nameof(CVR_MenuManager.ToggleQuickMenu), types), null, hook);
        }

        private static void HookMenuOpen(bool __0) => MovementSystem.Instance.controller.enabled = !__0;
    }
}
