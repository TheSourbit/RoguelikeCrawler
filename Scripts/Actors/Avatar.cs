using Godot;

public partial class Avatar : Actor
{
  public GameplayCamera Camera;

  double MoveInputMaxDelay = 0.06;
  double MoveInputDelay = 0;

  bool IsWaitingForMove = false;

  bool PressedUp = false;
  bool PressedDown = false;
  bool PressedLeft = false;
  bool PressedRight = false;

  protected override int PerformMoveAction(MoveAction action)
  {
    Camera.Recenter();

    return base.PerformMoveAction(action);
  }

  public override Action PlanAction()
  {
    var (move, waitInput) = ProcessInput();
    Vector2I target = GridPosition + move;

    if (waitInput || !DungeonLevel.IsTileNode(target)) return null;

    Vector2[] path = DungeonLevel.Pathing.GetPointPath(GridPosition, target);

    return new MoveAction(this)
    {
      TargetPosition = (Vector2I)path[1],
    };
  }

  // TODO: This should be refactored out of here
  public override void UpdateVisibleActors()
  {
    base.UpdateVisibleActors();

    foreach (Actor actor in DungeonLevel.Actors)
    {
      actor.Visible = false;
    }

    foreach (Actor actor in VisibleActors)
    {
      actor.Visible = true;
    }
  }

  (Vector2I move, bool wait) ProcessInput()
  {
    if (!PressedUp) PressedUp = Input.IsActionJustPressed("MoveUp");
    if (!PressedDown) PressedDown = Input.IsActionJustPressed("MoveDown");
    if (!PressedLeft) PressedLeft = Input.IsActionJustPressed("MoveLeft");
    if (!PressedRight) PressedRight = Input.IsActionJustPressed("MoveRight");

    if (
      Input.IsActionJustReleased("MoveUp") ||
      Input.IsActionJustReleased("MoveDown") ||
      Input.IsActionJustReleased("MoveLeft") ||
      Input.IsActionJustReleased("MoveRight")
    )
    {
      return ExecuteMove();
    }

    if (IsWaitingForMove)
    {
      MoveInputDelay += GetProcessDeltaTime();
      if (MoveInputDelay >= MoveInputMaxDelay)
      {
        return ExecuteMove();
      }
    }
    else
    {
      IsWaitingForMove = PressedUp || PressedDown || PressedLeft || PressedRight;
    }

    return (Vector2I.Zero, true);
  }

  (Vector2I move, bool wait) ExecuteMove()
  {
    if (!IsWaitingForMove)
    {
      return (Vector2I.Zero, true);
    }

    if ((PressedLeft && PressedRight) || (PressedDown && PressedUp))
    {
      ResetMove();
      return (Vector2I.Zero, true);
    }

    int horizontal = PressedLeft ? -1 : PressedRight ? 1 : 0;
    int vertical = PressedUp ? -1 : PressedDown ? 1 : 0;

    ResetMove();

    return (new(horizontal, vertical), false);
  }

  void ResetMove()
  {
    IsWaitingForMove = false;
    MoveInputDelay = 0;

    PressedUp = false;
    PressedDown = false;
    PressedLeft = false;
    PressedRight = false;
  }
}
