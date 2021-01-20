using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;
using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json.Linq;

namespace DevCesio.DevForm.SQL.K3Cloud
{
    using IDAL.K3Cloud;
    using Model.Global;
    using Model.XBT;

    public class CommFunction : ICommFunction
    {
        #region Const String

        #region DataBasic SQL
        /// <summary>
        /// T1 创建表DM_ExecuteLog
        /// </summary>
        private const string C_CreateTable = @"
        IF OBJECT_ID('DM_ExecuteLog','U') IS NULL
        BEGIN
	        CREATE TABLE [dbo].[DM_ExecuteLog](
		        [ID] [int] IDENTITY(1,1) NOT NULL,
		        [Fouc] [varchar](50) NOT NULL,
		        [CreaDate] [datetime] NOT NULL,
		        [Context] [varchar](max) NULL,
	         CONSTRAINT [PK_DM_ExecuteLog_ID] PRIMARY KEY CLUSTERED 
	        (
		        [ID] ASC
	        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	        ) ON [PRIMARY]

	        ALTER TABLE [dbo].[DM_ExecuteLog] ADD  CONSTRAINT [DF_DM_ExecuteLog_CreaDate]  DEFAULT (getdate()) FOR [CreaDate]
	        ALTER TABLE [dbo].[DM_ExecuteLog] ADD  CONSTRAINT [DF_DM_ExecuteLog_Context]  DEFAULT ('') FOR [Context]
        END";
        /// <summary>
        /// P1 是否有存储过程DM_GetOrders
        /// </summary>
        private const string C_CheckProcedure = "IF OBJECT_ID('DM_GetOrders','P') IS NOT NULL DROP PROCEDURE [DM_GetOrders]";
        /// <summary>
        /// P2 创建存储过程DM_GetOrders
        /// </summary>
        private const string C_CreateProcedure = @"CREATE PROCEDURE dbo.DM_GetOrders
        AS
        DELETE FROM xbt_uporder WHERE fskey IN
        (
        SELECT u.fskey
        FROM xbt_uporder u
        LEFT JOIN xbt_data d ON u.fscjqh = d.fscjqh AND u.fslxbs = d.fsrklx AND d.fsbs = 0
        WHERE d.fskey IS NULL
        )
        SELECT DISTINCT fscjqh,fslxbs,fsuser,fspwd FROM xbt_uporder WITH(TABLOCK) WHERE flag = 0";
        #endregion

        #region GetData
        /// <summary>
        /// 1 收料通知单-采购入库
        /// </summary>
        private const string C_PURReceiveBill_STKInStock = @"SELECT a.fskey,a.fsid FID,a.fsentry FENTRYID,a.Forgid
	        ,a.fspno 收料通知单号,org.FNUMBER 收料组织,isnull(dep.fnumber,' ') 收料部门,isnull(sup.FNUMBER,' ') 供应商,isnull(org2.fnumber,'') 货主
	        ,mtl.FNUMBER 物料编码,oe.FACTRECEIVEQTY 交货数量,oe.FACTRECEIVEQTY - oes.FINSTOCKQTY 剩余入库数量,a.fssl 入库数量,unt.FNUMBER 单位,isnull(stk.fnumber,' ') 仓库,ISNULL(FV.FNUMBER,'0') FV,ISNULL(F.FID,'0') FVV,ISNULL(F.FNUMBER,'0') 仓位,a.fspc 批号
	        ,ISNULL(us.FNUMBER,' ') 操作员,b.fscjqh 机器号,b.fsuser 用户,b.fspwd 密码
        FROM xbt_data a
        INNER JOIN  T_PUR_RECEIVEENTRY oe ON a.Fsentry = oe.FENTRYID
        INNER JOIN  T_PUR_RECEIVEENTRY_s oes ON oe.FENTRYID = oes.FENTRYID
        LEFT JOIN T_ORG_ORGANIZATIONS org ON a.Forgid = org.FORGID
        LEFT JOIN T_ORG_ORGANIZATIONS org2 ON a.fownerid = org2.FORGID
        LEFT JOIN T_BD_DEPARTMENT dep ON a.FsDepartid = dep.FDEPTID
        LEFT JOIN T_BD_SUPPLIER sup ON a.fsgysbh = sup.FSUPPLIERID
        INNER JOIN T_BD_MATERIAL mtl ON a.fshpid = mtl.FMATERIALID
        INNER JOIN T_BD_UNIT unt ON a.Fsunit = unt.FUNITID
        LEFT JOIN T_BD_STOCK stk ON a.fsckid = stk.FSTOCKID
		LEFT JOIN T_BD_STOCKFLEXITEM sf ON stk.FSTOCKID = sf.FSTOCKID
		LEFT JOIN T_BAS_FLEXVALUES fv ON sf.FFLEXID = FV.FID
		LEFT JOIN T_BAS_FLEXVALUESENTRY f on sf.FFLEXID = f.FID
        LEFT JOIN T_BD_STAFF us ON a.fsmen = us.FSTAFFID
        LEFT JOIN (SELECT DISTINCT fscjqh,fslxbs,u.FNAME fsuser,fspwd,fszl FROM xbt_uporder xu INNER JOIN T_SEC_USER u ON xu.fsuser = u.FUSERID) b ON a.fsrklx = b.fslxbs AND a.fscjqh = b.fscjqh
        WHERE a.fsbs = 0 AND a.fsrklx = 1";
        /// <summary>
        /// 11 采购订单-采购入库
        /// </summary>
        private const string C_PURPurchaseOrder_STKInStock = @"SELECT a.fskey,a.fsid FID,a.fsentry FENTRYID,a.Forgid
	        ,a.fspno 采购订单号,org.FNUMBER 收料组织,isnull(dep.fnumber,' ') 收料部门,isnull(sup.FNUMBER,' ') 供应商,isnull(org2.fnumber,'') 货主
	        ,mtl.FNUMBER 物料编码,oe.FQTY 采购数量,oer.FREMAINSTOCKINQTY 剩余入库数量,a.fssl 入库数量,unt.FNUMBER 单位,isnull(stk.fnumber,' ') 仓库,ISNULL(FV.FNUMBER,'0') FV,ISNULL(F.FID,'0') FVV,ISNULL(F.FNUMBER,'0') 仓位,a.fspc 批号
	        ,ISNULL(us.FNUMBER,' ') 操作员,b.fscjqh 机器号,b.fsuser 用户,b.fspwd 密码
        FROM xbt_data a
        INNER JOIN  T_PUR_POORDERENTRY oe ON a.Fsentry = oe.FENTRYID
        INNER JOIN  T_PUR_POORDERENTRY_R oer ON oe.FENTRYID = oer.FENTRYID
        LEFT JOIN T_ORG_ORGANIZATIONS org ON a.Forgid = org.FORGID
        LEFT JOIN T_ORG_ORGANIZATIONS org2 ON a.fownerid = org2.FORGID
        LEFT JOIN T_BD_DEPARTMENT dep ON a.FsDepartid = dep.FDEPTID
        LEFT JOIN T_BD_SUPPLIER sup ON a.fsgysbh = sup.FSUPPLIERID
        INNER JOIN T_BD_MATERIAL mtl ON a.fshpid = mtl.FMATERIALID
        INNER JOIN T_BD_UNIT unt ON a.Fsunit = unt.FUNITID
        LEFT JOIN T_BD_STOCK stk ON a.fsckid = stk.FSTOCKID
		LEFT JOIN T_BD_STOCKFLEXITEM sf ON stk.FSTOCKID = sf.FSTOCKID
		LEFT JOIN T_BAS_FLEXVALUES fv ON sf.FFLEXID = FV.FID
		LEFT JOIN T_BAS_FLEXVALUESENTRY f on sf.FFLEXID = f.FID
        LEFT JOIN T_BD_STAFF us ON a.fsmen = us.FSTAFFID
        LEFT JOIN (SELECT DISTINCT fscjqh,fslxbs,u.FNAME fsuser,fspwd,fszl FROM xbt_uporder xu INNER JOIN T_SEC_USER u ON xu.fsuser = u.FUSERID) b ON a.fsrklx = b.fslxbs AND a.fscjqh = b.fscjqh
        WHERE a.fsbs = 0 AND a.fsrklx = 11";
        /// <summary>
        /// 9 采购入库单-采购退料单
        /// </summary>
        private const string C_STKInStock_PUR_MRB = @"SELECT a.fskey,a.fsid FID,a.fsentry FENTRYID,a.Forgid,'采购入库-采购退料' 类型
	        ,a.fspno 采购入库单号,org.FNUMBER 退料组织,isnull(dep.fnumber,' ') 退料部门,isnull(sup.FNUMBER,' ') 供应商,isnull(org2.fnumber,'') 货主
	        ,mtl.FNUMBER 物料编码,oe.FMUSTQTY 应收数量,oe.FREALQTY 实收数量,a.fssl 实退数量,unt.FNUMBER 单位,isnull(stk.fnumber,' ') 仓库,ISNULL(FV.FNUMBER,'0') FV,ISNULL(F.FID,'0') FVV,ISNULL(F.FNUMBER,'0') 仓位,a.fspc 批号
	        ,ISNULL(us.FNUMBER,' ') 操作员,b.fscjqh 机器号,b.fsuser 用户,b.fspwd 密码
        FROM xbt_data a
        INNER JOIN  T_STK_INSTOCKENTRY oe ON a.Fsentry = oe.FENTRYID
        LEFT JOIN T_ORG_ORGANIZATIONS org ON a.Forgid = org.FORGID
        LEFT JOIN T_ORG_ORGANIZATIONS org2 ON a.fownerid = org2.FORGID
        LEFT JOIN T_BD_DEPARTMENT dep ON a.FsDepartid = dep.FDEPTID
        LEFT JOIN T_BD_SUPPLIER sup ON a.fsgysbh = sup.FSUPPLIERID
        INNER JOIN T_BD_MATERIAL mtl ON a.fshpid = mtl.FMATERIALID
        INNER JOIN T_BD_UNIT unt ON a.Fsunit = unt.FUNITID
        LEFT JOIN T_BD_STOCK stk ON a.fsckid = stk.FSTOCKID
		LEFT JOIN T_BD_STOCKFLEXITEM sf ON stk.FSTOCKID = sf.FSTOCKID
		LEFT JOIN T_BAS_FLEXVALUES fv ON sf.FFLEXID = FV.FID
		LEFT JOIN T_BAS_FLEXVALUESENTRY f on sf.FFLEXID = f.FID
        LEFT JOIN T_BD_STAFF us ON a.fsmen = us.FSTAFFID
        LEFT JOIN (SELECT DISTINCT fscjqh,fslxbs,u.FNAME fsuser,fspwd,fszl FROM xbt_uporder xu INNER JOIN T_SEC_USER u ON xu.fsuser = u.FUSERID) b ON a.fsrklx = b.fslxbs AND a.fscjqh = b.fscjqh
        WHERE a.fsbs = 0 AND a.fsrklx = 9";
        /// <summary>
        /// 7 发货通知单-销售出库
        /// </summary>
        private const string C_SALDELIVERYNOTICE_SALOUTSTOCK = @"SELECT a.fskey,a.fsid FID,a.fsentry FENTRYID,a.Forgid
	        ,a.fspno 发货通知单号,cus.FNUMBER 客户,org.FNUMBER 发货组织,isnull(dep.fnumber,' ') 发货部门
	        ,mtl.FNUMBER 物料编码,oe.FQTY - oe.FBASESUMOUTQTY 应发数量,a.fssl 实发数量,unt.FNUMBER 单位,isnull(stk.fnumber,' ') 仓库,ISNULL(FV.FNUMBER,'0') FV,ISNULL(F.FID,'0') FVV,ISNULL(F.FNUMBER,'0') 仓位,a.fspc 批号
            ,ISNULL(oe.F_SWH_TEXT,'') 订单号,ISNULL(oe.F_SWH_Text2,'') 非标尺寸,CONVERT(DECIMAL(18,4),oef.FPRICE) 单价,CONVERT(DECIMAL(18,2),oef.FTAXRATE) 税率
	        ,ISNULL(us.FNUMBER,' ') 操作员,b.fscjqh 机器号,b.fsuser 用户,b.fspwd 密码
        FROM xbt_data a
        INNER JOIN T_SAL_DELIVERYNOTICE o ON a.fsid = O.FID
        INNER JOIN T_SAL_DELIVERYNOTICEENTRY oe ON a.Fsentry = oe.FENTRYID
        INNER JOIN T_SAL_DELIVERYNOTICEENTRY_F oef ON oe.FENTRYID = oef.FENTRYID
        LEFT JOIN T_BD_CUSTOMER cus ON o.FCUSTOMERID = cus.FCUSTID
        LEFT JOIN T_ORG_ORGANIZATIONS org ON o.FDELIVERYORGID = org.FORGID
        LEFT JOIN T_BD_DEPARTMENT dep ON o.FDELIVERYDEPTID = dep.FDEPTID
        INNER JOIN T_BD_MATERIAL mtl ON a.fshpid = mtl.FMATERIALID
        INNER JOIN T_BD_UNIT unt ON a.Fsunit = unt.FUNITID
        LEFT JOIN T_BD_STOCK stk ON a.fsckid = stk.FSTOCKID
		LEFT JOIN T_BD_STOCKFLEXITEM sf ON stk.FSTOCKID = sf.FSTOCKID
		LEFT JOIN T_BAS_FLEXVALUES fv ON sf.FFLEXID = FV.FID
		LEFT JOIN T_BAS_FLEXVALUESENTRY f on sf.FFLEXID = f.FID
        LEFT JOIN T_BD_STAFF us ON a.fsmen = us.FSTAFFID
        LEFT JOIN (SELECT DISTINCT fscjqh,fslxbs,u.FNAME fsuser,fspwd,fszl FROM xbt_uporder xu INNER JOIN T_SEC_USER u ON xu.fsuser = u.FUSERID) b ON a.fsrklx = b.fslxbs AND a.fscjqh = b.fscjqh
        WHERE a.fsbs = 0 AND a.fsrklx = 7";
        /// <summary>
        /// 4 销售出库-销售退货
        /// </summary>
        private const string C_SALOUTSTOCK_SALRETURNSTOCK = @"SELECT a.fskey,a.fsid FID,a.fsentry FENTRYID,a.Forgid
	        ,a.fspno 销售出库单号,cus.FNUMBER 客户,org.FNUMBER 销售组织,isnull(dep.fnumber,' ') 销售部门
	        ,mtl.FNUMBER 物料编码,oe.FMUSTQTY 数量,a.fssl 实退数量,unt.FNUMBER 单位,isnull(stk.fnumber,' ') 仓库,ISNULL(FV.FNUMBER,'0') FV,ISNULL(F.FID,'0') FVV,ISNULL(F.FNUMBER,'0') 仓位,a.fspc 批号
	        ,ISNULL(us.FNUMBER,' ') 操作员,b.fscjqh 机器号,b.fsuser 用户,b.fspwd 密码
        FROM xbt_data a
        INNER JOIN T_SAL_OUTSTOCK o ON a.fsid = O.FID
        INNER JOIN T_SAL_OUTSTOCKENTRY oe ON a.Fsentry = oe.FENTRYID
        LEFT JOIN T_BD_CUSTOMER cus ON o.FCUSTOMERID = cus.FCUSTID
        LEFT JOIN T_ORG_ORGANIZATIONS org ON a.Forgid = org.FORGID
        LEFT JOIN T_BD_DEPARTMENT dep ON o.FDELIVERYDEPTID = dep.FDEPTID
        INNER JOIN T_BD_MATERIAL mtl ON a.fshpid = mtl.FMATERIALID
        INNER JOIN T_BD_UNIT unt ON a.Fsunit = unt.FUNITID
        LEFT JOIN T_BD_STOCK stk ON oe.FSTOCKID = stk.FSTOCKID
		LEFT JOIN T_BD_STOCKFLEXITEM sf ON stk.FSTOCKID = sf.FSTOCKID
		LEFT JOIN T_BAS_FLEXVALUES fv ON sf.FFLEXID = FV.FID
		LEFT JOIN T_BAS_FLEXVALUESENTRY f on sf.FFLEXID = f.FID
        LEFT JOIN T_BD_STAFF us ON a.fsmen = us.FSTAFFID
        LEFT JOIN (SELECT DISTINCT fscjqh,fslxbs,u.FNAME fsuser,fspwd,fszl FROM xbt_uporder xu INNER JOIN T_SEC_USER u ON xu.fsuser = u.FUSERID) b ON a.fsrklx = b.fslxbs AND a.fscjqh = b.fscjqh
        WHERE a.fsbs = 0 AND a.fsrklx = 4";
        /// <summary>
        /// 2 生产订单-生产入库
        /// </summary>
        private const string C_PrdMo_PrdInStock = @"SELECT a.fskey,a.fsid FID,a.fsentry FENTRYID,a.Forgid FORGID,a.fspno 生产订单号,org2.FNUMBER 入库组织,ISNULL(org.fnumber,'') 货主,oe.FPRODUCTTYPE 产品类型,oe.FCOSTRATE,ISNULL(DEP.FNUMBER,'') 部门
	        ,mtl.FNUMBER 物料编码,oq.FNOSTOCKINQTY 应收数量,a.fssl 实收数量,unt.FNUMBER 单位,ISNULL(stk.fnumber,' ') 仓库,ISNULL(FV.FNUMBER,'0') FV,ISNULL(F.FID,'0') FVV,ISNULL(F.FNUMBER,'0') 仓位,a.fspc 批号
	        ,ISNULL(us.FNUMBER,' ') 操作员,b.fscjqh 机器号,b.fsuser 用户,b.fspwd 密码	
        FROM xbt_data a
        INNER JOIN  T_PRD_MOENTRY oe ON a.Fsentry = oe.FENTRYID
        INNER JOIN T_PRD_MOENTRY_Q oq ON oe.FENTRYID = oq.FENTRYID
        INNER JOIN T_BD_MATERIAL mtl ON a.fshpid = mtl.FMATERIALID
        INNER JOIN T_BD_UNIT unt ON a.Fsunit = unt.FUNITID
        INNER JOIN T_BD_STOCK stk ON a.fsckid = stk.FSTOCKID
		LEFT JOIN T_BD_STOCKFLEXITEM sf ON stk.FSTOCKID = sf.FSTOCKID
		LEFT JOIN T_BAS_FLEXVALUES fv ON sf.FFLEXID = FV.FID
		LEFT JOIN T_BAS_FLEXVALUESENTRY f on sf.FFLEXID = f.FID
        LEFT JOIN T_BD_DEPARTMENT dep ON oe.FWORKSHOPID = dep.FDEPTID
        LEFT JOIN T_BD_STAFF us ON a.fsmen = us.FSTAFFID
        LEFT JOIN T_ORG_ORGANIZATIONS org ON a.fownerid = org.FORGID
        LEFT JOIN T_ORG_ORGANIZATIONS org2 ON a.Forgid = org2.FORGID
        LEFT JOIN 
        (
	        SELECT DISTINCT fscjqh,fslxbs,u.FNAME fsuser,fspwd,fszl FROM xbt_uporder xu INNER JOIN T_SEC_USER u ON xu.fsuser = u.FUSERID
        ) b ON a.fsrklx = b.fslxbs AND a.fscjqh = b.fscjqh
        WHERE a.fsbs = 0 AND a.fsrklx = 2";
        /// <summary>
        /// 21 生产入库-生产退库
        /// </summary>
        private const string C_PrdInStock_PrdRetStock = @"SELECT a.fskey,a.fsid FID,a.fsentry FENTRYID,a.Forgid FORGID,a.fspno 生产入库单号,org2.FNUMBER 退库组织,ISNULL(org.fnumber,'') 货主,oe.FPRODUCTTYPE 产品类型,ISNULL(DEP.FNUMBER,'') 部门
	        ,mtl.FNUMBER 物料编码, oe.FMUSTQTY 应退数量, a.fssl 实退数量, unt.FNUMBER 单位, ISNULL(stk.fnumber, ' ') 仓库,ISNULL(FV.FNUMBER,'0') FV,ISNULL(F.FID,'0') FVV,ISNULL(F.FNUMBER,'0') 仓位, a.fspc 批号
            , ISNULL(us.FNUMBER, ' ') 操作员,b.fscjqh 机器号, b.fsuser 用户, b.fspwd 密码
            , oe.FSRCBILLNO FMOBILLNO, OE.FSRCENTRYID FMOENTRYID
        FROM xbt_data a
        INNER JOIN T_PRD_INSTOCKENTRY oe ON a.Fsentry = oe.FENTRYID
        INNER JOIN T_BD_MATERIAL mtl ON a.fshpid = mtl.FMATERIALID
        INNER JOIN T_BD_UNIT unt ON a.Fsunit = unt.FUNITID
        LEFT JOIN T_BD_STOCK stk ON a.fsckid = stk.FSTOCKID
		LEFT JOIN T_BD_STOCKFLEXITEM sf ON stk.FSTOCKID = sf.FSTOCKID
        LEFT JOIN T_BAS_FLEXVALUES fv ON sf.FFLEXID = FV.FID
		LEFT JOIN T_BAS_FLEXVALUESENTRY f on sf.FFLEXID = f.FID
        LEFT JOIN T_BD_DEPARTMENT dep ON oe.FWORKSHOPID = dep.FDEPTID
        LEFT JOIN T_BD_STAFF us ON a.fsmen = us.FSTAFFID
        LEFT JOIN T_ORG_ORGANIZATIONS org ON a.fownerid = org.FORGID
        LEFT JOIN T_ORG_ORGANIZATIONS org2 ON a.Forgid = org2.FORGID
        LEFT JOIN
        (
            SELECT DISTINCT fscjqh, fslxbs, u.FNAME fsuser, fspwd, fszl FROM xbt_uporder xu INNER JOIN T_SEC_USER u ON xu.fsuser = u.FUSERID
        ) b ON a.fsrklx = b.fslxbs AND a.fscjqh = b.fscjqh
        WHERE a.fsbs = 0 AND a.fsrklx = 21";
        /// <summary>
        /// 更新实退数量&退库选单数
        /// </summary>
        private const string C_C_PrdPickMtrl_PrdReturnMtrl_RealQty = @"UPDATE AE SET FREALQTY = B.fssl
        FROM T_PRD_RESTOCKENTRY AE
        INNER JOIN T_PRD_RESTOCKENTRY_A AEA ON AE.FENTRYID = AEA.FENTRYID
        INNER JOIN xbt_data B ON AEA.FSRCENTRYID = B.Fsentry AND B.fsrklx = 21 AND B.fsbs = 0
        UPDATE AEA SET FSELRESTKQTY = ISNULL(C.FSELRESTKQTY,0),FBASESELRESTKQTY = ISNULL(C.FSELRESTKQTY,0)
        FROM T_PRD_INSTOCKENTRY_A AEA
        INNER JOIN xbt_data B ON AEA.FENTRYID = B.Fsentry AND B.fsrklx = 21 AND B.fsbs = 0
        LEFT JOIN
        (
        SELECT SUM(AE.FREALQTY) FSELRESTKQTY,AEA.FSRCENTRYID
        FROM T_PRD_RESTOCKENTRY AE
        INNER JOIN T_PRD_RESTOCKENTRY_A AEA ON AE.FENTRYID = AEA.FENTRYID
        GROUP BY AEA.FSRCENTRYID
        )AS C ON AEA.FENTRYID = C.FSRCENTRYID";
        /// <summary>
        /// 6 生产用料清单-生产领料
        /// </summary>
        private const string C_PrdPPBom_PrdPickMtrl = @"SELECT a.fskey,a.fsid FID,a.fsentry FENTRYID,a.Forgid FORGID,a.fspno 生产用料清单号,org.FNUMBER 发料组织,ISNULL(org2.fnumber,'') 货主,ISNULL(DEP.FNUMBER,'') 部门
	        ,mtl.FNUMBER 物料编码,oe.FNEEDQTY 申请数量,a.fssl 实发数量,unt.FNUMBER 单位,ISNULL(stk.fnumber,' ') 仓库,ISNULL(FV.FNUMBER,'0') FV,ISNULL(F.FID,'0') FVV,ISNULL(F.FNUMBER,'0') 仓位,a.fspc 批号
	        ,ISNULL(us.FNUMBER,' ') 操作员,b.fscjqh 机器号,b.fsuser 用户,b.fspwd 密码
	        ,oe.FMOID,oe.fmobillno,oe.fmoentryid,oe.FMOENTRYSEQ,oe.fbomentryid
        FROM xbt_data a
        INNER JOIN  T_PRD_PPBOMENTRY oe ON a.Fsentry = oe.FENTRYID
        INNER JOIN  T_PRD_PPBOMENTRY_C oec ON oe.FENTRYID = oec.FENTRYID
        LEFT JOIN T_ORG_ORGANIZATIONS org ON oec.FSUPPLYORG = org.FORGID
        LEFT JOIN T_ORG_ORGANIZATIONS org2 ON oec.FOWNERID = org2.FORGID
        INNER JOIN T_BD_MATERIAL mtl ON oe.FMATERIALID = mtl.FMATERIALID
        INNER JOIN T_BD_UNIT unt ON a.Fsunit = unt.FUNITID
        LEFT JOIN T_BD_STOCK stk ON a.fsckid = stk.FSTOCKID
		LEFT JOIN T_BD_STOCKFLEXITEM sf ON stk.FSTOCKID = sf.FSTOCKID
        LEFT JOIN T_BAS_FLEXVALUES fv ON sf.FFLEXID = FV.FID
		LEFT JOIN T_BAS_FLEXVALUESENTRY f on sf.FFLEXID = f.FID
        LEFT JOIN T_BD_DEPARTMENT dep ON a.FsDepartid = dep.FDEPTID
        LEFT JOIN T_BD_STAFF us ON a.fsmen = us.FSTAFFID
        LEFT JOIN 
        (
	        SELECT DISTINCT fscjqh,fslxbs,u.FNAME fsuser,fspwd,fszl FROM xbt_uporder xu INNER JOIN T_SEC_USER u ON xu.fsuser = u.FUSERID
        ) b ON a.fsrklx = b.fslxbs AND a.fscjqh = b.fscjqh
        WHERE a.fsbs = 0 AND a.fsrklx = 6";
        /// <summary>
        /// 5 生产领料-生产退料
        /// </summary>
        private const string C_PrdPickMtrl_PrdReturnMtrl = @"SELECT a.fskey,a.fsid FID,a.fsentry FENTRYID,oe.FSEQ,a.Forgid FORGID,a.fspno 生产领料单号,org2.FNUMBER 收料组织,ISNULL(org.fnumber,'') 货主,ISNULL(DEP.FNUMBER,'') 部门
	        ,oea.FPARENTMATERIALID,mtl2.FNUMBER 产品编码,mtl.FNUMBER 物料编码,oe.FAPPQTY 申请数量,a.fssl 实退数量,unt.FNUMBER 单位,ISNULL(stk.fnumber,' ') 仓库,ISNULL(FV.FNUMBER,'0') FV,ISNULL(F.FID,'0') FVV,ISNULL(F.FNUMBER,'0') 仓位,a.fspc 批号
	        ,ISNULL(us.FNUMBER,' ') 操作员,b.fscjqh 机器号,b.fsuser 用户,b.fspwd 密码
	        ,oe.FSRCBILLTYPE,oe.FSRCINTERID,FSRCENTRYID,oe.FSRCENTRYSEQ,oe.FSRCBILLNO,oe.FPPBOMBILLNO,oe.FPPBOMENTRYID,oe.FMOID,oe.FMOBILLNO,oe.FMOENTRYID,oe.FMOENTRYSEQ
        FROM xbt_data a
        LEFT JOIN  T_PRD_PICKMTRLDATA oe ON a.Fsentry = oe.FENTRYID
        LEFT JOIN  T_PRD_PICKMTRLDATA_A oea ON oe.FENTRYID = oea.FENTRYID
        LEFT JOIN T_ORG_ORGANIZATIONS org ON a.fownerid = org.FORGID
        LEFT JOIN T_ORG_ORGANIZATIONS org2 ON a.Forgid = org2.FORGID
        LEFT JOIN T_BD_MATERIAL mtl ON oe.FMATERIALID = mtl.FMATERIALID
        LEFT JOIN T_BD_MATERIAL mtl2 ON oea.FPARENTMATERIALID = mtl2.FMATERIALID
        LEFT JOIN T_BD_UNIT unt ON a.Fsunit = unt.FUNITID
        LEFT JOIN T_BD_STOCK stk ON a.fsckid = stk.FSTOCKID
		LEFT JOIN T_BD_STOCKFLEXITEM sf ON stk.FSTOCKID = sf.FSTOCKID
        LEFT JOIN T_BAS_FLEXVALUES fv ON sf.FFLEXID = FV.FID
		LEFT JOIN T_BAS_FLEXVALUESENTRY f on sf.FFLEXID = f.FID
        LEFT JOIN T_BD_DEPARTMENT dep ON oe.FWORKSHOPID = dep.FDEPTID
        LEFT JOIN T_BD_STAFF us ON a.fsmen = us.FSTAFFID
        LEFT JOIN 
        (
	        SELECT DISTINCT fscjqh,fslxbs,u.FNAME fsuser,fspwd,fszl FROM xbt_uporder xu INNER JOIN T_SEC_USER u ON xu.fsuser = u.FUSERID
        ) b ON a.fsrklx = b.fslxbs AND a.fscjqh = b.fscjqh
        WHERE a.fsbs = 0 AND a.fsrklx = 5";
        /// <summary>
        /// 3 其他入库
        /// </summary>
        private const string C_STK_MISCELLANEOUS = @"SELECT a.fskey,a.fsid FID,a.fsentry FENTRYID,a.Forgid,isnull(sup.FNUMBER,' ') 供应商
	        ,mtl.FNUMBER 物料编码,a.fssl 实收数量,unt.FNUMBER 单位,isnull(stk.fnumber,' ') 仓库,ISNULL(FV.FNUMBER,'0') FV,ISNULL(F.FID,'0') FVV,ISNULL(F.FNUMBER,'0') 仓位,org2.FNUMBER 库存组织,isnull(dep.fnumber,' ') 部门,a.fspc 批号
	        ,ISNULL(us.FNUMBER,' ') 操作员,isnull(org.fnumber,'') 货主
	        ,b.fscjqh 机器号,b.fsuser 用户,b.fspwd 密码
        FROM xbt_data a
        LEFT JOIN T_BD_SUPPLIER sup ON a.fsgysbh = sup.FSUPPLIERID
        INNER JOIN T_BD_MATERIAL mtl ON a.fshpid = mtl.FMATERIALID
        INNER JOIN T_BD_UNIT unt ON a.Fsunit = unt.FUNITID
        LEFT JOIN T_BD_STOCK stk ON a.fsckid = stk.FSTOCKID
		LEFT JOIN T_BD_STOCKFLEXITEM sf ON stk.FSTOCKID = sf.FSTOCKID
        LEFT JOIN T_BAS_FLEXVALUES fv ON sf.FFLEXID = FV.FID
		LEFT JOIN T_BAS_FLEXVALUESENTRY f on sf.FFLEXID = f.FID
        LEFT JOIN T_BD_DEPARTMENT dep ON a.FsDepartid = dep.FDEPTID
        LEFT JOIN T_BD_STAFF us ON a.fsmen = us.FSTAFFID
        LEFT JOIN T_ORG_ORGANIZATIONS org ON a.fownerid = org.FORGID
        LEFT JOIN T_ORG_ORGANIZATIONS org2 ON a.Forgid = org2.FORGID
        LEFT JOIN 
        (
	        SELECT DISTINCT fscjqh,fslxbs,u.FNAME fsuser,fspwd,fszl FROM xbt_uporder xu INNER JOIN T_SEC_USER u ON xu.fsuser = u.FUSERID
        ) b ON a.fsrklx = b.fslxbs AND a.fscjqh = b.fscjqh
        WHERE a.fsbs = 0 AND a.fsrklx = 3";
        /// <summary>
        /// 8 其他出库
        /// </summary>
        private const string C_STK_MisDelivery = @"SELECT a.fskey,a.fsid FID,a.fsentry FENTRYID,a.Forgid,isnull(cut.FNUMBER,' ') 客户
	        ,mtl.FNUMBER 物料编码,a.fssl 实发数量,unt.FNUMBER 单位,isnull(stk.fnumber,' ') 仓库,ISNULL(FV.FNUMBER,'0') FV,ISNULL(F.FID,'0') FVV,ISNULL(F.FNUMBER,'0') 仓位,org2.FNUMBER 库存组织,isnull(dep.fnumber,' ') 部门,a.fspc 批号
	        ,ISNULL(us.FNUMBER,' ') 操作员,isnull(org.fnumber,'') 货主
	        ,b.fscjqh 机器号,b.fsuser 用户,b.fspwd 密码
        FROM xbt_data a
        LEFT JOIN t_bd_customer cut ON a.fsgysbh = cut.fcustid
        INNER JOIN T_BD_MATERIAL mtl ON a.fshpid = mtl.FMATERIALID
        INNER JOIN T_BD_UNIT unt ON a.Fsunit = unt.FUNITID
        INNER JOIN T_BD_STOCK stk ON a.fsckid = stk.FSTOCKID
		LEFT JOIN T_BD_STOCKFLEXITEM sf ON stk.FSTOCKID = sf.FSTOCKID
        LEFT JOIN T_BAS_FLEXVALUES fv ON sf.FFLEXID = FV.FID
		LEFT JOIN T_BAS_FLEXVALUESENTRY f on sf.FFLEXID = f.FID
        LEFT JOIN T_BD_DEPARTMENT dep ON a.FsDepartid = dep.FDEPTID
        LEFT JOIN T_BD_STAFF us ON a.fsmen = us.FSTAFFID
        LEFT JOIN T_ORG_ORGANIZATIONS org ON a.fownerid = org.FORGID
        LEFT JOIN T_ORG_ORGANIZATIONS org2 ON a.Forgid = org2.FORGID
        LEFT JOIN 
        (
	        SELECT DISTINCT fscjqh,fslxbs,u.FNAME fsuser,fspwd,fszl FROM xbt_uporder xu INNER JOIN T_SEC_USER u ON xu.fsuser = u.FUSERID
        ) b ON a.fsrklx = b.fslxbs AND a.fscjqh = b.fscjqh
        WHERE a.fsbs = 0 AND a.fsrklx = 8";
        /// <summary>
        /// 34,35 组装拆卸单
        /// </summary>
        private const string C_STK_AssembleAPP = @"SELECT a.fskey,a.finnerid,org.FNUMBER 库存组织,org2.FNUMBER 货主,ISNULL(dep.FNUMBER,'') 部门,mtl.FNUMBER 物料编码,unt.FNUMBER 单位,a.fssl 数量,stk.FNUMBER 仓库,ISNULL(FV.FNUMBER,'0') FV,ISNULL(F.FID,'0') FVV,ISNULL(F.FNUMBER,'0') 仓位
        ,mtl2.FNUMBER 子件物料编码,unt2.FNUMBER 子件单位,ae.fssl 子件数量,stk2.FNUMBER 子件仓库,ISNULL(FV2.FNUMBER,'0') FV2,ISNULL(F2.FID,'0') FVV2,ISNULL(F2.FNUMBER,'0') 子件仓位
        ,ISNULL(us.FNUMBER,' ') 操作员,b.fscjqh 机器号,b.fsuser 用户,b.fspwd 密码
        FROM xbt_data a
        INNER JOIN T_ORG_ORGANIZATIONS org ON a.Forgid = org.FORGID
        INNER JOIN T_ORG_ORGANIZATIONS org2 ON a.fownerid = org2.FORGID
        LEFT JOIN T_BD_DEPARTMENT dep ON a.FsDepartid = dep.FDEPTID
        INNER JOIN T_BD_MATERIAL mtl ON a.fshpid = mtl.FMATERIALID
        INNER JOIN T_BD_UNIT unt ON a.Fsunit = unt.FUNITID
        INNER JOIN T_BD_STOCK stk ON a.fsckid = stk.FSTOCKID
		LEFT JOIN T_BD_STOCKFLEXITEM sf ON stk.FSTOCKID = sf.FSTOCKID
        LEFT JOIN T_BAS_FLEXVALUES fv ON sf.FFLEXID = FV.FID
		LEFT JOIN T_BAS_FLEXVALUESENTRY f on sf.FFLEXID = f.FID
        INNER join xbt_dataentry ae on a.finnerid = ae.finnerid
        INNER JOIN T_BD_MATERIAL mtl2 ON ae.Fshpid = mtl2.FMATERIALID
        INNER JOIN T_BD_UNIT unt2 ON ae.Fsunit = unt2.FUNITID
        INNER JOIN T_BD_STOCK stk2 ON ae.Fsckid = stk2.FSTOCKID
        LEFT JOIN T_BD_STOCKFLEXITEM sf2 ON stk2.FSTOCKID = sf2.FSTOCKID
        LEFT JOIN T_BAS_FLEXVALUES fv2 ON sf2.FFLEXID = FV2.FID
        LEFT JOIN T_BAS_FLEXVALUESENTRY f2 on sf2.FFLEXID = f2.FID
        LEFT JOIN T_BD_STAFF us ON a.fsmen = us.FSTAFFID
        LEFT JOIN
        (
            SELECT DISTINCT fscjqh,fslxbs,u.FNAME fsuser,fspwd,fszl FROM xbt_uporder xu INNER JOIN T_SEC_USER u ON xu.fsuser = u.FUSERID
        ) b ON a.fsrklx = b.fslxbs AND a.fscjqh = b.fscjqh
        WHERE a.fsbs = 0 AND a.fsrklx = {0}
        ORDER BY a.finnerid";
        /// <summary>
        /// 10 调拨申请单-直接调拨单
        /// </summary>
        private const string C_STK_TransferDirect = @"SELECT a.fskey,a.fsid FID,a.fsentry FENTRYID,a.Forgid,a.Fspno 调拨申请单号,org.fnumber 调出组织,org2.FNUMBER 调入组织,CASE WHEN oe.FSTOCKORGID = oe.FSTOCKORGINID THEN 'InnerOrgTransfer' ELSE 'OverOrgTransfer' END 调拨类型
	        ,mtl.FNUMBER 物料编码,a.fssl 调拨数量,unt.FNUMBER 单位,isnull(stk.fnumber,' ') 调入仓库,ISNULL(FV.FNUMBER,'0') FV,ISNULL(F.FID,'0') FVV,ISNULL(F.FNUMBER,'0') 调入仓位,isnull(stk2.fnumber,' ') 调出仓库,ISNULL(FV2.FNUMBER,'0') FV2,ISNULL(F2.FID,'0') FVV2,ISNULL(F2.FNUMBER,'0') 调出仓位,a.fspc 批号
	        ,ISNULL(us.FNUMBER,' ') 操作员,b.fscjqh 机器号,b.fsuser 用户,b.fspwd 密码
        FROM xbt_data a
		INNER JOIN T_STK_STKTRANSFERAPPENTRY oe ON a.Fsentry = oe.FENTRYID
        INNER JOIN T_ORG_ORGANIZATIONS org ON oe.FSTOCKORGID = org.FORGID
        INNER JOIN T_ORG_ORGANIZATIONS org2 ON oe.FSTOCKORGINID = org2.FORGID
        INNER JOIN T_BD_MATERIAL mtl ON a.fshpid = mtl.FMATERIALID
        INNER JOIN T_BD_UNIT unt ON a.Fsunit = unt.FUNITID
        INNER JOIN T_BD_STOCK stk ON a.fsckid = stk.FSTOCKID
		LEFT JOIN T_BD_STOCKFLEXITEM sf ON stk.FSTOCKID = sf.FSTOCKID
        LEFT JOIN T_BAS_FLEXVALUES fv ON sf.FFLEXID = FV.FID
		LEFT JOIN T_BAS_FLEXVALUESENTRY f on sf.FFLEXID = f.FID
        INNER JOIN T_BD_STOCK stk2 ON a.fsdcckid = stk2.FSTOCKID
		LEFT JOIN T_BD_STOCKFLEXITEM sf2 ON stk2.FSTOCKID = sf2.FSTOCKID
        LEFT JOIN T_BAS_FLEXVALUES fv2 ON sf2.FFLEXID = FV2.FID
		LEFT JOIN T_BAS_FLEXVALUESENTRY f2 on sf2.FFLEXID = f2.FID
        LEFT JOIN T_BD_STAFF us ON a.fsmen = us.FSTAFFID
        LEFT JOIN 
        (
	        SELECT DISTINCT fscjqh,fslxbs,u.FNAME fsuser,fspwd,fszl FROM xbt_uporder xu INNER JOIN T_SEC_USER u ON xu.fsuser = u.FUSERID
        ) b ON a.fsrklx = b.fslxbs AND a.fscjqh = b.fscjqh
        WHERE a.fsbs = 0 AND a.fsrklx = 10";
        #endregion

        #region Modify
        private const string C_UpdateMo = @"UPDATE A SET FPICKMTRLSTATUS = CASE WHEN B.FPICKEDQTY = 0 THEN 1 WHEN B.FMUSTQTY > B.FPICKEDQTY THEN 2 WHEN B.FMUSTQTY = B.FPICKEDQTY THEN 3 ELSE 4 END
        FROM T_PRD_MOENTRY_Q A
        INNER JOIN
        (
        SELECT A.FENTRYID,SUM(PE.FMUSTQTY) FMUSTQTY,SUM(PQ.FPICKEDQTY) FPICKEDQTY
        FROM T_PRD_MOENTRY_Q A
        INNER JOIN T_PRD_PPBOMENTRY PE ON PE.FMOENTRYID = A.FENTRYID
        INNER JOIN T_PRD_PPBOMENTRY_Q PQ ON PE.FENTRYID = PQ.FENTRYID
        WHERE A.FENTRYID IN({0})
        GROUP BY A.FENTRYID
        )B ON A.FENTRYID = B.FENTRYID
        WHERE A.FENTRYID IN({0})
        UPDATE A SET FSTATUS = CASE WHEN B.FLAG = 0 THEN 6 ELSE FSTATUS END
        FROM T_PRD_MOENTRY_A A 
        INNER JOIN
        (
        SELECT A.FENTRYID,SUM(CASE WHEN PE.FMUSTQTY = FBASEPICKEDQTY THEN 0 ELSE 1 END) FLAG
        FROM T_PRD_MOENTRY_A A 
        INNER JOIN T_PRD_PPBOMENTRY PE ON PE.FMOENTRYID = A.FENTRYID 
        INNER JOIN T_PRD_PPBOMENTRY_Q PQ ON PE.FENTRYID = PQ.FENTRYID 
        WHERE A.FENTRYID IN({0})
        GROUP BY A.FENTRYID
        )B ON A.FENTRYID = B.FENTRYID
        WHERE A.FENTRYID IN({0})";
        #endregion

        /// <summary>
        /// 32 委外用料清单-委外领料
        /// </summary>
        private const string C_SUB_PPBOM_SUB_PickMtrl = "";
        /// <summary>
        /// 33 委外领料单-委外退料
        /// </summary>
        private const string C_SUB_PickMtrl_SUB_RETURNMTRL = "";
        #endregion

        #region Prep
        /// <summary>
        /// 数据库链接验证
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <returns></returns>
        public string CheckedConnection(string pConnectionString)
        {
            return SQLHelper.ConnectionChecked(pConnectionString);
        }
        public DateTime GetDateTime()
        {
            return (DateTime)SQLHelper.ExecuteScalar("SELECT CONVERT(DATETIME,0)");
        }
        /// <summary>
        /// K3Cloud登陆验证
        /// </summary>
        /// <param name="pUrl"></param>
        /// <param name="pZTID"></param>
        /// <param name="pUser"></param>
        /// <param name="pPWD"></param>
        /// <returns></returns>
        public string Userlog(string pUrl, string pZTID, string pUser, string pPWD)
        {
            bool login = false;
            try
            {
                K3CloudApiClient client = new K3CloudApiClient(pUrl);
                login = client.Login(pZTID, pUser, pPWD, 2052);
            }
            catch
            {
                return "验证失败:请检查金蝶配置信息";
            }

            if (!login)
            {
                return "验证失败:用户名或密码错误";
            }
            return "";
        }
        /// <summary>
        /// 自动执行程序验证
        /// </summary>
        /// <param name="pFormId"></param>
        /// <returns></returns>
        public string CheckFoun(string pFormId)
        {
            SqlParameter[] parms = new SqlParameter[]
            {
                new SqlParameter("@FormID",SqlDbType.VarChar)
            };
            parms[0].Value = pFormId;

            try
            {
                object o = SQLHelper.ExecuteScalar("DM_CheckFoun", CommandType.StoredProcedure, parms);
                return o.ToString();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        /// <summary>
        /// 数据库预处理
        /// </summary>
        public void SetSql()
        {
            SQLHelper.ExecuteNonQuery(C_CreateTable);
            SQLHelper.ExecuteNonQuery(C_CheckProcedure);
            SQLHelper.ExecuteNonQuery(C_CreateProcedure);
        }
        /// <summary>
        /// 获取指令
        /// </summary>
        /// <returns></returns>
        public DataTable GetAllOrders()
        {
            return SQLHelper.ExecuteTable("DM_GetOrders");
        }
        #endregion

        /// <summary>
        /// 执行指令
        /// </summary>
        /// <param name="pEntity">指令实体</param>
        public void ExecuteApiByOrder(UpOrderInfo pEntity)
        {
            string billNo = string.Empty;
            DataTable dt = GetDataByEntry(pEntity);

            if (dt == null || dt.Rows.Count == 0)
            {
                goto A;
            }
            switch (pEntity.Fslxbs)
            {
                case "1":
                    billNo = STK_InStockR(dt);
                    ExecuteLog("收料通知单-采购入库", billNo);
                    break;
                case "11":
                    billNo = STK_InStock(dt);
                    ExecuteLog("采购订单-采购入库", billNo);
                    break;
                case "9":
                    billNo = PUR_MRB(dt);
                    ExecuteLog("采购入库单-采购退料单", billNo);
                    break;
                case "7":
                    billNo = SAL_OUTSTOCK(dt);
                    ExecuteLog("发货通知-销售出库", billNo);
                    break;
                case "4":
                    billNo = SAL_RETURNSTOCK(dt);
                    ExecuteLog("销售出库-销售退货", billNo);
                    break;
                case "2":
                    billNo = PRD_INSTOCK(dt);
                    ExecuteLog("生产订单-生产入库", billNo);
                    break;
                case "21":
                    billNo = PRD_RetStock(dt);
                    ExecuteLog("生产入库-生产退库", billNo);
                    break;
                case "6":
                    billNo = PRD_PickMtrl(dt);
                    ExecuteLog("生产用料清单-生产领料", billNo);
                    break;
                case "5":
                    billNo = PRD_ReturnMtrl(dt);
                    ExecuteLog("生产领料-生产退料", billNo);
                    break;
                case "3":
                    billNo = STK_MISCELLANEOUS(dt);
                    ExecuteLog("其他入库", billNo);
                    break;
                case "8":
                    billNo = STK_MisDelivery(dt);
                    ExecuteLog("其他出库", billNo);
                    break;
                case "34":
                case "35":
                    string Founc, FAffairType;
                    if (pEntity.Fslxbs == "34")
                    {
                        Founc = "组装拆卸单-组装";
                        FAffairType = "Assembly";
                    }
                    else
                    {
                        Founc = "组装拆卸单-拆卸";
                        FAffairType = "Dassembly";
                    }

                    IList<DataTable> list = new List<DataTable>();
                    DataTable dtSub = dt.Clone();
                    dtSub.ImportRow(dt.Rows[0]);
                    for (int i = 1; i < dt.Rows.Count; i++)
                    {
                        if (dtSub.Rows[0]["finnerid"].ToString() == dt.Rows[i]["finnerid"].ToString())//如果当前行与之前的数据集成品编码相同，则添加到dtSub
                        {
                            dtSub.ImportRow(dt.Rows[i]);
                        }
                        else //成品编码不同
                        {
                            list.Add(dtSub);//先把之前的数据添加到数据集
                            //重新添加数据
                            dtSub = dt.Clone();
                            dtSub.ImportRow(dt.Rows[i]);
                        }

                        if (i == dt.Rows.Count - 1)//最后一行数据
                            list.Add(dtSub);
                    }
                    if (dt.Rows.Count == 1)
                        list.Add(dtSub);

                    billNo = STK_AssembledApp(list, FAffairType);
                    ExecuteLog(Founc, billNo);
                    break;
                case "10":
                    billNo = STK_TransferDirect(dt);
                    ExecuteLog("调拨申请单-直接调拨单", billNo);
                    break;
                default:
                    billNo = string.Empty;
                    break;
            }

            A:
            if (billNo.Contains(";Number:"))
                DelOrders(pEntity.Fskey);//删除执行过的指令
            else
                UpdateXBT_UpOrderFlag(pEntity);//失败，则修改flag
        }

        /// <summary>
        /// 根据指令实体获取数据
        /// </summary>
        /// <param name="pEntity">指令实体</param>
        /// <returns></returns>
        private DataTable GetDataByEntry(UpOrderInfo pEntity)
        {
            string sql = string.Empty;

            switch (pEntity.Fslxbs)
            {
                case "1"://收料通知单-采购入库
                    sql = string.Format("{0} AND b.fscjqh = {1} ORDER BY 收料通知单号", C_PURReceiveBill_STKInStock, pEntity.Fscjqh);
                    break;
                case "11"://采购订单-采购入库
                    sql = string.Format("{0} AND b.fscjqh = {1} ORDER BY 采购订单号", C_PURPurchaseOrder_STKInStock, pEntity.Fscjqh);
                    break;
                case "9"://采购入库单-采购退料单
                    sql = string.Format("{0} AND b.fscjqh = {1} ORDER BY 采购入库单号", C_STKInStock_PUR_MRB, pEntity.Fscjqh);
                    break;
                case "7"://发货通知单-销售出库
                    sql = string.Format("{0} AND b.fscjqh = {1} ORDER BY 发货通知单号", C_SALDELIVERYNOTICE_SALOUTSTOCK, pEntity.Fscjqh);
                    break;
                case "4"://销售出库-销售退货
                    sql = string.Format("{0} AND b.fscjqh = {1} ORDER BY 销售出库单号", C_SALOUTSTOCK_SALRETURNSTOCK, pEntity.Fscjqh);
                    break;
                case "2"://生产订单-生产入库
                    sql = C_PrdMo_PrdInStock;
                    break;
                case "21"://生产入库-生产退库
                    sql = C_PrdInStock_PrdRetStock;
                    break;
                case "6"://生产用料清单-生产领料
                    sql = C_PrdPPBom_PrdPickMtrl;
                    break;
                case "5"://生产领料-生产退料
                    sql = C_PrdPickMtrl_PrdReturnMtrl;
                    break;
                case "3"://其他入库
                    sql = C_STK_MISCELLANEOUS;
                    break;
                case "8"://其他出库
                    sql = C_STK_MisDelivery;
                    break;
                case "34"://组装
                    sql = string.Format(C_STK_AssembleAPP, 34);
                    break;
                case "35"://拆卸
                    sql = string.Format(C_STK_AssembleAPP, 35);
                    break;
                case "10"://调拨申请单-直接调拨单
                    sql = C_STK_TransferDirect;
                    break;
            }


            if (sql.Equals(string.Empty))
                return null;

            return SQLHelper.ExecuteTable(sql);
        }

        #region KingdeeWebApi
        /// <summary>
        /// 采购订单-采购入库
        /// </summary>
        /// <param name="pDataTable"></param>
        /// <returns></returns>
        private string STK_InStock(DataTable pDataTable)
        {
            string BillNo = string.Empty;
            string fsKeys = string.Empty;
            Dictionary<string, float> lst = new Dictionary<string, float>();
            try
            {
                K3CloudApiClient client = new K3CloudApiClient(GlobalParameter.K3Inf.C_ERPADDRESS);
                bool login = client.Login(GlobalParameter.K3Inf.C_ZTID, GlobalParameter.K3Inf.C_USERNAME, GlobalParameter.K3Inf.C_PASSWORD, 2052);
                if (login)
                {
                    JObject jsonRoot = new JObject();
                    jsonRoot.Add("Creator", "PDA");
                    jsonRoot.Add("NeedUpDateFields", new JArray(""));
                    jsonRoot.Add("NeedReturnFields", new JArray(""));

                    jsonRoot.Add("IsDeleteEntry", "true");
                    jsonRoot.Add("SubSystemId", "");
                    jsonRoot.Add("IsVerifyBaseDataField", "false");
                    jsonRoot.Add("IsEntryBatchFill", "true");
                    jsonRoot.Add("ValidateFlag", "true");
                    jsonRoot.Add("NumberSearch", "true");
                    jsonRoot.Add("InterationFlags", "");
                    jsonRoot.Add("IsAutoSubmitAndAudit", "false");

                    // Model: 单据详细数据参数
                    JObject model = new JObject();
                    jsonRoot.Add("Model", model);

                    // 
                    model.Add("FID", 0);
                    // 单据类型
                    JObject basedata = new JObject();
                    basedata.Add("FNumber", "RKD01_SYS");
                    model.Add("FBillTypeID", basedata);
                    // 日期
                    model.Add("FDate", DateTime.Today);
                    //组织
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["收料组织"].ToString());
                    model.Add("FStockOrgId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["收料部门"].ToString());
                    model.Add("FStockDeptId", basedata);
                    //if (pDataTable.Rows[0]["操作员"].ToString() != string.Empty)
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["操作员"].ToString());//
                    model.Add("FStockerId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["收料组织"].ToString());
                    model.Add("FDemandOrgId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["收料组织"].ToString());
                    model.Add("FPurchaseOrgId", basedata);
                    // 供应商
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["供应商"].ToString());
                    model.Add("FSupplierId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["供应商"].ToString());
                    model.Add("FSupplyId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["供应商"].ToString());
                    model.Add("FSettleId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["供应商"].ToString());
                    model.Add("FChargeId", basedata);

                    model.Add("FOwnerTypeIdHead", "BD_OwnerOrg");
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["收料组织"].ToString());
                    model.Add("FOwnerIdHead", basedata);
                    
                    model.Add("FSupplierBillNo", "1");


                    //FPOOrderFinance
                    JObject InStockFin = new JObject();
                    model.Add("FInStockFin", InStockFin);

                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["收料组织"].ToString());
                    InStockFin.Add("FSettleOrgId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", "PRE001");
                    InStockFin.Add("FSettleCurrId", basedata);

                    InStockFin.Add("FIsIncludedTax", true);
                    InStockFin.Add("FPriceTimePoint", 1);

                    basedata = new JObject();
                    basedata.Add("FNumber", "PRE001");
                    InStockFin.Add("FLocalCurrId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", "HLTX01_SYS");
                    InStockFin.Add("FExchangeTypeId", basedata);

                    InStockFin.Add("FExchangeRate", 1.0);
                    InStockFin.Add("FISPRICEEXCLUDETAX", true);

                    // 开始构建单据体参数：集合参数JArray
                    JArray entryRows = new JArray();
                    // 把单据体行集合，添加到model中，以单据体Key为标识
                    string entityKey = "FInStockEntry";
                    model.Add(entityKey, entryRows);

                    // 通过循环创建单据体行：示例代码仅创建一行
                    for (int i = 0; i < pDataTable.Rows.Count; i++)
                    {
                        // 添加新行，把新行加入到单据体行集合
                        JObject entryRow = new JObject();
                        entryRows.Add(entryRow);

                        // 单据体主键：必须填写，系统据此判断是新增还是修改行
                        entryRow.Add("FEntryID", 0);

                        entryRow.Add("FRowType", "Standard");
                        //物料(FMaterialId)：基础资料，填写编码
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["物料编码"].ToString());
                        entryRow.Add("FMaterialId", basedata);

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["单位"].ToString());
                        entryRow.Add("FUnitID", basedata);
                        entryRow.Add("FRealQty", decimal.Parse(pDataTable.Rows[i]["入库数量"].ToString()));//实收数量
                        entryRow.Add("FMUSTQTY", decimal.Parse(pDataTable.Rows[i]["剩余入库数量"].ToString()));//应收数量

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["单位"].ToString());
                        entryRow.Add("FPriceUnitID", basedata);
                        string lot = pDataTable.Rows[i]["批号"].ToString().Trim();
                        if (lot == "0")
                        {
                            lot = "";
                        }
                        basedata = new JObject();
                        basedata.Add("FNumber", lot);//批号
                        entryRow.Add("FLot", basedata);
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["仓库"].ToString());//------------
                        entryRow.Add("FStockId", basedata);

                        //仓位
                        string cw = pDataTable.Rows[i]["FV"].ToString().Trim();
                        if (cw != "0")
                        {
                            JObject sp = new JObject();
                            sp.Add("FNumber", pDataTable.Rows[i]["仓位"].ToString());
                            basedata = new JObject();
                            basedata.Add("FSTOCKLOCID__FF" + pDataTable.Rows[i]["FVV"].ToString(), sp);
                            entryRow.Add("FStockLocId", basedata);
                        }

                        basedata = new JObject();
                        basedata.Add("FNumber", "KCZT01_SYS");
                        entryRow.Add("FStockStatusId", basedata);

                        entryRow.Add("FGiveAway", false);
                        entryRow.Add("FNote", "PDA InStock");

                        basedata = new JObject();
                        basedata.Add("FNumber", "bag");
                        entryRow.Add("FExtAuxUnitId", basedata);

                        entryRow.Add("FExtAuxUnitQty", 0.05);
                        entryRow.Add("FCheckInComing", false);
                        entryRow.Add("FIsReceiveUpdateStock", false);
                        entryRow.Add("FPriceBaseQty", "");

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["单位"].ToString());
                        entryRow.Add("FRemainInStockUnitId", basedata);

                        entryRow.Add("FBILLINGCLOSE", "false");
                        entryRow.Add("FRemainInStockQty", decimal.Parse(pDataTable.Rows[i]["入库数量"].ToString()));//采购数量
                        //entryRow.Add("FAPNotJoinQty", 0);//未关联应付数量（计价单位）
                        entryRow.Add("FRemainInStockBaseQty", decimal.Parse(pDataTable.Rows[i]["入库数量"].ToString()));//采购基本数量
                        entryRow.Add("FAuxUnitQty", "");//数量（库存辅单位）

                        //创建与源单之间的关联关系，以支持上查与反写源单
                        entryRow.Add("FSrcBillTypeId", "PUR_PurchaseOrder");
                        entryRow.Add("FSRCBILLNO", pDataTable.Rows[i]["采购订单号"].ToString());

                        JArray linkRows = new JArray();
                        string linkEntityKey = string.Format("{0}_Link", entityKey);
                        entryRow.Add(linkEntityKey, linkRows);

                        JObject linkRow = new JObject();
                        linkRows.Add(linkRow);

                        //FFlowId : 业务流程图，可选
                        string fldFlowIdKey = string.Format("{0}_FFlowId", linkEntityKey);
                        linkRow.Add(fldFlowIdKey, "");
                        //FFlowLineId ：业务流程图路线，可选
                        string fldFlowLineIdKey = string.Format("{0}_FFlowLineId", linkEntityKey);
                        linkRow.Add(fldFlowLineIdKey, "");

                        string fldRuleIdKey = string.Format("{0}_FRuleId", linkEntityKey);
                        linkRow.Add(fldRuleIdKey, "PUR_PurchaseOrder-STK_InStock");
                        string fldSTableNameKey = string.Format("{0}_FSTableName", linkEntityKey);
                        linkRow.Add(fldSTableNameKey, "t_PUR_POOrderEntry");

                        string fldSBillIdKey = string.Format("{0}_FSBillId", linkEntityKey);
                        linkRow.Add(fldSBillIdKey, int.Parse(pDataTable.Rows[i]["FID"].ToString()));
                        string fldSIdKey = string.Format("{0}_FSId", linkEntityKey);
                        linkRow.Add(fldSIdKey, int.Parse(pDataTable.Rows[i]["FENTRYID"].ToString()));

                        string fldSFStockInQtyKey = string.Format("{0}_FStockInQty", linkEntityKey);
                        linkRow.Add(fldSFStockInQtyKey, decimal.Parse(pDataTable.Rows[i]["入库数量"].ToString()));
                        string fldFSTOCKBASESTOCKINQTYKey = string.Format("{0}_FSTOCKBASESTOCKINQTY", linkEntityKey);
                        linkRow.Add(fldFSTOCKBASESTOCKINQTYKey, decimal.Parse(pDataTable.Rows[i]["入库数量"].ToString()));

                        if (i > 0)
                            fsKeys += ",";
                        fsKeys += pDataTable.Rows[i]["fskey"].ToString();

                        lst.Add(pDataTable.Rows[i]["FENTRYID"].ToString(), float.Parse(pDataTable.Rows[i]["入库数量"].ToString()));
                    }

                    // 调用Web API接口服务，保存单据
                    BillNo = client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save", new object[] { "STK_InStock", jsonRoot.ToString() });
                    JObject jo = JObject.Parse(BillNo);

                    if (!jo["Result"]["ResponseStatus"]["IsSuccess"].Value<bool>())
                    {
                        BillNo = string.Empty;
                        for (int i = 0; i < ((IList)jo["Result"]["ResponseStatus"]["Errors"]).Count; i++)
                            BillNo += jo["Result"]["ResponseStatus"]["Errors"][i]["Message"].Value<string>() + "\r\n";//保存不成功返错误信息
                        //反写失败状态
                        UpdateXBT_DataFSBS(fsKeys, false);
                    }
                    else
                    {
                        BillNo = "ID:" + jo["Result"]["Id"].Value<string>() + ";Number:" + jo["Result"]["Number"].Value<string>();//保存成功返回入库单FID和单据编号FBILLNO

                        client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Submit", new object[] { "STK_InStock", "{\"CreateOrgId\":\"0\",\"Numbers\":[\"" + jo["Result"]["Number"].Value<string>() + "\"]}" });//根据入库单号提交单据
                        client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Audit", new object[] { "STK_InStock", "{\"CreateOrgId\":\"0\",\"Numbers\":[\"" + jo["Result"]["Number"].Value<string>() + "\"]}" });//根据入库单号审核单据

                        //反写成功状态
                        UpdateXBT_DataFSBS(fsKeys, true);
                        //反写采购订单关联数量
                        UpdateT_Pur_Poorderentry_R_QTY(lst);
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return BillNo;
        }
        /// <summary>
        /// 收料通知-采购入库
        /// </summary>
        /// <param name="pDataTable"></param>
        /// <returns></returns>
        private string STK_InStockR(DataTable pDataTable)
        {
            string BillNo = string.Empty;
            string fsKeys = string.Empty;
            Dictionary<string, float> lst = new Dictionary<string, float>();
            try
            {
                K3CloudApiClient client = new K3CloudApiClient(GlobalParameter.K3Inf.C_ERPADDRESS);
                bool login = client.Login(GlobalParameter.K3Inf.C_ZTID, GlobalParameter.K3Inf.C_USERNAME, GlobalParameter.K3Inf.C_PASSWORD, 2052);
                if (login)
                {
                    JObject jsonRoot = new JObject();
                    jsonRoot.Add("Creator", "PDA");
                    jsonRoot.Add("NeedUpDateFields", new JArray(""));
                    jsonRoot.Add("NeedReturnFields", new JArray(""));

                    jsonRoot.Add("IsDeleteEntry", "true");
                    jsonRoot.Add("SubSystemId", "");
                    jsonRoot.Add("IsVerifyBaseDataField", "false");
                    jsonRoot.Add("IsEntryBatchFill", "true");
                    jsonRoot.Add("ValidateFlag", "true");
                    jsonRoot.Add("NumberSearch", "true");
                    jsonRoot.Add("InterationFlags", "");
                    jsonRoot.Add("IsAutoSubmitAndAudit", "false");

                    // Model: 单据详细数据参数
                    JObject model = new JObject();
                    jsonRoot.Add("Model", model);

                    // 
                    model.Add("FID", 0);
                    // 单据类型
                    JObject basedata = new JObject();
                    basedata.Add("FNumber", "RKD01_SYS");
                    model.Add("FBillTypeID", basedata);
                    // 日期
                    model.Add("FDate", DateTime.Today);
                    //组织
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["收料组织"].ToString());
                    model.Add("FStockOrgId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["收料部门"].ToString());
                    model.Add("FStockDeptId", basedata);
                    //if (pDataTable.Rows[0]["操作员"].ToString() != string.Empty)
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["操作员"].ToString());//
                    model.Add("FStockerId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["收料组织"].ToString());
                    model.Add("FDemandOrgId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["收料组织"].ToString());
                    model.Add("FPurchaseOrgId", basedata);
                    // 供应商
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["供应商"].ToString());
                    model.Add("FSupplierId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["供应商"].ToString());
                    model.Add("FSupplyId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["供应商"].ToString());
                    model.Add("FSettleId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["供应商"].ToString());
                    model.Add("FChargeId", basedata);

                    model.Add("FOwnerTypeIdHead", "BD_OwnerOrg");
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["收料组织"].ToString());
                    model.Add("FOwnerIdHead", basedata);

                    //model.Add("F_asd_Suppliershort", "HYG");
                    model.Add("FSupplierBillNo", "1");


                    //FPOOrderFinance
                    JObject InStockFin = new JObject();
                    model.Add("FInStockFin", InStockFin);

                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["收料组织"].ToString());
                    InStockFin.Add("FSettleOrgId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", "PRE001");
                    InStockFin.Add("FSettleCurrId", basedata);

                    InStockFin.Add("FIsIncludedTax", true);
                    InStockFin.Add("FPriceTimePoint", 1);

                    basedata = new JObject();
                    basedata.Add("FNumber", "PRE001");
                    InStockFin.Add("FLocalCurrId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", "HLTX01_SYS");
                    InStockFin.Add("FExchangeTypeId", basedata);

                    InStockFin.Add("FExchangeRate", 1.0);
                    InStockFin.Add("FISPRICEEXCLUDETAX", true);

                    // 开始构建单据体参数：集合参数JArray
                    JArray entryRows = new JArray();
                    // 把单据体行集合，添加到model中，以单据体Key为标识
                    string entityKey = "FInStockEntry";
                    model.Add(entityKey, entryRows);

                    // 通过循环创建单据体行：示例代码仅创建一行
                    for (int i = 0; i < pDataTable.Rows.Count; i++)
                    {
                        // 添加新行，把新行加入到单据体行集合
                        JObject entryRow = new JObject();
                        entryRows.Add(entryRow);

                        // 单据体主键：必须填写，系统据此判断是新增还是修改行
                        entryRow.Add("FEntryID", 0);

                        entryRow.Add("FRowType", "Standard");
                        //物料(FMaterialId)：基础资料，填写编码
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["物料编码"].ToString());
                        entryRow.Add("FMaterialId", basedata);

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["单位"].ToString());
                        entryRow.Add("FUnitID", basedata);
                        entryRow.Add("FRealQty", decimal.Parse(pDataTable.Rows[i]["入库数量"].ToString()));//实收数量
                        entryRow.Add("FMUSTQTY", decimal.Parse(pDataTable.Rows[i]["剩余入库数量"].ToString()));//应收数量

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["单位"].ToString());
                        entryRow.Add("FPriceUnitID", basedata);
                        string lot = pDataTable.Rows[i]["批号"].ToString().Trim();
                        if (lot == "0")
                        {
                            lot = "";
                        }
                        basedata = new JObject();
                        basedata.Add("FNumber", lot);//批号
                        entryRow.Add("FLot", basedata);
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["仓库"].ToString());//------------
                        entryRow.Add("FStockId", basedata);

                        //仓位
                        string cw = pDataTable.Rows[i]["FV"].ToString().Trim();
                        if (cw != "0")
                        {
                            JObject sp = new JObject();
                            sp.Add("FNumber", pDataTable.Rows[i]["仓位"].ToString());
                            basedata = new JObject();
                            basedata.Add("FSTOCKLOCID__FF" + pDataTable.Rows[i]["FVV"].ToString(), sp);
                            entryRow.Add("FStockLocId", basedata);
                        }

                        basedata = new JObject();
                        basedata.Add("FNumber", "KCZT01_SYS");
                        entryRow.Add("FStockStatusId", basedata);

                        entryRow.Add("FGiveAway", false);
                        entryRow.Add("FNote", "PDA InStock");

                        basedata = new JObject();
                        basedata.Add("FNumber", "bag");
                        entryRow.Add("FExtAuxUnitId", basedata);

                        entryRow.Add("FExtAuxUnitQty", 0.05);
                        entryRow.Add("FCheckInComing", false);
                        entryRow.Add("FIsReceiveUpdateStock", false);
                        entryRow.Add("FPriceBaseQty", "");

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["单位"].ToString());
                        entryRow.Add("FRemainInStockUnitId", basedata);

