using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.LambdaExtension.Extentions;
using Microsoft.Data.Sqlite;
using SQLitePCL;

namespace ConsoleApp1
{
    public class SqliteBase
    {
        protected string Connstr = "data source=file::memory:?cache=shared;Cache=shared;";

        private static SqliteConnection memdb;

        public SqliteBase()
        {
 
            // base(connstr);
        }

        public SqliteBase(string connstr)
        {
            Connstr = connstr;
        }

        public IDbConnection GetConnection()
        {
            var conn = new SqliteConnection(Connstr); //or  Microsoft.Data.Sqlite.SqliteConnection

            //raw.sqlite3_backup_init(conn.Handle,)



            conn.Open();
            return conn;
        }

        public IDbConnection GetStaticConnection()
        {
            var conn = new SqliteConnection(Connstr); //or  Microsoft.Data.Sqlite.SqliteConnection

            //raw.sqlite3_backup_init(conn.Handle,)
            


            conn.Open();
            return conn;
        }

        public void TestStr()
        {
            var builder = new SqliteConnectionStringBuilder(Connstr);
            
            string str = builder.DataSource;
            int num = 0;
            if (str.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
                num |= 64;
            int flags;
            switch (builder.Mode)
            {
                case SqliteOpenMode.ReadWrite:
                    flags = num | 2;
                    break;
                case SqliteOpenMode.ReadOnly:
                    flags = num | 1;
                    break;
                case SqliteOpenMode.Memory:
                    flags = num | 134;
                    if ((flags & 64) == 0)
                    {
                        flags |= 64;
                        str = "file:" + str;
                        break;
                    }
                    break;
                default:
                    flags = num | 6;
                    break;
            }
            switch (builder.Cache)
            {
                case SqliteCacheMode.Private:
                    flags |= 262144;
                    break;
                case SqliteCacheMode.Shared:
                    flags |= 131072;
                    break;
            }
            string data = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
            if (!string.IsNullOrEmpty(data) && (flags & 64) == 0 && (!str.Equals(":memory:", StringComparison.OrdinalIgnoreCase) && !Path.IsPathRooted(str)))
                str = Path.Combine(data, str);


            Console.WriteLine(str);
        }

        public void InitFromFile()
        {
            var dbfilePath = "e:\\testcsv.sqlite3";

            memdb = (SqliteConnection)GetStaticConnection();
            
               var ret=LodOrSaveDb(memdb.Handle, dbfilePath, false);

                Console.WriteLine("backup error code: "+ret);

                //var tables = memdb.Query("select name from sqlite_master where type='table' order by name;");

                //Console.WriteLine(tables.Count());

                var counts = memdb.QueryFirst<int>("select count(*) as counts from test1;");

                Console.WriteLine("table test1 has rows : "+counts);

               //var progres = Process.GetCurrentProcess();

               // var mem2 = progres.WorkingSet64 / (1024d * 1024d);// memory usage in Mb;
               // Console.WriteLine($"Memory Usage :\t\t{mem2} Mb.");
           
        }


        public int LodOrSaveDb(sqlite3 memoryDb, string dbfilePath, bool isSave)
        {
            var ret = 1;
            using (var fileDb = new SqliteConnection($"Data Source={dbfilePath};"))
            {
                fileDb.Open();

                var from = isSave ? memoryDb : fileDb.Handle;
                var to = isSave ? fileDb.Handle : memoryDb;
                var backup = raw.sqlite3_backup_init(to, "main", from, "main");

                if (backup != null)
                {
                    var step=raw.sqlite3_backup_step(backup, -1);
                    Console.WriteLine(" step result : "+step);
                   var finish= raw.sqlite3_backup_finish(backup);

                    Console.WriteLine("finish result : "+finish);
                }

                ret = raw.sqlite3_errcode(to);
            }

            

            return ret;
        }



        public IDbConnection GetConnection(string strConn)
        {
             
            var conn = new SqliteConnection(strConn);//or  Microsoft.Data.Sqlite.SqliteConnection
            conn.Open();
            return conn;
        }


        public Tuple<bool, string> TestConn(string connstring)
        {
            bool isopen = false;
            string msg = string.Empty;

            try
            {
                var conn = GetConnection(connstring);
                if (conn.State == ConnectionState.Open)
                {
                    isopen = true;
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }


            return new Tuple<bool, string>(isopen, msg);
        }
        public Tuple<bool, string> TestConn()
        {
            bool isopen = false;
            string msg = string.Empty;

            try
            {
                var conn = GetConnection(Connstr);
                if (conn.State == ConnectionState.Open)
                {
                    isopen = true;
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }


            return new Tuple<bool, string>(isopen, msg);
        }
    }
}
