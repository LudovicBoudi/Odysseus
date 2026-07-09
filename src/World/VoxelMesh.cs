using System;
using System.Collections.Generic;
using System.IO;
using Godot;

namespace Odysseus.World;

public sealed partial class VoxelMesh : MeshInstance3D
{
	[Export] public string VoxPath { get; set; } = "";
	[Export] public float VoxelSize { get; set; } = 0.1f;
	[Export] public bool GenerateCollision { get; set; } = true;
	[Export] public bool CenterMesh { get; set; } = true;

	private static readonly Vector3[][] FaceVerts =
	{
		new[] { new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,0,1), new Vector3(0,0,1) },
		new[] { new Vector3(0,1,1), new Vector3(1,1,1), new Vector3(1,1,0), new Vector3(0,1,0) },
		new[] { new Vector3(0,0,1), new Vector3(0,1,1), new Vector3(0,1,0), new Vector3(0,0,0) },
		new[] { new Vector3(1,0,0), new Vector3(1,1,0), new Vector3(1,1,1), new Vector3(1,0,1) },
		new[] { new Vector3(0,0,0), new Vector3(0,1,0), new Vector3(1,1,0), new Vector3(1,0,0) },
		new[] { new Vector3(0,0,1), new Vector3(1,0,1), new Vector3(1,1,1), new Vector3(0,1,1) },
	};

	public override void _Ready()
	{
		if (!string.IsNullOrEmpty(VoxPath))
			LoadVox(VoxPath);
	}

	public void LoadVox(string path)
	{
		var resolved = ProjectSettings.GlobalizePath(path);
		if (!File.Exists(resolved))
		{
			GD.PrintErr($"[VoxelMesh] .vox not found: {path}");
			return;
		}

		VoxData? vox = null;
		try
		{
			vox = ParseVoxFile(resolved);
		}
		catch (Exception ex)
		{
			GD.PrintErr($"[VoxelMesh] Exception parsing {path}: {ex.Message}");
			return;
		}
		if (vox == null)
		{
			GD.PrintErr($"[VoxelMesh] Failed to parse: {path}");
			return;
		}

		Mesh? mesh = BuildMesh(vox);
		if (mesh == null) return;
		Mesh = mesh;

		if (GenerateCollision)
		{
			var col = new ConcavePolygonShape3D();
			col.SetFaces(FacesFromVox(vox));
			var body = new StaticBody3D();
			var shape = new CollisionShape3D { Shape = col };
			body.AddChild(shape);
			AddChild(body);
		}

		GD.Print($"[VoxelMesh] Loaded {path} : {vox.Voxels.Count} voxels -> {mesh.GetSurfaceCount()} surface(s)");
	}

	private sealed class VoxData
	{
		public int SizeX, SizeY, SizeZ;
		public List<(byte x, byte y, byte z, byte colorIdx)> Voxels = new();
		public Color[] Palette = new Color[256];
	}

	private VoxData? ParseVoxFile(string path)
	{
		byte[] bytes;
		try { bytes = File.ReadAllBytes(path); }
		catch { return null; }

		if (bytes.Length < 12) return null;
		if (ReadAscii(bytes, 0, 4) != "VOX ") return null;
		int version = BitConverter.ToInt32(bytes, 4);
		if (version > 200) return null;

		var state = new ParseState(bytes, 8);
		if (!state.ExpectChunk("MAIN")) return null;
		state.SkipChunkContent();

		var data = new VoxData();
		while (state.Remaining >= 12)
		{
			string id = state.ReadChunkId();
			int contentSize = state.ReadInt();
			int childrenSize = state.ReadInt();
			int contentEnd = state.Pos + contentSize;
			int childrenEnd = contentEnd + childrenSize;

			if (id == "SIZE")
			{
				data.SizeX = state.ReadInt();
				data.SizeY = state.ReadInt();
				data.SizeZ = state.ReadInt();
			}
			else if (id == "XYZI")
			{
				int count = state.ReadInt();
				for (int i = 0; i < count; i++)
				{
					byte x = (byte)state.ReadByte();
					byte y = (byte)state.ReadByte();
					byte z = (byte)state.ReadByte();
					byte colorIdx = (byte)state.ReadByte();
					data.Voxels.Add((x, y, z, colorIdx));
				}
			}
			else if (id == "RGBA")
			{
				int count = contentSize / 4;
				for (int i = 0; i < count; i++)
				{
					byte r = (byte)state.ReadByte();
					byte g = (byte)state.ReadByte();
					byte b = (byte)state.ReadByte();
					byte a = (byte)state.ReadByte();
					data.Palette[i] = new Color(r / 255f, g / 255f, b / 255f, a / 255f);
				}
			}
			state.Pos = childrenEnd;
		}
		return data;
	}

	private sealed class ParseState
	{
		public byte[] Data;
		public int Pos;
		public ParseState(byte[] d, int p) { Data = d; Pos = p; }
		public int Remaining => Data.Length - Pos;
		public int ReadInt() { int v = BitConverter.ToInt32(Data, Pos); Pos += 4; return v; }
		public byte ReadByte() { return Data[Pos++]; }
		public string ReadChunkId()
		{
			string s = ReadAscii(Data, Pos, 4);
			Pos += 4;
			return s;
		}
		public bool ExpectChunk(string id) { return ReadChunkId() == id; }
		public void SkipChunkContent() { ReadInt(); ReadInt(); }
	}

	private static string ReadAscii(byte[] d, int offset, int len)
	{
		char[] chars = new char[len];
		for (int i = 0; i < len; i++) chars[i] = (char)d[offset + i];
		return new string(chars);
	}

	private Mesh? BuildMesh(VoxData vox)
	{
		if (vox.Voxels.Count == 0) return null;
		var st = new SurfaceTool();
		st.Begin(Mesh.PrimitiveType.Triangles);

		Vector3 offset = Vector3.Zero;
		if (CenterMesh)
			offset = new Vector3(-vox.SizeX * VoxelSize * 0.5f, 0, -vox.SizeZ * VoxelSize * 0.5f);

foreach (var v in vox.Voxels)
		{
			Color c = (v.colorIdx >= 1 && v.colorIdx <= 255) ? vox.Palette[v.colorIdx - 1] : new Color(0.7f, 0.55f, 0.4f);
			if (c == default) c = new Color(0.7f, 0.55f, 0.4f);
			Vector3 baseOrigin = offset + new Vector3(v.x, v.z, v.y) * VoxelSize;

			for (int face = 0; face < 6; face++)
			{
				if (!IsFaceVisible(v, face, vox)) continue;
				var verts = FaceVerts[face];
				Vector3 q0 = verts[0], q1 = verts[1], q2 = verts[2], q3 = verts[3];
				st.SetColor(c); st.AddVertex(baseOrigin + q0 * VoxelSize);
				st.SetColor(c); st.AddVertex(baseOrigin + q1 * VoxelSize);
				st.SetColor(c); st.AddVertex(baseOrigin + q2 * VoxelSize);
				st.SetColor(c); st.AddVertex(baseOrigin + q0 * VoxelSize);
				st.SetColor(c); st.AddVertex(baseOrigin + q2 * VoxelSize);
				st.SetColor(c); st.AddVertex(baseOrigin + q3 * VoxelSize);
			}
		}

		st.GenerateNormals();
		return st.Commit();
	}

	private static bool IsFaceVisible((byte x, byte y, byte z, byte colorIdx) v, int face, VoxData vox)
	{
		int dx = 0, dy = 0, dz = 0;
		switch (face)
		{
			case 0: dz = -1; break;
			case 1: dz = 1; break;
			case 2: dx = -1; break;
			case 3: dx = 1; break;
			case 4: dy = -1; break;
			case 5: dy = 1; break;
		}
		int nx = v.x + dx, ny = v.y + dy, nz = v.z + dz;
		if (nx < 0 || ny < 0 || nz < 0) return true;
		if (nx >= vox.SizeX || ny >= vox.SizeY || nz >= vox.SizeZ) return true;
		return false;
	}

	private Vector3[] FacesFromVox(VoxData vox)
	{
		var faces = new List<Vector3>();
		Vector3 offset = CenterMesh ? new Vector3(-vox.SizeX * VoxelSize * 0.5f, 0, -vox.SizeZ * VoxelSize * 0.5f) : Vector3.Zero;
		foreach (var v in vox.Voxels)
		{
			Vector3 baseOrigin = offset + new Vector3(v.x, v.z, v.y) * VoxelSize;
			for (int face = 0; face < 6; face++)
			{
				if (!IsFaceVisible(v, face, vox)) continue;
				var verts = FaceVerts[face];
				Vector3 q0 = baseOrigin + verts[0] * VoxelSize;
				Vector3 q1 = baseOrigin + verts[1] * VoxelSize;
				Vector3 q2 = baseOrigin + verts[2] * VoxelSize;
				Vector3 q3 = baseOrigin + verts[3] * VoxelSize;
				faces.Add(q0); faces.Add(q1); faces.Add(q2);
				faces.Add(q0); faces.Add(q2); faces.Add(q3);
			}
		}
		return faces.ToArray();
	}
}