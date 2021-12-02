using System;
using System.Threading;
using System.Threading.Tasks;
using PKHeX.Core;

namespace SysBot.Pokemon
{
    public sealed class EncounterBotCopySeedBS : EncounterBotBS
    {
        public EncounterBotCopySeedBS(PokeBotState cfg, PokeTradeHub<PB8> hub) : base(cfg, hub)
        {
        }

        private ulong MainRNGOffset;

        protected override async Task EncounterLoop(SAV8BS sav, CancellationToken token)
        {
            MainRNGOffset = await SwitchConnection.PointerAll(Offsets.MainRNGPointer, token).ConfigureAwait(false);
            var (s0, s1) = await GetGlobalRNGState(MainRNGOffset, false, token).ConfigureAwait(false);
            var output = GetSeedOutput(s0, s1, Hub.Config.EncounterRNGBS.DisplaySeedMode);
            Log($"Copying global RNG state to clipboard:{Environment.NewLine}{Environment.NewLine}{output}{Environment.NewLine}");
            CopyToClipboard(output);
        }
    }
}
