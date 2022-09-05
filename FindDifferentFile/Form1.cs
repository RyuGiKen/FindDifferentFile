using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RyuGiKen;
using static System.Net.WebRequestMethods;

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
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            List<FileInfo> result = GetFile.GetFileInfos(path);
            Console.WriteLine("找到" + result?.Count);
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
                if (array != null || array.Length > 0)
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
            UpdateList(listBox1, files1.ToArray(), textBox3);
            UpdateList(listBox2, files2.ToArray(), textBox4);
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
            UpdateList(listBox1, files1.ToArray(), textBox3);
            UpdateList(listBox2, files2.ToArray(), textBox4);
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
            UpdateList(listBox1, files1.ToArray(), textBox3);
            UpdateList(listBox2, files2.ToArray(), textBox4);
        }
    }
}
