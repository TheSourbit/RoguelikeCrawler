using Godot;

public struct Segment
{
  public readonly Vector2 A;
  public readonly Vector2 B;
  public readonly float Length;

  public Segment(Vector2 a, Vector2 b)
  {
    A = a;
    B = b;
    Length = A.DistanceTo(b);
  }

  public readonly bool Has(Vector2 point)
  {
    return point == A || point == B;
  }

  public readonly bool Equivalent(Segment segment)
  {
    return (segment.A == A && segment.B == B) || (segment.A == B && segment.B == A);
  }
}
