using PKHeX.Core;
using SysBot.Base;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SysBot.Pokemon
{
    public abstract class PokeRoutineExecutor<T> : PokeRoutineExecutorBase where T : PKM, new()
    {
        protected PokeRoutineExecutor(IConsoleBotManaged<IConsoleConnection, IConsoleConnectionAsync> cfg) : base(cfg)
        {
        }

        public abstract Task<T> ReadPokemon(ulong offset, CancellationToken token);
        public abstract Task<T> ReadPokemon(ulong offset, int size, CancellationToken token);
        public abstract Task<T> ReadPokemonPointer(IEnumerable<long> jumps, int size, CancellationToken token);
        public abstract Task<T> ReadBoxPokemon(int box, int slot, CancellationToken token);

        public async Task<T?> ReadUntilPresent(ulong offset, int waitms, int waitInterval, int size, CancellationToken token)
        {
            int msWaited = 0;
            while (msWaited < waitms)
            {
                var pk = await ReadPokemon(offset, size, token).ConfigureAwait(false);
                if (pk.Species != 0 && pk.ChecksumValid)
                    return pk;
                await Task.Delay(waitInterval, token).ConfigureAwait(false);
                msWaited += waitInterval;
            }
            return null;
        }

        public async Task<T?> ReadUntilPresentPointer(IReadOnlyList<long> jumps, int waitms, int waitInterval, int size, CancellationToken token)
        {
            int msWaited = 0;
            while (msWaited < waitms)
            {
                var pk = await ReadPokemonPointer(jumps, size, token).ConfigureAwait(false);
                if (pk.Species != 0 && pk.ChecksumValid)
                    return pk;
                await Task.Delay(waitInterval, token).ConfigureAwait(false);
                msWaited += waitInterval;
            }
            return null;
        }

        protected async Task<(bool, ulong)> ValidatePointerAll(IEnumerable<long> jumps, CancellationToken token)
        {
            var solved = await SwitchConnection.PointerAll(jumps, token).ConfigureAwait(false);
            return (solved != 0, solved);
        }

        public static void DumpPokemon(string folder, string subfolder, T pk)
        {
            if (!Directory.Exists(folder))
                return;
            var dir = Path.Combine(folder, subfolder);
            Directory.CreateDirectory(dir);
            var fn = Path.Combine(dir, Util.CleanFileName(pk.FileName));
            File.WriteAllBytes(fn, pk.DecryptedPartyData);
            LogUtil.LogInfo($"Saved file: {fn}", "Dump");
        }

        public string GetSeedOutput(ulong s0, ulong s1, DisplaySeedMode mode)
        {
            string seed0 = $"{s0:x16}";
            string seed1 = $"{s1:x16}";

            return mode switch
            {
                DisplaySeedMode.Bit128 => $"{seed1}{seed0}",
                DisplaySeedMode.Bit64 => $"{seed0}{Environment.NewLine}{seed1}",
                DisplaySeedMode.Bit64PokeFinder => SplitSeed32Bit(seed0, seed1, mode),
                DisplaySeedMode.Bit32 => SplitSeed32Bit(seed0, seed1, mode),
                _ => $"{seed0}{Environment.NewLine}{seed1}",
            };
        }

        // Realistically, people will only be monitoring in s0/s1 or s0/s1/s2/s3 format.
        public string GetSeedMonitorOutput(ulong s0, ulong s1, DisplaySeedMode mode)
        {
            string seed0 = $"{s0:x16}";
            string seed1 = $"{s1:x16}";

            return mode switch
            {
                DisplaySeedMode.Bit128 => $"{seed1}{seed0}",
                DisplaySeedMode.Bit64 => $"{seed0} {seed1}",
                DisplaySeedMode.Bit32 => SplitSeed32Bit(seed0, seed1, mode, false),
                _ => $"s0: {seed0} s1: {seed1}",
            };
        }

        // Accommodate standard RNG tools splitting this into 4 seeds.
        public string SplitSeed32Bit(string seed0, string seed1, DisplaySeedMode mode, bool copy = true)
        {
            var _s0 = seed0[8..];
            var _s1 = seed0[..8];
            var _s2 = seed1[8..];
            var _s3 = seed1[..8];

            // Format for setting seeds in clipboard or for copying.
            if (copy)
            {
                if (mode is DisplaySeedMode.Bit32)
                    return $"{_s0}{Environment.NewLine}{_s1}{Environment.NewLine}{_s2}{Environment.NewLine}{_s3}";
                return $"{_s0}{_s1}{Environment.NewLine}{_s2}{_s3}";
            }

            return $"{_s0} {_s1} {_s2} {_s3}";
        }

        public void CopyToClipboard(string output)
        {
            try
            {
                Thread thread = new(() => Clipboard.SetText(output));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
        }
    }
}
