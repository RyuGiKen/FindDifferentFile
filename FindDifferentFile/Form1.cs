using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RyuGiKen;
using WindowsAPI;
using WindowsThumbnailProvider;
using FileOperation;

namespace FindDifferentFile
{
    public partial class Form1 : Form
    {
        public static int ListItemHeight = 36;
        string path1;
        string path2;
        List<FileData> files1 = new List<FileData>();
        List<FileData> files2 = new List<FileData>();
        public Form1()
        {
            InitializeComponent();
            //Console.WriteLine(this.Bounds);
            //Console.WriteLine(this.panel1.Bounds);
            //Console.WriteLine(this.panel2.Bounds);
            AdjustPanel(null, null);
            this.SizeChanged += new EventHandler(AdjustPanel);
        }
        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<FileData> LoadFiles(string path, ListBox listBox = null, TextBox count = null)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;
            List<FileInfo> temp = new List<FileInfo>();
            List<FileData> result = new List<FileData>();
            if (File.Exists(path))
                temp.Add(new FileInfo(path));
            else if (Directory.Exists(path))
                temp = GetFile.GetFileInfos(path);

            Console.WriteLine("读取文件：" + temp?.Count);

            for (int i = 0; i < temp.Count; i++)
            {
                result.Add(new FileData(temp[i]));
            }
            UpdateList(listBox, result.ToArray(), count);
            return result;
        }
        /// <summary>
        /// 更新列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listBox"></param>
        /// <param name="array"></param>
        static void UpdateList<T>(ListBox listBox, T[] array, TextBox count = null) where T : class
        {
            if (listBox != null)
            {
                listBox.Items.Clear();
                if (array != null && array.Length > 0)
                    listBox.Items.AddRange(array);
                if (count != null)
                    count.Text = listBox.Items.Count.ToString();
            }
        }
        /// <summary>
        /// 导入目录1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
                return;
            path1 = textBox1.Text;
            ClearList1();
            files1 = LoadFiles(path1, listBox1, textBox3);
        }
        /// <summary>
        /// 导入目录2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox2.Text))
                return;
            path2 = textBox2.Text;
            ClearList2();
            files2 = LoadFiles(path2, listBox2, textBox4);
        }
        /// <summary>
        /// 找出差异
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            DateTime dateTime = DateTime.Now;
            if (files1 == null || files1.Count < 1 || files2 == null || files2.Count < 1)
                return;
            for (int i = files1.Count - 1; i >= 0; i--)
            {
                for (int j = files2.Count - 1; j >= 0; j--)
                {
                    bool? result = CompareFile(files1[i], files2[j], !checkBox1.Checked, checkBox2.Checked, checkBox3.Checked);
                    if (result == true)
                    {
                        files1[i]?.Destroy();
                        files2[j]?.Destroy();
                        files1.RemoveAt(i);
                        files2.RemoveAt(j);
                        break;
                    }
                }
            }
            UpdateList(listBox1, files1.ToArray(), textBox3);
            UpdateList(listBox2, files2.ToArray(), textBox4);
            Console.WriteLine("找到差异：" + (files1.Count + files2.Count) + "，耗时 " + (DateTime.Now - dateTime).TotalMilliseconds.ToString() + " ms");
        }
        /// <summary>
        /// 找出相同
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            DateTime dateTime = DateTime.Now;
            if (files1 == null || files1.Count < 1 || files2 == null || files2.Count < 1)
                return;
            List<FileData> data1 = new List<FileData>();
            List<FileData> data2 = new List<FileData>();
            for (int i = files1.Count - 1; i >= 0; i--)
            {
                for (int j = files2.Count - 1; j >= 0; j--)
                {
                    bool? result = CompareFile(files1[i], files2[j], !checkBox1.Checked, checkBox2.Checked, checkBox3.Checked);
                    if (result == true)
                    {
                        data1.Add(files1[i]);
                        files1.RemoveAt(i);
                        data2.Add(files2[j]);
                        files2.RemoveAt(j);
                        break;
                    }
                }
            }
            ClearList1();
            ClearList2();
            files1 = data1;
            files2 = data2;
            UpdateList(listBox1, files1.ToArray(), textBox3);
            UpdateList(listBox2, files2.ToArray(), textBox4);
            Console.WriteLine("找到相同：" + (files1.Count + files2.Count) + "，耗时 " + (DateTime.Now - dateTime).TotalMilliseconds.ToString() + " ms");
        }
        /// <summary>
        /// 比较文件
        /// </summary>
        /// <param name="file1"></param>
        /// <param name="file2"></param>
        /// <param name="CompareSimplifiedTraditional"></param>
        /// <param name="CompareLastWriteTime"></param>
        /// <param name="CompareSize"></param>
        /// <returns></returns>
        public static bool? CompareFile(FileData file1, FileData file2, bool CompareSimplifiedTraditional, bool CompareLastWriteTime, bool CompareSize)
        {
            if (file1?.Exists == true && file2?.Exists == true)
                return CompareFile(file1.Info, file2.Info, CompareSimplifiedTraditional, CompareLastWriteTime, CompareSize);
            else
                return null;
        }
        /// <summary>
        /// 比较文件
        /// </summary>
        /// <param name="file1"></param>
        /// <param name="file2"></param>
        /// <param name="CompareSimplifiedTraditional"></param>
        /// <param name="CompareLastWriteTime"></param>
        /// <param name="CompareSize"></param>
        /// <returns></returns>
        static bool CompareFile(FileInfo file1, FileInfo file2, bool CompareSimplifiedTraditional, bool CompareLastWriteTime, bool CompareSize)
        {
            bool[] state = new bool[3];
            state[0] = CompareSimplifiedTraditional ? ChineseConverter.ToSimplified(file1.Name) == ChineseConverter.ToSimplified(file2.Name) : file1.Name.ToLower() == file2.Name.ToLower();
            state[1] = CompareLastWriteTime ? file1.LastWriteTime == file2.LastWriteTime : true;
            state[2] = CompareSize ? file1.Length == file2.Length : true;
            return state[0] && state[1] && state[2];
        }
        /// <summary>
        /// 清空按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            ClearList1();
            ClearList2();
            UpdateList(listBox1, files1.ToArray(), textBox3);
            UpdateList(listBox2, files2.ToArray(), textBox4);
        }
        void ClearList1()
        {
            foreach (FileData file in files1)
                file.Destroy();
            files1?.Clear();
            files1 = new List<FileData>();
            GC.Collect();
        }
        void ClearList2()
        {
            foreach (FileData file in files2)
                file.Destroy();
            files2?.Clear();
            files2 = new List<FileData>();
            GC.Collect();
        }
        /// <summary>
        /// 重绘
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (sender is ListBox)
            {
                ListBox listBox = sender as ListBox;
                if (listBox.Items.Count > e.Index && e.Index >= 0)
                {
                    //e.DrawBackground();
                    Brush myBrush = Brushes.White;
                    Brush textBrush = Brushes.Black;
                    listBox.ItemHeight = ListItemHeight; //设置项高
                    if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                    {
                        myBrush = new SolidBrush(SystemColors.Highlight);
                        textBrush = new SolidBrush(SystemColors.Window);
                    }
                    else if (e.Index % 2 == 0)
                    {
                        myBrush = new SolidBrush(SystemColors.Window);
                    }
                    else
                    {
                        myBrush = new SolidBrush(SystemColors.Control);
                    }
                    e.Graphics.FillRectangle(myBrush, e.Bounds);
                    //e.Graphics.DrawString(listBox.Items[e.Index].ToString(), e.Font, textBrush, e.Bounds, null);
                    e.DrawFocusRectangle();//焦点框 
                    //绘制图标 
                    FileData file = null;
                    if (listBox == listBox1)
                    {
                        if (files1 != null && files1.Count > e.Index)
                            file = files1[e.Index];
                    }
                    else if (listBox == listBox2)
                    {
                        if (files2 != null && files2.Count > e.Index)
                            file = files2[e.Index];
                    }
                    Rectangle bounds = e.Bounds;
                    Rectangle imageRect = new Rectangle(bounds.X, bounds.Y, bounds.Height, bounds.Height);
                    if (file?.Exists == true)
                    {
                        if (file.Icon == null)
                        {
                            file.Refresh();
                        }
                        if (file.Icon != null)
                        {
                            e.Graphics.DrawImage(file.Icon, imageRect, 0, 0, file.Icon.Width, file.Icon.Height, GraphicsUnit.Pixel);
                        }
                    }
                    //文本
                    StringFormat sf = StringFormat.GenericTypographic;
                    sf.Trimming = StringTrimming.EllipsisCharacter;
                    if (file == null)
                    {
                        Rectangle textRect = new Rectangle(imageRect.Right, bounds.Y + 5, bounds.Width - imageRect.Right, bounds.Height - 5);
                        e.Graphics.DrawString(listBox.Items[e.Index].ToString(), e.Font, textBrush, textRect, sf);
                    }
                    else
                    {
                        if (file?.Exists != true)
                            textBrush = new SolidBrush(SystemColors.ActiveBorder);
                        int textHeight = (e.Font.Size * 1.5f).ToInteger();
                        Rectangle textRect = new Rectangle(imageRect.Right, bounds.Y + 5, bounds.Width - imageRect.Right, textHeight);
                        e.Graphics.DrawString(" " + file.Info.Name, e.Font, textBrush, textRect, sf);
                        textRect = new Rectangle(imageRect.Right + 5, bounds.Y + 5 + textHeight, bounds.Width - imageRect.Right - 5, textHeight);
                        string temp = "  ";
                        temp += file?.Info?.LastWriteTime.ToString();
                        if (file?.Exists == true)
                        {
                            for (int i = 0; i < 25 - temp.Length; i++)
                                temp += " ";
                            temp += ValueAdjust.ConvertSize(file.Info.Length);
                        }
                        e.Graphics.DrawString(temp, e.Font, textBrush, textRect, sf);
                    }
                }
            }
        }
        private void listBox_MouseDown(object sender, MouseEventArgs e)
        {
            ListBox listBox = GetListBox(sender, null);
            switch (e.Button)
            {
                case MouseButtons.Left:
                case MouseButtons.Right:
                    int index = listBox.IndexFromPoint(e.Location);
                    if (index >= 0)
                        listBox.SetSelected(index, true);
                    else
                        listBox.ClearSelected();
                    break;
            }
            switch (e.Button)
            {
                case MouseButtons.Right:
                    FileData file = GetFileFormListBox(sender, null);
                    int index = listBox.IndexFromPoint(e.Location);
                    toolStripMenuItem1.Enabled = index >= 0 && file?.Exists == true;
                    toolStripMenuItem2.Enabled = index >= 0 && file?.Exists == true;
                    toolStripMenuItem3.Enabled = index >= 0 && file?.Exists == true;
                    contextMenuStrip1.Show(listBox, e.Location);
                    break;
            }
        }
        /// <summary>
        /// 清空按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            List<FileData> data1 = new List<FileData>();
            List<FileData> data2 = new List<FileData>();
            if (files1 != null)
            {
                for (int i = files1.Count - 1; i >= 0; i--)
                {
                    if (files1[i].Exists)
                        data1.Add(files1[i]);
                }
            }
            if (files2 != null)
            {
                for (int i = files2.Count - 1; i >= 0; i--)
                {
                    if (files2[i].Exists)
                        data1.Add(files2[i]);
                }
            }
            ClearList1();
            ClearList2();
            files1 = data1;
            files2 = data2;
            UpdateList(listBox1, files1.ToArray(), textBox3);
            UpdateList(listBox2, files2.ToArray(), textBox4);
        }
        /// <summary>
        /// 获取文件图标
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Icon GetIconFromFile(string path)
        {
            if (File.Exists(path))
            {
                return Icon.ExtractAssociatedIcon(path);
            }
            return null;
        }
        void AdjustPanel(object sender, EventArgs e)
        {
            Size PanelSize = new Size((this.Size.Width - 24) / 2 - 6, this.Size.Height - 44 - 16);
            Rectangle rect1 = new Rectangle(12, 44, PanelSize.Width, PanelSize.Height);
            Rectangle rect2 = new Rectangle(12 + PanelSize.Width, 44, PanelSize.Width, PanelSize.Height);
            this.panel1.Bounds = rect1;
            this.panel2.Bounds = rect2;
        }
        ListBox GetListBox(object sender, EventArgs e)
        {
            ListBox result = null;
            if (sender is ListBox)
            {
                ListBox listBox = sender as ListBox;
                result = listBox;
            }
            else if (sender is ToolStripMenuItem)
            {
                ToolStripMenuItem menuItem = (sender as ToolStripMenuItem);
                ToolStrip parent = menuItem.GetCurrentParent();
                if (parent == contextMenuStrip1 && contextMenuStrip1.SourceControl == listBox1)
                {
                    result = listBox1;
                }
                else if (parent == contextMenuStrip1 && contextMenuStrip1.SourceControl == listBox2)
                {
                    result = listBox2;
                }
            }
            return result;
        }
        FileData GetFileFormListBox(object sender, EventArgs e)
        {
            FileData file = null;
            if (sender is ListBox)
            {
                ListBox listBox = sender as ListBox;
                if (listBox == listBox1)
                {
                    if (files1 != null && files1.Count > listBox1.SelectedIndex && listBox1.SelectedIndex >= 0)
                        file = files1[listBox1.SelectedIndex];
                }
                else if (listBox == listBox2)
                {
                    if (files2 != null && files2.Count > listBox2.SelectedIndex && listBox2.SelectedIndex >= 0)
                        file = files2[listBox2.SelectedIndex];
                }
            }
            else if (sender is ToolStripMenuItem)
            {
                ToolStripMenuItem menuItem = (sender as ToolStripMenuItem);
                ToolStrip parent = menuItem.GetCurrentParent();
                if (parent == contextMenuStrip1 && contextMenuStrip1.SourceControl == listBox1)
                {
                    if (files1 != null && files1.Count > listBox1.SelectedIndex && listBox1.SelectedIndex >= 0)
                        file = files1[listBox1.SelectedIndex];
                }
                else if (parent == contextMenuStrip1 && contextMenuStrip1.SourceControl == listBox2)
                {
                    if (files2 != null && files2.Count > listBox2.SelectedIndex && listBox2.SelectedIndex >= 0)
                        file = files2[listBox2.SelectedIndex];
                }
            }
            return file;
        }
        private void OpenFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileData file = GetFileFormListBox(sender, e);
            if (file?.Exists == true)
                Process.Start(file.Info.FullName);
        }
        private void OpenFilePositionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileData file = GetFileFormListBox(sender, e);
            if (file?.Exists == true)
                Process.Start("Explorer", "/select," + file.Info.DirectoryName + "\\" + file.Info.Name);
        }
        private void DeleteFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileData file = GetFileFormListBox(sender, e);
            if (file?.Exists == true)
                FileOperationAPIWrapper.Send(file.Info.FullName);
        }
    }
    public class FileData
    {
        public FileInfo Info;
        public Bitmap Icon;
        public bool Exists
        {
            get
            {
                Info?.Refresh();
                return Info?.Exists == true;
            }
        }
        private FileData() { }
        public FileData(FileInfo file)
        {
            Info = file;
            //Refresh();
        }
        public void Refresh()
        {
            Info?.Refresh();
            if (Info?.Exists == true)
            {
                Icon = WindowsThumbnailProvider.WindowsThumbnailProvider.GetThumbnail(Info.FullName, Form1.ListItemHeight, Form1.ListItemHeight, ThumbnailOptions.None);
                //Icon = GetIconFromFile(file.Info.FullName).ToBitmap();
            }
        }
        public void Destroy()
        {
            Info = null;
            Icon?.Dispose();
        }
        public override string ToString()
        {
            Info?.Refresh();
            if (Info != null && Info.Exists)
                return Info.Name + "\n\r " + Info.LastWriteTime.ToString() + "\t" + ValueAdjust.ConvertSize(Info.Length);
            else
                return "Null";
        }
    }
}