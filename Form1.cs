using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

//////////////////////////////////////////////////////////////////////////
//此处引用开发包的DLL
//所在目录：..\PMC400-TEST\PMC400-TEST\dll
//////////////////////////////////////////////////////////////////////////
using PMC_CS;
using System.Threading;

namespace PMC400_TEST
{
    public partial class Form1 : Form
    {
        /// <summary>声明一个PMC400控制器的对象类
        /// </summary>
        PMC400 pmc400;
        string s_ip = "192.168.1.101";
        string s_port = "80";
        /// <summary>用于刷新各轴数据的定时器，建议使用多线程的定时器以免程序卡顿
        /// </summary>
        private System.Threading.Timer showAxisInforTimer;

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>初始化类内所有参数
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void initialization_Click(object sender, EventArgs e)
        {
            pmc400 = new PMC400();//此处构造一个实例

            pmc400.Init();//使用前必须初始化pmc400类中的各个参数，默认4个轴

            ShowMessage("初始化完成");

            ConnectTest.Visible = true;
            //add by fyy 20230408
            SphericalImage.Visible = true;
            CenterBias.Visible = true;

            //add by fyy 20230406
            label3.Visible = true;
            label4.Visible = true;
            label24.Visible = true;
            AxesInformation.Visible = true;
            SetParameters.Visible = true;
            MoveControl.Visible = true;
            Save.Visible = true;
        }

        /// <summary>此功能仅在最新DLL中可用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectTest_Click(object sender, EventArgs e)
        {
            bool pingtest = pmc400.PingTest(s_ip) == "Ping succeed.";
            bool tocktest = false;
            if (pingtest)
            {
                tocktest = pmc400.ConnectTest(s_ip, s_port);
            }

            string temp1 = "Ping 测试.............." + ((pingtest) ? "√" : "X") + "\r\n";
            string temp2 = "控制器反馈............." + ((tocktest) ? "√" : "X") + "\r\n";
            string temp3 = (pingtest && tocktest) ? "测试成功" : "测试失败";

            ShowMessage(temp1 + temp2 + temp3);

            if (pingtest && tocktest)
            {
                Connect.Visible = true;
            }
        }

        /// <summary>光学系统球心像位置计算
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SphericalImage_Click(object sender, EventArgs e)
        {
            Form3 form3 = new Form3(); 
            form3.ShowDialog();
        }


        /// <summary>光学中心偏误差分析
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CenterBias_Click(object sender, EventArgs e)
        {
            bool pingtest = pmc400.PingTest(s_ip) == "Ping succeed.";
            bool tocktest = false;
            if (pingtest)
            {
                tocktest = pmc400.ConnectTest(s_ip, s_port);
            }

            string temp1 = "Ping 测试.............." + ((pingtest) ? "√" : "X") + "\r\n";
            string temp2 = "控制器反馈............." + ((tocktest) ? "√" : "X") + "\r\n";
            string temp3 = (pingtest && tocktest) ? "测试成功" : "测试失败";

            ShowMessage(temp1 + temp2 + temp3);

            if (pingtest && tocktest)
            {
                Connect.Visible = true;
            }
        }

        /// <summary>连接控制器
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Connect_Click(object sender, EventArgs e)
        {
            if (Connect.Text == "连接")
            {
                //默认IP地址"192.168.1.101"。端口号固定为80无法修改
                if (pmc400.Connect(s_ip, s_port))
                {
                    //此处用于检测连接是否持续稳定
                    while (pmc400.ConnectPercentValue < 100 && pmc400.IsConnected())
                    {
                        Application.DoEvents();
                        System.Threading.Thread.Sleep(50);
                    }

                    if (pmc400.IsConnected())
                    {
                        ShowMessage("连接成功");
                        label3.Visible = true;
                        label4.Visible = true;
                        label24.Visible = true;
                        AxesInformation.Visible = true;
                        SetParameters.Visible = true;
                        MoveControl.Visible = true;
                        Save.Visible = true;
                        //Set_Unrealtime_Infor();//在刷新之前主动设置一次非实时更新的数据（因为这些参数不在控制器中储存），请根据实际需求修改相关内容
                        SubscribeEvents();
                        showAxisInforTimer_Start();

                        Connect.Text = "断开连接";
                    }
                    else
                    {
                        ShowMessage("连接失败");
                    }
                }
                else if (Connect.Text == "断开连接")
                {
                    pmc400.Disconnect();
                    UnSubscribeEvents();
                    pmc400.Init();
                    label3.Visible = false;
                    label4.Visible = false;
                    label24.Visible = false;
                    AxesInformation.Visible = false;
                    SetParameters.Visible = false;
                    MoveControl.Visible = false;
                    Save.Visible = false;

                    Connect.Text = "连接";
                }
            }
        }

        #region 定时器-刷新各轴数据
        public void Set_Unrealtime_Infor()
        {
            //控制模式设置-必须设置，不然有可能造成上下位机模式不同
            pmc400.Set_Control_Mode(0, 0);//开环

            pmc400.Set_Control_Mode(1, 1);//闭环
            pmc400.Set_PID_ON_OFF(0, true);//打开PID，进入全闭环

            pmc400.Set_Control_Mode(2, 0);//开环
            pmc400.Set_Control_Mode(3, 0);//开环

            //移动模式-默认直线模式，非旋转台可忽略
            pmc400.LineMoveMode[0] = true;
            pmc400.LineMoveMode[1] = true;
            pmc400.LineMoveMode[2] = true;
            pmc400.LineMoveMode[3] = true;

            //限位电平触发模式-默认为0（低电平）。建议每次都设置
            pmc400.Set_Limit_TTL(0, 0);
            pmc400.Set_Limit_TTL(1, 0);
            pmc400.Set_Limit_TTL(2, 0);
            pmc400.Set_Limit_TTL(3, 0);

            //跟随误差极限，默认2000cts。建议每次都设置
            pmc400.FollowErrorLimit[0] = 2000;
            pmc400.FollowErrorLimit[1] = 2000;
            pmc400.FollowErrorLimit[2] = 2000;
            pmc400.FollowErrorLimit[3] = 2000;
        }

