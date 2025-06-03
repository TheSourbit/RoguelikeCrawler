using Godot;

class ThreeRoomsLevel(int size) : AbstractDungeonLevel((size + 1) * 3 + 1, size + 2)
{
  protected override void GenerateNodes()
  {
    int id = 0;

    Node firstRoom = new Node(id++, 1, 1, size, size);
    Node middleRoom = new Node(id++, (size + 1) * 1 + 1, 1, size, size);
    Node lastRoom = new Node(id++, (size + 1) * 2 + 1, 1, size, size);

    firstRoom.Entry = true;
    lastRoom.Exit = true;

    UseNode(firstRoom);
    UseNode(middleRoom);
    UseNode(lastRoom);
  }

  protected override void AddMissingWalls()
  {
    base.AddMissingWalls();
    InsertWall(7, 3);
    InsertWall(8, 2);
  }

  protected override void Populate()
  {
    AddDoor();
    AddAgent();
  }

  void UseNode(Node node)
  {
    node.Type = NodeType.Room;
    Nodes.Add(node);
    AssignTilesToWalledRoom(node);
  }

  void AddDoor()
  {
    Door door = Assets.DoorScene.Instantiate<Door>();

    MoveActorTo(door, 5, 2);
    door.RotateY(Mathf.DegToRad(90));

    InsertActor(door);
  }

  void AddAgent()
  {
    Agent agent = Assets.AgentScene.Instantiate<Agent>();
    agent.State = AgentState.Wandering;

    Node node = Rooms[2];
    // Node node = Dungeon.CurrentLevel.Rooms[Random.RandiRange(0, Dungeon.CurrentLevel.Rooms.Count - 1)];
    MoveActorTo(agent, node.Position + new Vector2I(
      Gameplay.Random.RandiRange(0, node.Size.X - 1),
      Gameplay.Random.RandiRange(0, node.Size.Y - 1)
    ));

    InsertActor(agent);
  }
}
