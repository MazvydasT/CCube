using System;

namespace CCube
{
    public class Stats : BindableBase
    {
        private Stats() { }

        public static Stats Service { get; } = new Stats();



        // INPUTS

        private uint inputsTotal = 0;
        public uint InputsTotal
        {
            get => inputsTotal;
            set => SetProperty(ref inputsTotal, value, new[] { "EstimatedRemainingTime", "EstimatedTimeOfCompletion" });
        }

        private uint inputsWaiting = 0;
        public uint InputsWaiting
        {
            get => inputsWaiting;
            set => SetProperty(ref inputsWaiting, value);
        }

        private uint inputsRunning = 0;
        public uint InputsRunning
        {
            get => inputsRunning;
            set => SetProperty(ref inputsRunning, value);
        }

        private uint inputsSuccessful = 0;
        public uint InputsSuccessful
        {
            get => inputsSuccessful;
            set => SetProperty(ref inputsSuccessful, value, new[] { "InputsProcessed", "InputsSuccessfulRate" });
        }

        private uint inputsFailed = 0;
        public uint InputsFailed
        {
            get => inputsFailed;
            set => SetProperty(ref inputsFailed, value, new[] { "InputsProcessed", "InputsSuccessfulRate" });
        }

        public uint InputsProcessed => InputsSuccessful + InputsFailed;

        public double InputsSuccessfulRate => InputsProcessed == 0 ? 1 : (double)InputsSuccessful / InputsProcessed;



        // TIMING

        private DateTime? importStartTime = null;
        public DateTime? ImportStartTime
        {
            get => importStartTime;
            set => SetProperty(ref importStartTime, value, new[] { "AverageIterationDuration", "EstimatedRemainingTime", "EstimatedTimeOfCompletion" });
        }

        public TimeSpan? EstimatedRemainingTime => InputsTotal < 1 || AverageIterationDuration == null ? (TimeSpan?)null :
            new TimeSpan((InputsTotal - InputsSuccessful) * AverageIterationDuration.Value.Ticks);

        public DateTime? EstimatedTimeOfCompletion => EstimatedRemainingTime == null ? (DateTime?)null :
            DateTime.Now.AddTicks(EstimatedRemainingTime.Value.Ticks);



        // ITERATIONS

        private uint iterationsTotal = 0;
        public uint IterationsTotal
        {
            get => iterationsTotal;
            set => SetProperty(ref iterationsTotal, value, "IterationsSuccessRate");
        }

        private uint iterationsSuccessful = 0;
        public uint IterationsSuccessful
        {
            get => iterationsSuccessful;
            set => SetProperty(ref iterationsSuccessful, value, "IterationsSuccessRate");
        }

        private uint iterationsSuccessfulSinceStart = 0;
        public uint IterationsSuccessfulSinceStart
        {
            get => iterationsSuccessfulSinceStart;
            set => SetProperty(ref iterationsSuccessfulSinceStart, value, new[] { "AverageIterationDuration", "EstimatedRemainingTime", "EstimatedTimeOfCompletion" });
        }

        private uint iterationsFailed = 0;
        public uint IterationsFailed
        {
            get => iterationsFailed;
            set => SetProperty(ref iterationsFailed, value);
        }

        public double IterationsSuccessRate => IterationsTotal == 0 ? 1 : (double)IterationsSuccessful / IterationsTotal;

        private DateTime? latestIterationCompleteTime = null;
        public DateTime? LatestIterationCompleteTime
        {
            get => latestIterationCompleteTime;
            set => SetProperty(ref latestIterationCompleteTime, value, new[] { "AverageIterationDuration", "EstimatedRemainingTime", "EstimatedTimeOfCompletion" });
        }

        public TimeSpan? AverageIterationDuration =>
            LatestIterationCompleteTime == null || ImportStartTime == null || IterationsSuccessfulSinceStart == 0 ? (TimeSpan?)null :
            new TimeSpan((LatestIterationCompleteTime - ImportStartTime).Value.Ticks / IterationsSuccessfulSinceStart);

        // METHODS

        public void Reset()
        {
            InputsTotal = InputsWaiting = InputsRunning = InputsSuccessful = InputsFailed = IterationsTotal = IterationsSuccessful = IterationsSuccessfulSinceStart = IterationsFailed = 0;
            ImportStartTime = null;
            LatestIterationCompleteTime = null;
        }

        public void Recalculate()
        {
            var service = ApplicationData.Service;

            var inputs = service.Inputs;
            //var inputsCount = inputs.Count;

            uint inputsWaiting = 0;
            uint inputsRunning = 0;
            uint inputsSuccessful = 0;
            uint inputsFailed = 0;

            foreach (var input in inputs)
            //for (var i = 0; i < inputsCount; ++i)
            {
                //var input = inputs[i];

                switch (input.CurrentActiveIteration.CurrentStatus)
                {
                    case Iteration.Status.Waiting:
                        ++inputsWaiting;
                        break;

                    case Iteration.Status.Running:
                        ++inputsRunning;
                        break;

                    case Iteration.Status.Succeeded:
                        ++inputsSuccessful;
                        break;

                    case Iteration.Status.Failed:
                        ++inputsFailed;
                        break;
                }
            }

            InputsWaiting = inputsWaiting;
            InputsRunning = inputsRunning;
            InputsSuccessful = inputsSuccessful;
            InputsFailed = inputsFailed;
        }
    }
}