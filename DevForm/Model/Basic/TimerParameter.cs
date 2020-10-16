using System;

namespace DevCesio.DevForm.Model.Basic
{
    /// <summary>
    /// 定时器参数实体
    /// </summary>
    public class TimerParameter
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public TimerParameter() { }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="pExeTimes">已经执行次数</param>
        /// <param name="pPickSeconds">间隔时间(秒)</param>
        /// <param name="pRunSeconds">运行时间</param>
        /// <param name="pPauseStatus">是否暂停</param>
        /// <param name="pIsRunning">是否运行中</param>
        /// <param name="pFuncID">执行方法名ID</param>
        public TimerParameter(int pExeTimes, int pPickSeconds, int pRunSeconds, bool pPauseStatus, bool pIsRunning, string pFuncID)
        {
            _ExeTimes = pExeTimes;
            _PickSeconds = pPickSeconds;
            _RunSeconds = pRunSeconds;
            _PauseStatus = pPauseStatus;
            _IsRunning = pIsRunning;
            _FuncID = pFuncID;
        }

        private int _ExeTimes;
        private int _PickSeconds;
        private int _RunSeconds;
        private bool _PauseStatus;
        private bool _IsRunning;
        private string _FuncID;

        /// <summary>
        /// 已经执行次数
        /// </summary>
        public int ExeTimes
        {
            get
            {
                return _ExeTimes;
            }

            set
            {
                _ExeTimes = value;
            }
        }
        /// <summary>
        /// 间隔时间(秒)
        /// </summary>
        public int PickSeconds
        {
            get
            {
                return _PickSeconds;
            }

            set
            {
                _PickSeconds = value;
            }
        }
        /// <summary>
        /// 进行时间(秒)
        /// </summary>
        public int RunSeconds
        {
            get
            {
                return _RunSeconds;
            }

            set
            {
                _RunSeconds = value;
            }
        }
        /// <summary>
        /// 是否暂停
        /// </summary>
        public bool PauseStatus
        {
            get
            {
                return _PauseStatus;
            }

            set
            {
                _PauseStatus = value;
            }
        }
        /// <summary>
        /// 是否执行中
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return _IsRunning;
            }

            set
            {
                _IsRunning = value;
            }
        }
        /// <summary>
        /// 执行方法名ID
        /// </summary>
        public string FuncID
        {
            get
            {
                return _FuncID;
            }

            set
            {
                _FuncID = value;
            }
        }
    }
}