                        entryRow.Add("FBILLINGCLOSE", "false");
                        entryRow.Add("FRemainInStockQty", decimal.Parse(pDataTable.Rows[i]["入库数量"].ToString()));//采购数量
                        //entryRow.Add("FAPNotJoinQty", 0);//未关联应付数量（计价单位）
                        entryRow.Add("FRemainInStockBaseQty", decimal.Parse(pDataTable.Rows[i]["入库数量"].ToString()));//采购基本数量
                        entryRow.Add("FAuxUnitQty", "");//数量（库存辅单位）

                        //创建与源单之间的关联关系，以支持上查与反写源单
                        entryRow.Add("FSrcBillTypeId", "PUR_ReceiveBill");
                        entryRow.Add("FSRCBILLNO", pDataTable.Rows[i]["收料通知单号"].ToString());

                        JArray linkRows = new JArray();
                        string linkEntityKey = string.Format("{0}_Link", entityKey);
                        entryRow.Add(linkEntityKey, linkRows);

                        JObject linkRow = new JObject();
                        linkRows.Add(linkRow);

                        //FFlowId : 业务流程图，可选
                        string fldFlowIdKey = string.Format("{0}_FFlowId", linkEntityKey);
                        linkRow.Add(fldFlowIdKey, "");
                        //FFlowLineId ：业务流程图路线，可选
                        string fldFlowLineIdKey = string.Format("{0}_FFlowLineId", linkEntityKey);
                        linkRow.Add(fldFlowLineIdKey, "");

