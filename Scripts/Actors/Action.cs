using Godot;

public interface Action;

public readonly record struct WaitAction() : Action;
public readonly record struct MoveAction(Vector2I TargetPosition) : Action;
public readonly record struct AttackAction(Vector2I TargetPosition) : Action;
