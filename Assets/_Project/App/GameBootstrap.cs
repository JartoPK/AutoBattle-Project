using AutoBattle.Meta;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AutoBattle.App
{
    public class GameBootstrap : MonoBehaviour
    {
        private static GameBootstrap _instance;

        private GameContext _ctx;
        private Camera _camera;
        private Hud _hud;
        private RecruitPanel _recruit;
        private UpgradeTreePanel _tree;
        private NodeInfoPanel _nodeInfo;

        private MapWorld _map;
        private BaseWorld _base;
        private bool _baseVisible;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (_instance != null) return;
            var go = new GameObject("AutoBattle.GameBootstrap");
            _instance = go.AddComponent<GameBootstrap>();
            DontDestroyOnLoad(go);
        }

        private void Awake()
        {
            var config = Resources.Load<GameConfig>("GameConfig");
            if (config == null)
            {
                Debug.LogError("[AutoBattle] Falta Resources/GameConfig.");
                return;
            }

            _ctx = new GameContext
            {
                Config = config,
                State = MetaSaveService.Load() ?? MetaGameState.New(config.startingCoins),
            };

            _camera = EnsureCamera();
            EnsureLight();
            EnsureEventSystem();

            var canvas = CreateCanvas();
            _hud = new Hud(canvas, _ctx, ShowBase, ShowMap);
            _recruit = new RecruitPanel(canvas, _ctx, OnChanged);
            _tree = new UpgradeTreePanel(canvas, _ctx, OnChanged);
            _nodeInfo = new NodeInfoPanel(canvas);

            var art = Resources.Load<ArtConfig>("ArtConfig");

            HideAllSceneObjects();

            _map = new MapWorld(transform, node => _nodeInfo.Show(node));
            _base = new BaseWorld(transform, art, () => _recruit.Show(), () => _tree.Show());

            var startScene = SceneManager.GetActiveScene().name;
            if (startScene == "Base")
                ShowBase();
            else
                ShowMap();
        }

        private void OnChanged()
        {
            _hud.Refresh();
            if (_baseVisible) _base.RefreshTroops(_ctx.State.roster);
        }

        // ── Scene transitions ──────────────────────────────────────────

        private void ShowMap()
        {
            _recruit.Hide();
            _tree.Hide();
            _nodeInfo.Hide();
            _base.Hide();
            HideScene("Base");
            _baseVisible = false;

            EnsureSceneAndRun("Map", () =>
            {
                ShowScene("Map");
                _map.OnSceneLoaded();
                _map.Show(_camera);
                _hud.SetContext(false);
                _hud.Refresh();
            });
        }

        private void ShowBase()
        {
            _recruit.Hide();
            _tree.Hide();
            _nodeInfo.Hide();
            _map.Hide();
            HideScene("Map");
            _hud.SetContext(true);
            _hud.Refresh();

            EnsureSceneAndRun("Base", () =>
            {
                ShowScene("Base");
                _base.OnSceneLoaded();
                _base.Show(_camera);
                _base.RefreshTroops(_ctx.State.roster);
                _baseVisible = true;
            });
        }

        // ── Scene helpers ──────────────────────────────────────────────

        private void EnsureSceneAndRun(string sceneName, System.Action then)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            if (scene.isLoaded)
            {
                then();
                return;
            }

            try
            {
                var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                if (op != null)
                {
                    op.completed += _ => then();
                    return;
                }
            }
            catch { /* scene not in build settings */ }

            then();
        }

        private static void ShowScene(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.isLoaded) return;
            foreach (var go in scene.GetRootGameObjects())
            {
                if (go.GetComponent<Camera>() != null) continue;
                go.SetActive(true);
            }
        }

        private static void HideScene(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.isLoaded) return;
            foreach (var go in scene.GetRootGameObjects())
                go.SetActive(false);
        }

        private static void HideAllSceneObjects()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                foreach (var go in scene.GetRootGameObjects())
                    go.SetActive(false);
            }
        }

        // ── Infrastructure ─────────────────────────────────────────────

        private Camera EnsureCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = go.AddComponent<Camera>();
            }
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.10f, 0.12f, 0.15f);
            if (cam.GetComponent<PhysicsRaycaster>() == null)
                cam.gameObject.AddComponent<PhysicsRaycaster>();
            if (cam.GetComponent<Physics2DRaycaster>() == null)
                cam.gameObject.AddComponent<Physics2DRaycaster>();
            DontDestroyOnLoad(cam.gameObject);
            return cam;
        }

        private static void EnsureLight()
        {
            if (Object.FindFirstObjectByType<Light>() != null) return;
            var go = new GameObject("Sun");
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            DontDestroyOnLoad(go);
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem", typeof(EventSystem));
            var module = go.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
            DontDestroyOnLoad(go);
        }

        private static Transform CreateCanvas()
        {
            var go = new GameObject("AutoBattleCanvas",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            DontDestroyOnLoad(go);
            return go.transform;
        }
    }
}
