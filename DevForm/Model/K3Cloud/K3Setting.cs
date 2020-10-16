using System;

namespace DevCesio.DevForm.Model.K3Cloud
{
    public class K3Setting
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public K3Setting() { }

        public K3Setting(string pC_ERPADDRESS, string pC_ZTID, string pC_USERNAME, string pC_PASSWORD)
        {
            _C_ERPADDRESS = pC_ERPADDRESS;
            _C_ZTID = pC_ZTID;
            _C_USERNAME = pC_USERNAME;
            _C_PASSWORD = pC_PASSWORD;
        }

        private string _C_ERPADDRESS;
        private string _C_ZTID;
        private string _C_USERNAME;
        private string _C_PASSWORD;

        /// <summary>
        /// ERP地址
        /// </summary>
        public string C_ERPADDRESS
        {
            get { return _C_ERPADDRESS; }
            set { _C_ERPADDRESS = value; }
        }
        /// <summary>
        /// 账套ID
        /// </summary>
        public string C_ZTID
        {
            get { return _C_ZTID; }
            set { _C_ZTID = value; }
        }
        /// <summary>
        /// 数据库登陆用户名称
        /// </summary>
        public string C_USERNAME
        {
            get { return _C_USERNAME; }
            set { _C_USERNAME = value; }
        }
        /// <summary>
        /// 数据库登陆密码
        /// </summary>
        public string C_PASSWORD
        {
            get { return _C_PASSWORD; }
            set { _C_PASSWORD = value; }
        }
    }
}
