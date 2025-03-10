using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using BlueEyes.Entities;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TeenJumper_Road96_Mod
{
    [BepInEx.BepInPlugin(mod_guid, "Teen Jumper", version)]
    [BepInEx.BepInProcess("Road 96.exe")]
    public class TeenJumperMod : BasePlugin
    {
        private const string mod_guid = "miroxy12.teenjumper";
        private const string version = "1.0";
        private readonly Harmony harmony = new Harmony(mod_guid);
        internal static new ManualLogSource Log;
        public static string scenename = "";
        public static GameObject playerobj = null;

        public override void Load()
        {
            Log = base.Log;
            Log.LogInfo(mod_guid + " started, version: " + version);
            harmony.PatchAll(typeof(LoadSceneAsyncHook));
            AddComponent<ModMain>();
        }
    }
    public class ModMain : MonoBehaviour
    {
        private CharacterController characterController;
        private float flySpeed = 10f;

        void Awake()
        {
            TeenJumperMod.Log.LogInfo("loading Teen Jumper");
        }
        void OnEnable()
        {
            TeenJumperMod.Log.LogInfo("enabled Teen Jumper");
        }
        void Update()
        {
            if (TeenJumperMod.scenename != "") {
                Scene scene = SceneManager.GetSceneByName(TeenJumperMod.scenename);
                if (scene.isLoaded) {
                    GameObject[] gos = scene.GetRootGameObjects();
                    foreach (var go in gos) {
                        if (go.name.Contains("Logic") || go.name.Contains("LOGIC")) {
                            for (int i = 0; i < go.transform.childCount; i++) {
                                if (go.transform.GetChild(i).gameObject.name.Equals("Player")) {
                                    TeenJumperMod.playerobj = go.transform.GetChild(i).gameObject;
                                    characterController = TeenJumperMod.playerobj.GetComponent<CharacterController>();
                                    if (characterController != null)
                                    {
                                        characterController.enabled = false; // Disable collision
                                    }
                                    TeenJumperMod.scenename = "";
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }
            }

            if (TeenJumperMod.playerobj != null)
            {
                Vector3 move = new Vector3(
                    Input.GetAxis("Horizontal"),
                    (Input.GetKey(KeyCode.Space) ? 1 : 0) - (Input.GetKey(KeyCode.LeftControl) ? 1 : 0),
                    Input.GetAxis("Vertical")
                );
                
                TeenJumperMod.playerobj.transform.position += move * flySpeed * Time.deltaTime;
            }
        }
    }
    [HarmonyPatch(typeof(SceneManager), "LoadSceneAsync", new System.Type[] { typeof(string), typeof(LoadSceneMode) })]
    public class LoadSceneAsyncHook
    {
        static void Postfix(string sceneName, LoadSceneMode mode)
        {
            string[] bannedscene = {
                "000_Game/Scenes/SONYA_4/SONYA_4_Logic",
                "000_Game/Scenes/ALEX_1/ALEX_1_Logic",
                "000_Game/Scenes/GEN_DRIVE_1/GEN_DRIVE_1_LOGIC"
            };
            
            TeenJumperMod.playerobj = null;
            foreach (string i in bannedscene) {
                if (sceneName == i) {
                    return;
                }
            }
            
            if (sceneName.Contains("Logic") || sceneName.Contains("LOGIC")) {
                string[] tokens = sceneName.Split('/');
                if (SceneManager.GetSceneByName(sceneName) != null) {
                    TeenJumperMod.scenename = tokens[3];
                }
            }
        }
    }
}
