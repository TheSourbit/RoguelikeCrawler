using System;
using System.Collections.Generic;

using Godot;

public enum TileType
{
  Void,
  Node,
  Wall,
}

public class TileData
{
  public static readonly int Invalid = -1;

  public TileType Type = TileType.Void;
  public Vector2I Position;
  public Node Node = null;
  public int Model = Invalid;
  public int Orientation = 0;

  // Orientations
  //      F  U - Looking Down
  //  0: +Z +Y --- Down
  //  1: +Z -X
  //  2: +Z -Y
  //  3: +Z +X
  //  4: -Y +Z
  //  5: +X +Z
  //  6: +Y +Z
  //  7: -X +Z
  //  8: -Z -Y
  //  9: -Z +X
  // 10: -Z +Y --- Up
  // 11: -Z -X
  // 12: +Y -Z
  // 13: -X -Z
  // 14: -Y -Z
  // 15: +X -Z
  // 16: +X +Y --- Right
  // 17: +Y -X
  // 18: -X -Y
  // 19: -Y +X
  // 20: +X -Y
  // 21: +Y +X
  // 22: -X +Y --- Left
  // 23: -Y -X

  public void SetWall()
  {
    Type = TileType.Wall;
    Model = Invalid;
  }

  public void SetNode(Node node)
  {
    Type = TileType.Node;
    Node = node;
    Model = Invalid;
  }

  public void Reset()
  {
    Type = TileType.Void;
    Node = null;
    Model = Invalid;
    Orientation = 0;
  }

  public override string ToString()
  {
    return $"[TileData {Type} {Position} | node: {Node}, model: {Model}, orientation: {Orientation}]";
  }
}

public abstract class AbstractDungeonLevel
{
  protected float AditionalConnections = 0.15f;

  protected AStarGrid2D CorridorPathing;

  protected readonly TileData[] Tiles;

  public readonly Rect2I Region;
  public readonly List<Node> Nodes;
  public readonly List<Segment> Graph;
  public readonly List<Segment> Connections;

  public readonly ActorQueue ActorQueue;
  public readonly List<Actor> Actors;
  public readonly AStarGrid2D Pathing;

  public Node EntryRoom { get; private set; }
  public Node ExitRoom { get; private set; }
  public List<Node> Rooms { get; private set; }
  public List<Node> DeadEnds { get; private set; }

  protected AbstractDungeonLevel(int width, int height)
  {
    Region = new Rect2I(0, 0, width, height);

    ActorQueue = new();

    Nodes = [];
    Graph = [];
    Connections = [];

    Tiles = new TileData[Region.Area];
    for (int i = 0; i < Tiles.Length; i++)
    {
      Tiles[i] = new();
    }

    Actors = [];
    Pathing = new()
    {
      Region = Region,
      DiagonalMode = AStarGrid2D.DiagonalModeEnum.AtLeastOneWalkable,
      DefaultComputeHeuristic = AStarGrid2D.Heuristic.Octile,
      DefaultEstimateHeuristic = AStarGrid2D.Heuristic.Octile
    };

    Pathing.Update();
    Pathing.FillSolidRegion(Region);
  }

  public Node GetNode(Vector2I point)
  {
    return GetNode(point.X, point.Y);
  }

  public Node GetNode(int x, int y)
  {
    return IsValidTilePosition(x, y) ? GetTileData(x, y).Node : null;
  }

  public Node GetNodeById(int id)
  {
    return id >= 0 ? Nodes.Find(node => node.Id == id) : null;
  }

  public TileData GetTileData(Vector2I point)
  {
    return GetTileData(point.X, point.Y);
  }

  public TileData GetTileData(int x, int y)
  {
    return IsValidTilePosition(x, y) ? Tiles[GetTileIndex(x, y)] : null;
  }

  protected int GetTileIndex(Vector2I point)
  {
    return GetTileIndex(point.X, point.Y);
  }

  protected int GetTileIndex(int x, int y)
  {
    return IsValidTilePosition(x, y) ? (x * Region.Size.Y + y) : TileData.Invalid;
  }

