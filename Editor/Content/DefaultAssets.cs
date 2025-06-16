using Editor.Common.Enums;
using Editor.DLLs;
using Editor.DLLs.Descriptors;
using Editor.Utilities;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Editor.Content
{
    static class DefaultAssets
    {
        public static AssetInfo BRDFIntegrationLUT { get; private set; }
        public static AssetInfo DefaultGeometry { get; private set; }
        public static AssetInfo DefaultMaterial { get; private set; }

        /// <summary>
        /// Generate default assets if necessary
        /// </summary>
        public static void GenerateDefaultAssets()
        {
            var defaultAssetsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\DefaultAssets");

            if (!Directory.Exists(defaultAssetsPath)) Directory.CreateDirectory(defaultAssetsPath);

            var brdfLUTFileName = $@"{defaultAssetsPath}BRDFIntegrationLUT{Asset.AssetFileExtension}";

            if (!File.Exists(brdfLUTFileName)) ComputeBRDFIntegrationLUT(brdfLUTFileName);

            var cubeFileName = $@"{defaultAssetsPath}DefaultCube{Asset.AssetFileExtension}";

            if (!File.Exists(cubeFileName)) CreateDefaultCube(cubeFileName);

            var materialFileName = $@"{defaultAssetsPath}DefaultMaterial{Asset.AssetFileExtension}";

            if (!File.Exists(materialFileName)) CreateDefaultMaterial(materialFileName);

            BRDFIntegrationLUT = Asset.GetAssetInfo(brdfLUTFileName);
            DefaultGeometry = Asset.GetAssetInfo(cubeFileName);
            DefaultMaterial = Asset.GetAssetInfo(materialFileName);
        }

        private static void ComputeBRDFIntegrationLUT(string file)
        {
            try
            {
                var brdfLUT = new Texture()
                {
                    FullPath = file,
                };

                ContentToolsAPI.ComputeBRDFIntegrationLUT(brdfLUT);
                brdfLUT.Save(file);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private static void CreateDefaultCube(string file)
        {
            try
            {
                var cube = new Geometry();

                var info = new PrimitiveInitInfo()
                {
                    Type = PrimitiveMeshType.CUBE,
                };

                ContentToolsAPI.CreatePrimitiveMesh(cube, info);
                cube.Save(file);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private static ShaderGroup CompileShaderGroup(
            ShaderType type,
            string code,
            string functionName,
            string[] defines,
            uint[] keys
        )
        {
            var extraArgs = new List<List<string>>();

            foreach (var def in defines)
            {
                extraArgs.Add(!string.IsNullOrEmpty(def.Trim()) ? new()
                {
                    "-D",
                    def
                } : new());
            }

            var shaderGroup = new ShaderGroup()
            {
                Type = type,
                Code = code,
                FunctionName = functionName,
                ExtraArgs = extraArgs,
                Keys = [.. keys]
            };

            EngineAPI.CompileShader(shaderGroup);

            return shaderGroup;
        }

        private static void CreateDefaultMaterial(string file)
        {
            var vsDefines = new[]
            {
                "ELEMENTS_TYPE=0",
                "ELEMENTS_TYPE=1",
                "ELEMENTS_TYPE=3"
            };

            var vsKeys = new[]
            {
                (uint)ElementsType.POSITION_ONLY,
                (uint)ElementsType.STATIC_NORMAL,
                (uint)ElementsType.STATIC_NORMAL_TEXTURE,
            };

            var psDefines = new[]
            {
                string.Empty,
            };

            var psKeys = new[]
            {
                (uint)Id.InvalidId,
            };

            try
            {
                var code = string.Empty;

                var shaderUri = ContentHelper.GetPackUri(
                    @"Resources/MaterialEditor/DefaultMaterialShaders.hlsl",
                    typeof(DefaultAssets)
                );

                var info = Application.GetResourceStream(shaderUri);

                using (var reader = new StreamReader(info.Stream)) code = reader.ReadToEnd();

                var vertexShaders = CompileShaderGroup(ShaderType.VERTEX, code, "main_vs", vsDefines, vsKeys);
                var pixelShaders = CompileShaderGroup(ShaderType.PIXEL, code, "main_ps", psDefines, psKeys);

                var material = new Material()
                {
                    MaterialMode = MaterialMode.DEFAULT,
                };

                material.AddShaderGroup(vertexShaders);
                material.AddShaderGroup(pixelShaders);
                material.Save(file);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
