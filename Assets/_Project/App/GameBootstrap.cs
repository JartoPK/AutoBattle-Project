using AutoBattle.Meta;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace AutoBattle.App
{
    /// <summary>
    /// Arranca la app al entrar en Play (sin montar nada en escena): cámara 3D,
    /// luz, EventSystem con raycaster 3D, canvas 2D, mundos (mapa/base) y la UI.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        private static GameBootstrap _instance;

        private GameContext _ctx;
        private Camera _camera;
        private MapWorld _map;
        private BaseWorld _base;
        private Hud _hud;
        private RecruitPanel _recruit;
        private UpgradeTreePanel _tree;
        private NodeInfoPanel _nodeInfo;

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
                Debug.LogError("[AutoBattle] Falta Resources/GameConfig. Ejecuta 'AutoBattle/Setup/Crear TODO + GameConfig (para la UI)'.");
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
            _recruit = new RecruitPanel(canvas, _ctx, OnChanged); // después del HUD: el modal queda por encima
            _tree = new UpgradeTreePanel(canvas, _ctx, OnChanged);
            _nodeInfo = new NodeInfoPanel(canvas);

            _map = new MapWorld(transform, config.campaignMap, node => _nodeInfo.Show(node));
            _base = new BaseWorld(transform, () => _recruit.Show(), () => _tree.Show());

            ShowMap();
        }

        private void OnChanged()
        {
            _hud.Refresh();
            _base.RefreshTroops(_ctx.State.roster);
        }

        private void ShowMap()
        {
            _recruit.Hide();
            _tree.Hide();
            _nodeInfo.Hide();
            _base.Hide();
            _map.Show(_camera);
            _hud.SetContext(false);
            _hud.Refresh();
        }

        private void ShowBase()
        {
            _recruit.Hide();
            _tree.Hide();
            _nodeInfo.Hide();
            _map.Hide();
            _base.Show(_camera);
            _base.RefreshTroops(_ctx.State.roster);
            _hud.SetContext(true);
            _hud.Refresh();
        }

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
                cam.gameObject.AddComponent<PhysicsRaycaster>(); // clics en objetos 3D
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
            module.AssignDefaultActions(); // necesario con el Input System nuevo
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
