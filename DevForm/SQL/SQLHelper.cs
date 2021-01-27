/**
 * 
 * DM Software
 * 
 * This is the confidential proprietary property of DM Software This document is
 * protected by copyright. No part of it may be reproduced or copied without the prior written
 * permission of DM Software DM products are supplied under licence and
 * may be used only in accordance with the terms of the contractual agreement between DM
 * and the licence holder. All products, brand names and trademarks referred to in this
 * publication are the property of DM or third party owners. Unauthorised use may
 * constitute an infringement. DM Software Inc reserves the right to change information
 * contained in this publication without notice. All efforts have been made to ensure accuracy
 * however DM Software Inc does not assume responsibility for errors or for any
 * consequences arising from errors in this publication.
 * 
 * 
 *                   Author:Damo
 *            Creation Date:Sep 24, 2020
 *              Description: 
 *            Last Modifier:
 *        Modification Date:
 *    Description of Change:   
 * ======== ======== =====================
 * 
 * 
 **/

namespace DevCesio.DevForm.SQL
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using Model.Global;

    /// <summary>
    /// 自定义SQLHelper
    /// </summary>
    internal static class SQLHelper
    {
        //ConnectionChecked
        public static string ConnectionChecked(string pConnectionString)
        {
            SqlConnection conn = new SqlConnection(pConnectionString);
            try
            {
                conn.Open();
                return "连接成功";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        //NonQuery
        public static string ExecuteNonQuery(string pCommandText)
        {
            SqlConnection conn = new SqlConnection(GlobalParameter.SQLInf.ConnectionString);
            try
            {
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = pCommandText;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
            return string.Empty;
        }

        //Scalar
        public static object ExecuteScalar(string pCommandText)
        {
            object o = new object();
            SqlConnection conn = new SqlConnection(GlobalParameter.SQLInf.ConnectionString);
            try
            {
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = pCommandText;
                o = cmd.ExecuteScalar();
            }
            catch { return null; }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
            return o;
        }
        public static object ExecuteScalar(string pCommandText, CommandType pCommandType, SqlParameter[] pParameters)
        {
            object o = new object();
            SqlConnection conn = new SqlConnection(GlobalParameter.SQLInf.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                CommandSetting(conn, cmd, pCommandType, pCommandText, null, pParameters);
                o = cmd.ExecuteScalar();
                cmd.Parameters.Clear();
            }
            catch (Exception ex) { return "Error:" + ex.Message; }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
            return o;
        }

        //DataTable
        public static DataTable ExecuteTable(string pCommandText)
        {
            DataTable dt = new DataTable();
            SqlConnection conn = new SqlConnection(GlobalParameter.SQLInf.ConnectionString);
            try
            {
                conn.Open();
                SqlDataAdapter adp = new SqlDataAdapter(pCommandText, conn);
                adp.Fill(dt);
            }
            catch { return null; }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
            return dt;
        }

        //DataSet

        //Private Custom Methods
        private static void CommandSetting(SqlConnection pConnection, SqlCommand pCommand, CommandType pCommandType, string pCommandText, SqlTransaction pTransation = null, SqlParameter[] pParameters = null)
        {
            if (pConnection.State != ConnectionState.Open)
                pConnection.Open();

            pCommand.Connection = pConnection;
            pCommand.CommandType = pCommandType;
            pCommand.CommandText = pCommandText;

            if (pCommandType == CommandType.StoredProcedure)
                pCommand.CommandTimeout = 10000;
            else
                pCommand.CommandTimeout = 500;

            if (pTransation != null)
                pCommand.Transaction = pTransation;

            if (pParameters != null)
            {
                foreach (SqlParameter parm in pParameters)
                    pCommand.Parameters.Add(parm);
            }
        }
    }
}
