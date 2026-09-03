using System;
using System.Collections;
using System.Collections.Generic;

using Godot;

public sealed class DenseTileSet : IEnumerable<Vector2I>
{
  int[] Stamps = [];
  readonly List<int> Entries = [];
  int Stamp = 1;
  int OriginX;
  int OriginY;
  int Height;
  int Width;

  public int Count => Entries.Count;

  public void Configure(Rect2I region)
  {
    if (
      Width == region.Size.X &&
      Height == region.Size.Y &&
      OriginX == region.Position.X &&
      OriginY == region.Position.Y
    )
    {
      return;
    }

    Width = region.Size.X;
    Height = region.Size.Y;
    OriginX = region.Position.X;
    OriginY = region.Position.Y;
    Stamps = new int[Width * Height];
    Entries.Clear();
    Stamp = 1;
  }

  public bool Add(Vector2I tile)
  {
    if (!TryGetIndex(tile, out int index) || Stamps[index] == Stamp)
    {
      return false;
    }

    Stamps[index] = Stamp;
    Entries.Add(index);
    return true;
  }

  public bool Contains(Vector2I tile)
  {
    return TryGetIndex(tile, out int index) && Stamps[index] == Stamp;
  }

  public bool Remove(Vector2I tile)
  {
    if (!TryGetIndex(tile, out int index) || Stamps[index] != Stamp)
    {
      return false;
    }

    Stamps[index] = 0;
    Entries.Remove(index);
    return true;
  }

  public void Clear()
  {
    Entries.Clear();
    if (Stamp == int.MaxValue)
    {
      Array.Clear(Stamps);
      Stamp = 1;
      return;
    }

    Stamp++;
  }

  bool TryGetIndex(Vector2I tile, out int index)
  {
    int x = tile.X - OriginX;
    int y = tile.Y - OriginY;
    if (x < 0 || y < 0 || x >= Width || y >= Height)
    {
      index = -1;
      return false;
    }

    index = x * Height + y;
    return true;
  }

  public Enumerator GetEnumerator() => new(this);
  IEnumerator<Vector2I> IEnumerable<Vector2I>.GetEnumerator() => GetEnumerator();
  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

  public struct Enumerator(DenseTileSet tileSet) : IEnumerator<Vector2I>
  {
    int EntryIndex = -1;

    public Vector2I Current
    {
      get
      {
        int index = tileSet.Entries[EntryIndex];
        return new(
          tileSet.OriginX + index / tileSet.Height,
          tileSet.OriginY + index % tileSet.Height
        );
      }
    }

    object IEnumerator.Current => Current;

    public bool MoveNext()
    {
      EntryIndex++;
      return EntryIndex < tileSet.Entries.Count;
    }

    public void Reset() => EntryIndex = -1;
    public void Dispose() { }
  }
}
