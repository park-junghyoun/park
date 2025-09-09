namespace System.Windows
{
    public enum MessageBoxButton { OK }
    public enum MessageBoxImage { Error }
    public enum MessageBoxResult { OK }
    public static class MessageBox
    {
        public static MessageBoxResult Show(string text, string caption, MessageBoxButton button, MessageBoxImage icon)
            => MessageBoxResult.OK;
    }
}
