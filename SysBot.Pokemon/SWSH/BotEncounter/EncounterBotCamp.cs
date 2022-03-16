﻿using System.Threading;
using System.Threading.Tasks;
using PKHeX.Core;
using static SysBot.Base.SwitchButton;
using static SysBot.Pokemon.PokeDataOffsets;

namespace SysBot.Pokemon
{
    public sealed class EncounterBotCamp : EncounterBot
    {
        public EncounterBotCamp(PokeBotState cfg, PokeTradeHub<PK8> hub) : base(cfg, hub)
        {
        }

        protected override async Task EncounterLoop(SAV8SWSH sav, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                PK8? pknew;

                // See if we're in a battle.
                pknew = await ReadUntilPresent(WildPokemonOffset, 0_200, 0_200, BoxFormatSlotSize, token).ConfigureAwait(false);
                if (pknew != null)
                {
                    if (await HandleEncounter(pknew, token).ConfigureAwait(false))
                        return;

                    // Offsets are flickery so make sure we see it 3 times.
                    for (int i = 0; i < 3; i++)
                        await ReadUntilChanged(BattleMenuOffset, BattleMenuReady, 5_000, 0_100, true, token).ConfigureAwait(false);

                    Log("Running away...");
                    await FleeToOverworld(token).ConfigureAwait(false);
                    await Task.Delay(1_000).ConfigureAwait(false);

                    // If you decide to camp the Galarian birds, you need to click out of a menu.
                    if (pknew.Species is 144 or 145 or 146)
                    {
                        await Click(A, 0_500, token).ConfigureAwait(false);
                        await Click(A, 0_500, token).ConfigureAwait(false);
                    }
                }

                await EnterLeaveCamp(token).ConfigureAwait(false);
            }
        }

        // Set up camp.  Assumes your cursor is already over the camp button and you are positioned on top of the spawn.
        // Will fail if either is not true, since that is human error a bot cannot fix.
        private async Task EnterLeaveCamp(CancellationToken token)
        {
            Connection.Log("Setting up camp.");
            await Click(X, 1_000, token).ConfigureAwait(false);
            for (int i = 0; i < 3; i++)
                await Click(A, 0_200, token).ConfigureAwait(false);
            var timer = 20_000;
            while (!await IsInBattle(token).ConfigureAwait(false))
            {
                await Click(B, 1_000, token).ConfigureAwait(false);
                await Click(A, 0_500, token).ConfigureAwait(false);
                timer -= 1_500;
                if (timer <= 0)
                {
                    Connection.Log("Camping failed, resetting the game...");
                    await CloseGame(Hub.Config, token).ConfigureAwait(false);
                    await StartGame(Hub.Config, token).ConfigureAwait(false);
                    await Task.Delay(1_000).ConfigureAwait(false);
                    return;
                }
            }
        }
    }
}
