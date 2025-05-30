using System;

using Godot;

class ForestLevel(int size) : AbstractDungeonLevel(size * size * 5, size * size * 2)
{
  protected override void GenerateNodes()
  {
    AditionalConnections = 0.1f;

    int minSide = Math.Max(size / 2, 2);

    float bigMean = size;
    float smallMean = size * 0.65f + minSide;
    float deviation = size;

    int id = 0;

    var firstRoom = CreateNode(id++, bigMean, deviation);
    var lastRoom = CreateNode(id++, bigMean, deviation);

    firstRoom.Entry = true;
    firstRoom.Position.X = 1;
    firstRoom.Position.Y = GetRandomYPostion(firstRoom, 0.5f, 0.1f);

    lastRoom.Exit = true;
    lastRoom.Position.X = Region.Size.X - lastRoom.Size.X - 1;
    lastRoom.Position.Y = GetRandomYPostion(lastRoom, 0.5f, 0.1f);

    UseNode(firstRoom);
    UseNode(lastRoom);

    int freeSpace = Region.Size.X - firstRoom.Size.X - lastRoom.Size.X - 2;
    int pointerOffset = firstRoom.Size.X + 2;

    while (freeSpace > smallMean)
    {
      Node node = CreateNode(id++, smallMean, deviation);
      if (node.Size.X < freeSpace)
      {
        node.Position.X = pointerOffset;
        node.Position.Y = GetRandomYPostion(node, 0.5f, 0.1f);

        int offset = node.Size.X + Math.Clamp((int)Math.Abs(Gameplay.Random.Randfn(minSide, size / 2f)), 1, size);
        pointerOffset += offset;
        freeSpace -= offset;

        UseNode(node);
      }
    }

    if (freeSpace >= minSide)
    {
      Node node = CreateNode(id++, smallMean, deviation);

      node.Size.X = freeSpace;
      node.Size.Y = (int)(freeSpace * Gameplay.Random.Randf() + freeSpace * 0.5f);
      node.Position.X = pointerOffset;
      node.Position.Y = GetRandomYPostion(node);

      UseNode(node);
    }

    int retries = 100;
    int additionalNodes = (int)Math.Clamp(Gameplay.Random.Randfn(size * 2, size / 2f), size, size * 3);
    while (additionalNodes > 0)
    {
      Node node = CreateNode(id++, smallMean, deviation);

      node.Position.X = (int)Gameplay.Random.RandfRange(Region.Size.X * 0.2f, (Region.Size.X - node.Size.X) * 0.8f);
      node.Position.Y = GetRandomYPostion(node, Gameplay.Random.Randf() < 0.5f ? 0 : 1);

      if (IsRegionEmpty(node))
      {
        UseNode(node);
        additionalNodes--;
      }

      if (retries-- < 0) break;
    }
  }

  Node CreateNode(int id, float mean, float deviation)
  {
    var w = Math.Clamp((int)Mathf.Round(Mathf.Abs(Gameplay.Random.Randfn(mean, deviation))), size, Region.Size.X);
    var h = Math.Clamp((int)Mathf.Round(Mathf.Abs(Gameplay.Random.Randfn(mean, deviation))), size, Region.Size.Y);

    if (Mathf.Min((float)w, h) / Mathf.Max((float)w, h) < 0.65f)
    {
      return CreateNode(id, mean, deviation);
    }

    return new Node(id, 0, 0, w, h);
  }

  void UseNode(Node node)
  {
    node.Type = NodeType.Room;
    Nodes.Add(node);
    AssignTilesToWalledRoom(node);
  }

  int GetRandomYPostion(Node node, float mean = 0.5f, float deviation = 0.15f)
  {
    return (int)(Mathf.Clamp(Gameplay.Random.Randfn(mean, deviation), 0, 1) * (Region.Size.Y - node.Size.Y - 2) + 1);
  }
}
