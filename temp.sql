
dm_getorders

select * from dm_executelog  --truncate table dm_executelog --delete from dm_executelog where id in(25,26)

--update xbt_data set Fspno = 'SCRK00001538',fsid = 101596,Fsentry = 103395,fssl = 8,fshpid = 194328 where fskey = 2
--update xbt_data set Fspno = 'SCRK00001538',fsid = 101596,Fsentry = 103396,fssl = 10,fshpid = 194621 where fskey = 3
--update xbt_data set fsckid = 195046 where fskey in(7,8)

select * from xbt_data where fsrklx in(8) and fsbs = 0	--update xbt_data set fsbs = 0 where fsrklx in(10)
select * from xbt_uporder where fslxbs in(10)	--update xbt_uporder set flag = 0 where fslxbs in(8) and fscjqh = 32
--insert into xbt_uporder(fscjqh,fslxbs,fsuser,fspwd,flag,fszl)values('31','2',116520,'111111',0,'Y')

--update xbt_data set FsDepartid = 122110 where fsrklx in(34,35)

update xbt_data set fsbs = 1 where fskey in(35,36,37,38,39,40,41)
update xbt_data set fsbs = 0 where fskey in(1)
delete from xbt_data where fskey = 80
delete from xbt_uporder where fskey in(127,128)
update xbt_uporder set flag = 1 where fslxbs <> 1
--采购订单-采购入库
INSERT INTO xbt_data(fspno,fsid,fsgysbh,fsckid,fscwid,fssl,fshpid,fsmen,fownerid,fscjqh,fsrklx,fsbs,forgid,Fsentry,fsunit)
values('PO-2020-11-00051',100170,122041,218096,100068,12,194043,110616,119179,31,11,0,119179,100543,10101)
insert into xbt_uporder(fscjqh,fslxbs,fsuser,fspwd,flag,fszl)values('31','11',110836,'111111',0,'Y')
--采购入库-采购退料
INSERT INTO xbt_data(fspno,fsid,fsgysbh,fsckid,fscwid,fssl,fshpid,fsmen,fownerid,fscjqh,fsrklx,fsbs,forgid,Fsentry,fsunit)
values('HS-20201130-00002',112777,122041,218096,100068,2,194043,110616,119179,31,9,0,119179,117322,10101)
insert into xbt_uporder(fscjqh,fslxbs,fsuser,fspwd,flag,fszl)values('31','9',110836,'111111',0,'Y')
--销售出库-销售退货
INSERT INTO xbt_data(fspno,fsid,fsgysbh,fsckid,fscwid,fssl,fshpid,fsmen,fownerid,fscjqh,fsrklx,fsbs,forgid,Fsentry,fsunit)
values('XSCKD002528',114496,0,0,0,2,194621,110616,119179,31,4,0,119179,122021,10101)
insert into xbt_uporder(fscjqh,fslxbs,fsuser,fspwd,flag,fszl)values('31','4',110836,'111111',0,'Y')
--生产入库-生产退库
INSERT INTO xbt_data(fspno,fsid,fsgysbh,fsckid,fscwid,fssl,fshpid,fsmen,fownerid,fscjqh,fsrklx,fsbs,forgid,Fsentry,fsunit)
values('SCRK00001550',101636,119179,218100,100057,5,194094,110616,119179,31,21,0,119179,103464,10101)
insert into xbt_uporder(fscjqh,fslxbs,fsuser,fspwd,flag,fszl)values('31','21',110836,'111111',0,'Y')
--生产用料清单-生产领料
INSERT INTO xbt_data(fspno,fsid,fsgysbh,fsckid,fscwid,fssl,fshpid,fsmen,fownerid,fscjqh,fsrklx,fsbs,forgid,Fsentry,fsunit)
values('PPBOM00003006',105814,0,218095,100067,1,194265,110616,119179,31,6,0,119179,120241,10101)
insert into xbt_uporder(fscjqh,fslxbs,fsuser,fspwd,flag,fszl)values('31','6',110836,'111111',0,'Y')
--生产领料-生产退料
INSERT INTO xbt_data(fspno,fsid,fsgysbh,fsckid,fscwid,fssl,fshpid,fsmen,fownerid,fscjqh,fsrklx,fsbs,forgid,Fsentry,fsunit)
values('SOUT00000267',101914,119179,0,0,1,194265,110616,194326,31,5,0,119179,112137,10101)
insert into xbt_uporder(fscjqh,fslxbs,fsuser,fspwd,flag,fszl)values('31','5',110836,'111111',0,'Y')
--其他入库
INSERT INTO xbt_data(fspno,fsid,fsgysbh,fsckid,fscwid,fssl,fshpid,fsmen,fownerid,fscjqh,fsrklx,fsbs,forgid,Fsentry,fsunit)
values('',0,0,218095,100067,100,194265,110616,194326,31,3,0,119179,0,10101)
insert into xbt_uporder(fscjqh,fslxbs,fsuser,fspwd,flag,fszl)values('31','3',110836,'111111',0,'Y')
--其他入库
INSERT INTO xbt_data(fspno,fsid,fsgysbh,fsckid,fscwid,fssl,fshpid,fsmen,fownerid,fscjqh,fsrklx,fsbs,forgid,Fsentry,fsunit)
values('',0,122041,218095,100067,100,194265,110616,194326,31,8,0,119179,0,10101)
insert into xbt_uporder(fscjqh,fslxbs,fsuser,fspwd,flag,fszl)values('31','8',110836,'111111',0,'Y')
--组装拆卸单-组装



