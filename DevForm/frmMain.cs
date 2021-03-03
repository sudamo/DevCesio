using System;
using System.Data;
using System.Drawing;
using System.Timers;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Configuration;

namespace DevCesio.DevForm
{
    using Model.Global;
    using Model.K3Cloud;
    using Model.Basic;
    using Model.XBT;
    using DALFactory.K3Cloud;

    /// <summary>
    /// FormMain
    /// </summary>
    public partial class frmMain : Form
    {
        #region Fields
        /// <summary>
        /// 时间周期
        /// </summary>
        private int _Period;
        /// <summary>
        /// 计数
        /// </summary>
        private int _Counter;
        /// <summary>
        /// 序号
        /// </summary>
        private int _Index;
        /// <summary>
        /// 文本内容
        /// </summary>
        private string _Context;
        /// <summary>
        /// 指令执行进度
        /// </summary>
        private string _Percent;
        /// <summary>
        /// 许可
        /// </summary>
        private string _Permit;
        /// <summary>
        /// ...集合
        /// </summary>
        private List<string> _lstDots;
        /// <summary>
        /// 日期设置
        /// </summary>
        private DateTime _SetDate;
        /// <summary>
        /// 定时器
        /// </summary>
        private System.Timers.Timer _Timer;
        /// <summary>
        /// 定时参数
        /// </summary>
        private TimerParameter _TimerPara;
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public frmMain()
        {
            InitializeComponent();
        }

        /// <summary>
        /// FormLoad
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmMain_Load(object sender, EventArgs e)
        {
            //配置信息验证
            if (!CheckConfig())
                Application.Exit();

            //初始化参数
            InitialSetting();
            //定时器设置
            TimerSetting();
        }

        #region ConfigSetting
        /// <summary>
        /// 配置检查
        /// </summary>
        /// <returns></returns>
        private bool CheckConfig()
        {
            string K3_Url, K3_ZTID, K3_User, K3_PWD, SQL_IP, SQL_Port, SQL_Catalog, SQL_User, SQL_PWD, T_PickSeconds;
            try
            {
                #region 配置信息
                //K3Inf
                K3_Url = ConfigurationManager.AppSettings["K3_URL"];
                K3_ZTID = ConfigurationManager.AppSettings["K3_ZTID"];
                K3_User = ConfigurationManager.AppSettings["K3_USERNAME"];
                K3_PWD = ConfigurationManager.AppSettings["K3_PWD"];
                //SQLInf
                SQL_IP = ConfigurationManager.AppSettings["SQL_IP"];
                SQL_Port = ConfigurationManager.AppSettings["SQL_Port"];
                SQL_Catalog = ConfigurationManager.AppSettings["SQL_Catalog"];
                SQL_User = ConfigurationManager.AppSettings["SQL_User"];
                SQL_PWD = ConfigurationManager.AppSettings["SQL_PWD"];
                T_PickSeconds = ConfigurationManager.AppSettings["T_PickSeconds"];
                //parameters
                _Permit = ConfigurationManager.AppSettings["ClientSettingsProvider.ServiceUri"];
                #endregion
            }
            catch(Exception ex)
            {
                MessageBox.Show("配置信息有误:" + ex.Message);
                return false;
            }

            #region 设置信息
            //全局参数
            new GlobalParameter(new K3Setting(K3_Url, K3_ZTID, K3_User, K3_PWD), new SQLConfig(SQL_IP, SQL_Port, SQL_User, SQL_PWD, SQL_Catalog));
            #endregion

            #region 调用服务方法验证用户登录
            string log = DalCreator.CommFunction.Userlog(K3_Url, K3_ZTID, K3_User, K3_PWD);
            if (log != "")
            {
                MessageBox.Show(log);
                return false;
            }
            #endregion

            #region 数据库连接检验
            string strDBStatus = DalCreator.CommFunction.CheckedConnection(GlobalParameter.SQLInf.ConnectionString);
            if (strDBStatus != "连接成功")
            {
                MessageBox.Show(strDBStatus);
                return false;
            }
            #endregion

            _TimerPara = new TimerParameter(0, int.Parse(T_PickSeconds), 0, false, true, "");

            #region 判断程序是否启用
            //string strFoun = DalCreator.CommFunction.CheckFoun("CesioK3");//-----------------            
            //else if (strFoun != string.Empty)
            //{
            //    MessageBox.Show(strFoun);
            //    return false;
            //}
            _Period = 44262;
            //_SetDate = DateTime.Parse("1900-01-01").AddDays(_Period);
            _SetDate = DalCreator.CommFunction.GetDateTime().AddDays(_Period);
            if (!CheckDate())
                return false;
            #endregion

            #region 判断MAC是否已注册使用 User <> Administrator
            #endregion

            #region 批处理脚本
            DalCreator.CommFunction.SetSql();
            #endregion

            return true;
        }
        /// <summary>
        /// 默认值设置
        /// </summary>
        private void InitialSetting()
        {
            GlobalParameter.IsPause = false;
            _Counter = 0;
            _Percent = string.Empty;
            _lstDots = new List<string>();

            for (int i = 0; i < 15; i++)
            {
                string dos = "";
                for (int j = 0; j < i + 1; j++)
                {
                    dos += ".";
                }
                _lstDots.Add(dos);
            }

            Text = string.Format("金蝶云星空 仓库条码管理软件专用 当前数据库[{0}]", GlobalParameter.SQLInf.Catalog);            
            _Context = "\r      条码系统在工作中，必须启动本软件\n\r\n产品名称：金蝶K3云星空条码系统数据管理软件\n\r版权所有：广州馨宝信息技术有限公司\n\r             www.cesio.cn\n          版本号：V2020.0011";
            rtbContext.Text = _Context;
            rtbContext.Font = new Font(rtbContext.Font.FontFamily, 12, rtbContext.Font.Style);
        }
        /// <summary>
        /// 检查日期
        /// </summary>
        /// <returns></returns>
        private bool CheckDate()
        {
            if (_Permit.Equals(string.Empty))
            if (_SetDate < DateTime.Now)
            {
                MessageBox.Show("试用期已过");
                GlobalParameter.IsPause = true;
                return false;
            }
            return true;
        }
        /// <summary>
        /// 定时器设置
        /// </summary>
        private void TimerSetting()
        {
            timerShow.Enabled = true;
            timerShow.Interval = 1000;
            timerShow.Start();

            //方法自动执行
            _Timer = new System.Timers.Timer();
            _Timer.Enabled = true;
            _Timer.Interval = 1000 * _TimerPara.PickSeconds;//每PickSeconds秒执行一次
            _Timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            _Timer.Start();
        }
        #endregion

