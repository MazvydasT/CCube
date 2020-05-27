using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;

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
            get { return startTime; }
            set { SetProperty(ref startTime, value, new[] { "Duration" }); }
        }

        private DateTime? endTime;
        public DateTime? EndTime
        {
            get { return endTime; }
            set { SetProperty(ref endTime, value, new[] { "Duration" }); }
        }

        public TimeSpan? Duration
        {
            get { return StartTime == null || EndTime == null ? null : EndTime - StartTime; }
        }

        private Status? currentStatus;
        public Status? CurrentStatus
        {
            get { return currentStatus; }
            set { SetProperty(ref currentStatus, value, new[] { "CurrentStatusName" }); }
        }

        private string message;
        public string Message
        {
            get { return message; }
            set { SetProperty(ref message, value); }
        }

        private string[] executionLog;
        public string[] ExecutionLog
        {
            get { return executionLog; }
            set { SetProperty(ref executionLog, value, new[] { "ExecutionLogString" }); }
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
    public class Input : BindableBase
    {
        public uint SequenceNumber { get; private set; }

        public string PathToSourceFile { get; private set; }

        public string ChunkName { get; set; }

        public XElement XMLElement { get; set; }

        //public ObservableCollection<Iteration> Iterations { get; } = new ObservableCollection<Iteration>();
        private Iteration[] iterations = new Iteration[0];
        public Iteration[] Iterations
        {
            get { return iterations; }
            private set { SetProperty(ref iterations, value, new[] { "CurrentActiveIteration", "LatestIteration" }); }
        }

        //private Iteration currentActiveIteration;
        public Iteration CurrentActiveIteration
        {
            get { return Iterations.LastOrDefault(iteration => iteration.CurrentStatus != null); }
        }

        public Iteration LatestIteration { get { return Iterations.LastOrDefault(); } }

        public CCCommandParameters CCCommandParameters { get; private set; }

        public Iteration AddIteration()
        {
            var newIteration = CurrentActiveIteration == null ? new Iteration() : CurrentActiveIteration.NextIteration;

            if (newIteration != null)
            {
                void callback(object sender, PropertyChangedEventArgs e)
                {
                    if (e.PropertyName == "CurrentStatus")
                    {
                        var senderIteration = (Iteration)sender;

                        if (senderIteration.CurrentStatus != null)
                        {
                            if (senderIteration.CurrentStatus == Iteration.Status.Failed || senderIteration.CurrentStatus == Iteration.Status.Succeeded)
                            {
                                senderIteration.PropertyChanged -= callback;
                            }

                            OnPropertyChanged("CurrentActiveIteration");
                            OnPropertyChanged("LatestIteration");
                        }
                    }
                }

                newIteration.PropertyChanged += callback;

                Iterations = new List<Iteration>(Iterations) { newIteration }.ToArray();
            }

            return newIteration;
        }

        public Input(uint sequenceNumber, string pathToSourceFile, CCCommandParameters ccCommandParameters)
        {
            SequenceNumber = sequenceNumber;
            PathToSourceFile = pathToSourceFile;
            CCCommandParameters = ccCommandParameters;
        }
    }
}