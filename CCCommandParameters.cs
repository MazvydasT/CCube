using System;

namespace CCube
{
    public class CCCommandParameters
    {
        public long ProjectId { get; private set; }
        public string NodeExternalId { get; private set; }

        public bool? Incremental { get; set; }
        public bool? CheckIn { get; private set; }
        public bool? ThreeDMapping { get; set; }
        public string VersionName { get; private set; }
        public string Comment { get; set; }

        public CCCommandParameters(long projectId, string nodeExternalId, string versionName = null)
        {
            ProjectId = projectId;
            NodeExternalId = nodeExternalId ?? throw new ArgumentNullException("nodeExternalId");
            CheckIn = versionName != null;
            VersionName = versionName;
        }

        public override string ToString()
        {
            var outputString = $"-ProjId {ProjectId} -NodeExtId {NodeExternalId}";

            if (Incremental != null) outputString += $" -Incremental {(Incremental.Value ? "y" : "n")}";
            if (CheckIn != null) outputString += $" -checkIn {(CheckIn.Value ? "y" : "n")}";
            if (ThreeDMapping != null) outputString += $" -3DMapping {(ThreeDMapping.Value ? "y" : "n")}";
            if (!string.IsNullOrWhiteSpace(VersionName)) outputString += $" -VerName {VersionName}";
            if (!string.IsNullOrWhiteSpace(Comment)) outputString += $" -Comment {Comment}";

            return outputString;
        }
    }
}