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
    }
}