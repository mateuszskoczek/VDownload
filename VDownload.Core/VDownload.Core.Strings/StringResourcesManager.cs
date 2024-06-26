﻿using Microsoft.UI.Xaml;
using Windows.ApplicationModel.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VDownload.Core.Strings
{
    public static class StringResourcesManager
    {
        #region PROPERTIES

        public static StringResource BaseView { get; } = BuildResource("BaseViewResources");
        public static StringResource HomeView { get; } = BuildResource("HomeViewResources");
        public static StringResource HomeVideoView { get; } = BuildResource("HomeVideoViewResources");
        public static StringResource HomeVideoCollectionView { get; } = BuildResource("HomeVideoCollectionViewResources");
        public static StringResource HomeDownloadsView { get; } = BuildResource("HomeDownloadsViewResources");
        public static StringResource AuthenticationView { get; } = BuildResource("AuthenticationViewResources");
        public static StringResource Notifications { get; } = BuildResource("NotificationsResources");
        public static StringResource Search { get; } = BuildResource("SearchResources");
        public static StringResource Common { get; } = BuildResource("CommonResources");
        public static StringResource DialogButtons { get; } = BuildResource("DialogButtonsResources");
        public static StringResource SettingsView { get; } = BuildResource("SettingsViewResources");
        public static StringResource FilenameTemplate { get; } = BuildResource("FilenameTemplateResources");
        public static StringResource AboutView { get; } = BuildResource("AboutViewResources");
        public static StringResource SubscriptionsView { get; } = BuildResource("SubscriptionsViewResources");

        #endregion



        #region PRIVATE METHODS

        private static StringResource BuildResource(string resourceName)
        {
            File.AppendAllText("C:\\Users\\mateusz\\Desktop\\test.txt", $"teststring {resourceName}\n");
            ResourceLoader loader;
            try
            {
                loader = new ResourceLoader($"VDownload.Core.Strings/{resourceName}");
                File.AppendAllText("C:\\Users\\mateusz\\Desktop\\test.txt", $"afterteststring {resourceName}\n");
            }
            catch (Exception e)
            {

                File.AppendAllText("C:\\Users\\mateusz\\Desktop\\test.txt", $"teststringerror {e.Message}\n");
                throw;
            }
            return new StringResource(loader);
        }

        #endregion
    }
}
