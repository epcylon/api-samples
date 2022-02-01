namespace Epcylon.Net.APIs.Account
{
    /// <summary>
    /// Server environments.
    /// </summary>
    public enum Environments
    {
        /// <summary>
        /// Staging environment - use this for main development.
        /// </summary>
        Staging = 0,
        /// <summary>
        /// Production environment - use this in the production environment (not during testing).
        /// </summary>
        Production = 1,
        /// <summary>
        /// Development environment - used only by internal developers (protocol may not match).
        /// </summary>
        Development = 2,
        /// <summary>
        /// Connection to a local machine - used by internal developers only.
        /// </summary>
        Local = 3,
    }
}
