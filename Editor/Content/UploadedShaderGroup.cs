using Editor.DLLs;
using Editor.Utilities;
using System.Diagnostics;

namespace Editor.Content
{
    internal sealed class UploadedShaderGroup
    {
        private static readonly Lock _lock = new();
        private static readonly Dictionary<string, UploadedShaderGroup> _uploadedShaders = [];
        private static readonly Dictionary<IdType, UploadedShaderGroup> _uploadedShaderIds = [];

        public IdType ContentId { get; private set; } = Id.InvalidId;
        public byte[] CombinedHashes { get; private set; } = [];
        public int ReferenceCount { get; private set; }

        public static UploadedShaderGroup? UploadToEngine(ShaderGroup shaderGroup)
        {
            if (shaderGroup.Count == 0 ||
                shaderGroup.ByteCode.Any(x => x.Length == 0) ||
                shaderGroup.Hash.Any(x => x.Length == 0)) return null;

            lock (_lock)
            {
                var combinedHashes = shaderGroup.Hash.SelectMany(x => x).ToArray();

                if (Id.IsValid(shaderGroup.ContentId) &&
                    _uploadedShaderIds.TryGetValue(shaderGroup.ContentId, out var uploadedShader)
                )
                {
                    if (uploadedShader.CombinedHashes.SequenceEqual(combinedHashes))
                    {
                        ++uploadedShader.ReferenceCount;

                        return uploadedShader;
                    }
                    else UnloadFromEngine(uploadedShader.ContentId);
                }
                else Debug.Assert(!Id.IsValid(shaderGroup.ContentId));

                var hashString = Convert.ToBase64String(combinedHashes);

                if (_uploadedShaders.TryGetValue(hashString, out var identicalShader))
                {
                    ++identicalShader.ReferenceCount;

                    return identicalShader;
                }

                var newUploadedShader = new UploadedShaderGroup()
                {
                    ContentId = EngineAPI.AddShaderGroup(shaderGroup),
                    CombinedHashes = combinedHashes,
                    ReferenceCount = 1
                };

                Debug.Assert(Id.IsValid(newUploadedShader.ContentId));

                _uploadedShaders.Add(hashString, newUploadedShader);
                _uploadedShaderIds.Add(newUploadedShader.ContentId, newUploadedShader);

                return newUploadedShader;
            }
        }

        public static void UnloadFromEngine(IdType id)
        {
            lock (_lock)
            {
                Debug.Assert(Id.IsValid(id) && _uploadedShaderIds.ContainsKey(id));

                if (Id.IsValid(id) && _uploadedShaderIds.TryGetValue(id, out var uploadedShader))
                {
                    Debug.Assert(uploadedShader.ReferenceCount > 0);

                    --uploadedShader.ReferenceCount;

                    if (uploadedShader.ReferenceCount == 0)
                    {
                        EngineAPI.RemoveShaderGroup(uploadedShader.ContentId);

                        var hashString = Convert.ToBase64String(uploadedShader.CombinedHashes);

                        Debug.Assert(_uploadedShaders.ContainsKey(hashString));

                        _uploadedShaders.Remove(hashString);
                        _uploadedShaderIds.Remove(uploadedShader.ContentId);
                    }
                }
            }
        }

        internal UploadedShaderGroup() { }
    }
}
