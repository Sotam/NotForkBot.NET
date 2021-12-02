using PKHeX.Core;
using SysBot.Base;
using System;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Base.SwitchStick;

namespace SysBot.Pokemon
{
    public abstract class EncounterBotBS : PokeRoutineExecutor8BS, IEncounterBot
    {
        protected readonly PokeTradeHub<PB8> Hub;
        private readonly EncounterRNGBSSettings Settings;
        private readonly int[] DesiredMinIVs;
        private readonly int[] DesiredMaxIVs;
        public ICountSettings Counts => Settings;

        protected EncounterBotBS(PokeBotState cfg, PokeTradeHub<PB8> hub) : base(cfg)
        {
            Hub = hub;
            Settings = Hub.Config.EncounterRNGBS;
            StopConditionSettings.InitializeTargetIVs(Hub, out DesiredMinIVs, out DesiredMaxIVs);
        }

        private int encounterCount;

        public override async Task MainLoop(CancellationToken token)
        {
            var settings = Hub.Config.EncounterRNGBS;
            Log("Identifying trainer data of the host console.");
            var sav = await IdentifyTrainer(false, token).ConfigureAwait(false);
            await InitializeHardware(settings, token).ConfigureAwait(false);

            try
            {
                Log($"Starting main {GetType().Name} loop.");
                Config.IterateNextRoutine();

                // Clear out any residual stick weirdness.
                await ResetStick(token).ConfigureAwait(false);
                await EncounterLoop(sav, token).ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Log(e.Message);
            }

            Log($"Ending {GetType().Name} loop.");
            await HardStop().ConfigureAwait(false);
        }

        public override async Task HardStop()
        {
            await ResetStick(CancellationToken.None).ConfigureAwait(false);
            await CleanExit(Settings, CancellationToken.None).ConfigureAwait(false);
        }

        protected abstract Task EncounterLoop(SAV8BS sav, CancellationToken token);

        // return true if breaking loop
        protected async Task<bool> HandleEncounter(PB8 pk, EncounterMode emode, uint chain, CancellationToken token)
        {
            encounterCount++;
            var print = Hub.Config.StopConditions.GetPrintName(pk);
            Log($"Encounter: {encounterCount}{Environment.NewLine}{print}{Environment.NewLine}");

            if (!StopConditionSettings.EncounterFound(pk, DesiredMinIVs, DesiredMaxIVs, Hub.Config.StopConditions, null))
                return false;

            if (Hub.Config.StopConditions.CaptureVideoClip)
            {
                await Task.Delay(Hub.Config.StopConditions.ExtraTimeWaitCaptureVideo, token).ConfigureAwait(false);
                await PressAndHold(CAPTURE, 2_000, 1_000, token).ConfigureAwait(false);
            }

            var msg = $"Result found!\n{print}";
            if (!string.IsNullOrWhiteSpace(Hub.Config.StopConditions.MatchFoundEchoMention))
                msg = $"{Hub.Config.StopConditions.MatchFoundEchoMention} {msg}";
            EchoUtil.Echo(msg);

            var mode = Settings.ContinueAfterMatch;
            msg = mode switch
            {
                ContinueAfterMatch.Continue             => "Continuing...",
                ContinueAfterMatch.PauseWaitAcknowledge => "Waiting for instructions to continue.",
                ContinueAfterMatch.StopExit             => "Stopping routine execution; restart the bot(s) to search again.",
                _ => throw new ArgumentOutOfRangeException(),
            };

            if (!string.IsNullOrWhiteSpace(Hub.Config.StopConditions.MatchFoundEchoMention))
                msg = $"{Hub.Config.StopConditions.MatchFoundEchoMention} {msg}";
            EchoUtil.Echo(msg);
            Log(msg);

            if (mode == ContinueAfterMatch.StopExit)
                return true;
            if (mode == ContinueAfterMatch.Continue)
                return false;

            IsWaiting = true;
            while (IsWaiting)
                await Task.Delay(1_000, token).ConfigureAwait(false);
            return false;
        }

        private bool IsWaiting;
        public void Acknowledge() => IsWaiting = false;

        protected async Task ResetStick(CancellationToken token)
        {
            // If aborting the sequence, we might have the stick set at some position. Clear it just in case.
            await SetStick(LEFT, 0, 0, 0_500, token).ConfigureAwait(false); // reset
        }
    }
}
