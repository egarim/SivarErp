using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using System;

namespace Sivar.Erp.Xpo.Core
{
    /// <summary>
    /// Provides XPO data access services
    /// </summary>
    public class XpoDataAccessService
    {
        private static readonly object _lockObject = new object();
        private static string _connectionString;

        /// <summary>
        /// Initializes the XPO data layer
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        public static void Initialize(string connectionString)
        {
            lock (_lockObject)
            {
                _connectionString = connectionString;
                XpoDefault.DataLayer = XpoDefault.GetDataLayer(
                    connectionString, AutoCreateOption.DatabaseAndSchema);
            }
        }

        /// <summary>
        /// Gets a unit of work for the current operation
        /// </summary>
        /// <returns>A new unit of work</returns>
        public static UnitOfWork GetUnitOfWork()
        {
            return new UnitOfWork();
        }

        /// <summary>
        /// Gets a session for the current operation
        /// </summary>
        /// <returns>A new session</returns>
        public static Session GetSession()
        {
            return new Session(XpoDefault.DataLayer);
        }
    }
}