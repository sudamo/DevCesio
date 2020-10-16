using System;
using System.Data;
using System.Collections.Generic;

namespace DevCesio.DevForm.IDAL.K3Cloud
{
    public interface ICommFunction
    {
        string Userlog(string pUrl, string pZTID, string pUser, string pPWD);
        string CheckedConnection(string pConnectionString);
        string CheckFoun(string pFormId);
        void SetSql();
        //DataTable GetAllNavigation();
        //List<string> GetNavigation();
        //int ChildNumber(string pParentId);
        //--
        //List<string> GetFormID();
        //DataTable Pur_GetData();
        //string Pur_Instock(DataTable pDataTable);
        //DataTable Pur_GetDataRec();
        //string Pur_InstockRec(DataTable pDataTable);
        DataTable GetAllOrders();
        void ExecuteApiByOrder(Model.XBT.UpOrderInfo pEntry);
    }
}
