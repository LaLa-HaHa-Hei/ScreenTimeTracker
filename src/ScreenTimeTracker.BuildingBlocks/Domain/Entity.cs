using System.ComponentModel.DataAnnotations;

namespace ScreenTimeTracker.BuildingBlocks.Domain;

public abstract class Entity
{
    [Key]
    public Guid Id { get; protected set; }

    [Obsolete("a", true)]
    public Entity() { }

    public Entity(Guid id) => Id = id;
}
