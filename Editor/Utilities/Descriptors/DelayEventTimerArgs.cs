namespace Editor.Utilities.Descriptors
{
    internal class DelayEventTimerArgs : EventArgs
    {
        public bool RepeatEvent { get; set; }
        public object Data { get; set; }

        public DelayEventTimerArgs(object data)
        {
            Data = data;
        }
    }
}
