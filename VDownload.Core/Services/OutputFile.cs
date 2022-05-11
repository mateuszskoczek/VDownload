using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Core.Enums;
using Windows.Storage;

namespace VDownload.Core.Services
{
    public class OutputFile
    {
        #region CONSTRUCTORS

        public OutputFile(string name, MediaFileExtension extension, StorageFolder location)
        {
            Name = name;
            Extension = extension;
            Location = location;
        }

        public OutputFile(string name, MediaFileExtension extension)
        {
            Name = name;
            Extension = extension;
            Location = null;
        }

        #endregion



        #region PROPERTIES

        public string Name { get; private set; }
        public MediaFileExtension Extension { get; private set; }
        public StorageFolder Location { get; private set; }

        #endregion



        #region PUBLIC METHODS

        public async Task<StorageFile> Create()
        {
            string filename = $"{Name}.{Extension.ToString().ToLower()}";
            CreationCollisionOption collisionOption = (bool)Config.GetValue("replace_output_file_if_exists") ? CreationCollisionOption.ReplaceExisting : CreationCollisionOption.GenerateUniqueName;
            return await(!(Location is null) ? Location.CreateFileAsync(filename, collisionOption) : DownloadsFolder.CreateFileAsync(filename, collisionOption));
        }

        public string GetPath() => $@"{(Location != null ? Location.Path : $@"{UserDataPaths.GetDefault().Downloads}\VDownload")}\{Name}.{Extension.ToString().ToLower()}";

        #endregion
    }
}
