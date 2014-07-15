namespace WindowsLGFramwareExtract
{
    using Ionic.Zlib;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    public class Form1 : Form
    {
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button9;
        private IContainer components;
        private static string DZ_Header = "32-96-18-74";
        private static string DZ_Partition_Header = "30-12-95-78";
        private static string DZAction = "DZ";
        public string DZFile = "";
        private int DZHeaderSize = 4;
        private static int DZPartitionHeaderSize = 0x200;
        public ArrayList DZpartitions = new ArrayList();
        private Thread DZThread;
        private FolderBrowserDialog folderBrowserDialog1;
        public static int GPTHeader_post = 0x100000;
        private static int GPTHeaderIdLength = 8;
        private static int GPTHeaderSize = 0x200;
        public ArrayList GPTpartitions = new ArrayList();
        private static int GPTPartitionSize = 0x80;
        public string isAction = KDZAction;
        private static string KDZ_Header = "28-05-00-00-24-38-22-25";
        private static string KDZAction = "KDZ";
        public string KDZFile = "";
        private int KDZHeaderSize = 8;
        private static int KDZPartitionHeaderSize = 0x110;
        private Label label1;
        private Label LabelFileName;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ListView listView2;
        private System.Windows.Forms.CheckBox LV1ckheader;
        private System.Windows.Forms.CheckBox LV2ckheader;
        private static int MBRSize = 0x200;
        private Thread oThread;
        public ArrayList partitions = new ArrayList();
        public int sizeBlock = 0x200;
        private Thread SysMrgThread;
        private backprocess tbackprocess;
        private DZExtractBackprocess tDZExtractBackprocess;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
        private static string TOT_Header = "45-46-49-20-50-41-52-54";
        private static string TOTAction = "TOT";
        private int TOTPartitionsInfo_length = 0x10;
        private int TOTPartitionsInfo_pos = 0x2010;
        private int TOTPartitionsName_length = 0x20;
        private int TOTPartitionsName_pos = 0x6230;
        private SystemBinMergingBackprocess tSystemBinMergingBackprocess;
        private TOTPartitionExtraction tTOTPartitionExtraction;
        private TOTPartitionMergeBackprocess tTOTPartitionMergeBackprocess;
        private ProgressBar pgBar1;
        private GroupBox groupBox1;
        private PictureBox pictureBox1;
        private TextBox textBox4;
        private string workingdir;

        public Form1()
        {
            this.InitializeComponent();
            this.initLVsckheader();
            this.workingdir = Directory.GetCurrentDirectory();
            this.button8.Text = this.button8.Text + " " + this.workingdir;
        }

        private void backgroundWorker1_DoWork(ArrayList al)
        {
            this.tbackprocess = new backprocess(this, al, this.workingdir);
            this.tbackprocess.onProgressChange += new ProgressChangeDelegate(this.copy_progressChanged);
            this.tbackprocess.onCopyError += new CopyErrorDelegate(this.copy_errorRaise);
            this.tbackprocess.onProgressComplete += new Completedelegate(this.copy_progressFinish);
            this.oThread = new Thread(new ThreadStart(this.tbackprocess.do_extraction));
            this.oThread.Start();
        }

        private string binTOhex(byte[] bytes)
        {
            return BitConverter.ToString(bytes);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog {
                InitialDirectory = this.workingdir
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                this.Clearlistview1();
                this.Clearlistview2();
                if (dialog.FileName != "")
                {
                    this.KDZFile = dialog.FileName;
                    this.textBox1.Text = this.KDZFile;
                    this.logme("KDZ file " + this.KDZFile);
                    if (this.readKDZFile())
                    {
                        if (this.isAction == TOTAction)
                        {
                            this.changeActionAlert(KDZAction);
                        }
                        this.listKDZContents();
                        this.isAction = KDZAction;
                        this.restUIAccessAble("");
                    }
                    else if (!this.readTOTFile())
                    {
                        this.logme("Not a KDZ or a TOT File");
                    }
                    else if (this.partitions.Count > 0)
                    {
                        if (this.isAction == KDZAction)
                        {
                            this.changeActionAlert(TOTAction);
                        }
                        this.listTOTPartitionsPart();
                        this.isAction = TOTAction;
                        this.restUIAccessAble("");
                    }
                }
                else
                {
                    this.logme("no file found");
                }
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
        }

        private void button2_Click(object sender, EventArgs e)
        {
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (this.textBox3.Text.Trim() == "")
            {
                OpenFileDialog dialog = new OpenFileDialog {
                    InitialDirectory = this.workingdir
                };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    this.textBox3.Text = dialog.FileName;
                }
            }
            if (File.Exists(this.textBox3.Text))
            {
                this.DZFile = this.textBox3.Text;
                this.logme("DZ file " + this.DZFile);
                this.readDZFile();
                if (this.DZpartitions.Count > 0)
                {
                    this.listDZFileContents();
                    if (this.isAction == TOTAction)
                    {
                        this.changeActionAlert(KDZAction);
                    }
                    this.restUIAccessAble("DZ");
                }
                else
                {
                    this.logme("Not a Valid DZ File!");
                }
            }
            else
            {
                this.logme(" File Not Exists " + this.textBox3.Text.ToString());
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            bool flag = false;
            if ((this.oThread != null) && this.oThread.IsAlive)
            {
                flag = true;
                if (this.isAction == KDZAction)
                {
                    this.tbackprocess.CancelProcess();
                }
                else if (this.isAction == TOTAction)
                {
                    this.tTOTPartitionExtraction.CancelProcess();
                }
            }
            if (!flag)
            {
                if (this.isAction == KDZAction)
                {
                    ArrayList al = new ArrayList();
                    foreach (ListViewItem item in this.listView1.Items)
                    {
                        if (item.Checked)
                        {
                            this.logme(item.SubItems[0].Text.ToString());
                            foreach (KDZContents contents in this.partitions)
                            {
                                if (contents.Name == item.SubItems[0].Text.ToString())
                                {
                                    al.Add(contents);
                                }
                            }
                        }
                    }
                    if (al.Count > 0)
                    {
                        this.setUIAccessAble(KDZAction);
                        this.backgroundWorker1_DoWork(al);
                    }
                }
                else if (this.isAction == TOTAction)
                {
                    ArrayList list2 = new ArrayList();
                    foreach (ListViewItem item2 in this.listView2.Items)
                    {
                        if (item2.Checked)
                        {
                            this.logme(item2.SubItems[0].Text.ToString());
                            foreach (PartPartitionInfo info in this.partitions)
                            {
                                if (info.name == item2.SubItems[0].Text.ToString())
                                {
                                    list2.Add(info);
                                    break;
                                }
                            }
                        }
                    }
                    if (list2.Count > 0)
                    {
                        this.setUIAccessAble(TOTAction);
                        this.TOTExtractionWorker_DoWork(list2);
                    }
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            bool flag = false;
            if ((this.DZThread != null) && this.DZThread.IsAlive)
            {
                flag = true;
                this.tDZExtractBackprocess.CancelProcess();
            }
            if (!flag)
            {
                ArrayList al = new ArrayList();
                foreach (ListViewItem item in this.listView2.Items)
                {
                    if (item.Checked)
                    {
                        this.logme(item.SubItems[0].Text.ToString());
                        foreach (DZContentInfo info in this.DZpartitions)
                        {
                            if (info.Name == item.SubItems[0].Text.ToString())
                            {
                                al.Add(info);
                                break;
                            }
                        }
                    }
                }
                if (al.Count > 0)
                {
                    this.setUIAccessAble(DZAction);
                    this.DZExBackWorker_DoWork(al);
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
        }

        private void button7_Click(object sender, EventArgs e)
        {
            bool flag = false;
            if ((this.SysMrgThread != null) && this.SysMrgThread.IsAlive)
            {
                flag = true;
                if (this.isAction == KDZAction)
                {
                    this.tSystemBinMergingBackprocess.CancelProcess();
                }
                else if (this.isAction == TOTAction)
                {
                    this.tTOTPartitionMergeBackprocess.CancelProcess();
                }
            }
            if (!flag)
            {
                string isAction = this.isAction;
                if (MessageBox.Show("Start merging " + isAction + " Partition part ? ", "Merge Action", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    if (this.isAction == KDZAction)
                    {
                        this.setUIAccessAble("SYSMRG");
                        this.MergetSystembinWorker_DoWork();
                    }
                    else if (this.isAction == TOTAction)
                    {
                        this.showPartitionDialogBox();
                    }
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            string workingdir = this.workingdir;
            workingdir = Directory.Exists(workingdir) ? workingdir : "";
            FolderBrowserDialogEx ex = new FolderBrowserDialogEx {
                Description = "Select a folder for the extracted files:",
                ShowNewFolderButton = true,
                ShowEditBox = true,
                SelectedPath = workingdir,
                ShowFullPathInEditBox = true
            };
            ex.RootFolder = Environment.SpecialFolder.Desktop;
            if (ex.ShowDialog() == DialogResult.OK)
            {
                workingdir = ex.SelectedPath;
                this.logme(" path " + workingdir);
                this.workingdir = workingdir;
                this.button8.Text = "Working Folder : " + this.workingdir;
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
        }

        private void button9_Click_1(object sender, EventArgs e)
        {
            new AboutBox1().ShowDialog();
        }

        private void byteArrayToContent(byte[] bytes)
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            KDZContents contents = (KDZContents) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(KDZContents));
            handle.Free();
            this.logme(" found file " + contents.Name);
            this.partitions.Add(contents);
        }

        private void byteArrayToDZContent(byte[] bytes)
        {
            DZContentInfo info = new DZContentInfo();
            info.mappingProperty(bytes);
            this.DZpartitions.Add(info);
        }

        private GPTHeader byteArrayToGPTHeaderContent(byte[] bytes)
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            GPTHeader header = (GPTHeader) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(GPTHeader));
            handle.Free();
            this.logme(" found file " + header.signature);
            return header;
        }

        private void changeActionAlert(string title)
        {
            if (title == KDZAction)
            {
                title = "KDZ and DZ";
            }
            MessageBox.Show(title + " is Current Active Action !! ", "Change Action", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
        }

        private void Clearlistview1()
        {
            this.listView1.Clear();
            this.LV1ckheader.Visible = false;
        }

        private void Clearlistview2()
        {
            this.listView2.Clear();
            this.LV2ckheader.Visible = false;
        }

        private void copy_errorRaise(object sender, msgEventArgs e)
        {
            this.logme(e.Msg);
        }

        private void copy_progressChanged(object sender, ProgressEventArgs e)
        {
            int num = (int) ((100.0 * e.Pos) / ((double) e.Length));
            this.pgBar1.Value = num;
            this.groupBox1.Text = e.Title;
        }

        private void copy_progressFinish(object sender, msgEventArgs e)
        {
            this.restUIAccessAble("");
            this.logme(e.Msg);
            this.tbackprocess.onProgressChange -= new ProgressChangeDelegate(this.copy_progressChanged);
            this.tbackprocess.onCopyError -= new CopyErrorDelegate(this.copy_errorRaise);
            this.tbackprocess.onProgressComplete -= new Completedelegate(this.copy_progressFinish);
        }

        private static void CopyStream(Stream src, Stream dest)
        {
            byte[] buffer = new byte[0x2000];
            for (int i = src.Read(buffer, 0, buffer.Length); i > 0; i = src.Read(buffer, 0, buffer.Length))
            {
                dest.Write(buffer, 0, i);
            }
            dest.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void DZExBackWorker_DoWork(ArrayList al)
        {
            this.tDZExtractBackprocess = new DZExtractBackprocess(this, al, this.workingdir);
            this.tDZExtractBackprocess.onProgressChange += new ProgressChangeDelegate(this.copy_progressChanged);
            this.tDZExtractBackprocess.onCopyError += new CopyErrorDelegate(this.copy_errorRaise);
            this.tDZExtractBackprocess.onProgressComplete += new Completedelegate(this.DZExt_progressFinish);
            this.DZThread = new Thread(new ThreadStart(this.tDZExtractBackprocess.do_extractDZFlle));
            this.DZThread.Start();
        }

        private void DZExt_progressFinish(object sender, msgEventArgs e)
        {
            this.restUIAccessAble("");
            this.logme(e.Msg);
            this.tDZExtractBackprocess.onProgressChange -= new ProgressChangeDelegate(this.copy_progressChanged);
            this.tDZExtractBackprocess.onCopyError -= new CopyErrorDelegate(this.copy_errorRaise);
            this.tDZExtractBackprocess.onProgressComplete -= new Completedelegate(this.DZExt_progressFinish);
        }

        private void extractDZFiles(ArrayList contents)
        {
            FileStream input = null;
            FileStream stream = null;
            string currentDirectory = Directory.GetCurrentDirectory();
            bool flag = false;
            try
            {
                input = new FileStream(this.DZFile, FileMode.Open);
                BinaryReader reader = new BinaryReader(input);
                foreach (DZContentInfo info in contents)
                {
                    if (flag)
                    {
                        break;
                    }
                    reader.BaseStream.Seek((long) info.Offset, SeekOrigin.Begin);
                    using (MemoryStream stream3 = new MemoryStream())
                    {
                        stream3.Write(reader.ReadBytes(info.Length), 0, info.Length);
                        stream3.Seek(0L, SeekOrigin.Begin);
                        Console.WriteLine(" MemoryStream " + stream3.Length.ToString());
                        stream = new FileStream(Path.Combine(currentDirectory, info.Name), FileMode.CreateNew);
                        using (ZlibStream stream4 = new ZlibStream(stream, CompressionMode.Decompress, true))
                        {
                            CopyStream(stream3, stream4);
                        }
                    }
                }
                if (flag)
                {
                    this.logme(" Abort extract partition file !! ");
                }
            }
            catch (Exception exception)
            {
                this.logme(" Exception on extract partition file " + exception.ToString());
            }
            finally
            {
                if (input != null)
                {
                    input.Close();
                }
                if (stream != null)
                {
                    stream.Close();
                }
                if (!flag)
                {
                    this.logme(" Complete on extract partition files ");
                }
            }
        }

        private int getPartitionLength(string name)
        {
            foreach (GPTPartitionInfo info in this.GPTpartitions)
            {
                if (name == info.part_name)
                {
                    return (int) (info.last_lba - info.first_lba);
                }
            }
            return -1;
        }

        private string getStringFromByte(byte[] bytes)
        {
            if ((bytes[0] != 0xff) && (bytes[0] != 0))
            {
                return Encoding.Default.GetString(bytes).Replace("\0", "");
            }
            return "";
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.button9 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.listView2 = new System.Windows.Forms.ListView();
            this.button6 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.label1 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.LabelFileName = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.pgBar1 = new System.Windows.Forms.ProgressBar();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // button9
            // 
            this.button9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button9.Location = new System.Drawing.Point(12, 12);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(55, 21);
            this.button9.TabIndex = 35;
            this.button9.Text = "关于";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button9_Click_1);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(206, 183);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(91, 21);
            this.button8.TabIndex = 34;
            this.button8.Text = "输出目录";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(303, 183);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(103, 21);
            this.button7.TabIndex = 33;
            this.button7.Text = "合并 System.bin";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // listView2
            // 
            this.listView2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView2.CheckBoxes = true;
            this.listView2.FullRowSelect = true;
            this.listView2.Location = new System.Drawing.Point(412, 84);
            this.listView2.Name = "listView2";
            this.listView2.Size = new System.Drawing.Size(448, 420);
            this.listView2.TabIndex = 32;
            this.listView2.UseCompatibleStateImageBehavior = false;
            this.listView2.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.listView2_DrawColumnHeader);
            this.listView2.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.listView2_DrawSubItem);
            // 
            // button6
            // 
            this.button6.Enabled = false;
            this.button6.Location = new System.Drawing.Point(341, 510);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(65, 21);
            this.button6.TabIndex = 31;
            this.button6.Text = "退出";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Visible = false;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(109, 183);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(91, 21);
            this.button5.TabIndex = 30;
            this.button5.Text = "提取 DZ";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(12, 183);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(91, 21);
            this.button4.TabIndex = 28;
            this.button4.Text = "提取 KDZ";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.CheckBoxes = true;
            this.listView1.FullRowSelect = true;
            this.listView1.Location = new System.Drawing.Point(12, 84);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(394, 92);
            this.listView1.TabIndex = 27;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.listView1_DrawColumnHeader);
            this.listView1.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.listView1_DrawSubItem);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(412, 51);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 12);
            this.label1.TabIndex = 26;
            this.label1.Text = "DZ 文件";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(798, 47);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(62, 21);
            this.button3.TabIndex = 25;
            this.button3.Text = "打开";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(465, 48);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(327, 21);
            this.textBox3.TabIndex = 24;
            // 
            // button2
            // 
            this.button2.Enabled = false;
            this.button2.Location = new System.Drawing.Point(270, 510);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(65, 21);
            this.button2.TabIndex = 23;
            this.button2.Text = "读取 KDZ";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Visible = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBox2
            // 
            this.textBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox2.Location = new System.Drawing.Point(12, 253);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(394, 251);
            this.textBox2.TabIndex = 22;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(347, 47);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(59, 21);
            this.button1.TabIndex = 21;
            this.button1.Text = "打开";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // LabelFileName
            // 
            this.LabelFileName.AutoSize = true;
            this.LabelFileName.Location = new System.Drawing.Point(12, 51);
            this.LabelFileName.Name = "LabelFileName";
            this.LabelFileName.Size = new System.Drawing.Size(77, 12);
            this.LabelFileName.TabIndex = 20;
            this.LabelFileName.Text = "KDZ/TOT 文件";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(95, 48);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(246, 21);
            this.textBox1.TabIndex = 19;
            // 
            // pgBar1
            // 
            this.pgBar1.Location = new System.Drawing.Point(6, 14);
            this.pgBar1.Name = "pgBar1";
            this.pgBar1.Size = new System.Drawing.Size(380, 21);
            this.pgBar1.TabIndex = 11;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.pgBar1);
            this.groupBox1.Location = new System.Drawing.Point(14, 205);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(392, 42);
            this.groupBox1.TabIndex = 29;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "-";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.InitialImage = null;
            this.pictureBox1.Location = new System.Drawing.Point(751, 505);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(108, 25);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 38;
            this.pictureBox1.TabStop = false;
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(95, 13);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(246, 21);
            this.textBox4.TabIndex = 39;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(872, 533);
            this.Controls.Add(this.textBox4);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.button9);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.listView2);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.LabelFileName);
            this.Controls.Add(this.textBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "LG 固件提取工具";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void initLVsckheader()
        {
            this.LV1ckheader = new System.Windows.Forms.CheckBox();
            this.LV1ckheader.Text = "";
            this.LV1ckheader.AutoSize = true;
            this.LV1ckheader.CheckedChanged += new EventHandler(this.LVsckheader_CheckedChanged);
            this.LV2ckheader = new System.Windows.Forms.CheckBox();
            this.LV2ckheader.Text = "";
            this.LV2ckheader.AutoSize = true;
            this.LV2ckheader.CheckedChanged += new EventHandler(this.LVsckheader_CheckedChanged);
        }

        private void listDZFileContents()
        {
            this.listView2.Clear();
            this.listView2.View = View.Details;
            this.listView2.LabelEdit = false;
            this.listView2.AllowColumnReorder = true;
            this.listView2.CheckBoxes = true;
            this.listView2.FullRowSelect = true;
            this.listView2.GridLines = true;
            this.listView2.Sorting = SortOrder.None;
            this.listView2.Columns.Add(" 文件名", 150, HorizontalAlignment.Center);
            this.listView2.Columns.Add("Length", 70, HorizontalAlignment.Right);
            this.listView2.Columns.Add("Offset", 100, HorizontalAlignment.Right);
            this.listView2.Columns.Add("Checksum", 150, HorizontalAlignment.Center);
            this.listView2.OwnerDraw = true;
            Point location = base.ClientRectangle.Location;
            location.X = 3;
            location.Y = 2;
            this.LV2ckheader.Visible = true;
            this.LV2ckheader.Location = location;
            string[] items = new string[4];
            foreach (DZContentInfo info in this.DZpartitions)
            {
                items[0] = info.Name;
                items[1] = info.Length.ToString();
                items[2] = info.Offset.ToString();
                items[3] = info.Checksum;
                ListViewItem item = new ListViewItem(items);
                this.listView2.Items.Add(item);
            }
        }

        private void listKDZContents()
        {
            this.listView1.Clear();
            this.listView1.View = View.Details;
            this.listView1.LabelEdit = false;
            this.listView1.AllowColumnReorder = true;
            this.listView1.CheckBoxes = true;
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.Sorting = SortOrder.Ascending;
            this.listView1.Columns.Add(" 文件名", 150, HorizontalAlignment.Center);
            this.listView1.Columns.Add("Length", 90, HorizontalAlignment.Right);
            this.listView1.Columns.Add("Offset", 70, HorizontalAlignment.Right);
            this.listView1.OwnerDraw = true;
            Point location = base.ClientRectangle.Location;
            location.X = 3;
            location.Y = 2;
            this.LV1ckheader.Visible = true;
            this.LV1ckheader.Location = location;
            string[] items = new string[4];
            foreach (KDZContents contents in this.partitions)
            {
                items[0] = contents.Name;
                items[1] = contents.Length.ToString();
                items[2] = contents.Offset.ToString();
                ListViewItem item = new ListViewItem(items);
                this.listView1.Items.Add(item);
            }
        }

        private void listTOTPartitionsPart()
        {
            this.listView2.Clear();
            this.listView2.View = View.Details;
            this.listView2.LabelEdit = false;
            this.listView2.AllowColumnReorder = true;
            this.listView2.CheckBoxes = true;
            this.listView2.FullRowSelect = true;
            this.listView2.GridLines = true;
            this.listView2.Sorting = SortOrder.None;
            this.listView2.Columns.Add(" 文件名", 150, HorizontalAlignment.Center);
            this.listView2.Columns.Add("Length", 70, HorizontalAlignment.Right);
            this.listView2.Columns.Add("Offset", 100, HorizontalAlignment.Right);
            this.listView2.Columns.Add("Disk Offset", 70, HorizontalAlignment.Center);
            this.listView2.Columns.Add("Pad", 70, HorizontalAlignment.Center);
            this.listView2.OwnerDraw = true;
            Point location = base.ClientRectangle.Location;
            location.X = 3;
            location.Y = 2;
            this.LV2ckheader.Visible = true;
            this.LV2ckheader.Location = location;
            string[] items = new string[5];
            foreach (PartPartitionInfo info in this.partitions)
            {
                items[0] = info.name;
                items[1] = info.length.ToString();
                items[2] = info.offset.ToString();
                items[3] = info.unknow_0.ToString();
                items[4] = info.whitespace.ToString();
                ListViewItem item = new ListViewItem(items);
                this.listView2.Items.Add(item);
            }
        }

        private void listView1_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            TextFormatFlags leftAndRightPadding = TextFormatFlags.LeftAndRightPadding;
            e.DrawBackground();
            ((System.Windows.Forms.ListView) sender).Controls.Add(this.LV1ckheader);
            CheckBoxRenderer.DrawCheckBox(e.Graphics, this.LV1ckheader.Location, this.LV1ckheader.Checked ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal);
            e.DrawText(leftAndRightPadding);
        }

        private void listView1_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void listView2_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            TextFormatFlags leftAndRightPadding = TextFormatFlags.LeftAndRightPadding;
            e.DrawBackground();
            ((System.Windows.Forms.ListView) sender).Controls.Add(this.LV2ckheader);
            CheckBoxRenderer.DrawCheckBox(e.Graphics, this.LV2ckheader.Location, this.LV2ckheader.Checked ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal);
            e.DrawText(leftAndRightPadding);
        }

        private void listView2_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        public void LoCopyStream(Stream input, Stream output)
        {
            int num;
            byte[] buffer = new byte[0x7d0];
            while ((num = input.Read(buffer, 0, 0x7d0)) > 0)
            {
                output.Write(buffer, 0, num);
            }
            output.Flush();
        }

        private void logme(string val)
        {
            this.textBox2.Text = val + "\r\n" + this.textBox2.Text;
            Console.WriteLine("MSG]-> " + val);
        }

        private void LVsckheader_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.ListView parent = (System.Windows.Forms.ListView) ((System.Windows.Forms.CheckBox) sender).Parent;
            foreach (ListViewItem item in parent.Items)
            {
                item.Checked = ((System.Windows.Forms.CheckBox) sender).Checked;
            }
        }

        private void MergetSystembinWorker_DoWork()
        {
            this.tSystemBinMergingBackprocess = new SystemBinMergingBackprocess(this, this.workingdir);
            this.tSystemBinMergingBackprocess.onProgressChange += new ProgressChangeDelegate(this.copy_progressChanged);
            this.tSystemBinMergingBackprocess.onCopyError += new CopyErrorDelegate(this.copy_errorRaise);
            this.tSystemBinMergingBackprocess.onProgressComplete += new Completedelegate(this.SysMrg_progressFinish);
            this.SysMrgThread = new Thread(new ThreadStart(this.tSystemBinMergingBackprocess.do_merge_systembin));
            this.SysMrgThread.Start();
        }

        private void popDialog(string title, string msg)
        {
            MessageBox.Show(title, msg, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
        }

        private void readDZFile()
        {
            this.DZpartitions.Clear();
            FileStream input = null;
            try
            {
                input = new FileStream(this.DZFile, FileMode.Open);
                BinaryReader reader = new BinaryReader(input);
                this.logme(" byte read position " + reader.BaseStream.Position.ToString());
                bool flag = this.readDZHeader(reader.ReadBytes(this.DZHeaderSize));
                this.logme(" byte read position " + reader.BaseStream.Position.ToString());
                if (!flag)
                {
                    return;
                }
                reader.BaseStream.Seek((long) DZPartitionHeaderSize, SeekOrigin.Begin);
            Label_0095:
                this.byteArrayToDZContent(reader.ReadBytes(DZPartitionHeaderSize));
                DZContentInfo info = (DZContentInfo) this.DZpartitions[this.DZpartitions.Count - 1];
                info.Offset = (int) reader.BaseStream.Position;
                if (BitConverter.ToString(info.Header) != DZ_Partition_Header)
                {
                    this.logme(" Bad DZ sub header!! ");
                    this.DZpartitions.RemoveAt(this.DZpartitions.Count - 1);
                }
                else if ((reader.BaseStream.Position + info.Length) < ((int) reader.BaseStream.Length))
                {
                    reader.BaseStream.Seek((long) info.Length, SeekOrigin.Current);
                    goto Label_0095;
                }
            }
            catch (Exception exception)
            {
                this.logme(" Exception on read DZ file " + exception.ToString());
            }
            finally
            {
                if (input != null)
                {
                    input.Close();
                }
            }
        }

        private bool readDZHeader(byte[] bytes)
        {
            string str = BitConverter.ToString(bytes);
            this.logme(" header  " + str);
            return (str == DZ_Header);
        }

        private GPTHeader readGPTHeader(BinaryReader readBinary)
        {
            readBinary.BaseStream.Seek((long) GPTHeader_post, SeekOrigin.Begin);
            readBinary.BaseStream.Seek((long) MBRSize, SeekOrigin.Current);
            return this.byteArrayToGPTHeaderContent(readBinary.ReadBytes(GPTHeaderSize));
        }

        private void readGPTPartitions(BinaryReader readBinary, int count)
        {
            this.GPTpartitions.Clear();
            for (int i = 0; i < count; i++)
            {
                GPTPartitionInfo info = new GPTPartitionInfo();
                info.mappingProperty(readBinary.ReadBytes(GPTPartitionSize));
                this.GPTpartitions.Add(info);
                this.logme("Name " + info.part_name + " First LBA " + info.first_lba.ToString() + " Last LBA " + info.last_lba.ToString());
            }
        }

        private bool readKDZFile()
        {
            this.partitions.Clear();
            FileStream input = null;
            long position = 0L;
            bool flag = false;
            try
            {
                input = new FileStream(this.KDZFile, FileMode.Open);
                BinaryReader reader = new BinaryReader(input);
                this.logme(" byte read position " + reader.BaseStream.Position.ToString());
                bool flag2 = this.readKDZHeader(reader.ReadBytes(this.KDZHeaderSize));
                this.logme(" byte read position " + reader.BaseStream.Position.ToString());
                if (flag2)
                {
                    while (true)
                    {
                        this.byteArrayToContent(reader.ReadBytes(KDZPartitionHeaderSize));
                        position = reader.BaseStream.Position;
                        if (!(this.binTOhex(reader.ReadBytes(4)) != "00-00-00-00"))
                        {
                            break;
                        }
                        this.logme(" byte read position " + reader.BaseStream.Position.ToString());
                        reader.BaseStream.Position = position;
                        this.logme(" byte read position " + reader.BaseStream.Position.ToString());
                    }
                }
                if (this.partitions.Count > 0)
                {
                    KDZContents contents = (KDZContents) this.partitions[0];
                    this.logme(" found file " + contents.Name);
                    flag = true;
                }
            }
            catch (Exception exception)
            {
                this.logme(" Exception on read kdz file " + exception.ToString());
            }
            finally
            {
                if (input != null)
                {
                    input.Close();
                }
            }
            return flag;
        }

        private bool readKDZHeader(byte[] bytes)
        {
            string str = BitConverter.ToString(bytes);
            this.logme(" header  " + str);
            return (str == KDZ_Header);
        }

        private bool readTOTFile()
        {
            bool flag = false;
            this.partitions.Clear();
            FileStream input = null;
            try
            {
                input = new FileStream(this.KDZFile, FileMode.Open);
                BinaryReader readBinary = new BinaryReader(input);
                readBinary.BaseStream.Seek((long) GPTHeader_post, SeekOrigin.Begin);
                readBinary.BaseStream.Seek((long) MBRSize, SeekOrigin.Current);
                this.logme(" byte read position " + readBinary.BaseStream.Position.ToString());
                bool flag2 = this.readTOTHeader(readBinary.ReadBytes(GPTHeaderIdLength));
                this.logme(" byte read position " + readBinary.BaseStream.Position.ToString());
                if (flag2)
                {
                    flag2 = false;
                    string str = "";
                    foreach (Dictionary<string, int> dictionary in this.setupTOTPossibility())
                    {
                        readBinary.BaseStream.Seek((long) dictionary["Model_Pos"], SeekOrigin.Begin);
                        str = this.getStringFromByte(readBinary.ReadBytes(dictionary["Model_Length"]));
                        if (str != "")
                        {
                            this.logme("Model Name " + str);
                            readBinary.BaseStream.Seek((long) dictionary["FileName_Pos"], SeekOrigin.Begin);
                            this.logme("File Name " + this.getStringFromByte(readBinary.ReadBytes(dictionary["FileName_Length"])));
                            this.TOTPartitionsInfo_length = dictionary["PtInfo_Length"];
                            this.TOTPartitionsInfo_pos = dictionary["PtInfo_Pos"];
                            this.TOTPartitionsName_length = dictionary["PtName_Length"];
                            this.TOTPartitionsName_pos = dictionary["PtName_Pos"];
                            flag2 = true;
                            break;
                        }
                    }
                    if (flag2)
                    {
                        GPTHeader header = this.readGPTHeader(readBinary);
                        this.readGPTPartitions(readBinary, header.pent_num - 1);
                        this.readTOTPartitions(readBinary);
                    }
                }
                if (this.partitions.Count > 0)
                {
                    PartPartitionInfo info = (PartPartitionInfo) this.partitions[0];
                    this.logme(" found file " + info.name);
                    flag = true;
                }
            }
            catch (Exception exception)
            {
                this.logme(" Exception on read kdz file " + exception.ToString());
            }
            finally
            {
                if (input != null)
                {
                    input.Close();
                }
            }
            return flag;
        }

        private bool readTOTHeader(byte[] bytes)
        {
            string str = BitConverter.ToString(bytes);
            this.logme(" header  " + str);
            return (str == TOT_Header);
        }

        private void readTOTPartitions(BinaryReader readBinary)
        {
            ArrayList list = new ArrayList();
            string str2 = "";
            int num = 0;
            this.partitions.Clear();
            readBinary.BaseStream.Seek((long) this.TOTPartitionsName_pos, SeekOrigin.Begin);
            while (true)
            {
                byte[] bytes = readBinary.ReadBytes(this.TOTPartitionsName_length);
                if (bytes[0] == 0xff)
                {
                    break;
                }
                string str = Encoding.Default.GetString(bytes).Replace("\0", "");
                list.Add(str);
            }
            if (list.Count > 0)
            {
                int num2 = 0;
                readBinary.BaseStream.Seek((long) this.TOTPartitionsInfo_pos, SeekOrigin.Begin);
                foreach (string str3 in list)
                {
                    PartPartitionInfo info2;
                    string[] strArray;
                    PartPartitionInfo info = new PartPartitionInfo();
                    info.mappingProperty(readBinary.ReadBytes(this.TOTPartitionsInfo_length), str3);
                    if ((str3 != str2) && (this.partitions.Count > 0))
                    {
                        info2 = (PartPartitionInfo) this.partitions[this.partitions.Count - 1];
                        str2 = info.name.Split(new char[] { '_' })[0];
                        if (Regex.IsMatch(info2.name, "^(system|cache|userdata)"))
                        {
                            strArray = info2.name.Split(new char[] { '_' });
                            int num3 = (this.getPartitionLength(strArray[0]) - num) - info2.length;
                            info2.whitespace = num3;
                            this.partitions[this.partitions.Count - 1] = info2;
                        }
                        num = 0;
                        num2 = 0;
                    }
                    if (num2 > 0)
                    {
                        strArray = info.name.Split(new char[] { '_' });
                        if ((strArray.Length == 2) && Regex.IsMatch(strArray[0], "^(system|cache|userdata)"))
                        {
                            info2 = (PartPartitionInfo) this.partitions[this.partitions.Count - 1];
                            int num4 = (info.unknow_0 - info2.unknow_0) - info2.length;
                            info2.whitespace = num4;
                            this.partitions[this.partitions.Count - 1] = info2;
                            num += info2.length + info2.whitespace;
                        }
                    }
                    this.partitions.Add(info);
                    num2++;
                }
            }
            this.logme(" count parts partition found " + list.Count.ToString());
        }

        private bool removeAnyExistingFile(string filepath)
        {
            bool flag = true;
            try
            {
                if (File.Exists(filepath))
                {
                    File.Delete(filepath);
                }
            }
            catch (Exception exception)
            {
                flag = false;
                this.logme(" Exception on extract partition file " + exception.ToString());
            }
            return flag;
        }

        private void restUIAccessAble(string action)
        {
            this.button7.Enabled = true;
            this.button4.Enabled = true;
            this.button5.Enabled = true;
            this.button1.Enabled = true;
            this.button3.Enabled = true;
            this.LabelFileName.Text = "KDZ 文件";
            this.button4.Text = "提取 KDZ";
            this.button5.Text = "提取 DZ";
            this.button7.Text = "合并 System.bin";
            if (this.isAction == KDZAction)
            {
                this.LabelFileName.Text = "KDZ 文件";
                this.button4.Text = "提取 KDZ";
                this.button7.Text = "合并 System.bin";
            }
            else if (action == "DZ")
            {
                if (this.isAction == TOTAction)
                {
                    this.KDZFile = "";
                    this.textBox1.Text = "";
                }
                this.partitions.Clear();
                this.isAction = KDZAction;
                this.button5.Enabled = true;
                this.button4.Text = "提取 KDZ";
                this.button7.Text = "合并 System.bin";
            }
            else if (this.isAction == TOTAction)
            {
                this.textBox3.Text = "";
                this.DZFile = "";
                this.LabelFileName.Text = "TOT 文件";
                this.button4.Text = "提取 TOT";
                this.button7.Text = "合并部分文件";
                this.button5.Enabled = false;
            }
        }

        private void setUIAccessAble(string action)
        {
            this.button4.Enabled = true;
            this.button5.Enabled = true;
            this.button7.Enabled = true;
            if (action == KDZAction)
            {
                this.button4.Text = "退出";
                this.button7.Enabled = false;
                this.button5.Enabled = false;
                this.button1.Enabled = false;
                this.button3.Enabled = false;
            }
            else if (action == "DZ")
            {
                this.button5.Text = "退出";
                this.button7.Enabled = false;
                this.button4.Enabled = false;
                this.button1.Enabled = false;
                this.button3.Enabled = false;
            }
            else if (action == "SYSMRG")
            {
                this.button7.Text = "退出";
                this.button5.Enabled = false;
                this.button4.Enabled = false;
                this.button1.Enabled = false;
                this.button3.Enabled = false;
            }
            else if (action == "PRTMRG")
            {
                this.button7.Text = "退出";
                this.button5.Enabled = false;
                this.button4.Enabled = false;
                this.button1.Enabled = false;
                this.button3.Enabled = false;
            }
            else if (action == TOTAction)
            {
                this.button4.Text = "退出";
                this.button7.Enabled = false;
                this.button5.Enabled = false;
                this.button1.Enabled = false;
                this.button3.Enabled = false;
            }
        }

        private List<object> setupTOTPossibility()
        {
            List<object> list = new List<object>();
            Dictionary<string, int> item = new Dictionary<string, int>();
            item.Add("PtName_Pos", 0x6230);
            item.Add("PtName_Length", 0x20);
            item.Add("PtInfo_Pos", 0x2010);
            item.Add("PtInfo_Length", 0x10);
            item.Add("Model_Pos", 0x6013);
            item.Add("Model_Length", 0x11);
            item.Add("FileName_Pos", 0x6024);
            item.Add("FileName_Length", 0xc2);
            list.Add(item);
            item = new Dictionary<string, int>();
            item.Add("PtName_Pos", 0x4220);
            item.Add("PtName_Length", 0x20);
            item.Add("PtInfo_Pos", 0x2010);
            item.Add("PtInfo_Length", 0x10);
            item.Add("Model_Pos", 0x4003);
            item.Add("Model_Length", 0x2d);
            item.Add("FileName_Pos", 0x4030);
            item.Add("FileName_Length", 100);
            list.Add(item);
            return list;
        }

        private void showPartitionDialogBox()
        {
            new FormDialogBoxPartition().showme(this);
        }

        private void SysMrg_progressFinish(object sender, msgEventArgs e)
        {
            this.restUIAccessAble("");
            this.logme(e.Msg);
            this.tSystemBinMergingBackprocess.onProgressChange -= new ProgressChangeDelegate(this.copy_progressChanged);
            this.tSystemBinMergingBackprocess.onCopyError -= new CopyErrorDelegate(this.copy_errorRaise);
            this.tSystemBinMergingBackprocess.onProgressComplete -= new Completedelegate(this.SysMrg_progressFinish);
        }

        private void totextract_progressFinish(object sender, msgEventArgs e)
        {
            this.restUIAccessAble("");
            this.logme(e.Msg);
            this.tTOTPartitionExtraction.onProgressChange -= new ProgressChangeDelegate(this.copy_progressChanged);
            this.tTOTPartitionExtraction.onCopyError -= new CopyErrorDelegate(this.copy_errorRaise);
            this.tTOTPartitionExtraction.onProgressComplete -= new Completedelegate(this.totextract_progressFinish);
        }

        private void TOTExtractionWorker_DoWork(ArrayList al)
        {
            this.tTOTPartitionExtraction = new TOTPartitionExtraction(this, al, this.workingdir);
            this.tTOTPartitionExtraction.onProgressChange += new ProgressChangeDelegate(this.copy_progressChanged);
            this.tTOTPartitionExtraction.onCopyError += new CopyErrorDelegate(this.copy_errorRaise);
            this.tTOTPartitionExtraction.onProgressComplete += new Completedelegate(this.totextract_progressFinish);
            this.oThread = new Thread(new ThreadStart(this.tTOTPartitionExtraction.do_extractTOTFlle));
            this.oThread.Start();
        }

        private void totmerge_progressFinish(object sender, msgEventArgs e)
        {
            this.restUIAccessAble("");
            this.logme(e.Msg);
            this.tTOTPartitionMergeBackprocess.onProgressChange -= new ProgressChangeDelegate(this.copy_progressChanged);
            this.tTOTPartitionMergeBackprocess.onCopyError -= new CopyErrorDelegate(this.copy_errorRaise);
            this.tTOTPartitionMergeBackprocess.onProgressComplete -= new Completedelegate(this.totmerge_progressFinish);
        }

        public void TOTMergingWorker_DoWork(string partition_name)
        {
            this.setUIAccessAble("PRTMRG");
            this.tTOTPartitionMergeBackprocess = new TOTPartitionMergeBackprocess(this, this.workingdir, partition_name);
            this.tTOTPartitionMergeBackprocess.onProgressChange += new ProgressChangeDelegate(this.copy_progressChanged);
            this.tTOTPartitionMergeBackprocess.onCopyError += new CopyErrorDelegate(this.copy_errorRaise);
            this.tTOTPartitionMergeBackprocess.onProgressComplete += new Completedelegate(this.totmerge_progressFinish);
            this.SysMrgThread = new Thread(new ThreadStart(this.tTOTPartitionMergeBackprocess.do_merge_totpartition));
            this.SysMrgThread.Start();
        }

        public class backprocess
        {
            private static Semaphore _pool = new Semaphore(3, 3);
            private bool Cancel;
            private ArrayList listContent;
            private Form1 parent;
            private string workingdir;

            public event Form1.CopyErrorDelegate onCopyError;

            public event Form1.ProgressChangeDelegate onProgressChange;

            public event Form1.Completedelegate onProgressComplete;

            public backprocess(object parent, ArrayList al, string workingdir)
            {
                this.workingdir = workingdir;
                this.listContent = al;
                this.parent = (Form1) parent;
            }

            public void CancelProcess()
            {
                _pool.WaitOne();
                this.Cancel = true;
                _pool.Release();
            }

            protected virtual void CopyError(Form1.msgEventArgs e)
            {
                Form1.CopyErrorDelegate onCopyError = this.onCopyError;
                if (onCopyError != null)
                {
                    foreach (Form1.CopyErrorDelegate delegate3 in onCopyError.GetInvocationList())
                    {
                        ISynchronizeInvoke target = delegate3.Target as ISynchronizeInvoke;
                        try
                        {
                            if ((target != null) && target.InvokeRequired)
                            {
                                target.Invoke(delegate3, new object[] { this, e });
                            }
                            else
                            {
                                delegate3.DynamicInvoke(new object[] { this, e });
                            }
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine("MSG]-> CopyError " + exception.Message.ToString());
                        }
                    }
                }
            }

            public void do_extraction()
            {
                this.extractKDZPartitions(this.listContent);
            }

            public void extractKDZPartitions(ArrayList contents)
            {
                Form1.msgEventArgs args2;
                FileStream input = null;
                FileStream output = null;
                string workingdir = this.workingdir;
                int num = 0x3200000;
                int num2 = 0xc8000;
                int count = 0x2000;
                try
                {
                    input = new FileStream(this.parent.KDZFile, FileMode.Open);
                    BinaryReader reader = new BinaryReader(input);
                    foreach (Form1.KDZContents contents2 in contents)
                    {
                        if (this.Cancel)
                        {
                            break;
                        }
                        if (contents2.Length > num)
                        {
                            count = num2;
                        }
                        string filepath = Path.Combine(workingdir, contents2.Name);
                        if (!this.removeAnyExistingFile(filepath))
                        {
                            break;
                        }
                        output = new FileStream(filepath, FileMode.CreateNew);
                        BinaryWriter writer = new BinaryWriter(output);
                        reader.BaseStream.Seek((long) contents2.Offset, SeekOrigin.Begin);
                        Form1.ProgressEventArgs e = new Form1.ProgressEventArgs(contents2.Name, (int) reader.BaseStream.Position, contents2.Length);
                        do
                        {
                            e.setValues(contents2.Name, (int) reader.BaseStream.Position, contents2.Length);
                            this.ProgressChange(e);
                            writer.Write(reader.ReadBytes(count));
                            if ((writer.BaseStream.Position + count) > contents2.Length)
                            {
                                long num4 = contents2.Length - writer.BaseStream.Position;
                                writer.Write(reader.ReadBytes((int) num4));
                                e.setValues(contents2.Name, contents2.Length, contents2.Length);
                                this.ProgressChange(e);
                                break;
                            }
                        }
                        while (!this.Cancel);
                        output.Close();
                    }
                    if (this.Cancel)
                    {
                        args2 = new Form1.msgEventArgs(" Abort extract partition file !! ");
                        this.CopyError(args2);
                    }
                }
                catch (Exception exception)
                {
                    args2 = new Form1.msgEventArgs(" Exception on extract partition file " + exception.ToString());
                    this.CopyError(args2);
                }
                finally
                {
                    if (input != null)
                    {
                        input.Close();
                    }
                    if (output != null)
                    {
                        output.Close();
                    }
                    if (!this.Cancel)
                    {
                        args2 = new Form1.msgEventArgs(" Complete on extract partition files ");
                        this.CopyError(args2);
                    }
                    args2 = new Form1.msgEventArgs(" Progress Done !! ");
                    this.ProgressComplete(args2);
                }
            }

            protected virtual void ProgressChange(Form1.ProgressEventArgs e)
            {
                Form1.ProgressChangeDelegate onProgressChange = this.onProgressChange;
                if (onProgressChange != null)
                {
                    foreach (Form1.ProgressChangeDelegate delegate3 in onProgressChange.GetInvocationList())
                    {
                        ISynchronizeInvoke target = delegate3.Target as ISynchronizeInvoke;
                        try
                        {
                            if ((target != null) && target.InvokeRequired)
                            {
                                target.Invoke(delegate3, new object[] { this, e });
                            }
                            else
                            {
                                delegate3.DynamicInvoke(new object[] { this, e });
                            }
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine("MSG]-> ProgressChange " + exception.Message.ToString());
                        }
                    }
                }
            }

            protected virtual void ProgressComplete(Form1.msgEventArgs e)
            {
                Form1.Completedelegate onProgressComplete = this.onProgressComplete;
                if (onProgressComplete != null)
                {
                    foreach (Form1.Completedelegate completedelegate2 in onProgressComplete.GetInvocationList())
                    {
                        ISynchronizeInvoke target = completedelegate2.Target as ISynchronizeInvoke;
                        try
                        {
                            if ((target != null) && target.InvokeRequired)
                            {
                                target.Invoke(completedelegate2, new object[] { this, e });
                            }
                            else
                            {
                                completedelegate2.DynamicInvoke(new object[] { this, e });
                            }
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine("MSG]-> ProgressComplete " + exception.Message.ToString());
                        }
                    }
                }
            }

            private bool removeAnyExistingFile(string filepath)
            {
                bool flag = true;
                try
                {
                    if (File.Exists(filepath))
                    {
                        File.Delete(filepath);
                    }
                }
                catch (Exception exception)
                {
                    flag = false;
                    Form1.msgEventArgs e = new Form1.msgEventArgs(" Exception on extract partition file " + exception.ToString());
                    this.CopyError(e);
                }
                return flag;
            }
        }

        public delegate void Completedelegate(object sender, Form1.msgEventArgs e);

        public delegate void CopyErrorDelegate(object sender, Form1.msgEventArgs e);

        public class DZContentInfo
        {
            public string Checksum;
            public byte[] Header;
            public int Length;
            public string Name;
            public int Offset;
            public byte[] Pad;
            public int Spacer1;
            public int Spacer2;
            public int Spacer3;
            public string type;
            public int Unknow;

            private byte[] getValue(byte[] src, int pos, int len)
            {
                byte[] destinationArray = new byte[len];
                Array.Copy(src, pos, destinationArray, 0, len);
                return destinationArray;
            }

            public void mappingProperty(byte[] bytes)
            {
                this.Header = this.getValue(bytes, 0, 4);
                this.type = Encoding.Default.GetString(this.getValue(bytes, 4, 0x20)).Replace("\0", "");
                this.Name = Encoding.Default.GetString(this.getValue(bytes, 0x24, 0x40)).Replace("\0", "");
                this.Unknow = BitConverter.ToInt32(this.getValue(bytes, 100, 4), 0);
                this.Length = BitConverter.ToInt32(this.getValue(bytes, 0x68, 4), 0);
                this.Checksum = BitConverter.ToString(this.getValue(bytes, 0x6c, 0x10));
                this.Spacer1 = BitConverter.ToInt32(this.getValue(bytes, 0x7c, 4), 0);
                this.Spacer2 = BitConverter.ToInt32(this.getValue(bytes, 0x80, 4), 0);
                this.Spacer3 = BitConverter.ToInt32(this.getValue(bytes, 0x84, 4), 0);
                this.Pad = this.getValue(bytes, 0x88, 0x178);
            }
        }

        public class DZExtractBackprocess
        {
            private static Semaphore _pool = new Semaphore(3, 3);
            private bool Cancel;
            private ArrayList listContent;
            private Form1 parent;
            private string workingdir;

            public event Form1.CopyErrorDelegate onCopyError;

            public event Form1.ProgressChangeDelegate onProgressChange;

            public event Form1.Completedelegate onProgressComplete;

            public DZExtractBackprocess(object parent, ArrayList al, string workingdir)
            {
                this.workingdir = workingdir;
                this.listContent = al;
                this.parent = (Form1) parent;
            }

            public void CancelProcess()
            {
                _pool.WaitOne();
                this.Cancel = true;
                _pool.Release();
            }

            protected virtual void CopyError(Form1.msgEventArgs e)
            {
                Form1.CopyErrorDelegate onCopyError = this.onCopyError;
                if (onCopyError != null)
                {
                    foreach (Form1.CopyErrorDelegate delegate3 in onCopyError.GetInvocationList())
                    {
                        ISynchronizeInvoke target = delegate3.Target as ISynchronizeInvoke;
                        try
                        {
                            if ((target != null) && target.InvokeRequired)
                            {
                                target.Invoke(delegate3, new object[] { this, e });
                            }
                            else
                            {
                                delegate3.DynamicInvoke(new object[] { this, e });
                            }
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine("MSG]-> CopyError " + exception.Message.ToString());
                        }
                    }
                }
            }

            public void do_extractDZFlle()
            {
                this.extractDZFlle(this.listContent);
            }

            public void extractDZFlle(ArrayList contents)
            {
                Form1.msgEventArgs args2;
                FileStream input = null;
                FileStream stream = null;
                string workingdir = this.workingdir;
                int num = 0x3200000;
                int num2 = 0xc8000;
                int num3 = 0x2000;
                try
                {
                    input = new FileStream(this.parent.DZFile, FileMode.Open);
                    BinaryReader reader = new BinaryReader(input);
                    foreach (Form1.DZContentInfo info in contents)
                    {
                        if (this.Cancel)
                        {
                            break;
                        }
                        if (info.Length > num)
                        {
                            num3 = num2;
                        }
                        reader.BaseStream.Seek((long) info.Offset, SeekOrigin.Begin);
                        using (MemoryStream stream3 = new MemoryStream())
                        {
                            stream3.Write(reader.ReadBytes(info.Length), 0, info.Length);
                            stream3.Seek(0L, SeekOrigin.Begin);
                            Console.WriteLine(" MemoryStream " + stream3.Length.ToString());
                            string filepath = Path.Combine(workingdir, info.Name);
                            if (!this.removeAnyExistingFile(filepath))
                            {
                                break;
                            }
                            Form1.ProgressEventArgs e = new Form1.ProgressEventArgs(info.Name, (int) stream3.Position, info.Length);
                            stream = new FileStream(filepath, FileMode.CreateNew);
                            using (ZlibStream stream4 = new ZlibStream(stream, CompressionMode.Decompress, true))
                            {
                                byte[] buffer = new byte[num3];
                                int count = stream3.Read(buffer, 0, buffer.Length);
                                while (count > 0)
                                {
                                    if (this.Cancel)
                                    {
                                        break;
                                    }
                                    stream4.Write(buffer, 0, count);
                                    count = stream3.Read(buffer, 0, buffer.Length);
                                    e.setValues(info.Name, (int) stream3.Position, info.Length);
                                    this.ProgressChange(e);
                                }
                                stream4.Flush();
                            }
                            stream.Close();
                        }
                    }
                    if (this.Cancel)
                    {
                        args2 = new Form1.msgEventArgs(" Abort extract partition file !! ");
                        this.CopyError(args2);
                    }
                }
                catch (Exception exception)
                {
                    args2 = new Form1.msgEventArgs(" Exception on extract partition file " + exception.ToString());
                    this.CopyError(args2);
                }
                finally
                {
                    if (input != null)
                    {
                        input.Close();
                    }
                    if (stream != null)
                    {
                        stream.Close();
                    }
                    if (!this.Cancel)
                    {
                        args2 = new Form1.msgEventArgs(" Complete on extract DZ partition files ");
                        this.CopyError(args2);
                    }
                    args2 = new Form1.msgEventArgs(" Progress extractDZFlle Done !! ");
                    this.ProgressComplete(args2);
                }
            }

            protected virtual void ProgressChange(Form1.ProgressEventArgs e)
            {
                Form1.ProgressChangeDelegate onProgressChange = this.onProgressChange;
                if (onProgressChange != null)
                {
                    foreach (Form1.ProgressChangeDelegate delegate3 in onProgressChange.GetInvocationList())
                    {
                        ISynchronizeInvoke target = delegate3.Target as ISynchronizeInvoke;
                        try
                        {
                            if ((target != null) && target.InvokeRequired)
                            {
                                target.Invoke(delegate3, new object[] { this, e });
                            }
                            else
                            {
                                delegate3.DynamicInvoke(new object[] { this, e });
                            }
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine("MSG]-> ProgressChange " + exception.Message.ToString());
                        }
                    }
                }
            }

            protected virtual void ProgressComplete(Form1.msgEventArgs e)
            {
                Form1.Completedelegate onProgressComplete = this.onProgressComplete;
                if (onProgressComplete != null)
                {
                    foreach (Form1.Completedelegate completedelegate2 in onProgressComplete.GetInvocationList())
                    {
                        ISynchronizeInvoke target = completedelegate2.Target as ISynchronizeInvoke;
                        try
                        {
                            if ((target != null) && target.InvokeRequired)
                            {
                                target.Invoke(completedelegate2, new object[] { this, e });
                            }
                            else
                            {
                                completedelegate2.DynamicInvoke(new object[] { this, e });
                            }
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine("MSG]-> ProgressComplete " + exception.Message.ToString());
                        }
                    }
                }
            }

            private bool removeAnyExistingFile(string filepath)
            {
                bool flag = true;
                try
                {
                    if (File.Exists(filepath))
                    {
                        File.Delete(filepath);
                    }
                }
                catch (Exception exception)
                {
                    flag = false;
                    Form1.msgEventArgs e = new Form1.msgEventArgs(" Exception on extract partition file " + exception.ToString());
                    this.CopyError(e);
                }
                return flag;
            }
        }

        [StructLayout(LayoutKind.Sequential, Size=0x200, Pack=1)]
        public struct GPTHeader
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=8)]
            public string signature;
            public int reversion;
            public int hsize;
            public int crc;
            public int unknow;
            public long reserved;
            public long current_lba;
            public long first_lba;
            public long last_lba;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x10)]
            public string disk_guid;
            public long pent_lba;
            public int pent_num;
            public int pent_size;
            public int crc_part;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=420)]
            public string char_end;
        }

        public class GPTPartitionInfo
        {
            public int att_flags;
            public long first_lba;
            public long last_lba;
            public string part_name;
            public string ptype_guid;
            public byte[] unknow;
            public string upart_guid;

            private byte[] getValue(byte[] src, int pos, int len)
            {
                byte[] destinationArray = new byte[len];
                Array.Copy(src, pos, destinationArray, 0, len);
                return destinationArray;
            }

            public void mappingProperty(byte[] bytes)
            {
                this.ptype_guid = BitConverter.ToString(this.getValue(bytes, 0, 0x10));
                this.upart_guid = BitConverter.ToString(this.getValue(bytes, 0x10, 0x10));
                this.first_lba = BitConverter.ToInt64(this.getValue(bytes, 0x20, 8), 0);
                this.last_lba = BitConverter.ToInt64(this.getValue(bytes, 40, 8), 0);
                this.unknow = this.getValue(bytes, 0x30, 4);
                this.att_flags = BitConverter.ToInt32(this.getValue(bytes, 0x34, 4), 0);
                this.part_name = Encoding.Default.GetString(this.getValue(bytes, 0x38, 0x48)).Replace("\0", "");
            }
        }

        [StructLayout(LayoutKind.Explicit, Size=0x110, Pack=1)]
        public struct KDZContents
        {
            [MarshalAs(UnmanagedType.I4), FieldOffset(0x100)]
            public int Length;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x20), FieldOffset(0)]
            public string Name;
            [MarshalAs(UnmanagedType.I4), FieldOffset(0x108)]
            public int Offset;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0xe0), FieldOffset(0x20)]
            public string Pad;
            [MarshalAs(UnmanagedType.I4), FieldOffset(260)]
            public int Unknow1;
            [MarshalAs(UnmanagedType.I4), FieldOffset(0x10c)]
            public int Unknow2;
        }

        public class msgEventArgs : EventArgs
        {
            private string msg;

            public msgEventArgs(string msg)
            {
                this.msg = msg;
            }

            public string Msg
            {
                get
                {
                    return this.msg;
                }
            }
        }

        public class PartPartitionInfo
        {
            public int length;
            public string name;
            public int offset;
            public int unknow_0;
            public int unknow_1;
            public int whitespace;

            private byte[] getValue(byte[] src, int pos, int len)
            {
                byte[] destinationArray = new byte[len];
                Array.Copy(src, pos, destinationArray, 0, len);
                return destinationArray;
            }

            public void mappingProperty(byte[] bytes, string name)
            {
                string[] collection = new string[] { "system", "cache", "userdata" };
                new HashSet<string>(collection);
                this.unknow_0 = BitConverter.ToInt32(this.getValue(bytes, 0, 4), 0);
                this.offset = BitConverter.ToInt32(this.getValue(bytes, 4, 4), 0);
                this.length = BitConverter.ToInt32(this.getValue(bytes, 8, 4), 0);
                this.unknow_1 = BitConverter.ToInt32(this.getValue(bytes, 12, 4), 0);
                this.whitespace = 0;
                this.name = name + ".bin";
                if (Regex.IsMatch(name, "^(system|cache|userdata)"))
                {
                    this.name = name + "_" + this.offset.ToString() + ".bin";
                }
            }
        }

        public delegate void ProgressChangeDelegate(object sender, Form1.ProgressEventArgs e);

        public class ProgressEventArgs : EventArgs
        {
            private int length;
            private int pos;
            private string title;

            public ProgressEventArgs(string title, int pos, int length)
            {
                this.title = title;
                this.pos = pos;
                this.length = length;
            }

            public void setValues(string title, int pos, int length)
            {
                this.title = title;
                this.pos = pos;
                this.length = length;
            }

            public int Length
            {
                get
                {
                    return this.length;
                }
            }

            public int Pos
            {
                get
                {
                    return this.pos;
                }
            }

            public string Title
            {
                get
                {
                    return this.title;
                }
            }
        }

        public class SystemBinMergingBackprocess
        {
            private static Semaphore _pool = new Semaphore(3, 3);
            private Form1.ProgressEventArgs args;
            private bool Cancel;
            private string full_filepath;
            private Form1.msgEventArgs margs;
            private string merged_systemfile_path = "merge_output";
            private string output_filename = "system.img";
            private string outputdir;
            private Form1 parent;
            private string workingdir;

            public event Form1.CopyErrorDelegate onCopyError;

            public event Form1.ProgressChangeDelegate onProgressChange;

            public event Form1.Completedelegate onProgressComplete;

            public SystemBinMergingBackprocess(object parent, string workingdir)
            {
                this.workingdir = workingdir;
                this.parent = (Form1) parent;
                this.outputdir = Path.Combine(this.workingdir, this.merged_systemfile_path);
                this.full_filepath = Path.Combine(this.outputdir, this.output_filename);
            }

            public void CancelProcess()
            {
                _pool.WaitOne();
                this.Cancel = true;
                _pool.Release();
            }

            protected virtual void CopyError(Form1.msgEventArgs e)
            {
                Form1.CopyErrorDelegate onCopyError = this.onCopyError;
                if (onCopyError != null)
                {
                    foreach (Form1.CopyErrorDelegate delegate3 in onCopyError.GetInvocationList())
                    {
                        ISynchronizeInvoke target = delegate3.Target as ISynchronizeInvoke;
                        try
                        {
                            if ((target != null) && target.InvokeRequired)
                            {
                                target.Invoke(delegate3, new object[] { this, e });
                            }
                            else
                            {
                                delegate3.DynamicInvoke(new object[] { this, e });
                            }
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine("MSG]-> CopyError " + exception.Message.ToString());
                        }
                    }
                }
            }

            private void CopyStream(Stream src, Stream dest, string filename)
            {
                long num = 0x9c4000L;
                int num2 = 0xc8000;
                int num3 = 0x2000;
                this.args = new Form1.ProgressEventArgs(filename, (int) src.Position, (int) src.Length);
                if (src.Length > num)
                {
                    num3 = num2;
                }
                byte[] buffer = new byte[num3];
                int count = src.Read(buffer, 0, buffer.Length);
                while (count > 0)
                {
                    if (this.Cancel)
                    {
                        break;
                    }
                    dest.Write(buffer, 0, count);
                    count = src.Read(buffer, 0, buffer.Length);
                    this.args.setValues(filename, (int) src.Position, (int) src.Length);
                    this.ProgressChange(this.args);
                }
                this.args.setValues(filename, (int) src.Length, (int) src.Length);
                this.ProgressChange(this.args);
                dest.Flush();
            }

            public void do_merge_systembin()
            {
                this.merge_systembin();
            }

            private IEnumerable<packfilelist> findSystemBins()
            {
                string workingdir = this.workingdir;
                if (!Directory.Exists(this.outputdir))
                {
                    Directory.CreateDirectory(this.outputdir);
                }
                if (Directory.Exists(this.outputdir))
                {
                    IOrderedEnumerable<Form1.SystemBinMergingBackprocess.packfilelist> source = from file in Directory.GetFiles(workingdir, "*.bin", SearchOption.TopDirectoryOnly)
                        where Path.GetFileName(file).Contains("system_")
                        select new Form1.SystemBinMergingBackprocess.packfilelist { File = file, FileName = Path.GetFileName(file), Id_file = Convert.ToInt32(Path.GetFileName(file).Replace("system_", "").Replace(".bin", "")) } into c
                        orderby c.Id_file
                        select c;
                    foreach (Form1.SystemBinMergingBackprocess.packfilelist packfilelist in source)
                    {
                        this.margs = new Form1.msgEventArgs(string.Concat(new object[] { " files     ||--> ", packfilelist.FileName, " ||  ", packfilelist.Id_file }));
                        this.CopyError(this.margs);
                    }
                    this.margs = new Form1.msgEventArgs(" count " + source.Count<Form1.SystemBinMergingBackprocess.packfilelist>().ToString());
                    this.CopyError(this.margs);
                    return source;
                }
                this.margs = new Form1.msgEventArgs(" failed on creating " + this.outputdir + " Folder ");
                this.CopyError(this.margs);
                return null;
            }

            private void merge_systembin()
            {
                string filepath = this.full_filepath;
                FileStream src = null;
                FileStream dest = null;
                long num = 0L;
                int num2 = 0;
                long num3 = 0L;
                long num4 = 0x200L;
                long offset = 0L;
                int num6 = 0;
                try
                {
                    IEnumerable<packfilelist> source = this.findSystemBins();
                    if (source.Count<packfilelist>() <= 0)
                    {
                        this.margs = new Form1.msgEventArgs(" Exception on findSystemBins cannot find system*.bin files / fail to create folder ");
                        this.CopyError(this.margs);
                        return;
                    }
                    this.removeAnyExistingFile(filepath);
                    source.Count<packfilelist>();
                    num2 = source.First<packfilelist>().Id_file;
                    num = source.Last<packfilelist>().Id_file;
                    num *= num4;
                    using (dest = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        dest.SetLength(num);
                        foreach (packfilelist packfilelist2 in source)
                        {
                            if (this.Cancel)
                            {
                                goto Label_01B8;
                            }
                            num3 = packfilelist2.Id_file;
                            num3 -= num2;
                            offset = num3 * num4;
                            this.margs = new Form1.msgEventArgs(" [+] partial " + num6.ToString() + " file " + packfilelist2.Id_file.ToString() + " offset " + num3.ToString() + " || seek " + offset.ToString());
                            this.CopyError(this.margs);
                            if (src != null)
                            {
                                src.Close();
                            }
                            src = new FileStream(packfilelist2.File, FileMode.Open);
                            dest.Seek(offset, SeekOrigin.Begin);
                            this.CopyStream(src, dest, packfilelist2.FileName);
                            num6++;
                        }
                    }
                Label_01B8:
                    if (this.Cancel)
                    {
                        this.margs = new Form1.msgEventArgs(" Abort merge_systembin !! ");
                        this.CopyError(this.margs);
                    }
                }
                catch (Exception exception)
                {
                    this.margs = new Form1.msgEventArgs(" Exception on merge_systembin " + exception.ToString());
                    this.CopyError(this.margs);
                }
                finally
                {
                    if (src != null)
                    {
                        src.Close();
                    }
                    if (dest != null)
                    {
                        dest.Close();
                    }
                    if (!this.Cancel)
                    {
                        this.margs = new Form1.msgEventArgs(" Complete on merging system files ");
                        this.CopyError(this.margs);
                        this.margs = new Form1.msgEventArgs(" SYSTEM IMAGE FILE LOCATE IN " + this.full_filepath);
                        this.CopyError(this.margs);
                    }
                    this.margs = new Form1.msgEventArgs(" Progress merge_systembin Done !! ");
                    this.ProgressComplete(this.margs);
                }
            }

            protected virtual void ProgressChange(Form1.ProgressEventArgs e)
            {
                Form1.ProgressChangeDelegate onProgressChange = this.onProgressChange;
                if (onProgressChange != null)
                {
                    foreach (Form1.ProgressChangeDelegate delegate3 in onProgressChange.GetInvocationList())
                    {
                        ISynchronizeInvoke target = delegate3.Target as ISynchronizeInvoke;
                        try
                        {
                            if ((target != null) && target.InvokeRequired)
                            {
                                target.Invoke(delegate3, new object[] { this, e });
                            }
                            else
                            {
                                delegate3.DynamicInvoke(new object[] { this, e });
                            }
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine("MSG]-> ProgressChange " + exception.Message.ToString());
                        }
                    }
                }
            }

            protected virtual void ProgressComplete(Form1.msgEventArgs e)
            {
                Form1.Completedelegate onProgressComplete = this.onProgressComplete;
                if (onProgressComplete != null)
                {
                    foreach (Form1.Completedelegate completedelegate2 in onProgressComplete.GetInvocationList())
                    {
                        ISynchronizeInvoke target = completedelegate2.Target as ISynchronizeInvoke;
                        try
                        {
                            if ((target != null) && target.InvokeRequired)
                            {
                                target.Invoke(completedelegate2, new object[] { this, e });
                            }
                            else
                            {
                                completedelegate2.DynamicInvoke(new object[] { this, e });
                            }
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine("MSG]-> ProgressComplete " + exception.Message.ToString());
                        }
                    }
                }
            }

            private bool removeAnyExistingFile(string filepath)
            {
                bool flag = true;
                try
                {
                    if (File.Exists(filepath))
                    {
                        File.Delete(filepath);
                    }
                }
                catch (Exception exception)
                {
                    flag = false;
                    this.margs = new Form1.msgEventArgs(" Exception on extract partition file " + exception.ToString());
                    this.CopyError(this.margs);
                }
                return flag;
            }

            private class packfilelist
            {
                public string File { get; set; }

                public string FileName { get; set; }

                public int Id_file { get; set; }
            }
        }

        public class TOTPartitionExtraction
        {
            private static Semaphore _pool = new Semaphore(3, 3);
            private bool Cancel;
            private ArrayList listContent;
            private Form1 parent;
            private string workingdir;

            public event Form1.CopyErrorDelegate onCopyError;

            public event Form1.ProgressChangeDelegate onProgressChange;

            public event Form1.Completedelegate onProgressComplete;

            public TOTPartitionExtraction(object parent, ArrayList al, string workingdir)
            {
                this.workingdir = workingdir;
                this.listContent = al;
                this.parent = (Form1) parent;
            }

            public void CancelProcess()
            {
                _pool.WaitOne();
                this.Cancel = true;
                _pool.Release();
            }

            protected virtual void CopyError(Form1.msgEventArgs e)
            {
                Form1.CopyErrorDelegate onCopyError = this.onCopyError;
                if (onCopyError != null)
                {
                    foreach (Form1.CopyErrorDelegate delegate3 in onCopyError.GetInvocationList())
                    {
                        ISynchronizeInvoke target = delegate3.Target as ISynchronizeInvoke;
                        try
                        {
                            if ((target != null) && target.InvokeRequired)
                            {
                                target.Invoke(delegate3, new object[] { this, e });
                            }
                            else
                            {
                                delegate3.DynamicInvoke(new object[] { this, e });
                            }
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine("MSG]-> CopyError " + exception.Message.ToString());
                        }
                    }
                }
            }

            public void do_extractTOTFlle()
            {
                this.extractPartitionParts(this.listContent);
            }

            public void extractPartitionParts(ArrayList contents)
            {
                Form1.msgEventArgs args2;
                FileStream input = null;
                FileStream stream2 = null;
                string workingdir = this.workingdir;
                int num = 0x3200000;
                int num2 = 0xc8000;
                int num3 = 0x2000;
                long offset = 0L;
                long num5 = 0L;
                int count = 0;
                long num7 = 0L;
                byte[] buffer2 = new byte[0x200];
                try
                {
                    input = new FileStream(this.parent.KDZFile, FileMode.Open);
                    BinaryReader reader = new BinaryReader(input);
                    for (int i = 0; i < buffer2.Length; i++)
                    {
                        buffer2[i] = 0;
                    }
                    foreach (Form1.PartPartitionInfo info in contents)
                    {
                        num7 = info.length * 0x200;
                        num5 = num7;
                        Form1.ProgressEventArgs e = new Form1.ProgressEventArgs(info.name, 0, (int) num5);
                        if (this.Cancel)
                        {
                            return;
                        }
                        num3 = 0x2000;
                        if (num5 > num)
                        {
                            num3 = num2;
                        }
                        string filepath = Path.Combine(workingdir, info.name);
                        if (!this.removeAnyExistingFile(filepath))
                        {
                            return;
                        }
                        offset = info.offset;
                        offset = Form1.GPTHeader_post + (offset * 0x200L);
                        reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                        args2 = new Form1.msgEventArgs(" [+] extract " + info.name + " file  size " + num5.ToString());
                        this.CopyError(args2);
                        using (stream2 = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            byte[] buffer = new byte[num3];
                            do
                            {
                                e.setValues(info.name, (int) stream2.Position, (int) num5);
                                this.ProgressChange(e);
                                count = input.Read(buffer, 0, buffer.Length);
                                stream2.Write(buffer, 0, count);
                            }
                            while ((stream2.Position + num3) <= num5);
                            long num9 = num5 - stream2.Position;
                            if (num9 > 0L)
                            {
                                buffer = new byte[(int) num9];
                                count = input.Read(buffer, 0, buffer.Length);
                                stream2.Write(buffer, 0, count);
                            }
                            e.setValues(info.name, (int) num5, (int) num5);
                            this.ProgressChange(e);
                            for (int j = 0; j < info.whitespace; j++)
                            {
                                stream2.Write(buffer2, 0, buffer2.Length);
                                if (this.Cancel)
                                {
                                    break;
                                }
                            }
                            if (this.Cancel)
                            {
                                return;
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    args2 = new Form1.msgEventArgs(" Exception on extract TOT partition file " + exception.ToString());
                    this.CopyError(args2);
                }
                finally
                {
                    if (input != null)
                    {
                        input.Close();
                    }
                    if (stream2 != null)
                    {
                        stream2.Close();
                    }
                    if (!this.Cancel)
                    {
                        args2 = new Form1.msgEventArgs(" Complete on extract TOT partition files ");
                        this.CopyError(args2);
                    }
                    args2 = new Form1.msgEventArgs(" Progress extractTOTFlle Done !! ");
                    this.ProgressComplete(args2);
                }
            }

            protected virtual void ProgressChange(Form1.ProgressEventArgs e)
            {
                Form1.ProgressChangeDelegate onProgressChange = this.onProgressChange;
                if (onProgressChange != null)
                {
                    foreach (Form1.ProgressChangeDelegate delegate3 in onProgressChange.GetInvocationList())
                    {
                        ISynchronizeInvoke target = delegate3.Target as ISynchronizeInvoke;
                        try
                        {
                            if ((target != null) && target.InvokeRequired)
                            {
                                target.Invoke(delegate3, new object[] { this, e });
                            }
                            else
                            {
                                delegate3.DynamicInvoke(new object[] { this, e });
                            }
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine("MSG]-> ProgressChange " + exception.Message.ToString());
                        }
                    }
                }
            }

            protected virtual void ProgressComplete(Form1.msgEventArgs e)
            {
                Form1.Completedelegate onProgressComplete = this.onProgressComplete;
                if (onProgressComplete != null)
                {
                    foreach (Form1.Completedelegate completedelegate2 in onProgressComplete.GetInvocationList())
                    {
                        ISynchronizeInvoke target = completedelegate2.Target as ISynchronizeInvoke;
                        try
                        {
                            if ((target != null) && target.InvokeRequired)
                            {
                                target.Invoke(completedelegate2, new object[] { this, e });
                            }
                            else
                            {
                                completedelegate2.DynamicInvoke(new object[] { this, e });
                            }
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine("MSG]-> ProgressComplete " + exception.Message.ToString());
                        }
                    }
                }
            }

            private bool removeAnyExistingFile(string filepath)
            {
                bool flag = true;
                try
                {
                    if (File.Exists(filepath))
                    {
                        File.Delete(filepath);
                    }
                }
                catch (Exception exception)
                {
                    flag = false;
                    Form1.msgEventArgs e = new Form1.msgEventArgs(" Exception on extract partition file " + exception.ToString());
                    this.CopyError(e);
                }
                return flag;
            }
        }

        public class TOTPartitionMergeBackprocess
        {
            private static Semaphore _pool = new Semaphore(3, 3);
            private Form1.ProgressEventArgs args;
            private bool Cancel;
            private string full_filepath;
            private Form1.msgEventArgs margs;
            private string merged_systemfile_path = "merge_output";
            private string outputdir;
            private Form1 parent;
            private string partitionName = "";
            private string workingdir;

            public event Form1.CopyErrorDelegate onCopyError;

            public event Form1.ProgressChangeDelegate onProgressChange;

            public event Form1.Completedelegate onProgressComplete;

            public TOTPartitionMergeBackprocess(object parent, string workingdir, string partitionname)
            {
                this.workingdir = workingdir;
                this.parent = (Form1) parent;
                this.partitionName = partitionname;
                this.outputdir = Path.Combine(this.workingdir, this.merged_systemfile_path);
                this.full_filepath = Path.Combine(this.outputdir, this.partitionName + ".img");
            }

            public void CancelProcess()
            {
                _pool.WaitOne();
                this.Cancel = true;
                _pool.Release();
            }

            protected virtual void CopyError(Form1.msgEventArgs e)
            {
                Form1.CopyErrorDelegate onCopyError = this.onCopyError;
                if (onCopyError != null)
                {
                    foreach (Form1.CopyErrorDelegate delegate3 in onCopyError.GetInvocationList())
                    {
                        ISynchronizeInvoke target = delegate3.Target as ISynchronizeInvoke;
                        try
                        {
                            if ((target != null) && target.InvokeRequired)
                            {
                                target.Invoke(delegate3, new object[] { this, e });
                            }
                            else
                            {
                                delegate3.DynamicInvoke(new object[] { this, e });
                            }
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine("MSG]-> CopyError " + exception.Message.ToString());
                        }
                    }
                }
            }

            private void CopyStream(Stream src, Stream dest, string filename)
            {
                long num = 0x9c4000L;
                int num2 = 0xc8000;
                int num3 = 0x2000;
                this.args = new Form1.ProgressEventArgs(filename, (int) src.Position, (int) src.Length);
                if (src.Length > num)
                {
                    num3 = num2;
                }
                byte[] buffer = new byte[num3];
                int count = src.Read(buffer, 0, buffer.Length);
                while (count > 0)
                {
                    if (this.Cancel)
                    {
                        break;
                    }
                    dest.Write(buffer, 0, count);
                    count = src.Read(buffer, 0, buffer.Length);
                    this.args.setValues(filename, (int) src.Position, (int) src.Length);
                    this.ProgressChange(this.args);
                }
                this.args.setValues(filename, (int) src.Length, (int) src.Length);
                this.ProgressChange(this.args);
                dest.Flush();
            }

            public void do_merge_totpartition()
            {
                this.merge_totpartition();
            }

            private IEnumerable<packfilelist> findPartBins()
            {
                Func<string, bool> func = null;
                Func<string, Form1.TOTPartitionMergeBackprocess.packfilelist> func2 = null;
                string workingdir = this.workingdir;
                if (!Directory.Exists(this.outputdir))
                {
                    Directory.CreateDirectory(this.outputdir);
                }
                if (Directory.Exists(this.outputdir))
                {
                    if (func == null)
                    {
                        func = file => Path.GetFileName(file).Contains(this.partitionName + "_");
                    }
                    if (func2 == null)
                    {
                        func2 = file => new Form1.TOTPartitionMergeBackprocess.packfilelist { File = file, FileName = Path.GetFileName(file), Id_file = Convert.ToInt32(Path.GetFileName(file).Replace(this.partitionName + "_", "").Replace(".bin", "")) };
                    }
                    IOrderedEnumerable<Form1.TOTPartitionMergeBackprocess.packfilelist> source = from c in Enumerable.Select<string, Form1.TOTPartitionMergeBackprocess.packfilelist>(Enumerable.Where<string>(Directory.GetFiles(workingdir, "*.bin", SearchOption.TopDirectoryOnly), func), func2)
                        orderby c.Id_file
                        select c;
                    foreach (Form1.TOTPartitionMergeBackprocess.packfilelist packfilelist in source)
                    {
                        long length = new FileInfo(packfilelist.File).Length;
                        packfilelist.Id_file = length;
                        this.margs = new Form1.msgEventArgs(string.Concat(new object[] { " files     ||--> ", packfilelist.FileName, " ||  ", packfilelist.Id_file }));
                        this.CopyError(this.margs);
                    }
                    this.margs = new Form1.msgEventArgs(" count " + source.Count<Form1.TOTPartitionMergeBackprocess.packfilelist>().ToString());
                    this.CopyError(this.margs);
                    return source;
                }
                this.margs = new Form1.msgEventArgs(" failed on creating " + this.outputdir + " Folder ");
                this.CopyError(this.margs);
                return null;
            }

            public void merge_totpartition()
            {
                string filepath = this.full_filepath;
                FileStream src = null;
                FileStream dest = null;
                long num = 0L;
                int num2 = 0;
                try
                {
                    IEnumerable<Form1.TOTPartitionMergeBackprocess.packfilelist> source = this.findPartBins();
                    if (source.Count<Form1.TOTPartitionMergeBackprocess.packfilelist>() <= 0)
                    {
                        this.margs = new Form1.msgEventArgs(" Exception on find Part Bins cannot find " + this.partitionName + "*.bin files / fail to create folder ");
                        this.CopyError(this.margs);
                        return;
                    }
                    this.removeAnyExistingFile(filepath);
                    foreach (Form1.TOTPartitionMergeBackprocess.packfilelist packfilelist in source)
                    {
                        num += new FileInfo(packfilelist.File).Length;
                    }
                    source.Count<Form1.TOTPartitionMergeBackprocess.packfilelist>();
                    using (dest = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        dest.SetLength(num);
                        dest.Seek(0L, SeekOrigin.Begin);
                        foreach (Form1.TOTPartitionMergeBackprocess.packfilelist packfilelist2 in source)
                        {
                            if (this.Cancel)
                            {
                                goto Label_0162;
                            }
                            this.margs = new Form1.msgEventArgs(" [+] partial " + num2.ToString() + " file " + packfilelist2.FileName);
                            this.CopyError(this.margs);
                            if (src != null)
                            {
                                src.Close();
                            }
                            src = new FileStream(packfilelist2.File, FileMode.Open);
                            this.CopyStream(src, dest, packfilelist2.FileName);
                            num2++;
                        }
                    }
                Label_0162:
                    if (this.Cancel)
                    {
                        this.margs = new Form1.msgEventArgs(" Abort merge_systembin !! ");
                        this.CopyError(this.margs);
                    }
                }
                catch (Exception exception)
                {
                    this.margs = new Form1.msgEventArgs(" Exception on merge part files  " + exception.ToString());
                    this.CopyError(this.margs);
                }
                finally
                {
                    if (src != null)
                    {
                        src.Close();
                    }
                    if (dest != null)
                    {
                        dest.Close();
                    }
                    if (!this.Cancel)
                    {
                        this.margs = new Form1.msgEventArgs(" Complete on merging part files ");
                        this.CopyError(this.margs);
                        this.margs = new Form1.msgEventArgs(" IMAGE FILE LOCATE IN " + this.full_filepath);
                        this.CopyError(this.margs);
                    }
                    this.margs = new Form1.msgEventArgs(" Progress merge partition part Done !! ");
                    this.ProgressComplete(this.margs);
                }
            }

            protected virtual void ProgressChange(Form1.ProgressEventArgs e)
            {
                Form1.ProgressChangeDelegate onProgressChange = this.onProgressChange;
                if (onProgressChange != null)
                {
                    foreach (Form1.ProgressChangeDelegate delegate3 in onProgressChange.GetInvocationList())
                    {
                        ISynchronizeInvoke target = delegate3.Target as ISynchronizeInvoke;
                        try
                        {
                            if ((target != null) && target.InvokeRequired)
                            {
                                target.Invoke(delegate3, new object[] { this, e });
                            }
                            else
                            {
                                delegate3.DynamicInvoke(new object[] { this, e });
                            }
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine("MSG]-> ProgressChange " + exception.Message.ToString());
                        }
                    }
                }
            }

            protected virtual void ProgressComplete(Form1.msgEventArgs e)
            {
                Form1.Completedelegate onProgressComplete = this.onProgressComplete;
                if (onProgressComplete != null)
                {
                    foreach (Form1.Completedelegate completedelegate2 in onProgressComplete.GetInvocationList())
                    {
                        ISynchronizeInvoke target = completedelegate2.Target as ISynchronizeInvoke;
                        try
                        {
                            if ((target != null) && target.InvokeRequired)
                            {
                                target.Invoke(completedelegate2, new object[] { this, e });
                            }
                            else
                            {
                                completedelegate2.DynamicInvoke(new object[] { this, e });
                            }
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine("MSG]-> ProgressComplete " + exception.Message.ToString());
                        }
                    }
                }
            }

            private bool removeAnyExistingFile(string filepath)
            {
                bool flag = true;
                try
                {
                    if (File.Exists(filepath))
                    {
                        File.Delete(filepath);
                    }
                }
                catch (Exception exception)
                {
                    flag = false;
                    this.margs = new Form1.msgEventArgs(" Exception on extract partition file " + exception.ToString());
                    this.CopyError(this.margs);
                }
                return flag;
            }

            private class packfilelist
            {
                public string File { get; set; }

                public string FileName { get; set; }

                public long Id_file { get; set; }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}

