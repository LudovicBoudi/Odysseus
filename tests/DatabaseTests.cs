using System;
using System.IO;
using Xunit;
using Odysseus.Data;

namespace Odysseus.Tests;

public class DatabaseTests : IDisposable
{
	private readonly string _tmp;
	private readonly Database _db;
	private const string Schema = """
		PRAGMA foreign_keys = ON;
		CREATE TABLE IF NOT EXISTS meta (key TEXT PRIMARY KEY, value TEXT NOT NULL);
		CREATE TABLE IF NOT EXISTS player (
			id INTEGER PRIMARY KEY CHECK (id = 1),
			name TEXT NOT NULL,
			level INTEGER NOT NULL DEFAULT 1
		);
		CREATE TABLE IF NOT EXISTS equipment_slot (
			slot TEXT PRIMARY KEY,
			inventory_item_id INTEGER
		);
	""";

	public DatabaseTests()
	{
		_tmp = Path.Combine(Path.GetTempPath(), $"ody_test_{Guid.NewGuid():N}.sqlite");
		_db = Database.Open(_tmp, Schema);
	}

	public void Dispose()
	{
		_db.Dispose();
		try { File.Delete(_tmp); } catch { }
	}

	[Fact]
	public void Open_CreatesDbFile_And_SetsSchemaVersion()
	{
		Assert.Equal(1, _db.LoadedSchemaVersion);
		Assert.True(File.Exists(_tmp));
	}

	[Fact]
	public void Reopen_OnExistingDb_DoesNotResetSchemaVersion()
	{
		_db.Dispose();
		var db2 = Database.Open(_tmp, Schema);
		Assert.Equal(1, db2.LoadedSchemaVersion);
		db2.Dispose();
	}

	[Fact]
	public void Execute_Inserts_And_Scalar_ReadsBack()
	{
		_db.Execute("INSERT INTO player(id, name, level) VALUES(1, @name, @lvl);",
			("name", "Perseus"), ("lvl", 12));
		var count = (long?)_db.Scalar("SELECT COUNT(*) FROM player;");
		Assert.Equal(1L, count);
		var name = (string?)_db.Scalar("SELECT name FROM player WHERE id = 1;");
		Assert.Equal("Perseus", name);
	}

	[Fact]
	public void ForeignKeys_AreEnabled()
	{
		var fk = (long?)_db.Scalar("PRAGMA foreign_keys;");
		Assert.Equal(1L, fk);
	}

	[Fact]
	public void MetaTable_StoresSchemaVersion()
	{
		var v = (string?)_db.Scalar("SELECT value FROM meta WHERE key = 'schema_version';");
		Assert.Equal("1", v);
	}

	[Fact]
	public void HasPlayerExists_ReturnsFalse_OnFreshDb()
	{
		Assert.False(_db is null);
		var count = (long?)_db.Scalar("SELECT COUNT(*) FROM player WHERE id = 1;");
		Assert.Equal(0L, count);
	}
}