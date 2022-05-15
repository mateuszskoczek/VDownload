using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VDownload.Core.Enums;
using VDownload.Core.Services;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Editing;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace VDownload.Views.Settings
{
    public sealed partial class MainPage : Page
    {
        #region CONSTRUCTORS

        public MainPage()
        {
            this.InitializeComponent();
        }

        #endregion



        #region EVENT HANDLERS

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            MaxNumberOfActiveTasksSettingControlNumberBox.Value = (int)Config.GetValue("max_active_video_task");
            ReplaceOutputFileIfExistsSettingControlToggleSwitch.IsOn = (bool)Config.GetValue("replace_output_file_if_exists");
            RemoveTaskWhenSuccessfullyEndedSettingControlToggleSwitch.IsOn = (bool)Config.GetValue("remove_task_when_successfully_ended");
            ShowWarningWhenTaskStartsOnMeteredConnectionSettingControlToggleSwitch.IsOn = (bool)Config.GetValue("show_warning_when_task_starts_on_metered_network");
            DelayWhenQueuedTaskStartsOnMeteredConnectionSettingControlToggleSwitch.IsOn = (bool)Config.GetValue("delay_task_when_queued_task_starts_on_metered_network");
            FilenameTemplateSettingControlTextBox.Text = (string)Config.GetValue("default_filename");

            foreach (string mediaType in Enum.GetNames(typeof(MediaType)))
            {
                DefaultMediaTypeSettingControlComboBox.Items.Add(ResourceLoader.GetForCurrentView().GetString($"Base_MediaType_{mediaType}Text"));
            }
            DefaultMediaTypeSettingControlComboBox.SelectedIndex = (int)Config.GetValue("default_media_type");

            foreach (string videoExtension in Enum.GetNames(typeof(VideoFileExtension)))
            {
                VideoExtensionSettingControlComboBox.Items.Add(videoExtension);
            }
            VideoExtensionSettingControlComboBox.SelectedIndex = (int)Config.GetValue("default_video_extension");

            foreach (string audioExtension in Enum.GetNames(typeof(AudioFileExtension)))
            {
                AudioExtensionSettingControlComboBox.Items.Add(audioExtension);
            }
            AudioExtensionSettingControlComboBox.SelectedIndex = (int)Config.GetValue("default_audio_extension") - 3;

            RememberLastMediaLocationSettingControlToggleSwitch.IsOn = !(bool)Config.GetValue("custom_media_location");

            if (StorageApplicationPermissions.FutureAccessList.ContainsItem("custom_media_location"))
            {
                CustomMediaLocationSettingControl.Description = (await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("custom_media_location")).Path;
            }
            else
            {
                CustomMediaLocationSettingControl.Description = $@"{UserDataPaths.GetDefault().Downloads}\VDownload";
            }

            ShowNotificationWhenTaskEndedSuccessfullySettingControlToggleSwitch.IsOn = (bool)Config.GetValue("show_notification_when_task_ended_successfully");

            ShowNotificationWhenTaskEndedUnsuccessfullySettingControlToggleSwitch.IsOn = (bool)Config.GetValue("show_notification_when_task_ended_unsuccessfully");

            UseHardwareAccelerationSettingControlToggleSwitch.IsOn = (bool)Config.GetValue("media_transcoding_use_hardware_acceleration");

            foreach (string transcodingAlgorithm in Enum.GetNames(typeof(MediaVideoProcessingAlgorithm)))
            {
                TranscodingAlgorithmSettingControlComboBox.Items.Add(transcodingAlgorithm);
            }
            TranscodingAlgorithmSettingControlComboBox.SelectedIndex = (int)Config.GetValue("media_transcoding_algorithm");

            foreach (string editingAlgorithm in Enum.GetNames(typeof(MediaTrimmingPreference)))
            {
                EditingAlgorithmSettingControlComboBox.Items.Add(editingAlgorithm);
            }
            EditingAlgorithmSettingControlComboBox.SelectedIndex = (int)Config.GetValue("media_editing_algorithm");

            DeleteTemporaryFilesOnStartSettingControlToggleSwitch.IsOn = (bool)Config.GetValue("delete_temp_on_start");

            if (StorageApplicationPermissions.FutureAccessList.ContainsItem("custom_temp_location"))
            {
                TemporaryFilesLocationSettingControl.Description = (await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("custom_temp_location")).Path;
            }
            else
            {
                TemporaryFilesLocationSettingControl.Description = ApplicationData.Current.TemporaryFolder.Path;
            }

            DeleteTasksTemporaryFilesIfEndedWithErrorSettingControlToggleSwitch.IsOn = (bool)Config.GetValue("delete_task_temp_when_ended_with_error");

            PassiveVodTrimmingSettingControlToggleSwitch.IsOn = (bool)Config.GetValue("twitch_vod_passive_trim");

            VodChunkDownloadingErrorRetryAfterErrorSettingControlToggleSwitch.IsOn = (bool)Config.GetValue("twitch_vod_downloading_chunk_retry_after_error");

            VodChunkDownloadingErrorMaxNumberOfRetriesSettingControlNumberBox.Value = (int)Config.GetValue("twitch_vod_downloading_chunk_max_retries");

            VodChunkDownloadingErrorRetriesDelaySettingControlNumberBox.Value = (int)Config.GetValue("twitch_vod_downloading_chunk_retries_delay");
        }

        private void MaxNumberOfActiveTasksSettingControlNumberBox_LostFocus(object sender, RoutedEventArgs e)
        {
            double value = ((NumberBox)sender).Value;
            if (double.IsNaN(value))
            {
                ((NumberBox)sender).Value = (int)Config.GetValue("max_active_video_task");
            }
            else
            {
                Config.SetValue("max_active_video_task", (int)value);
            }
        }

        private void ReplaceOutputFileIfExistsSettingControlToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            Config.SetValue("replace_output_file_if_exists", ((ToggleSwitch)sender).IsOn);
        }

        private void RemoveTaskWhenSuccessfullyEndedSettingControlToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            Config.SetValue("remove_task_when_successfully_ended", ((ToggleSwitch)sender).IsOn);
        }

        private void ShowWarningWhenTaskStartsOnMeteredConnectionSettingControlToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            Config.SetValue("show_warning_when_task_starts_on_metered_network", ((ToggleSwitch)sender).IsOn);
        }

        private void DelayWhenQueuedTaskStartsOnMeteredConnectionSettingControlToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            Config.SetValue("delay_task_when_queued_task_starts_on_metered_network", ((ToggleSwitch)sender).IsOn);
        }

        private void FilenameTemplateSettingControlTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Config.SetValue("default_filename", ((TextBox)sender).Text);
        }

        private void FilenameTemplateSettingControlHelpButton_Click(object sender, RoutedEventArgs e)
        {
            FilenameTemplateSettingControlInfoBox.IsOpen = !FilenameTemplateSettingControlInfoBox.IsOpen;
        }

        private void DefaultMediaTypeSettingControlComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Config.SetValue("default_media_type", ((ComboBox)sender).SelectedIndex);
        }

        private void VideoExtensionSettingControlComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Config.SetValue("default_video_extension", ((ComboBox)sender).SelectedIndex);
        }

        private void AudioExtensionSettingControlComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Config.SetValue("default_audio_extension", ((ComboBox)sender).SelectedIndex + 3);
        }

        private void RememberLastMediaLocationSettingControlToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            Config.SetValue("custom_media_location", !((ToggleSwitch)sender).IsOn);
        }

        private async void CustomMediaLocationSettingControlBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker picker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.Downloads
            };
            picker.FileTypeFilter.Add("*");

            StorageFolder selectedFolder = await picker.PickSingleFolderAsync();

            if (selectedFolder != null)
            {
                try
                {
                    await(await selectedFolder.CreateFileAsync("VDownloadLocationAccessTest")).DeleteAsync();
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace("custom_media_location", selectedFolder);
                    CustomMediaLocationSettingControl.Description = selectedFolder.Path;
                }
                catch (UnauthorizedAccessException) { }
            }
        }

        private void ShowNotificationWhenTaskEndedSuccessfullySettingControlToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            Config.SetValue("show_notification_when_task_ended_successfully", !((ToggleSwitch)sender).IsOn);
        }

        private void ShowNotificationWhenTaskEndedUnsuccessfullySettingControlToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            Config.SetValue("show_notification_when_task_ended_unsuccessfully", !((ToggleSwitch)sender).IsOn);
        }

        private void UseHardwareAccelerationSettingControlToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            Config.SetValue("media_transcoding_use_hardware_acceleration", ((ToggleSwitch)sender).IsOn);
        }

        private void TranscodingAlgorithmSettingControlComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Config.SetValue("media_transcoding_algorithm", ((ComboBox)sender).SelectedIndex);
        }

        private void EditingAlgorithmSettingControlComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Config.SetValue("media_editing_algorithm", ((ComboBox)sender).SelectedIndex);
        }

        private void DeleteTemporaryFilesOnStartSettingControlToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            Config.SetValue("delete_temp_on_start", ((ToggleSwitch)sender).IsOn);
        }

        private async void TemporaryFilesLocationSettingControlBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker picker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.Downloads
            };
            picker.FileTypeFilter.Add("*");

            StorageFolder selectedFolder = await picker.PickSingleFolderAsync();

            if (selectedFolder != null)
            {
                try
                {
                    await(await selectedFolder.CreateFileAsync("VDownloadLocationAccessTest")).DeleteAsync();
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace("custom_temp_location", selectedFolder);
                    TemporaryFilesLocationSettingControl.Description = selectedFolder.Path;
                }
                catch (UnauthorizedAccessException) { }
            }
        }

        private void DeleteTasksTemporaryFilesIfEndedWithErrorSettingControlToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            Config.SetValue("delete_task_temp_when_ended_with_error", ((ToggleSwitch)sender).IsOn);
        }

        private void PassiveVodTrimmingSettingControlToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            Config.SetValue("twitch_vod_passive_trim", ((ToggleSwitch)sender).IsOn);
        }

        private void VodChunkDownloadingErrorRetryAfterErrorSettingControlToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            Config.SetValue("twitch_vod_downloading_chunk_retry_after_error", ((ToggleSwitch)sender).IsOn);
        }

        private void VodChunkDownloadingErrorMaxNumberOfRetriesSettingControlNumberBox_LostFocus(object sender, RoutedEventArgs e)
        {
            double value = ((NumberBox)sender).Value;
            if (double.IsNaN(value))
            {
                ((NumberBox)sender).Value = (int)Config.GetValue("twitch_vod_downloading_chunk_max_retries");
            }
            else
            {
                Config.SetValue("twitch_vod_downloading_chunk_max_retries", (int)value);
            }
        }

        private void VodChunkDownloadingErrorRetriesDelaySettingControlNumberBox_LostFocus(object sender, RoutedEventArgs e)
        {
            double value = ((NumberBox)sender).Value;
            if (double.IsNaN(value))
            {
                ((NumberBox)sender).Value = (int)Config.GetValue("twitch_vod_downloading_chunk_retries_delay");
            }
            else
            {
                Config.SetValue("twitch_vod_downloading_chunk_retries_delay", (int)value);
            }
        }

        private void RestoreDefaultSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Config.SetDefault();
            StorageApplicationPermissions.FutureAccessList.Remove("custom_media_location");
            StorageApplicationPermissions.FutureAccessList.Remove("custom_temp_location");
            OnNavigatedTo(null);
        }

        #endregion
    }
}
