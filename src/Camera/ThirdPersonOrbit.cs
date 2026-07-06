using Godot;

namespace Odysseus.Camera;

public sealed partial class ThirdPersonOrbit : Camera3D
{
	[Export] public NodePath? TargetPath;
	[Export] public float OrbitDistance { get; set; } = 6.0f;
	[Export] public float OrbitMinDistance { get; set; } = 2.0f;
	[Export] public float OrbitMaxDistance { get; set; } = 15.0f;
	[Export] public float OrbitYaw { get; set; } = 0.0f;
	[Export] public float OrbitPitch { get; set; } = -0.35f;
	[Export] public float OrbitSpeed { get; set; } = 0.005f;
	[Export] public float PitchMin { get; set; } = -1.2f;
	[Export] public float PitchMax { get; set; } = 0.2f;
	[Export] public float ZoomStep { get; set; } = 0.8f;
	[Export] public float Smoothing { get; set; } = 10.0f;
	[Export] public float HeightOffset { get; set; } = 1.5f;

	private Node3D? _target;
	private Vector3 _desiredPosition;
	private Vector3 _lookAt;
	private Vector3 _smoothedPosition;

	public override void _Ready()
	{
		if (TargetPath != null)
			_target = GetNodeOrNull<Node3D>(TargetPath);
		Smoothing = Mathf.Max(Smoothing, 0.01f);
	}

	public void SetTarget(Node3D target)
	{
		_target = target;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mm && Input.IsMouseButtonPressed(MouseButton.Left))
		{
			OrbitYaw -= mm.Relative.X * OrbitSpeed;
			OrbitPitch = Mathf.Clamp(OrbitPitch - mm.Relative.Y * OrbitSpeed * 0.6f, PitchMin, PitchMax);
			GetViewport().SetInputAsHandled();
		}
		else if (@event is InputEventMouseButton mb)
		{
			if (mb.IsPressed())
			{
				if (mb.ButtonIndex == MouseButton.WheelUp)
				{
					OrbitDistance = Mathf.Max(OrbitMinDistance, OrbitDistance - ZoomStep);
					GetViewport().SetInputAsHandled();
				}
				else if (mb.ButtonIndex == MouseButton.WheelDown)
				{
					OrbitDistance = Mathf.Min(OrbitMaxDistance, OrbitDistance + ZoomStep);
					GetViewport().SetInputAsHandled();
				}
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_target == null) return;
		Vector3 pivot = _target.GlobalPosition + new Vector3(0, HeightOffset, 0);
		float cp = Mathf.Cos(OrbitPitch);
		Vector3 offset = new Vector3(
			Mathf.Sin(OrbitYaw) * cp,
			Mathf.Sin(-OrbitPitch),
			Mathf.Cos(OrbitYaw) * cp
		);
		Vector3 planar = new Vector3(offset.X, 0, offset.Z);
		if (planar.LengthSquared() > 0.0001f) planar = planar.Normalized() * OrbitDistance;
		_desiredPosition = pivot + new Vector3(planar.X, OrbitDistance * Mathf.Sin(-OrbitPitch) * 0.6f + offset.Y * OrbitDistance, planar.Z);

		if (_smoothedPosition == Vector3.Zero) _smoothedPosition = _desiredPosition;
		_smoothedPosition = _smoothedPosition.Lerp(_desiredPosition, Mathf.Clamp((float)delta * Smoothing, 0.0f, 1.0f));
		GlobalPosition = _smoothedPosition;
		_lookAt = pivot;
		LookAt(_lookAt, Vector3.Up);
	}
}