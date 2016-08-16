using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace ItemsReader
{
    public partial class Form1 : Form
    {
        string COSTUMES;
        string WEAPON;
        BackgroundWorker thread;
        CParser cparser;
        List<string> items;
        bool mouseIsDown = false;
        Point firstPoint;


        public Form1()
        {
            InitializeComponent();
            items = new List<string>();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
        }

        private void ParseDocument()
        {
            //Check if file exist
            string nexon_directory = "C:\\Nexon\\WarRock EU\\data";
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "(*.bin*)|*.bin";
            fileDialog.FileName = "items";

            if (Directory.Exists(nexon_directory))
                fileDialog.InitialDirectory = nexon_directory;

            if (fileDialog.ShowDialog() != DialogResult.OK)
                return;

            progressBar.Visible = true;
            thread = new BackgroundWorker();
            thread.WorkerReportsProgress = true;
            thread.DoWork+=thread_DoWork;
            thread.ProgressChanged+=thread_ProgressChanged;
            thread.RunWorkerCompleted+=thread_RunWorkerCompleted;
            thread.RunWorkerAsync(fileDialog.FileName);

        }

        private void thread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar.Visible = false;
            statusBar.Text = "Ready";

            AutoCompleteStringCollection autocomplete = new AutoCompleteStringCollection();
            autocomplete.AddRange(items.ToArray());

            textBox1.AutoCompleteCustomSource = autocomplete;
            textBox1.AutoCompleteSource = AutoCompleteSource.CustomSource;
            textBox1.AutoCompleteMode = AutoCompleteMode.SuggestAppend;

            this.richTextBox1.AppendText(cparser.GetDocument);
        }

        private void thread_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void thread_DoWork(object sender, DoWorkEventArgs e)
        {
            statusBar.Text = "Parsing the document, please wait for a while...";
            cparser = new CParser(e.Argument.ToString());
            cparser.Load();
            cparser.Decrypt();
          
            COSTUMES = cparser.Find("\\[COSTUME\\]", "\\[/COSTUME\\]");//Store Costume block
            WEAPON = cparser.Find("\\[WEAPON\\]", "\\[/WEAPON\\]");//Store WEAPON block

            MatchCollection costumesCollection = cparser.FindCollection(COSTUMES, "<!--", "//-->");//Costume Collection
            MatchCollection weaponCollection = cparser.FindCollection(WEAPON, "<!--", "//-->");//Weapon Collection

            foreach (Match match in costumesCollection)
            {
                string doc = match.Groups[1].Value;

                string basic_info = cparser.Find(doc, "<BASIC_INFO>", "</BASIC_INFO>");
                basic_info = basic_info.Replace("\r", "").Replace("\t", "");
                string[] line = basic_info.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                string[] value = line[1].Split(new char[1] { '=' }, StringSplitOptions.RemoveEmptyEntries);//ENGLISH
                items.Add(value[1]);
            }

            thread.ReportProgress(50);
          
            foreach (Match match in weaponCollection)
            {
                string doc = match.Groups[1].Value;

                string basic_info = cparser.Find(doc, "<BASIC_INFO>", "</BASIC_INFO>");
                basic_info = basic_info.Replace("\r", "").Replace("\t", "");
                string[] line = basic_info.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
               
                string[] value = line[1].Split(new char[1] { '=' }, StringSplitOptions.RemoveEmptyEntries);//ENGLISH
                items.Add(value[1]);
            }
            thread.ReportProgress(100);
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode==Keys.Enter)
            {
                int index = richTextBox1.Find(textBox1.Text);
                richTextBox1.SelectionStart = index;
                richTextBox1.ScrollToCaret();
            }
        }

        private void pictureBox2_MouseEnter(object sender, EventArgs e)
        {
            pictureBox2.Image = global::ItemsReader.Properties.Resources.win32_win_close_hover;
        }

        private void pictureBox2_MouseLeave(object sender, EventArgs e)
        {
            pictureBox2.Image = global::ItemsReader.Properties.Resources.win32_win_close;
        }

        private void pictureBox3_MouseEnter(object sender, EventArgs e)
        {
            pictureBox3.Image = global::ItemsReader.Properties.Resources.win32_win_max_hover;
        }

        private void pictureBox3_MouseLeave(object sender, EventArgs e)
        {
            pictureBox3.Image = global::ItemsReader.Properties.Resources.win32_win_max;
        }

        private void pictureBox4_MouseEnter(object sender, EventArgs e)
        {
            pictureBox4.Image = global::ItemsReader.Properties.Resources.win32_win_min_hover;
        }

        private void pictureBox4_MouseLeave(object sender, EventArgs e)
        {
            pictureBox4.Image = global::ItemsReader.Properties.Resources.win32_win_min;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

            if (this.WindowState == FormWindowState.Normal)
                this.WindowState = FormWindowState.Maximized;
            else
                this.WindowState = FormWindowState.Normal;
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }


        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseIsDown)
            {
                // Get the difference between the two points
                int xDiff = firstPoint.X - e.Location.X;
                int yDiff = firstPoint.Y - e.Location.Y;

                // Set the new point
                int x = this.Location.X - xDiff;
                int y = this.Location.Y - yDiff;
                this.Location = new Point(x, y);
            }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            firstPoint = e.Location;
            mouseIsDown = true;
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            mouseIsDown = false;
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            ParseDocument();
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            cparser.Save(richTextBox1.Text); 
            statusBar.Text = "File has been saved successfull";
        }


    }

}
