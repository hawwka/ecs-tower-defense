using Game.Placement;

namespace Game.UI
{
    public enum RadialMenuAction
    {
        PlaceDefenseTower,
        PlaceIntelTower,
        Close
    }

    public struct RadialMenuItem
    {
        public string Label;
        public RadialMenuAction Action;
        public PlaceablePrototype Prototype;

        public RadialMenuItem(string label, RadialMenuAction action, PlaceablePrototype prototype = null)
        {
            Label = label;
            Action = action;
            Prototype = prototype;
        }
    }
}
