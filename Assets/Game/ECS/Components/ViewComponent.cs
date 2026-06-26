using UnityEngine;

namespace Game.ECS.Components
{
    public struct ViewComponent
    {
        public GameObject View;

        public ViewComponent(GameObject view)
        {
            View = view;
        }
    }
}
