using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;

namespace CCube
{
    public class Input : BindableBase
    {
        public uint SequenceNumber { get; private set; }

        public string PathToSourceFile { get; private set; }

        public string ChunkName { get; set; }

        public XElement XMLElement { get; set; }

        private Iteration[] iterations = new Iteration[0];
        public Iteration[] Iterations
        {
            get => iterations;
            private set => SetProperty(ref iterations, value, new[] { "CurrentActiveIteration", "LatestIteration" });
        }

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