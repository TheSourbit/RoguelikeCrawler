using Godot;

#if RUN_TESTS
using System.Reflection;

using Chickensoft.GoDotTest;
#endif

public partial class Gameplay : Godot.Node
{
  [Export] GameplayCamera Camera;

#if RUN_TESTS
  public TestEnvironment Environment = default!;
#endif

  public static Avatar Avatar { get; private set; }
  public static Dungeon Dungeon { get; private set; }
  public static RandomNumberGenerator Random { get; private set; }
  public static AbstractDungeonLevel CurrentLevel { get; private set; }
  public static Vector3 GridToWorld(Vector2I tile)
  {
    return new Vector3(tile.X + 0.5f, 0, tile.Y + 0.5f);
  }

  public override void _Ready()
  {
#if RUN_TESTS
    Environment = TestEnvironment.From(OS.GetCmdlineArgs());
    if (Environment.ShouldRunTests)
    {
      CallDeferred("RunTests");
      return;
    }
#endif

    Random = new RandomNumberGenerator
    {
      Seed = "random".Hash()
    };

    CreateDungeon();

    Avatar = Assets.AvatarScene.Instantiate<Avatar>();
    Avatar.Camera = Camera;
    CurrentLevel.MoveActorTo(Avatar, (Vector2I)Dungeon.CurrentLevel.EntryRoom.Midpoint);

    Camera.Tracking = Avatar;

    CurrentLevel.InsertActor(Avatar);
    CurrentLevel.Update();
  }

#if RUN_TESTS
  private void RunTests()
    => _ = GoTest.RunTests(Assembly.GetExecutingAssembly(), this, Environment);
#endif

  protected void CreateDungeon()
  {
    if (Dungeon != null)
    {
      RemoveChild(Dungeon);
      Dungeon.QueueFree();
    }

    Dungeon = new Dungeon
    {
      VisibleMeshLibrary = Assets.BlankTilesLibrary,
      KnownMeshLibrary = Assets.ShadowBlankTilesLibrary
    };

    Dungeon.Generate();
    CurrentLevel = Dungeon.CurrentLevel;

    AddChild(Dungeon);
  }
}
