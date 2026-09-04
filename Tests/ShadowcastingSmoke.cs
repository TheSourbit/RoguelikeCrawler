using System;
using System.Collections.Generic;
using System.Linq;

using Chickensoft.GoDotTest;
using Godot;

public class ShadowcastingSmoke : TestClass
{
  public ShadowcastingSmoke(Godot.Node testScene) : base(testScene) { }

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
      tile =>
      {
        if (tile.X >= 0 && tile.Y >= 0 && tile.X < width && tile.Y < height)
        {
          visible.Add(tile);
        }
      }
    );

    blockingCalls = calls;
    return visible;
  }

  static void Require(string name, bool condition, string details)
  {
    if (!condition)
    {
      throw new InvalidOperationException($"{name}: {details}");
    }
  }

  static string Describe(IEnumerable<Vector2I> tiles)
  {
    return string.Join(", ", tiles.Select(tile => tile.ToString()).OrderBy(tile => tile));
  }

  [Test]
  public void OpenFieldCoverage()
  {
    HashSet<Vector2I> expected =
    [
      new(2, 2), new(3, 2), new(4, 2), new(5, 2), new(6, 2),
      new(2, 3), new(3, 3), new(4, 3), new(5, 3), new(6, 3),
      new(2, 4), new(3, 4), new(4, 4), new(5, 4), new(6, 4),
      new(2, 5), new(3, 5), new(4, 5), new(5, 5), new(6, 5),
      new(2, 6), new(3, 6), new(4, 6), new(5, 6), new(6, 6),
    ];

    HashSet<Vector2I> visible = ComputeVisible(9, 9, new(4, 4), 2, [], out _);

    Require(
      nameof(OpenFieldCoverage),
      visible.SetEquals(expected),
      $"expected {expected.Count} tiles, got {visible.Count}"
    );
  }

  [Test]
  public void RangeZeroShowsOnlyOrigin()
  {
    HashSet<Vector2I> expected = [new(4, 4)];
    HashSet<Vector2I> visible = ComputeVisible(9, 9, new(4, 4), 0, [], out _);

    Require(nameof(RangeZeroShowsOnlyOrigin), visible.SetEquals(expected), $"got {Describe(visible)}");
  }

  [Test]
  public void OpaqueWallIsVisible()
  {
    HashSet<Vector2I> walls = CreateBarrierWalls();
    HashSet<Vector2I> visible = ComputeVisible(9, 9, new(4, 4), 4, walls, out _);

    Require(
      nameof(OpaqueWallIsVisible),
      visible.Contains(new(3, 5)),
      "the wall tile itself was hidden"
    );
  }

  [Test]
  public void GapRemainsVisible()
  {
    HashSet<Vector2I> walls = CreateBarrierWalls();
    HashSet<Vector2I> visible = ComputeVisible(9, 9, new(4, 4), 4, walls, out _);

    Require(
      nameof(GapRemainsVisible),
      visible.Contains(new(4, 6)),
      "the tile through the gap was hidden"
    );
  }

  [Test]
  public void ClosedSectionIsBlocked()
  {
    HashSet<Vector2I> walls = CreateBarrierWalls();
    HashSet<Vector2I> visible = ComputeVisible(9, 9, new(4, 4), 4, walls, out _);

    Require(
      nameof(ClosedSectionIsBlocked),
      !visible.Contains(new(2, 6)),
      "a tile behind the wall was visible"
    );
  }

  [Test]
  public void EdgeVisibilityStaysInBounds()
  {
    HashSet<Vector2I> visible = ComputeVisible(5, 5, new(0, 0), 2, [], out _);
    IEnumerable<Vector2I> outOfBounds = visible.Where(
      tile => tile.X < 0 || tile.Y < 0 || tile.X >= 5 || tile.Y >= 5
    );

    Require(
      nameof(EdgeVisibilityStaysInBounds),
      visible.All(tile => tile.X >= 0 && tile.Y >= 0 && tile.X < 5 && tile.Y < 5),
      $"out-of-bounds tiles: {Describe(outOfBounds)}"
    );
  }

  [Test]
  public void EdgeOpenCoverage()
  {
    HashSet<Vector2I> expected =
    [
      new(0, 0), new(1, 0), new(2, 0),
      new(0, 1), new(1, 1), new(2, 1),
      new(0, 2), new(1, 2), new(2, 2),
    ];
    HashSet<Vector2I> visible = ComputeVisible(5, 5, new(0, 0), 2, [], out _);

    Require(
      nameof(EdgeOpenCoverage),
      visible.SetEquals(expected),
      $"expected {expected.Count} tiles, got {visible.Count}"
    );
  }

  static HashSet<Vector2I> CreateBarrierWalls()
  {
    HashSet<Vector2I> walls = [];
    for (int x = 0; x < 9; x++)
    {
      if (x != 4) walls.Add(new(x, 5));
    }

    return walls;
  }
}
