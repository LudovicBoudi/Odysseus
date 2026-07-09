using System;
using Godot;

namespace Odysseus.Entities.Player;

public sealed partial class PlayerController : CharacterBody3D
{
	[Export] public float MaxSpeed { get; set; } = 5.0f;
	[Export] public float Acceleration { get; set; } = 12.0f;
	[Export] public float RotationSpeed { get; set; } = 10.0f;
	[Export] public float Gravity { get; set; } = 9.8f;
	[Export] public string CharacterScenePath { get; set; } = "res://assets/Universal Animation Library[Standard]/Unreal-Godot/UAL1_Standard.glb";

	public bool InputEnabled { get; set; } = true;

	private AnimationPlayer? _animPlayer;
	private string _currentAnim = "";
	private string _idleAnim = "";
	private string _walkAnim = "";
	private Node3D? _characterModel;
	private bool _animationsLoaded;

	public override void _Ready()
	{
		LoadCharacterModel();
	}

	private void LoadCharacterModel()
	{
		if (string.IsNullOrEmpty(CharacterScenePath)) return;
		var scene = ResourceLoader.Load<PackedScene>(CharacterScenePath);
		if (scene == null)
		{
			GD.PushWarning($"[Player] Character scene not found: {CharacterScenePath}");
			return;
		}
		_characterModel = scene.Instantiate<Node3D>();
		AddChild(_characterModel);
		_characterModel.Rotation = new Vector3(0, Mathf.Pi, 0);

		_animPlayer = FindFirstAnimPlayer(_characterModel);
		if (_animPlayer == null)
		{
			GD.PushWarning("[Player] No AnimationPlayer found in character scene.");
			return;
		}

		BuildAnimMap();
		TryLoadLibrariesFromImportedScene();

		if (!string.IsNullOrEmpty(_idleAnim))
		{
			_currentAnim = _idleAnim;
			_animPlayer.Play(_idleAnim);
			if (_animPlayer.HasAnimation(_idleAnim))
				_animPlayer.GetAnimation(_idleAnim).LoopMode = Animation.LoopModeEnum.Linear;
			_animationsLoaded = true;
			GD.Print($"[Player] Animations loaded: idle={_idleAnim}, walk={_walkAnim}");
		}
		else
		{
			GD.PushWarning("[Player] No idle/walk animations found — character will stay in T-Pose. (Phase 1.A: deferred)");
		}
	}

	private void TryLoadLibrariesFromImportedScene()
	{
		if (_animPlayer == null) return;
		foreach (string libName in _animPlayer.GetAnimationLibraryList())
		{
			var lib = _animPlayer.GetAnimationLibrary(libName);
			foreach (string animName in lib.GetAnimationList())
			{
				string full = $"{libName}/{animName}";
				string lower = animName.ToLowerInvariant();
				if (lower.Contains("idle") || lower.Contains("stance")) _idleAnim ??= full;
				else if (lower.Contains("walk") || lower.Contains("jog")) _walkAnim ??= full;
				else if (lower.Contains("run") && _walkAnim == null) _walkAnim = full;
			}
		}
	}

	public void LoadAnimationLibraryFromExternal(AnimationLibrary lib, string libName)
	{
		if (_animPlayer == null) return;
		_animPlayer.AddAnimationLibrary(libName, lib);
		BuildAnimMap();
		if (!string.IsNullOrEmpty(_idleAnim) && !_animationsLoaded)
		{
			_currentAnim = _idleAnim;
			_animPlayer.Play(_idleAnim);
			if (_animPlayer.HasAnimation(_idleAnim))
				_animPlayer.GetAnimation(_idleAnim).LoopMode = Animation.LoopModeEnum.Linear;
			_animationsLoaded = true;
		}
	}

	private void BuildAnimMap()
	{
		if (_animPlayer == null) return;
		foreach (string libName in _animPlayer.GetAnimationLibraryList())
		{
			var lib = _animPlayer.GetAnimationLibrary(libName);
			foreach (string animName in lib.GetAnimationList())
			{
				string full = $"{libName}/{animName}";
				string lower = animName.ToLowerInvariant();
				if (lower.Contains("idle") || lower.Contains("stance")) _idleAnim ??= full;
				else if (lower.Contains("walk") || lower.Contains("jog")) _walkAnim ??= full;
				else if (lower.Contains("run") && _walkAnim == null) _walkAnim = full;
			}
		}
	}

	private static AnimationPlayer? FindFirstAnimPlayer(Node root)
	{
		if (root is AnimationPlayer ap) return ap;
		foreach (var c in root.GetChildren())
		{
			var found = FindFirstAnimPlayer(c);
			if (found != null) return found;
		}
		return null;
	}

	private void UpdateAnimation()
	{
		if (!_animationsLoaded || _animPlayer == null) return;
		string desired = Velocity.LengthSquared() > 0.25f ? _walkAnim : _idleAnim;
		if (string.IsNullOrEmpty(desired)) return;
		if (desired != _currentAnim)
		{
			_currentAnim = desired;
			_animPlayer.Play(desired);
			if (_animPlayer.HasAnimation(desired))
				_animPlayer.GetAnimation(desired).LoopMode = Animation.LoopModeEnum.Linear;
		}
	}

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
			Velocity = Velocity.MoveToward(direction * MaxSpeed, Acceleration * (float)delta);
		else
			Velocity = Velocity.MoveToward(Vector3.Zero, Acceleration * (float)delta);

		MoveAndSlide();
		UpdateAnimation();
	}

	private void ApplyGravity(double delta)
	{
		if (!IsOnFloor())
			Velocity = Velocity + new Vector3(0, -Gravity * (float)delta, 0);
	}
}