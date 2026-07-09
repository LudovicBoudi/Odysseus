using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Odysseus.Combat;

public sealed partial class CombatManager : Node
{
	public static CombatManager Instance { get; private set; } = null!;

	private Node3D? _playerNode;
	private readonly List<Node3D> _nearbyTargets = new();
	private Node3D? _currentTarget;
	private float _scanAccum;

	public Node3D? CurrentTarget => _currentTarget;
	public event System.Action<Node3D?>? TargetChanged;

	public override void _EnterTree() => Instance = this;

	public void SetPlayerNode(Node3D player)
	{
		_playerNode = player;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_playerNode == null) return;
		_scanAccum += (float)delta;
		if (_scanAccum > 0.5f)
		{
			_scanAccum = 0;
			ScanNearbyTargets();
		}
	}

	private void ScanNearbyTargets()
	{
		_nearbyTargets.Clear();
		Vector3 origin = _playerNode!.GlobalPosition;
		foreach (var n in GetTree().GetNodesInGroup("mob"))
		{
			if (n is Node3D node && node.HasMethod("TakeDamage"))
			{
				_nearbyTargets.Add(node);
			}
		}
		_nearbyTargets.Sort((a, b) =>
			a.GlobalPosition.DistanceTo(origin).CompareTo(b.GlobalPosition.DistanceTo(origin)));
		if (_currentTarget != null && !GodotObject.IsInstanceValid(_currentTarget))
		{
			_currentTarget = null;
			TargetChanged?.Invoke(null);
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (_playerNode == null) return;
		if (Input.IsActionJustPressed("toggle_target"))
		{
			CycleTarget();
			GetViewport().SetInputAsHandled();
		}
		else if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
		{
			TryClickTarget(mb);
		}
	}

	private void CycleTarget()
	{
		if (_nearbyTargets.Count == 0) return;
		int idx = _currentTarget != null ? _nearbyTargets.IndexOf(_currentTarget) : -1;
		for (int i = 1; i <= _nearbyTargets.Count; i++)
		{
			int next = (idx + i) % _nearbyTargets.Count;
			var t = _nearbyTargets[next];
			if (GodotObject.IsInstanceValid(t) && t.HasMethod("TakeDamage"))
			{
				SetTarget(t);
				return;
			}
		}
	}

	private void TryClickTarget(InputEventMouseButton mb)
	{
		Vector2 screenPos = mb.Position;
		var viewport = GetViewport();
		var cam = viewport.GetCamera3D();
		if (cam == null) return;
		var from = cam.ProjectRayOrigin(screenPos);
		var dir = cam.ProjectRayNormal(screenPos);
		var space = viewport.FindWorld3D().DirectSpaceState;
		var query = PhysicsRayQueryParameters3D.Create(from, from + dir * 100, 0xFFFFFFFF);
		var result = space.IntersectRay(query);
		if (result.Count > 0 && result["collider"].Obj is Node n && n.HasMethod("TakeDamage"))
		{
			SetTarget((Node3D)n);
		}
	}

	private void SetTarget(Node3D? target)
	{
		_currentTarget = target;
		TargetChanged?.Invoke(target);
		if (_currentTarget != null) GD.Print($"[Combat] Target = {_currentTarget.Name}");
		else GD.Print("[Combat] Target cleared.");
	}
}