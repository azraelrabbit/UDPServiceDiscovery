using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.Data.Sqlite.Properties;
using SQLitePCL;

namespace Microsoft.Data.Sqlite
{
    public class MySqliteConnection:SqliteConnection
    {
        private const string MainDatabaseName = "main";
        private string _connectionString;
        private ConnectionState _state;
        private sqlite3 _db;

        static MySqliteConnection()
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Data.Sqlite.SqliteConnection" /> class.
        /// </summary>
        public MySqliteConnection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Data.Sqlite.SqliteConnection" /> class.
        /// </summary>
        /// <param name="connectionString">The string used to open the connection.</param>
        /// <seealso cref="T:Microsoft.Data.Sqlite.SqliteConnectionStringBuilder" />
        public MySqliteConnection(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        /// <summary>Gets a handle to underlying database connection.</summary>
        /// <value>A handle to underlying database connection.</value>
        /// <seealso href="http://sqlite.org/c3ref/sqlite3.html">Database Connection Handle</seealso>
        public virtual sqlite3 Handle
        {
            get
            {
                return this._db;
            }
        }

        /// <summary>Gets or sets a string used to open the connection.</summary>
        /// <value>A string used to open the connection.</value>
        /// <seealso cref="T:Microsoft.Data.Sqlite.SqliteConnectionStringBuilder" />
        public override string ConnectionString
        {
            get
            {
                return this._connectionString;
            }
            set
            {
                if (this.State != ConnectionState.Closed)
                    throw new InvalidOperationException("ConnectionStringRequiresClosedConnection");
                this._connectionString = value;
                this.ConnectionStringBuilder = new SqliteConnectionStringBuilder(value);
            }
        }

        internal SqliteConnectionStringBuilder ConnectionStringBuilder { get; set; }

        /// <summary>Gets the name of the current database. Always 'main'.</summary>
        /// <value>The name of the current database.</value>
        public override string Database
        {
            get
            {
                return "main";
            }
        }

        /// <summary>
        /// Gets the path to the database file. Will be absolute for open connections.
        /// </summary>
        /// <value>The path to the database file.</value>
        public override string DataSource
        {
            get
            {
                string str = (string)null;
                if (this.State == ConnectionState.Open)
                    str = raw.sqlite3_db_filename(this._db, "main");
                return str ?? this.ConnectionStringBuilder.DataSource;
            }
        }

        /// <summary>Gets the version of SQLite used by the connection.</summary>
        /// <value>The version of SQLite used by the connection.</value>
        public override string ServerVersion
        {
            get
            {
                return raw.sqlite3_libversion();
            }
        }

        /// <summary>Gets the current state of the connection.</summary>
        /// <value>The current state of the connection.</value>
        public override ConnectionState State
        {
            get
            {
                return this._state;
            }
        }

        /// <summary>
        /// Gets or sets the transaction currently being used by the connection, or null if none.
        /// </summary>
        /// <value>The transaction currently being used by the connection.</value>
        protected internal virtual SqliteTransaction Transaction { get; set; }

        private void SetState(ConnectionState value)
        {
            ConnectionState state = this._state;
            if (state == value)
                return;
            this._state = value;
            this.OnStateChange(new StateChangeEventArgs(state, value));
        }

        /// <summary>
        /// Opens a connection to the database using the value of <see cref="P:Microsoft.Data.Sqlite.SqliteConnection.ConnectionString" />.
        /// </summary>
        /// <exception cref="T:Microsoft.Data.Sqlite.SqliteException">A SQLite error occurs while opening the connection.</exception>
        public override void Open()
        {
            if (this.State == ConnectionState.Open)
                return;
            if (this.ConnectionString == null)
                throw new InvalidOperationException("OpenRequiresSetConnectionString");
            string str = this.ConnectionStringBuilder.DataSource;
            int num = 0;
            if (str.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
                num |= 64;
            int flags;
            switch (this.ConnectionStringBuilder.Mode)
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
            switch (this.ConnectionStringBuilder.Cache)
            {
                case SqliteCacheMode.Private:
                    flags |= 262144;
                    break;
                case SqliteCacheMode.Shared:
                    flags |= 131072;
                    break;
            }

            flags |= raw.SQLITE_OPEN_NOMUTEX;


            string data = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
            if (!string.IsNullOrEmpty(data) && (flags & 64) == 0 && (!str.Equals(":memory:", StringComparison.OrdinalIgnoreCase) && !Path.IsPathRooted(str)))
                str = Path.Combine(data, str);
            SqliteException.ThrowExceptionForRC(raw.sqlite3_open_v2(str, out this._db, flags, (string)null), this._db);
            this.SetState(ConnectionState.Open);
        }

        /// <summary>
        /// Closes the connection to the database. Open transactions are rolled back.
        /// </summary>
        public override void Close()
        {
            if (this._db == null || this._db.ptr == IntPtr.Zero)
                return;
            SqliteTransaction transaction = this.Transaction;
            if (transaction != null)
            {
                // ISSUE: explicit non-virtual call
                transaction.Dispose() ;
            }
            this._db.Dispose();
            this._db = (sqlite3)null;
            this.SetState(ConnectionState.Closed);
        }
 

        /// <summary>
        /// Releases any resources used by the connection and closes it.
        /// </summary>
        /// <param name="disposing">
        /// true to release managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                this.Close();
            base.Dispose(disposing);
        }

        /// <summary>Creates a new command associated with the connection.</summary>
        /// <returns>The new command.</returns>
        /// <remarks>
        /// The command's <seealso cref="P:Microsoft.Data.Sqlite.SqliteCommand.Transaction" /> property will also be set to the current
        /// transaction.
        /// </remarks>
        public virtual SqliteCommand CreateCommand()
        {
            return new SqliteCommand()
            {
                Connection = this,
                Transaction = this.Transaction
            };
        }

        /// <summary>Creates a new command associated with the connection.</summary>
        /// <returns>The new command.</returns>
        protected override DbCommand CreateDbCommand()
        {
            return (DbCommand)this.CreateCommand();
        }

        /// <summary>Create custom collation.</summary>
        /// <param name="name">Name of the collation.</param>
        /// <param name="comparison">Method that compares two strings.</param>
        public virtual void CreateCollation(string name, Comparison<string> comparison)
        {
            this.CreateCollation<object>(name, (object)null, comparison != null ? (Func<object, string, string, int>)((_, s1, s2) => comparison(s1, s2)) : (Func<object, string, string, int>)null);
        }

        /// <summary>Create custom collation.</summary>
        /// <typeparam name="T">The type of the state object.</typeparam>
        /// <param name="name">Name of the collation.</param>
        /// <param name="state">State object passed to each invocation of the collation.</param>
        /// <param name="comparison">Method that compares two strings, using additional state.</param>
        public virtual void CreateCollation<T>(string name, T state, Func<T, string, string, int> comparison)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            if (this.State != ConnectionState.Open)
                throw new InvalidOperationException("RequiresOpenConnection"+((object)nameof(CreateCollation)));
            delegate_collation f = comparison != null ? (delegate_collation)((v, s1, s2) => comparison((T)v, s1, s2)) : (delegate_collation)null;
            SqliteException.ThrowExceptionForRC(raw.sqlite3_create_collation(this._db, name, (object)state, f), this._db);
        }

