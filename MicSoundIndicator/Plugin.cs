using ABI_RC.Core;
using ABI_RC.Core.Base;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace com.github.xKiraiChan.CVRPlugins.MicSoundIndicator
{
    [BepInPlugin(GUID, "MicSoundIndicator", "0.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "com.github.xKiraiChan.CVRPlugins.MicSoundIndicator";

        public ConfigEntry<int> volume;

        private static readonly Assembly assembly = Assembly.GetExecutingAssembly();
        private static AudioSource mute;
        private static AudioSource unmute;

        private void Awake()
        {
            GameObject go = new("MicSoundIndicator");
            DontDestroyOnLoad(go);

            (mute = go.AddComponent<AudioSource>()).clip = LoadAudioClip("Mute");
            (unmute = go.AddComponent<AudioSource>()).clip = LoadAudioClip("Unmute");

            volume = Config.Bind("General", "Volume", 40, "Percentage volume for the sound indicator");
            EventHandler onVolumeChanged = (_, __) => mute.volume = unmute.volume = volume.Value / 100f;
            onVolumeChanged.Invoke(null, null);

            new Harmony(GUID).Patch(
                typeof(Audio).GetMethod(nameof(Audio.SetMicrophoneActive)),
                new(typeof(Plugin).GetMethod(nameof(Plugin.HookSetMicrophoneActive), BindingFlags.NonPublic | BindingFlags.Static))
            );
        }

        private AudioClip LoadAudioClip(string name)
        {
            string path = Config.Bind("Resources", name + "Path", "BepInEx/config/MicSoundIndicator/" + name + ".mp3").Value;
            if (File.Exists(path))
            {
                WWW request = new(Path.Combine(Environment.CurrentDirectory, path));
                while (!request.isDone) request.MoveNext();
                return request.GetAudioClip();
            }

            MemoryStream mem = new();
            assembly.GetManifestResourceStream(GUID + ".Resources." + name + ".dat").CopyTo(mem);
            byte[] bytes = mem.ToArray();
            mem.Dispose();

            int samples = bytes.Length / 4;
            float[] data = new float[samples];
            Buffer.BlockCopy(bytes, 0, data, 0, bytes.Length);

            AudioClip clip = AudioClip.Create(name, samples, 2, 44100, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static bool HookSetMicrophoneActive(bool __0)
        {
            if (__0 == RootLogic.Instance.comms.IsMuted)
                return false;

            (__0 ? mute : unmute).Play();

            return true;
        }
    }
}
