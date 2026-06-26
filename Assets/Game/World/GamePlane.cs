using UnityEngine;

namespace Game.World
{
    public class GamePlane : MonoBehaviour
    {
        Plane _plane;
        Bounds _bounds;
        Collider _collider;

        public Bounds Bounds => _bounds;

        void Awake()
        {
            RefreshPlaneData();
        }

        void OnValidate()
        {
            RefreshPlaneData();
        }

        public void RefreshPlaneData()
        {
            var transform = this.transform;
            _plane = new Plane(transform.up, transform.position);
            _collider = GetComponent<Collider>();

            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                _bounds = renderer.bounds;
                EnsureBoundsHeight();
                return;
            }

            if (_collider != null)
            {
                _bounds = _collider.bounds;
                EnsureBoundsHeight();
                return;
            }

            var lossyScale = transform.lossyScale;
            var center = transform.position;
            var size = new Vector3(Mathf.Abs(lossyScale.x), 0.01f, Mathf.Abs(lossyScale.z));
            _bounds = new Bounds(center, size);
        }

        void EnsureBoundsHeight()
        {
            if (_bounds.size.y > 0.001f)
                return;

            var size = _bounds.size;
            size.y = 0.01f;
            _bounds.size = size;
        }

        public bool TryRaycast(Ray ray, out Vector3 hitPoint)
        {
            if (!TryRaycastUnbounded(ray, out hitPoint))
                return false;

            return Contains(hitPoint);
        }

        public bool TryRaycastUnbounded(Ray ray, out Vector3 hitPoint)
        {
            hitPoint = default;

            if (_collider != null && _collider.Raycast(ray, out var hit, 1000f))
            {
                hitPoint = hit.point;
                return true;
            }

            if (!_plane.Raycast(ray, out var distance))
                return false;

            hitPoint = ray.GetPoint(distance);
            return true;
        }

        public bool Contains(Vector3 worldPoint)
        {
            var point = worldPoint;
            point.y = _bounds.center.y;
            return _bounds.Contains(point);
        }

        public Vector3 ClampToBounds(Vector3 worldPoint)
        {
            var min = _bounds.min;
            var max = _bounds.max;
            return new Vector3(
                Mathf.Clamp(worldPoint.x, min.x, max.x),
                worldPoint.y,
                Mathf.Clamp(worldPoint.z, min.z, max.z));
        }
    }
}
