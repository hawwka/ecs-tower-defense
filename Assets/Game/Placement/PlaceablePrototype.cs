using UnityEngine;

namespace Game.Placement
{
    public enum PlaceableKind
    {
        DefenseTower,
        IntelTower
    }

    public enum PlaceableBehavior
    {
        None,
        LogNextWaveDirection
    }

    [CreateAssetMenu(fileName = "PlaceablePrototype", menuName = "Game/Placeable Prototype")]
    public class PlaceablePrototype : ScriptableObject
    {
        public string id;
        public string displayName;
        public PlaceableKind kind;
        public GameObject prefab;
        public Color color = Color.gray;
        public PlaceableBehavior placementBehavior;
    }
}