        private delegate void myDelegate_AxisInforTimer();
        /// <summary> 显示表格所选轴的当前信息
        /// </summary>
        public void showAxisInfomation()
        {
            try
            {
                if (this.CurrentPosX.InvokeRequired)
                {
                    myDelegate_AxisInforTimer md = new myDelegate_AxisInforTimer(this.showAxisInfomation);
                    this.Invoke(md, new object[] { });
                }
                else if (pmc400.IsConnected())
                {
                    string strTemp = "";
                    string strData = "";

                    #region 控制模式，直线运动，限位电平，跟随误差极限
                    //控制模式
                    int controlmode = 0;

                    controlmode = pmc400.Get_Control_Mode(0);
                    if (controlmode == 0)//开环
                    {
                        if (ControlModeX.Text != "开环")
                            ControlModeX.Text = "开环";
                    }
                    else if (controlmode == 1)//闭环
                    {
                        if (ControlModeX.Text != "闭环")
                            ControlModeX.Text = "闭环";
                    }
                    else if (controlmode == 2)//手杆
                    {
                        if (ControlModeX.Text != "手杆")
                            ControlModeX.Text = "手杆";
                    }
                    else if (controlmode == 3)//速度
                    {
                        if (ControlModeX.Text != "速度")
                            ControlModeX.Text = "速度";
                    }

                    controlmode = pmc400.Get_Control_Mode(1);
                    if (controlmode == 0)//开环
                    {
                        if (ControlModeY.Text != "开环")
                            ControlModeY.Text = "开环";
                    }
                    else if (controlmode == 1)//闭环
                    {
                        if (ControlModeY.Text != "闭环")
                            ControlModeY.Text = "闭环";
                    }
                    else if (controlmode == 2)//手杆
                    {
                        if (ControlModeY.Text != "手杆")
                            ControlModeY.Text = "手杆";
                    }
                    else if (controlmode == 3)//速度
                    {
                        if (ControlModeY.Text != "速度")
                            ControlModeY.Text = "速度";
                    }

                    controlmode = pmc400.Get_Control_Mode(2);
                    if (controlmode == 0)//开环
                    {
                        if (ControlModeZ.Text != "开环")
                            ControlModeZ.Text = "开环";
                    }
                    else if (controlmode == 1)//闭环
                    {
                        if (ControlModeZ.Text != "闭环")
                            ControlModeZ.Text = "闭环";
                    }
                    else if (controlmode == 2)//手杆
                    {
                        if (ControlModeZ.Text != "手杆")
                            ControlModeZ.Text = "手杆";
                    }
                    else if (controlmode == 3)//速度
                    {
                        if (ControlModeZ.Text != "速度")
                            ControlModeZ.Text = "速度";
                    }

                    controlmode = pmc400.Get_Control_Mode(3);
                    if (controlmode == 0)//开环
                    {
                        if (ControlModeU.Text != "开环")
                            ControlModeU.Text = "开环";
                    }
                    else if (controlmode == 1)//闭环
                    {
                        if (ControlModeU.Text != "闭环")
                            ControlModeU.Text = "闭环";
                    }
                    else if (controlmode == 2)//手杆
                    {
                        if (ControlModeU.Text != "手杆")
                            ControlModeU.Text = "手杆";
                    }
                    else if (controlmode == 3)//速度
                    {
                        if (ControlModeU.Text != "速度")
                            ControlModeU.Text = "速度";
                    }

                    //是否直线运动
                    strTemp = MoveModeX.Text;
                    strData = pmc400.LineMoveMode[0] ? "直线" : "旋转";
                    if (strTemp != strData)
                        MoveModeX.Text = strData;

                    strTemp = MoveModeY.Text;
                    strData = pmc400.LineMoveMode[1] ? "直线" : "旋转";
                    if (strTemp != strData)
                        MoveModeY.Text = strData;

                    strTemp = MoveModeZ.Text;
                    strData = pmc400.LineMoveMode[2] ? "直线" : "旋转";
                    if (strTemp != strData)
                        MoveModeZ.Text = strData;

                    strTemp = MoveModeU.Text;
                    strData = pmc400.LineMoveMode[3] ? "直线" : "旋转";
                    if (strTemp != strData)
                        MoveModeU.Text = strData;

                    //限位电平
                    strTemp = LimitTTLX.Text;
                    strData = pmc400.Limit_TTL_Low[0] ? "低电平" : "高电平";
                    if (strTemp != strData)
                        LimitTTLX.Text = strData;

                    strTemp = LimitTTLY.Text;
                    strData = pmc400.Limit_TTL_Low[1] ? "低电平" : "高电平";
                    if (strTemp != strData)
                        LimitTTLY.Text = strData;

                    strTemp = LimitTTLZ.Text;
                    strData = pmc400.Limit_TTL_Low[2] ? "低电平" : "高电平";
                    if (strTemp != strData)
                        LimitTTLZ.Text = strData;

                    strTemp = LimitTTLU.Text;
                    strData = pmc400.Limit_TTL_Low[3] ? "低电平" : "高电平";
                    if (strTemp != strData)
                        LimitTTLU.Text = strData;

                    //跟随误差极限
                    strTemp = FollowErrorLimitX.Text;
                    strData = pmc400.FollowErrorLimit[0].ToString();
                    if (strTemp != strData)
                        FollowErrorLimitX.Text = strData;

                    strTemp = FollowErrorLimitY.Text;
                    strData = pmc400.FollowErrorLimit[1].ToString();
                    if (strTemp != strData)
                        FollowErrorLimitY.Text = strData;

                    strTemp = FollowErrorLimitZ.Text;
                    strData = pmc400.FollowErrorLimit[2].ToString();
                    if (strTemp != strData)
                        FollowErrorLimitZ.Text = strData;

                    strTemp = FollowErrorLimitU.Text;
                    strData = pmc400.FollowErrorLimit[3].ToString();
                    if (strTemp != strData)
                        FollowErrorLimitU.Text = strData;
                    #endregion

                    #region 移动判断-此处是反逻辑：0代表运行，1代表停止
                    strTemp = IsMovingX.Text;
                    strData = (pmc400.Get_Is_Stopped(0)) == 0 ? "运行" : "停止";
                    if (strTemp != strData)
                        IsMovingX.Text = strData;

                    strTemp = IsMovingY.Text;
                    strData = (pmc400.Get_Is_Stopped(1)) == 0 ? "运行" : "停止";
                    if (strTemp != strData)
                        IsMovingY.Text = strData;

                    strTemp = IsMovingZ.Text;
                    strData = (pmc400.Get_Is_Stopped(2)) == 0 ? "运行" : "停止";
                    if (strTemp != strData)
                        IsMovingZ.Text = strData;

                    strTemp = IsMovingU.Text;
                    strData = (pmc400.Get_Is_Stopped(3)) == 0 ? "运行" : "停止";
                    if (strTemp != strData)
                        IsMovingU.Text = strData;

                    #endregion

                    #region 位置：跟随误差，当前脉冲，反馈脉冲

                    //跟随误差cts
                    if (pmc400.Get_Control_Mode(0) == 1)//建议此功能在闭环模式下使用
                    {
                        strTemp = FollowErrorX.Text;
                        strData = (pmc400.Get_FollowError(0)).ToString();
                        if (strTemp != strData && !pmc400.FollowErrorAlarm[0])
                            FollowErrorX.Text = strData;
                    }
                    else
                    {
                        if (FollowErrorX.Text != "—")
                            FollowErrorX.Text = "—";
                    }
                    if (pmc400.Get_Control_Mode(1) == 1)//建议此功能在闭环模式下使用
                    {
                        strTemp = FollowErrorY.Text;
                        strData = (pmc400.Get_FollowError(1)).ToString();
                        if (strTemp != strData && !pmc400.FollowErrorAlarm[1])
                            FollowErrorY.Text = strData;
                    }
                    else
                    {
                        if (FollowErrorY.Text != "—")
                            FollowErrorY.Text = "—";
                    }
                    if (pmc400.Get_Control_Mode(2) == 1)//建议此功能在闭环模式下使用
                    {
                        strTemp = FollowErrorZ.Text;
                        strData = (pmc400.Get_FollowError(2)).ToString();
                        if (strTemp != strData && !pmc400.FollowErrorAlarm[2])
                            FollowErrorZ.Text = strData;
                    }
                    else
                    {
                        if (FollowErrorZ.Text != "—")
                            FollowErrorZ.Text = "—";
                    }
                    if (pmc400.Get_Control_Mode(3) == 1)//建议此功能在闭环模式下使用
                    {
                        strTemp = FollowErrorU.Text;
                        strData = (pmc400.Get_FollowError(3)).ToString();
                        if (strTemp != strData && !pmc400.FollowErrorAlarm[3])
                            FollowErrorU.Text = strData;
                    }
                    else
                    {
                        if (FollowErrorU.Text != "—")
                            FollowErrorU.Text = "—";
                    }

                    //当前脉冲cts
                    strTemp = CurrentPosX.Text;
                    strData = pmc400.Get_CurrentPosition(0).ToString();
                    if (strTemp != strData)
                        CurrentPosX.Text = strData;

                    strTemp = CurrentPosY.Text;
                    strData = pmc400.Get_CurrentPosition(1).ToString();
                    if (strTemp != strData)
                        CurrentPosY.Text = strData;

                    strTemp = CurrentPosZ.Text;
                    strData = pmc400.Get_CurrentPosition(2).ToString();
                    if (strTemp != strData)
                        CurrentPosZ.Text = strData;

                    strTemp = CurrentPosU.Text;
                    strData = pmc400.Get_CurrentPosition(3).ToString();
                    if (strTemp != strData)
                        CurrentPosU.Text = strData;

                    //反馈脉冲cts
                    strTemp = PostionEncodeX.Text;
                    strData = pmc400.Get_PositionEncode(0).ToString();
                    if (strTemp != strData)
                        PostionEncodeX.Text = strData;

                    strTemp = PostionEncodeY.Text;
                    strData = pmc400.Get_PositionEncode(1).ToString();
                    if (strTemp != strData)
                        PostionEncodeY.Text = strData;

                    strTemp = PostionEncodeZ.Text;
                    strData = pmc400.Get_PositionEncode(2).ToString();
                    if (strTemp != strData)
                        PostionEncodeZ.Text = strData;

                    strTemp = PostionEncodeU.Text;
                    strData = pmc400.Get_PositionEncode(3).ToString();
                    if (strTemp != strData)
                        PostionEncodeU.Text = strData;

                    #endregion

                    #region 限位,零位,已回零标志
                    //左限位（负限位，近限位）
                    strTemp = LeftLimitX.Text;
                    if (pmc400.Limit_TTL_Low[0])
                    {
                        strData = (pmc400.Get_El_Left_Data(0) == 0).ToString();
                    }
                    else
                    {
                        strData = (pmc400.Get_El_Left_Data(0) == 1).ToString();
                    }
                    if (strTemp != strData)
                        LeftLimitX.Text = strData;

                    strTemp = LeftLimitY.Text;
                    if (pmc400.Limit_TTL_Low[1])
                    {
                        strData = (pmc400.Get_El_Left_Data(1) == 0).ToString();
                    }
                    else
                    {
                        strData = (pmc400.Get_El_Left_Data(1) == 1).ToString();
                    }
                    if (strTemp != strData)
                        LeftLimitY.Text = strData;

                    strTemp = LeftLimitZ.Text;
                    if (pmc400.Limit_TTL_Low[2])
                    {
                        strData = (pmc400.Get_El_Left_Data(2) == 0).ToString();
                    }
                    else
                    {
                        strData = (pmc400.Get_El_Left_Data(2) == 1).ToString();
                    }
                    if (strTemp != strData)
                        LeftLimitZ.Text = strData;

                    strTemp = LeftLimitU.Text;
                    if (pmc400.Limit_TTL_Low[3])
                    {
                        strData = (pmc400.Get_El_Left_Data(3) == 0).ToString();
                    }
                    else
                    {
                        strData = (pmc400.Get_El_Left_Data(3) == 1).ToString();
                    }
                    if (strTemp != strData)
                        LeftLimitU.Text = strData;

                    //右限位（正限位，远限位）
                    strTemp = RightLimitX.Text;
                    if (pmc400.Limit_TTL_Low[0])
                    {
                        strData = (pmc400.Get_El_Right_Data(0) == 0).ToString();
                    }
                    else
                    {
                        strData = (pmc400.Get_El_Right_Data(0) == 1).ToString();
                    }
                    if (strTemp != strData)
                        RightLimitX.Text = strData;

                    strTemp = RightLimitY.Text;
                    if (pmc400.Limit_TTL_Low[1])
                    {
                        strData = (pmc400.Get_El_Right_Data(1) == 0).ToString();
                    }
                    else
                    {
                        strData = (pmc400.Get_El_Right_Data(1) == 1).ToString();
                    }
                    if (strTemp != strData)
                        RightLimitY.Text = strData;

                    strTemp = RightLimitZ.Text;
                    if (pmc400.Limit_TTL_Low[2])
                    {
                        strData = (pmc400.Get_El_Right_Data(2) == 0).ToString();
                    }
                    else
                    {
                        strData = (pmc400.Get_El_Right_Data(2) == 1).ToString();
                    }
                    if (strTemp != strData)
                        RightLimitZ.Text = strData;

                    strTemp = RightLimitU.Text;
                    if (pmc400.Limit_TTL_Low[3])
                    {
                        strData = (pmc400.Get_El_Right_Data(3) == 0).ToString();
                    }
                    else
                    {
                        strData = (pmc400.Get_El_Right_Data(3) == 1).ToString();
                    }
                    if (strTemp != strData)
                        RightLimitU.Text = strData;


                    //零位
                    strTemp = HomeX.Text;
                    if (pmc400.Limit_TTL_Low[0])
                    {
                        strData = (pmc400.Get_Org_Data(0) == 0).ToString();
                    }
                    else
                    {
                        strData = (pmc400.Get_Org_Data(0) == 1).ToString();
                    }
                    if (strTemp != strData)
                        HomeX.Text = strData;

                    strTemp = HomeY.Text;
                    if (pmc400.Limit_TTL_Low[1])
                    {
                        strData = (pmc400.Get_Org_Data(1) == 0).ToString();
                    }
                    else
                    {
                        strData = (pmc400.Get_Org_Data(1) == 1).ToString();
                    }
                    if (strTemp != strData)
                        HomeY.Text = strData;

                    strTemp = HomeZ.Text;
                    if (pmc400.Limit_TTL_Low[2])
                    {
                        strData = (pmc400.Get_Org_Data(2) == 0).ToString();
                    }
                    else
                    {
                        strData = (pmc400.Get_Org_Data(2) == 1).ToString();
                    }
                    if (strTemp != strData)
                        HomeZ.Text = strData;

                    strTemp = HomeU.Text;
                    if (pmc400.Limit_TTL_Low[3])
                    {
                        strData = (pmc400.Get_Org_Data(3) == 0).ToString();
                    }
                    else
                    {
                        strData = (pmc400.Get_Org_Data(3) == 1).ToString();
                    }
                    if (strTemp != strData)
                        HomeU.Text = strData;

                    //已回零标志
                    strTemp = HomeCompleteX.Text;
                    strData = (pmc400.HomeComplete[0]).ToString();
                    if (strTemp != strData)
                        HomeCompleteX.Text = strData;

                    strTemp = HomeCompleteY.Text;
                    strData = (pmc400.HomeComplete[1]).ToString();
                    if (strTemp != strData)
                        HomeCompleteY.Text = strData;

                    strTemp = HomeCompleteZ.Text;
                    strData = (pmc400.HomeComplete[2]).ToString();
                    if (strTemp != strData)
                        HomeCompleteZ.Text = strData;

                    strTemp = HomeCompleteU.Text;
                    strData = (pmc400.HomeComplete[3]).ToString();
                    if (strTemp != strData)
                        HomeCompleteU.Text = strData;

                    #endregion

                    #region 速度(单位：脉冲/秒)
                    //初速度
                    strTemp = StartSpeedX.Text;
                    strData = pmc400.Get_StartSpeed(0).ToString();
                    if (strTemp != strData)
                        StartSpeedX.Text = strData;

                    strTemp = StartSpeedY.Text;
                    strData = pmc400.Get_StartSpeed(1).ToString();
                    if (strTemp != strData)
                        StartSpeedY.Text = strData;

                    strTemp = StartSpeedZ.Text;
                    strData = pmc400.Get_StartSpeed(2).ToString();
                    if (strTemp != strData)
                        StartSpeedZ.Text = strData;

                    strTemp = StartSpeedU.Text;
                    strData = pmc400.Get_StartSpeed(3).ToString();
                    if (strTemp != strData)
                        StartSpeedU.Text = strData;


                    //加速度
                    strTemp = AccSpeedX.Text;
                    strData = pmc400.Get_AccSpeed(0).ToString();
                    if (strTemp != strData)
                        AccSpeedX.Text = strData;

                    strTemp = AccSpeedY.Text;
                    strData = pmc400.Get_AccSpeed(1).ToString();
                    if (strTemp != strData)
                        AccSpeedY.Text = strData;

                    strTemp = AccSpeedZ.Text;
                    strData = pmc400.Get_AccSpeed(2).ToString();
                    if (strTemp != strData)
                        AccSpeedZ.Text = strData;

                    strTemp = AccSpeedU.Text;
                    strData = pmc400.Get_AccSpeed(3).ToString();
                    if (strTemp != strData)
                        AccSpeedU.Text = strData;

                    //终速度
                    strTemp = FinalSpeedX.Text;
                    strData = pmc400.Get_FinalSpeed(0).ToString();
                    if (strTemp != strData)
                        FinalSpeedX.Text = strData;

                    strTemp = FinalSpeedY.Text;
                    strData = pmc400.Get_FinalSpeed(1).ToString();
                    if (strTemp != strData)
                        FinalSpeedY.Text = strData;

                    strTemp = FinalSpeedZ.Text;
                    strData = pmc400.Get_FinalSpeed(2).ToString();
                    if (strTemp != strData)
                        FinalSpeedZ.Text = strData;

                    strTemp = FinalSpeedU.Text;
                    strData = pmc400.Get_FinalSpeed(3).ToString();
                    if (strTemp != strData)
                        FinalSpeedU.Text = strData;

                    #endregion

                    #region 参数设置：细分数，导程，转速比，分辨率
                    //细分数
                    strTemp = MicroStepsX.Text;
                    strData = pmc400.Get_MicroSteps(0).ToString();
                    if (strTemp != strData)
                        MicroStepsX.Text = strData;

                    strTemp = MicroStepsY.Text;
                    strData = pmc400.Get_MicroSteps(1).ToString();
                    if (strTemp != strData)
                        MicroStepsY.Text = strData;

                    strTemp = MicroStepsZ.Text;
                    strData = pmc400.Get_MicroSteps(2).ToString();
                    if (strTemp != strData)
                        MicroStepsZ.Text = strData;

                    strTemp = MicroStepsU.Text;
                    strData = pmc400.Get_MicroSteps(3).ToString();
                    if (strTemp != strData)
                        MicroStepsU.Text = strData;

                    //分辨率，单位：脉冲/毫米(cts/mm)
                    strTemp = ResolutionX.Text;
                    if (pmc400.Get_Control_Mode(0) == 1)//开环和闭环的分辨率是两种情况
                    {//闭环时
                        strData = pmc400.Get_Resolution_Ratio(0).ToString();
                    }
                    else
                    {
                        strData = pmc400.Get_Convert(0, pmc400.LineMoveMode[0]).ToString();
                    }
                    if (strTemp != strData)
                    {
                        ResolutionX.Text = strData;
                    }

                    strTemp = ResolutionY.Text;
                    if (pmc400.Get_Control_Mode(1) == 1)//开环和闭环的分辨率是两种情况
                    {//闭环时
                        strData = pmc400.Get_Resolution_Ratio(1).ToString();
                    }
                    else
                    {
                        strData = pmc400.Get_Convert(1, pmc400.LineMoveMode[1]).ToString();
                    }
                    if (strTemp != strData)
                    {
                        ResolutionY.Text = strData;
                    }

                    strTemp = ResolutionZ.Text;
                    if (pmc400.Get_Control_Mode(2) == 1)//开环和闭环的分辨率是两种情况
                    {//闭环时
                        strData = pmc400.Get_Resolution_Ratio(2).ToString();
                    }
                    else
                    {
                        strData = pmc400.Get_Convert(2, pmc400.LineMoveMode[2]).ToString();
                    }
                    if (strTemp != strData)
                    {
                        ResolutionZ.Text = strData;
                    }

                    strTemp = ResolutionU.Text;
                    if (pmc400.Get_Control_Mode(3) == 1)//开环和闭环的分辨率是两种情况
                    {//闭环时
                        strData = pmc400.Get_Resolution_Ratio(3).ToString();
                    }
                    else
                    {
                        strData = pmc400.Get_Convert(3, pmc400.LineMoveMode[3]).ToString();
                    }
                    if (strTemp != strData)
                    {
                        ResolutionU.Text = strData;
                    }

                    //导程，单位：mm
                    strTemp = ScrewLeedX.Text;
                    strData = (pmc400.Get_ScrewLead(0) / 1000000.0).ToString();
                    if (strTemp != strData)
                        ScrewLeedX.Text = strData;
                    strTemp = ScrewLeedY.Text;
                    strData = (pmc400.Get_ScrewLead(1) / 1000000.0).ToString();
                    if (strTemp != strData)
                        ScrewLeedY.Text = strData;
                    strTemp = ScrewLeedZ.Text;
                    strData = (pmc400.Get_ScrewLead(2) / 1000000.0).ToString();
                    if (strTemp != strData)
                        ScrewLeedZ.Text = strData;
                    strTemp = ScrewLeedU.Text;
                    strData = (pmc400.Get_ScrewLead(3) / 1000000.0).ToString();
                    if (strTemp != strData)
                        ScrewLeedU.Text = strData;


                    //导程，单位：mm
                    strTemp = SpeedRatioX.Text;
                    strData = (pmc400.Get_SpeedRatio(0) / 1000.0).ToString();
                    if (strTemp != strData)
                        SpeedRatioX.Text = strData;
                    strTemp = SpeedRatioY.Text;
                    strData = (pmc400.Get_SpeedRatio(1) / 1000.0).ToString();
                    if (strTemp != strData)
                        SpeedRatioY.Text = strData;
                    strTemp = SpeedRatioZ.Text;
                    strData = (pmc400.Get_SpeedRatio(2) / 1000.0).ToString();
                    if (strTemp != strData)
                        SpeedRatioZ.Text = strData;
                    strTemp = SpeedRatioU.Text;
                    strData = (pmc400.Get_SpeedRatio(3) / 1000.0).ToString();
                    if (strTemp != strData)
                        SpeedRatioU.Text = strData;
                    #endregion

                    #region 输入输出

                    //输入
                    UInt32 myIO = 0;
                    strTemp = Input.Text;
                    myIO = pmc400.Get_IO_IN();
                    strData = Convert.ToString(myIO, 2);
                    strData = strData.PadLeft(8, '0');
                    if (strTemp != strData)
                    {
                        Input.Text = strData;
                    }

                    //输出
                    strTemp = Output.Text;
                    myIO = pmc400.Get_IO_OUT();
                    strData = Convert.ToString(myIO, 2);
                    strData = strData.PadLeft(8, '0');
                    if (strTemp != strData)
                    {
                        Output.Text = strData;
                    }
                    #endregion

                }

            }
            catch (ObjectDisposedException)
            {
                return;
            }
        }

