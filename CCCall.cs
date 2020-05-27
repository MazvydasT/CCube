using System;

namespace CCube
{
    public class CheckInOptions
    {
        public bool CheckIn { get; private set; }
        public bool? CheckInAsNew { get; private set; }
        public string Version { get; private set; }
        public string Comment { get; set; }
        public bool? KeepCheckOut { get; set; }

        public CheckInOptions(bool checkIn)
        {
            CheckIn = checkIn;
        }

        public CheckInOptions(string version) : this(true)
        {
            CheckInAsNew = true;
            Version = version ?? throw new ArgumentNullException("version");
        }
    }
    public class CCCall
    {
        public long ProjectId { get; private set; }
        public string NodeExternalId { get; private set; }
        public bool? Incremental { get; set; }
        public bool? Skip3Dmapping { get; set; }

        public CheckInOptions CheckInOptions { get; set; }

        public CCCall(long projectId, string nodeExternalId)
        {
            ProjectId = projectId;
            NodeExternalId = nodeExternalId ?? throw new ArgumentNullException("nodeExternalId");
        }
    }
}