namespace Common.Infrastructure.Persistence.EF.Enums;
public enum PersistenceDatabase
{
    /// <summary>
    /// Main Database.
    /// </summary>
    Core = 0,

    /// <summary>
    /// News and Events Database.
    /// </summary>
    News = 1,

    /// <summary>
    /// Foreign exchange Database.
    /// </summary>
    Forex = 2,

    /// <summary>
    /// Cryptocurrency Database.
    /// </summary>
    Crypto = 3,

    /// <summary>
    /// Stock Database.
    /// </summary>
    Stocks = 4,

    /// <summary>
    /// Exchange-traded fund Database.
    /// </summary>
    Metals = 5,

    /// <summary>
    /// Commodity Database.
    /// </summary>
    Commodity = 6,

    /// <summary>
    /// Index Database.
    /// </summary>
    Indices = 7,
}