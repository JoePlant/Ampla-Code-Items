using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

using Code.Database;

namespace Code.Sms
{
    public interface ISmsService
    {
        void SendSms(string mobile, string message);
    }

    public class SqlSmsService : ISmsService
    {
        private readonly ISqlHelper sqlHelper;

        public SqlSmsService() : this(ConnectionStrings.SmsDatabase)
        {
        }

        public SqlSmsService(string connectionString)
        {
            sqlHelper = new SqlHelper(connectionString);
        }

        public void SendSms(string mobile, string message)
        {
            SqlParameter mobileP = new SqlParameter("mobile", mobile);
            SqlParameter messageP = new SqlParameter("message", message);
            const string sql = "INSERT INTO [SMS].[dbo].[OUTBOX] ([Mobile_no] ,[Message]) VALUES (@mobile, @message)";
            sqlHelper.ExecuteNonQuery(CommandType.Text, sql, mobileP, messageP);
        }
    }

    public class AmplaMessageService : ISmsService
    {
        public void SendSms(string mobile, string message)
        {
            string result = string.Format("Mobile: {0} - Message: {1}", mobile, message);

            Citect.Common.Diagnostics.Write(TraceLevel.Warning, result);
        }
    }
}