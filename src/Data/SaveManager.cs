using System;
using System.Globalization;
using Godot;

namespace Odysseus.Data;

public sealed partial class SaveManager : Node
{
	public static SaveManager Instance { get; private set; } = null!;

	private Database? _db;
	private float _autosaveAccum;
	private const float AutosaveIntervalSeconds = 30.0f;

	public Database Db
	{
		get
		{
			if (_db == null) throw new InvalidOperationException("SaveManager not initialised.");
			return _db;
		}
	}

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _Ready()
	{
		string savePath = ProjectSettings.GlobalizePath("user://save.sqlite");
		string schemaPath = ProjectSettings.GlobalizePath("res://src/Data/Schema.sql");
		string schemaSql = FileAccess.GetFileAsString(schemaPath);
		_db = Database.Open(savePath, schemaSql);
		GD.Print($"[SaveManager] DB open at {savePath} (schema v{_db.LoadedSchemaVersion})");
	}

	public override void _Process(double delta)
	{
		_autosaveAccum += (float)delta;
		if (_autosaveAccum < AutosaveIntervalSeconds) return;
		_autosaveAccum = 0.0f;
		Flush();
	}

	public void Flush()
	{
		if (_db == null) return;
		_db.Execute("UPDATE player SET updated_at = @now WHERE id = 1;",
			("now", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)));
	}

	public bool HasPlayerExists()
	{
		if (_db == null) return false;
		return (long?)_db.Scalar("SELECT COUNT(*) FROM player WHERE id = 1;") > 0;
	}

	public override void _ExitTree()
	{
		Flush();
		_db?.Dispose();
		_db = null;
	}
}