  public bool IsValidTilePosition(Vector2I point)
  {
    return IsValidTilePosition(point.X, point.Y);
  }

  public bool IsValidTilePosition(int x, int y)
  {
    return !(x < 0 || y < 0 || x >= Region.Size.X || y >= Region.Size.Y);
  }

  public bool IsTileVoid(Vector2I point)
  {
    return IsTileVoid(point.X, point.Y);
  }

  public bool IsTileVoid(int x, int y)
  {
    return GetTileData(x, y).Type == TileType.Void;
  }

  public bool IsTileWall(Vector2I point)
  {
    return IsTileWall(point.X, point.Y);
  }

  public bool IsTileWall(int x, int y)
  {
    return GetTileData(x, y).Type == TileType.Wall;
  }

  public bool IsTileNode(Vector2I point)
  {
    return IsTileNode(point.X, point.Y);
  }

  public bool IsTileNode(int x, int y)
  {
    return GetTileData(x, y).Type == TileType.Node;
  }

  public bool IsRegionEmpty(Rect2I region)
  {
    for (int x = 0; x < region.Size.X; x++)
    {
      for (int y = 0; y < region.Size.Y; y++)
      {
        if (!IsTileVoid(x + region.Position.X, y + region.Position.Y))
        {
          return false;
        }
      }
    }

    return true;
  }

  public virtual void Update()
  {
    foreach (Actor actor in Actors)
    {
      actor.Position = Gameplay.GridToWorld(actor.GridPosition);
    }

    Gameplay.Dungeon.DrawDungeon();
  }

  public void Generate()
  {
    GenerateNodes();
    ConnectNodes();
    MarkDeadEnds();
    GenerateCorridors();
    Prune();
    UpdatePathing();
    AddMissingWalls();
    CacheRooms();
    UpdateTilesData();
    Populate();
  }

  protected abstract void GenerateNodes();

  protected virtual void Populate() { }

  protected virtual void ConnectNodes()
  {
    List<Vector2> siteList = [];
    foreach (var node in Nodes)
    {
      if (
        node.Position.X <= 0 ||
        node.Position.Y <= 0 ||
        node.Position.X + node.Size.X >= Region.Size.X ||
        node.Position.Y + node.Size.Y >= Region.Size.Y
      )
      {
        GD.PushWarning($"Node {node} is out-of-bounds");
      }

      if (node.Type == NodeType.Room)
      {
        // The "new Vector2(rand, rand)" is here to fudge the room position (slightly) to avoid colinear rooms in the Delaunay triangulation
        siteList.Add(node.Midpoint + new Vector2(
          Gameplay.Random.Randf() * 0.1f,
          Gameplay.Random.Randf() * 0.1f
        ));
      }
    }

    var sites = siteList.ToArray();
    var graph = Geometry2D.TriangulateDelaunay(sites);

    Graph.AddRange(GenerateGraphSegments(sites, graph));
    Connections.AddRange(GenerateMST(sites, graph));

    int retries = 100;
    int aditional = (int)(Connections.Count * AditionalConnections);
    while (aditional > 0)
    {
      Segment segment = Graph[Gameplay.Random.RandiRange(0, Graph.Count - 1)];
      if (Connections.FindIndex(s => s.Equivalent(segment)) == -1)
      {
        Connections.Add(segment);
        aditional--;
      }

      if (retries-- < 0)
      {
        break;
      }
    }
  }

  protected virtual void MarkDeadEnds()
  {
    List<Vector2I> deadEnds = [];
    foreach (var segmentA in Connections)
    {
      var aIsDeadEnd = true;
      var bIsDeadEnd = true;

      foreach (var segmentB in Connections)
      {
        if (segmentA.Equivalent(segmentB)) continue;

        if (segmentB.Has(segmentA.A)) aIsDeadEnd = false;
        if (segmentB.Has(segmentA.B)) bIsDeadEnd = false;
      }

      if (aIsDeadEnd) deadEnds.Add((Vector2I)segmentA.A);
      if (bIsDeadEnd) deadEnds.Add((Vector2I)segmentA.B);
    }

    foreach (var deadEnd in deadEnds)
    {
      GetNode(deadEnd).DeadEnd = true;
    }
  }

