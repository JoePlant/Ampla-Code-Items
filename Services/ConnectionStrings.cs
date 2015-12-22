namespace Code.Services
{
    /// <summary>
    ///     Connection Strings 
    /// </summary>
    public static class ConnectionStrings
    {
        /// <summary>
        ///     Connection String for the Extras database
        /// </summary>
        public static readonly string ExtrasDatabase 
            = "Data Source=localhost;" +
              "Initial Catalog=AmplaProjectExtras;" +
              "Trusted_Connection=True;";

        /// <summary>
        ///     Connection String for the SMS database
        /// </summary>
        public static readonly string SmsDatabase
            = "Data Source=localhost;" +
              "Initial Catalog=SMS;" +
              "Trusted_Connection=True;";
    }
}