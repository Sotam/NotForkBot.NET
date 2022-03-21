namespace SysBot.Pokemon
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using PKHeX.Core;
    using PokeNX.Core.Generators;
    using PokeNX.Core.Models;
    using PokeNX.Core.Models.Enums;
    using PokeNX.Core.RNG;
    using static Base.SwitchButton;
    using static Base.SwitchStick;

    public class EncounterBotMtCoronetBS : EncounterBotBS
    {
        private ulong _mainRNGOffset;

        public EncounterBotMtCoronetBS(PokeBotState cfg, PokeTradeHub<PB8> hub) : base(cfg, hub)
        {
        }

        protected override async Task EncounterLoop(SAV8BS sav, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await ResetStick(token).ConfigureAwait(false);

                const uint initialAdvances = 85; // Dialga/Palkia = 84


                ulong s0 = 0, s1 = 0, _s0 = 0, _s1 = 0;
                byte[] data, _data;



                Log("Looking for a new seed...");

                _mainRNGOffset = await SwitchConnection.PointerAll(Offsets.MainRNGPointer, token).ConfigureAwait(false);

                (s0, s1, data) = await GetGlobalRNGStateWithData(_mainRNGOffset, false, token).ConfigureAwait(false);
                Log($"Initial seed (before set): {s0:X8}:{s1:X8}");

                var rq = new Stationary8Request
                {
                    Filter = new Filter
                    {
                        Ability = AbilityFilter.Any,
                        Gender = GenderFilter.Any,
                        MaxIVs = new byte[] { 31, 31, 31, 31, 31, 31 },
                        MinIVs = new byte[] { 31, 31, 31, 31, 31, 31 },
                        //MinIVs = new byte[] { 0, 0, 0, 0, 0, 0 },
                        Natures = new[] { NatureFilter.Any },
                        Shiny = ShinyFilter.StarSquare
                    },
                    GenderRatio = 255, // Genderless
                    SetIVs = true
                };

                var rqT = new Stationary8Request
                {
                    Filter = new Filter
                    {
                        Ability = AbilityFilter.Any,
                        Gender = GenderFilter.Any,
                        MaxIVs = new byte[] { 31, 31, 31, 31, 31, 31 },
                        MinIVs = new byte[] { 0, 0, 0, 0, 0, 0 },
                        Natures = new[] { NatureFilter.Any },
                        Shiny = ShinyFilter.Any
                    },
                    GenderRatio = 255, // Genderless
                    SetIVs = true
                };

                var g = new List<GenerateResult>();

                var r = new Random();

                var i = 0;

                ParallelOptions parallelOptions = new ParallelOptions
                {
                    CancellationToken = token,
                    MaxDegreeOfParallelism = 16
                };

                var stop = false;

                ulong is0 = 0, is1 = 0;

                Parallel.ForEach(Infinite(), parallelOptions, (ignored, loopState) =>
                {
                    var fakeSeed = new[]
                    {
                        (byte)r.Next(0,0xFF),
                        (byte)r.Next(0,0xFF),
                        (byte)r.Next(0,0xFF),
                        (byte)r.Next(0,0xFF),
                        (byte)r.Next(0,0xFF),
                        (byte)r.Next(0,0xFF),
                        (byte)r.Next(0,0xFF),
                        (byte)r.Next(0,0xFF),
                        (byte)r.Next(0,0xFF),
                        (byte)r.Next(0,0xFF),
                        (byte)r.Next(0,0xFF),
                        (byte)r.Next(0,0xFF),
                        (byte)r.Next(0,0xFF),
                        (byte)r.Next(0,0xFF),
                        (byte)r.Next(0,0xFF),
                        (byte)r.Next(0,0xFF)
                    };

                    var fs0 = BitConverter.ToUInt64(fakeSeed, 0);
                    var fs1 = BitConverter.ToUInt64(fakeSeed, 8);

                    //var cb0 = BitConverter.GetBytes(fs0);
                    //var cb1 = BitConverter.GetBytes(fs1);
                    //var cb = cb0.Concat(cb1).ToArray();

                    var gr = new StationaryGenerator8(initialAdvances, 5_000_000).Generate(fs0, fs1, rq);

                    Log($"Attempt {++i}, {Task.CurrentId}");

                    if (stop)
                    {
                        stop = true;
                        loopState.Stop();
                    }

                    if (gr.Any())
                    {
                        Log($"Fake seed gen gave {fs0:X8}:{fs1:X8}");
                        is0 = fs0;
                        is1 = fs1;

                        g = gr;
                        stop = true;

                    }
                });

                //do
                //{
                //    var fakeSeed = new[]
                //    {
                //        (byte)r.Next(0,0xFF),
                //        (byte)r.Next(0,0xFF),
                //        (byte)r.Next(0,0xFF),
                //        (byte)r.Next(0,0xFF),
                //        (byte)r.Next(0,0xFF),
                //        (byte)r.Next(0,0xFF),
                //        (byte)r.Next(0,0xFF),
                //        (byte)r.Next(0,0xFF),
                //        (byte)r.Next(0,0xFF),
                //        (byte)r.Next(0,0xFF),
                //        (byte)r.Next(0,0xFF),
                //        (byte)r.Next(0,0xFF),
                //        (byte)r.Next(0,0xFF),
                //        (byte)r.Next(0,0xFF),
                //        (byte)r.Next(0,0xFF),
                //        (byte)r.Next(0,0xFF)
                //    };

                //    var fs0 = BitConverter.ToUInt64(fakeSeed, 0);
                //    var fs1 = BitConverter.ToUInt64(fakeSeed, 8);

                //    //var cb0 = BitConverter.GetBytes(fs0);
                //    //var cb1 = BitConverter.GetBytes(fs1);
                //    //var cb = cb0.Concat(cb1).ToArray();

                //    g = new StationaryGenerator8(184, 5_000_000).Generate(fs0, fs1, rq);
                //    Log($"Attempt {++i}");
                //} while (!g.Any());
                Log($"is0:{is0:X8}, is1:{is1:X8}");
                var gr = new StationaryGenerator8(initialAdvances, 5_000_000).Generate(is0, is1, rqT);

                var inject = gr.Skip((int)g.First().Advances - 200).First();

                var cb0 = BitConverter.GetBytes(inject.Seed0);
                var cb1 = BitConverter.GetBytes(inject.Seed1);
                var fakeSeedBytes = cb0.Concat(cb1).ToArray();

                await SetGlobalRNGState(fakeSeedBytes, _mainRNGOffset, false, token).ConfigureAwait(false);
                Log($"Perfect seed (at {g.First().Advances}): {inject.Seed0:X8}, {inject.Seed1:X8}");

                (s0, s1, data) = await GetGlobalRNGStateWithData(_mainRNGOffset, false, token).ConfigureAwait(false);
                Log($"Initial seed (after set): {s0:X8}:{s1:X8}");
                //var g1 = new StationaryGenerator8(84, 10).Generate(s0, s1, rqT);
                //foreach (var res in g1)
                //    Log($"{res.Advances:00},{res.EC:X8},{res.PID:X8}");

                //await Task.Delay(5000, token);

                //(_s0, _s1, _data) = await GetGlobalRNGStateWithData(_mainRNGOffset, false, token).ConfigureAwait(false);
                //Log($"Second seed: {_s0:X8}:{_s1:X8}");
                //var g2 = new StationaryGenerator8(84, 10).Generate(_s0, _s1, rqT);
                //foreach (var res in g2)
                //    Log($"{res.Advances:00},{res.EC:X8},{res.PID:X8}");

                //await SetGlobalRNGState(data, _mainRNGOffset, false, token).ConfigureAwait(false);

                //(s0, s1, data) = await GetGlobalRNGStateWithData(_mainRNGOffset, false, token).ConfigureAwait(false);
                //Log($"After set seed: {s0:X8}:{s1:X8}");
                //var g3 = new StationaryGenerator8(84, 10).Generate(s0, s1, rqT);
                //foreach (var res in g3)
                //    Log($"{res.Advances:00},{res.EC:X8},{res.PID:X8}");

                //await Task.Delay(5000, token);

                //(_s0, _s1, _data) = await GetGlobalRNGStateWithData(_mainRNGOffset, false, token).ConfigureAwait(false);
                //Log($"Second seed: {_s0:X8}:{_s1:X8}");
                //var g4 = new StationaryGenerator8(84, 10).Generate(_s0, _s1, rqT);
                //foreach (var res in g4)
                //    Log($"{res.Advances:00},{res.EC:X8},{res.PID:X8}");

                ////(s0, s1) = await SetGlobalRNGState(_mainRNGOffset, false, token).ConfigureAwait(false);
                ////Log($"Initial seed: {s0:X8}:{s1:X8}");

                //return;

                g = new StationaryGenerator8(initialAdvances, 5_000_000).Generate(s0, s1, rq);


                if (!g.Any())
                {
                    Log("Restart, nothing found");
                    await CloseGame(Hub.Config, token).ConfigureAwait(false);
                    await StartGame(false, Hub.Config, token).ConfigureAwait(false);

                    continue;
                }

                var result = g.First();

                Log($"Total advances: {result.Advances}, {result.Seed0:X8}:{result.Seed1:X8}, PID:{result.PID}, EC:{result.EC}");

                if (result.Advances > 300)
                {
                    var (success, advancesPassed) = await DexFlip(result.Advances - 400, s0, s1, token);
                    Log($"DexFlip success {success}, advances passed {advancesPassed}");

                    if (result.Advances < advancesPassed)
                    {
                        Log($"DexFlip to much.. advances passed {advancesPassed}");

                        return;
                    }
                }

                // Move up (Dialga/Palkia)
                await Click(DUP, 1_000, token);
                await Click(DUP, 1_000, token);


                // Double click needed
                //await Click(A, 2_000, token).ConfigureAwait(false); // Dialga Palkia
                await Click(A, 4_000, token).ConfigureAwait(false); // Arceus

                // Recalculate
                (s0, s1) = await GetGlobalRNGState(_mainRNGOffset, false, token).ConfigureAwait(false);
                g = new StationaryGenerator8(initialAdvances, 10_000).Generate(s0, s1, rq);
                if (!g.Any())
                {
                    Log("Restart, advanced to much..");
                    await CloseGame(Hub.Config, token).ConfigureAwait(false);
                    await StartGame(false, Hub.Config, token).ConfigureAwait(false);

                    continue;
                }

                result = g.First();

                Log($"Last time waiting for advances: {result.Advances}");
                if (!await WaitForAdvances(result.Advances, token))
                {
                    Log("Restart, waited to much..");
                    await CloseGame(Hub.Config, token).ConfigureAwait(false);
                    await StartGame(false, Hub.Config, token).ConfigureAwait(false);

                    continue;
                }
                await Click(A, 2_000, token).ConfigureAwait(false);
                Log("Seed reached, entering battle!");

                return;
            }
        }

        private static IEnumerable<bool> Infinite()
        {
            while (true) yield return true;
        }

        private async Task<(bool, int)> DexFlip(uint advances, ulong initialS0, ulong initialS1, CancellationToken token = default)
        {
            await Click(X, 1_000, token);
            await Click(A, 2_000, token);

            // Reducing sys-botbase's sleep time can allow for faster sending of commands.
            //var cmd = SwitchCommand.Configure(SwitchConfigureParameter.mainLoopSleepTime, Hub.Config.EncounterRNGBS.DexFlipMainLoopSleepTime, UseCRLF);
            //await Connection.SendAsync(cmd, token).ConfigureAwait(false);

            //var (initialS0, initialS1) = await GetGlobalRNGState(_mainRNGOffset, false, token).ConfigureAwait(false);

            ulong s0 = 0, s1 = 0, _s0, _s1;

            while (!token.IsCancellationRequested)
            {
                // Performance degrades after about 3 minutes, so reset the Dex.
                for (var i = 0; i < 3000; i++)
                {
                    // Check on the global RNG state every 750 passes.
                    if (i % 2 == 0)
                        (s0, s1) = await GetGlobalRNGState(_mainRNGOffset, false, token).ConfigureAwait(false);

                    await SetStick(LEFT, -30000, 0, Hub.Config.EncounterRNGBS.DexFlipStickSetTime, token).ConfigureAwait(false);
                    await SetStick(LEFT, 30000, 0, Hub.Config.EncounterRNGBS.DexFlipStickSetTime, token).ConfigureAwait(false);

                    // Make sure the RNG state is still changing periodically. If not, we've hit a stop condition or error.
                    if (i % 2 == 0)
                    {
                        (_s0, _s1) = await GetGlobalRNGState(_mainRNGOffset, false, token).ConfigureAwait(false);

                        var advancesPassed = GetAdvancesPassed(initialS0, initialS1, _s0, _s1);

                        Log($"Frames advanced: {advancesPassed}");

                        if (advancesPassed >= advances)
                        {
                            Log("RNG state has been reached... ending DexFlip routine!");

                            // Get rid of any stick stuff left over so we can flee properly.
                            await ResetStick(token).ConfigureAwait(false);

                            await Click(B, 2_000, token);
                            await Click(B, 2_000, token);

                            return (true, advancesPassed);
                        }

                        if (GetAdvancesPassed(s0, s1, _s0, _s1) == 0)
                        {
                            await ResetStick(token).ConfigureAwait(false);
                            Log("RNG state has stopped changing... ending DexFlip routine!");

                            return (false, 0);
                        }
                    }
                }

                await ResetStick(token).ConfigureAwait(false);
                await Click(B, 1_000, token).ConfigureAwait(false);
                Log("Resetting the Pokédex to handle degraded performance.");
                await Click(A, 1_000, token).ConfigureAwait(false);
            }

            return (false, 0);
        }

        private async Task<bool> WaitForAdvances(uint wAdvances, CancellationToken token = default)
        {
            uint advances = 0;
            ulong seed0 = 0;
            ulong seed1 = 0;

            var (s0, s1) = await GetGlobalRNGState(_mainRNGOffset, false, token).ConfigureAwait(false);
            var rng = new XorShift(s0, s1);

            var (tmpS0, tmpS1) = rng.Seed();

            while (true)
            {
                if (token.IsCancellationRequested) break;

                var (ramS0, ramS1) = await GetGlobalRNGState(_mainRNGOffset, false, token).ConfigureAwait(false);

                while (ramS0 != tmpS0 || ramS1 != tmpS1)
                {
                    if (token.IsCancellationRequested) break;

                    rng.Next();
                    (tmpS0, tmpS1) = rng.Seed();
                    advances++;

                    if (ramS0 == tmpS0 && ramS1 == tmpS1)
                    {
                        seed0 = ramS0;
                        seed1 = ramS1;
                    }

                    if (advances == wAdvances)
                        return true;

                    if (advances > wAdvances)
                        return false;
                }
            }

            return false;
        }
    }

}