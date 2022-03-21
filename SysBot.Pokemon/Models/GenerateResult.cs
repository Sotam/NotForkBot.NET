namespace PokeNX.Core.Models
{
    using Enums;

    public class GenerateResult
    {
        public ulong Seed0 { get; set; }

        public ulong Seed1 { get; set; }

        public uint Advances { get; set; }

        public uint Seed { get; set; }

        public uint PID { get; set; }

        public uint EC { get; set; }

        public Shiny Shiny { get; set; }

        public Nature Nature { get; set; }

        public Ability Ability { get; set; }

        public Gender Gender { get; set; }

        public IVs HP { get; set; }

        public IVs Atk { get; set; }

        public IVs Def { get; set; }

        public IVs SpA { get; set; }

        public IVs SpD { get; set; }

        public IVs Speed { get; set; }

        internal byte[] IVs => new[] { HP.Value, Atk.Value, Def.Value, SpA.Value, SpD.Value, Speed.Value };
    }
}