        #region Events
        /// <summary>
        /// 定时任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (GlobalParameter.IsPause)
                return;
            if (!CheckDate())
                return;
            try
            {
                GlobalParameter.IsPause = true;//执行指令中，暂停定时器
                _Percent = "0 %";

                DataTable dt = DalCreator.CommFunction.GetAllOrders();
                if (dt == null || dt.Rows.Count == 0)
                    return;

                UpOrderInfo entity;
                for (int i = 0; i < dt.Rows.Count; i++)//循环执行所有指令
                {
                    //if (i == 0) _Percent = "0 %";

                    entity = new UpOrderInfo();
                    entity.Fscjqh = dt.Rows[i]["fscjqh"].ToString();
                    entity.Fslxbs = dt.Rows[i]["fslxbs"].ToString();
                    entity.Fsuser = dt.Rows[i]["fsuser"].ToString();
                    entity.Fspwd = dt.Rows[i]["fspwd"].ToString();

                    DalCreator.CommFunction.ExecuteApiByOrder(entity);//执行一条指令
                    _Percent = string.Format("{0} %", (i + 1) * 100 / dt.Rows.Count);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                GlobalParameter.IsPause = false;//启动定时器
            }
        }
        /// <summary>
        /// 显示运行时间和状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trShow_Tick(object sender, EventArgs e)
        {
            _TimerPara.RunSeconds++;

            int iDay = _TimerPara.RunSeconds / 86400;

            lblDayTime.Text = string.Format("{0:d} 天 {1:d2}:{2:d2}:{3:d2}", _TimerPara.RunSeconds / 86400, _TimerPara.RunSeconds / 3600 % 24, _TimerPara.RunSeconds / 60 % 60, _TimerPara.RunSeconds % 60);

            if (GlobalParameter.IsPause)
            {
                _Counter++;
                _Index = _Counter % 15;
                lblRuning.Text = "指令执行中(" + _Percent + ")" + _lstDots[_Index];
            }
            else
            {
                lblRuning.Text = "未检测到新的指令";
                _Counter = 0;
            }
        }
        #endregion
    }
}
