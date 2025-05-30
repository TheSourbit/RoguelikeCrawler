using Godot;

public static class Assets
{
  public static readonly PackedScene AgentScene = GD.Load<PackedScene>("res://Scenes/Actors/Agent.tscn");
  public static readonly PackedScene AvatarScene = GD.Load<PackedScene>("res://Scenes/Actors/Avatar.tscn");
  public static readonly PackedScene DoorScene = GD.Load<PackedScene>("res://Scenes/Environment/GenericDoor.tscn");

  public static readonly MeshLibrary BlankTilesLibrary = UpdateMeshLibraryMaterial(
    GD.Load<MeshLibrary>("res://BlankTilesLibrary.tres"),
    GD.Load<Material>("res://Materials/BlankGridMap.material")
  );

  public static readonly MeshLibrary ShadowBlankTilesLibrary = UpdateMeshLibraryMaterial(
    (MeshLibrary)BlankTilesLibrary.Duplicate(true),
    GD.Load<Material>("res://Materials/ShadowBlankGridMap.material")
  );

  static MeshLibrary UpdateMeshLibraryMaterial(MeshLibrary library, Material material)
  {
    foreach (int id in library.GetItemList())
    {
      library.GetItemMesh(id).SurfaceSetMaterial(0, material);
    }

    return library;
  }
}
