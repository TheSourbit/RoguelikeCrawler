using System.Collections.Generic;

using Godot;

public enum TileType
{
  Void,
  Open,
  Solid,
  Entry,
  Exit,
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
    Type = TileType.Solid;
    Model = Invalid;
  }

  public void SetNode(Node node)
  {
    Type = TileType.Open;
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

public static class BlobTileset
{
  static readonly Dictionary<int, (int index, int orientation)> LookupTable = new()
  {
    {0, (0, 0)},
    {1, (1, 0)},
    {3, (2, 0)},
    {5, (2, 2)},
    {7, (3, 0)},
    {9, (2, 2)},
    {11, (4, 0)},
    {13, (3, 3)},
    {15, (5, 0)},
    {17, (2, 1)},
    {19, (3, 1)},
    {21, (4, 1)},
    {23, (5, 1)},
    {25, (3, 2)},
    {27, (5, 2)},
    {29, (5, 3)},
    {31, (6, 0)},
    {51, (7, 0)},
    {55, (8, 0)},
    {59, (9, 0)},
    {63, (10, 0)},
    {71, (7, 3)},
    {79, (8, 3)},
    {87, (9, 3)},
    {95, (10, 3)},
    {119, (11, 0)},
    {127, (12, 0)},
    {141, (7, 2)},
    {143, (9, 2)},
    {157, (8, 2)},
    {159, (10, 2)},
    {191, (13, 0)},
    {207, (11, 3)},
    {223, (12, 3)},
    {255, (14, 0)},
    {281, (7, 1)},
    {283, (8, 1)},
    {285, (9, 1)},
    {287, (10, 1)},
    {315, (11, 1)},
    {319, (12, 1)},
    {351, (13, 1)},
    {383, (14, 1)},
    {413, (11, 2)},
    {415, (12, 2)},
    {447, (14, 2)},
    {479, (14, 3)},
    {511, (15, 0)},
  };

  // TODO: Test this shit!!
  public static (int index, int orientation) GeBlob(int u, int d, int l, int r, int ur, int ul, int dr, int dl)
  {
    r = (r + 1) < 2 ? 0 : 1;
    u = (u + 1) < 2 ? 0 : 1;
    l = (l + 1) < 2 ? 0 : 1;
    d = (d + 1) < 2 ? 0 : 1;
    dr = (dr + d + r) < 3 ? 0 : 1;
    ur = (ur + r + u) < 3 ? 0 : 1;
    ul = (ul + u + l) < 3 ? 0 : 1;
    dl = (dl + l + d) < 3 ? 0 : 1;

    int bitmask =
      1 + // c << 0 -> center is always filled
      r << 1 +
      u << 2 +
      l << 3 +
      d << 4 +
      dr << 5 +
      ur << 6 +
      ul << 7 +
      dl << 8;

    return LookupTable[bitmask];
  }
}
