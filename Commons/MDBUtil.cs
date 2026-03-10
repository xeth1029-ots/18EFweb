using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Reflection;
using System.Web;
using WDAIIP.WEB.Models;

namespace WDAIIP.WEB.Commons
{
    /// <summary>
    /// 微軟 ACCESS (.mdb) 資料檔資料處理
    /// </summary>
    public class MDBUtil : WDAIIP.WEB.DataLayers.BaseDAO
    {
        /// <summary>將資料 Model 轉換成 Hashtable。</summary>
        /// <typeparam name="TEntity">資料 Model 型別。</typeparam>
        /// <param name="entity">資料 Model 物件。</param>
        /// <returns></returns>
        public static Hashtable EntityToHashList<TEntity>(TEntity entity)
        {
            var bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;
            PropertyInfo[] props = entity.GetType().GetProperties(bindingFlags);
            Hashtable ret = new Hashtable();

            foreach (PropertyInfo p in props)
            {
                object value = p.GetValue(entity);
                if (value != null)
                {
                    ret.Add(p.Name, value);
                }
            }

            return ret;
        }

        /// <summary>將資料 Model 轉換成 Hashtable。</summary>
        /// <typeparam name="TEntity">資料 Model 型別。</typeparam>
        /// <param name="src">資料 Model 集合。</param>
        /// <param name="dest">Hashtable 集合</param>
        /// <returns></returns>
        public static void EntityToHashList<TEntity>(IList<TEntity> src, IList<Hashtable> dest)
        {
            if (src == null) throw new ArgumentNullException("src");
            if (dest == null) throw new ArgumentNullException("dest");

            Hashtable row = null;
            PropertyInfo[] props = null;
            var bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;

            foreach (TEntity entity in src)
            {
                row = new Hashtable();
                props = entity.GetType().GetProperties(bindingFlags);
                foreach (PropertyInfo p in props)
                {
                    object value = p.GetValue(entity);
                    if (value != null)
                    {
                        row.Add(p.Name, value);
                    }
                }
                dest.Add(row);
            }
        }

        /// <summary>將資料寫入微軟 ACCESS (.mdb) 資料檔。</summary>
        /// <param name="model">要寫入的資料。</param>
        /// <param name="fileName">微軟 ACCESS (.mdb) 檔案路徑。</param>
        public static void Export(MDBModel model, string fileName)
        {
            //等總監補上使用 SqlMap 處理的程式碼。
            //string conn = string.Concat(@"Driver={Microsoft Access Driver (*.mdb)};DBQ=", fileName);

            OdbcConnection dbConn = new System.Data.Odbc.OdbcConnection(@"Driver={Microsoft Access Driver (*.mdb, *.accdb)};DBQ=" + fileName + ";Uid=;Pwd=;");
            OdbcCommand nonqueryCommand = dbConn.CreateCommand();

            try
            {
                dbConn.Open();

                // Create Table
                nonqueryCommand.CommandText = string.Format("CREATE TABLE {0} ({1})", model.TableName, string.Join(",", model.Fields.Select(s => string.Format("{0} {1}", s.Name, s.Type.Name))));
                nonqueryCommand.ExecuteNonQuery();
                if (model.Rows != null && model.Rows.Count != 0)
                {
                    // Make INSERT SQLCommand
                    nonqueryCommand.CommandText = string.Format("INSERT INTO {0} VALUES ({1})", model.TableName, string.Join(",", model.Fields.Select(s => "?")));
                    foreach (var item in model.Fields)
                    {
                        nonqueryCommand.Parameters.Add("@" + item.Name, OdbcType.NVarChar);
                    }

                    // nonqueryCommand.Prepare();
                    foreach (var item in model.Rows)
                    {
                        // Set Parameters
                        foreach (var f in model.Fields)
                        {
                            nonqueryCommand.Parameters["@" + f.Name].Value = (item[f.Name] != null ? item[f.Name] : " ");
                        }
                        nonqueryCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (OdbcException ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                dbConn.Close();
                Console.WriteLine("Connection Closed.");
            }
        }

        /// <summary>將微軟 ACCESS (.mdb) 資料檔寫入資料庫。</summary>
        /// <param name="model">要寫入的資料。</param>
        /// <param name="fileName">資料表路徑。</param>
        public static IList<Hashtable> Load(MDBModel model, string fileName)
        {
            //等總監補上使用 SqlMap 處理的程式碼。
            //string conn = string.Concat(@"Driver={Microsoft Access Driver (*.mdb)};DBQ=", fileName);
            string path = @"Driver={Microsoft Access Driver (*.mdb, *.accdb)};DBQ=" + fileName + ";Uid=;Pwd=;";
            OdbcConnection dbConn = new System.Data.Odbc.OdbcConnection(path);
            OdbcCommand nonqueryCommand = dbConn.CreateCommand();
            IList<Hashtable> table = new List<Hashtable>();
            try
            {
                dbConn.Open();

                // Make SELECT SQLCommand
                nonqueryCommand.CommandText += string.Format(" SELECT {1} FROM {0} ", model.TableName, string.Join(",", model.Fields.Select(s => s.Name)));
                OdbcDataReader reader = nonqueryCommand.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        Hashtable para = new Hashtable();
                        for (int i = 0; i < model.Fields.Count; i++)
                        {
                            para[model.Fields[i].Name] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                        }
                        table.Add(para);
                    }
                }

            }
            catch (OdbcException ex)
            {
                Console.WriteLine(ex.Message);//ex.ToString()
            }
            finally
            {
                dbConn.Close();
                Console.WriteLine("Connection Closed.");
            }
            return table;
        }
    }
}
