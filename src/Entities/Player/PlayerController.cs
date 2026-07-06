using Godot;

namespace Odysseus.Entities.Player;

public sealed partial class PlayerController : CharacterBody3D
{
	[Export] public float MaxSpeed { get; set; } = 5.0f;
	[Export] public float Acceleration { get; set; } = 12.0f;
	[Export] public float RotationSpeed { get; set; } = 10.0f;
	[Export] public float Gravity { get; set; } = 9.8f;

	public bool InputEnabled { get; set; } = true;

	public override void _PhysicsProcess(double delta)
	{
		ApplyGravity(delta);

		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
		if (!InputEnabled) inputDir = Vector2.Zero;

		Vector3 forward = -Transform.Basis.Z;
		Vector3 right = Transform.Basis.X;
		Vector3 raw = (forward * inputDir.Y) + (right * inputDir.X);
		Vector3 direction = raw.LengthSquared() > 0.001f ? raw.Normalized() : Vector3.Zero;

		if (direction != Vector3.Zero)
		{
			Vector2 flat = new Vector2(direction.X, direction.Z);
			float targetAngle = Mathf.Atan2(flat.X, flat.Y);
			float currentAngle = Rotation.Y;
			float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, (float)delta * RotationSpeed);
			Rotation = new Vector3(Rotation.X, newAngle, Rotation.Z);
		}

		if (direction != Vector3.Zero)
		{
			Velocity = Velocity.MoveToward(direction * MaxSpeed, Acceleration * (float)delta);
		}
		else
		{
			Velocity = Velocity.MoveToward(Vector3.Zero, Acceleration * (float)delta);
		}

		MoveAndSlide();
	}

	private void ApplyGravity(double delta)
	{
		if (!IsOnFloor())
			Velocity = Velocity + new Vector3(0, -Gravity * (float)delta, 0);
	}
}