        bool showAxisInforTimerInUse = false;
        /// <summary> 刷新信息定时器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showAxisInforTimer_Tick(object sender)
        {
            if (showAxisInforTimerInUse)//防止重复调用showAxisInfomation()
                return;

            showAxisInforTimerInUse = true;

            if (pmc400.IsConnected())
            {
                showAxisInfomation();
            }

            showAxisInforTimerInUse = false;
        }
        /// <summary> 打开轴信息计时器
        /// </summary>
        public void showAxisInforTimer_Start()
        {
            showAxisInforTimer = new System.Threading.Timer(new TimerCallback(showAxisInforTimer_Tick), this, 10, 20);
        }
        /// <summary> 关闭轴信息计时器
        /// </summary>
        public void showAxisInforTimer_Stop()
        {
            if (showAxisInforTimer != null)
            {
                showAxisInforTimer.Dispose();
            }
        }
        #endregion

        #region 获取信息事件
        /// <summary>订阅PMC动态库中的各个事件</summary>
        public void SubscribeEvents()
        {
            pmc400.OnInforStringChanged += new PMC.InforStringChange(ShowInforString);
            pmc400.OnCommandStringChanged += new PMC.CommandStringChange(ShowCommandString);
        }
        /// <summary>取消订阅PMC动态库中的各个事件，建议在断连接后取消订阅</summary>
        public void UnSubscribeEvents()
        {
            pmc400.OnInforStringChanged -= new PMC.InforStringChange(ShowInforString);
            pmc400.OnCommandStringChanged -= new PMC.CommandStringChange(ShowCommandString);
        }

