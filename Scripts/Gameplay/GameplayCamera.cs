using Godot;

public partial class GameplayCamera : Node3D
{
  [Export] public Node3D Tracking;

  [ExportGroup("Movement")]
  [Export] public float AngularSpeed = 12.5f;
  [Export] public float TrackingSpeed = 9.5f;
  [Export] public float FocusLead = 0.8f;
  [Export] public float ZoomSensitivity = 2f;
  [Export] public float PanSensitivity = 1.35f;
  [Export] public float PitchSensitivity = 2.25f;

  [ExportGroup("Camera Distance")]
  [Export] public float DefaultDistance = 35f;
  [Export] public Vector2 DistanceLimit = new(15f, 150f);

  [ExportGroup("Camera Angle")]
  [Export] public float DefaultAngle = 62.5f;
  [Export] public Vector2 AngleLimit = new(35f, 90f);

  [ExportGroup("Camera Attributes")]
  [Export] public float DefaultFov = 15f;

  Camera3D Camera;
  CameraAttributesPractical Attributes;
  float LensFocusDistance;

  bool Panning = false;
  bool Pitching = false;

  Vector3 offset;
  float angle;

  public override void _Ready()
  {
    Attributes = new CameraAttributesPractical()
    {
      DofBlurFarEnabled = true,
      DofBlurFarTransition = 10f,
      DofBlurNearEnabled = true,
      DofBlurNearTransition = 10f,
      DofBlurAmount = 0.13f,
    };

    Camera = new Camera3D
    {
      Fov = DefaultFov,
      Attributes = Attributes,
    };

    AddChild(Camera);
    Recenter(true);
  }

  public override void _Process(double delta)
  {
    Rotation = Rotation.Lerp(new Vector3(Mathf.DegToRad(-angle), 0, 0), (float)delta * AngularSpeed);
    Camera.Position = Camera.Position.Lerp(offset, (float)delta * TrackingSpeed);

    LensFocusDistance = Mathf.Lerp(LensFocusDistance, offset.Z, (float)delta * TrackingSpeed * FocusLead);

    Attributes.DofBlurFarDistance = LensFocusDistance;
    Attributes.DofBlurNearDistance = LensFocusDistance;

    if (Tracking != null)
    {
      Position = Position.Lerp(Tracking.Position + Vector3.Up * 0.5f, (float)delta * TrackingSpeed);
    }
  }

  public override void _Input(InputEvent @event)
  {
    if (@event is InputEventMouseButton mouseButton)
    {
      if (mouseButton.ButtonIndex == MouseButton.Middle)
      {
        Panning = mouseButton.Pressed;
      }
      else if (mouseButton.ButtonIndex == MouseButton.Right)
      {
        Pitching = mouseButton.Pressed;
      }
      else if (mouseButton.ButtonIndex == MouseButton.WheelUp || mouseButton.ButtonIndex == MouseButton.WheelDown)
      {
        Zoom(mouseButton.ButtonIndex == MouseButton.WheelDown);
      }
    }
    else if (@event is InputEventMouseMotion mouseMotion)
    {
      if (Panning)
      {
        Pan(mouseMotion.Relative);
      }
      else if (Pitching)
      {
        Pitch(mouseMotion.Relative.Y);
      }
    }
  }

  void Zoom(bool @out = false)
  {
    var scale = offset.Z / DistanceLimit.X;

    offset.Z += (@out ? ZoomSensitivity : -ZoomSensitivity) * scale;
    offset.Z = Mathf.Clamp(offset.Z, DistanceLimit.X, DistanceLimit.Y);
  }

  void Pan(Vector2 relative)
  {
    offset.X += -relative.X * (PanSensitivity / 100f) * (offset.Z / DefaultDistance);
    offset.Y += relative.Y * (PanSensitivity / 100f) * (offset.Z / DefaultDistance);
  }

  void Pitch(float relative)
  {
    angle = Mathf.Clamp(angle + relative * PitchSensitivity / 10f, AngleLimit.X, AngleLimit.Y);
  }

  public void Recenter(bool reset = false)
  {
    offset.X = 0;
    offset.Y = 0;

    if (reset)
    {
      offset.Z = DefaultDistance;
      angle = DefaultAngle;
    }
  }
}
