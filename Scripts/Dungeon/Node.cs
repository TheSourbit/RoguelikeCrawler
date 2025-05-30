using Godot;

public enum NodeType
{
  Void,
  Corridor,
  Room,
}

public class Node(int id, int x, int y, int w, int h)
{
  public readonly int Id = id;

  public Vector2I Position = new(x, y);
  public Vector2I Size = new(w, h);

  public NodeType Type = NodeType.Void;

  public bool DeadEnd = false;
  public bool Entry = false;
  public bool Exit = false;
  public bool Door = false;

  public Vector2 Midpoint { get => new(Size.X / 2f + Position.X, Size.Y / 2f + Position.Y); }

  public int Area { get => Size.X * Size.Y; }

  public Segment diagonalA { get => new Segment(Position, Position + Size); }
  public Segment diagonalB { get => new Segment(new Vector2I(Position.X, Position.Y + Size.Y), new Vector2I(Position.X + Size.X, Position.Y)); }

  public bool Overlaps(Node node) =>
    Position.X < node.Position.X + node.Size.X && Position.X + Size.X > node.Position.X &&
    Position.Y < node.Position.Y + node.Size.Y && Position.Y + Size.Y > node.Position.Y
  ;

  public override string ToString()
  {
    return $"[Node {Id} {Type} {Position},{Size}]";
  }

  public static implicit operator Rect2I(Node node)
  {
    return new Rect2I(node.Position, node.Size);
  }
}
