﻿using Files.Dialogs;
using Files.Enums;
using Files.Helpers;
using Files.ViewModels.Properties;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Files.Views
{
    public sealed partial class PropertiesHashes : PropertiesTab
    {
        public PropertiesHashes()
        {
            InitializeComponent();
            base.ItemMD5HashProgress = ItemMD5HashProgress;
        }

        protected override void Properties_Loaded(object sender, RoutedEventArgs e)
        {
            base.Properties_Loaded(sender, e);
        }

        /// <summary>
        /// Tries to save changed properties to file.
        /// </summary>
        /// <returns>Returns true if properties have been saved successfully.</returns>
        public async Task<bool> SaveChangesAsync()
        {
            while (true)
            {
                using DynamicDialog dialog = DynamicDialogFactory.GetFor_PropertySaveErrorDialog();
                try
                {
                    await (BaseProperties as FileProperties).SyncPropertyChangesAsync();
                    return true;
                }
                catch
                {
                    // Attempting to open more than one ContentDialog
                    // at a time will throw an error)
                    if (UIHelpers.IsAnyContentDialogOpen())
                    {
                        return false;
                    }
                    await dialog.ShowAsync();
                    switch (dialog.DynamicResult)
                    {
                        case DynamicDialogResult.Primary:
                            break;

                        case DynamicDialogResult.Secondary:
                            return true;

                        case DynamicDialogResult.Cancel:
                            return false;
                    }
                }
            }
        }

        private async void CompareHash_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;

            openPicker.FileTypeFilter.Clear();
            openPicker.FileTypeFilter.Add("*");

            StorageFile file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                file = await Filesystem.FilesystemTasks.Wrap(() => StorageFile.GetFileFromPathAsync(file.Path).AsTask());

                if ((BaseProperties as FileProperties) != null)
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();

                    ComboBoxItem ComboItem = (ComboBoxItem)CompareHashSelector.SelectedItem;
                    switch (ComboItem.Name)
                    {
                        case "MD5":
                            await (BaseProperties as FileProperties).GetSystemFileHashes(HashAlgorithmNames.Md5, file);
                            break;
                        case "SHA1":
                            await (BaseProperties as FileProperties).GetSystemFileHashes(HashAlgorithmNames.Sha1, file);
                            break;
                        default:
                            await (BaseProperties as FileProperties).GetSystemFileHashes(string.Empty, file);
                            break;
                    }

                    stopwatch.Stop();
                    Debug.WriteLine(string.Format("System file properties were obtained in {0} milliseconds", stopwatch.ElapsedMilliseconds));
                }
            }
        }

        private void CompareHashSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CompareHashOutput.Text = string.Empty;
        }
    }
}