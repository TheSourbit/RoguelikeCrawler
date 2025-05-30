public partial class Door : Actor
{
  bool Closed = true;

  public void Open()
  {
    Closed = false;
  }

  public void Close()
  {
    Closed = true;
  }

  public void Toggle()
  {
    Closed = !Closed;
  }

  public override bool IsBlockingLoS()
  {
    return Closed;
  }
}
