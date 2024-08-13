namespace Epcylon.APIs.Account;

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

/// <summary>
/// Product types.
/// </summary>
public enum Products : int
{
    /// <summary>
    /// No product type, or unknown.
    /// </summary>
    None = 0x0000,
    /// <summary>
    /// Pilot app.
    /// </summary>
    Pilot = 0x0001,
    /// <summary>
    /// Co-pilot app.
    /// </summary>
    CoPilot = 0x0002,
    /// <summary>
    /// Stealth client.
    /// </summary>
    Stealth = 0x0100,
    /// <summary>
    /// StealthPro client.
    /// </summary>
    StealthPro = 0x0101,
    /// <summary>
    /// Hydra client.
    /// </summary>
    Hydra = 0x0102,
    /// <summary>
    /// InvestGuide client.
    /// </summary>
    InvestGuide = 0x0103,
}
