using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;

#if BEPINEX
using BepInEx;
using HarmonyLib;

namespace BrittleHolds {
    [BepInPlugin("com.github.Kaden5480.poy-brittle-holds", "BrittleHolds", PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin {
        public void Awake() {
            instance = this;

            Harmony.CreateAndPatchAll(typeof(Patches.PatchClimbing));

            config.maxHp = Config.Bind(
                "General", "maxHp", defaultMaxHp,
                "The max HP to set on holds before they break"
            );

            SceneManager.sceneLoaded += OnSceneLoaded;

            StartCoroutine(LoadCrumbleSound());
        }

        public void OnDestroy() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            CommonSceneLoad();
        }

#elif MELONLOADER
using MelonLoader;
using MelonLoader.Utils;

[assembly: MelonInfo(typeof(BrittleHolds.Plugin), "BrittleHolds", PluginInfo.PLUGIN_VERSION, "Kaden5480")]
[assembly: MelonGame("TraipseWare", "Peaks of Yore")]

namespace BrittleHolds {
    public class Plugin: MelonMod {
        public override void OnInitializeMelon() {
            instance = this;

            string path = $"{MelonEnvironment.UserDataDirectory}/com.github.Kaden5480.poy-brittle-holds.cfg";

            MelonPreferences_Category general = MelonPreferences.CreateCategory("BrittleHolds_General");
            general.SetFilePath(path);

            config.maxHp = general.CreateEntry("maxHp", defaultMaxHp);

            MelonCoroutines.Start(LoadCrumbleSound());
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName) {
            CommonSceneLoad();
        }

#endif

        public static Plugin instance = null;

        private const int defaultMaxHp = 4;
        public Cfg config { get; } = new Cfg();

        AudioClip crumblingHoldClip = null;

        private IEnumerator LoadCrumbleSound() {
            const int buildIndex = 14;

            AsyncOperation load = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);
            while (load.isDone == false) {
                yield return null;
            }

            foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>()) {
                CrumblingHoldRegular crumblingHold = obj.GetComponent<CrumblingHoldRegular>();
                if (crumblingHold == null
                    || crumblingHold.pebbleSoundL == null
                    || crumblingHold.pebbleSoundL.clip == null
                ) {
                    continue;
                }

                crumblingHoldClip = crumblingHold.pebbleSoundL.clip;
                break;
            }

            AsyncOperation unload = SceneManager.UnloadSceneAsync(buildIndex);
            while (unload.isDone == false) {
                yield return null;
            }

            SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
        }

        private void MakeBrittle(GameObject obj) {
            MeshFilter filter = obj.GetComponent<MeshFilter>();
            MeshCollider collider = obj.GetComponent<MeshCollider>();

            if (filter == null || collider == null) {
                return;
            }

            Rigidbody rigidBody = obj.GetComponent<Rigidbody>();

            if (rigidBody == null) {
                rigidBody = obj.AddComponent<Rigidbody>();
            }

            rigidBody.isKinematic = true;

            BrittleHold brittleHold = obj.AddComponent<BrittleHold>();
            brittleHold.source = obj.AddComponent<AudioSource>();
            brittleHold.source.clip = crumblingHoldClip;

            if (collider.sharedMesh != null) {
                filter.sharedMesh = collider.sharedMesh;
            }
        }

        private void CommonSceneLoad() {
            GameObject[] objs = GameObject.FindObjectsOfType<GameObject>();

            foreach (GameObject obj in objs) {
                if ("Climbable".Equals(obj.tag) == false) {
                    continue;
                }

                MakeBrittle(obj);
            }
        }
    }
}
