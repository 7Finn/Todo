using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace Todo
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ItemPage : Page
    {
        public ItemPage()
        {
            this.InitializeComponent();
            DatePicker.MinYear = DateTimeOffset.Now;

            var viewTitleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            viewTitleBar.BackgroundColor = Windows.UI.Colors.CornflowerBlue;
            viewTitleBar.ButtonBackgroundColor = Windows.UI.Colors.CornflowerBlue;
        }

        private ViewModels.TodoItemViewModel ViewModel;
        Uri tempImageUri;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            this.ViewModel = (ViewModels.TodoItemViewModel)e.Parameter;

            if (ViewModel.SelectedItem != null)
            {
                HeaderTitle.Text = "Edit Todo";
                image.Source = ViewModel.SelectedItem.bitmapImage;
                TextTitle.Text = ViewModel.SelectedItem.title;
                TextDetails.Text = ViewModel.SelectedItem.description;
                DatePicker.Date = ViewModel.SelectedItem.date;
                // ...
            }
            else
            {
                HeaderTitle.Text = "Create Todo";
            }

            Frame rootFrame = this.Frame;
            if (rootFrame.CanGoBack)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            }
            else
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            }

        }


        private void CreateButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (TextTitle.Text == "")
            {
                var i = new MessageDialog("Title can't be empty.").ShowAsync();
                return;
            }
            else if (TextDetails.Text == "")
            {
                var i = new MessageDialog("Details can't be empty.").ShowAsync();
                return;
            }
            else if (DatePicker.Date.AddDays(1) < DateTime.Now)
            {
                var i = new MessageDialog("You can't select the date before.").ShowAsync();
                return;
            }
            Frame rootFrame = Window.Current.Content as Frame;
            BitmapImage bitmapImage = (BitmapImage)image.Source;

            if (ViewModel.SelectedItem != null)
            {
                ViewModel.UpdateTodoItem(
                    ViewModel.SelectedItem.GetId(),
                    bitmapImage,
                    tempImageUri,
                    TextTitle.Text,
                    TextDetails.Text,
                    DatePicker.Date
                    );
                rootFrame.Navigate(typeof(MainPage), ViewModel);
            }
            else
            {
                ViewModel.AddTodoItem(bitmapImage, tempImageUri, TextTitle.Text, TextDetails.Text, DatePicker.Date);
                rootFrame.Navigate(typeof(MainPage), ViewModel);
            }
        }

        private void CancelButton_Clicked(object sender, RoutedEventArgs e)
        {
            TextTitle.Text = "";
            TextDetails.Text = "";
            DatePicker.Date = DateTime.Now; //还原现在时间

            ///把图片还原为空白图片
            image.Source = new BitmapImage(new Uri("ms-appx:///Assets/Picture_246px.png"));
        }

        private void DeleteButton_Clicked(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (ViewModel.SelectedItem != null)
            {
                ViewModel.RemoveTodoItem(ViewModel.SelectedItem.GetId());
                rootFrame.Navigate(typeof(MainPage), ViewModel);
            }
            else
            {
                rootFrame.Navigate(typeof(MainPage), ViewModel);
            }
        }

        private async void PickPicture(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            // var myPictures = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Pictures);


            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                using (Windows.Storage.Streams.IRandomAccessStream fileStream =
                    await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    // Set the image source to the selected bitmap.
                    Windows.UI.Xaml.Media.Imaging.BitmapImage bitmapImage =
                        new Windows.UI.Xaml.Media.Imaging.BitmapImage();

                    bitmapImage.SetSource(fileStream);
                    image.Source = bitmapImage;
                }
                //image.Source = new BitmapImage(new Uri(file.Path.ToString()));
                // Application now has read/write access to the picked file
                tempImageUri = new Uri(file.Path);
                Debug.WriteLine(file.Path.ToString());
            }
            else
            {
                // this.textBlock.Text = "Operation cancelled.";
            }
        }
    }
}
