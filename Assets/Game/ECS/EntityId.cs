using System;

namespace Game.ECS
{
    public readonly struct EntityId : IEquatable<EntityId>
    {
        public int Value { get; }

        public EntityId(int value)
        {
            Value = value;
        }

        public bool IsValid => Value > 0;

        public bool Equals(EntityId other) => Value == other.Value;

        public override bool Equals(object obj) => obj is EntityId other && Equals(other);

        public override int GetHashCode() => Value;

        public static bool operator ==(EntityId left, EntityId right) => left.Equals(right);

        public static bool operator !=(EntityId left, EntityId right) => !left.Equals(right);

        public override string ToString() => $"Entity({Value})";
    }
}
