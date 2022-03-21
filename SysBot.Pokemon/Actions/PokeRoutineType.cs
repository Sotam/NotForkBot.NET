namespace SysBot.Pokemon
{
    /// <summary>
    /// Type of routine the Bot carries out.
    /// </summary>
    public enum PokeRoutineType
    {
        /// <summary> Sits idle waiting to be re-tasked. </summary>
        Idle = 0,

        /// <summary> Performs random trades using a predetermined pool of data. </summary>
        SurpriseTrade = 1,

        /// <summary> Performs the behavior of all trade bots. </summary>
        FlexTrade = 2,
        /// <summary> Performs only P2P Link Trades of specific data. </summary>
        LinkTrade = 3,
        /// <summary> Performs a seed check without transferring data from the bot. </summary>
        SeedCheck = 4,
        /// <summary> Performs a clone operation on the partner's data, sending them a copy of what they show. </summary>
        Clone = 5,
        /// <summary> Exports files for all data shown to the bot. </summary>
        Dump = 6,

        /// <summary> Retrieves eggs from the Day Care. </summary>
        EggFetch = 7,

        /// <summary> Revives fossils until the criteria is satisfied. </summary>
        FossilBot = 8,

        /// <summary> Performs group battles as a host. </summary>
        RaidBot = 9,

        /// <summary> Triggers encounters until the criteria is satisfied. </summary>
        EncBotLine = 1000,

        /// <summary> Triggers encounters until the criteria is satisfied. </summary>
        EncBotReset = 1001,

        /// <summary> Triggers encounters until the criteria is satisfied. </summary>
        EncBotDog = 1002,

        /// <summary> Triggers encounters until the criteria is satisfied. </summary>
        EncBotCamp = 1003,

        /// <summary> Triggers encounters until the criteria is satisfied. </summary>
        EncBotFishing = 1004,

        /// <summary> Triggers encounters until the criteria is satisfied. </summary>
        EncBotTeaSmash = 1005,

        /// <summary> Copies out the current global RNG state in the specified format. </summary>
        EncBotCopySeed = 1006,

        /// <summary> Prints out the global RNG state and information on advances passed. </summary>
        EncBotRNGMonitor = 1007,

        /// <summary> Triggers encounters until the criteria is satisfied. </summary>
        EncBotTIDBS = 2000,

        /// <summary> Continously prints the player character's current zoneID. </summary>
        EncBotZoneIDBS = 2001,

        /// <summary> Copies out the current global RNG state in the specified format. </summary>
        EncBotCopySeedBS = 2002,

        /// <summary> Prints out the global RNG state and information on advances passed. </summary>
        EncBotRNGMonitorBS = 2003,

        /// <summary> Prints out the global RNG state and information on advances passed. </summary>
        EncBotDexFlipBS = 2004,

        /// <summary> RNG's Mt. Coronet legendary encounters. </summary>
        EncBotMtCoronetBS = 2005,

        /// <summary> Similar to idle, but identifies the bot as available for Remote input (Twitch Plays, etc). </summary>
        RemoteControl = 6_000,

        // Add your own custom bots here so they don't clash for future main-branch bot releases.
    }

    public static class PokeRoutineTypeExtensions
    {
        public static bool IsTradeBot(this PokeRoutineType type) => type is >= PokeRoutineType.FlexTrade and <= PokeRoutineType.Dump;
        public static bool IsMonitorTool(this PokeRoutineType type) =>
            type is PokeRoutineType.EncBotCopySeed or PokeRoutineType.EncBotRNGMonitor
            or (>= PokeRoutineType.EncBotZoneIDBS and <= PokeRoutineType.EncBotRNGMonitorBS);
    }
}
