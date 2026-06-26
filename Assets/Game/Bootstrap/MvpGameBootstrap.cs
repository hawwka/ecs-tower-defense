using System.Collections.Generic;
using Game.Camera;
using Game.ECS;
using Game.Placement;
using Game.UI;
using Game.World;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Bootstrap
{
    public class MvpGameBootstrap : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] UnityEngine.Camera _mainCamera;
        [SerializeField] GamePlane _gamePlane;
        [SerializeField] Transform _placedEntitiesRoot;

        [Header("Controllers")]
        [SerializeField] IsometricCameraController _cameraController;
        [SerializeField] PlacementInputController _placementInputController;
        [SerializeField] RadialMenuView _radialMenuView;

        [Header("Prototypes")]
        [SerializeField] PlaceablePrototype _defenseTowerPrototype;
        [SerializeField] PlaceablePrototype _intelTowerPrototype;

        EntityWorld _entityWorld;
        PlacementService _placementService;

        void Awake()
        {
            ValidateReferences();
            ValidatePrototypeIds();

            _gamePlane.RefreshPlaneData();

            _entityWorld = new EntityWorld();
            _placementService = new PlacementService(_entityWorld, _placedEntitiesRoot);

            _cameraController.Configure(_mainCamera, _gamePlane);
            _placementInputController.Configure(_mainCamera, _gamePlane);
            _radialMenuView.Configure(_radialMenuView.GetComponent<UnityEngine.UIElements.UIDocument>());

            ConfigureRadialMenuItems();
            WireEvents();
        }

        void Update()
        {
            _cameraController.ProcessInput();
        }

        void LateUpdate()
        {
            ProcessWorldClickInput();
        }

        void ProcessWorldClickInput()
        {
            if (IsUiPointerBlocking())
                return;

            if (_cameraController.IsDragging)
                return;

            if (!_placementInputController.TryHandleWorldClick())
                return;

            var request = _placementInputController.PendingRequest;
            if (_radialMenuView.IsOpen)
                _radialMenuView.MoveTo(request);
            else
                _radialMenuView.OpenAt(request);
        }

        bool IsUiPointerBlocking()
        {
            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
                return false;

            var screenPosition = Mouse.current.position.ReadValue();
            return _radialMenuView.ShouldBlockWorldClick(screenPosition);
        }

        void WireEvents()
        {
            _radialMenuView.PlacementConfirmed += HandlePlacementConfirmed;
            _radialMenuView.MenuClosed += HandleMenuClosed;
        }

        void OnDestroy()
        {
            if (_radialMenuView == null)
                return;

            _radialMenuView.PlacementConfirmed -= HandlePlacementConfirmed;
            _radialMenuView.MenuClosed -= HandleMenuClosed;
        }

        void HandlePlacementConfirmed(PlaceablePrototype prototype)
        {
            var request = _radialMenuView.ActiveRequest;
            if (!request.HasPosition || prototype == null)
                return;

            _placementService.Place(prototype, request.WorldPosition);
        }

        void HandleMenuClosed()
        {
        }

        void ConfigureRadialMenuItems()
        {
            var items = new List<RadialMenuItem>
            {
                new("Tower", RadialMenuAction.PlaceDefenseTower, _defenseTowerPrototype),
                new("Intel", RadialMenuAction.PlaceIntelTower, _intelTowerPrototype),
                new("X", RadialMenuAction.Close)
            };

            _radialMenuView.SetItems(items);
        }

        void ValidateReferences()
        {
            if (_mainCamera == null)
                Debug.LogError($"{nameof(MvpGameBootstrap)}: Main Camera reference is missing.", this);

            if (_gamePlane == null)
                Debug.LogError($"{nameof(MvpGameBootstrap)}: GamePlane reference is missing.", this);

            if (_placedEntitiesRoot == null)
                Debug.LogError($"{nameof(MvpGameBootstrap)}: PlacedEntities root reference is missing.", this);

            if (_cameraController == null)
                Debug.LogError($"{nameof(MvpGameBootstrap)}: IsometricCameraController reference is missing.", this);

            if (_placementInputController == null)
                Debug.LogError($"{nameof(MvpGameBootstrap)}: PlacementInputController reference is missing.", this);

            if (_radialMenuView == null)
                Debug.LogError($"{nameof(MvpGameBootstrap)}: RadialMenuView reference is missing.", this);

            if (_defenseTowerPrototype == null)
                Debug.LogError($"{nameof(MvpGameBootstrap)}: Defense tower prototype reference is missing.", this);

            if (_intelTowerPrototype == null)
                Debug.LogError($"{nameof(MvpGameBootstrap)}: Intel tower prototype reference is missing.", this);
        }

        void ValidatePrototypeIds()
        {
            var ids = new HashSet<string>();
            ValidatePrototypeId(_defenseTowerPrototype, ids);
            ValidatePrototypeId(_intelTowerPrototype, ids);
        }

        void ValidatePrototypeId(PlaceablePrototype prototype, HashSet<string> ids)
        {
            if (prototype == null || string.IsNullOrWhiteSpace(prototype.id))
                return;

            if (!ids.Add(prototype.id))
                Debug.LogError($"{nameof(MvpGameBootstrap)}: Duplicate PlaceablePrototype id '{prototype.id}'.", this);
        }

        void OnValidate()
        {
            ValidateReferences();
            ValidatePrototypeIds();
        }
    }
}
