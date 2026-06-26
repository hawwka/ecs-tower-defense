using System.Collections.Generic;
using Game.ECS;
using Game.ECS.Components;
using UnityEngine;

namespace Game.Placement
{
    public class PlacementService
    {
        static readonly string[] WaveDirections = { "North", "East", "South", "West" };

        readonly EntityWorld _world;
        readonly Transform _placedEntitiesRoot;

        public PlacementService(EntityWorld world, Transform placedEntitiesRoot)
        {
            _world = world;
            _placedEntitiesRoot = placedEntitiesRoot;
        }

        public ECS.EntityId Place(PlaceablePrototype prototype, Vector3 worldPosition)
        {
            var entityId = _world.CreateEntity();
            var rotation = Quaternion.identity;
            var scale = Vector3.one;

            _world.AddComponent(entityId, new WorldTransformComponent(worldPosition, rotation, scale));
            _world.AddComponent(entityId, new PrototypeComponent(prototype.id));
            _world.AddComponent(entityId, new PlaceableComponent());

            var view = CreateView(prototype, worldPosition, rotation, scale);
            _world.AddComponent(entityId, new ViewComponent(view));

            if (prototype.placementBehavior == PlaceableBehavior.LogNextWaveDirection)
                LogNextWaveDirection();

            return entityId;
        }

        public void DestroyEntity(ECS.EntityId entityId)
        {
            if (_world.TryGet(entityId, out ViewComponent viewComponent) && viewComponent.View != null)
                Object.Destroy(viewComponent.View);

            _world.DestroyEntity(entityId);
        }

        GameObject CreateView(PlaceablePrototype prototype, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            GameObject view;
            if (prototype.prefab != null)
            {
                view = Object.Instantiate(prototype.prefab, position, rotation, _placedEntitiesRoot);
            }
            else
            {
                view = GameObject.CreatePrimitive(PrimitiveType.Cube);
                view.transform.SetParent(_placedEntitiesRoot, false);
                view.transform.SetPositionAndRotation(position, rotation);
                view.transform.localScale = scale;

                var renderer = view.GetComponent<Renderer>();
                if (renderer != null)
                {
                    var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    material.color = prototype.color;
                    renderer.sharedMaterial = material;
                }
            }

            view.name = string.IsNullOrEmpty(prototype.displayName) ? prototype.id : prototype.displayName;
            return view;
        }

        static void LogNextWaveDirection()
        {
            var direction = WaveDirections[Random.Range(0, WaveDirections.Length)];
            Debug.Log($"Intel tower placed. Next wave expected from: {direction}");
        }
    }
}
