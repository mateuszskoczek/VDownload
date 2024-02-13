using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Common.Models
{
    public abstract partial class Video : ObservableObject
    {
        #region PROPERTIES

        [ObservableProperty]
        protected string _title;

        [ObservableProperty]
        protected string _description;

        [ObservableProperty]
        protected string _author;

        [ObservableProperty]
        protected DateTime _publishDate;

        [ObservableProperty]
        protected TimeSpan _duration;

        [ObservableProperty]
        protected int _viewCount;

        [ObservableProperty]
        protected string? _thumbnailUrl;

        [ObservableProperty]
        protected ObservableCollection<VideoStream> _streams;

        [ObservableProperty]
        protected string _url;

        [ObservableProperty]
        protected Source _source;

        #endregion



        #region CONSTRUCTORS

        protected Video()
        {
            _streams = new ObservableCollection<VideoStream>();
        }

        #endregion
    }
}
