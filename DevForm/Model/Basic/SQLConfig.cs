using System;

namespace DevCesio.DevForm.Model.Basic
{
    /// <summary>
    /// SQLServer配置
    /// </summary>
    public class SQLConfig
    {
        public SQLConfig() { }
        public SQLConfig(string pConnectionString)
        {
            _ConnectionString = pConnectionString;
        }
        public SQLConfig(string pIP, string pPort, string pUserName, string pPassword, string pCatalog)
        {
            _IP = pIP;
            _Port = pPort;
            _UserName = pUserName;
            _Password = pPassword;
            _Catalog = pCatalog;

            if (pPort.Trim().Length > 0)
                _ConnectionString = "Data Source=" + pIP + ":" + pPort + ";Initial Catalog=" + pCatalog + ";User ID=" + pUserName + ";Password=" + pPassword + ";Max Pool Size=1024;";
            else
                _ConnectionString = "Data Source=" + pIP + ";Initial Catalog=" + pCatalog + ";User ID=" + pUserName + ";Password=" + pPassword + ";Max Pool Size=1024;";
        }

        private string _IP;
        private string _Port;
        private string _UserName;
        private string _Password;
        private string _Catalog;
        private int _Size;
        private string _ConnectionString;

        /// <summary>
        /// IP地址
        /// </summary>
        public string IP
        {
            get
            {
                return _IP;
            }

            set
            {
                _IP = value;
            }
        }
        /// <summary>
        /// 端口
        /// </summary>
        public string Port
        {
            get
            {
                return _Port;
            }

            set
            {
                _Port = value;
            }
        }
        /// <summary>
        /// 数据库用户名
        /// </summary>
        public string UserName
        {
            get
            {
                return _UserName;
            }

            set
            {
                _UserName = value;
            }
        }
        /// <summary>
        /// 数据库密码
        /// </summary>
        public string Password
        {
            get
            {
                return _Password;
            }

            set
            {
                _Password = value;
            }
        }
        /// <summary>
        /// 数据库
        /// </summary>
        public string Catalog
        {
            get
            {
                return _Catalog;
            }

            set
            {
                _Catalog = value;
            }
        }
        /// <summary>
        /// 大小
        /// </summary>
        public int Size
        {
            get
            {
                return _Size;
            }

            set
            {
                _Size = value;
            }
        }
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return _ConnectionString;
            }

            set
            {
                _ConnectionString = value;
            }
        }
    }
}
