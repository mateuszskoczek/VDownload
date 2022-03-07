using VDownload.Core.Enums;
using VDownload.Core.Structs;

namespace VDownload.Core.EventArgs
{
    public class TasksAddingRequestedEventArgs : System.EventArgs
    {
        public TaskData[] TaskData { get; set; }
        public TaskAddingRequestSource RequestSource { get; set; }
    }
}
