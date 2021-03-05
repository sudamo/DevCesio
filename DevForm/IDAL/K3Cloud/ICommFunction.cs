using System;
using System.Data;

namespace DevCesio.DevForm.IDAL.K3Cloud
{
    public interface ICommFunction
    {
        string Userlog(string pUrl, string pZTID, string pUser, string pPWD);
        string CheckedConnection(string pConnectionString);
        DateTime GetDateTime();
        string CheckFoun(string pFormId);
        void SetSql();
        DataTable GetAllOrders();
        void ExecuteApiByOrder(Model.XBT.UpOrderInfo pEntry);
    }
}
