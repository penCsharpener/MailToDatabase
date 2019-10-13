using System;
using System.Collections.Generic;
using System.Text;

namespace penCsharpener.Mail2DB {
    public class AsyncRetrievalInfo {
        public int CountRetrievedMessages { get; set; }
        public uint[] UniqueIds { get; set; }
    }
}
