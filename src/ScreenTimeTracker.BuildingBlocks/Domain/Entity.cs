using System.ComponentModel.DataAnnotations;

namespace ScreenTimeTracker.BuildingBlocks.Domain;

public abstract class Entity
{
    [Key]
    public Guid Id { get; protected set; }

    [Obsolete("This constructor is reserved for framework use only. Do not call directly.", true)]
    public Entity() { }

    public Entity(Guid id) => Id = id;
}
