using Game.World;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Camera
{
    public class IsometricCameraController : MonoBehaviour
    {
        [SerializeField] UnityEngine.Camera _camera;
        [SerializeField] GamePlane _gamePlane;
        [SerializeField] float _dragSensitivity = 1f;
        [SerializeField] float _moveSmoothTime = 0.08f;
        [SerializeField] float _zoomSensitivity = 0.01f;
        [SerializeField] float _minOrthographicSize = 3f;
        [SerializeField] float _maxOrthographicSize = 18f;
        [SerializeField] float _minCameraDistance = 5f;
        [SerializeField] float _maxCameraDistance = 40f;

        Vector3 _targetFocusPoint;
        Vector3 _cameraOffset;
        Vector3 _smoothVelocity;
        float _targetOrthographicSize;
        float _orthographicSizeVelocity;
        bool _isDragging;

        public bool IsDragging => _isDragging;

        public void Configure(UnityEngine.Camera camera, GamePlane gamePlane)
        {
            _camera = camera;
            _gamePlane = gamePlane;
            InitializeFocusPoint();
        }

        void Start()
        {
            if (_gamePlane != null)
                InitializeFocusPoint();
        }

        void InitializeFocusPoint()
        {
            if (_gamePlane == null)
                return;

            var focusPoint = _gamePlane.Bounds.center;
            focusPoint.y = _gamePlane.transform.position.y;
            _targetFocusPoint = focusPoint;
            _cameraOffset = transform.position - focusPoint;
            _smoothVelocity = Vector3.zero;

            if (_camera != null)
            {
                _targetOrthographicSize = Mathf.Clamp(_camera.orthographicSize, _minOrthographicSize, _maxOrthographicSize);
                _orthographicSizeVelocity = 0f;
            }
        }

        public void ProcessInput()
        {
            if (_camera == null || _gamePlane == null || Mouse.current == null)
                return;

            ProcessZoom();

            var dragStartedThisFrame = Mouse.current.rightButton.wasPressedThisFrame;
            if (dragStartedThisFrame)
                BeginDrag();

            if (_isDragging && Mouse.current.rightButton.isPressed && !dragStartedThisFrame)
                ContinueDrag();

            if (_isDragging && Mouse.current.rightButton.wasReleasedThisFrame)
                EndDrag();
        }

        void LateUpdate()
        {
            if (_camera == null || _gamePlane == null)
                return;

            var targetPosition = _targetFocusPoint + _cameraOffset;
            if (_moveSmoothTime <= 0f)
            {
                transform.position = targetPosition;
                ApplyInstantZoom();
                return;
            }

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _smoothVelocity, _moveSmoothTime);
            ApplySmoothZoom();
        }

        void BeginDrag()
        {
            if (!TryGetWorldPointUnderCursor(out _))
                return;

            _isDragging = true;
        }

        void ContinueDrag()
        {
            var mouseDelta = Mouse.current.delta.ReadValue();
            if (mouseDelta.sqrMagnitude <= Mathf.Epsilon)
                return;

            var mousePosition = Mouse.current.position.ReadValue();
            var previousMousePosition = mousePosition - mouseDelta;
            if (!TryGetWorldPoint(mousePosition, out var worldPoint))
                return;

            if (!TryGetWorldPoint(previousMousePosition, out var previousWorldPoint))
                return;

            var delta = worldPoint - previousWorldPoint;
            delta.y = 0f;

            _targetFocusPoint -= delta * _dragSensitivity;
            _targetFocusPoint = _gamePlane.ClampToBounds(_targetFocusPoint);
        }

        void EndDrag()
        {
            _isDragging = false;
        }

        void ProcessZoom()
        {
            var scrollDelta = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(scrollDelta) <= Mathf.Epsilon)
                return;

            var zoomAmount = scrollDelta * _zoomSensitivity;
            if (_camera.orthographic)
            {
                _targetOrthographicSize = Mathf.Clamp(
                    _targetOrthographicSize - zoomAmount,
                    _minOrthographicSize,
                    _maxOrthographicSize);
                return;
            }

            var distance = _cameraOffset.magnitude;
            if (distance <= Mathf.Epsilon)
                return;

            var targetDistance = Mathf.Clamp(distance - zoomAmount, _minCameraDistance, _maxCameraDistance);
            _cameraOffset = _cameraOffset.normalized * targetDistance;
        }

        void ApplyInstantZoom()
        {
            if (!_camera.orthographic)
                return;

            _camera.orthographicSize = _targetOrthographicSize;
        }

        void ApplySmoothZoom()
        {
            if (!_camera.orthographic)
                return;

            _camera.orthographicSize = Mathf.SmoothDamp(
                _camera.orthographicSize,
                _targetOrthographicSize,
                ref _orthographicSizeVelocity,
                _moveSmoothTime);
        }

        bool TryGetWorldPointUnderCursor(out Vector3 worldPoint)
        {
            var mousePosition = Mouse.current.position.ReadValue();
            return TryGetWorldPoint(mousePosition, out worldPoint);
        }

        bool TryGetWorldPoint(Vector2 mousePosition, out Vector3 worldPoint)
        {
            worldPoint = default;
            var ray = _camera.ScreenPointToRay(mousePosition);
            return _gamePlane.TryRaycastUnbounded(ray, out worldPoint);
        }
    }
}