update xbt_data set fsgysbh = 122090 where fskey = 83
select * from T_BD_SUPPLIER where FNUMBER = '2001'
select * from T_BD_CUSTOMER where fnumber = '9001'

--仓位
select a.fstockid,b.FSTOCKLOCID
,a.FNUMBER 仓库编码,al.FNAME 仓库名称,ol.FNAME 使用组织
,fv.FNUMBER 仓位值集编码,fvl.FNAME 仓位值集名称,fve.FNUMBER 仓位值编码,fvel.FNAME 仓位值名称,fvel.FDESCRIPTION 仓位值描述
from T_BD_STOCK a
inner join T_BD_STOCK_L al on a.FSTOCKID = al.FSTOCKID and FLOCALEID = 2052
LEFT join T_BD_FLEXVALUESCOM b on a.FSTOCKID = b.FSTOCKID
LEFT join T_BD_STOCKFLEXITEM f on a.FSTOCKID = f.FSTOCKID
LEFT join T_BD_STOCKFLEXDETAIL fd on f.FENTRYID = fd.FENTRYID
LEFT join T_BAS_FLEXVALUES fv on f.FFLEXID = fv.FID
LEFT join T_BAS_FLEXVALUES_L fvl on fv.FID = fvl.FID and fvl.FLOCALEID = 2052
LEFT join T_BAS_FLEXVALUESENTRY fve on fv.FID = fve.FID
LEFT join T_BAS_FLEXVALUESENTRY_L fvel on fve.FENTRYID = fvel.FENTRYID and fvel.FLOCALEID = 2052
LEFT join T_ORG_ORGANIZATIONS_L ol on a.FUSEORGID = ol.FORGID and ol.FLOCALEID = 2052
where a.FNUMBER = 'CK023'



select * from T_BD_STOCK where FSTOCKID =152171
select * from T_BD_FLEXVALUESCOM where FSTOCKID =152171
select * from T_BD_STOCKFLEXITEM where FSTOCKID =152171
select * from T_BD_STOCKFLEXDETAIL where FENTRYID =100004
select * from T_BAS_FLEXVALUES where FID = 100008
select * from T_BAS_FLEXVALUES_L where FID =100008
select * from T_BAS_FLEXVALUESENTRY where FID  =100008
select * from T_BAS_FLEXVALUESENTRY_L where FENTRYID =100070














