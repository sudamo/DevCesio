using System;

namespace DevCesio.DevForm.DALFactory.K3Cloud
{
    public static class DalCreator
    {
        public static IDAL.K3Cloud.ICommFunction CommFunction
        {
            get { return new SQL.K3Cloud.CommFunction(); }
        }
    }
}
