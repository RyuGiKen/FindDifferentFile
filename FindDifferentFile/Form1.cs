using System;
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
using Color = System.Drawing.Color;

namespace FindDifferentFile
{
    public partial class Form1 : Form
    {
        string path1;
        string path2;
        List<FileInfo> files1 = new List<FileInfo>();
        List<FileInfo> files2 = new List<FileInfo>();
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
        public static List<FileInfo> LoadFiles(string path, ListBox listBox = null, TextBox count = null)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;
            List<FileInfo> result = new List<FileInfo>();
            if (File.Exists(path))
                result.Add(new FileInfo(path));
            else if (Directory.Exists(path))
                result = GetFile.GetFileInfos(path);

            Console.WriteLine("找到" + result?.Count);
            UpdateList(listBox, FileInfoToString(result), count);
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
                    if (files1[i].Name.ToLower() == files2[j].Name.ToLower())
                    {
                        files1.RemoveAt(i);
                        files2.RemoveAt(j);
                        break;
                    }
                }
            }
            UpdateList(listBox1, FileInfoToString(files1), textBox3);
            UpdateList(listBox2, FileInfoToString(files2), textBox4);
            Console.WriteLine("找到" + (files1.Count + files2.Count) + "，耗时 " + (DateTime.Now - dateTime).TotalMilliseconds.ToString() + " ms");
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
            List<FileInfo> data1 = new List<FileInfo>();
            List<FileInfo> data2 = new List<FileInfo>();
            for (int i = files1.Count - 1; i >= 0; i--)
            {
                for (int j = files2.Count - 1; j >= 0; j--)
                {
                    if (files1[i].Name.ToLower() == files2[j].Name.ToLower())
                    {
                        data1.Add(files1[i]);
                        files1.RemoveAt(i);
                        data2.Add(files2[j]);
                        files2.RemoveAt(j);
                        break;
                    }
                }
            }
            files1 = data1;
            files2 = data2;
            UpdateList(listBox1, FileInfoToString(files1), textBox3);
            UpdateList(listBox2, FileInfoToString(files2), textBox4);
            Console.WriteLine("找到" + (files1.Count + files2.Count) + "，耗时 " + (DateTime.Now - dateTime).TotalMilliseconds.ToString() + " ms");
        }
        /// <summary>
        /// 清空按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            files1?.Clear();
            files2?.Clear();
            UpdateList(listBox1, FileInfoToString(files1), textBox3);
            UpdateList(listBox2, FileInfoToString(files2), textBox4);
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
                    listBox.ItemHeight = 35; //设置项高
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
                    FileInfo file = null;
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
                    if (file != null)
                    {
                        Image image = GetIconFromFile(file.FullName).ToBitmap();
                        Graphics g = e.Graphics;
                        if (image != null)
                        {
                            g.DrawImage(image, imageRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
                        }
                    }
                    //文本
                    Rectangle textRect = new Rectangle(imageRect.Right, bounds.Y + 5, bounds.Width - imageRect.Right, bounds.Height - 5);
                    e.Graphics.DrawString(listBox.Items[e.Index].ToString(), e.Font, textBrush, textRect, null);
                }
            }
        }
        /// <summary>
        /// 双击打开文件位置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox_DoubleClick(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (sender is ListBox)
                    {
                        ListBox listBox = sender as ListBox;
                        if (listBox.SelectedIndex >= 0)
                        {
                            FileInfo file = null;
                            if (listBox == listBox1)
                            {
                                if (files1 != null && files1.Count > listBox.SelectedIndex)
                                    file = files1[listBox.SelectedIndex];
                            }
                            else if (listBox == listBox2)
                            {
                                if (files2 != null && files2.Count > listBox.SelectedIndex)
                                    file = files2[listBox.SelectedIndex];
                            }
                            if (file != null)
                                Process.Start("Explorer", "/select," + file.DirectoryName + "\\" + file.Name);
                        }
                        /*if (listBox.SelectedIndex >= 0)
                            Console.WriteLine("点击：" + listBox.SelectedIndex + " " + listBox.SelectedItem);
                        else
                            Console.WriteLine("点击：" + listBox.SelectedIndex);*/
                    }
                    break;
            }
        }
        /// <summary>
        /// 右键取消选中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox_MouseRightClick(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Right:
                    if (sender is ListBox)
                    {
                        ListBox listBox = sender as ListBox;
                        //listBox.SelectedIndex = -1;
                        listBox.SelectedItem = null;
                        //Console.WriteLine("点击：" + listBox.SelectedIndex);
                    }
                    break;
            }
        }
        public static string[] FileInfoToString(List<FileInfo> files)
        {
            if (files == null || files.Count < 1)
                return null;
            List<string> result = new List<string>();
            for (int i = 0; i < files.Count; i++)
            {
                result.Add(files[i].Name + "\n\r" + files[i].LastWriteTime.ToString() + "\t" + ValueAdjust.ConvertSize(files[i].Length));
            }
            return result.ToArray();
        }
        /// <summary>
        /// 清空按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            if (files1 != null)
                files1 = ValueAdjust.ClearNullItem(files1.ToArray()).ToList();
            if (files2 != null)
                files2 = ValueAdjust.ClearNullItem(files2.ToArray()).ToList();
            UpdateList(listBox1, FileInfoToString(files1), textBox3);
            UpdateList(listBox2, FileInfoToString(files2), textBox4);
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
    }
}
