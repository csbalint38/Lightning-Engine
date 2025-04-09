namespace Editor.Content.ContentBrowser.Descriptors
{
    public class ContentModifiedEventArgs : EventArgs
    {
        public string FullPath { get; }

        public ContentModifiedEventArgs(string path) {
            FullPath = path;
        }
    }
}