        private delegate void myDelegate_ShowInforString(string msg);
        public void ShowInforString(object sender, EventArgs e)
        {
            try
            {
                if (this.MessageBox1.InvokeRequired)
                {
                    myDelegate_ShowInforString md = new myDelegate_ShowInforString(this.ShowMessage);
                    this.Invoke(md, new object[] { pmc400.InforString });
                }
                else
                {
                    ShowMessage(pmc400.InforString);
                }
            }
            catch (ObjectDisposedException)
            {
                return;
            }
        }
        public void ShowCommandString(object sender, EventArgs e)
        {
            try
            {
                if (this.MessageBox1.InvokeRequired)
                {
                    myDelegate_ShowInforString md = new myDelegate_ShowInforString(this.ShowMessage);
                    this.Invoke(md, new object[] { pmc400.CommandString });
                }
                else
                {
                    ShowMessage(pmc400.CommandString);
                }
            }
            catch (ObjectDisposedException)
            {
                return;
            }
        }

        private void ShowMessage(string msg)
        {
            msg += "\r\n";
            MessageBox1.AppendText(msg);
        }
        #endregion

        #region 辅助

        /// <summary> 判断字符串是否是数字
        /// </summary>
        public bool IsNumber(string s)
        {
            try
            {
                Convert.ToDouble(s);
            }
            catch (System.Exception)
            {
                return false;
            }
            return true;
        }

