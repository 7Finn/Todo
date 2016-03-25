using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace Todo
{
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();
            var viewTitleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            viewTitleBar.BackgroundColor = Windows.UI.Colors.CornflowerBlue;
            viewTitleBar.ButtonBackgroundColor = Windows.UI.Colors.CornflowerBlue;
            this.ViewModel = new ViewModels.TodoItemViewModel();
        }

        ViewModels.TodoItemViewModel ViewModel { get; set; }

        //临时储存图片选择器的Uri
        Uri tempImageUri;
        StorageFile tempImageFile;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (e.Parameter.GetType() == typeof(ViewModels.TodoItemViewModel))
            {
                this.ViewModel = (ViewModels.TodoItemViewModel)(e.Parameter);
            }

            //注册共享数据
            dataTransferManager.DataRequested += DataTransferManager_DataRequested; 
        }


        private void AddItemButtonClick(object sender, RoutedEventArgs e)
        {
            if (InlineToDoItemViewGrid.Visibility == Visibility.Collapsed)
            {
                Frame.Navigate(typeof(ItemPage), ViewModel);
            }
            else
            {
                Clear();
                this.ViewModel.SelectedItem = null;
            }
        }

        private void TodoItem_ItemClicked(object sender, ItemClickEventArgs e)
        {
            ViewModel.SelectedItem = (Models.TodoItem)(e.ClickedItem);
            if (InlineToDoItemViewGrid.Visibility == Visibility.Visible)
            {
                CreateButton.Content = "Update";
                image.Source = ViewModel.SelectedItem.bitmapImage;
                TextTitle.Text = ViewModel.SelectedItem.title;
                TextDetails.Text = ViewModel.SelectedItem.description;
                DatePicker.Date = ViewModel.SelectedItem.date;
            }
            else
            {
                Frame.Navigate(typeof(ItemPage), ViewModel);
            }
        }

        private async void CreateButton_Clicked(object sender, RoutedEventArgs e)
        {

            //检查是否为空
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
            tempImageUri = await ViewModels.TodoItemViewModel.SaveLocalImage(tempImageFile);

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
                Debug.WriteLine(tempImageUri);
                Clear();
            }
            else
            {
                ViewModel.AddTodoItem(
                    bitmapImage,
                    tempImageUri,
                    TextTitle.Text,
                    TextDetails.Text,
                    DatePicker.Date
                    );
                Clear();
            }

            ViewModel.SelectedItem = null;
        }

        private void DeleteButton_Clicked(object sender, RoutedEventArgs e)
        {
            AppBarButton appBarBtn = sender as AppBarButton;
            ViewModel.RemoveTodoItem(appBarBtn.Tag.ToString());
            ViewModel.SelectedItem = null;
        }

        private void ClearButton_Clicked(object sender, RoutedEventArgs e)
        {
            Clear();
        }


        private async void PickPicture(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage bitmapImage = new BitmapImage();

                    bitmapImage.SetSource(fileStream);
                    image.Source = bitmapImage;
                    tempImageFile = file;
                }
            }
            else
            {
                // this.textBlock.Text = "Operation cancelled.";
            }
        }

        


        private void Clear()
        {
            TextTitle.Text = "";
            TextDetails.Text = "";
            DatePicker.Date = DateTime.Now; //还原现在时间

            image.Source = new BitmapImage(new Uri("ms-appx:///Assets/Picture_246px.png"));

            CreateButton.Content = "Create";
        }

        private async void UpdateTileButton_Clicked(object sender, RoutedEventArgs e)
        {

            //CreateTileUpdaterForApplication
            StorageFile xmlFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Tile.xml"));
            XmlDocument doc = await XmlDocument.LoadFromFileAsync(xmlFile);

            //Get the latest todoitem
            Models.TodoItem lastTodoItem = ViewModel.GetLastTodoItem();

            //Get the text Node
            XmlNodeList textNodeList = doc.GetElementsByTagName("text");
            foreach (var node in textNodeList)
            {
                if (node.InnerText == "Title") node.InnerText = lastTodoItem.title;
                if (node.InnerText == "Details") node.InnerText = lastTodoItem.description;
            }

            //Get the image Node
            XmlNodeList imageNodeList = doc.GetElementsByTagName("image");
            foreach (var node in imageNodeList)
            {
                Debug.WriteLine(lastTodoItem.ImageUri);
                node.Attributes[0].NodeValue = lastTodoItem.ImageUri.ToString();
            }

            //Update the Tile
            TileNotification notifi = new TileNotification(doc);
            var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.Update(notifi);
        }

        private void ShareItemButton_Clicked(object sender, RoutedEventArgs e)
        {
            AppBarButton appBarBtn = sender as AppBarButton;
            shareTodoItem = ViewModel.GetTodoItem(appBarBtn.Tag.ToString());

            DataTransferManager.ShowShareUI();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            DataTransferManager.GetForCurrentView().DataRequested -= DataTransferManager_DataRequested;
        }


        DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();//数据共享对象
        private Models.TodoItem shareTodoItem;

        private async void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequestDeferral GetFiles = args.Request.GetDeferral();
            try
            {
                DataRequest request = args.Request;
                request.Data.SetText(shareTodoItem.description);
                request.Data.Properties.Title = shareTodoItem.title;
                request.Data.Properties.Description = shareTodoItem.description;

                /* todo list 3.1
                1. 异步方式获取文件
                2. 添加到 List<IStorageItem>集合
                */
                StorageFile imageFile = await Package.Current.InstalledLocation.GetFileAsync("ms-appx:///Assets/Test.jpg");

                //新方法；使用文件的方法也是可以的，但是要根据共享目标应用是否可用
                request.Data.Properties.Thumbnail = RandomAccessStreamReference.CreateFromFile(imageFile);
                request.Data.SetBitmap(RandomAccessStreamReference.CreateFromFile(imageFile));
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message, "爆了").ShowAsync();
            }
            finally
            {
                GetFiles.Complete();
            }
        }
    }
}
