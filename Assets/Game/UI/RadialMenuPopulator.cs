using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    public static class RadialMenuPopulator
    {
        public static void Populate(
            VisualElement host,
            IReadOnlyList<RadialMenuItem> items,
            Action<RadialMenuItem> onItemClicked)
        {
            if (host == null)
                return;

            host.Clear();

            if (items == null || items.Count == 0 || onItemClicked == null)
                return;

            var stepDegrees = 360f / items.Count;

            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var angleDegrees = RadialMenuLayout.StartAngleDegrees + stepDegrees * i;
                var button = CreateButton(item, onItemClicked);
                PlaceOnRing(button, angleDegrees);
                host.Add(button);
            }
        }

        static Button CreateButton(RadialMenuItem item, Action<RadialMenuItem> onItemClicked)
        {
            var button = new Button { text = item.Label };
            button.AddToClassList("radial-menu-button");

            if (item.Action == RadialMenuAction.Close)
                button.AddToClassList("radial-menu-button-close");

            button.clicked += () => onItemClicked(item);
            return button;
        }

        static void PlaceOnRing(VisualElement element, float angleDegrees)
        {
            var angle = angleDegrees * Mathf.Deg2Rad;
            var center = RadialMenuLayout.PanelPixels * 0.5f;
            var half = RadialMenuLayout.ButtonSizePixels * 0.5f;
            var x = center + Mathf.Sin(angle) * RadialMenuLayout.ButtonOrbitRadiusPixels - half;
            var y = center - Mathf.Cos(angle) * RadialMenuLayout.ButtonOrbitRadiusPixels - half;

            element.style.position = Position.Absolute;
            element.style.left = x;
            element.style.top = y;
        }
    }
}
