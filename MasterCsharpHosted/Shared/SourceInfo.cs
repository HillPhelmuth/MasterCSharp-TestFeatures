using System.Collections.Generic;
using Newtonsoft.Json;

namespace MasterCsharpHosted.Shared
{
    public class SourceInfo
    {

        public string SourceCode { get; set; }

        public int LineNumberOffsetFromTemplate { get; set; }
        
        public int Line { get; set; }
        
        public int Column { get; set; }

    }

    public class SignatureHelpRequestItem
    {
        [JsonProperty("Line")]
        public int Line { get; set; }
        [JsonProperty("Column")]
        public int Column { get; set; }

    }
}
