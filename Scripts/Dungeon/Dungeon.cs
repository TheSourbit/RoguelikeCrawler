using System.Collections.Generic;

using Godot;

public partial class Dungeon : Node3D
{
  public MeshLibrary VisibleMeshLibrary;
  public MeshLibrary KnownMeshLibrary;

  public AbstractDungeonLevel CurrentLevel { get; private set; }
  public AStarGrid2D Pathing { get => CurrentLevel.Pathing; }

  GridMap VisibleGridMap;
  GridMap KnownGridMap;

  protected readonly List<AbstractDungeonLevel> Levels = [];

  public void Generate()
  {
    CreateLevels();
    CreateGridMaps();
  }

  void CreateLevels()
  {
    // TODO: Generalize this!
    // CurrentLevel = new ForestLevel(5);
    CurrentLevel = new ThreeRoomsLevel(4);
    CurrentLevel.Generate();

    Levels.Add(CurrentLevel);
  }

  void CreateGridMaps()
  {
    VisibleGridMap = new() { MeshLibrary = VisibleMeshLibrary };
    KnownGridMap = new() { MeshLibrary = KnownMeshLibrary };

    VisibleGridMap.CellSize = Vector3.One;
    VisibleGridMap.CellCenterY = false;

    KnownGridMap.CellSize = Vector3.One;
    KnownGridMap.CellCenterY = false;

    AddChild(VisibleGridMap);
    AddChild(KnownGridMap);
  }

  public void DrawDungeon(Actor asActor = null)
  {
    asActor ??= Gameplay.Avatar;

    for (int x = 0; x < CurrentLevel.Region.Size.X; x++)
    {
      for (int y = 0; y < CurrentLevel.Region.Size.Y; y++)
      {
        Vector2I tile = new(x, y);

        if (!asActor.KnownTiles.Contains(tile))
        {
          // HACK: Render everything
          continue;
        }

        Vector3I position = new(x, 0, y);
        TileData data = CurrentLevel.GetTileData(tile);

        // HACK: Colorcoding the ground tiles
        // Node node = CurrentLevel.GetNode(tile);
        // int item = data.Type == TileType.Open
        //   ? (int)node.Type + 3
        //   : data.Model
        // ;
        int item = data.Model;

        // HACK: Render everything
        // VisibleGridMap.SetCellItem(position, item, data.Orientation);
        var (set, unset) = asActor.VisibleTiles.Contains(tile)
          ? (VisibleGridMap, KnownGridMap)
          : (KnownGridMap, VisibleGridMap);

        set.SetCellItem(position, item, data.Orientation);
        unset.SetCellItem(position, (int)GridMap.InvalidCellItem);
      }
    }

    // foreach (Segment segment in CurrentLevel.Graph)
    // {
    //   Draw3D.Line(new (segment.A.X, 1, segment.A.Y), new (segment.B.X, 1, segment.B.Y));
    // }

    // foreach (Segment segment in CurrentLevel.Connections)
    // {
    //   Draw3D.Line(new (segment.A.X, 1, segment.A.Y), new (segment.B.X, 1, segment.B.Y), Color.FromHtml("ff0000"));
    // }
  }

  public override void _Process(double delta)
  {
    CurrentLevel.ActorQueue.ProcessQueue();
  }
}
