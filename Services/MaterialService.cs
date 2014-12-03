using System.Data;
using System.Data.SqlClient;
using Code.Database;

namespace Code.Services
{
    /// <summary>
    /// Material Service for extracting material details.
    /// </summary>
    public interface IMaterialService
    {
        /// <summary>
        /// Gets the vendor for the material code
        /// </summary>
        /// <param name="material">The material.</param>
        /// <returns></returns>
        string GetVendor(string material);
    }

    /// <summary>
    ///     A Test material service
    /// </summary>
    public class TestMaterialService : IMaterialService
    {
        public string GetVendor(string material)
        {
            if (string.IsNullOrEmpty(material))
            {
                return "n/a";
            }
            return material + " Vendor";
        }
    }

    /// <summary>
    ///     Material Service based on Sql Server
    /// </summary>
    public class SqlMaterialService : IMaterialService
    {
        private readonly ISqlHelper sqlHelper;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlMaterialService"/> class.
        /// </summary>
        public SqlMaterialService() : this(ConnectionStrings.ExtrasDatabase)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlMaterialService"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        private SqlMaterialService(string connectionString)
        {
            sqlHelper = new SqlHelper(connectionString, "Materials Service");
        }

        /// <summary>
        /// Gets the vendor for the material
        /// </summary>
        /// <param name="material">The material.</param>
        /// <returns></returns>
        public string GetVendor(string material)
        {
            SqlParameter matSp = new SqlParameter("material", material);
            return (string) sqlHelper.ExecuteScalar(CommandType.StoredProcedure, "usp_Materials_GetVendor", matSp);
        }
    }
}