﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IDAL;
using Model;
using DBUtility;
using System.Data.SqlClient;
using Common;
using System.Data;

namespace SQLServerDAL
{
    public class SailBill:ISailBill
    {
        private void InsertDetail(SailBillBody ckbody, SqlTransaction trans)
        {
            string sql = @"INSERT INTO [SailBillBody]
                                   ([HeadId]
                                   ,[ProductID]
                                   ,[SailPrice]
                                   ,[Num])
                             VALUES
                                   (@HeadId
                                   ,@ProductID
                                   ,@SailPrice
                                   ,@Num)";
            SqlParameter[] spvalues = DBTool.GetSqlPm(ckbody);
            SqlHelper.ExecuteNonQuery(trans, CommandType.Text, sql, spvalues);
        }


        /// <summary>
        /// 增加采购单
        /// </summary>
        /// <param name="changeStock"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public int InsertSailBill(SailBillInfo saleBill, SqlTransaction trans)
        {
            Guid g = Guid.NewGuid();
            saleBill.id = g;
            string sql = @"INSERT INTO [ProductBillHead]
                                   ([id]
                                   ,[BuyNO]
                                   ,[BatchNO]
                                   ,[BuyDate]
                                   ,[SupplyID]
                                   ,[IsReview]
                                   ,[ReviewUser]
                                   ,[WareHouseID]
                                   ,[Detail]
                                   ,[Define1]
                                   ,[Define2]
                                   ,[Define3]
                                   ,[InsertDateTime]
                                   ,[InsertUser])
                             VALUES
                                   (@id
                                   ,@BuyNO
                                   ,@BatchNO
                                   ,@BuyDate
                                   ,@SupplyID
                                   ,@IsReview
                                   ,@ReviewUser
                                   ,@WareHouseID
                                   ,@Detail
                                   ,@Define1
                                   ,@Define2
                                   ,@Define3
                                   ,@InsertDateTime
                                   ,@InsertUser)";
            SqlParameter[] spvalues = DBTool.GetSqlPm(saleBill);
            int res = SqlHelper.ExecuteNonQuery(trans, CommandType.Text, sql, spvalues);
            foreach (SailBillBody ckb in saleBill.SaleBillDetail)
            {
                ckb.HeadId = g;
                InsertDetail(ckb, trans);
            }
            return res;
        }
        /// <summary>
        /// 更新采购单
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public int UpdateSailBill(SailBillInfo saleBill,bool changebody, SqlTransaction trans)
        {
            string sql = @"UPDATE [SailBillHead]
                           SET [id] = @id
                              ,[WareHouseID] = @WareHouseID
                              ,[SailNO] = @SailNO
                              ,[SailDate] = @SailDate
                              ,[IsReview] = @IsReview
                              ,[ReviewUser] = @ReviewUser
                              ,[StuffID] = @StuffID
                              ,[Detail] = @Detail
                              ,[Define1] = @Define1
                              ,[Define2] = @Define2
                              ,[Define3] = @Define3
                              ,[UpdateDateTime] = @UpdateDateTime
                              ,[UpdateUser] = @UpdateUser
                         WHERE id=@id";
            SqlParameter[] spvalues = DBTool.GetSqlPm(saleBill);
            int res = SqlHelper.ExecuteNonQuery(trans, CommandType.Text, sql, spvalues);
            if (changebody)
            {
                DeleteDetail(saleBill.id, trans);
                foreach (SailBillBody ckb in saleBill.SaleBillDetail)
                {
                    InsertDetail(ckb, trans);
                }
            }
            return res;
        }
        private void DeleteDetail(Guid headId, SqlTransaction trans)
        {
            string sql = @"DELETE FROM [SailBillBody] WHERE HeadId=@HeadId";
            SqlParameter sp = new SqlParameter("@HeadId", headId);
            SqlHelper.ExecuteNonQuery(trans, CommandType.Text, sql, sp);
        }

