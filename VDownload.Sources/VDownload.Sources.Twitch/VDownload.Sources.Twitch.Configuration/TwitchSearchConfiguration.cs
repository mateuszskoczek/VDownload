using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Configuration
{
    public class TwitchSearchConfiguration
    {
        #region PROPERTIES

        public IEnumerable<Regex> GeneralRegexes { get; protected set; }
        public IEnumerable<Regex> VodRegexes { get; protected set; }
        public Regex VodStreamPlaylistRegex { get; protected set; }
        public int VodThumbnailWidth { get; protected set; }
        public int VodThumbnailHeight { get; protected set; }

        #endregion



        #region CONSTRUCTORS

        internal TwitchSearchConfiguration(IConfigurationSection configuration)
        {
            GeneralRegexes = configuration.GetSection("general_regexes").Get<IEnumerable<string>>().Select(x => new Regex(x));
            VodRegexes = configuration.GetSection("vod_regexes").Get<IEnumerable<string>>().Select(x => new Regex(x));
            VodStreamPlaylistRegex = new Regex(configuration["vod_stream_playlist_regex"]);
            VodThumbnailWidth = int.Parse(configuration["vod_thumbnail_width"]);
            VodThumbnailHeight = int.Parse(configuration["vod_thumbnail_height"]);
        }

        #endregion
    }
}