        /// <summary>Begins a transaction on the connection.</summary>
        /// <returns>The transaction.</returns>
        public virtual SqliteTransaction BeginTransaction()
        {
            return this.BeginTransaction(IsolationLevel.Unspecified);
        }

        /// <summary>Begins a transaction on the connection.</summary>
        /// <param name="isolationLevel">The isolation level of the transaction.</param>
        /// <returns>The transaction.</returns>
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return (DbTransaction)this.BeginTransaction(isolationLevel);
        }

        ///// <summary>Begins a transaction on the connection.</summary>
        ///// <param name="isolationLevel">The isolation level of the transaction.</param>
        ///// <returns>The transaction.</returns>
        //public virtual SqliteTransaction BeginTransaction(IsolationLevel isolationLevel)
        //{
        //    if (this.State != ConnectionState.Open)
        //        throw new InvalidOperationException("CallRequiresOpenConnection"+((object)nameof(BeginTransaction)));
        //    if (this.Transaction != null)
        //        throw new InvalidOperationException("ParallelTransactionsNotSupported");
        //    //return this.Transaction = new SqliteTransaction(this, isolationLevel);

        //    return this.Transaction = this.BeginTransaction();
        //}

        /// <summary>Changes the current database. Not supported.</summary>
        /// <param name="databaseName">The name of the database to use.</param>
        /// <exception cref="T:System.NotSupportedException">Always.</exception>
        public override void ChangeDatabase(string databaseName)
        {
            throw new NotSupportedException();
        }

        /// <summary>Enables extension loading on the connection.</summary>
        /// <param name="enable">true to enable; false to disable</param>
        /// <seealso href="http://sqlite.org/loadext.html">Run-Time Loadable Extensions</seealso>
        public virtual void EnableExtensions(bool enable = true)
        {
            if (this._db == null || this._db.ptr == IntPtr.Zero)
                throw new InvalidOperationException("CallRequiresOpenConnection"+((object)nameof(EnableExtensions)));
            SqliteException.ThrowExceptionForRC(raw.sqlite3_enable_load_extension(this._db, enable ? 1 : 0), this._db);
        }


        public static void Initialize()
        {
            Assembly assembly;
            try
            {
                assembly = Assembly.Load(new AssemblyName("SQLitePCLRaw.batteries_v2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=8226ea5df37bcae9"));
            }
            catch
            {
                return;
            }
            assembly.GetType("SQLitePCL.Batteries_V2").GetTypeInfo().GetDeclaredMethod("Init").Invoke((object)null, (object[])null);
        }
    }
}
