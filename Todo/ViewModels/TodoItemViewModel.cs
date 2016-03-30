﻿using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Todo.ViewModels
{
    class TodoItemViewModel
    {
        private ObservableCollection<Models.TodoItem> allItems = new ObservableCollection<Models.TodoItem>();
        public ObservableCollection<Models.TodoItem> AllItems { get { return this.allItems; } }

        private Models.TodoItem selectedItem = default(Models.TodoItem);
        public Models.TodoItem SelectedItem { get { return selectedItem; } set { this.selectedItem = value; } }


        private void LoadDatabase()
        {
            var db = App.conn;
            string sql = @"CREATE TABLE IF NOT EXISTS
                           Customer (Id     INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
                                     ImageUri   VARCHAR(140),
                                     Title      VARCHAR(140),
                                     Details    VARCHAR(140),
                                     Date       VARCHAR(140)
                           );";
            using (var statement = db.Prepare(sql))
            {
                statement.Step();
            }
        }
        public TodoItemViewModel()
        {
            LoadDatabase();
            // 加个用来测试的item
            AddTodoItem(
                new BitmapImage(new Uri("ms-appx:///Assets/Test.jpg")),
                new Uri("ms-appx:///Assets/Test.jpg"),
                "测试项目",
                "测试内容",
                DateTimeOffset.Now
                );
        }

        public void AddTodoItem(
            BitmapImage bitmapImage,
            Uri imageUri,
            string title,
            string description,
            DateTimeOffset date
            )
        {
            this.allItems.Add(new Models.TodoItem(bitmapImage, imageUri, title, description, date));

            //保存数据到数据库
            try
            {
                using (var custstmt = App.conn.Prepare("INSERT INTO Customer (ImageUri, Title, Details, Date) VALUES (?, ?, ?)"))
                {
                    custstmt.Bind(1, imageUri.ToString());
                    custstmt.Bind(2, title);
                    custstmt.Bind(3, description);
                    custstmt.Bind(4, date.ToString());
                    custstmt.Step();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public void RemoveTodoItem(string id)
        {
            // DIY
            Models.TodoItem temp = null;
            for (int i = 0; i < allItems.Count; ++i)
            {
                if (allItems[i].GetId() == id) temp = allItems[i];
            }
            allItems.Remove(temp);

            // set selectedItem to null after remove
            this.selectedItem = null;
        }

        public Models.TodoItem GetLastTodoItem()
        {
            return allItems[allItems.Count - 1];
        }

        public Models.TodoItem GetTodoItem(string id)
        {
            Models.TodoItem temp = null;
            for (int i = 0; i < allItems.Count; ++i)
            {
                if (allItems[i].GetId() == id) temp = allItems[i];
            }
            return temp;
        }

        public void UpdateTodoItem(
            string id,
            BitmapImage bitmapImage,
            Uri imageUri,
            string title,
            string description,
            DateTimeOffset date)
        {
            // DIY
            Models.TodoItem temp = null;
            for (int i = 0; i < allItems.Count; ++i)
            {
                if (allItems[i].GetId() == id) temp = allItems[i];
            }
            temp.bitmapImage = bitmapImage;
            temp.ImageUri = imageUri;
            temp.title = title;
            temp.description = description;
            temp.date = date;

            // set selectedItem to null after update
            this.selectedItem = null;
        }


        //把图片保存到应用目录下
        public static async Task<Uri> SaveLocalImage(StorageFile file)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder; //应用数据目录

            using (FileRandomAccessStream stream = (FileRandomAccessStream)await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                var inputStream = stream.GetInputStreamAt(0);
                using (DataReader reader = new DataReader(inputStream))
                {
                    uint size = await reader.LoadAsync((uint)stream.Size);
                    var buffer = reader.ReadBuffer(size);

                    StorageFile sampleFile = await folder.CreateFileAsync(file.Name, CreationCollisionOption.ReplaceExisting);
                    using (FileRandomAccessStream sampleStream = (FileRandomAccessStream)await sampleFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        await sampleStream.WriteAsync(buffer);
                        return new Uri(sampleFile.Path);
                    }
                }
            }

        }
    }
}
