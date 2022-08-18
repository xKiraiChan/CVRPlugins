using ABI_RC.Systems.MovementSystem;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.Mono;
using System;
using System.Reflection;
using UnityEngine;

namespace com.github.xKiraiChan.CVRPlugins.LocalMirror
{
    [BepInPlugin(GUID, "LocalMirror", "0.2.0")]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "com.github.xKiraiChan.CVRPlugins.LocalMirror";

        public static Plugin Instance;

        private static readonly FieldInfo m_ReflectionTextureLeft = typeof(CVRMirror).GetField("m_ReflectionTextureLeft", BindingFlags.NonPublic | BindingFlags.Instance);

        public ConfigEntry<bool> Enabled;
        public ConfigEntry<KeyCode> Keybind;
        public ConfigEntry<float> ScaleX;
        public ConfigEntry<float> ScaleY;
        public GameObject Mirror;

        private void Awake()
        {
            Instance = this;

            Enabled = Config.Bind("General", "Enabled", true, "Should the mod be active?");
            Keybind = Config.Bind("General", "Keybind", KeyCode.L, "What key should toggle the mirror?");
            ScaleX = Config.Bind("Scale", "X", 1f, "The horizontal scaling");
            ScaleY = Config.Bind("Scale", "Y", 1f, "The vertical scaling");
        }

        private void Update()
        {
            if (!Enabled.Value || !Input.GetKeyDown(KeyCode.L))
                return;

            Mirror ??= CreateMirror();
            if (!Mirror) throw new Exception("Failed to create mirror");

            Mirror.SetActive(!Mirror.activeSelf);
            if (Mirror.activeSelf)
            {
                Transform local = MovementSystem.Instance.transform;
                Transform transform = Mirror.transform;

                transform.position = local.position + local.forward + new Vector3(0, ScaleY.Value / 2);
                transform.LookAt(local, Vector3.up);
                transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y + 180, 0);
            }
        }

        private GameObject CreateMirror()
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            DontDestroyOnLoad(obj);

            Destroy(obj.GetComponent<Collider>());

            Transform transform = obj.transform;
            EventHandler onScaleChanged = (sender, args) => transform.localScale = new(ScaleX.Value, ScaleY.Value);
            onScaleChanged.Invoke(null, null); // setup initial scaling

            ScaleX.SettingChanged += onScaleChanged;
            ScaleY.SettingChanged += onScaleChanged;

            CVRMirror mirror = obj.AddComponent<CVRMirror>();

            // this could be changed to use reflection to get the shader if its name ever changes
            // however the shader is a static field only assigned on start
            // that field is created next frame and we can't wait with this design
            obj.GetComponent<Renderer>().material = new(Shader.Find("FX/MirrorReflection"))
            {
                mainTexture = (RenderTexture)m_ReflectionTextureLeft.GetValue(mirror)
            };

            obj.SetActive(false);
            return obj;
        }
    }
}
