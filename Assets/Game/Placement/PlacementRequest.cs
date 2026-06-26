using UnityEngine;

namespace Game.Placement
{
    public struct PlacementRequest
    {
        public Vector3 WorldPosition;
        public bool HasPosition;

        public void Set(Vector3 worldPosition)
        {
            WorldPosition = worldPosition;
            HasPosition = true;
        }

        public void Clear()
        {
            HasPosition = false;
        }
    }
}
