using System;

using Godot;

public enum Cardinals
{
  North,
  East,
  South,
  West,
}

public struct Quadrant(Cardinals cardinal, Vector2I origin)
{
  public readonly Vector2I GetTransformed(Vector2I tile) => cardinal switch
  {
    Cardinals.North => new(origin.X + tile.Y, origin.Y - tile.X),
    Cardinals.South => new(origin.X + tile.Y, origin.Y + tile.X),
    Cardinals.East => new(origin.X + tile.X, origin.Y + tile.Y),
    Cardinals.West => new(origin.X - tile.X, origin.Y + tile.Y),
  };
}

public struct Row(int depth, float startSlope, float endSlope)
{
  public int Depth = depth;
  public float StartSlope = startSlope;
  public float EndSlope = endSlope;

  public readonly Row Next() => new(Depth + 1, StartSlope, EndSlope);
  public readonly Vector2I[] Tiles()
  {
    int min = (int)Mathf.Floor(Depth * StartSlope + 0.5f);
    int max = (int)Mathf.Ceil(Depth * EndSlope - 0.5f);

    int count = max - min + 1;
    var tiles = new Vector2I[count];
    for (int i = 0; i < count; i++)
    {
      tiles[i] = new(Depth, i + min);
    }

    return tiles;
  }
}

public class Shadowcasting(Quadrant quadrant, Func<Vector2I, bool> isBlocking, Action<Vector2I> markVisible)
{
  static Vector2I None = -Vector2I.One;

  public static void ComputeFOV(Vector2I origin, int range, Func<Vector2I, bool> isBlocking, Action<Vector2I> markVisible)
  {
    markVisible(origin);
    for (int i = 0; i < 4; i++)
    {
      Quadrant quadrant = new((Cardinals)i, origin);
      Shadowcasting shadowcasting = new(quadrant, isBlocking, markVisible);
      shadowcasting.Scan(new(1, -1, 1), range);
    }
  }

  static float Slope(Vector2I tile) => (2 * tile.Y - 1) / (float)(2 * tile.X);
  static bool IsSymmetric(Row row, Vector2I tile) => tile.Y >= row.Depth * row.StartSlope && tile.Y <= row.Depth * row.EndSlope;

  void Scan(Row row, int maxDepth)
  {
    if (row.Depth > maxDepth) return;

    bool isWall = false;
    Vector2I prevTile = None;

    foreach (Vector2I tile in row.Tiles())
    {
      Vector2I transformedTile = quadrant.GetTransformed(tile);
      isWall = isBlocking(transformedTile);

      if (isWall || IsSymmetric(row, tile))
      {
        markVisible(transformedTile);
      }

      if (prevTile != None)
      {
        bool isWallPrev = isBlocking(quadrant.GetTransformed(prevTile));

        if (isWallPrev && !isWall)
        {
          row.StartSlope = Slope(tile);
        }

        if (!isWallPrev && isWall)
        {
          Row nextRow = row.Next();
          nextRow.EndSlope = Slope(tile);
          Scan(nextRow, maxDepth);
        }
      }

      prevTile = tile;
    }

    if (!isWall)
    {
      Scan(row.Next(), maxDepth);
    }
  }
}
