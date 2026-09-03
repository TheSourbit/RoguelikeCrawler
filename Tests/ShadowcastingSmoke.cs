using System;
using System.Collections.Generic;
using System.Linq;

using Godot;

static class ShadowcastingSmoke
{
  static int passed;
  static int failed;

  static HashSet<Vector2I> ComputeVisible(
    int width,
    int height,
    Vector2I origin,
    int range,
    HashSet<Vector2I> walls,
    out int blockingCalls
  )
  {
    int calls = 0;
    HashSet<Vector2I> visible = [];

    Shadowcasting.ComputeFOV(
      origin,
      range,
      tile =>
      {
        calls++;
        return tile.X < 0 || tile.Y < 0 || tile.X >= width || tile.Y >= height || walls.Contains(tile);
      },
      tile => visible.Add(tile)
    );

    blockingCalls = calls;
    return visible;
  }

  static void Check(string name, bool condition, string details)
  {
    if (!condition)
    {
      failed++;
      Console.WriteLine($"FAIL: {name}: {details}");
      return;
    }

    passed++;
    Console.WriteLine($"PASS: {name}");
  }

  static string Describe(IEnumerable<Vector2I> tiles)
  {
    return string.Join(", ", tiles.Select(tile => tile.ToString()).OrderBy(tile => tile));
  }

  static void Main()
  {
    HashSet<Vector2I> openExpected =
    [
      new(2, 2), new(3, 2), new(4, 2), new(5, 2), new(6, 2),
      new(2, 3), new(3, 3), new(4, 3), new(5, 3), new(6, 3),
      new(2, 4), new(3, 4), new(4, 4), new(5, 4), new(6, 4),
      new(2, 5), new(3, 5), new(4, 5), new(5, 5), new(6, 5),
      new(2, 6), new(3, 6), new(4, 6), new(5, 6), new(6, 6),
    ];

    HashSet<Vector2I> open = ComputeVisible(9, 9, new(4, 4), 2, [], out int openCalls);
    Check("open field coverage", open.SetEquals(openExpected), $"expected {openExpected.Count} tiles, got {open.Count}");
    Console.WriteLine($"INFO: open range-2 blocking calls = {openCalls}");

    HashSet<Vector2I> rangeZeroExpected = [new(4, 4)];
    HashSet<Vector2I> rangeZero = ComputeVisible(9, 9, new(4, 4), 0, [], out _);
    Check("range zero shows only origin", rangeZero.SetEquals(rangeZeroExpected), $"got {Describe(rangeZero)}");

    HashSet<Vector2I> barrierWalls = [];
    for (int x = 0; x < 9; x++)
    {
      if (x != 4) barrierWalls.Add(new(x, 5));
    }

    HashSet<Vector2I> barrier = ComputeVisible(9, 9, new(4, 4), 4, barrierWalls, out _);
    Check("opaque wall is visible", barrier.Contains(new(3, 5)), "the wall tile itself was hidden");
    Check("gap remains visible", barrier.Contains(new(4, 6)), "the tile through the gap was hidden");
    Check("closed section is blocked", !barrier.Contains(new(2, 6)), "a tile behind the wall was visible");

    HashSet<Vector2I> edgeExpected =
    [
      new(0, 0), new(1, 0), new(2, 0),
      new(0, 1), new(1, 1), new(2, 1),
      new(0, 2), new(1, 2), new(2, 2),
    ];
    HashSet<Vector2I> edge = ComputeVisible(5, 5, new(0, 0), 2, [], out _);
    Check(
      "edge visibility stays in bounds",
      edge.All(tile => tile.X >= 0 && tile.Y >= 0 && tile.X < 5 && tile.Y < 5),
      $"out-of-bounds tiles: {Describe(edge.Where(tile => tile.X < 0 || tile.Y < 0 || tile.X >= 5 || tile.Y >= 5))}"
    );
    Check("edge open coverage", edge.SetEquals(edgeExpected), $"expected {edgeExpected.Count} tiles, got {edge.Count}");

    Console.WriteLine($"Smoke test complete: {passed} passed, {failed} failed");
    System.Environment.ExitCode = failed == 0 ? 0 : 1;
  }
}