                        string fldRuleIdKey = string.Format("{0}_FRuleId", linkEntityKey);
                        linkRow.Add(fldRuleIdKey, "PUR_ReceiveBill-STK_InStock");
                        string fldSTableNameKey = string.Format("{0}_FSTableName", linkEntityKey);
                        linkRow.Add(fldSTableNameKey, "T_PUR_ReceiveEntry");

                        string fldSBillIdKey = string.Format("{0}_FSBillId", linkEntityKey);
                        linkRow.Add(fldSBillIdKey, int.Parse(pDataTable.Rows[i]["FID"].ToString()));
                        string fldSIdKey = string.Format("{0}_FSId", linkEntityKey);
                        linkRow.Add(fldSIdKey, int.Parse(pDataTable.Rows[i]["FENTRYID"].ToString()));

                        string fldSFStockInQtyKey = string.Format("{0}_FStockInQty", linkEntityKey);
                        linkRow.Add(fldSFStockInQtyKey, decimal.Parse(pDataTable.Rows[i]["入库数量"].ToString()));
                        string fldFSTOCKBASESTOCKINQTYKey = string.Format("{0}_FSTOCKBASESTOCKINQTY", linkEntityKey);
                        linkRow.Add(fldFSTOCKBASESTOCKINQTYKey, decimal.Parse(pDataTable.Rows[i]["入库数量"].ToString()));


