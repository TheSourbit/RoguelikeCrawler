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
}

public static class Shadowcasting
{
  public static void ComputeFOV(Vector2I origin, int range, Func<Vector2I, bool> isBlocking, Action<Vector2I> markVisible)
  {
    markVisible(origin);
    for (int i = 0; i < 4; i++)
    {
      Quadrant quadrant = new((Cardinals)i, origin);
      Scan(new(1, -1, 1), range, quadrant, isBlocking, markVisible);
    }
  }

  static float Slope(Vector2I tile) => (2 * tile.Y - 1) / (float)(2 * tile.X);
  static bool IsSymmetric(Row row, Vector2I tile) => tile.Y >= row.Depth * row.StartSlope && tile.Y <= row.Depth * row.EndSlope;

  static void Scan(
    Row row,
    int maxDepth,
    Quadrant quadrant,
    Func<Vector2I, bool> isBlocking,
    Action<Vector2I> markVisible
  )
  {
    if (row.Depth > maxDepth) return;

    bool isWall = false;
    bool isWallPrevious = false;
    bool hasPreviousTile = false;
    int minY = (int)Mathf.Floor(row.Depth * row.StartSlope + 0.5f);
    int maxY = (int)Mathf.Ceil(row.Depth * row.EndSlope - 0.5f);

    for (int y = minY; y <= maxY; y++)
    {
      Vector2I tile = new(row.Depth, y);
      Vector2I transformedTile = quadrant.GetTransformed(tile);
      isWall = isBlocking(transformedTile);

      if (isWall || IsSymmetric(row, tile))
      {
        markVisible(transformedTile);
      }

      if (hasPreviousTile)
      {
        if (isWallPrevious && !isWall)
        {
          row.StartSlope = Slope(tile);
        }

        if (!isWallPrevious && isWall)
        {
          Row nextRow = row.Next();
          nextRow.EndSlope = Slope(tile);
          Scan(nextRow, maxDepth, quadrant, isBlocking, markVisible);
        }
      }

      isWallPrevious = isWall;
      hasPreviousTile = true;
    }

    if (!isWall)
    {
      Scan(row.Next(), maxDepth, quadrant, isBlocking, markVisible);
    }
  }
}
