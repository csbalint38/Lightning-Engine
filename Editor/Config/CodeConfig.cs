using Editor.Common;
using Editor.Common.Enums;
using Editor.GameCode;
using System.Text.Json.Serialization;

namespace Editor.Config
{
    public class CodeConfig : ViewModelBase
    {
        private CodeEditor _codeEditor;
        private string _msbuildPath = string.Empty;


        [JsonPropertyName("code_editor")]
        public required CodeEditor CodeEditor
        {
            get => _codeEditor;
            set {
                if (_codeEditor != value)
                {
                    _codeEditor = value;

                    OnPropertyChanged(nameof(CodeEditor));
                    ICodeEditor.SetCurrent(value);
                }
            }
        }

        [JsonPropertyName("msbuild_path")]
        public string MSBuildPath
        {
            get => _msbuildPath;
            set {
                if (_msbuildPath != value)
                {
                    _msbuildPath = value;
                    OnPropertyChanged(nameof(MSBuildPath));
                }
            }
        }
    }
}