        public bool IsInt32(string s)
        {
            try
            {
                Convert.ToInt32(s);
            }
            catch (System.Exception)
            {
                return false;
            }
            return true;
        }
        #endregion

        #region 设置
        private void ControlMode_Click(object sender, EventArgs e)
        {
            int Axis = comboBox1.SelectedIndex;
            switch (comboBox11.SelectedItem.ToString())
            {
                case "开环":
                    pmc400.Set_Control_Mode(Axis, 0);
                    break;
                case "闭环":
                    pmc400.Set_Control_Mode(Axis, 1);
                    pmc400.Set_PID_ON_OFF(Axis);
                    break;
                case "手杆":
                    pmc400.Set_Control_Mode(Axis, 2);
                    break;
                case "速度":
                    pmc400.Set_Control_Mode(Axis, 3);
                    break;
                default:
                    break;
            }
        }

        private void MoveMode_Click(object sender, EventArgs e)
        {
            int Axis = comboBox2.SelectedIndex;
            switch (comboBox12.SelectedItem.ToString())
            {
                case "直线":
                    pmc400.LineMoveMode[Axis] = true;
                    break;
                case "旋转":
                    pmc400.LineMoveMode[Axis] = false;
                    break;
                default:
                    break;
            }
        }

        private void LimitTTL_Click(object sender, EventArgs e)
        {
            int Axis = comboBox3.SelectedIndex;
            switch (comboBox13.SelectedItem.ToString())
            {
                case "低电平":
                    pmc400.Set_Limit_TTL(Axis, 0);
                    break;
                case "高电平":
                    pmc400.Set_Limit_TTL(Axis, 1);
                    break;
                default:
                    break;
            }
        }

