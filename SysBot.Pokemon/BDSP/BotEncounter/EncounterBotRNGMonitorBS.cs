﻿using System.Threading;
using System.Threading.Tasks;
using PKHeX.Core;
using static SysBot.Base.SwitchButton;

namespace SysBot.Pokemon
{
    public sealed class EncounterBotRNGMonitorBS : EncounterBotBS
    {
        public EncounterBotRNGMonitorBS(PokeBotState cfg, PokeTradeHub<PB8> hub) : base(cfg, hub)
        {
        }

        private ulong MainRNGOffset;
        private int TotalAdvances;

        protected override async Task EncounterLoop(SAV8BS sav, CancellationToken token)
        {
            MainRNGOffset = await SwitchConnection.PointerAll(Offsets.MainRNGPointer, token).ConfigureAwait(false);
            var (s0, s1) = await GetGlobalRNGState(MainRNGOffset, false, token).ConfigureAwait(false);
            var output = GetSeedMonitorOutput(s0, s1, Hub.Config.EncounterRNGBS.DisplaySeedMode);

            // Attempt to copy initial state to clipboard.
            CopyToClipboard(output);

            Log("Initial RNG state copied to the clipboard.");
            Log($"Start: {output}");

            while (!token.IsCancellationRequested)
            {
                await Task.Delay(Hub.Config.EncounterRNGBS.MonitorRefreshRate, token).ConfigureAwait(false);

                var (_s0, _s1) = await GetGlobalRNGState(MainRNGOffset, false, token).ConfigureAwait(false);

                // Only update if it changed.
                if (_s0 == s0 && _s1 == s1)
                    continue;

                output = GetSeedMonitorOutput(s0, s1, Hub.Config.EncounterRNGBS.DisplaySeedMode);
                var passed = GetAdvancesPassed(s0, s1, _s0, _s1);
                TotalAdvances += passed;
                Log($"{output} - Advances: {TotalAdvances} | {passed}");

                // Store the state for the next pass.
                s0 = _s0;
                s1 = _s1;

                var maxAdvance = Hub.Config.EncounterRNGBS.MaxTotalAdvances;
                if (maxAdvance != 0 && TotalAdvances >= maxAdvance)
                {
                    Log($"Hitting HOME to pause the game. Max total advances is {maxAdvance} and {TotalAdvances} advances have passed.");
                    await Click(HOME, 2_000, token).ConfigureAwait(false);
                    return;
                }
            }
        }
    }
}
