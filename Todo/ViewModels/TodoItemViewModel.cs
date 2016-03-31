using SQLitePCL;
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
                           TodoItem (Id     INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
                                     ImageUri   VARCHAR(140),
                                     Title      VARCHAR(140),
                                     Details    VARCHAR(140),
                                     Date       DATETIME
                           );";

            using (var statement = db.Prepare(sql))
            {
                statement.Step();
            }

            using (var statement = App.conn.Prepare("SELECT * FROM TodoItem"))
            {
                while (SQLiteResult.DONE != statement.Step())
                {
                    string id = ((long)statement[0]).ToString();
                    Uri imageUri = new Uri(statement[1].ToString());
                    string title = (string)statement[2];
                    string description = (string)statement[3];

                    

                    //在本地列表中添加
                    this.allItems.Add(new Models.TodoItem(id, imageUri, title, description, DateTimeOffset.Now));
                }
            }
        }

        public TodoItemViewModel()
        {
            LoadDatabase();
        }

        public void AddTodoItem(Uri imageUri, string title, string description, DateTimeOffset date)
        {

            //保存数据到数据库
            try
            {
                using (var lodoitem = App.conn.Prepare("INSERT INTO TodoItem (ImageUri, Title, Details, Date) VALUES (?, ?, ?, ?)"))
                {
                    lodoitem.Bind(1, imageUri.ToString());
                    lodoitem.Bind(2, title);
                    lodoitem.Bind(3, description);
                    lodoitem.Bind(4, date.ToString("s"));
                    lodoitem.Step();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            //获取刚刚添加数据的ID
            string id = App.conn.LastInsertRowId().ToString();

            //在本地列表中添加
            this.allItems.Add(new Models.TodoItem(id, imageUri, title, description, date));
        }

        public void RemoveTodoItem(string id)
        {
            // DIY
            Models.TodoItem temp = null;
            for (int i = 0; i < allItems.Count; ++i)
            {
                if (allItems[i].id == id) temp = allItems[i];
            }
            allItems.Remove(temp);

            // set selectedItem to null after remove
            this.selectedItem = null;

            //在数据库中删除对应id的数据
            using (var statement = App.conn.Prepare("DELETE FROM TodoItem WHERE Id = ?"))
            {
                statement.Bind(1, int.Parse(id));
                statement.Step();
            }
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
                if (allItems[i].id == id) temp = allItems[i];
            }
            return temp;
        }

        public void UpdateTodoItem(
            string id,
            Uri imageUri,
            string title,
            string description,
            DateTimeOffset date)
        {
            // DIY
            Models.TodoItem temp = null;
            for (int i = 0; i < allItems.Count; ++i)
            {
                if (allItems[i].id == id) temp = allItems[i];
            }
            temp.ImageUri = imageUri;
            temp.title = title;
            temp.description = description;
            temp.date = date;

            // set selectedItem to null after update
            this.selectedItem = null;

            //更新数据库：
            using (var item = App.conn.Prepare("UPDATE TodoItem SET ImageUri = ?, Title = ?, Details = ?, Date = ? WHERE Id = ?"))
            {
                item.Bind(1, imageUri.ToString());
                item.Bind(2, title);
                item.Bind(3, description);
                item.Bind(4, date.ToString());
                item.Bind(5, int.Parse(id));
                item.Step();
            }
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

        public StringBuilder GetQueryTodoItem(string key)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < allItems.Count; ++i)
            {
                Models.TodoItem temp = allItems[i];
                if (temp.title.Contains(key)
                    || temp.description.Contains(key)
                    || temp.date.ToString().Contains(key))
                {
                    string item = "Title: " + temp.title + " Description: " + temp.description
                        + " Date: " + temp.date.ToString() + "\n";
                    sb.Append(item);
                }
            }
            return sb;
        }
    }
}