        private void StartSpeed_Click(object sender, EventArgs e)
        {
            int Axis = comboBox4.SelectedIndex;
            if (IsNumber(textBox1.Text))
            {
                double temp = Convert.ToDouble(textBox1.Text);
                pmc400.Set_StartSpeed(Axis, temp);
            }
            else
            {
                ShowMessage("请输入数字");
            }
        }

        private void AccSpeed_Click(object sender, EventArgs e)
        {
            int Axis = comboBox5.SelectedIndex;
            if (IsNumber(textBox2.Text))
            {
                double temp = Convert.ToDouble(textBox2.Text);
                pmc400.Set_AccSpeed(Axis, temp);
            }
            else
            {
                ShowMessage("请输入数字");
            }
        }

        private void FinalSpeed_Click(object sender, EventArgs e)
        {
            int Axis = comboBox6.SelectedIndex;
            if (IsNumber(textBox3.Text))
            {
                double temp = Convert.ToDouble(textBox3.Text);
                pmc400.Set_FinalSpeed(Axis, temp);
            }
            else
            {
                ShowMessage("请输入数字");
            }
        }

        private void MicroSteps7_Click(object sender, EventArgs e)
        {
            int Axis = comboBox7.SelectedIndex;
            if (IsInt32(textBox4.Text))
            {
                int temp = Convert.ToInt32(textBox4.Text);
                pmc400.Set_MicroSteps(Axis, temp);
            }
            else
            {
                ShowMessage("请输入整数");
            }
        }

        /// <summary>丝杆导程仅在运动方式为直线时可用,旋转模式时请设置为0
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrewLeed_Click(object sender, EventArgs e)
        {//输入框单位为mm，函数设置时单位为nm
            int Axis = comboBox8.SelectedIndex;
            if (IsNumber(textBox5.Text))
            {
                double temp = Convert.ToDouble(textBox5.Text);
                pmc400.Set_ScrewLead(Axis, (int)(temp * 1000000));
            }
            else
            {
                ShowMessage("请输入数字");
            }
        }

        /// <summary>转速比仅在运动方式为旋转时可用，直线模式时请设置为0
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SpeedRatio_Click(object sender, EventArgs e)
        {//函数设置时请放大1000倍
            int Axis = comboBox9.SelectedIndex;
            if (IsNumber(textBox6.Text))
            {
                double temp = Convert.ToDouble(textBox6.Text);
                pmc400.Set_SpeedRatio(Axis, (int)(temp * 1000));
            }
            else
            {
                ShowMessage("请输入数字");
            }
        }

        private void Resolution_Click(object sender, EventArgs e)
        {
            int Axis = comboBox10.SelectedIndex;
            if (IsInt32(textBox7.Text))
            {
                int temp = Convert.ToInt32(textBox7.Text);
                pmc400.Set_Resolution_Ratio(Axis, temp);
            }
            else
            {
                ShowMessage("请输入整数");
            }
        }

        private void SetOutput_Click(object sender, EventArgs e)
        {
            if (textBox11 != null)
            {
                string strTemp = textBox11.Text;
                if (strTemp.Length == 8)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (strTemp.Substring(i, 1) != "0" && strTemp.Substring(i, 1) != "1")
                        {
                            ShowMessage("请输入二进制数");
                            return;
                        }
                    }
                }
                else
                {
                    ShowMessage("请输入8位二进制数");
                    return;
                }

