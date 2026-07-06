using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace Odysseus.Data;

public sealed class Database : IDisposable
{
	private const int CurrentSchemaVersion = 1;

	private readonly SqliteConnection _connection;
	private bool _disposed;

	public SqliteConnection Connection => _connection;
	public string Path { get; }
	public int LoadedSchemaVersion { get; private set; }

	private Database(string path, SqliteConnection connection)
	{
		Path = path;
		_connection = connection;
	}

	public static Database Open(string filePath, string? schemaSql = null)
	{
		string? dir = System.IO.Path.GetDirectoryName(filePath);
		if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
			Directory.CreateDirectory(dir);

		string connStr = $"Data Source={filePath};Foreign Keys=True;Mode=ReadWriteCreate";
		var conn = new SqliteConnection(connStr);
		conn.Open();

		Exec(conn, "PRAGMA journal_mode = WAL;");
		Exec(conn, "PRAGMA foreign_keys = ON;");
		EnsureMetaTable(conn);

		var db = new Database(filePath, conn);
		db.LoadedSchemaVersion = ReadSchemaVersion(conn);

		if (db.LoadedSchemaVersion < CurrentSchemaVersion)
		{
			if (string.IsNullOrEmpty(schemaSql))
				throw new InvalidOperationException(
					"Schema SQL required to bootstrap database but none was provided.");
			ApplyMigrations(conn, db.LoadedSchemaVersion, schemaSql);
			WriteSchemaVersion(conn, CurrentSchemaVersion);
			db.LoadedSchemaVersion = CurrentSchemaVersion;
		}

		return db;
	}

	public SqliteCommand CreateCommand()
	{
		return _connection.CreateCommand();
	}

	public int Execute(string sql, params (string, object?)[] args)
	{
		using var cmd = _connection.CreateCommand();
		cmd.CommandText = sql;
		foreach (var (name, value) in args)
		{
			cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
		}
		return cmd.ExecuteNonQuery();
	}

	public object? Scalar(string sql, params (string, object?)[] args)
	{
		using var cmd = _connection.CreateCommand();
		cmd.CommandText = sql;
		foreach (var (name, value) in args)
			cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
		return cmd.ExecuteScalar();
	}

	private static void EnsureMetaTable(SqliteConnection conn)
	{
		Exec(conn, "CREATE TABLE IF NOT EXISTS meta (key TEXT PRIMARY KEY, value TEXT NOT NULL);");
	}

	private static int ReadSchemaVersion(SqliteConnection conn)
	{
		using var cmd = conn.CreateCommand();
		cmd.CommandText = "SELECT value FROM meta WHERE key = 'schema_version';";
		var result = cmd.ExecuteScalar();
		return result == null ? 0 : int.Parse((string)result);
	}

	private static void WriteSchemaVersion(SqliteConnection conn, int version)
	{
		using var cmd = conn.CreateCommand();
		cmd.CommandText = "INSERT INTO meta(key, value) VALUES('schema_version', @v) " +
			"ON CONFLICT(key) DO UPDATE SET value = excluded.value;";
		cmd.Parameters.AddWithValue("@v", version.ToString());
		cmd.ExecuteNonQuery();
	}

	private static void ApplyMigrations(SqliteConnection conn, int fromVersion, string schemaSql)
	{
		using var tx = conn.BeginTransaction();
		try
		{
			if (fromVersion < 1)
			{
				Exec(conn, schemaSql, tx);
			}
			tx.Commit();
		}
		catch
		{
			tx.Rollback();
			throw;
		}
	}

	private static void Exec(SqliteConnection conn, string sql, SqliteTransaction? tx = null)
	{
		using var cmd = conn.CreateCommand();
		cmd.CommandText = sql;
		if (tx != null) cmd.Transaction = tx;
		cmd.ExecuteNonQuery();
	}

	public void Dispose()
	{
		if (_disposed) return;
		_disposed = true;
		try { _connection.Close(); _connection.Dispose(); } catch { }
	}
}