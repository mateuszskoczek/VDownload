using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VDownload.Services.UI.StringResources;

namespace VDownload.Services.UI.Dialogs
{
    public interface IDialogsService
    {
        #region PROPERTIES

        XamlRoot DefaultRoot { get; set; }

        #endregion



        #region METHODS

        Task ShowClose(string title, string message);
        Task<DialogResult> ShowDouble(string title, string message, string primaryButtonText, string secondaryButtonText);
        Task ShowOk(string title, string message);
        Task<DialogResultOkCancel> ShowOkCancel(string title, string message);
        Task ShowSingle(string title, string message, string buttonText);
        Task<DialogResult> ShowTriple(string title, string message, string primaryButtonText, string secondaryButtonText, string cancelButtonText);
        Task<DialogResultYesNo> ShowYesNo(string title, string message);
        Task<DialogResultYesNoCancel> ShowYesNoCancel(string title, string message);

        #endregion
    }



    public class DialogsService : IDialogsService
    {
        #region SERVICES

        protected readonly IStringResourcesService _stringResourcesService;

        #endregion



        #region FIELDS

        protected string _okString;
        protected string _closeString;
        protected string _cancelString;
        protected string _yesString;
        protected string _noString;

        #endregion



        #region PROPERTIES

        public XamlRoot DefaultRoot { get; set; }

        #endregion



        #region CONSTRUCTORS

        public DialogsService(IStringResourcesService stringResourcesService) 
        { 
            _stringResourcesService = stringResourcesService;
            _okString = _stringResourcesService.DialogButtonsResources.Get("OK");
            _closeString = _stringResourcesService.DialogButtonsResources.Get("Close");
            _cancelString = _stringResourcesService.DialogButtonsResources.Get("Cancel");
            _yesString = _stringResourcesService.DialogButtonsResources.Get("Yes");
            _noString = _stringResourcesService.DialogButtonsResources.Get("No");
        }

        #endregion



        #region PUBLIC METHODS

        public async Task ShowOk(string title, string message) => await ShowSingle(title, message, _okString);
        public async Task ShowClose(string title, string message) => await ShowSingle(title, message, _closeString);
        public async Task ShowSingle(string title, string message, string buttonText)
        {
            ContentDialog contentDialog = BuildDialog(title, message);
            contentDialog.CloseButtonText = buttonText;
            await ShowDialog(contentDialog);
        }

        public async Task<DialogResultOkCancel> ShowOkCancel(string title, string message) => await ShowDouble(title, message, _okString, _cancelString) switch
        {
            DialogResult.Primary => DialogResultOkCancel.Ok,
            _ => DialogResultOkCancel.Cancel
        };
        public async Task<DialogResultYesNo> ShowYesNo(string title, string message) => await ShowDouble(title, message, _yesString, _noString) switch
        {
            DialogResult.Primary => DialogResultYesNo.Yes,
            _ => DialogResultYesNo.No
        };
        public async Task<DialogResult> ShowDouble(string title, string message, string primaryButtonText, string secondaryButtonText)
        {
            ContentDialog contentDialog = BuildDialog(title, message);
            contentDialog.PrimaryButtonText = primaryButtonText;
            contentDialog.SecondaryButtonText = secondaryButtonText;
            return await ShowDialog(contentDialog);
        }

        public async Task<DialogResultYesNoCancel> ShowYesNoCancel(string title, string message) => await ShowTriple(title, message, _yesString, _noString, _cancelString) switch
        {
            DialogResult.Primary => DialogResultYesNoCancel.Yes,
            DialogResult.Secondary => DialogResultYesNoCancel.Yes,
            _ => DialogResultYesNoCancel.Cancelled
        };
        public async Task<DialogResult> ShowTriple(string title, string message, string primaryButtonText, string secondaryButtonText, string cancelButtonText)
        {
            ContentDialog contentDialog = BuildDialog(title, message);
            contentDialog.PrimaryButtonText = primaryButtonText;
            contentDialog.SecondaryButtonText = secondaryButtonText;
            contentDialog.CloseButtonText = cancelButtonText;
            return await ShowDialog(contentDialog);
        }

        #endregion



        #region PRIVATE METHODS

        private ContentDialog BuildDialog(string title, string message)
        {
            return new ContentDialog()
            {
                Title = title,
                Content = message,
                XamlRoot = DefaultRoot
            };
        }

        private async Task<DialogResult> ShowDialog(ContentDialog dialog)
        {
            ContentDialogResult result = await dialog.ShowAsync();
            return result switch
            {
                ContentDialogResult.Primary => DialogResult.Primary,
                ContentDialogResult.Secondary => DialogResult.Secondary,
                _ => DialogResult.Cancelled
            }; ;
        }

        #endregion
    }
}