        /// <summary>
        /// 根据分类id删除采购单
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public int DeleteSailBill(Guid saleBillId, SqlTransaction trans)
        {
            DeleteDetail(saleBillId, trans);
            string sql = @"DELETE FROM [SailBillHead] WHERE id=@id";
            SqlParameter sp = new SqlParameter("@id", saleBillId);
            return SqlHelper.ExecuteNonQuery(trans, CommandType.Text, sql, sp);
        }
        /// <summary>
        /// 根据条件获取采购单
        /// </summary>
        /// <param name="conditon"></param>
        /// <returns></returns>
        public IList<SailBillInfo> GetSailBill(IEnumerable<SearchCondition> conditon, SqlConnection conn)
        {
            IList<SailBillInfo> l = new List<SailBillInfo>();
            string sql = @"SELECT [id]
                                  ,[WareHouseID]
                                  ,[SailNO]
                                  ,[SailDate]
                                  ,[IsReview]
                                  ,[ReviewUser]
                                  ,[StuffID]
                                  ,[Detail]
                                  ,[Define1]
                                  ,[Define2]
                                  ,[Define3]
                                  ,[InsertDateTime]
                                  ,[InsertUser]
                                  ,[UpdateDateTime]
                                  ,[UpdateUser]
                              FROM [SailBillHead]";
            if (conditon.Count() > 0)
            {
                string con = DBTool.GetSqlcon(conditon);
                sql += " where " + con;
            }
            SqlParameter[] spvalues = DBTool.GetSqlParam(conditon);
            DataTable dt = SqlHelper.Squery(sql, conn, spvalues);
            l = DBTool.GetListFromDatatable<SailBillInfo>(dt);
            foreach (SailBillInfo csi in l)
            {
                csi.SaleBillDetail = GetDetail(csi.id, conn);
            }
            return l;
        }

        private IEnumerable<SailBillBody> GetDetail(Guid headId, SqlConnection conn)
        {
            IList<SailBillBody> lckbody = new List<SailBillBody>();
            string sql = @"SELECT [HeadId]
                                  ,[ProductID]
                                  ,[SailPrice]
                                  ,[Num]
                              FROM [SailBillBody] where HeadId=@HeadId";
            SqlParameter sp = new SqlParameter("@HeadId", headId);
            DataTable dt = SqlHelper.Squery(sql, conn, sp);
            lckbody = DBTool.GetListFromDatatable<SailBillBody>(dt);
            return lckbody;
        }
        /// <summary>
        /// 获取满足条件的采购单数量
        /// </summary>
        /// <param name="conditon"></param>
        /// <returns></returns>
        public int GetSailBillCount(IEnumerable<SearchCondition> conditon, SqlConnection conn)
        {
            string sql = @"SELECT count(*) as count FROM [SailBillHead]";
            if (conditon.Count() > 0)
            {
                string con = DBTool.GetSqlcon(conditon);
                sql += " where " + con;
            }
            SqlParameter[] spvalues = DBTool.GetSqlParam(conditon);
            int count = (int)SqlHelper.ExecuteScalar(conn, CommandType.Text, sql, spvalues);
            return count;
        }
        /// <summary>
        /// 获取指定页的采购单
        /// </summary>
        /// <param name="conditon"></param>
        /// <param name="page">页数</param>
        /// <param name="pagesize">每页数量</param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public IList<SailBillInfo> GetPageSailBill(IEnumerable<SearchCondition> conditon, int page, int pagesize, SqlConnection conn)
        {
            IList<SailBillInfo> l = new List<SailBillInfo>();
            string sql = @"SELECT [id]
                                  ,[WareHouseID]
                                  ,[SailNO]
                                  ,[SailDate]
                                  ,[IsReview]
                                  ,[ReviewUser]
                                  ,[StuffID]
                                  ,[Detail]
                                  ,[Define1]
                                  ,[Define2]
                                  ,[Define3]
                                  ,[InsertDateTime]
                                  ,[InsertUser]
                                  ,[UpdateDateTime]
                                  ,[UpdateUser]
                                  ,ROW_NUMBER() over(order by InsertDateTime) as row
                          FROM [SailBillHead] ";
            if (conditon.Count() > 0)
            {
                string con = DBTool.GetSqlcon(conditon);
                sql += " where " + con;
            }
            sql = "select * from (" + sql + ") as a where row>" + (page - 1) * pagesize + " and row<=" + page * pagesize;
            SqlParameter[] spvalues = DBTool.GetSqlParam(conditon);
            DataTable dt = SqlHelper.Squery(sql, conn, spvalues);
            l = DBTool.GetListFromDatatable<SailBillInfo>(dt);
            foreach (SailBillInfo csi in l)
            {
                csi.SaleBillDetail = GetDetail(csi.id, conn);
            }
            return l;
        }
    }
}
