using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace provodnik2
{
    public partial class Explorer : Form
    {
        private int iFiles = 0;
        FileStream log;
        public Explorer()
        {
            InitializeComponent();

            treeView1.BeforeSelect += treeView1_BeforeSelect;
            treeView1.BeforeExpand += treeView1_BeforeExpand;
            listView1.MouseDoubleClick += listView1_MouseDoubleClick;
            // заполняем дерево дисками
            FillDriveNodes();
        }

        //событие после нажатия 1 кликом
        void listView1_MouseKlick(object o, MouseEventArgs e)
        {
            string name = ((System.Windows.Forms.ListView)o).FocusedItem.Text;
            string format = name.Substring(name.LastIndexOf(".") + 1);
            if ((format == "png") || (format == "jpeg") || (format == "jpg"))
            {
                pictureBox1.Image = new Bitmap(label1.Text + name);
                pictureBox1.Visible = true;
                richTextBox1.Clear();
                richTextBox1.Visible = false;
            }
            else if (format == "txt")
            {
                WritRichTextBox1(label1.Text + name);
                richTextBox1.Visible = true;
                pictureBox1.Visible = true;
                pictureBox1.Image = imageList1.Images["txt.png"];
            }
            else
            {
                richTextBox1.Visible = false;
                richTextBox1.Clear();
                if (imageList1.Images.ContainsKey(format + ".png"))
                    pictureBox1.Image = imageList1.Images[format + ".png"];
                else
                    pictureBox1.Image = imageList1.Images["unknown.png"];
            }

            label2.Text = "Формат: " + format;
        }

        // событие после нажатия 2 кликом
        void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            System.Diagnostics.Process.Start( label1.Text + ((System.Windows.Forms.ListView)sender).FocusedItem.Text );
            log = new FileStream(@"log.txt", FileMode.Append, FileAccess.Write);
            StreamWriter writer = new StreamWriter(log);
            writer.WriteLine(DateTime.Now.ToShortDateString() + "/" + DateTime.Now.ToShortTimeString() + ":" + label1.Text + ((System.Windows.Forms.ListView)sender).FocusedItem.Text);
            writer.Close();
        }

        // событие перед раскрытием узла
        void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            e.Node.Nodes.Clear();
            string[] dirs;
            try
            {
                if (Directory.Exists(e.Node.FullPath))
                {
                    dirs = Directory.GetDirectories(e.Node.FullPath);
                    if (dirs.Length != 0)
                    {
                        for (int i = 0; i < dirs.Length; i++)
                        {
                            TreeNode dirNode = new TreeNode(new DirectoryInfo(dirs[i]).Name);
                            FillTreeNode(dirNode, dirs[i]);
                            e.Node.Nodes.Add(dirNode);
                        }
                    }
                }
            }
            catch (Exception ex) { }
        }
        // событие перед выделением узла
        void treeView1_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            e.Node.Nodes.Clear();
            string[] dirs;
            try
            {
                if (Directory.Exists(e.Node.FullPath))
                {
                    dirs = Directory.GetDirectories(e.Node.FullPath);
                    if (dirs.Length != 0)
                    {
                        for (int i = 0; i < dirs.Length; i++)
                        {
                            TreeNode dirNode = new TreeNode(new DirectoryInfo(dirs[i]).Name);
                            FillTreeNode(dirNode, dirs[i]);
                            e.Node.Nodes.Add(dirNode);
                        }
                    }
                }
            }
            catch (Exception ex) { }
            label1.Text = e.Node.FullPath.ToString();
            log = new FileStream(@"log.txt", FileMode.Append, FileAccess.Write);
            StreamWriter writer = new StreamWriter(log);
            writer.WriteLine(DateTime.Now.ToShortDateString() + "/" + DateTime.Now.ToShortTimeString() + ":" + e.Node.FullPath.ToString());
            writer.Close();
            AddFiles(e.Node.FullPath.ToString());            
        }

        // получаем все диски на компьютере
        private void FillDriveNodes()
        {
            try
            {
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    TreeNode driveNode = new TreeNode { Text = drive.Name };
                    FillTreeNode(driveNode, drive.Name);
                    treeView1.Nodes.Add(driveNode);
                }
            }
            catch (Exception ex) { }
        }
        // получаем дочерние узлы для определенного узла
        private void FillTreeNode(TreeNode driveNode, string path)
        {
            try
            {
                string[] dirs = Directory.GetDirectories(path);
                foreach (string dir in dirs)
                {
                    TreeNode dirNode = new TreeNode();
                    dirNode.Text = dir.Remove(0, dir.LastIndexOf("\\") + 1);
                    driveNode.Nodes.Add(dirNode);
                }
            }
            catch (Exception ex) { }
        }
        //добавление файлов в listView
        private void AddFiles(string strPath)
        {
            listView1.BeginUpdate();

            listView1.Items.Clear();
            iFiles = 0;
            try
            {
                DirectoryInfo di = new DirectoryInfo(strPath + "\\");
                FileInfo[] theFiles = di.GetFiles();
                foreach (FileInfo theFile in theFiles)
                {
                    iFiles++;
                    System.Windows.Forms.ListViewItem lvItem = new System.Windows.Forms.ListViewItem(theFile.Name);
                    lvItem.SubItems.Add(theFile.Length.ToString());
                    lvItem.SubItems.Add(theFile.LastWriteTime.ToShortDateString());
                    lvItem.SubItems.Add(theFile.LastWriteTime.ToShortTimeString());                                    
                    string loc = lvItem.Text;
                    string format = loc.Substring(loc.LastIndexOf(".") + 1) + ".png";
                    if (imageList1.Images.ContainsKey(format))
                        lvItem.ImageKey = format;
                    else
                        lvItem.ImageKey = "unknown.png";
                    listView1.Items.Add(lvItem);
                }
            }
            catch (Exception Exc) {}

            listView1.EndUpdate();
        }
        // чтение из файла
        public void WritRichTextBox1(string filePath)
        {
            try
            {
                richTextBox1.Clear();
                string[] lines = File.ReadAllLines(filePath, Encoding.GetEncoding(1251));
                for (int i = 0; i < lines.Length; i++)
                    richTextBox1.Text += lines[i] + "\n";
            }
            catch (Exception)
            {

            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
