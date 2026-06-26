using System;
using System.Collections.Generic;
using Game.Placement;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class RadialMenuView : MonoBehaviour
    {
        const float MenuHeightOffset = RadialMenuLayout.PanelWorldMeters * 0.5f;

        [SerializeField] UIDocument _uiDocument;

        VisualElement _root;
        VisualElement _buttonHost;

        readonly List<RadialMenuItem> _items = new();
        PlacementRequest _activeRequest;
        bool _isOpen;

        int _blockWorldClickFrame = -1;

        public bool IsOpen => _isOpen;
        public PlacementRequest ActiveRequest => _activeRequest;
        public event Action<PlaceablePrototype> PlacementConfirmed;
        public event Action MenuClosed;

        public void Configure(UIDocument uiDocument)
        {
            _uiDocument = uiDocument;
            ConfigureWorldSpace();
            BindUi();
        }

        void Awake()
        {
            if (_uiDocument == null)
                _uiDocument = GetComponent<UIDocument>();

            ConfigureWorldSpace();
            BindUi();
            Hide();
        }

        void Start()
        {
            BindUi();
            Hide();
        }

        void LateUpdate()
        {
            if (!_isOpen)
                return;

            FaceCamera();
        }

        void BindUi()
        {
            if (_uiDocument == null)
                return;

            _root = _uiDocument.rootVisualElement.Q<VisualElement>("radial-menu-root");
            _buttonHost = _uiDocument.rootVisualElement.Q<VisualElement>("button-host");
            if (_buttonHost == null)
                return;

            RadialMenuPopulator.Populate(_buttonHost, _items, HandleItemClicked);
            RegisterPointerBlocks();
        }

        void ConfigureWorldSpace()
        {
            if (_uiDocument == null)
                return;

            _uiDocument.worldSpaceSizeMode = UIDocument.WorldSpaceSizeMode.Fixed;
            _uiDocument.worldSpaceSize = new Vector2(RadialMenuLayout.PanelPixels, RadialMenuLayout.PanelPixels);
            _uiDocument.pivot = Pivot.Center;
            _uiDocument.sortingOrder = 1000;
            transform.localScale = Vector3.one;
        }

        void FaceCamera()
        {
            var camera = UnityEngine.Camera.main;
            if (camera == null)
                return;

            transform.rotation = camera.transform.rotation;
        }

        public void SetItems(IReadOnlyList<RadialMenuItem> items)
        {
            _items.Clear();
            if (items != null)
                _items.AddRange(items);

            if (_buttonHost != null)
                RadialMenuPopulator.Populate(_buttonHost, _items, HandleItemClicked);

            RegisterPointerBlocks();
        }

        public void OpenAt(PlacementRequest request)
        {
            if (!request.HasPosition)
                return;

            BindUi();
            _activeRequest = request;
            _isOpen = true;
            transform.position = request.WorldPosition + Vector3.up * MenuHeightOffset;
            FaceCamera();
            SetVisible(true);
        }

        public void MoveTo(PlacementRequest request)
        {
            if (!request.HasPosition)
                return;

            _activeRequest = request;
            transform.position = request.WorldPosition + Vector3.up * MenuHeightOffset;
            FaceCamera();
        }

        public void Hide()
        {
            _isOpen = false;
            _activeRequest.Clear();
            SetVisible(false);
        }

        public bool ShouldBlockWorldClick(Vector2 screenPosition)
        {
            if (_blockWorldClickFrame == Time.frameCount)
                return true;

            if (!_isOpen)
                return false;

            return IsPointerOverMenu(screenPosition);
        }

        void HandleItemClicked(RadialMenuItem item)
        {
            _blockWorldClickFrame = Time.frameCount;

            switch (item.Action)
            {
                case RadialMenuAction.PlaceDefenseTower:
                case RadialMenuAction.PlaceIntelTower:
                    if (item.Prototype != null)
                        PlacementConfirmed?.Invoke(item.Prototype);
                    Hide();
                    break;
                case RadialMenuAction.Close:
                    MenuClosed?.Invoke();
                    Hide();
                    break;
            }
        }

        void RegisterPointerBlocks()
        {
            if (_buttonHost == null)
                return;

            for (var i = 0; i < _buttonHost.childCount; i++)
                RegisterPointerBlock(_buttonHost[i]);

            var ring = _root?.Q<VisualElement>(className: "radial-menu-ring");
            RegisterPointerBlock(ring);
        }

        void RegisterPointerBlock(VisualElement element)
        {
            if (element == null)
                return;

            element.UnregisterCallback<PointerDownEvent>(OnMenuPointerDown);
            element.RegisterCallback<PointerDownEvent>(OnMenuPointerDown);
        }

        void OnMenuPointerDown(PointerDownEvent _) => _blockWorldClickFrame = Time.frameCount;

        bool IsPointerOverMenu(Vector2 screenPosition)
        {
            if (!_isOpen || _uiDocument == null || _root == null)
                return false;

            var camera = UnityEngine.Camera.main;
            if (camera == null)
                return false;

            var panel = _uiDocument.rootVisualElement.panel;
            if (panel != null)
            {
                var panelPosition = RuntimePanelUtils.ScreenToPanel(panel, screenPosition);
                var picked = panel.Pick(panelPosition);
                if (picked != null && _root.Contains(picked))
                    return true;
            }

            return IsScreenPointOverRing(screenPosition, camera);
        }

        bool IsScreenPointOverRing(Vector2 screenPosition, UnityEngine.Camera camera)
        {
            var toCamera = camera.transform.position - transform.position;
            if (toCamera.sqrMagnitude < 0.0001f)
                return false;

            var plane = new Plane(toCamera.normalized, transform.position);
            var ray = camera.ScreenPointToRay(screenPosition);
            if (!plane.Raycast(ray, out var distance))
                return false;

            var localHit = transform.InverseTransformPoint(ray.GetPoint(distance));
            if (Mathf.Abs(localHit.z) > RadialMenuLayout.PanelWorldMeters * 0.25f)
                return false;

            var outerRadius = RadialMenuLayout.OuterRadiusPixels / RadialMenuLayout.PixelsPerMeter;
            var innerRadius = RadialMenuLayout.InnerRadiusPixels / RadialMenuLayout.PixelsPerMeter;
            var distSq = localHit.x * localHit.x + localHit.y * localHit.y;
            return distSq <= outerRadius * outerRadius && distSq >= innerRadius * innerRadius;
        }

        void SetVisible(bool visible)
        {
            if (_root == null)
                return;

            _root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            _root.pickingMode = PickingMode.Ignore;

            var ring = _root.Q<VisualElement>(className: "radial-menu-ring");
            if (ring != null)
                ring.pickingMode = visible ? PickingMode.Position : PickingMode.Ignore;

            if (_buttonHost == null)
                return;

            for (var i = 0; i < _buttonHost.childCount; i++)
                _buttonHost[i].pickingMode = visible ? PickingMode.Position : PickingMode.Ignore;
        }
    }
}
