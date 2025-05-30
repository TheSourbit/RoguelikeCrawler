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
  [Export(PropertyHint.Range, "1,20")] public int VisionRange = 10;

  public int Turns;

  public AbstractDungeonLevel DungeonLevel;
  public Vector2I GridPosition;

  public readonly HashSet<Actor> VisibleActors = [];
  public readonly HashSet<Vector2I> VisibleTiles = [];
  public readonly HashSet<Vector2I> KnownTiles = [];

  public virtual void FlowTurns(int turns) { }
  public virtual Action PlanAction() { return null; }

  public virtual int PerformAction(Action action)
  {
    int turns = action switch
    {
      WaitAction wait => PerformWaitAction(wait),
      MoveAction move => PerformMoveAction(move),
      _ => 100,
    };

    return turns;
  }

  protected virtual int PerformWaitAction(Action action)
  {
    return 100;
  }

  protected virtual int PerformMoveAction(MoveAction action)
  {
    if (!DungeonLevel.IsTileNode(action.TargetPosition))
    {
      return 0;
    }

    int turns = action.ExpectedCost > 0
      ? action.ExpectedCost
      : GridPosition.DistanceSquaredTo(action.TargetPosition) > 1 ? 140 : 100;

    GridPosition = action.TargetPosition;

    UpdateLineOfSight();

    return turns;
  }

  protected virtual int PerformVoidAction(Action action)
  {
    return 0;
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
}
