using System;
using System.Drawing;
using System.IO;
using System.Media;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Server
{
    //定义委托类型
    //这些委托用来在子线程中更新界面
    public delegate void StepsChangeHandler(int changeValue);  //将步数设置到label上
    public delegate void HelpHandler(string helpOrColor);  //将帮助信息设置到label上        
    public delegate void ChessEventReceiveHander(object sender, Message e);  //消息事件委托。
    public delegate void PPHandler(Message e);  //匹配玩家后更新界面
    public delegate void PictureChooser(object tag);//选择表情后插入到richTextBox，窗体间传值
    public delegate void UpdatePicture();

    public partial class FormPVE : Form
    {
        //声明委托变量
        public static StepsChangeHandler delStep;
        public static HelpHandler delHelp;
        public static UpdatePicture updataPic;

        public static string myUUID = "";
        public static string yourUUID = "";
        private DateTime startTime;//记录游戏开始时间
        public string[] mName;//系统自带的自定义昵称
        Random r = new Random();
        ChessPanel chessPanel;


        /// <summary>
        /// 更新步数到界面上，如果传入0，则将步数清零
        /// </summary>
        /// <param name="changeValue">变化量</param>
        private void SetStepsOnLabel(int changeValue)
        {
            lblStep.Invoke(new Action(() =>
            {
                if (changeValue==0)
                {
                    lblStep.Text = "0";
                }
                else
                {
                    lblStep.Text = Convert.ToInt32(lblStep.Text) + changeValue + string.Empty;
                }
            }));
        }
        /// <summary>
        /// 更新帮助信息到界面
        /// </summary>
        /// <param name="helpString"></param>
        private void SetHelpOnLabel(string helpString)
        {
            lblHelp.Invoke(new Action(() =>
            {
                lblHelp.Text = helpString;
            }));
        }
        /// <summary>
        /// 窗体的构造函数
        /// </summary>
        public FormPVE()
        {
            InitializeComponent();
            chessPanel = new ChessPanel();
            this.splitContainer2.Panel1.Controls.Add(this.chessPanel);
            chessPanel.BringToFront();
            //chesspanel的设置
            this.chessPanel.BackColor = System.Drawing.Color.DarkOrange;
            this.chessPanel.BackgroundImage = global::Server.Resource.back;
            this.chessPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.chessPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chessPanel.Location = new System.Drawing.Point(0, 67);
            this.chessPanel.MinimumSize = new System.Drawing.Size(100, 100);
            timer1.Stop();

            updataPic = new UpdatePicture(UpDatePicture);
            delStep = new StepsChangeHandler(SetStepsOnLabel);
            delHelp = new HelpHandler(SetHelpOnLabel);

        }

        public void updataImage()
        {
            if (ChessPanel.ACurrentColors.Count == ChessPanel.AColorQ.Count)
            {
                for (int i = 0; i < ChessPanel.AColorQ.Count; i++)
                {
                    ChessPanel.ACurrentColors[i] = chessPanel .colors[ChessPanel.AColorQ[i]];
                }
            }

            if (ChessPanel.BCurrentColors.Count == ChessPanel.BColorQ.Count)
            {
                for (int i = 0; i < ChessPanel.BColorQ.Count; i++)
                {
                    ChessPanel.BCurrentColors[i] = chessPanel .colors[ChessPanel.BColorQ[i]];
                }
            }
        }
        private void UpDatePicture()
        {
            updataImage();
            /*
            pictureBox1.BackgroundImage = chessPanel.ACurrentColors[0];
            pictureBox2.BackgroundImage = chessPanel.ACurrentColors[1];
            pictureBox3.BackgroundImage = chessPanel.ACurrentColors[2];
            pictureBox4.BackgroundImage = chessPanel.BCurrentColors[0];
            pictureBox5.BackgroundImage = chessPanel.BCurrentColors[1];
            pictureBox6.BackgroundImage = chessPanel.BCurrentColors[2];
            */
            pictureBox1.Invoke(new Action(() =>
            {
                pictureBox1.BackgroundImage = ChessPanel.ACurrentColors[0];

            }));
            pictureBox2.Invoke(new Action(() =>
            {
                pictureBox2.BackgroundImage = ChessPanel.ACurrentColors[1];
            }));
            pictureBox3.Invoke(new Action(() =>
            {
                pictureBox3.BackgroundImage = ChessPanel.ACurrentColors[2];
            }));
            pictureBox4.Invoke(new Action(() =>
            {
                pictureBox4.BackgroundImage = ChessPanel.BCurrentColors[0];
            }));
            pictureBox5.Invoke(new Action(() =>
            {
                pictureBox5.BackgroundImage = ChessPanel.BCurrentColors[1];
            }));
            pictureBox6.Invoke(new Action(() =>
            {
                pictureBox6.BackgroundImage = ChessPanel.BCurrentColors[2];
            }));
        }
        
        /// <summary>
        /// 定时器每隔1秒更新界面上的计时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            //Console.WriteLine((DateTime.Now - startTime).ToString("hhmmss"));
            lblTime.Text = (DateTime.Now - startTime).ToString("m\\:ss");
        }
        /// <summary>
        /// 窗体加载的时候，随机设置昵称
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormMain_Load(object sender, EventArgs e)
        {
        }
        /// <summary>
        /// 设置棋盘纯色背景
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog dlg = new ColorDialog())
            {
                DialogResult dr = dlg.ShowDialog();
                if (dr==DialogResult.OK)
                {
                    chessPanel.BackColor = dlg.Color;
                    chessPanel.BackgroundImage = null;

                    chessPanel.Refresh();
                    chessPanel.ChessPanel_DoubleClick(sender, e);
                }
            }
        }

        /// <summary>
        /// 悔棋
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBack_Click(object sender, EventArgs e)
        {
            if (ChessPanel.mWhoseTurn==ChessPanel.mColor) //本方落子 对方可悔棋
            {
                return;
            }

        }
        /// <summary>
        /// 设置棋盘的图片背景
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPicBack_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg=new OpenFileDialog())
            {
                dlg.Filter = "位图图像|*.jpg;*.png;*.bmp";
                dlg.InitialDirectory = Application.StartupPath+"\\background\\";
                if (dlg.ShowDialog()==DialogResult.OK)
                {
                    chessPanel.BackgroundImage= Image.FromFile(dlg.FileName);
                    chessPanel.BackColor = Color.Transparent;

                    chessPanel.Refresh();
                    chessPanel.ChessPanel_DoubleClick(sender, e);
                }
                
            }
            
        }

        /// <summary>
        /// 重新开始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, EventArgs e)
        {
            lblHelp.Text= "等待对方确认";
        }

       

    }
}
