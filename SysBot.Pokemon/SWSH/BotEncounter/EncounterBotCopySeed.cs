using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using PKHeX.Core;
using static SysBot.Pokemon.PokeDataOffsets;

namespace SysBot.Pokemon
{
    public sealed class EncounterBotCopySeed : EncounterBot
    {
        public EncounterBotCopySeed(PokeBotState cfg, PokeTradeHub<PK8> hub) : base(cfg, hub)
        {
        }

        protected override async Task EncounterLoop(SAV8SWSH sav, CancellationToken token)
        {
            var (s0, s1) = await GetGlobalRNGState(SWSHMainRNGOffset, false, token).ConfigureAwait(false);
            var output = GetSeedOutput(s0, s1, Hub.Config.Encounter.DisplaySeedMode);
            Log($"Copying global RNG state to clipboard:{Environment.NewLine}{Environment.NewLine}{output}{Environment.NewLine}");
            CopyToClipboard(output);
        }
    }
}
