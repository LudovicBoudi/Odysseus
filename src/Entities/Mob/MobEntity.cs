using System;
using Godot;
using Odysseus.Core;

namespace Odysseus.Entities.Mob;

public sealed partial class MobEntity : CharacterBody3D
{
	[Export] public string MobId { get; set; } = "mob_default";
	[Export] public string DisplayName { get; set; } = "Sanglier";
	[Export] public int Level { get; set; } = 1;
	[Export] public int MaxHp { get; set; } = 50;
	[Export] public int Attack { get; set; } = 5;
	[Export] public int Defense { get; set; } = 2;
	[Export] public MobBehavior Behavior { get; set; } = MobBehavior.NonAggressive;
	[Export] public float AggroRange { get; set; } = 8.0f;
	[Export] public float MoveSpeed { get; set; } = 2.5f;
	[Export] public float AttackRange { get; set; } = 1.8f;
	[Export] public float AttackCooldown { get; set; } = 1.5f;
	[Export] public float Gravity { get; set; } = 9.8f;

	public int CurrentHp { get; set; }
	public bool IsDead => CurrentHp <= 0;
	public Node3D? Target { get; set; }

	private float _attackTimer;
	private Vector3 _spawnPos;
	private float _wanderTimer;
	private Vector3 _wanderDir;

	public event Action<MobEntity>? Died;

	public override void _Ready()
	{
		CurrentHp = MaxHp;
		_spawnPos = GlobalPosition;
		AddToGroup("mob");
		if (MobId == "mob_default")
		{
			var col = new CollisionShape3D();
			var box = new BoxShape3D { Size = new Vector3(1.0f, 1.0f, 1.0f) };
			col.Shape = box;
			AddChild(col);
			var mesh = new MeshInstance3D();
			var cube = new BoxMesh { Size = new Vector3(1.0f, 1.0f, 1.0f) };
			var mat = new StandardMaterial3D { AlbedoColor = new Color(0.6f, 0.4f, 0.3f) };
			cube.SurfaceSetMaterial(0, mat);
			mesh.Mesh = cube;
			AddChild(mesh);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (IsDead) return;
		if (!IsOnFloor())
			Velocity = Velocity + new Vector3(0, -Gravity * (float)delta, 0);

		switch (Behavior)
		{
			case MobBehavior.Aggressive when Target != null:
				ChaseTarget((float)delta);
				break;
			case MobBehavior.NonAggressive:
				Wander((float)delta);
				break;
		}

		MoveAndSlide();
		_attackTimer = Math.Max(0, _attackTimer - (float)delta);
	}

	private void ChaseTarget(float delta)
	{
		if (Target == null) return;
		Vector3 toTarget = Target.GlobalPosition - GlobalPosition;
		float dist = toTarget.Length();
		if (dist > AggroRange * 2.5f)
		{
			Target = null;
			Velocity = new Vector3(0, Velocity.Y, 0);
			return;
		}
		if (dist > AttackRange)
		{
			Vector3 dir = toTarget.Normalized();
			Velocity = new Vector3(dir.X * MoveSpeed, Velocity.Y, dir.Z * MoveSpeed);
		}
		else
		{
			Velocity = new Vector3(0, Velocity.Y, 0);
			if (_attackTimer <= 0)
			{
				PerformAttack();
				_attackTimer = AttackCooldown;
			}
		}
	}

	private void Wander(float delta)
	{
		_wanderTimer -= delta;
		if (_wanderTimer <= 0)
		{
			_wanderTimer = 3.0f + GD.Randf() * 2.0f;
			float a = GD.Randf() * Mathf.Tau;
			_wanderDir = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
			if (GD.Randf() < 0.3f) _wanderDir = Vector3.Zero;
		}
		Vector3 toSpawn = _spawnPos - GlobalPosition;
		if (toSpawn.Length() > 8.0f) _wanderDir = toSpawn.Normalized();
		Velocity = new Vector3(_wanderDir.X * MoveSpeed * 0.4f, Velocity.Y, _wanderDir.Z * MoveSpeed * 0.4f);
	}

	private void PerformAttack()
	{
		if (Target == null) return;
		if (Target is CharacterBody3D cb && cb.HasMethod("TakeDamage"))
		{
			cb.Call("TakeDamage", Attack);
		}
	}

	public void TakeDamage(int amount)
	{
		if (IsDead) return;
		int dmg = Math.Max(1, amount - Defense);
		CurrentHp = Math.Max(0, CurrentHp - dmg);
		if (Behavior == MobBehavior.NonAggressive && Target == null)
		{
			Behavior = MobBehavior.Aggressive;
		}
		if (CurrentHp <= 0) Die();
	}

	private void Die()
	{
		Died?.Invoke(this);
		QueueFree();
	}
}