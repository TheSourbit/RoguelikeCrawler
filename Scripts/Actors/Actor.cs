using System.Collections.Generic;

using Godot;

public enum Allegiance
{
  None,
  Player,
  Dungeon,
}

public abstract partial class Actor : Node3D
{
  [Export] public Allegiance Allegiance = Allegiance.None;
  [Export] public int InitialHealth = 1;
  [Export(PropertyHint.Range, "1,20")] public int VisionRange = 10;

  public bool IsActive { get; private set; } = true;
  public bool IsDisposable { get; protected set; } = true;

  public System.Action<Actor> OnActiveChange;

  public int Turns;

  public AbstractDungeonLevel DungeonLevel;
  public Vector2I GridPosition;

  public readonly HashSet<Actor> VisibleActors = [];
  public readonly HashSet<Vector2I> VisibleTiles = [];
  public readonly HashSet<Vector2I> KnownTiles = [];

  public Status Status { get; private set; }

  public virtual void FlowTurns(int turns) { }
  public virtual Action PlanAction() { return null; }

  public override void _Ready()
  {
    Status = new Status(this, InitialHealth);
  }

  protected void Activate()
  {
    IsActive = true;
    OnActiveChange?.Invoke(this);
  }

  protected void Deactivate()
  {
    IsActive = false;
    OnActiveChange?.Invoke(this);
  }

  public virtual int PerformAction(Action action)
  {
    int turns = action switch
    {
      WaitAction wait => PerformWaitAction(wait),
      MoveAction move => PerformMoveAction(move),
      AttackAction attack => PerformAttackAction(attack),
      _ => 100,
    };

    return turns;
  }

  protected virtual int PerformWaitAction(WaitAction action)
  {
    return 100;
  }

  protected virtual int PerformMoveAction(MoveAction action)
  {
    if (!DungeonLevel.IsTileNode(action.TargetPosition))
    {
      return 0;
    }

    int turns = action.ExpectedCost;

    DungeonLevel.MoveActorTo(this, action.TargetPosition);

    UpdateLineOfSight();

    return turns;
  }

  protected virtual int PerformAttackAction(AttackAction action)
  {
    if (DungeonLevel.TryGetActorAt(action.TargetPosition, out Actor actor))
    {
      GD.Print($"Attacking actor: {actor}");
      // TODO: Implement proper weapon handling
      actor.Status.Damage(2);
    }

    return 100;
  }

  public Action GetNextAction()
  {
    UpdateVisibleActors();

    return PlanAction();
  }

  public void UpdateLineOfSight()
  {
    VisibleTiles.Clear();
    Shadowcasting.ComputeFOV(GridPosition, VisionRange, IsBlockingTile, MarkTileVisible);
  }

  public virtual void UpdateVisibleActors()
  {
    // TODO: Can we cache the actors per tile in the AbstractDungeonLevel?
    VisibleActors.Clear();
    foreach (Vector2I tile in VisibleTiles)
    {
      foreach (Actor actor in DungeonLevel.Actors)
      {
        if (actor.GridPosition == tile)
        {
          VisibleActors.Add(actor);
        }
      }
    }
  }

  protected virtual bool IsBlockingTile(Vector2I tile)
  {
    return !DungeonLevel.IsValidTilePosition(tile)
      || DungeonLevel.IsTileWall(tile)
      || (DungeonLevel.TryGetActorAt(tile, out var actor) && actor.IsBlockingLoS());
  }

  protected virtual void MarkTileVisible(Vector2I tile)
  {
    KnownTiles.Add(tile);
    VisibleTiles.Add(tile);
  }

  public virtual bool IsBlockingLoS()
  {
    return false;
  }

  public virtual bool IsBlockingPathing()
  {
    return false;
  }

  public virtual bool IsNeutral(Actor actor)
  {
    return Allegiance == Allegiance.None || actor.Allegiance == Allegiance.None;
  }

  public virtual bool IsFriend(Actor actor)
  {
    return !IsNeutral(actor) && Allegiance == actor.Allegiance;
  }

  public virtual bool IsFoe(Actor actor)
  {
    return !IsNeutral(actor) && Allegiance != actor.Allegiance;
  }
}
