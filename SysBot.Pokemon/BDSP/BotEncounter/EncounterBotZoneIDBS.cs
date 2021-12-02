using System;
using System.Threading;
using System.Threading.Tasks;
using PKHeX.Core;

namespace SysBot.Pokemon
{
    public sealed class EncounterBotZoneIDBS : EncounterBotBS
    {

        public EncounterBotZoneIDBS(PokeBotState cfg, PokeTradeHub<PB8> hub) : base(cfg, hub)
        {
        }

        private ulong ZoneIDOffset;

        protected override async Task EncounterLoop(SAV8BS sav, CancellationToken token)
        {
            ZoneIDOffset = await SwitchConnection.PointerAll(Offsets.ZoneIDPointer, token).ConfigureAwait(false);
            while (!token.IsCancellationRequested)
            {
                var value = await SwitchConnection.ReadBytesAbsoluteAsync(ZoneIDOffset, 2, token).ConfigureAwait(false);
                var zoneID = BitConverter.ToUInt16(value, 0);
                var name = GameInfo.GetLocationName(false, zoneID, 8, -1, GameVersion.BDSP);
                Log($"{zoneID:000} - {name}");
                await Task.Delay(1_000).ConfigureAwait(false);
            }
        }
    }
}