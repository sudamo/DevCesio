
namespace DevCesio.DevForm.Model.XBT
{
    public enum WebApiType : int
    {
        /// <summary>
        /// 采购入库(收料通知单)
        /// </summary>
        收料通知单_采购入库 = 1,
        /// <summary>
        /// 采购入库(采购订单)
        /// </summary>
        采购订单_采购入库 = 11,
        /// <summary>
        /// 采购退货
        /// </summary>
        采购入库单_采购退料单 = 9,
        /// <summary>
        /// 销售出库
        /// </summary>
        发货通知_销售出库 = 7,
        /// <summary>
        /// 销售退货
        /// </summary>
        销售出库_销售退货 = 4,
        /// <summary>
        /// 生产入库
        /// </summary>
        生产订单_生产入库 = 2,
        /// <summary>
        /// 生产退库
        /// </summary>
        生产入库_生产退库 = 21,
        /// <summary>
        /// 生产领料
        /// </summary>
        生产用料清单_生产领料 = 6,
        /// <summary>
        /// 生产退料
        /// </summary>
        生产领料_生产退料 = 5,
        /// <summary>
        /// 其它入库
        /// </summary>
        其他入库 = 3,
        /// <summary>
        /// 其他出库
        /// </summary>
        其他出库 = 8,
        /// <summary>
        /// 组装
        /// </summary>
        组装拆卸单_组装 = 34,
        /// <summary>
        /// 拆装
        /// </summary>
        组装拆卸单_拆卸 = 35,
        /// <summary>
        /// 仓库调拨
        /// </summary>
        调拨申请单_直接调拨单 = 10,
        /// <summary>
        /// 委外领料
        /// </summary>
        委外用料清单_委外领料 = 32,
        /// <summary>
        /// 委外退料
        /// </summary>
        委外领料单_委外退料 = 33,
        /// <summary>
        /// (委外)采购订单_采购入库
        /// </summary>
        委外采购订单_委外入库 = 31,
        /// <summary>
        /// (委外)采购入库_生产退库
        /// </summary>
        委外入库_委外退库 = 37
    }
}
