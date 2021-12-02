using System;
using PKHeX.Core;

namespace SysBot.Pokemon
{
    public sealed class BotFactory8 : BotFactory<PK8>
    {
        public override PokeRoutineExecutorBase CreateBot(PokeTradeHub<PK8> Hub, PokeBotState cfg) => cfg.NextRoutineType switch
        {
            PokeRoutineType.FlexTrade or PokeRoutineType.Idle
                or PokeRoutineType.SurpriseTrade
                or PokeRoutineType.LinkTrade
                or PokeRoutineType.Clone
                or PokeRoutineType.Dump
                or PokeRoutineType.SeedCheck
                => new PokeTradeBot(Hub, cfg),

            PokeRoutineType.EggFetch => new EggBot(cfg, Hub),
            PokeRoutineType.FossilBot => new FossilBot(cfg, Hub),
            PokeRoutineType.RaidBot => new RaidBot(cfg, Hub),
            PokeRoutineType.EncBotLine => new EncounterBotLine(cfg, Hub),
            PokeRoutineType.EncBotReset => new EncounterBotReset(cfg, Hub),
            PokeRoutineType.EncBotDog => new EncounterBotDog(cfg, Hub),
            PokeRoutineType.EncBotCamp => new EncounterBotCamp(cfg, Hub),
            PokeRoutineType.EncBotFishing => new EncounterBotFish(cfg, Hub),
            PokeRoutineType.EncBotTeaSmash => new EncounterBotTeaSmash(cfg, Hub),
            PokeRoutineType.EncBotCopySeed => new EncounterBotCopySeed(cfg, Hub),
            PokeRoutineType.EncBotRNGMonitor => new EncounterBotRNGMonitor(cfg, Hub),

            PokeRoutineType.RemoteControl => new RemoteControlBot(cfg),
            _ => throw new ArgumentException(nameof(cfg.NextRoutineType)),
        };

        public override bool SupportsRoutine(PokeRoutineType type) => type switch
        {
            PokeRoutineType.FlexTrade or PokeRoutineType.Idle
                or PokeRoutineType.SurpriseTrade
                or PokeRoutineType.LinkTrade
                or PokeRoutineType.Clone
                or PokeRoutineType.Dump
                or PokeRoutineType.SeedCheck
                => true,

            PokeRoutineType.EggFetch => true,
            PokeRoutineType.FossilBot => true,
            PokeRoutineType.RaidBot => true,
            PokeRoutineType.EncBotLine => true,
            PokeRoutineType.EncBotReset => true,
            PokeRoutineType.EncBotDog => true,
            PokeRoutineType.EncBotCamp => true,
            PokeRoutineType.EncBotFishing => true,
            PokeRoutineType.EncBotTeaSmash => true,
            PokeRoutineType.EncBotCopySeed => true,
            PokeRoutineType.EncBotRNGMonitor => true,

            PokeRoutineType.RemoteControl => true,

            _ => false,
        };
    }
}
