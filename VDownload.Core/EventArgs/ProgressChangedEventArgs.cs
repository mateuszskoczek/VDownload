namespace VDownload.Core.EventArgs
{
    public class ProgressChangedEventArgs : System.EventArgs
    {
        #region CONSTRUCTORS

        public ProgressChangedEventArgs(double progress, bool isCompleted = false)
        {
            Progress = progress;
            IsCompleted = isCompleted;
        }

        #endregion



        #region PROPERTIES

        public double Progress { get; private set; }
        public bool IsCompleted { get; private set; }

        #endregion
    }
}
