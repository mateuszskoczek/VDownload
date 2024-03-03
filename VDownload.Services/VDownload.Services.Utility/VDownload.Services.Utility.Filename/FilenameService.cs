using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VDownload.Models;
using VDownload.Services.Data.Configuration;
using VDownload.Services.Data.Configuration.Models;

namespace VDownload.Services.Utility.Filename
{
    public interface IFilenameService
    {
        string CreateFilename(string template, Video video);
        string SanitizeFilename(string filename);
    }



    public class FilenameService : IFilenameService
    {
        #region SERVICES

        protected readonly IConfigurationService _configurationService;

        #endregion



        #region CONSTRUCTORS

        public FilenameService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        #endregion



        #region PUBLIC METHODS

        public string CreateFilename(string template, Video video)
        {
            string filename = template;
            foreach (FilenameTemplate templateElement in _configurationService.Common.FilenameTemplates)
            {
                switch (templateElement.Name)
                {
                    case "id": ReplaceInPlace(ref filename, templateElement.Wildcard, video.Id); break;
                    case "title": ReplaceInPlace(ref filename, templateElement.Wildcard, video.Title); break;
                    case "author": ReplaceInPlace(ref filename, templateElement.Wildcard, video.Author); break;
                    case "views": ReplaceInPlace(ref filename, templateElement.Wildcard, video.Views.ToString()); break;
                    case "source": ReplaceInPlace(ref filename, templateElement.Wildcard, video.Source.ToString()); break;
                    case "date":
                    {
                        Regex regex = new Regex(templateElement.Wildcard);
                        foreach (Match match in regex.Matches(filename))
                        {
                            ReplaceInPlace(ref filename, match.Value, video.PublishDate.ToString(match.Groups[1].Value));
                        }
                        break;
                    }
                    case "duration":
                    {
                        Regex regex = new Regex(templateElement.Wildcard);
                        foreach (Match match in regex.Matches(filename))
                        {
                            ReplaceInPlace(ref filename, match.Value, video.Duration.ToString(match.Groups[1].Value));
                        }
                        break;
                    }
                    default: throw new Exception("Invalid template");
                }
            }
            filename = SanitizeFilename(filename);
            return filename;
        }

        public string SanitizeFilename(string filename)
        {
            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
            {
                filename = filename.Replace(c, '_');
            }
            return filename;
        }

        #endregion



        #region PRIVATE METHODS

        protected void ReplaceInPlace(ref string filename, string oldValue, string newValue) => filename = filename.Replace(oldValue, newValue);

        #endregion
    }
}
