using Godot;

public partial class BillboardHealthBar : ProgressBar
{
  public override void _Process(double delta)
  {
    Agent agent = Owner as Agent;

    Value = agent.Status.Health;
    MaxValue = agent.Status.MaxHealth;
  }
}