  protected virtual void GenerateCorridors()
  {
    CorridorPathing = new()
    {
      Region = Region,
      DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never,
      DefaultComputeHeuristic = AStarGrid2D.Heuristic.Manhattan,
      DefaultEstimateHeuristic = AStarGrid2D.Heuristic.Manhattan,
    };

    CorridorPathing.Update();

    int id = 0;
    foreach (Node node in Nodes)
    {
      if (node.Id >= id)
      {
        id = node.Id + 1;
      }

      Vector2I ul = node.Position + Vector2I.Up + Vector2I.Left;
      Vector2I ur = node.Position + Vector2I.Up + new Vector2I(node.Size.X, 0);
      Vector2I dl = node.Position + Vector2I.Left + new Vector2I(0, node.Size.Y);
      Vector2I dr = node.Position + new Vector2I(node.Size.X, node.Size.Y);

      if (IsValidTilePosition(ul)) CorridorPathing.SetPointSolid(ul);
      if (IsValidTilePosition(ur)) CorridorPathing.SetPointSolid(ur);
      if (IsValidTilePosition(dl)) CorridorPathing.SetPointSolid(dl);
      if (IsValidTilePosition(dr)) CorridorPathing.SetPointSolid(dr);
    }

    for (int x = 0; x < Region.Size.X; x++)
    {
      for (int y = 0; y < Region.Size.Y; y++)
      {
        CorridorPathing.SetPointWeightScale(new Vector2I(x, y), IsTileVoid(x, y) ? 3 : IsTileNode(x, y) ? 4 : 12);
      }
    }

    foreach (var segment in Connections)
    {
      Node fromNode = GetNode((Vector2I)segment.A);
      Node toNode = GetNode((Vector2I)segment.B);

      (int x, int w, bool overlapsH) = HorizontalSpace(fromNode, toNode);
      (int y, int h, bool overlapsV) = VerticalSpace(fromNode, toNode);

      Vector2I from = (Vector2I)fromNode.Midpoint;
      Vector2I to = (Vector2I)toNode.Midpoint;

      if (overlapsV)
      {
        from.X = x - 1;
        from.Y = y + Gameplay.Random.RandiRange(0, h - 1);

        to.X = x + w;
        to.Y = (w < 3) ? from.Y : Gameplay.Random.RandiRange(0, h - 1) + y;
      }
      else if (overlapsH)
      {
        from.X = x + Gameplay.Random.RandiRange(0, w - 1);
        from.Y = y - 1;

        to.X = (h < 3) ? from.X : Gameplay.Random.RandiRange(0, w - 1) + x;
        to.Y = y + h;
      }
      else
      {
        var (l, r) = (fromNode.Position.X < toNode.Position.X) ? (fromNode, toNode) : (toNode, fromNode);

        from.X = x - 1;
        from.Y = (l.Position.Y < r.Position.Y) ? y - 1 : y + h;

        to.X = x + w;
        to.Y = (l.Position.Y < r.Position.Y) ? y + h : y - 1;
      }

      Vector2[] path = CorridorPathing.GetPointPath(from, to);
      for (int i = 0; i < path.Length; i++)
      {
        Vector2I step = (Vector2I)path[i];
        if (IsTileNode(step))
        {
          continue;
        }

        SetCorridor(step, id++);
      }
    }
  }

  Node SetCorridor(Vector2I point, int id)
  {
    Node node = new(id, point.X, point.Y, 1, 1)
    {
      Type = NodeType.Corridor
    };

    Nodes.Add(node);
    AssignTilesRegion(node);
    Tiles[GetTileIndex(point)].SetNode(node);
    CorridorPathing.SetPointWeightScale(point, 1);

    return node;
  }

