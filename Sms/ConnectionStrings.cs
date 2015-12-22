namespace Code.Sms
{
    /// <summary>
    ///     Connection Strings 
    /// </summary>
    public static class ConnectionStrings
    {
        /// <summary>
        ///     Connection String for the SMS database
        /// </summary>
        public static readonly string SmsDatabase
            = "Data Source=localhost;" +
              "Initial Catalog=SMS;" +
              "Trusted_Connection=True;";
    }
}