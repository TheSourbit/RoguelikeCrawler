public partial class Door : Prop
{
  bool Closed = true;

  public override void Interact()
  {
    if (Closed)
    {
      Open();
    }
    else
    {
      Close();
    }
  }

  public void Open()
  {
    Closed = false;
  }

  public void Close()
  {
    Closed = true;
  }

  public override bool IsBlockingLoS()
  {
    return Closed;
  }
}
