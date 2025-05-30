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

  protected override void Populate()
  {
    AddDoor();

    // for (int i = 0; i < 10; i++)
    // {
      AddAgent();
    // }
  }

  void UseNode(Node node)
  {
    node.Type = NodeType.Room;
    Nodes.Add(node);
    AssignTilesToWalledRoom(node);
  }

  protected override void AddMissingWalls()
  {
    base.AddMissingWalls();
    Tiles[GetTileIndex(7, 3)].SetWall();
    Pathing.SetPointSolid(new(7, 3), true);
  }

  void AddDoor()
  {
    Door door = Assets.DoorScene.Instantiate<Door>();

    door.GridPosition = new Vector2I(5, 2);
    door.Position = Gameplay.GridToWorld(door.GridPosition);
    door.RotateY(Mathf.DegToRad(90));

    InsertActor(door);
  }

  void AddAgent()
  {
    Agent agent = Assets.AgentScene.Instantiate<Agent>();
    agent.State = AgentState.Wandering;

    Node node = Rooms[2];
    // Node node = Dungeon.CurrentLevel.Rooms[Random.RandiRange(0, Dungeon.CurrentLevel.Rooms.Count - 1)];
    agent.GridPosition = node.Position + new Vector2I(
      Gameplay.Random.RandiRange(0, node.Size.X - 1),
      Gameplay.Random.RandiRange(0, node.Size.Y - 1)
    );
    agent.Position = Gameplay.GridToWorld(agent.GridPosition);

    InsertActor(agent);
  }
}
