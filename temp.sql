
select a.fstockid,b.FSTOCKLOCID
,a.FNUMBER �ֿ����,al.FNAME �ֿ�����,ol.FNAME ʹ����֯
,fv.FNUMBER ��λֵ������,fvl.FNAME ��λֵ������,fve.FNUMBER ��λֵ����,fvel.FNAME ��λֵ����,fvel.FDESCRIPTION ��λֵ����
from T_BD_STOCK a
inner join T_BD_STOCK_L al on a.FSTOCKID = al.FSTOCKID and FLOCALEID = 2052
inner join T_BD_FLEXVALUESCOM b on a.FSTOCKID = b.FSTOCKID
inner join T_BD_STOCKFLEXITEM f on a.FSTOCKID = f.FSTOCKID
inner join T_BD_STOCKFLEXDETAIL fd on f.FENTRYID = fd.FENTRYID
inner join T_BAS_FLEXVALUES fv on f.FFLEXID = fv.FID
inner join T_BAS_FLEXVALUES_L fvl on fv.FID = fvl.FID and fvl.FLOCALEID = 2052
inner join T_BAS_FLEXVALUESENTRY fve on fv.FID = fve.FID
inner join T_BAS_FLEXVALUESENTRY_L fvel on fve.FENTRYID = fvel.FENTRYID and fvel.FLOCALEID = 2052
inner join T_ORG_ORGANIZATIONS_L ol on a.FUSEORGID = ol.FORGID and ol.FLOCALEID = 2052
where a.FSTOCKID = 152171

select * from T_BD_STOCK where FSTOCKID =152171
select * from T_BD_FLEXVALUESCOM where FSTOCKID =152171
select * from T_BD_STOCKFLEXITEM where FSTOCKID =152171
select * from T_BD_STOCKFLEXDETAIL where FENTRYID =100004
select * from T_BAS_FLEXVALUES where FID = 100008
select * from T_BAS_FLEXVALUES_L where FID =100008
select * from T_BAS_FLEXVALUESENTRY where FID  =100008
select * from T_BAS_FLEXVALUESENTRY_L where FENTRYID =100070