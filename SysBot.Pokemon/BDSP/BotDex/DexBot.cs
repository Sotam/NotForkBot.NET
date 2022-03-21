namespace SysBot.Pokemon.BDSP.BotDex
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using PKHeX.Core;

    public class DexBot : PokeRoutineExecutor8BS
    {
        private readonly PokeTradeHub<PB8> Hub;

        // Cached offsets that stay the same per session.
        private ulong BoxStartOffset;
        private ulong UnionGamingOffset;
        private ulong UnionTalkingOffset;
        private ulong SoftBanOffset;
        private ulong LinkTradePokemonOffset;

        public DexBot(PokeTradeHub<PB8> hub, PokeBotState cfg) : base(cfg)
        {
            Hub = hub;
        }

        public override async Task MainLoop(CancellationToken token)
        {
            try
            {
                await InitializeHardware(Hub.Config.Trade, token).ConfigureAwait(false);

                Log("Identifying trainer data of the host console.");
                var sav = await IdentifyTrainer(token).ConfigureAwait(false);

                await InitializeSessionOffsets(token).ConfigureAwait(false);

                Log($"Starting main {nameof(PokeTradeBotBS)} loop.");
                await InnerLoop(sav, token).ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Log(e.Message);
            }

            Log($"Ending {nameof(PokeTradeBotBS)} loop.");
            await HardStop().ConfigureAwait(false);
        }

        public override Task HardStop()
        {
            throw new System.NotImplementedException();
        }

        // These don't change per session and we access them frequently, so set these each time we start.
        private async Task InitializeSessionOffsets(CancellationToken token)
        {
            Log("Caching session offsets...");
            BoxStartOffset = await SwitchConnection.PointerAll(Offsets.BoxStartPokemonPointer, token).ConfigureAwait(false);
            UnionGamingOffset = await SwitchConnection.PointerAll(Offsets.UnionWorkIsGamingPointer, token).ConfigureAwait(false);
            UnionTalkingOffset = await SwitchConnection.PointerAll(Offsets.UnionWorkIsTalkingPointer, token).ConfigureAwait(false);
            SoftBanOffset = await SwitchConnection.PointerAll(Offsets.UnionWorkPenaltyPointer, token).ConfigureAwait(false);
        }

        private async Task InnerLoop(SAV8BS sav, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                Config.IterateNextRoutine();
                var task = Config.CurrentRoutineType switch
                {
                    PokeRoutineType.Idle => DoNothing(token),
                    _ => DoTrades(sav, token),
                };
                try
                {
                    await task.ConfigureAwait(false);
                }
                catch (SocketException e)
                {
                    Log(e.Message);
                    Connection.Reset();
                }
            }
        }

        private async Task DoNothing(CancellationToken token)
        {
            while (!token.IsCancellationRequested && Config.NextRoutineType == PokeRoutineType.Idle)
            {
                await Task.Delay(100, token);
            }
        }

        //public override async Task<PB8> ReadBoxPokemon(int box, int slot, CancellationToken token)
        //{
        //    // Shouldn't be reading anything but box1slot1 here. Slots are not consecutive.
        //    var jumps = Offsets.BoxStartPokemonPointer.ToArray();
        //    return await ReadPokemonPointer(jumps, BoxFormatSlotSize, token).ConfigureAwait(false);
        //}



        private async Task DoTrades(SAV8BS sav, CancellationToken token)
        {
            var type = Config.CurrentRoutineType;
            while (!token.IsCancellationRequested && Config.NextRoutineType == type)
            {

                Log($"Starting next {type} Bot READ. Getting data...");
                await Task.Delay(500, token).ConfigureAwait(false);

                var jumps = Offsets.BoxStartPokemonPointer.ToArray();


                var read = await ReadBoxPokemon(1, 1, token);
                ////read.Species = 482;
                ///

                var fn = "blissey";
                fn = "breloom";
                var x = new PB8(File.ReadAllBytes($@"C:\Users\Kevin\Desktop\{fn}.pb8"));



                await SetBoxPokemonAbsolute(BoxStartOffset, x, token, sav).ConfigureAwait(false);

            }
        }
    }
}