                        if (i > 0)
                            fsKeys += ",";
                        fsKeys += pDataTable.Rows[i]["fskey"].ToString();

                        lst.Add(pDataTable.Rows[i]["FENTRYID"].ToString(), float.Parse(pDataTable.Rows[i]["入库数量"].ToString()));
                    }

                    // 调用Web API接口服务，保存单据
                    BillNo = client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save", new object[] { "STK_InStock", jsonRoot.ToString() });
                    JObject jo = JObject.Parse(BillNo);

                    if (!jo["Result"]["ResponseStatus"]["IsSuccess"].Value<bool>())
                    {
                        BillNo = string.Empty;
                        for (int i = 0; i < ((IList)jo["Result"]["ResponseStatus"]["Errors"]).Count; i++)
                            BillNo += jo["Result"]["ResponseStatus"]["Errors"][i]["Message"].Value<string>() + "\r\n";//保存不成功返错误信息

                        //反写失败状态
                        UpdateXBT_DataFSBS(fsKeys, false);
                    }
                    else
                    {
                        BillNo = "ID:" + jo["Result"]["Id"].Value<string>() + ";Number:" + jo["Result"]["Number"].Value<string>();//保存成功返回入库单FID和单据编号FBILLNO

                        client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Submit", new object[] { "STK_InStock", "{\"CreateOrgId\":\"0\",\"Numbers\":[\"" + jo["Result"]["Number"].Value<string>() + "\"]}" });//根据入库单号提交单据
                        client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Audit", new object[] { "STK_InStock", "{\"CreateOrgId\":\"0\",\"Numbers\":[\"" + jo["Result"]["Number"].Value<string>() + "\"]}" });//根据入库单号审核单据

                        //反写成功状态
                        UpdateXBT_DataFSBS(fsKeys, true);

                        //反写收料通知单入库数量
                        UpdateT_Pur_ReceiveEntry_S_QTY(lst);
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return BillNo;
        }
        /// <summary>
        /// 采购入库-采购退料
        /// </summary>
        /// <param name="pDataTable"></param>
        /// <returns></returns>
        private string PUR_MRB(DataTable pDataTable)
        {
            string BillNo = string.Empty;
            string fsKeys = string.Empty;
            Dictionary<string, float> lst = new Dictionary<string, float>();
            try
            {
                K3CloudApiClient client = new K3CloudApiClient(GlobalParameter.K3Inf.C_ERPADDRESS);
                bool login = client.Login(GlobalParameter.K3Inf.C_ZTID, GlobalParameter.K3Inf.C_USERNAME, GlobalParameter.K3Inf.C_PASSWORD, 2052);
                if (login)
                {
                    JObject jsonRoot = new JObject();
                    jsonRoot.Add("Creator", "PDA");
                    jsonRoot.Add("NeedUpDateFields", new JArray(""));
                    jsonRoot.Add("NeedReturnFields", new JArray(""));

                    jsonRoot.Add("IsDeleteEntry", "true");
                    jsonRoot.Add("SubSystemId", "");
                    jsonRoot.Add("IsVerifyBaseDataField", "false");
                    jsonRoot.Add("IsEntryBatchFill", "true");
                    jsonRoot.Add("ValidateFlag", "true");
                    jsonRoot.Add("NumberSearch", "true");
                    jsonRoot.Add("InterationFlags", "");
                    //jsonRoot.Add("IsAutoSubmitAndAudit", "false");

                    // Model: 单据详细数据参数
                    JObject model = new JObject();
                    jsonRoot.Add("Model", model);

                    // 
                    model.Add("FID", 0);
                    // 单据类型
                    JObject basedata = new JObject();
                    basedata.Add("FNumber", "TLD01_SYS");
                    model.Add("FBillTypeID", basedata);
                    // 日期
                    model.Add("FDate", DateTime.Today);
                    //
                    model.Add("FMRTYPE", "B");
                    model.Add("FMRMODE", "A");
                    //组织
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["退料组织"].ToString());
                    model.Add("FStockOrgId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["退料部门"].ToString());
                    model.Add("FMRDeptId", basedata);
                    //if (pDataTable.Rows[0]["操作员"].ToString() != string.Empty)
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["操作员"].ToString());//
                    model.Add("FStockerId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["退料组织"].ToString());
                    model.Add("FRequireOrgId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["退料组织"].ToString());
                    model.Add("FPurchaseOrgId", basedata);
                    // 供应商
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["供应商"].ToString());
                    model.Add("FSupplierID", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["供应商"].ToString());
                    model.Add("FACCEPTORID", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["供应商"].ToString());
                    model.Add("FSettleId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["供应商"].ToString());
                    model.Add("FCHARGEID", basedata);

                    model.Add("FOwnerTypeIdHead", "BD_OwnerOrg");
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["退料组织"].ToString());
                    model.Add("FOwnerIdHead", basedata);

                    //model.Add("FSupplierBillNo", "1");

                    //FPURMRBFIN
                    JObject InStockFin = new JObject();
                    model.Add("FPURMRBFIN", InStockFin);

                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["退料组织"].ToString());
                    InStockFin.Add("FSettleOrgId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", "PRE001");
                    InStockFin.Add("FSettleCurrId", basedata);

                    InStockFin.Add("FIsIncludedTax", true);
                    InStockFin.Add("FPRICETIMEPOINT", 1);

                    basedata = new JObject();
                    basedata.Add("FNumber", "PRE001");
                    InStockFin.Add("FLOCALCURRID", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", "HLTX01_SYS");
                    InStockFin.Add("FEXCHANGETYPEID", basedata);

                    InStockFin.Add("FEXCHANGERATE", 1.0);
                    InStockFin.Add("FISPRICEEXCLUDETAX", true);

                    // 开始构建单据体参数：集合参数JArray
                    JArray entryRows = new JArray();
                    // 把单据体行集合，添加到model中，以单据体Key为标识
                    string entityKey = "FPURMRBENTRY";
                    model.Add(entityKey, entryRows);

                    // 通过循环创建单据体行：示例代码仅创建一行
                    for (int i = 0; i < pDataTable.Rows.Count; i++)
                    {
                        // 添加新行，把新行加入到单据体行集合
                        JObject entryRow = new JObject();
                        entryRows.Add(entryRow);

                        // 单据体主键：必须填写，系统据此判断是新增还是修改行
                        entryRow.Add("FEntryID", 0);

                        entryRow.Add("FRowType", "Standard");
                        //物料(FMaterialId)：基础资料，填写编码
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["物料编码"].ToString());
                        entryRow.Add("FMATERIALID", basedata);

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["单位"].ToString());
                        entryRow.Add("FUnitID", basedata);
                        entryRow.Add("FRMREALQTY", decimal.Parse(pDataTable.Rows[i]["实退数量"].ToString()));//
                        entryRow.Add("FREPLENISHQTY", decimal.Parse(pDataTable.Rows[i]["实退数量"].ToString()));//
                        entryRow.Add("FKEAPAMTQTY", decimal.Parse(pDataTable.Rows[i]["实退数量"].ToString()));//
                        entryRow.Add("FPriceBaseQty", decimal.Parse(pDataTable.Rows[i]["实退数量"].ToString()));//

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["单位"].ToString());
                        entryRow.Add("FPRICEUNITID", basedata);
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["单位"].ToString());
                        entryRow.Add("FCarryUnitId", basedata);
                        entryRow.Add("FCarryQty", decimal.Parse(pDataTable.Rows[i]["实退数量"].ToString()));//
                        entryRow.Add("FCarryBaseQty", decimal.Parse(pDataTable.Rows[i]["实退数量"].ToString()));//
                        //string lot = pDataTable.Rows[i]["批号"].ToString().Trim();
                        //if (lot == "0")
                        //{
                        //    lot = "";
                        //}
                        //basedata = new JObject();
                        //basedata.Add("FNumber", lot);//批号
                        //entryRow.Add("FLot", basedata);
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["仓库"].ToString());//------------
                        entryRow.Add("FSTOCKID", basedata);
                        basedata = new JObject();
                        basedata.Add("FNumber", "KCZT01_SYS");//------------
                        entryRow.Add("FStockStatusId", basedata);

                        //仓位
                        string cw = pDataTable.Rows[i]["FV"].ToString().Trim();
                        if (cw != "0")
                        {
                            JObject sp = new JObject();
                            sp.Add("FNumber", pDataTable.Rows[i]["仓位"].ToString());
                            basedata = new JObject();
                            basedata.Add("FSTOCKLOCID__FF" + pDataTable.Rows[i]["FVV"].ToString(), sp);
                            entryRow.Add("FStockLocId", basedata);
                        }


                        //创建与源单之间的关联关系，以支持上查与反写源单
                        entryRow.Add("FSrcBillTypeId", "STK_InStock");
                        entryRow.Add("FSRCBILLNO", pDataTable.Rows[i]["采购入库单号"].ToString());

                        JArray linkRows = new JArray();
                        string linkEntityKey = string.Format("{0}_Link", entityKey);
                        entryRow.Add(linkEntityKey, linkRows);

                        JObject linkRow = new JObject();
                        linkRows.Add(linkRow);

                        //FFlowId : 业务流程图，可选
                        string fldFlowIdKey = string.Format("{0}_FFlowId", linkEntityKey);
                        linkRow.Add(fldFlowIdKey, "");
                        //FFlowLineId ：业务流程图路线，可选
                        string fldFlowLineIdKey = string.Format("{0}_FFlowLineId", linkEntityKey);
                        linkRow.Add(fldFlowLineIdKey, "");

                        string fldRuleIdKey = string.Format("{0}_FRuleId", linkEntityKey);
                        linkRow.Add(fldRuleIdKey, "STK_InStock-PUR_MRB");
                        string fldSTableNameKey = string.Format("{0}_FSTableName", linkEntityKey);
                        linkRow.Add(fldSTableNameKey, "T_STK_INSTOCKENTRY");

                        string fldSBillIdKey = string.Format("{0}_FSBillId", linkEntityKey);
                        linkRow.Add(fldSBillIdKey, int.Parse(pDataTable.Rows[i]["FID"].ToString()));
                        string fldSIdKey = string.Format("{0}_FSId", linkEntityKey);
                        linkRow.Add(fldSIdKey, int.Parse(pDataTable.Rows[i]["FENTRYID"].ToString()));

                        //string fldSFStockInQtyKey = string.Format("{0}_FStockInQty", linkEntityKey);
                        //linkRow.Add(fldSFStockInQtyKey, decimal.Parse(pDataTable.Rows[i]["实退数量"].ToString()));
                        //string fldFSTOCKBASESTOCKINQTYKey = string.Format("{0}_FSTOCKBASESTOCKINQTY", linkEntityKey);
                        //linkRow.Add(fldFSTOCKBASESTOCKINQTYKey, decimal.Parse(pDataTable.Rows[i]["实退数量"].ToString()));

                        string fldBaseQtyOldKey = string.Format("{0}_FBaseUnitQtyOld", linkEntityKey);
                        linkRow.Add(fldBaseQtyOldKey, decimal.Parse(pDataTable.Rows[i]["实退数量"].ToString()));
                        string fldBaseQtyKey = string.Format("{0}_FBaseUnitQty", linkEntityKey);
                        linkRow.Add(fldBaseQtyKey, decimal.Parse(pDataTable.Rows[i]["实退数量"].ToString()));


                        if (i > 0)
                            fsKeys += ",";
                        fsKeys += pDataTable.Rows[i]["fskey"].ToString();

                        lst.Add(pDataTable.Rows[i]["FENTRYID"].ToString(), float.Parse(pDataTable.Rows[i]["实退数量"].ToString()));
                    }

                    // 调用Web API接口服务，保存单据
                    BillNo = client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save", new object[] { "PUR_MRB", jsonRoot.ToString() });
                    JObject jo = JObject.Parse(BillNo);

                    if (!jo["Result"]["ResponseStatus"]["IsSuccess"].Value<bool>())
                    {
                        BillNo = string.Empty;
                        for (int i = 0; i < ((IList)jo["Result"]["ResponseStatus"]["Errors"]).Count; i++)
                            BillNo += jo["Result"]["ResponseStatus"]["Errors"][i]["Message"].Value<string>() + "\r\n";//保存不成功返错误信息

                        //反写失败状态
                        UpdateXBT_DataFSBS(fsKeys, false);
                    }
                    else
                    {
                        BillNo = "ID:" + jo["Result"]["Id"].Value<string>() + ";Number:" + jo["Result"]["Number"].Value<string>();//保存成功返回入库单FID和单据编号FBILLNO

                        client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Submit", new object[] { "PUR_MRB", "{\"CreateOrgId\":\"0\",\"Numbers\":[\"" + jo["Result"]["Number"].Value<string>() + "\"]}" });//根据单号提交单据
                        client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Audit", new object[] { "PUR_MRB", "{\"CreateOrgId\":\"0\",\"Numbers\":[\"" + jo["Result"]["Number"].Value<string>() + "\"]}" });//根据单号审核单据

                        //反写成功状态
                        UpdateXBT_DataFSBS(fsKeys, true);

                        //反写采购入库单 关联数量
                        UpdateT_Stk_instockEntry_QTY(lst);
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return BillNo;
        }
        /// <summary>
        /// 发货通知-销售出库
        /// </summary>
        /// <param name="pDataTable"></param>
        /// <returns></returns>
        private string SAL_OUTSTOCK(DataTable pDataTable)
        {
            string BillNo = string.Empty;
            string fsKeys = string.Empty;//发货通知单号,客户,发货组织,发货部门,物料编码,应发数量,实发数量,单位,仓库,仓位,批号,操作员,类型,用户,密码,fskey
            Dictionary<string, float> lst = new Dictionary<string, float>();
            try
            {
                K3CloudApiClient client = new K3CloudApiClient(GlobalParameter.K3Inf.C_ERPADDRESS);
                bool login = client.Login(GlobalParameter.K3Inf.C_ZTID, GlobalParameter.K3Inf.C_USERNAME, GlobalParameter.K3Inf.C_PASSWORD, 2052);
                if (login)
                {
                    JObject jsonRoot = new JObject();
                    jsonRoot.Add("Creator", "PDA");
                    jsonRoot.Add("NeedUpDateFields", new JArray(""));
                    jsonRoot.Add("NeedReturnFields", new JArray(""));

                    jsonRoot.Add("IsDeleteEntry", "true");
                    jsonRoot.Add("SubSystemId", "");
                    jsonRoot.Add("IsVerifyBaseDataField", "false");
                    jsonRoot.Add("IsEntryBatchFill", "true");
                    jsonRoot.Add("ValidateFlag", "true");
                    jsonRoot.Add("NumberSearch", "true");
                    jsonRoot.Add("InterationFlags", "");
                    jsonRoot.Add("IsAutoSubmitAndAudit", "false");

                    JObject model = new JObject();
                    jsonRoot.Add("Model", model);
                    model.Add("FID", 0);

                    JObject basedata = new JObject();
                    basedata.Add("FNumber", "XSCKD01_SYS");
                    model.Add("FBillTypeID", basedata);

                    // 日期
                    model.Add("FDate", DateTime.Today);

                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["发货组织"].ToString());
                    model.Add("FSaleOrgId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["客户"].ToString());
                    model.Add("FCustomerID", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["发货组织"].ToString());
                    model.Add("FStockOrgId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["发货部门"].ToString());
                    model.Add("FDeliveryDeptID", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["客户"].ToString());
                    model.Add("FReceiverID", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["客户"].ToString());
                    model.Add("FSettleID", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["客户"].ToString());
                    model.Add("FPayerID", basedata);

                    model.Add("FOwnerTypeIdHead", "BD_OwnerOrg");
                    model.Add("FIsTotalServiceOrCost", false);
                    model.Add("F_SWH_CheckBox", false);

                    //SubHeadEntity
                    JObject SubHeadEntity = new JObject();
                    model.Add("SubHeadEntity", SubHeadEntity);

                    basedata = new JObject();
                    basedata.Add("FNumber", "PRE001");
                    SubHeadEntity.Add("FSettleCurrID", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["发货组织"].ToString());
                    SubHeadEntity.Add("FSettleOrgID", basedata);

                    SubHeadEntity.Add("FIsIncludedTax", true);
                    basedata = new JObject();
                    basedata.Add("FNumber", "PRE001");
                    SubHeadEntity.Add("FLocalCurrID", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", "HLTX01_SYS");
                    SubHeadEntity.Add("FExchangeTypeID", basedata);

                    SubHeadEntity.Add("FExchangeRate", 1.0);
                    SubHeadEntity.Add("FIsPriceExcludeTax", true);

                    // 开始构建单据体参数：集合参数JArray
                    JArray entryRows = new JArray();
                    string entityKey = "FEntity";
                    model.Add(entityKey, entryRows);

                    for (int i = 0; i < pDataTable.Rows.Count; i++)
                    {
                        // 添加新行，把新行加入到单据体行集合
                        JObject entryRow = new JObject();
                        entryRows.Add(entryRow);

                        // 单据体主键：必须填写，系统据此判断是新增还是修改行
                        entryRow.Add("FEntryID", 0);

                        entryRow.Add("FRowType", "Standard");
                        //物料(FMaterialId)：基础资料，填写编码
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["物料编码"].ToString());
                        entryRow.Add("FMaterialID", basedata);

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["单位"].ToString());
                        entryRow.Add("FUnitID", basedata);

                        entryRow.Add("FRealQty", decimal.Parse(pDataTable.Rows[i]["实发数量"].ToString()));
                        entryRow.Add("FIsFree", false);
                        entryRow.Add("FOwnerTypeID", "BD_OwnerOrg");

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["发货组织"].ToString());
                        entryRow.Add("FOwnerID", basedata);

                        entryRow.Add("FEntryTaxRate", 13.00);
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["仓库"].ToString());
                        entryRow.Add("FStockID", basedata);
                        basedata = new JObject();
                        basedata.Add("FNumber", "KCZT01_SYS");
                        entryRow.Add("FStockStatusID", basedata);
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["单位"].ToString());
                        entryRow.Add("FSalUnitID", basedata);
                        
                        entryRow.Add("FSALUNITQTY", decimal.Parse(pDataTable.Rows[i]["实发数量"].ToString()));
                        entryRow.Add("FSALBASEQTY", decimal.Parse(pDataTable.Rows[i]["实发数量"].ToString()));
                        entryRow.Add("FPRICEBASEQTY", decimal.Parse(pDataTable.Rows[i]["实发数量"].ToString()));
                        entryRow.Add("FOUTCONTROL", false);
                        entryRow.Add("FIsOverLegalOrg", false);
                        entryRow.Add("FCarryBaseQty", decimal.Parse(pDataTable.Rows[i]["实发数量"].ToString()));

                        entryRow.Add("FMustQty", decimal.Parse(pDataTable.Rows[i]["应发数量"].ToString()));
                        entryRow.Add("FBaseMustQty", decimal.Parse(pDataTable.Rows[i]["应发数量"].ToString()));


                        entryRow.Add("FPrice", decimal.Parse(pDataTable.Rows[i]["单价"].ToString()));
                        //entryRow.Add("FEntryTaxRate", decimal.Parse(pDataTable.Rows[i]["税率"].ToString()));

                        //仓位
                        string cw = pDataTable.Rows[i]["FV"].ToString().Trim();
                        if (cw != "0")
                        {
                            JObject sp = new JObject();
                            sp.Add("FNumber", pDataTable.Rows[i]["仓位"].ToString());
                            basedata = new JObject();
                            basedata.Add("FSTOCKLOCID__FF" + pDataTable.Rows[i]["FVV"].ToString(), sp);
                            entryRow.Add("FStockLocId", basedata);
                        }

                        entryRow.Add("F_SWH_Text", pDataTable.Rows[i]["订单号"].ToString());
                        entryRow.Add("F_SWH_Text2", pDataTable.Rows[i]["非标尺寸"].ToString());


                        //创建与源单之间的关联关系，以支持上查与反写源单
                        entryRow.Add("FSrcBillTypeId", "SAL_DELIVERYNOTICE");
                        entryRow.Add("FSRCBILLNO", pDataTable.Rows[i]["发货通知单号"].ToString());

                        JArray linkRows = new JArray();
                        string linkEntityKey = string.Format("{0}_Link", entityKey);
                        entryRow.Add(linkEntityKey, linkRows);

                        JObject linkRow = new JObject();
                        linkRows.Add(linkRow);

                        //FFlowId : 业务流程图，可选
                        string fldFlowIdKey = string.Format("{0}_FFlowId", linkEntityKey);
                        linkRow.Add(fldFlowIdKey, "");
                        //FFlowLineId ：业务流程图路线，可选
                        string fldFlowLineIdKey = string.Format("{0}_FFlowLineId", linkEntityKey);
                        linkRow.Add(fldFlowLineIdKey, "");

                        string fldRuleIdKey = string.Format("{0}_FRuleId", linkEntityKey);
                        linkRow.Add(fldRuleIdKey, "SAL_DELIVERYNOTICE-SAL_OUTSTOCK");
                        string fldSTableNameKey = string.Format("{0}_FSTableName", linkEntityKey);
                        linkRow.Add(fldSTableNameKey, "T_SAL_DELIVERYNOTICEENTRY");

                        string fldSBillIdKey = string.Format("{0}_FSBillId", linkEntityKey);
                        linkRow.Add(fldSBillIdKey, int.Parse(pDataTable.Rows[i]["FID"].ToString()));
                        string fldSIdKey = string.Format("{0}_FSId", linkEntityKey);
                        linkRow.Add(fldSIdKey, int.Parse(pDataTable.Rows[i]["FENTRYID"].ToString()));

                        string fldBaseQtyOldKey = string.Format("{0}_FBaseUnitQtyOld", linkEntityKey);
                        linkRow.Add(fldBaseQtyOldKey, decimal.Parse(pDataTable.Rows[i]["实发数量"].ToString()));
                        string fldBaseQtyKey = string.Format("{0}_FBaseUnitQty", linkEntityKey);
                        linkRow.Add(fldBaseQtyKey, decimal.Parse(pDataTable.Rows[i]["实发数量"].ToString()));

                        //string fldSFStockInQtyKey = string.Format("{0}_FStockInQty", linkEntityKey);
                        //linkRow.Add(fldSFStockInQtyKey, decimal.Parse(pDataTable.Rows[i]["实发数量"].ToString()));
                        //string fldFSTOCKBASESTOCKINQTYKey = string.Format("{0}_FSTOCKBASESTOCKINQTY", linkEntityKey);
                        //linkRow.Add(fldFSTOCKBASESTOCKINQTYKey, decimal.Parse(pDataTable.Rows[i]["实发数量"].ToString()));

                        if (i > 0)
                            fsKeys += ",";
                        fsKeys += pDataTable.Rows[i]["fskey"].ToString();

                        lst.Add(pDataTable.Rows[i]["FENTRYID"].ToString(), float.Parse(pDataTable.Rows[i]["实发数量"].ToString()));
                    }

                    // 调用Web API接口服务，保存单据
                    BillNo = client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save", new object[] { "SAL_OUTSTOCK", jsonRoot.ToString() });
                    JObject jo = JObject.Parse(BillNo);

                    if (!jo["Result"]["ResponseStatus"]["IsSuccess"].Value<bool>())
                    {
                        BillNo = string.Empty;
                        for (int i = 0; i < ((IList)jo["Result"]["ResponseStatus"]["Errors"]).Count; i++)
                            BillNo += jo["Result"]["ResponseStatus"]["Errors"][i]["Message"].Value<string>() + "\r\n";//保存不成功返错误信息

                        //反写失败状态
                        UpdateXBT_DataFSBS(fsKeys, false);
                    }
                    else
                    {
                        BillNo = "ID:" + jo["Result"]["Id"].Value<string>() + ";Number:" + jo["Result"]["Number"].Value<string>();//保存成功返回入库单FID和单据编号FBILLNO

                        client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Submit", new object[] { "SAL_OUTSTOCK", "{\"CreateOrgId\":\"0\",\"Numbers\":[\"" + jo["Result"]["Number"].Value<string>() + "\"]}" });//根据单号提交单据
                        client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Audit", new object[] { "SAL_OUTSTOCK", "{\"CreateOrgId\":\"0\",\"Numbers\":[\"" + jo["Result"]["Number"].Value<string>() + "\"]}" });//根据单号审核单据

                        //反写成功状态
                        UpdateXBT_DataFSBS(fsKeys, true);

                        //反写发货通知单关联数量
                        UpdateT_SAL_DELIVERYNOTICEENTRY_QTY(lst);
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return BillNo;
        }
        /// <summary>
        /// 销售出库-销售退货
        /// </summary>
        /// <param name="pDataTable"></param>
        /// <returns></returns>
        private string SAL_RETURNSTOCK(DataTable pDataTable)
        {
            string BillNo = string.Empty;
            string fsKeys = string.Empty;//销售出库单号,客户,销售组织,销售部门,物料编码,数量,实退数量,单位,仓库,仓位,批号,操作员,类型,用户,密码,fskey
            Dictionary<string, float> lst = new Dictionary<string, float>();
            try
            {
                K3CloudApiClient client = new K3CloudApiClient(GlobalParameter.K3Inf.C_ERPADDRESS);
                bool login = client.Login(GlobalParameter.K3Inf.C_ZTID, GlobalParameter.K3Inf.C_USERNAME, GlobalParameter.K3Inf.C_PASSWORD, 2052);
                if (login)
                {
                    JObject jsonRoot = new JObject();
                    jsonRoot.Add("Creator", "PDA");
                    jsonRoot.Add("NeedUpDateFields", new JArray(""));
                    jsonRoot.Add("NeedReturnFields", new JArray(""));

                    jsonRoot.Add("IsDeleteEntry", "true");
                    jsonRoot.Add("SubSystemId", "");
                    jsonRoot.Add("IsVerifyBaseDataField", "false");
                    jsonRoot.Add("IsEntryBatchFill", "true");
                    jsonRoot.Add("ValidateFlag", "true");
                    jsonRoot.Add("NumberSearch", "true");
                    jsonRoot.Add("InterationFlags", "");
                    jsonRoot.Add("IsAutoSubmitAndAudit", "false");

                    JObject model = new JObject();
                    jsonRoot.Add("Model", model);
                    model.Add("FID", 0);

                    JObject basedata = new JObject();
                    basedata.Add("FNumber", "XSTHD01_SYS");
                    model.Add("FBillTypeID", basedata);

                    // 日期
                    model.Add("FDate", DateTime.Today);

                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["销售组织"].ToString());
                    model.Add("FSaleOrgId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["客户"].ToString());
                    model.Add("FRetcustId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", "OverOrgSal");
                    model.Add("FTransferBizType", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["销售组织"].ToString());
                    model.Add("FStockOrgId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["客户"].ToString());
                    model.Add("FReceiveCustId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["客户"].ToString());
                    model.Add("FSettleCustId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["客户"].ToString());
                    model.Add("FPayCustId", basedata);

                    model.Add("FOwnerTypeIdHead", "BD_OwnerOrg");
                    model.Add("FIsTotalServiceOrCost", false);

                    //SubHeadEntity
                    JObject SubHeadEntity = new JObject();
                    model.Add("SubHeadEntity", SubHeadEntity);

                    basedata = new JObject();
                    basedata.Add("FNumber", "PRE001");
                    SubHeadEntity.Add("FSettleCurrId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["销售组织"].ToString());
                    SubHeadEntity.Add("FSettleOrgId", basedata);
                    
                    basedata = new JObject();
                    basedata.Add("FNumber", "PRE001");
                    SubHeadEntity.Add("FLocalCurrId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", "HLTX01_SYS");
                    SubHeadEntity.Add("FExchangeTypeId", basedata);

                    SubHeadEntity.Add("FExchangeRate", 1.0);

                    // 开始构建单据体参数：集合参数JArray
                    JArray entryRows = new JArray();
                    string entityKey = "FEntity";
                    model.Add(entityKey, entryRows);

                    for (int i = 0; i < pDataTable.Rows.Count; i++)
                    {
                        // 添加新行，把新行加入到单据体行集合
                        JObject entryRow = new JObject();
                        entryRows.Add(entryRow);

                        // 单据体主键：必须填写，系统据此判断是新增还是修改行
                        entryRow.Add("FEntryID", 0);

                        entryRow.Add("FRowType", "Standard");
                        //物料(FMaterialId)：基础资料，填写编码
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["物料编码"].ToString());
                        entryRow.Add("FMaterialId", basedata);

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["单位"].ToString());
                        entryRow.Add("FUnitID", basedata);

                        entryRow.Add("FRealQty", decimal.Parse(pDataTable.Rows[i]["实退数量"].ToString()));
                        entryRow.Add("FIsFree", false);
                        entryRow.Add("FEntryTaxRate", 13.00);

                        basedata = new JObject();
                        basedata.Add("FNumber", "THLX01_SYS");
                        entryRow.Add("FReturnType", basedata);

                        entryRow.Add("FOwnerTypeId", "BD_OwnerOrg");

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["销售组织"].ToString());
                        entryRow.Add("FOwnerId", basedata);

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["仓库"].ToString());
                        entryRow.Add("FStockId", basedata);
                        basedata = new JObject();
                        basedata.Add("FNumber", "KCZT01_SYS");
                        entryRow.Add("FStockstatusId", basedata);

                        entryRow.Add("FDeliveryDate", DateTime.Today);

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["单位"].ToString());
                        entryRow.Add("FSalUnitID", basedata);

                        entryRow.Add("FSalUnitQty", decimal.Parse(pDataTable.Rows[i]["实退数量"].ToString()));
                        entryRow.Add("FSalBaseQty", decimal.Parse(pDataTable.Rows[i]["实退数量"].ToString()));
                        entryRow.Add("FPriceBaseQty", decimal.Parse(pDataTable.Rows[i]["实退数量"].ToString()));
                        entryRow.Add("FIsOverLegalOrg", false);
                        entryRow.Add("FARNOTJOINQTY", decimal.Parse(pDataTable.Rows[i]["实退数量"].ToString()));
                        entryRow.Add("FIsReturnCheck", false);

                        //仓位
                        string cw = pDataTable.Rows[i]["FV"].ToString().Trim();
                        if (cw != "0")
                        {
                            JObject sp = new JObject();
                            sp.Add("FNumber", pDataTable.Rows[i]["仓位"].ToString());
                            basedata = new JObject();
                            basedata.Add("FSTOCKLOCID__FF" + pDataTable.Rows[i]["FVV"].ToString(), sp);
                            entryRow.Add("FStockLocId", basedata);
                        }


                        //创建与源单之间的关联关系，以支持上查与反写源单
                        entryRow.Add("FSrcBillTypeId", "SAL_OUTSTOCK");
                        entryRow.Add("FSRCBILLNO", pDataTable.Rows[i]["销售出库单号"].ToString());

                        JArray linkRows = new JArray();
                        string linkEntityKey = string.Format("{0}_Link", entityKey);
                        entryRow.Add(linkEntityKey, linkRows);

                        JObject linkRow = new JObject();
                        linkRows.Add(linkRow);

                        //FFlowId : 业务流程图，可选
                        string fldFlowIdKey = string.Format("{0}_FFlowId", linkEntityKey);
                        linkRow.Add(fldFlowIdKey, "");
                        //FFlowLineId ：业务流程图路线，可选
                        string fldFlowLineIdKey = string.Format("{0}_FFlowLineId", linkEntityKey);
                        linkRow.Add(fldFlowLineIdKey, "");

                        string fldRuleIdKey = string.Format("{0}_FRuleId", linkEntityKey);
                        linkRow.Add(fldRuleIdKey, "SAL_OUTSTOCK-SAL_RETURNSTOCK");
                        string fldSTableNameKey = string.Format("{0}_FSTableName", linkEntityKey);
                        linkRow.Add(fldSTableNameKey, "T_SAL_OUTSTOCKENTRY");

                        string fldSBillIdKey = string.Format("{0}_FSBillId", linkEntityKey);
                        linkRow.Add(fldSBillIdKey, int.Parse(pDataTable.Rows[i]["FID"].ToString()));
                        string fldSIdKey = string.Format("{0}_FSId", linkEntityKey);
                        linkRow.Add(fldSIdKey, int.Parse(pDataTable.Rows[i]["FENTRYID"].ToString()));

                        //string fldBaseQtyOldKey = string.Format("{0}_FBaseUnitQtyOld", linkEntityKey);
                        //linkRow.Add(fldBaseQtyOldKey, decimal.Parse(pDataTable.Rows[i]["实退数量"].ToString()));
                        //string fldBaseQtyKey = string.Format("{0}_FBaseUnitQty", linkEntityKey);
                        //linkRow.Add(fldBaseQtyKey, decimal.Parse(pDataTable.Rows[i]["实退数量"].ToString()));

                        if (i > 0)
                            fsKeys += ",";
                        fsKeys += pDataTable.Rows[i]["fskey"].ToString();

                        lst.Add(pDataTable.Rows[i]["FENTRYID"].ToString(), float.Parse(pDataTable.Rows[i]["实退数量"].ToString()));
                    }

                    // 调用Web API接口服务，保存单据
                    BillNo = client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save", new object[] { "SAL_RETURNSTOCK", jsonRoot.ToString() });
                    JObject jo = JObject.Parse(BillNo);

                    if (!jo["Result"]["ResponseStatus"]["IsSuccess"].Value<bool>())
                    {
                        BillNo = string.Empty;
                        for (int i = 0; i < ((IList)jo["Result"]["ResponseStatus"]["Errors"]).Count; i++)
                            BillNo += jo["Result"]["ResponseStatus"]["Errors"][i]["Message"].Value<string>() + "\r\n";//保存不成功返错误信息

                        //反写失败状态
                        UpdateXBT_DataFSBS(fsKeys, false);
                    }
                    else
                    {
                        BillNo = "ID:" + jo["Result"]["Id"].Value<string>() + ";Number:" + jo["Result"]["Number"].Value<string>();//保存成功返回入库单FID和单据编号FBILLNO

                        client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Submit", new object[] { "SAL_RETURNSTOCK", "{\"CreateOrgId\":\"0\",\"Numbers\":[\"" + jo["Result"]["Number"].Value<string>() + "\"]}" });//根据单号提交单据
                        client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Audit", new object[] { "SAL_RETURNSTOCK", "{\"CreateOrgId\":\"0\",\"Numbers\":[\"" + jo["Result"]["Number"].Value<string>() + "\"]}" });//根据单号审核单据

                        //反写成功状态
                        UpdateXBT_DataFSBS(fsKeys, true);

                        //反写销售出库关联数量
                        UpdateT_SAL_OUTSTOCKENTRY_R_QTY(lst);
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return BillNo;
        }
        /// <summary>
        /// 生产入库
        /// </summary>
        /// <param name="pDataTable"></param>
        /// <returns></returns>
        public string PRD_INSTOCK(DataTable pDataTable)
        {
            string BillNo = string.Empty;
            string fsKeys = string.Empty;//
            Dictionary<string, float> lst = new Dictionary<string, float>();
            try
            {
                K3CloudApiClient client = new K3CloudApiClient(GlobalParameter.K3Inf.C_ERPADDRESS);
                bool login = client.Login(GlobalParameter.K3Inf.C_ZTID, GlobalParameter.K3Inf.C_USERNAME, GlobalParameter.K3Inf.C_PASSWORD, 2052);
                if (login)
                {
                    JObject jsonRoot = new JObject();
                    jsonRoot.Add("Creator", "PDA");
                    jsonRoot.Add("NeedUpDateFields", new JArray(""));
                    jsonRoot.Add("NeedReturnFields", new JArray(""));

                    jsonRoot.Add("IsDeleteEntry", "true");
                    jsonRoot.Add("SubSystemId", "");
                    jsonRoot.Add("IsVerifyBaseDataField", "false");
                    jsonRoot.Add("IsEntryBatchFill", "true");
                    jsonRoot.Add("ValidateFlag", "true");
                    jsonRoot.Add("NumberSearch", "true");
                    jsonRoot.Add("InterationFlags", "");
                    jsonRoot.Add("IsAutoSubmitAndAudit", "false");

                    JObject model = new JObject();
                    jsonRoot.Add("Model", model);
                    model.Add("FID", 0);

                    JObject basedata = new JObject();
                    basedata.Add("FNumber", "SCRKD02_SYS");
                    model.Add("FBillType", basedata);

                    // 日期
                    model.Add("FDate", DateTime.Today);

                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["入库组织"].ToString());
                    model.Add("FStockOrgId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["入库组织"].ToString());
                    model.Add("FPrdOrgId", basedata);

                    model.Add("FOwnerTypeId0", "BD_OwnerOrg");
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["货主"].ToString());
                    model.Add("FOwnerId0", basedata);

                    model.Add("FIsEntrust", false);

                    basedata = new JObject();
                    basedata.Add("FNumber", "PRE001");
                    model.Add("FCurrId", basedata);

                    // 开始构建单据体参数：集合参数JArray
                    JArray entryRows = new JArray();
                    string entityKey = "FEntity";
                    model.Add(entityKey, entryRows);

                    for (int i = 0; i < pDataTable.Rows.Count; i++)
                    {
                        // 添加新行，把新行加入到单据体行集合
                        JObject entryRow = new JObject();
                        entryRows.Add(entryRow);

                        // 单据体主键：必须填写，系统据此判断是新增还是修改行
                        //entryRow.Add("FEntryID", 0);
                        entryRow.Add("FIsNew", true);

                        //物料(FMaterialId)：基础资料，填写编码
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["物料编码"].ToString());
                        entryRow.Add("FMaterialId", basedata);

                        entryRow.Add("FCheckProduct", false);
                        entryRow.Add("FInStockType", "1");
                        entryRow.Add("FProductType", pDataTable.Rows[i]["产品类型"].ToString());

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["单位"].ToString());
                        entryRow.Add("FUnitID", basedata);

                        entryRow.Add("FMustQty", decimal.Parse(pDataTable.Rows[i]["应收数量"].ToString()));
                        entryRow.Add("FRealQty", decimal.Parse(pDataTable.Rows[i]["实收数量"].ToString()));
                        entryRow.Add("FCostRate", decimal.Parse(pDataTable.Rows[i]["FCOSTRATE"].ToString()));

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["单位"].ToString());
                        entryRow.Add("FBaseUnitId", basedata);

                        entryRow.Add("FBaseMustQty", decimal.Parse(pDataTable.Rows[i]["应收数量"].ToString()));
                        entryRow.Add("FBaseRealQty", decimal.Parse(pDataTable.Rows[i]["实收数量"].ToString()));

                        entryRow.Add("FOwnerTypeId", "BD_OwnerOrg");
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["货主"].ToString());
                        entryRow.Add("FOwnerId", basedata);

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["仓库"].ToString());
                        entryRow.Add("FStockId", basedata);

                        //仓位
                        string cw = pDataTable.Rows[i]["FV"].ToString().Trim();
                        if (cw != "0")
                        {
                            JObject sp = new JObject();
                            sp.Add("FNumber", pDataTable.Rows[i]["仓位"].ToString());
                            basedata = new JObject();
                            basedata.Add("FSTOCKLOCID__FF" + pDataTable.Rows[i]["FVV"].ToString(), sp);
                            entryRow.Add("FStockLocId", basedata);
                        }


                        entryRow.Add("FISBACKFLUSH", false);
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["部门"].ToString());
                        entryRow.Add("FWorkShopId1", basedata);

                        entryRow.Add("FMoBillNo", pDataTable.Rows[i]["生产订单号"].ToString());
                        entryRow.Add("FMoId", pDataTable.Rows[i]["FID"].ToString());
                        entryRow.Add("FMoEntryId", pDataTable.Rows[i]["FENTRYID"].ToString());
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["单位"].ToString());
                        entryRow.Add("FStockUnitId", basedata);


                        entryRow.Add("FStockRealQty", pDataTable.Rows[i]["实收数量"].ToString());

                        //entryRow.Add("FSrcBillType", "PRD_MO");
                        //entryRow.Add("FSrcBillNo", pDataTable.Rows[i]["生产订单号"].ToString());
                        entryRow.Add("FSrcInterId", pDataTable.Rows[i]["FID"].ToString());
                        entryRow.Add("FBasePrdRealQty", pDataTable.Rows[i]["实收数量"].ToString());
                        entryRow.Add("FIsFinished", false);
                        basedata = new JObject();
                        basedata.Add("FNumber", "KCZT01_SYS");
                        entryRow.Add("FStockStatusId", basedata);
                        entryRow.Add("FMOMAINENTRYID", pDataTable.Rows[i]["FENTRYID"].ToString());

                        entryRow.Add("FKeeperTypeId", "BD_KeeperOrg");
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["货主"].ToString());
                        entryRow.Add("FKeeperId", basedata);


                        //创建与源单之间的关联关系，以支持上查与反写源单
                        entryRow.Add("FSrcBillType", "PRD_MO");
                        entryRow.Add("FSrcBillNo", pDataTable.Rows[i]["生产订单号"].ToString());

                        JArray linkRows = new JArray();
                        string linkEntityKey = string.Format("{0}_Link", entityKey);
                        entryRow.Add(linkEntityKey, linkRows);

                        JObject linkRow = new JObject();
                        linkRows.Add(linkRow);

                        //FFlowId : 业务流程图，可选
                        string fldFlowIdKey = string.Format("{0}_FFlowId", linkEntityKey);
                        linkRow.Add(fldFlowIdKey, "");
                        //FFlowLineId ：业务流程图路线，可选
                        string fldFlowLineIdKey = string.Format("{0}_FFlowLineId", linkEntityKey);
                        linkRow.Add(fldFlowLineIdKey, "");

                        string fldRuleIdKey = string.Format("{0}_FRuleId", linkEntityKey);
                        linkRow.Add(fldRuleIdKey, "PRD_MO-PRD_INSTOCK");
                        string fldSTableNameKey = string.Format("{0}_FSTableName", linkEntityKey);
                        linkRow.Add(fldSTableNameKey, "T_PRD_MOENTRY");

                        string fldSBillIdKey = string.Format("{0}_FSBillId", linkEntityKey);
                        linkRow.Add(fldSBillIdKey, int.Parse(pDataTable.Rows[i]["FID"].ToString()));
                        string fldSIdKey = string.Format("{0}_FSId", linkEntityKey);
                        linkRow.Add(fldSIdKey, int.Parse(pDataTable.Rows[i]["FENTRYID"].ToString())); ;

                        if (i > 0)
                            fsKeys += ",";
                        fsKeys += pDataTable.Rows[i]["fskey"].ToString();

                        lst.Add(pDataTable.Rows[i]["FENTRYID"].ToString(), float.Parse(pDataTable.Rows[i]["实收数量"].ToString()));
                    }

                    // 调用Web API接口服务，保存单据
                    BillNo = client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save", new object[] { "PRD_INSTOCK", jsonRoot.ToString() });
                    JObject jo = JObject.Parse(BillNo);

                    if (!jo["Result"]["ResponseStatus"]["IsSuccess"].Value<bool>())
                    {
                        BillNo = string.Empty;
                        for (int i = 0; i < ((IList)jo["Result"]["ResponseStatus"]["Errors"]).Count; i++)
                            BillNo += jo["Result"]["ResponseStatus"]["Errors"][i]["Message"].Value<string>() + "\r\n";//保存不成功返错误信息

                        //反写失败状态
                        UpdateXBT_DataFSBS(fsKeys, false);
                    }
                    else
                    {
                        BillNo = "ID:" + jo["Result"]["Id"].Value<string>() + ";Number:" + jo["Result"]["Number"].Value<string>();//保存成功返回入库单FID和单据编号FBILLNO

                        client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Submit", new object[] { "PRD_INSTOCK", "{\"CreateOrgId\":\"0\",\"Numbers\":[\"" + jo["Result"]["Number"].Value<string>() + "\"]}" });//根据单号提交单据
                        client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Audit", new object[] { "PRD_INSTOCK", "{\"CreateOrgId\":\"0\",\"Numbers\":[\"" + jo["Result"]["Number"].Value<string>() + "\"]}" });//根据单号审核单据

                        //反写成功状态
                        UpdateXBT_DataFSBS(fsKeys, true);

                        //反写生产订单关联数量
                        UpdateT_PRD_MOENTRY_Q_QTY(lst);
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return BillNo;
        }
        /// <summary>
        /// 生产退库
        /// </summary>
        /// <param name="pDataTable"></param>
        /// <returns></returns>
        private string PRD_RetStock(DataTable pDataTable)
        {
            string BillNo = string.Empty;
            string fsKeys = string.Empty;//
            try
            {
                K3CloudApiClient client = new K3CloudApiClient(GlobalParameter.K3Inf.C_ERPADDRESS);
                bool login = client.Login(GlobalParameter.K3Inf.C_ZTID, GlobalParameter.K3Inf.C_USERNAME, GlobalParameter.K3Inf.C_PASSWORD, 2052);
                if (login)
                {
                    //JObject jsonRoot = new JObject();
                    //jsonRoot.Add("Creator", "PDA");
                    //jsonRoot.Add("NeedUpDateFields", new JArray(""));
                    //jsonRoot.Add("NeedReturnFields", new JArray(""));

                    //jsonRoot.Add("IsDeleteEntry", "true");
                    //jsonRoot.Add("SubSystemId", "");
                    //jsonRoot.Add("IsVerifyBaseDataField", "false");
                    //jsonRoot.Add("IsEntryBatchFill", "true");
                    //jsonRoot.Add("ValidateFlag", "true");
                    //jsonRoot.Add("NumberSearch", "true");
                    //jsonRoot.Add("InterationFlags", "");
                    //jsonRoot.Add("IsAutoSubmitAndAudit", "false");

                    //JObject model = new JObject();
                    //jsonRoot.Add("Model", model);
                    //model.Add("FID", 0);

                    //JObject basedata = new JObject();
                    //basedata.Add("FNumber", "SCTK01_SYS");
                    //model.Add("FBillType", basedata);

                    //// 日期
                    //model.Add("FDate", DateTime.Today);

                    //basedata = new JObject();
                    //basedata.Add("FNumber", pDataTable.Rows[0]["退库组织"].ToString());
                    //model.Add("FStockOrgId", basedata);
                    //basedata = new JObject();
                    //basedata.Add("FNumber", pDataTable.Rows[0]["退库组织"].ToString());
                    //model.Add("FPrdOrgId", basedata);
                    //model.Add("FOwnerTypeId0", "BD_OwnerOrg");
                    //basedata = new JObject();
                    //basedata.Add("FNumber", pDataTable.Rows[0]["退库组织"].ToString());
                    //model.Add("FOwnerId0", basedata);
                    //basedata = new JObject();
                    //model.Add("FIsEntrust", false);

                    //// 开始构建单据体参数：集合参数JArray
                    //JArray entryRows = new JArray();
                    //string entityKey = "FEntity";
                    //model.Add(entityKey, entryRows);

                    //for (int i = 0; i < pDataTable.Rows.Count; i++)
                    //{
                    //    // 添加新行，把新行加入到单据体行集合
                    //    JObject entryRow = new JObject();
                    //    entryRows.Add(entryRow);

                    //    // 单据体主键：必须填写，系统据此判断是新增还是修改行
                    //    entryRow.Add("FEntryID", 0);

                    //    //entryRow.Add("FRowType", "Standard");
                    //    //物料(FMaterialId)：基础资料，填写编码
                    //    basedata = new JObject();
                    //    basedata.Add("FNumber", pDataTable.Rows[i]["物料编码"].ToString());
                    //    entryRow.Add("FMaterialId", basedata);

                    //    basedata = new JObject();
                    //    basedata.Add("FNumber", pDataTable.Rows[i]["单位"].ToString());
                    //    entryRow.Add("FUnitID", basedata);
                    //    basedata = new JObject();
                    //    basedata.Add("FNumber", pDataTable.Rows[i]["单位"].ToString());
                    //    entryRow.Add("FBaseUnitId", basedata);

                    //    entryRow.Add("FRealQty", decimal.Parse(pDataTable.Rows[i]["实退数量"].ToString()));
                    //    entryRow.Add("FIsFree", false);
                    //    entryRow.Add("FEntryTaxRate", 13.00);

                    //    basedata = new JObject();
                    //    basedata.Add("FNumber", "THLX01_SYS");
                    //    entryRow.Add("FReturnType", basedata);

                    //    entryRow.Add("FOwnerTypeId", "BD_OwnerOrg");

                    //    basedata = new JObject();
                    //    basedata.Add("FNumber", pDataTable.Rows[i]["退库组织"].ToString());
                    //    entryRow.Add("FOwnerId", basedata);

                    //    basedata = new JObject();
                    //    basedata.Add("FNumber", pDataTable.Rows[i]["仓库"].ToString());
                    //    entryRow.Add("FStockId", basedata);
                    //    basedata = new JObject();
                    //    basedata.Add("FNumber", "KCZT01_SYS");
                    //    entryRow.Add("FStockStatusId", basedata);
                    //    entryRow.Add("FKeeperTypeId", "BD_KeeperOrg");
                    //    basedata = new JObject();
                    //    basedata.Add("FNumber", pDataTable.Rows[i]["退库组织"].ToString());
                    //    entryRow.Add("FKeeperId", basedata);

                    //    entryRow.Add("FDeliveryDate", DateTime.Today);

                    //    entryRow.Add("FMustQty", decimal.Parse(pDataTable.Rows[i]["应退数量"].ToString()));
                    //    entryRow.Add("FBaseMustQty", decimal.Parse(pDataTable.Rows[i]["应退数量"].ToString()));

                    //    entryRow.Add("FMoBillNo", pDataTable.Rows[i]["FMOBILLNO"].ToString());
                    //    entryRow.Add("FMoEntryId", pDataTable.Rows[i]["FMOENTRYID"].ToString());


                    //    //创建与源单之间的关联关系，以支持上查与反写源单
                    //    entryRow.Add("FSrcBillTypeId", "PRD_INSTOCK");
                    //    entryRow.Add("FSRCBILLNO", pDataTable.Rows[i]["生产入库单号"].ToString());

                    //    JArray linkRows = new JArray();
                    //    string linkEntityKey = string.Format("{0}_Link", entityKey);
                    //    entryRow.Add(linkEntityKey, linkRows);

                    //    JObject linkRow = new JObject();
                    //    linkRows.Add(linkRow);

                    //    //FFlowId : 业务流程图，可选
                    //    string fldFlowIdKey = string.Format("{0}_FFlowId", linkEntityKey);
                    //    linkRow.Add(fldFlowIdKey, "");
                    //    //FFlowLineId ：业务流程图路线，可选
                    //    string fldFlowLineIdKey = string.Format("{0}_FFlowLineId", linkEntityKey);
                    //    linkRow.Add(fldFlowLineIdKey, "");

                    //    string fldRuleIdKey = string.Format("{0}_FRuleId", linkEntityKey);
                    //    linkRow.Add(fldRuleIdKey, "PRD_INSTOCK-PRD_RetStock");
                    //    string fldSTableNameKey = string.Format("{0}_FSTableName", linkEntityKey);
                    //    linkRow.Add(fldSTableNameKey, "T_PRD_INSTOCKENTRY");

                    //    string fldSBillIdKey = string.Format("{0}_FSBillId", linkEntityKey);
                    //    linkRow.Add(fldSBillIdKey, int.Parse(pDataTable.Rows[i]["FID"].ToString()));
                    //    string fldSIdKey = string.Format("{0}_FSId", linkEntityKey);
                    //    linkRow.Add(fldSIdKey, int.Parse(pDataTable.Rows[i]["FENTRYID"].ToString()));

                    //    if (i > 0)
                    //        fsKeys += ",";
                    //    fsKeys += pDataTable.Rows[i]["fskey"].ToString();
                    //}

                    JObject jsonRoot = new JObject();
                    jsonRoot.Add("Ids", "");

                    JArray entryRow = new JArray();
                    jsonRoot.Add("Numbers", entryRow);

                    for (int i = 0; i < pDataTable.Rows.Count; i++)
                    {
                        entryRow.Add(pDataTable.Rows[i]["生产入库单号"].ToString());

                        if (i > 0)
                            fsKeys += ",";
                        fsKeys += pDataTable.Rows[i]["fskey"].ToString();
                    }
                    jsonRoot.Add("EntryIds", "");
                    jsonRoot.Add("RuleId", "InStock2ReStockConvert");
                    jsonRoot.Add("TargetBillTypeId", "");
                    jsonRoot.Add("TargetOrgId", 0);
                    jsonRoot.Add("TargetFormId", "PRD_RetStock");
                    jsonRoot.Add("IsEnableDefaultRule", false);
                    //jsonRoot.Add("IsDraftWhenSaveFail", false);
                    jsonRoot.Add("CustomParams", new JObject());


                    // 调用Web API接口服务，保存单据
                    BillNo = client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Push", new object[] { "PRD_INSTOCK", jsonRoot.ToString() });
                    JObject jo = JObject.Parse(BillNo);

                    if (!jo["Result"]["ResponseStatus"]["IsSuccess"].Value<bool>())
                    {
                        BillNo = string.Empty;
                        for (int i = 0; i < ((IList)jo["Result"]["ResponseStatus"]["Errors"]).Count; i++)
                            BillNo += jo["Result"]["ResponseStatus"]["Errors"][i]["Message"].Value<string>() + "\r\n";//保存不成功返错误信息

                        //反写失败状态
                        UpdateXBT_DataFSBS(fsKeys, false);
                    }
                    else
                    {
                        BillNo = "ID:" + jo["Result"]["ResponseStatus"]["SuccessEntitys"].First["Id"].Value<string>() + ";Number:" + jo["Result"]["ResponseStatus"]["SuccessEntitys"].First["Number"].Value<string>();//保存成功返回入库单FID和单据编号FBILLNO

                        //反写入库数量
                        SQLHelper.ExecuteNonQuery(C_C_PrdPickMtrl_PrdReturnMtrl_RealQty);

                        client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Submit", new object[] { "PRD_RetStock", "{\"CreateOrgId\":\"0\",\"Numbers\":[\"" + jo["Result"]["ResponseStatus"]["SuccessEntitys"].First["Number"].Value<string>() + "\"]}" });//根据单号提交单据
                        client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Audit", new object[] { "PRD_RetStock", "{\"CreateOrgId\":\"0\",\"Numbers\":[\"" + jo["Result"]["ResponseStatus"]["SuccessEntitys"].First["Number"].Value<string>() + "\"]}" });//根据单号审核单据

                        //反写成功状态
                        UpdateXBT_DataFSBS(fsKeys, true);
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return BillNo;
        }
        /// <summary>
        /// 生产领料
        /// </summary>
        /// <param name="pDataTable"></param>
        /// <returns></returns>
        private string PRD_PickMtrl(DataTable pDataTable)
        {
            string BillNo = string.Empty;
            string fsKeys = string.Empty;
            Dictionary<string, float> lst = new Dictionary<string, float>();
            string fmoentryids = string.Empty;
            try
            {
                K3CloudApiClient client = new K3CloudApiClient(GlobalParameter.K3Inf.C_ERPADDRESS);
                bool login = client.Login(GlobalParameter.K3Inf.C_ZTID, GlobalParameter.K3Inf.C_USERNAME, GlobalParameter.K3Inf.C_PASSWORD, 2052);
                if (login)
                {
                    JObject jsonRoot = new JObject();
                    jsonRoot.Add("Creator", "PDA");
                    jsonRoot.Add("NeedUpDateFields", new JArray(""));
                    jsonRoot.Add("NeedReturnFields", new JArray(""));

                    jsonRoot.Add("IsDeleteEntry", "true");
                    jsonRoot.Add("SubSystemId", "");
                    jsonRoot.Add("IsVerifyBaseDataField", "false");
                    jsonRoot.Add("IsEntryBatchFill", "true");
                    jsonRoot.Add("ValidateFlag", "true");
                    jsonRoot.Add("NumberSearch", "true");
                    jsonRoot.Add("InterationFlags", "");
                    jsonRoot.Add("IsAutoSubmitAndAudit", "false");

                    JObject model = new JObject();
                    jsonRoot.Add("Model", model);
                    model.Add("FID", 0);

                    JObject basedata = new JObject();
                    basedata.Add("FNumber", "SCLLD01_SYS");
                    model.Add("FBillType", basedata);

                    // 日期
                    model.Add("FDate", DateTime.Today);

                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["发料组织"].ToString());
                    model.Add("FStockOrgId", basedata);

                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["发料组织"].ToString());
                    model.Add("FPrdOrgId", basedata);

                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["货主"].ToString());
                    model.Add("FOwnerId0", basedata);
                    model.Add("FOwnerTypeId0", "BD_OwnerOrg");

                    JArray entryRows = new JArray();
                    string entityKey = "FEntity";
                    model.Add(entityKey, entryRows);
                    for (int i = 0; i < pDataTable.Rows.Count; i++)
                    {
                        JObject entryRow = new JObject();
                        entryRows.Add(entryRow);
                        entryRow.Add("FEntryID", 0);

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["物料编码"].ToString());
                        entryRow.Add("FMaterialId", basedata);

                        entryRow.Add("FAppQty", pDataTable.Rows[i]["申请数量"].ToString());
                        entryRow.Add("FActualQty", pDataTable.Rows[i]["实发数量"].ToString());

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["单位"].ToString());
                        entryRow.Add("FUnitID", basedata);
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["单位"].ToString());
                        entryRow.Add("FStockUnitId", basedata);
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["单位"].ToString());
                        entryRow.Add("FBaseUnitId", basedata);

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["仓库"].ToString());
                        entryRow.Add("FStockId", basedata);
                        basedata = new JObject();
                        basedata.Add("FNumber", "KCZT01_SYS");
                        entryRow.Add("FStockStatusId", basedata);

                        //----仓位
                        string cw = pDataTable.Rows[i]["FV"].ToString().Trim();
                        if (cw != "0")
                        {
                            JObject sp = new JObject();
                            sp.Add("FNumber", pDataTable.Rows[i]["仓位"].ToString());
                            basedata = new JObject();
                            basedata.Add("FSTOCKLOCID__FF" + pDataTable.Rows[i]["FVV"].ToString(), sp);                            
                            entryRow.Add("FStockLocId", basedata);
                        }


                        entryRow.Add("FOwnerTypeId", "BD_OwnerOrg");
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["货主"].ToString());
                        entryRow.Add("FOwnerId", basedata);

                        entryRow.Add("FParentOwnerTypeId", "BD_OwnerOrg");

                        entryRow.Add("FKeeperTypeId", "BD_KeeperOrg");
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["货主"].ToString());
                        entryRow.Add("FKeeperId", basedata);

                        //单据类型
                        entryRow.Add("FSrcBillType", "PRD_PPBOM");

                        //entryRow.Add("FSRCBIZBILLNO", pDataTable.Rows[i]["INSTOCKBILLNO"].ToString());
                        //entryRow.Add("FSRCBIZINTERID", pDataTable.Rows[i]["INSTOCKFID"].ToString());
                        //entryRow.Add("FSRCBIZENTRYID", pDataTable.Rows[i]["INSTOCKFENTRYID"].ToString());
                        //entryRow.Add("FSRCBIZENTRYSEQ", pDataTable.Rows[i]["INSTOCKSEQ"].ToString());

                        entryRow.Add("FMoBillNo", pDataTable.Rows[i]["fmobillno"].ToString());
                        entryRow.Add("FMoId", pDataTable.Rows[i]["FMOID"].ToString());
                        entryRow.Add("FMoEntryId", pDataTable.Rows[i]["fmoentryid"].ToString());
                        entryRow.Add("FMoEntrySeq", pDataTable.Rows[i]["FMOENTRYSEQ"].ToString());

                        //entryRow.Add("FSrcBillNo", pDataTable.Rows[i]["PPBILLNO"].ToString());
                        //entryRow.Add("FEntrySrcInterId", pDataTable.Rows[i]["PPFID"].ToString());
                        //entryRow.Add("FEntrySrcEnteryId", pDataTable.Rows[i]["PPFENTRYID"].ToString());
                        //entryRow.Add("FEntrySrcEntrySeq", pDataTable.Rows[i]["PPFSEQ"].ToString());

                        //entryRow.Add("FPPBomBillNo", pDataTable.Rows[i]["PPBILLNO"].ToString());
                        //entryRow.Add("FPPBOMENTRYID", pDataTable.Rows[i]["PPFENTRYID"].ToString());

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["部门"].ToString());
                        entryRow.Add("FEntryWorkShopId", basedata);

                        //entryRow.Add("FOPERID", pDataTable.Rows[i]["FOPERID"].ToString());

                        //创建与源单之间的关联关系，以支持上查与反写源单
                        entryRow.Add("FSrcBillTypeId", "PRD_PPBOM");
                        JArray linkRows = new JArray();
                        string linkEntityKey = string.Format("{0}_Link", entityKey);
                        entryRow.Add(linkEntityKey, linkRows);
                        JObject linkRow = new JObject();
                        linkRows.Add(linkRow);

                        //FFlowId : 业务流程图，可选
                        string fldFlowIdKey = string.Format("{0}_FFlowId", linkEntityKey);
                        linkRow.Add(fldFlowIdKey, "");
                        //FFlowLineId ：业务流程图路线，可选
                        string fldFlowLineIdKey = string.Format("{0}_FFlowLineId", linkEntityKey);
                        linkRow.Add(fldFlowLineIdKey, "");

                        string fldRuleIdKey = string.Format("{0}_FRuleId", linkEntityKey);
                        linkRow.Add(fldRuleIdKey, "PRD_PPBOM-PRD_PICKMTRL");
                        string fldSTableNameKey = string.Format("{0}_FSTableName", linkEntityKey);
                        linkRow.Add(fldSTableNameKey, "T_PRD_PPBOMENTRY");

                        string fldSBillIdKey = string.Format("{0}_FSBillId", linkEntityKey);
                        linkRow.Add(fldSBillIdKey, int.Parse(pDataTable.Rows[i]["FID"].ToString()));
                        string fldSIdKey = string.Format("{0}_FSId", linkEntityKey);
                        linkRow.Add(fldSIdKey, int.Parse(pDataTable.Rows[i]["FENTRYID"].ToString()));

                        if (i > 0)
                        {
                            fsKeys += ",";
                            fmoentryids += ",";
                        }
                        fsKeys += pDataTable.Rows[i]["fskey"].ToString();
                        lst.Add(pDataTable.Rows[i]["FENTRYID"].ToString(), float.Parse(pDataTable.Rows[i]["实发数量"].ToString()));
                        fmoentryids += pDataTable.Rows[i]["fmoentryid"].ToString();
                    }

                    // 调用Web API接口服务，保存单据
                    BillNo = client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save", new object[] { "PRD_PickMtrl", jsonRoot.ToString() });
                    JObject jo = JObject.Parse(BillNo);

                    if (!jo["Result"]["ResponseStatus"]["IsSuccess"].Value<bool>())
                    {
                        BillNo = string.Empty;
                        for (int i = 0; i < ((IList)jo["Result"]["ResponseStatus"]["Errors"]).Count; i++)
                            BillNo += jo["Result"]["ResponseStatus"]["Errors"][i]["Message"].Value<string>() + "\r\n";//保存不成功返错误信息

                        //反写失败状态
                        UpdateXBT_DataFSBS(fsKeys, false);
                    }
                    else
                    {
                        BillNo = "ID:" + jo["Result"]["Id"].Value<string>() + ";Number:" + jo["Result"]["Number"].Value<string>();//保存成功返回入库单FID和单据编号FBILLNO

                        client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Submit", new object[] { "PRD_PickMtrl", "{\"CreateOrgId\":\"0\",\"Numbers\":[\"" + jo["Result"]["Number"].Value<string>() + "\"]}" });//根据单号提交单据
                        client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Audit", new object[] { "PRD_PickMtrl", "{\"CreateOrgId\":\"0\",\"Numbers\":[\"" + jo["Result"]["Number"].Value<string>() + "\"]}" });//根据单号审核单据

                        //反写成功状态
                        UpdateXBT_DataFSBS(fsKeys, true);
                        //反写用料清单已领数量
                        UpdateT_PRD_PPBOMENTRY_Q_FPICKEDQTY(lst);
                        //反写生产订单领料状态
                        UpdateMoEntry_Q_FPICKMTRLSTATUS(fmoentryids);
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return BillNo;
        }
        /// <summary>
        /// 生产退料
        /// </summary>
        /// <param name="pDataTable"></param>
        /// <returns></returns>
        private string PRD_ReturnMtrl(DataTable pDataTable)
        {
            string BillNo = string.Empty;
            string fsKeys = string.Empty;//
            Dictionary<string, float> lst = new Dictionary<string, float>();
            try
            {
                K3CloudApiClient client = new K3CloudApiClient(GlobalParameter.K3Inf.C_ERPADDRESS);
                bool login = client.Login(GlobalParameter.K3Inf.C_ZTID, GlobalParameter.K3Inf.C_USERNAME, GlobalParameter.K3Inf.C_PASSWORD, 2052);
                if (login)
                {
                    JObject jsonRoot = new JObject();
                    jsonRoot.Add("Creator", "PDA");
                    jsonRoot.Add("NeedUpDateFields", new JArray(""));
                    jsonRoot.Add("NeedReturnFields", new JArray(""));

                    jsonRoot.Add("IsDeleteEntry", "true");
                    jsonRoot.Add("SubSystemId", "");
                    jsonRoot.Add("IsVerifyBaseDataField", "false");
                    jsonRoot.Add("IsEntryBatchFill", "true");
                    jsonRoot.Add("ValidateFlag", "true");
                    jsonRoot.Add("NumberSearch", "true");
                    jsonRoot.Add("InterationFlags", "");
                    jsonRoot.Add("IsAutoSubmitAndAudit", "false");

                    JObject model = new JObject();
                    jsonRoot.Add("Model", model);
                    model.Add("FID", 0);

                    JObject basedata = new JObject();
                    basedata.Add("FNumber", "SCTLD01_SYS");
                    model.Add("FBillType", basedata);

                    // 日期
                    model.Add("FDate", DateTime.Today);

                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["收料组织"].ToString());
                    model.Add("FStockOrgId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["收料组织"].ToString());
                    model.Add("FPrdOrgId", basedata);

                    model.Add("FOwnerTypeId0", "BD_OwnerOrg");
                    model.Add("FIsCrossTrade", false);
                    model.Add("FVmiBusiness", false);
                    //basedata = new JObject();
                    //basedata.Add("FNumber", pDataTable.Rows[0]["货主"].ToString());
                    //model.Add("FOwnerId0", basedata);

                    JArray entryRows = new JArray();
                    string entityKey = "FEntity";
                    model.Add(entityKey, entryRows);
                    for (int i = 0; i < pDataTable.Rows.Count; i++)
                    {
                        JObject entryRow = new JObject();
                        entryRows.Add(entryRow);
                        //entryRow.Add("FEntryID", 0);

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["物料编码"].ToString());
                        entryRow.Add("FMaterialId", basedata);
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["单位"].ToString());
                        entryRow.Add("FUnitID", basedata);
                        entryRow.Add("FAPPQty", pDataTable.Rows[i]["申请数量"].ToString());
                        entryRow.Add("FQty", pDataTable.Rows[i]["实退数量"].ToString());
                        entryRow.Add("FReturnType", "1");
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["仓库"].ToString());
                        entryRow.Add("FStockId", basedata);
                        //仓位
                        string cw = pDataTable.Rows[i]["FV"].ToString().Trim();
                        if (cw != "0")
                        {
                            JObject sp = new JObject();
                            sp.Add("FNumber", pDataTable.Rows[i]["仓位"].ToString());
                            basedata = new JObject();
                            basedata.Add("FSTOCKLOCID__FF" + pDataTable.Rows[i]["FVV"].ToString(), sp);
                            entryRow.Add("FStockLocId", basedata);
                        }

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["产品编码"].ToString());
                        entryRow.Add("FParentMaterialId", basedata);

                        entryRow.Add("FSrcBillType", "PRD_PICKMTRL");
                        entryRow.Add("FSrcBillNo", pDataTable.Rows[i]["生产领料单号"].ToString());
                        entryRow.Add("FEntrySrcEnteryId", pDataTable.Rows[i]["FENTRYID"].ToString());
                        entryRow.Add("FPPBomBillNo", pDataTable.Rows[i]["FPPBOMBILLNO"].ToString());

                        entryRow.Add("FMoBillNo", pDataTable.Rows[i]["FMOBILLNO"].ToString());
                        entryRow.Add("FMoId", pDataTable.Rows[i]["FMOID"].ToString());
                        entryRow.Add("FMoEntryId", pDataTable.Rows[i]["FMOENTRYID"].ToString());
                        entryRow.Add("FMoEntrySeq", pDataTable.Rows[i]["FMOENTRYSEQ"].ToString());
                        entryRow.Add("FReserveType", "1");
                        entryRow.Add("FBASESTOCKQTY", pDataTable.Rows[i]["实退数量"].ToString());
                        entryRow.Add("FEntryVmiBusiness", false);
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["单位"].ToString());
                        entryRow.Add("FStockUnitId", basedata);
                        entryRow.Add("FStockAppQty", pDataTable.Rows[i]["申请数量"].ToString());
                        entryRow.Add("FStockQty", pDataTable.Rows[i]["实退数量"].ToString());
                        basedata = new JObject();
                        basedata.Add("FNumber", "KCZT01_SYS");
                        entryRow.Add("FStockStatusId", basedata);
                        entryRow.Add("FKeeperTypeId", "BD_KeeperOrg");
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["货主"].ToString());
                        entryRow.Add("FKeeperId", basedata);
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["单位"].ToString());
                        entryRow.Add("FBaseUnitId", basedata);
                        entryRow.Add("FBaseAppQty", pDataTable.Rows[i]["申请数量"].ToString());
                        entryRow.Add("FBaseQty", pDataTable.Rows[i]["实退数量"].ToString());

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["部门"].ToString());
                        entryRow.Add("FWorkShopId1", basedata);
                        entryRow.Add("FParentOwnerTypeId", "BD_OwnerOrg");
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["货主"].ToString());
                        entryRow.Add("FParentOwnerId", basedata);
                                                
                        //创建与源单之间的关联关系，以支持上查与反写源单
                        //entryRow.Add("FSrcBillTypeId", "PRD_PICKMTRL");
                        JArray linkRows = new JArray();
                        string linkEntityKey = string.Format("{0}_Link", entityKey);
                        entryRow.Add(linkEntityKey, linkRows);
                        JObject linkRow = new JObject();
                        linkRows.Add(linkRow);

                        //FFlowId : 业务流程图，可选
                        string fldFlowIdKey = string.Format("{0}_FFlowId", linkEntityKey);
                        linkRow.Add(fldFlowIdKey, "");
                        //FFlowLineId ：业务流程图路线，可选
                        string fldFlowLineIdKey = string.Format("{0}_FFlowLineId", linkEntityKey);
                        linkRow.Add(fldFlowLineIdKey, "");

                        string fldRuleIdKey = string.Format("{0}_FRuleId", linkEntityKey);
                        linkRow.Add(fldRuleIdKey, "PRD_PICKMTRL-PRD_ReturnMtrl");
                        string fldSTableNameKey = string.Format("{0}_FSTableName", linkEntityKey);
                        linkRow.Add(fldSTableNameKey, "T_PRD_PICKMTRLDATA");

                        string fldSBillIdKey = string.Format("{0}_FSBillId", linkEntityKey);
                        linkRow.Add(fldSBillIdKey, int.Parse(pDataTable.Rows[i]["FID"].ToString()));
                        string fldSIdKey = string.Format("{0}_FSId", linkEntityKey);
                        linkRow.Add(fldSIdKey, int.Parse(pDataTable.Rows[i]["FENTRYID"].ToString()));

                        if (i > 0)
                            fsKeys += ",";
                        fsKeys += pDataTable.Rows[i]["fskey"].ToString();

                        lst.Add(pDataTable.Rows[i]["FENTRYID"].ToString(), float.Parse(pDataTable.Rows[i]["实退数量"].ToString()));
                    }

                    // 调用Web API接口服务，保存单据
                    BillNo = client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save", new object[] { "PRD_ReturnMtrl", jsonRoot.ToString() });
                    JObject jo = JObject.Parse(BillNo);

                    if (!jo["Result"]["ResponseStatus"]["IsSuccess"].Value<bool>())
                    {
                        BillNo = string.Empty;
                        for (int i = 0; i < ((IList)jo["Result"]["ResponseStatus"]["Errors"]).Count; i++)
                            BillNo += jo["Result"]["ResponseStatus"]["Errors"][i]["Message"].Value<string>() + "\r\n";//保存不成功返错误信息

                        //反写失败状态
                        UpdateXBT_DataFSBS(fsKeys, false);
                    }
                    else
                    {
                        BillNo = "ID:" + jo["Result"]["Id"].Value<string>() + ";Number:" + jo["Result"]["Number"].Value<string>();//保存成功返回入库单FID和单据编号FBILLNO

                        client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Submit", new object[] { "PRD_ReturnMtrl", "{\"CreateOrgId\":\"0\",\"Numbers\":[\"" + jo["Result"]["Number"].Value<string>() + "\"]}" });//根据单号提交单据
                        client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Audit", new object[] { "PRD_ReturnMtrl", "{\"CreateOrgId\":\"0\",\"Numbers\":[\"" + jo["Result"]["Number"].Value<string>() + "\"]}" });//根据单号审核单据

                        //反写成功状态
                        UpdateXBT_DataFSBS(fsKeys, true);

                        //反写生产领料单
                        UpdateT_PRD_PICKMTRLDATA_QTY(lst);
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return BillNo;
        }
        /// <summary>
        /// 其他入库
        /// </summary>
        /// <param name="pDataTable"></param>
        /// <returns></returns>
        private string STK_MISCELLANEOUS(DataTable pDataTable)
        {
            string BillNo = string.Empty;
            string fsKeys = string.Empty;//
            try
            {
                K3CloudApiClient client = new K3CloudApiClient(GlobalParameter.K3Inf.C_ERPADDRESS);
                bool login = client.Login(GlobalParameter.K3Inf.C_ZTID, GlobalParameter.K3Inf.C_USERNAME, GlobalParameter.K3Inf.C_PASSWORD, 2052);
                if (login)
                {
                    JObject jsonRoot = new JObject();
                    jsonRoot.Add("Creator", "PDA");
                    jsonRoot.Add("NeedUpDateFields", new JArray(""));
                    jsonRoot.Add("NeedReturnFields", new JArray(""));

                    jsonRoot.Add("IsDeleteEntry", "true");
                    jsonRoot.Add("SubSystemId", "");
                    jsonRoot.Add("IsVerifyBaseDataField", "false");
                    jsonRoot.Add("IsEntryBatchFill", "true");
                    jsonRoot.Add("ValidateFlag", "true");
                    jsonRoot.Add("NumberSearch", "true");
                    jsonRoot.Add("InterationFlags", "");
                    jsonRoot.Add("IsAutoSubmitAndAudit", "false");

                    JObject model = new JObject();
                    jsonRoot.Add("Model", model);
                    model.Add("FID", 0);

                    JObject basedata = new JObject();
                    basedata.Add("FNumber", "QTRKD01_SYS");
                    model.Add("FBillTypeID", basedata);

                    // 日期
                    basedata.Add("FDate", DateTime.Now);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["库存组织"].ToString());
                    model.Add("FStockOrgId", basedata);
                    model.Add("FStockDirect", "GENERAL");
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["供应商"].ToString());
                    model.Add("FSUPPLIERID", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["部门"].ToString());
                    model.Add("FDEPTID", basedata);
                    basedata = new JObject();
                    model.Add("FOwnerTypeIdHead", "BD_OwnerOrg");
                    basedata.Add("FNumber", pDataTable.Rows[0]["库存组织"].ToString());
                    model.Add("FOwnerIdHead", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", "PRE001");
                    model.Add("FBaseCurrId", basedata);
                    

                    // 开始构建单据体参数：集合参数JArray
                    JArray entryRows = new JArray();
                    string entityKey = "FEntity";
                    model.Add(entityKey, entryRows);

                    for (int i = 0; i < pDataTable.Rows.Count; i++)
                    {
                        // 添加新行，把新行加入到单据体行集合
                        JObject entryRow = new JObject();
                        entryRows.Add(entryRow);

                        // 单据体主键：必须填写，系统据此判断是新增还是修改行
                        entryRow.Add("FEntryID", 0);

                        //物料(FMaterialId)：基础资料，填写编码
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["物料编码"].ToString());
                        entryRow.Add("FMATERIALID", basedata);

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["单位"].ToString());
                        entryRow.Add("FUnitID", basedata);

                        basedata = new JObject();
                        basedata.Add("FNumber", "KCZT01_SYS");
                        entryRow.Add("FSTOCKSTATUSID", basedata);

                        entryRow.Add("FQty", pDataTable.Rows[i]["实收数量"].ToString());                        

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["仓库"].ToString());
                        entryRow.Add("FSTOCKID", basedata);
                        entryRow.Add("FOWNERTYPEID", "BD_OwnerOrg");

                        //仓位
                        string cw = pDataTable.Rows[i]["FV"].ToString().Trim();
                        if (cw != "0")
                        {
                            JObject sp = new JObject();
                            sp.Add("FNumber", pDataTable.Rows[i]["仓位"].ToString());
                            basedata = new JObject();
                            basedata.Add("FSTOCKLOCID__FF" + pDataTable.Rows[i]["FVV"].ToString(), sp);
                            entryRow.Add("FStockLocId", basedata);
                        }

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[0]["库存组织"].ToString());
                        entryRow.Add("FOWNERID", basedata);
                        entryRow.Add("FKEEPERTYPEID", "BD_KeeperOrg");
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["库存组织"].ToString());
                        entryRow.Add("FKEEPERID", basedata);

                        if (i > 0)
                            fsKeys += ",";
                        fsKeys += pDataTable.Rows[i]["fskey"].ToString();
                    }

                    // 调用Web API接口服务，保存单据
                    BillNo = client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save", new object[] { "STK_MISCELLANEOUS", jsonRoot.ToString() });
                    JObject jo = JObject.Parse(BillNo);

                    if (!jo["Result"]["ResponseStatus"]["IsSuccess"].Value<bool>())
                    {
                        BillNo = string.Empty;
                        for (int i = 0; i < ((IList)jo["Result"]["ResponseStatus"]["Errors"]).Count; i++)
                            BillNo += jo["Result"]["ResponseStatus"]["Errors"][i]["Message"].Value<string>() + "\r\n";//保存不成功返错误信息

                        //反写失败状态
                        UpdateXBT_DataFSBS(fsKeys, false);
                    }
                    else
                    {
                        BillNo = "ID:" + jo["Result"]["Id"].Value<string>() + ";Number:" + jo["Result"]["Number"].Value<string>();//保存成功返回入库单FID和单据编号FBILLNO

                        client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Submit", new object[] { "STK_MISCELLANEOUS", "{\"CreateOrgId\":\"0\",\"Numbers\":[\"" + jo["Result"]["Number"].Value<string>() + "\"]}" });//根据单号提交单据
                        client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Audit", new object[] { "STK_MISCELLANEOUS", "{\"CreateOrgId\":\"0\",\"Numbers\":[\"" + jo["Result"]["Number"].Value<string>() + "\"]}" });//根据单号审核单据

                        //反写成功状态
                        UpdateXBT_DataFSBS(fsKeys, true);
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return BillNo;
        }
        /// <summary>
        /// 其他出库
        /// </summary>
        /// <param name="pDataTable"></param>
        /// <returns></returns>
        private string STK_MisDelivery(DataTable pDataTable)
        {
            string BillNo = string.Empty;
            string fsKeys = string.Empty;//,fskey
            try
            {
                K3CloudApiClient client = new K3CloudApiClient(GlobalParameter.K3Inf.C_ERPADDRESS);
                bool login = client.Login(GlobalParameter.K3Inf.C_ZTID, GlobalParameter.K3Inf.C_USERNAME, GlobalParameter.K3Inf.C_PASSWORD, 2052);
                if (login)
                {
                    JObject jsonRoot = new JObject();
                    jsonRoot.Add("Creator", "PDA");
                    jsonRoot.Add("NeedUpDateFields", new JArray(""));
                    jsonRoot.Add("NeedReturnFields", new JArray(""));

                    jsonRoot.Add("IsDeleteEntry", "true");
                    jsonRoot.Add("SubSystemId", "");
                    jsonRoot.Add("IsVerifyBaseDataField", "false");
                    jsonRoot.Add("IsEntryBatchFill", "true");
                    jsonRoot.Add("ValidateFlag", "true");
                    jsonRoot.Add("NumberSearch", "true");
                    jsonRoot.Add("InterationFlags", "");
                    jsonRoot.Add("IsAutoSubmitAndAudit", "false");

                    JObject model = new JObject();
                    jsonRoot.Add("Model", model);
                    model.Add("FID", 0);

                    JObject basedata = new JObject();
                    basedata.Add("FNumber", "QTCKD01_SYS");
                    model.Add("FBillTypeID", basedata);

                    // 日期
                    basedata.Add("FDate", DateTime.Now);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["库存组织"].ToString());
                    model.Add("FStockOrgId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["库存组织"].ToString());
                    model.Add("FPickOrgId", basedata);
                    model.Add("FStockDirect", "GENERAL");
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["客户"].ToString());
                    model.Add("FCustId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["部门"].ToString());
                    model.Add("FDeptId", basedata);
                    basedata = new JObject();
                    model.Add("FOwnerTypeIdHead", "BD_OwnerOrg");
                    basedata.Add("FNumber", pDataTable.Rows[0]["库存组织"].ToString());
                    model.Add("FOwnerIdHead", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", "PRE001");
                    model.Add("FBaseCurrId", basedata);

                    // 开始构建单据体参数：集合参数JArray
                    JArray entryRows = new JArray();
                    string entityKey = "FEntity";
                    model.Add(entityKey, entryRows);

                    for (int i = 0; i < pDataTable.Rows.Count; i++)
                    {
                        // 添加新行，把新行加入到单据体行集合
                        JObject entryRow = new JObject();
                        entryRows.Add(entryRow);

                        // 单据体主键：必须填写，系统据此判断是新增还是修改行
                        entryRow.Add("FEntryID", 0);

                        //物料(FMaterialId)：基础资料，填写编码
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["物料编码"].ToString());
                        entryRow.Add("FMaterialId", basedata);

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["单位"].ToString());
                        entryRow.Add("FUnitID", basedata);

                        entryRow.Add("FQty", pDataTable.Rows[i]["实发数量"].ToString());
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["单位"].ToString());
                        entryRow.Add("FBaseUnitId", basedata);

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["仓库"].ToString());
                        entryRow.Add("FStockId", basedata);

                        //仓位
                        string cw = pDataTable.Rows[i]["FV"].ToString().Trim();
                        if (cw != "0")
                        {
                            JObject sp = new JObject();
                            sp.Add("FNumber", pDataTable.Rows[i]["仓位"].ToString());
                            basedata = new JObject();
                            basedata.Add("FSTOCKLOCID__FF" + pDataTable.Rows[i]["FVV"].ToString(), sp);
                            entryRow.Add("FStockLocId", basedata);
                        }

                        entryRow.Add("FOwnerTypeId", "BD_OwnerOrg");

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[0]["库存组织"].ToString());
                        entryRow.Add("FOwnerId", basedata);
                        basedata = new JObject();
                        basedata.Add("FNumber", "KCZT01_SYS");
                        entryRow.Add("FStockStatusId", basedata);
                        entryRow.Add("FKeeperTypeId", "BD_KeeperOrg");
                        entryRow.Add("FDistribution", false);
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["库存组织"].ToString());
                        entryRow.Add("FKeeperId", basedata);

                        if (i > 0)
                            fsKeys += ",";
                        fsKeys += pDataTable.Rows[i]["fskey"].ToString();
                    }

                    // 调用Web API接口服务，保存单据
                    BillNo = client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save", new object[] { "STK_MisDelivery", jsonRoot.ToString() });
                    JObject jo = JObject.Parse(BillNo);

                    if (!jo["Result"]["ResponseStatus"]["IsSuccess"].Value<bool>())
                    {
                        BillNo = string.Empty;
                        for (int i = 0; i < ((IList)jo["Result"]["ResponseStatus"]["Errors"]).Count; i++)
                            BillNo += jo["Result"]["ResponseStatus"]["Errors"][i]["Message"].Value<string>() + "\r\n";//保存不成功返错误信息

                        //反写失败状态
                        UpdateXBT_DataFSBS(fsKeys, false);
                    }
                    else
                    {
                        BillNo = "ID:" + jo["Result"]["Id"].Value<string>() + ";Number:" + jo["Result"]["Number"].Value<string>();//保存成功返回入库单FID和单据编号FBILLNO

                        client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Submit", new object[] { "STK_MisDelivery", "{\"CreateOrgId\":\"0\",\"Numbers\":[\"" + jo["Result"]["Number"].Value<string>() + "\"]}" });//根据单号提交单据
                        client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Audit", new object[] { "STK_MisDelivery", "{\"CreateOrgId\":\"0\",\"Numbers\":[\"" + jo["Result"]["Number"].Value<string>() + "\"]}" });//根据单号审核单据

                        //反写成功状态
                        UpdateXBT_DataFSBS(fsKeys, true);
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return BillNo;
        }
        /// <summary>
        /// 组装拆卸单
        /// </summary>
        /// <param name="pData">数据集</param>
        /// <param name="pFAffairType">事务类型：Assembly|Dassembly</param>
        /// <returns></returns>
        private string STK_AssembledApp(IList<DataTable> pData, string pFAffairType)
        {
            string BillNo = string.Empty;
            string fsKeys = string.Empty;//,fskey
            try
            {
                K3CloudApiClient client = new K3CloudApiClient(GlobalParameter.K3Inf.C_ERPADDRESS);
                bool login = client.Login(GlobalParameter.K3Inf.C_ZTID, GlobalParameter.K3Inf.C_USERNAME, GlobalParameter.K3Inf.C_PASSWORD, 2052);
                if (login)
                {
                    JObject jsonRoot = new JObject();
                    jsonRoot.Add("Creator", "");
                    jsonRoot.Add("NeedUpDateFields", new JArray(""));
                    jsonRoot.Add("NeedReturnFields", new JArray(""));

                    jsonRoot.Add("IsDeleteEntry", "True");
                    jsonRoot.Add("SubSystemId", "");
                    jsonRoot.Add("IsVerifyBaseDataField", "false");
                    jsonRoot.Add("IsEntryBatchFill", "True");
                    jsonRoot.Add("ValidateFlag", "True");
                    jsonRoot.Add("NumberSearch", "True");
                    jsonRoot.Add("InterationFlags", "");
                    jsonRoot.Add("IsAutoSubmitAndAudit", "false");

                    JObject model = new JObject();
                    jsonRoot.Add("Model", model);
                    model.Add("FID", 0);

                    JObject basedata = new JObject();
                    basedata.Add("FNumber", "ZZCX01_SYS");
                    model.Add("FBillTypeID", basedata);

                    basedata = new JObject();
                    basedata.Add("FNumber", pData[0].Rows[0]["库存组织"].ToString());
                    model.Add("FStockOrgId", basedata);
                    model.Add("FAffairType", pFAffairType);
                    // 日期
                    basedata.Add("FDate", DateTime.Now);
                    basedata.Add("FNote", "PDABill");

                    model.Add("FOwnerTypeIdHead", "BD_OwnerOrg");
                    basedata = new JObject();
                    basedata.Add("FNumber", pData[0].Rows[0]["货主"].ToString());//成品货主
                    model.Add("FOwnerIdHead", basedata);
                    model.Add("FSubProOwnTypeIdH", "BD_OwnerOrg");
                    basedata = new JObject();
                    basedata.Add("FNumber", pData[0].Rows[0]["货主"].ToString());//子件货主
                    model.Add("FSubProOwnerIdH", basedata);

                    // 开始构建单据体参数：集合参数JArray
                    JArray entryRows = new JArray();
                    string entityKey = "FEntity";
                    model.Add(entityKey, entryRows);

                    for (int i = 0; i < pData.Count; i++)
                    {
                        JObject entryRow = new JObject();
                        entryRows.Add(entryRow);

                        // 单据体主键：必须填写，系统据此判断是新增还是修改行
                        //entryRow.Add("FEntryID", 0);

                        basedata = new JObject();
                        basedata.Add("FNumber", pData[i].Rows[0]["物料编码"].ToString());
                        entryRow.Add("FMaterialID", basedata);
                        basedata = new JObject();
                        basedata.Add("FNumber", pData[i].Rows[0]["单位"].ToString());
                        entryRow.Add("FUnitID", basedata);

                        entryRow.Add("FQty", pData[i].Rows[0]["数量"].ToString());

                        basedata = new JObject();
                        basedata.Add("FNumber", pData[i].Rows[0]["仓库"].ToString());
                        entryRow.Add("FStockID", basedata);

                        //仓位
                        string cw = pData[i].Rows[0]["FV"].ToString().Trim();
                        if (cw != "0")
                        {
                            JObject sp = new JObject();
                            sp.Add("FNumber", pData[i].Rows[0]["仓位"].ToString());
                            basedata = new JObject();
                            basedata.Add("FSTOCKLOCID__FF" + pData[i].Rows[0]["FVV"].ToString(), sp);
                            entryRow.Add("FStockLocId", basedata);
                        }

                        basedata = new JObject();
                        basedata.Add("FNumber", "KCZT01_SYS");
                        entryRow.Add("FStockStatusID", basedata);
                        basedata = new JObject();
                        basedata.Add("FNumber", pData[i].Rows[0]["单位"].ToString());
                        entryRow.Add("FBaseUnitID", basedata);

                        entryRow.Add("FOwnerTypeID", "BD_OwnerOrg");
                        basedata = new JObject();
                        basedata.Add("FNumber", pData[i].Rows[0]["货主"].ToString());
                        entryRow.Add("FOwnerID", basedata);
                        entryRow.Add("FKeeperTypeID", "BD_KeeperOrg");
                        basedata = new JObject();
                        basedata.Add("FNumber", pData[i].Rows[0]["货主"].ToString());
                        entryRow.Add("FKeeperID", basedata);

                        JArray entrySubRows = new JArray();
                        string SubEntityKey = "FSubEntity";
                        entryRow.Add(SubEntityKey, entrySubRows);
                        for (int j = 0; j < pData[i].Rows.Count; j++)
                        {
                            JObject entrySubRow = new JObject();
                            entrySubRows.Add(entrySubRow);

                            basedata = new JObject();
                            basedata.Add("FNumber", pData[i].Rows[j]["子件物料编码"].ToString());
                            entrySubRow.Add("FMaterialIDSETY", basedata);
                            basedata = new JObject();
                            basedata.Add("FNumber", pData[i].Rows[j]["子件单位"].ToString());
                            entrySubRow.Add("FUnitIDSETY", basedata);

                            entrySubRow.Add("FQtySETY", pData[i].Rows[j]["子件数量"].ToString());

                            basedata = new JObject();
                            basedata.Add("FNumber", pData[i].Rows[j]["子件仓库"].ToString());
                            entrySubRow.Add("FStockIDSETY", basedata);
                            //子件仓位
                            string cw2 = pData[i].Rows[j]["FV2"].ToString();
                            if (cw != "0")
                            {
                                JObject sp2 = new JObject();
                                sp2.Add("FNumber", pData[i].Rows[j]["子件仓位"].ToString());
                                basedata = new JObject();
                                basedata.Add("FSTOCKLOCIDSETY__FF" + pData[i].Rows[j]["FVV2"].ToString(), sp2);
                                entrySubRow.Add("FStockLocIdSETY", basedata);
                            }

                            basedata = new JObject();
                            basedata.Add("FNumber", "KCZT01_SYS");
                            entrySubRow.Add("FStockStatusIDSETY", basedata);
                            basedata = new JObject();
                            basedata.Add("FNumber", pData[i].Rows[j]["子件单位"].ToString());
                            entrySubRow.Add("FBaseUnitIDSETY", basedata);

                            entrySubRow.Add("FKeeperTypeIDSETY", "BD_KeeperOrg");
                            basedata = new JObject();
                            basedata.Add("FNumber", pData[i].Rows[j]["货主"].ToString());
                            entrySubRow.Add("FKeeperIDSETY", basedata);
                            entrySubRow.Add("FOwnerTypeIDSETY", "BD_OwnerOrg");
                            basedata = new JObject();
                            basedata.Add("FNumber", pData[i].Rows[j]["货主"].ToString());
                            entrySubRow.Add("FOwnerIDSETY", basedata);

                            if (!fsKeys.Contains(pData[i].Rows[j]["fskey"].ToString()))
                            {
                                if (i > 0 || j > 0)
                                    fsKeys += ",";
                                fsKeys += pData[i].Rows[j]["fskey"].ToString();
                            }
                        }
                    }

                    // 调用Web API接口服务，保存单据
                    BillNo = client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save", new object[] { "STK_AssembledApp", jsonRoot.ToString() });
                    JObject jo = JObject.Parse(BillNo);

                    if (!jo["Result"]["ResponseStatus"]["IsSuccess"].Value<bool>())
                    {
                        BillNo = string.Empty;
                        for (int i = 0; i < ((IList)jo["Result"]["ResponseStatus"]["Errors"]).Count; i++)
                            BillNo += jo["Result"]["ResponseStatus"]["Errors"][i]["Message"].Value<string>() + "\r\n";//保存不成功返错误信息

                        //反写失败状态
                        UpdateXBT_DataFSBS(fsKeys, false);
                    }
                    else
                    {
                        BillNo = "ID:" + jo["Result"]["Id"].Value<string>() + ";Number:" + jo["Result"]["Number"].Value<string>();//保存成功返回入库单FID和单据编号FBILLNO

                        client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Submit", new object[] { "STK_AssembledApp", "{\"CreateOrgId\":\"0\",\"Numbers\":[\"" + jo["Result"]["Number"].Value<string>() + "\"]}" });//根据单号提交单据
                        client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Audit", new object[] { "STK_AssembledApp", "{\"CreateOrgId\":\"0\",\"Numbers\":[\"" + jo["Result"]["Number"].Value<string>() + "\"]}" });//根据单号审核单据

                        //反写成功状态
                        UpdateXBT_DataFSBS(fsKeys, true);
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return BillNo;
        }
        /// <summary>
        /// 调拨申请单-直接调拨单
        /// </summary>
        /// <param name="pDataTable"></param>
        /// <returns></returns>
        public string STK_TransferDirect(DataTable pDataTable)
        {
            string BillNo = string.Empty;
            string fsKeys = string.Empty;//
            try
            {
                K3CloudApiClient client = new K3CloudApiClient(GlobalParameter.K3Inf.C_ERPADDRESS);
                bool login = client.Login(GlobalParameter.K3Inf.C_ZTID, GlobalParameter.K3Inf.C_USERNAME, GlobalParameter.K3Inf.C_PASSWORD, 2052);
                if (login)
                {
                    JObject jsonRoot = new JObject();
                    jsonRoot.Add("Creator", "PDA");
                    jsonRoot.Add("NeedUpDateFields", new JArray(""));
                    jsonRoot.Add("NeedReturnFields", new JArray(""));

                    jsonRoot.Add("IsDeleteEntry", "true");
                    jsonRoot.Add("SubSystemId", "");
                    jsonRoot.Add("IsVerifyBaseDataField", "false");
                    jsonRoot.Add("IsEntryBatchFill", "true");
                    jsonRoot.Add("ValidateFlag", "true");
                    jsonRoot.Add("NumberSearch", "true");
                    jsonRoot.Add("InterationFlags", "");
                    jsonRoot.Add("IsAutoSubmitAndAudit", "false");

                    JObject model = new JObject();
                    jsonRoot.Add("Model", model);
                    model.Add("FID", 0);

                    JObject basedata = new JObject();
                    basedata.Add("FNumber", "ZJDB01_SYS");
                    model.Add("FBillTypeID", basedata);

                    // 日期
                    model.Add("FDate", DateTime.Today);
                    model.Add("FBizType", "NORMAL");
                    model.Add("FTransferDirect", "GENERAL");
                    model.Add("FTransferBizType", pDataTable.Rows[0]["调拨类型"].ToString());//InnerOrgTransfer,OverOrgTransfer

                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["调出组织"].ToString());
                    model.Add("FSettleOrgId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["调出组织"].ToString());
                    model.Add("FSaleOrgId", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["调出组织"].ToString());
                    model.Add("FStockOutOrgId", basedata);

                    model.Add("FOwnerTypeOutIdHead", "BD_OwnerOrg");
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["调出组织"].ToString());
                    model.Add("FOwnerOutIdHead", basedata);

                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["调入组织"].ToString());
                    model.Add("FStockOrgId", basedata);

                    model.Add("FIsIncludedTax", true);
                    model.Add("FIsPriceExcludeTax", true);
                    model.Add("FOwnerTypeIdHead", "BD_OwnerOrg");
                    basedata = new JObject();
                    basedata.Add("FNumber", "PRE001");
                    model.Add("FSETTLECURRID", basedata);
                    basedata = new JObject();
                    basedata.Add("FNumber", pDataTable.Rows[0]["调入组织"].ToString());
                    model.Add("FOwnerIdHead", basedata);

                    basedata = new JObject();
                    basedata.Add("FNumber", "PRE001");
                    model.Add("FBaseCurrId", basedata);

                    // 开始构建单据体参数：集合参数JArray
                    JArray entryRows = new JArray();
                    string entityKey = "FBillEntry";
                    model.Add(entityKey, entryRows);

                    for (int i = 0; i < pDataTable.Rows.Count; i++)
                    {
                        // 添加新行，把新行加入到单据体行集合
                        JObject entryRow = new JObject();
                        entryRows.Add(entryRow);

                        // 单据体主键：必须填写，系统据此判断是新增还是修改行
                        //entryRow.Add("FEntryID", 0);
                        entryRow.Add("FIsNew", true);

                        //物料(FMaterialId)：基础资料，填写编码
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["物料编码"].ToString());
                        entryRow.Add("FMaterialId", basedata);
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["单位"].ToString());
                        entryRow.Add("FUnitID", basedata);
                        entryRow.Add("FQty", decimal.Parse(pDataTable.Rows[i]["调拨数量"].ToString()));
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["调出仓库"].ToString());
                        entryRow.Add("FSrcStockId", basedata);

                        //调出仓位
                        string cw2 = pDataTable.Rows[i]["FV2"].ToString().Trim();
                        if (cw2 != "0")
                        {
                            JObject sp2 = new JObject();
                            sp2.Add("FNumber", pDataTable.Rows[i]["调出仓位"].ToString());
                            basedata = new JObject();
                            basedata.Add("FSRCSTOCKLOCID__FF" + pDataTable.Rows[i]["FVV2"].ToString(), sp2);
                            entryRow.Add("FSrcStockLocId", basedata);
                        }

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["调入仓库"].ToString());
                        entryRow.Add("FDestStockId", basedata);

                        //调入仓位
                        string cw = pDataTable.Rows[i]["FV"].ToString().Trim();
                        if (cw != "0")
                        {
                            JObject sp = new JObject();
                            sp.Add("FNumber", pDataTable.Rows[i]["调入仓位"].ToString());
                            basedata = new JObject();
                            basedata.Add("FDESTSTOCKLOCID__FF" + pDataTable.Rows[i]["FVV"].ToString(), sp);                            
                            entryRow.Add("FDestStockLocId", basedata);
                        }

                        basedata = new JObject();
                        basedata.Add("FNumber", "KCZT01_SYS");
                        entryRow.Add("FSrcStockStatusId", basedata);
                        basedata = new JObject();
                        basedata.Add("FNumber", "KCZT01_SYS");
                        entryRow.Add("FDestStockStatusId", basedata);

                        entryRow.Add("FBusinessDate", DateTime.Now);

                        entryRow.Add("FOwnerTypeOutId", "BD_OwnerOrg");
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["调出组织"].ToString());
                        entryRow.Add("FOwnerOutId", basedata);
                        entryRow.Add("FOwnerTypeId", "BD_OwnerOrg");
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["调入组织"].ToString());
                        entryRow.Add("FOwnerId", basedata);

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["单位"].ToString());
                        entryRow.Add("FBaseUnitId", basedata);

                        entryRow.Add("FBaseQty", decimal.Parse(pDataTable.Rows[i]["调拨数量"].ToString()));
                        entryRow.Add("FISFREE", false);

                        entryRow.Add("FKeeperTypeId", "BD_KeeperOrg");
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["调入组织"].ToString());
                        entryRow.Add("FKeeperId", basedata);
                        entryRow.Add("FKeeperTypeOutId", "BD_KeeperOrg");
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["调出组织"].ToString());
                        entryRow.Add("FKeeperOutId", basedata);

                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["物料编码"].ToString());
                        entryRow.Add("FDestMaterialId", basedata);
                        basedata = new JObject();
                        basedata.Add("FNumber", pDataTable.Rows[i]["单位"].ToString());
                        entryRow.Add("FPriceUnitID", basedata);
                        entryRow.Add("FPriceQty", decimal.Parse(pDataTable.Rows[i]["调拨数量"].ToString()));
                        entryRow.Add("FPriceBaseQty", decimal.Parse(pDataTable.Rows[i]["调拨数量"].ToString()));
                        entryRow.Add("FTransReserveLink", false);
                        entryRow.Add("F_SWH_Qty", decimal.Parse(pDataTable.Rows[i]["调拨数量"].ToString()));
                        entryRow.Add("FCheckDelivery", false);

                        //创建与源单之间的关联关系，以支持上查与反写源单
                        entryRow.Add("FSrcBillType", "STK_TRANSFERAPPLY");
                        entryRow.Add("FSrcBillNo", pDataTable.Rows[i]["调拨申请单号"].ToString());

                        JArray linkRows = new JArray();
                        string linkEntityKey = string.Format("{0}_Link", entityKey);
                        entryRow.Add(linkEntityKey, linkRows);

                        JObject linkRow = new JObject();
                        linkRows.Add(linkRow);

                        //FFlowId : 业务流程图，可选
                        string fldFlowIdKey = string.Format("{0}_FFlowId", linkEntityKey);
                        linkRow.Add(fldFlowIdKey, "");
                        //FFlowLineId ：业务流程图路线，可选
                        string fldFlowLineIdKey = string.Format("{0}_FFlowLineId", linkEntityKey);
                        linkRow.Add(fldFlowLineIdKey, "");

                        string fldRuleIdKey = string.Format("{0}_FRuleId", linkEntityKey);
                        linkRow.Add(fldRuleIdKey, "StkTransferApply-StkTransferDirect");
                        string fldSTableNameKey = string.Format("{0}_FSTableName", linkEntityKey);
                        linkRow.Add(fldSTableNameKey, "T_STK_STKTRANSFERAPPENTRY");

                        string fldSBillIdKey = string.Format("{0}_FSBillId", linkEntityKey);
                        linkRow.Add(fldSBillIdKey, int.Parse(pDataTable.Rows[i]["FID"].ToString()));
                        string fldSIdKey = string.Format("{0}_FSId", linkEntityKey);
                        linkRow.Add(fldSIdKey, int.Parse(pDataTable.Rows[i]["FENTRYID"].ToString())); ;

                        if (i > 0)
                            fsKeys += ",";
                        fsKeys += pDataTable.Rows[i]["fskey"].ToString();
                    }

                    // 调用Web API接口服务，保存单据
                    BillNo = client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save", new object[] { "STK_TransferDirect", jsonRoot.ToString() });
                    JObject jo = JObject.Parse(BillNo);

                    if (!jo["Result"]["ResponseStatus"]["IsSuccess"].Value<bool>())
                    {
                        BillNo = string.Empty;
                        for (int i = 0; i < ((IList)jo["Result"]["ResponseStatus"]["Errors"]).Count; i++)
                            BillNo += jo["Result"]["ResponseStatus"]["Errors"][i]["Message"].Value<string>() + "\r\n";//保存不成功返错误信息

                        //反写失败状态
                        UpdateXBT_DataFSBS(fsKeys, false);
                    }
                    else
                    {
                        BillNo = "ID:" + jo["Result"]["Id"].Value<string>() + ";Number:" + jo["Result"]["Number"].Value<string>();//保存成功返回入库单FID和单据编号FBILLNO

                        client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Submit", new object[] { "STK_TransferDirect", "{\"CreateOrgId\":\"0\",\"Numbers\":[\"" + jo["Result"]["Number"].Value<string>() + "\"]}" });//根据单号提交单据
                        client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Audit", new object[] { "STK_TransferDirect", "{\"CreateOrgId\":\"0\",\"Numbers\":[\"" + jo["Result"]["Number"].Value<string>() + "\"]}" });//根据单号审核单据

                        //反写成功状态
                        UpdateXBT_DataFSBS(fsKeys, true);
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return BillNo;
        }

        #region 委外


        #endregion
        #endregion


        //未测试
        //----------------



        #region fina
        /// <summary>
        /// 单据生成成功 更新xbt_data fsbs标识
        /// </summary>
        /// <param name="pfskeys"></param>
        /// <param name="pIsSuccess">fsbs：0、未执行接口操作；1、生成成功；2、生成失败</param>
        private void UpdateXBT_DataFSBS(string pfskeys, bool pIsSuccess)
        {
            string sql;
            if (pIsSuccess)
                sql = string.Format("UPDATE xbt_data SET fsbs = 1 WHERE fskey IN({0})", pfskeys);
            else
                sql = string.Format("UPDATE xbt_data SET fsbs = 2 WHERE fskey IN({0})", pfskeys);
            SQLHelper.ExecuteNonQuery(sql);
        }
        /// <summary>
        /// 单据生成失败，反写flag
        /// </summary>
        /// <param name="pEntity">指令实体</param>
        private void UpdateXBT_UpOrderFlag(UpOrderInfo pEntity)
        {
            string sql = string.Format("UPDATE xbt_uporder SET flag = 1 WHERE fscjqh = '{0}' AND fslxbs = '{1}'", pEntity.Fscjqh, pEntity.Fslxbs);
            SQLHelper.ExecuteNonQuery(sql);
        }
        /// <summary>
        /// 删除指令
        /// </summary>
        /// <param name="pfskey"></param>
        private void DelOrders(int pfskey)
        {
            string sql = string.Format("DELETE FROM xbt_uporder WHERE fskey = {0}", pfskey.ToString());
            SQLHelper.ExecuteNonQuery(sql);
        }

        //----------------------------------------------

        /// <summary>
        /// 采购入库成功 反写采购订单关联数量
        /// </summary>
        /// <param name="pList"></param>
        private void UpdateT_Pur_Poorderentry_R_QTY(Dictionary<string, float> pList)
        {
            string sql = string.Empty;
            foreach (KeyValuePair<string, float> lst in pList)
            {
                sql += string.Format(" UPDATE t_pur_poorderentry_r SET FJOINQTY = FJOINQTY + {1},FBASEJOINQTY = FBASEJOINQTY + {1} WHERE FENTRYID = {0} ", lst.Key, lst.Value);//,FREMAINSTOCKINQTY = FREMAINSTOCKINQTY - {1}FSTOCKINQTY = FSTOCKINQTY + {1},FBASESTOCKINQTY = FBASESTOCKINQTY + {1},FSTOCKBASESTOCKINQTY = FSTOCKBASESTOCKINQTY + {1},
            }

            SQLHelper.ExecuteNonQuery(sql);
        }
        /// <summary>
        /// 采购入库成功 反写收料通知单入库数量
        /// </summary>
        /// <param name="pList"></param>
        private void UpdateT_Pur_ReceiveEntry_S_QTY(Dictionary<string, float> pList)
        {
            //string sql = string.Empty;
            //foreach (KeyValuePair<string, float> lst in pList)
            //{
            //    sql += string.Format(" UPDATE T_PUR_ReceiveEntry_s SET FJOINBASEQTY = FJOINBASEQTY + {1} WHERE FENTRYID = {0} ", lst.Key, lst.Value);//FINSTOCKBASEQTY = FINSTOCKBASEQTY + {1},
            //}

            //SQLHelper.ExecuteNonQuery(sql);
        }
        /// <summary>
        /// 采购退料成功 反写采购入库单关联数量
        /// </summary>
        /// <param name="pList"></param>
        private void UpdateT_Stk_instockEntry_QTY(Dictionary<string, float> pList)
        {
            //string sql = string.Empty;
            //foreach (KeyValuePair<string, float> lst in pList)
            //{
            //    sql += string.Format(" UPDATE T_Stk_InstockEntry SET FRETURNJOINQTY = FRETURNJOINQTY + {1},FBASERETURNJOINQTY = FBASERETURNJOINQTY + {1},FBASEJOINQTY = FBASEJOINQTY + {1} WHERE FENTRYID = {0} ", lst.Key, lst.Value);
            //}

            //SQLHelper.ExecuteNonQuery(sql);
        }
        /// <summary>
        /// 销售出库成功 反写发货通知单关联数量
        /// </summary>
        /// <param name="pList"></param>
        private void UpdateT_SAL_DELIVERYNOTICEENTRY_QTY(Dictionary<string, float> pList)
        {
            string sql = string.Empty;
            foreach (KeyValuePair<string, float> lst in pList)
            {
                sql += string.Format(" UPDATE T_SAL_DELIVERYNOTICEENTRY SET FJOINOUTQTY = FJOINOUTQTY + {1},FBASEJOINOUTQTY = FBASEJOINOUTQTY + {1} WHERE FENTRYID = {0} ", lst.Key, lst.Value);//,FSUMOUTQTY = FSUMOUTQTY + {1},FBASESUMOUTQTY = FBASESUMOUTQTY + {1}
                sql += string.Format(" UPDATE T_SAL_DELIVERYNOTICEENTRY_E SET FSTOCKBASEJOINOUTQTY = FSTOCKBASEJOINOUTQTY + {1} WHERE FENTRYID = {0} ", lst.Key, lst.Value);//,FSTOCKBASESUMOUTQTY = FSTOCKBASESUMOUTQTY + {1}
            }

            SQLHelper.ExecuteNonQuery(sql);
        }
        /// <summary>
        /// 销售退货成功 反写销售出库单关联数量
        /// </summary>
        /// <param name="pList"></param>
        private void UpdateT_SAL_OUTSTOCKENTRY_R_QTY(Dictionary<string, float> pList)
        {
            string sql = string.Empty;
            foreach (KeyValuePair<string, float> lst in pList)
            {
                sql += string.Format(" UPDATE T_SAL_OUTSTOCKENTRY_R SET FRETURNQTY = FRETURNQTY + {1},FBASERETURNQTY = FBASERETURNQTY + {1},FSTOCKBASERETURNQTY = FSTOCKBASERETURNQTY + {1},FSUMRETSTOCKQTY = FSUMRETSTOCKQTY + {1},FBASESUMRETSTOCKQTY = FBASESUMRETSTOCKQTY + {1},FSTOCKBASESUMRETSTOCKQTY = FSTOCKBASESUMRETSTOCKQTY + {1} WHERE FENTRYID = {0} ", lst.Key, lst.Value);
            }

            SQLHelper.ExecuteNonQuery(sql);
        }
        /// <summary>
        /// 生产入库成功 反写生产订单关联数量
        /// </summary>
        /// <param name="pList"></param>
        private void UpdateT_PRD_MOENTRY_Q_QTY(Dictionary<string, float> pList)
        {
            //string sql = string.Empty;
            //foreach (KeyValuePair<string, float> lst in pList)
            //{
            //    //sql += string.Format(" UPDATE T_PRD_MOENTRY_A SET FSTOCKINQUAAUXQTY = FSTOCKINQUAAUXQTY + {1},FSTOCKINQUAQTY = FSTOCKINQUAQTY + {1},FSTOCKINQUASELAUXQTY = FSTOCKINQUASELAUXQTY + {1},FSTOCKINQUASELQTY = FSTOCKINQUASELQTY + {1} WHERE FENTRYID = {0} ", lst.Key, lst.Value);
            //    //sql += string.Format(" UPDATE T_PRD_MOENTRY_Q SET FNOSTOCKINQTY = FNOSTOCKINQTY - {1},FBASENOSTOCKINQTY = FBASENOSTOCKINQTY - {1} WHERE FENTRYID = {0} ", lst.Key, lst.Value);
            //}

            //SQLHelper.ExecuteNonQuery(sql);
        }

        /// <summary>
        /// 生产领料领料成功 反写生产用料清单已领数量
        /// </summary>
        /// <param name="pList"></param>
        private void UpdateT_PRD_PPBOMENTRY_Q_FPICKEDQTY(Dictionary<string, float> pList)
        {
            string sql = string.Empty;
            foreach (KeyValuePair<string, float> lst in pList)
            {
                sql += string.Format(" UPDATE T_PRD_PPBOMENTRY_Q SET FPICKEDQTY = FPICKEDQTY + {1},FBASEPICKEDQTY = FBASEPICKEDQTY + {1},FSELPICKEDQTY = FSELPICKEDQTY + {1},FBASESELPICKEDQTY = FBASESELPICKEDQTY + {1} WHERE FENTRYID = {0} UPDATE A SET FNOPICKEDQTY = E.FMUSTQTY - FPICKEDQTY,FBASENOPICKEDQTY = E.FMUSTQTY - FPICKEDQTY FROM T_PRD_PPBOMENTRY_Q A INNER JOIN T_PRD_PPBOMENTRY E ON A.FENTRYID = E.FENTRYID WHERE FENTRYID = {0}", lst.Key, lst.Value);
            }

            SQLHelper.ExecuteNonQuery(sql);
        }
        /// <summary>
        /// 生产领料成功 反写生产订单 领料状态
        /// </summary>
        /// <param name="pmoentryids">领料状态：1、未领料；2、部分领料；3、全部领料；4、超额领料。全部领料后反写生产订单业务状态未 结案</param>
        private void UpdateMoEntry_Q_FPICKMTRLSTATUS(string pmoentryids)
        {

            //string sql = string.Format("UPDATE A SET FPICKMTRLSTATUS = CASE WHEN PQ.FPICKEDQTY = 0 THEN 1 WHEN PE.FMUSTQTY > PQ.FPICKEDQTY THEN 2 WHEN PE.FMUSTQTY = PQ.FPICKEDQTY THEN 3 ELSE 4 END FROM T_PRD_MOENTRY_Q A INNER JOIN T_PRD_PPBOMENTRY PE ON PE.FMOENTRYID = A.FENTRYID INNER JOIN T_PRD_PPBOMENTRY_Q PQ ON PE.FENTRYID = PQ.FENTRYID WHERE A.FENTRYID IN({0}) UPDATE A SET FSTATUS = CASE WHEN PE.FMUSTQTY = FBASEPICKEDQTY THEN 6 ELSE FSTATUS END FROM T_PRD_MOENTRY_A A INNER JOIN T_PRD_PPBOMENTRY PE ON PE.FMOENTRYID = A.FENTRYID INNER JOIN T_PRD_PPBOMENTRY_Q PQ ON PE.FENTRYID = PQ.FENTRYID WHERE A.FENTRYID IN({0})", pmoentryids);
            string sql = string.Format(C_UpdateMo, pmoentryids);
            SQLHelper.ExecuteNonQuery(sql);
        }
        /// <summary>
        /// 生产退料成功 反写生产领料单关联数量
        /// </summary>
        /// <param name="pList"></param>
        private void UpdateT_PRD_PICKMTRLDATA_QTY(Dictionary<string, float> pList)
        {
            //string sql = string.Empty;
            //foreach (KeyValuePair<string, float> lst in pList)
            //{
            //    sql += string.Format(" UPDATE T_PRD_PICKMTRLDATA SET FSELPRCDRETURNQTY = FSELPRCDRETURNQTY + {1},FBASESELPRCDRETURNQTY = FBASESELPRCDRETURNQTY + {1} WHERE FENTRYID = {0} ", lst.Key, lst.Value);
            //}

            //SQLHelper.ExecuteNonQuery(sql);
        }
        /// <summary>
        /// 日志
        /// </summary>
        /// <param name="pFounc"></param>
        /// <param name="pContext"></param>
        private void ExecuteLog(string pFounc, string pContext)
        {
            string sql = string.Format("INSERT INTO DM_ExecuteLog(Fouc,Context) VALUES('{0}','{1}')", pFounc, pContext);
            SQLHelper.ExecuteNonQuery(sql);
        }
        #endregion
    }
}
