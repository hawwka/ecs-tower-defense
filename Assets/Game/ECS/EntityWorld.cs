using System;
using System.Collections.Generic;

namespace Game.ECS
{
    public class EntityWorld
    {
        int _nextEntityId = 1;
        readonly Dictionary<Type, Dictionary<EntityId, object>> _components = new();

        public EntityId CreateEntity()
        {
            return new EntityId(_nextEntityId++);
        }

        public void AddComponent<T>(EntityId entityId, T component)
        {
            if (!entityId.IsValid)
                throw new ArgumentException("Invalid entity id.", nameof(entityId));

            var type = typeof(T);
            if (!_components.TryGetValue(type, out var store))
            {
                store = new Dictionary<EntityId, object>();
                _components[type] = store;
            }

            store[entityId] = component;
        }

        public T Get<T>(EntityId entityId)
        {
            if (!TryGet(entityId, out T component))
                throw new KeyNotFoundException($"Entity {entityId} has no component {typeof(T).Name}.");

            return component;
        }

        public bool Has<T>(EntityId entityId)
        {
            return _components.TryGetValue(typeof(T), out var store) && store.ContainsKey(entityId);
        }

        public bool TryGet<T>(EntityId entityId, out T component)
        {
            component = default;
            if (!_components.TryGetValue(typeof(T), out var store))
                return false;

            if (!store.TryGetValue(entityId, out var value))
                return false;

            component = (T)value;
            return true;
        }

        public bool Remove<T>(EntityId entityId)
        {
            if (!_components.TryGetValue(typeof(T), out var store))
                return false;

            return store.Remove(entityId);
        }

        public void DestroyEntity(EntityId entityId)
        {
            if (!entityId.IsValid)
                return;

            foreach (var store in _components.Values)
                store.Remove(entityId);
        }
    }
}
