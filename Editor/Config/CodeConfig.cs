using Editor.Common;
using Editor.Common.Enums;
using System.ComponentModel;
using System.IO;
using System.Text.Json.Serialization;

namespace Editor.Config
{
    public class CodeConfig : ViewModelBase, IDataErrorInfo
    {
        private CodeEditor _codeEditor;
        private string _msbuildPath = string.Empty;

        public string Error => string.Empty;

        public string this[string columnName]
        {
            get
            {
                return columnName switch
                {
                    nameof(MSBuildPath) => ValidateMSBuildPath(),
                    _ => string.Empty,
                };
            }
        }

        [JsonPropertyName("code_editor")]
        public required CodeEditor CodeEditor
        {
            get => _codeEditor;
            set
            {
                if (_codeEditor != value)
                {
                    _codeEditor = value;
                    OnPropertyChanged(nameof(CodeEditor));
                }
            }
        }

        [JsonPropertyName("msbuild_path")]
        public string MSBuildPath
        {
            get => _msbuildPath;
            set
            {
                if (_msbuildPath != value)
                {
                    _msbuildPath = value;
                    OnPropertyChanged(nameof(MSBuildPath));
                }
            }
        }

        public string ValidateMSBuildPath()
        {
            if (string.IsNullOrWhiteSpace(MSBuildPath)) return "MSBuild path cannot be empty.";
            if (!File.Exists(MSBuildPath)) return "MSBuild path does not exist.";

            var fileName = Path.GetFileName(MSBuildPath);

            if (!string.Equals(fileName, "MSBuild.exe", StringComparison.OrdinalIgnoreCase))
            {
                return "MSBuild path must point to msbuild executable.";
            }

            return string.Empty;
        }
    }
}
