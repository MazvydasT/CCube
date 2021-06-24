using System;

namespace CCube
{
    public class Iteration : BindableBase
    {
        public enum Status
        {
            Waiting,
            Running,
            Succeeded,
            Failed
        }

        public ulong IterationNumber { get; private set; }

        private DateTime? startTime;
        public DateTime? StartTime
        {
            get => startTime;
            set => SetProperty(ref startTime, value, new[] { "Duration" });
        }

        private DateTime? endTime;
        public DateTime? EndTime
        {
            get => endTime;
            set => SetProperty(ref endTime, value, new[] { "Duration" });
        }

        public TimeSpan? Duration
        {
            get { return StartTime == null || EndTime == null ? null : EndTime - StartTime; }
        }

        private Status? currentStatus;
        public Status? CurrentStatus
        {
            get => currentStatus;
            set => SetProperty(ref currentStatus, value, new[] { "CurrentStatusName" });
        }

        private string message;
        public string Message
        {
            get => message;
            set => SetProperty(ref message, value);
        }

        private string[] executionLog;
        public string[] ExecutionLog
        {
            get => executionLog;
            set => SetProperty(ref executionLog, value, new[] { "ExecutionLogString" });
        }

        public string ExecutionLogString { get { return string.Join("\r\n", ExecutionLog ?? new string[0]); } }

        private Iteration nextIteration = null;
        public Iteration NextIteration
        {
            get
            {
                if (nextIteration == null && (CurrentStatus != Status.Succeeded || CurrentStatus != Status.Failed))
                    nextIteration = new Iteration(IterationNumber + 1);

                return nextIteration;
            }
        }

        public Iteration() : this(1) { }

        private Iteration(ulong iterationNumber)
        {
            IterationNumber = iterationNumber;
        }
    }
}