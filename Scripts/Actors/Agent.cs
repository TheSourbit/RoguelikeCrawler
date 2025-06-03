using System.Collections.Generic;

using Godot;

public enum AgentState
{
  Asleep,
  Idle,
  Wandering,
  Alerted,
  Fleeing,
  Hunting,
}

public partial class Agent : Actor
{
  public AgentState State = AgentState.Idle;
  public Actor TargetActor;
  public Vector2I TargetLastKnownPosition;

  protected override int PerformMoveAction(MoveAction action)
  {
    int turns = base.PerformMoveAction(action);
    UpdateVisibleActors();

    if (VisibleActors.Contains(TargetActor))
    {
      TargetLastKnownPosition = TargetActor.GridPosition;
      GD.Print($"Target at {TargetLastKnownPosition}");
    }

    return turns;
  }

  public override Action PlanAction()
  {
    GD.Print($"{State} : {TargetActor} @ {TargetLastKnownPosition}");

    if (TargetActor == null)
    {
      if (AcquireTarget())
      {
        GD.Print($"Saw Target {TargetActor} at {TargetLastKnownPosition}");
        State = AgentState.Hunting;
        // TODO: Should this agent broadcast it's target last known position
      }
      else if (CheckAlertStatus())
      {
        GD.Print($"Alerted agents around");
        State = AgentState.Alerted;
      }
    }

    return State switch
    {
      AgentState.Asleep => Asleep(),
      AgentState.Idle => Idle(),
      AgentState.Wandering => Wandering(),
      AgentState.Alerted => Alerted(),
      AgentState.Fleeing => Fleeing(),
      AgentState.Hunting => Hunting(),
    };
  }

  protected virtual Action Asleep()
  {
    // TODO: Check vicinity (near) to wake up (-> Alerted)
    // TODO: Do nothing for the turn
    return null;
  }

  protected virtual Action Idle()
  {
    // TODO: Take a walk after some turns (-> Wandering)
    // TODO: Go back to sleep after some turns (-> Asleep)
    return null;
  }

  protected virtual Action Wandering()
  {
    if (Gameplay.Random.Randf() < 0.5f)
    {
      Vector2I move = new(
        Gameplay.Random.RandiRange(-1, 1),
        Gameplay.Random.RandiRange(-1, 1)
      );

      return new MoveAction(this, GridPosition + move)
      {
        ExpectedCost = move.Length() > 1 ? 140 : 100,
      };
    }

    // TODO: Move somewhere
    // TODO: Go back to idle after walking (-> Idle)
    return null;
  }

  protected virtual Action Alerted()
  {
    // TODO: Move around looking for a target
    // TODO: Starts fleeing if needed (-> Fleeing)
    // TODO: Chill down after some turns (-> Idle)
    return null;
  }

  protected virtual Action Fleeing()
  {
    // TODO: Flee from Target
    // TODO: Stop fleeing after some turns (-> Alerted)
    return null;
  }

  protected virtual Action Hunting()
  {
    // TODO: Chase & Attack Target
    // TODO: Starts fleeing if needed (-> Fleeing)
    // TODO: Look for target if lost contact (-> Alerted)

    if (VisibleActors.Contains(TargetActor))
    {
      GD.Print($"Seeing target at ${TargetActor.GridPosition}");
      TargetLastKnownPosition = TargetActor.GridPosition;
    }
    else if (GridPosition == TargetLastKnownPosition)
    {
      GD.Print($"Lost Target");

      TargetActor = null;
      State = AgentState.Alerted;

      return Alerted();
    }

    /*
      path.Length == 0 means we can't find path
      path.Length == 1 means we are standing in the target's last known tile
      path.Length == 2 means we are at melee range
      path.Length > 2 means we are at distance of path.Length - 2 tiles
    */
    Vector2[] path = DungeonLevel.Pathing.GetPointPath(GridPosition, TargetLastKnownPosition);

    // TODO: The possible move logic should be elsewhere
    return path.Length > 1
      ? new MoveAction(this, (Vector2I)path[1])
      : null;
  }

  protected virtual bool AcquireTarget()
  {
    if (Allegiance == Allegiance.None)
    {
      return false;
    }

    Allegiance foe = Allegiance == Allegiance.Dungeon
      ? Allegiance.Player
      : Allegiance.Dungeon;

    List<(Actor actor, int distance)> validTargets = [];
    foreach (Actor actor in VisibleActors)
    {
      if (actor.Allegiance == foe)
      {
        validTargets.Add((actor, GetManhattanDistanceTo(actor)));
      }
    }

    if (validTargets.Count == 0)
    {
      return false;
    }

    validTargets.Sort((a, b) => a.distance - b.distance);

    TargetActor = validTargets[0].actor;
    TargetLastKnownPosition = TargetActor.GridPosition;

    return true;
  }

  protected int GetManhattanDistanceTo(Actor actor)
  {
    return Mathf.Abs(actor.GridPosition.X - GridPosition.X) + Mathf.Abs(actor.GridPosition.Y - GridPosition.Y);
  }

  protected virtual bool CheckAlertStatus()
  {
    // TODO: Look around for alerted agents
    return false;
  }

  protected bool HasTarget()
  {
    return TargetActor != null;
  }

  // TODO: Update this to reflect the agent actual blocking state
  public override bool IsBlockingPathing()
  {
    return true;
  }
}
