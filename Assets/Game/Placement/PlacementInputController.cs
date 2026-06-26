using Game.World;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Placement
{
    public class PlacementInputController : MonoBehaviour
    {
        [SerializeField] UnityEngine.Camera _camera;
        [SerializeField] GamePlane _gamePlane;

        PlacementRequest _pendingRequest;
        bool _enabled = true;

        public PlacementRequest PendingRequest => _pendingRequest;

        public void Configure(UnityEngine.Camera camera, GamePlane gamePlane)
        {
            _camera = camera;
            _gamePlane = gamePlane;
        }

        public void SetEnabled(bool enabled)
        {
            _enabled = enabled;
        }

        public bool TryHandleWorldClick()
        {
            if (!_enabled || Mouse.current == null)
                return false;

            if (!Mouse.current.leftButton.wasPressedThisFrame)
                return false;

            if (!TryRaycastPlane(out var hitPoint))
                return false;

            _pendingRequest.Set(hitPoint);
            return true;
        }

        public bool TryRaycastPlane(out Vector3 hitPoint)
        {
            hitPoint = default;
            if (_camera == null || _gamePlane == null)
                return false;

            var mousePosition = Mouse.current.position.ReadValue();
            var ray = _camera.ScreenPointToRay(mousePosition);
            return _gamePlane.TryRaycast(ray, out hitPoint);
        }
    }
}
