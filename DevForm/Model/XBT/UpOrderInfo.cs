using System;

namespace DevCesio.DevForm.Model.XBT
{
    public class UpOrderInfo
    {
        public UpOrderInfo() { }

        private int _fskey;
        private string _fscjqh;
        private string _fslxbs;
        private string _fsuser;
        private string _fspwd;
        private string _fszl;
        private string _flag;

        public int Fskey
        {
            get
            {
                return _fskey;
            }

            set
            {
                _fskey = value;
            }
        }

        public string Fscjqh
        {
            get
            {
                return _fscjqh;
            }

            set
            {
                _fscjqh = value;
            }
        }

        public string Fslxbs
        {
            get
            {
                return _fslxbs;
            }

            set
            {
                _fslxbs = value;
            }
        }

        public string Fsuser
        {
            get
            {
                return _fsuser;
            }

            set
            {
                _fsuser = value;
            }
        }

        public string Fspwd
        {
            get
            {
                return _fspwd;
            }

            set
            {
                _fspwd = value;
            }
        }

        public string Fszl
        {
            get
            {
                return _fszl;
            }

            set
            {
                _fszl = value;
            }
        }

        public string Flag
        {
            get
            {
                return _flag;
            }

            set
            {
                _flag = value;
            }
        }
    }
}