  protected virtual void Prune()
  {
    for (var i = Nodes.Count - 1; i >= 0; i--)
    {
      var node = Nodes[i];
      if (node.Type == NodeType.Void)
      {
        Nodes.RemoveAt(i);

        for (int x = 0; x < node.Size.X; x++)
        {
          for (int y = 0; y < node.Size.Y; y++)
          {
            TileData tile = Tiles[GetTileIndex(node.Position.X + x, node.Position.Y + y)];
            tile.Reset();
          }
        }
      }
    }
  }

  protected virtual void AddMissingWalls()
  {
    for (int x = 0; x < Region.Size.X; x++)
    {
      for (int y = 0; y < Region.Size.Y; y++)
      {
        if (!IsTileVoid(x, y)) continue;

        Vector2I tile = new(x, y);

        var u = tile + Vector2I.Up;
        var ul = u + Vector2I.Left;
        var ur = u + Vector2I.Right;

        var d = tile + Vector2I.Down;
        var dl = d + Vector2I.Left;
        var dr = d + Vector2I.Right;

        var l = tile + Vector2I.Left;
        var r = tile + Vector2I.Right;

        if (
          IsTileNode(u) ||
          IsTileNode(ul) ||
          IsTileNode(ur) ||
          IsTileNode(d) ||
          IsTileNode(dl) ||
          IsTileNode(dr) ||
          IsTileNode(l) ||
          IsTileNode(r)
        )
        {
          // TODO: Use correct model & orientation
          TileData data = Tiles[GetTileIndex(x, y)];
          data.Type = TileType.Wall;
        }
      }
    }
  }

  protected virtual void UpdatePathing()
  {
    foreach (Node node in Nodes)
    {
      Pathing.FillSolidRegion(node, false);
    }
  }

  protected virtual void CacheRooms()
  {
    Rooms = [];
    DeadEnds = [];

    foreach (Node node in Nodes)
    {
      if (node.Type != NodeType.Room) continue;

      if (node.Entry) EntryRoom = node;
      if (node.Exit) ExitRoom = node;
      if (node.DeadEnd) DeadEnds.Add(node);

      Rooms.Add(node);
    }
  }

  protected virtual void UpdateTilesData()
  {
    for (int x = 0; x < Region.Size.X; x++)
    {
      for (int y = 0; y < Region.Size.Y; y++)
      {
        TileData tile = Tiles[GetTileIndex(x, y)];
        tile.Position = new(x, y);

        // TODO: This feels wrong
        if (IsTileNode(x, y))
        {
          tile.Type = TileType.Node;
          tile.Model = 0;
        }
        else if (IsTileWall(x, y))
        {
          tile.Type = TileType.Wall;
          tile.Model = 20;
        }
      }
    }
  }

  protected void AssignTilesRegion(Node node)
  {
    AssignTilesRegion(node.Position, node.Size, node);
  }

  protected void AssignTilesRegion(Vector2I position, Vector2I size, Node value)
  {
    for (int x = position.X; x < position.X + size.X; x++)
    {
      for (int y = position.Y; y < position.Y + size.Y; y++)
      {
        if (IsValidTilePosition(x, y))
        {
          Tiles[GetTileIndex(x, y)].Node = value;
        }
      }
    }
  }

  protected void AssignTilesToWalledRoom(Node node)
  {
    int up = node.Position.Y - 1;
    int down = node.Position.Y + node.Size.Y;
    int left = node.Position.X - 1;
    int right = node.Position.X + node.Size.X;

    for (int x = left; x <= right; x++)
    {
      for (int y = up; y <= down; y++)
      {
        if (IsValidTilePosition(x, y))
        {
          TileData tile = Tiles[GetTileIndex(x, y)];

          if (y == up || y == down || x == left || x == right)
          {
            tile.Type = TileType.Wall;
            continue;
          }

          tile.Type = TileType.Node;
          tile.Node = node;
        }
      }
    }
  }