                string strData = Convert.ToString(pmc400.Get_IO_OUT(), 2);
                strData = strData.PadLeft(8, '0');
                if (strTemp != strData && IsNumber(strTemp))
                {
                    int IO_OUT = Convert.ToInt32(strTemp, 2);
                    pmc400.Set_IO_OUT(IO_OUT);
                }
            }
            else
                textBox11.Text = "00000000";
        }
        
        private void FollowErrorLimit_Click(object sender, EventArgs e)
        {

            int Axis = comboBox14.SelectedIndex;
            if (IsInt32(textBox13.Text))
            {
                int temp = Convert.ToInt32(textBox13.Text);
                if (temp >= 0)
                    pmc400.FollowErrorLimit[Axis] = temp;
                else
                    ShowMessage("请输入非负整数");
            }
            else
            {
                ShowMessage("请输入整数");
            }
        }
        
        private void Save_Click(object sender, EventArgs e)
        {
            //参数设置保存
            pmc400.Save_Setup();
            //控制模式保存
            pmc400.Save_Control_Mode();
            //保存手杆阈值-相关设置及读取请参考“控制模式”
            //pmc400.Save_Threshold();
        }
        #endregion

        #region 运动控制
        private void MoveForewordX_Click(object sender, EventArgs e)
        {
            ChangeMoveButtonEnable(false);
            if (IsInt32(textBox8.Text))
            {
                int temp = Convert.ToInt32(textBox8.Text);
                //首先设置移动距离
                if (pmc400.Get_Control_Mode(0) == 0)
                {
                    pmc400.Set_OpenMoveDistance(0, temp);//开环移动
                }
                else if (pmc400.Get_Control_Mode(0) == 1)
                {
                    pmc400.Set_CloseMoveDistance(0, temp);//闭环移动
                    //此处注意，将exe同目录下生成的ControlerData.ini文件中的UseUpperCloseLoop=True改为UseUpperCloseLoop=False
                }
                //或者使用自动移动函数
                pmc400.Set_MoveDistance_Convert(0,2);
                //此处运动距离为2mm，当细分数，丝杆导程，分辨率设置正确时可使用此函数

                //其次发送运动开始指令
                pmc400.StartMove();
                //注：此指令为所有轴同时接受，若之前未设置移动距离则相应轴不移动
            }
            ChangeMoveButtonEnable(true);
        }

        private void MoveReverseX_Click(object sender, EventArgs e)
        {
            ChangeMoveButtonEnable(false);
            if (IsInt32(textBox8.Text))
            {
                int temp = Convert.ToInt32(textBox8.Text);
                //首先设置移动距离
                if (pmc400.Get_Control_Mode(0) == 0)
                {
                    pmc400.Set_OpenMoveDistance(0, -temp);//开环移动
                }
                else if (pmc400.Get_Control_Mode(0) == 1)
                {
                    pmc400.Set_CloseMoveDistance(0, -temp);//闭环移动
                    //此处注意，将exe同目录下生成的ControlerData.ini文件中的UseUpperCloseLoop=True改为UseUpperCloseLoop=False
                }
                //或者使用自动移动函数
                //pmc400.Set_MoveDistance_Convert(0,2);
                //此处运动距离为2mm，当细分数，丝杆导程，分辨率设置正确时可使用此函数

                //其次发送运动开始指令
                pmc400.StartMove();
                //注：此指令为所有轴同时接受，若之前未设置移动距离则相应轴不移动
            }
            ChangeMoveButtonEnable(true);
        }

        private void StopX_Click(object sender, EventArgs e)
        {
            pmc400.StopMove();
            //注：此指令为所有轴同时接受,发送后所有中同时停止移动
        }

        private void MoveForewordY_Click(object sender, EventArgs e)
        {
            ChangeMoveButtonEnable(false);
            if (IsInt32(textBox9.Text))
            {
                int temp = Convert.ToInt32(textBox9.Text);
                //首先设置移动距离
                if (pmc400.Get_Control_Mode(1) == 0)
                {
                    pmc400.Set_OpenMoveDistance(1, temp);//开环移动
                }
                else if (pmc400.Get_Control_Mode(1) == 1)
                {
                    pmc400.Set_CloseMoveDistance(1, temp);//闭环移动
                    //此处注意，将exe同目录下生成的ControlerData.ini文件中的UseUpperCloseLoop=True改为UseUpperCloseLoop=False
                }
                //或者使用自动移动函数
                //pmc400.Set_MoveDistance_Convert(1,2);
                //此处运动距离为2mm，当细分数，丝杆导程，分辨率设置正确时可使用此函数

                //其次发送运动开始指令
                pmc400.StartMove();
                //注：此指令为所有轴同时接受，若之前未设置移动距离则相应轴不移动
            }
            ChangeMoveButtonEnable(true);
        }

        private void MoveReverseY_Click(object sender, EventArgs e)
        {
            ChangeMoveButtonEnable(false);
            if (IsInt32(textBox9.Text))
            {
                int temp = Convert.ToInt32(textBox9.Text);
                //首先设置移动距离
                if (pmc400.Get_Control_Mode(1) == 0)
                {
                    pmc400.Set_OpenMoveDistance(1, -temp);//开环移动
                }
                else if (pmc400.Get_Control_Mode(1) == 1)
                {
                    pmc400.Set_CloseMoveDistance(1, -temp);//闭环移动
                    //此处注意，将exe同目录下生成的ControlerData.ini文件中的UseUpperCloseLoop=True改为UseUpperCloseLoop=False
                }
                //或者使用自动移动函数
                //pmc400.Set_MoveDistance_Convert(1,2);
                //此处运动距离为2mm，当细分数，丝杆导程，分辨率设置正确时可使用此函数

                //其次发送运动开始指令
                pmc400.StartMove();
                //注：此指令为所有轴同时接受，若之前未设置移动距离则相应轴不移动
            }
            ChangeMoveButtonEnable(true);
        }

        private void StopY_Click(object sender, EventArgs e)
        {
            pmc400.StopMove();
            //注：此指令为所有轴同时接受,发送后所有中同时停止移动
        }

        private void MoveForewordZ_Click(object sender, EventArgs e)
        {
            ChangeMoveButtonEnable(false);
            if (IsInt32(textBox10.Text))
            {
                int temp = Convert.ToInt32(textBox10.Text);
                //首先设置移动距离
                if (pmc400.Get_Control_Mode(2) == 0)
                {
                    pmc400.Set_OpenMoveDistance(2, temp);//开环移动
                }
                else if (pmc400.Get_Control_Mode(2) == 1)
                {
                    pmc400.Set_CloseMoveDistance(2, temp);//闭环移动
                    //此处注意，将exe同目录下生成的ControlerData.ini文件中的UseUpperCloseLoop=True改为UseUpperCloseLoop=False
                }
                //或者使用自动移动函数
                //pmc400.Set_MoveDistance_Convert(2,2);
                //此处运动距离为2mm，当细分数，丝杆导程，分辨率设置正确时可使用此函数

                //其次发送运动开始指令
                pmc400.StartMove();
                //注：此指令为所有轴同时接受，若之前未设置移动距离则相应轴不移动
            }
            ChangeMoveButtonEnable(true);
        }

        private void MoveReverseZ_Click(object sender, EventArgs e)
        {
            ChangeMoveButtonEnable(false);
            if (IsInt32(textBox10.Text))
            {
                int temp = Convert.ToInt32(textBox10.Text);
                //首先设置移动距离
                if (pmc400.Get_Control_Mode(2) == 0)
                {
                    pmc400.Set_OpenMoveDistance(2, -temp);//开环移动
                }
                else if (pmc400.Get_Control_Mode(2) == 1)
                {
                    pmc400.Set_CloseMoveDistance(2, -temp);//闭环移动
                    //此处注意，将exe同目录下生成的ControlerData.ini文件中的UseUpperCloseLoop=True改为UseUpperCloseLoop=False
                }
                //或者使用自动移动函数
                //pmc400.Set_MoveDistance_Convert(2,2);
                //此处运动距离为2mm，当细分数，丝杆导程，分辨率设置正确时可使用此函数

                //其次发送运动开始指令
                pmc400.StartMove();
                //注：此指令为所有轴同时接受，若之前未设置移动距离则相应轴不移动
            }
            ChangeMoveButtonEnable(true);
        }

        private void StopZ_Click(object sender, EventArgs e)
        {
            pmc400.StopMove();
            //注：此指令为所有轴同时接受,发送后所有中同时停止移动
        }

        private void MoveForewordU_Click(object sender, EventArgs e)
        {
            ChangeMoveButtonEnable(false);
            if (IsInt32(textBox12.Text))
            {
                int temp = Convert.ToInt32(textBox12.Text);
                //首先设置移动距离
                if (pmc400.Get_Control_Mode(3) == 0)
                {
                    pmc400.Set_OpenMoveDistance(3, temp);//开环移动
                }
                else if (pmc400.Get_Control_Mode(3) == 1)
                {
                    pmc400.Set_CloseMoveDistance(3, temp);//闭环移动
                    //此处注意，将exe同目录下生成的ControlerData.ini文件中的UseUpperCloseLoop=True改为UseUpperCloseLoop=False
                }
                //或者使用自动移动函数
                //pmc400.Set_MoveDistance_Convert(3,2);
                //此处运动距离为2mm，当细分数，丝杆导程，分辨率设置正确时可使用此函数

                //其次发送运动开始指令
                pmc400.StartMove();
                //注：此指令为所有轴同时接受，若之前未设置移动距离则相应轴不移动
            }
            ChangeMoveButtonEnable(true);
        }

        private void MoveReverseU_Click(object sender, EventArgs e)
        {
            ChangeMoveButtonEnable(false);
            if (IsInt32(textBox12.Text))
            {
                int temp = Convert.ToInt32(textBox12.Text);
                //首先设置移动距离
                if (pmc400.Get_Control_Mode(3) == 0)
                {
                    pmc400.Set_OpenMoveDistance(3, -temp);//开环移动
                }
                else if (pmc400.Get_Control_Mode(3) == 1)
                {
                    pmc400.Set_CloseMoveDistance(3, -temp);//闭环移动
                    //此处注意，将exe同目录下生成的ControlerData.ini文件中的UseUpperCloseLoop=True改为UseUpperCloseLoop=False
                }
                //或者使用自动移动函数
                //pmc400.Set_MoveDistance_Convert(2,2);
                //此处运动距离为2mm，当细分数，丝杆导程，分辨率设置正确时可使用此函数

                //其次发送运动开始指令
                pmc400.StartMove();
                //注：此指令为所有轴同时接受，若之前未设置移动距离则相应轴不移动
            }
            ChangeMoveButtonEnable(true);
        }

        private void StopU_Click(object sender, EventArgs e)
        {
            pmc400.StopMove();
            //注：此指令为所有轴同时接受,发送后所有中同时停止移动
        }

        private void ClearX_Click(object sender, EventArgs e)
        {
            //位置清零
            pmc400.Set_CurrentPosition(0, 0);
            //注：此处第二个参数不建议改为除0以外的其他数字
            //在闭环移动中不要按此按钮
        }

        private void ClearY_Click(object sender, EventArgs e)
        {
            //位置清零
            pmc400.Set_CurrentPosition(1, 0);
            //注：此处第二个参数不建议改为除0以外的其他数字
            //在闭环移动中不要按此按钮
        }

        private void ClearZ_Click(object sender, EventArgs e)
        {
            //位置清零
            pmc400.Set_CurrentPosition(2, 0);
            //注：此处第二个参数不建议改为除0以外的其他数字
            //在闭环移动中不要按此按钮
        }

        private void ClearU_Click(object sender, EventArgs e)
        {
            //位置清零
            pmc400.Set_CurrentPosition(3, 0);
            //注：此处第二个参数不建议改为除0以外的其他数字
            //在闭环移动中不要按此按钮
        }

        private void HomeButtonX_Click(object sender, EventArgs e)
        {
            ChangeMoveButtonEnable(false);
            //发送回零指令,等待回零结束
            pmc400.Set_HomeDirection(0, 2);
            pmc400.HomeBack(0);

            ChangeMoveButtonEnable(true);
        }

        private void HomeButtonY_Click(object sender, EventArgs e)
        {
            ChangeMoveButtonEnable(false);
            //发送回零指令,等待回零结束
            pmc400.Set_HomeDirection(1, 2);
            pmc400.HomeBack(1);

            ChangeMoveButtonEnable(true);
        }

        private void HomeButtonZ_Click(object sender, EventArgs e)
        {
            ChangeMoveButtonEnable(false);
            //发送回零指令,等待回零结束
            pmc400.Set_HomeDirection(2, 2);
            pmc400.HomeBack(2);

            ChangeMoveButtonEnable(true);
        }

        private void HomeButtonU_Click(object sender, EventArgs e)
        {
            ChangeMoveButtonEnable(false);
            //发送回零指令,等待回零结束
            pmc400.Set_HomeDirection(3, 2);
            pmc400.HomeBack(3);

            ChangeMoveButtonEnable(true);
        }

        private void ChangeMoveButtonEnable(bool enable)
        {
            MoveForewordX.Enabled = enable;
            MoveReverseX.Enabled = enable;
            MoveForewordY.Enabled = enable;
            MoveReverseY.Enabled = enable;
            MoveForewordZ.Enabled = enable;
            MoveReverseZ.Enabled = enable;
            MoveForewordU.Enabled = enable;
            MoveReverseU.Enabled = enable;
            ClearX.Enabled = enable;
            ClearY.Enabled = enable;
            ClearZ.Enabled = enable;
            ClearU.Enabled = enable;
            HomeButtonX.Enabled = enable;
            HomeButtonY.Enabled = enable;
            HomeButtonZ.Enabled = enable;
            HomeButtonU.Enabled = enable;
        }


        #endregion

        private void MoveControl_Paint(object sender, PaintEventArgs e)
        {

        }
    }

    #region PID相关
    /*
     * 由于PID参数修改繁琐，建议使用我方提供的软件修改并保存，则之后便不需修改
     * 若有需求请咨询销售或客服
     */
    #endregion

}
