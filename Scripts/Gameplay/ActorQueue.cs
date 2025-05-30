using System.Collections.Generic;

public class ActorQueue
{
  readonly LinkedList<Actor> Queue;

  public ActorQueue()
  {
    Queue = new LinkedList<Actor>();
  }

  public void Enqueue(Actor actor, int turns = 0)
  {
    actor.Turns = turns;

    if (Queue.Count == 0)
    {
      Queue.AddLast(actor);
      return;
    }

    LinkedListNode<Actor> current = Queue.First;
    while (current != null)
    {
      if (current.Value.Turns > turns)
      {
        break;
      }
      current = current.Next;
    }

    if (current != null)
    {
      Queue.AddBefore(current, actor);
      return;
    }

    Queue.AddLast(actor);
  }

  // TODO: Implement action chunking & frame spreading
  public void ProcessQueue()
  {
    while (Queue.First != null)
    {
      Actor actor = Queue.First.Value;
      int flowTurns = actor.Turns;

      if (flowTurns > 0)
      {
        foreach (Actor queuedActor in Queue)
        {
          queuedActor.Turns -= flowTurns;
          queuedActor.FlowTurns(flowTurns);
        }
      }

      Action action = actor.GetNextAction();
      if (actor is Avatar && action is null)
      {
        // Wait for player action
        return;
      }

      int turns = actor.PerformAction(action);
      actor.UpdateVisibleActors();

      Queue.RemoveFirst();
      Enqueue(actor, turns);

      Gameplay.CurrentLevel.Update();
    }
  }
}
