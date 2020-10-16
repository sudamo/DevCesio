using System;

namespace DevCesio.DevForm.Model.Global
{
    using Basic;
    using K3Cloud;

    /// <summary>
    /// 全局参数实体
    /// </summary>
    public class GlobalParameter
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public GlobalParameter()
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="pK3Inf"></param>
        public GlobalParameter(K3Setting pK3Inf)
        {
            _K3Inf = pK3Inf;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="pSQLInf"></param>
        public GlobalParameter(SQLConfig pSQLInf)
        {
            _SQLInf = pSQLInf;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="pK3Inf">配置信息</param>
        public GlobalParameter(K3Setting pK3Inf, SQLConfig pSQLInf)
        {
            _K3Inf = pK3Inf;
            _SQLInf = pSQLInf;
        }

        private static bool _IsPause;
        private static bool _IsJournal;
        private static K3Setting _K3Inf;
        private static SQLConfig _SQLInf;

        /// <summary>
        /// K3配置信息
        /// </summary>
        public static K3Setting K3Inf
        {
            get
            {
                return _K3Inf;
            }

            set
            {
                _K3Inf = value;
            }
        }

        /// <summary>
        /// SQLServer配置信息
        /// </summary>
        public static SQLConfig SQLInf
        {
            get
            {
                return _SQLInf;
            }

            set
            {
                _SQLInf = value;
            }
        }

        /// <summary>
        /// 定时器执行
        /// </summary>
        public static bool IsPause
        {
            get
            {
                return _IsPause;
            }

            set
            {
                _IsPause = value;
            }
        }
        /// <summary>
        /// 是否记录操作日志
        /// </summary>
        public static bool IsJournal
        {
            get
            {
                return _IsJournal;
            }

            set
            {
                _IsJournal = value;
            }
        }
    }
}
