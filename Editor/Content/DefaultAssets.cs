using Editor.Common.Enums;
using Editor.DLLs;
using Editor.DLLs.Descriptors;
using System.Diagnostics;
using System.IO;

namespace Editor.Content
{
    static class DefaultAssets
    {
        public static AssetInfo BRDFIntegrationLUT { get; private set; }
        public static AssetInfo DefaultGeometry {  get; private set; }

        /// <summary>
        /// Generate default assets if necessary
        /// </summary>
        public static void GenerateDefaultAssets()
        {
            var defaultAssetsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @".\Resources\DefaultAssets");

            if(!Directory.Exists(defaultAssetsPath)) Directory.CreateDirectory(defaultAssetsPath);

            var brdfLUTFileName = $@"{defaultAssetsPath}BRDFIntegrationLUT{Asset.AssetFileExtension}";

            if (!File.Exists(brdfLUTFileName)) ComputeBRDFIntegrationLUT(brdfLUTFileName);

            var cubeFileName = $@"{defaultAssetsPath}DefaultCube{Asset.AssetFileExtension}";

            if (!File.Exists(cubeFileName)) CreateDefaultCube(cubeFileName);
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

                BRDFIntegrationLUT = Asset.GetAssetInfo(file);
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

                DefaultGeometry = Asset.GetAssetInfo(file);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
