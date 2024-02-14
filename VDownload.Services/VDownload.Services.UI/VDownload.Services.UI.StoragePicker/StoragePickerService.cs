using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace VDownload.Services.UI.StoragePicker
{
    public interface IStoragePickerService
    {
        #region PROPERTIES

        Window DefaultRoot { get; set; }

        #endregion



        #region METHODS

        Task<string?> OpenDirectory();
        Task<string?> OpenDirectory(StoragePickerStartLocation startLocation);
        Task<IEnumerable<string>> OpenMultipleFiles();
        Task<IEnumerable<string>> OpenMultipleFiles(StoragePickerStartLocation startLocation);
        Task<IEnumerable<string>> OpenMultipleFiles(string[] fileTypes);
        Task<IEnumerable<string>> OpenMultipleFiles(string[] fileTypes, StoragePickerStartLocation startLocation);
        Task<string?> OpenSingleFile();
        Task<string?> OpenSingleFile(StoragePickerStartLocation startLocation);
        Task<string?> OpenSingleFile(string[] fileTypes);
        Task<string?> OpenSingleFile(string[] fileTypes, StoragePickerStartLocation startLocation);
        Task<string?> SaveFile(FileSavePickerFileTypeChoice[] fileTypes, string defaultFileType);
        Task<string?> SaveFile(FileSavePickerFileTypeChoice[] fileTypes, string defaultFileType, StoragePickerStartLocation startLocation);

        #endregion
    }



    public class StoragePickerService : IStoragePickerService
    {
        #region PROPERTIES

        public Window DefaultRoot { get; set; }

        #endregion



        #region PUBLIC METHODS

        public async Task<string?> OpenDirectory() => await OpenDirectory(StoragePickerStartLocation.Unspecified);
        public async Task<string?> OpenDirectory(StoragePickerStartLocation startLocation)
        {
            FolderPicker picker = new FolderPicker();
            InitializePicker(picker);

            ConfigureFolderPicker(picker, startLocation);

            StorageFolder directory = await picker.PickSingleFolderAsync();
            return directory?.Path;
        }

        public async Task<string?> OpenSingleFile() => await OpenSingleFile(["*"], StoragePickerStartLocation.Unspecified);
        public async Task<string?> OpenSingleFile(string[] fileTypes) => await OpenSingleFile(fileTypes, StoragePickerStartLocation.Unspecified);
        public async Task<string?> OpenSingleFile(StoragePickerStartLocation startLocation) => await OpenSingleFile(["*"], startLocation);
        public async Task<string?> OpenSingleFile(string[] fileTypes, StoragePickerStartLocation startLocation)
        {
            FileOpenPicker picker = new FileOpenPicker();
            InitializePicker(picker);

            ConfigureFileOpenPicker(picker, fileTypes, startLocation);

            StorageFile storageFile = await picker.PickSingleFileAsync();
            return storageFile?.Path;
        }

        public async Task<IEnumerable<string>> OpenMultipleFiles() => await OpenMultipleFiles(["*"], StoragePickerStartLocation.Unspecified);
        public async Task<IEnumerable<string>> OpenMultipleFiles(string[] fileTypes) => await OpenMultipleFiles(fileTypes, StoragePickerStartLocation.Unspecified);
        public async Task<IEnumerable<string>> OpenMultipleFiles(StoragePickerStartLocation startLocation) => await OpenMultipleFiles(["*"], startLocation);
        public async Task<IEnumerable<string>> OpenMultipleFiles(string[] fileTypes, StoragePickerStartLocation startLocation)
        {
            FileOpenPicker picker = new FileOpenPicker();
            InitializePicker(picker);

            ConfigureFileOpenPicker(picker, fileTypes, startLocation);

            IEnumerable<StorageFile> list = await picker.PickMultipleFilesAsync();
            return list.Select(x => x.Path);
        }

        public async Task<string?> SaveFile(FileSavePickerFileTypeChoice[] fileTypes, string defaultFileType) => await SaveFile(fileTypes, defaultFileType, StoragePickerStartLocation.Unspecified);
        public async Task<string?> SaveFile(FileSavePickerFileTypeChoice[] fileTypes, string defaultFileType, StoragePickerStartLocation startLocation)
        {
            FileSavePicker picker = new FileSavePicker();
            InitializePicker(picker);

            ConfigureFileSavePicker(picker, fileTypes, defaultFileType, startLocation);

            StorageFile file = await picker.PickSaveFileAsync();
            return file?.Path;
        }

        #endregion



        #region PRIVATE METHODS

        protected void InitializePicker(object picker)
        {
            var hwnd = WindowNative.GetWindowHandle(DefaultRoot);
            InitializeWithWindow.Initialize(picker, hwnd);
        }

        protected void ConfigureFolderPicker(FolderPicker picker, StoragePickerStartLocation startLocation)
        {
            if (startLocation != StoragePickerStartLocation.Unspecified)
            {
                picker.SuggestedStartLocation = (PickerLocationId)startLocation;
            }
        }

        protected void ConfigureFileOpenPicker(FileOpenPicker picker, string[] fileTypes, StoragePickerStartLocation startLocation)
        {
            foreach (string fileType in fileTypes)
            {
                picker.FileTypeFilter.Add(fileType);
            }

            if (startLocation != StoragePickerStartLocation.Unspecified)
            {
                picker.SuggestedStartLocation = (PickerLocationId)startLocation;
            }
        }

        protected void ConfigureFileSavePicker(FileSavePicker picker, FileSavePickerFileTypeChoice[] fileTypes, string defaultFileType, StoragePickerStartLocation startLocation)
        {
            if (startLocation != StoragePickerStartLocation.Unspecified)
            {
                picker.SuggestedStartLocation = (PickerLocationId)startLocation;
            }

            foreach (FileSavePickerFileTypeChoice fileType in fileTypes)
            {
                picker.FileTypeChoices.Add(fileType.Description, fileType.Extensions.ToList());
            }

            if (!defaultFileType.StartsWith('.'))
            {
                defaultFileType = $".{defaultFileType}";
            }

            if (!fileTypes.Any(x => x.Extensions.Contains(defaultFileType)))
            {
                picker.FileTypeChoices.Add("Default", [defaultFileType]);
            }

            picker.DefaultFileExtension = defaultFileType;
        }

        #endregion
    }
}
