using Godot;

public class Action(Actor actor)
{
  public Actor Actor = actor;
  public int ExpectedCost = 100;
}

public class WaitAction(Actor actor) : Action(actor) { }

public class MoveAction(Actor actor, Vector2I to) : Action(actor)
{
  public Vector2I TargetPosition = to;
}
