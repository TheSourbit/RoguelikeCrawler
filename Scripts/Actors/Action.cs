using Godot;

// TODO: Is it possible to exchange `record` for `record struct`?

public record Action(Actor Actor, int ExpectedCost = 100);
public record WaitAction(Actor Actor) : Action(Actor);
public record MoveAction(Actor Actor, Vector2I TargetPosition) : Action(Actor);
public record AttackAction(Actor Actor, Vector2I TargetPosition) : Action(Actor);
