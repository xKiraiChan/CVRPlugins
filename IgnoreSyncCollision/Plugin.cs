using ABI.CCK.Components;
using ABI_RC.Systems.MovementSystem;
using System.Linq;
using UnityEngine;

#if MELONLOADER
[assembly: MelonLoader.MelonInfo(typeof(com.github.xKiraiChan.CVRPlugins.IgnoreSyncCollision.Mod), "IgnoreSyncCollision", "0.1.0", "xKiraiChan")]
[assembly: MelonLoader.MelonGame("Alpha Blend Interactive", "ChilloutVR")]
#else
using BepInEx;
#endif

namespace com.github.xKiraiChan.CVRPlugins.IgnoreSyncCollision
{
#if MELONLOADER
    public class Mod : MelonLoader.MelonMod {
#else
    [BepInPlugin(GUID, "IgnoreSyncCollision", "0.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "com.github.xKiraiChan.CVRPlugins.IgnoreSyncCollision";
#endif

#if MELONLOADER
        public override void OnApplicationStart()
#else
        private void Awake()
#endif
        {
            HarmonyLib.Harmony.CreateAndPatchAll(typeof(Hooks));
        }

        private static class Hooks
        {
            [HarmonyLib.HarmonyPrefix, HarmonyLib.HarmonyPatch(typeof(CVRObjectSync), nameof(CVRObjectSync.Start))]
            public static void Hook_Start(CVRObjectSync __instance)
            {
                Collider player = MovementSystem.Instance.GetComponent<Collider>();

                foreach (Collider collider in __instance.GetComponents<Collider>()
                    .Concat(__instance.GetComponentsInChildren<Collider>(true))
                    .Where(collider => !Physics.GetIgnoreCollision(collider, player)))
                    Physics.IgnoreCollision(collider, player);
            }
        }
    }
}