  protected static List<Segment> GenerateGraphSegments(Vector2[] points, int[] triangles)
  {
    List<Segment> edges = new List<Segment>();

    for (int i = 0; i < triangles.Length; i += 3)
    {
      var a = points[triangles[i + 0]];
      var b = points[triangles[i + 1]];
      var c = points[triangles[i + 2]];

      bool hasAB = false;
      bool hasBC = false;
      bool hasCD = false;

      foreach (var segment in edges)
      {
        hasAB = hasAB || (segment.Has(a) && segment.Has(b));
        hasBC = hasBC || (segment.Has(b) && segment.Has(c));
        hasCD = hasCD || (segment.Has(c) && segment.Has(a));
      }

      if (!hasAB) edges.Add(new Segment(a, b));
      if (!hasBC) edges.Add(new Segment(b, c));
      if (!hasCD) edges.Add(new Segment(c, a));
    }

    return edges;
  }

  protected static List<Segment> GenerateMST(Vector2[] points, int[] triangles)
  {
    var edges = new List<Segment>();
    var edgeSet = new HashSet<(int, int)>();

    for (int i = 0; i < triangles.Length; i += 3)
    {
      int[] indices = { triangles[i], triangles[i + 1], triangles[i + 2] };

      for (int j = 0; j < 3; j++)
      {
        int a = indices[j];
        int b = indices[(j + 1) % 3];

        var edgeKey = (Mathf.Min(a, b), Mathf.Max(a, b));
        if (!edgeSet.Contains(edgeKey))
        {
          edgeSet.Add(edgeKey);
          edges.Add(new Segment(points[a], points[b]));
        }
      }
    }

    edges.Sort((a, b) => (int)(a.Length - b.Length));

    var mst = new List<Segment>();
    var parent = new int[points.Length];
    var rank = new int[points.Length];

    for (int i = 0; i < points.Length; i++)
    {
      parent[i] = i;
    }

    int Find(int node)
    {
      if (parent[node] != node)
      {
        parent[node] = Find(parent[node]);
      }
      return parent[node];
    }

    void Union(int u, int v)
    {
      int rootU = Find(u);
      int rootV = Find(v);

      if (rootU != rootV)
      {
        if (rank[rootU] > rank[rootV])
        {
          parent[rootV] = rootU;
        }
        else if (rank[rootU] < rank[rootV])
        {
          parent[rootU] = rootV;
        }
        else
        {
          parent[rootV] = rootU;
          rank[rootU]++;
        }
      }
    }

    foreach (var edge in edges)
    {
      var indexA = Array.IndexOf(points, edge.A);
      var indexB = Array.IndexOf(points, edge.B);

      if (Find(indexA) != Find(indexB))
      {
        mst.Add(edge);
        Union(indexA, indexB);
      }

      if (mst.Count == points.Length - 1) break;
    }

    return mst;
  }

  protected static (int x, int w, bool overlaps) HorizontalSpace(Node a, Node b)
  {
    int x = Mathf.Max(a.Position.X, b.Position.X);
    int w = Mathf.Min(a.Position.X + a.Size.X, b.Position.X + b.Size.X) - x;

    return w > 0 ? (x, w, true) : (x + w, -w, false);
  }

  protected static (int y, int h, bool overlaps) VerticalSpace(Node a, Node b)
  {
    int y = Mathf.Max(a.Position.Y, b.Position.Y);
    int h = Mathf.Min(a.Position.Y + a.Size.Y, b.Position.Y + b.Size.Y) - y;

    return h > 0 ? (y, h, true) : (y + h, -h, false);
  }

  public bool TryGetActorAt(Vector2I tile, out Actor actor)
  {
    foreach (Actor _actor in Actors)
    {
      if (_actor.GridPosition == tile)
      {
        actor = _actor;
        return true;
      }
    }

    actor = default;
    return false;
  }

  public void InsertActor(Actor actor)
  {
    actor.DungeonLevel = this;
    Actors.Add(actor);
    ActorQueue.Enqueue(actor);
    actor.UpdateLineOfSight();

    Gameplay.Dungeon.AddChild(actor);
  }
}
