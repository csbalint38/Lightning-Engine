using Editor.Common.Enums;
using Editor.Utilities;
using System.Diagnostics;
using System.IO;

namespace Editor.Content;

public class ShaderGroup
{
    private UploadedShaderGroup? _uploadedShader;

    public static readonly int HashSize = 16;

    public ShaderType Type { get; set; }
    public string? Code { get; set; }
    public string? FunctionName { get; set; }
    public List<List<string>> ExtraArgs { get; set; } = [];
    public List<uint> Keys { get; set; } = [];
    public List<byte[]> ByteCode { get; set; } = [];
    public List<string> Errors { get; set; } = [];
    public List<string> Assembly { get; set; } = [];
    public List<byte[]> Hash { get; set; } = [];
    public IdType ContentId { get; private set; } = Id.InvalidId;

    public int Count
    {
        get
        {
            Debug.Assert(new int[]
            {
                ExtraArgs.Count,
                Keys.Count,
                ByteCode.Count,
                Errors.Count,
                Assembly.Count,
                Hash.Count,
            }.Distinct().Count() == 1);

            return Keys.Count;
        }
    }

    public void ToBinary(BinaryWriter writer)
    {
        writer.Write((int)Type);
        writer.Write(Code!);
        writer.Write(FunctionName!);
        writer.Write(Count);

        ExtraArgs.ForEach(args => writer.Write(string.Join(';', args)));

        PackForEngine(writer);

        Errors.ForEach(writer.Write);
        Assembly.ForEach(writer.Write);
    }

    public void FromBinary(BinaryReader reader)
    {
        ExtraArgs.Clear();
        Keys.Clear();
        ByteCode.Clear();
        Errors.Clear();
        Assembly.Clear();
        Hash.Clear();

        Type = (ShaderType)reader.ReadInt32();
        Code = reader.ReadString();
        FunctionName = reader.ReadString();

        var count = reader.ReadInt32();

        ExtraArgs.AddRange(Enumerable.Range(0, count).Select(_ => reader.ReadString().Split(';').ToList()));
        Keys.AddRange(Enumerable.Range(0, count).Select(_ => reader.ReadUInt32()));

        for (int i = 0; i < count; i++)
        {
            var byteCodeLength = reader.ReadInt64();

            if (byteCodeLength > 0)
            {
                Hash.Add(reader.ReadBytes(HashSize));
                ByteCode.Add(reader.ReadBytes((int)byteCodeLength));
            }
        }

        Errors.AddRange(Enumerable.Range(0, count).Select(_ => reader.ReadString()));
        Assembly.AddRange(Enumerable.Range(0, count).Select(_ => reader.ReadString()));
    }

    public byte[]? PackForEngine()
    {
        if (Count == 0 || ByteCode.Any(x => x.Length == 0) || Hash.Any(x => x.Length == 0)) return null;

        using var writer = new BinaryWriter(new MemoryStream());

        PackForEngine(writer);
        writer.Flush();

        return (writer.BaseStream as MemoryStream)?.ToArray();
    }

    public IdType UploadToEngine()
    {
        var uploadedShader = UploadedShaderGroup.UploadToEngine(this);

        Debug.Assert(uploadedShader is not null && Id.IsValid(uploadedShader.ContentId));

        if (uploadedShader is null || !Id.IsValid(uploadedShader.ContentId)) return Id.InvalidId;

        _uploadedShader = uploadedShader;
        ContentId = uploadedShader.ContentId;

        return ContentId;
    }

    public void UnloadFromEngine()
    {
        Debug.Assert(Id.IsValid(ContentId) && _uploadedShader is not null);

        UploadedShaderGroup.UnloadFromEngine(ContentId);

        if (_uploadedShader.ReferenceCount == 0)
        {
            ContentId = Id.InvalidId;
            _uploadedShader = null;
        }
    }

    private void PackForEngine(BinaryWriter writer)
    {
        Keys.ForEach(key => writer.Write(key));

        for (int i = 0; i < Count; i++)
        {
            var byteCodeLength = ByteCode[i].LongLength;

            writer.Write(byteCodeLength);

            if (byteCodeLength > 0)
            {
                writer.Write(Hash[i]);
                writer.Write(ByteCode[i]);
            }
        }
    }
}
