using System;
using System.Collections.Generic;
using System.Text;

namespace penCsharpener.Mail2DB {
    public class AsyncRetrievalInfo {
        public int CountRetrievedMessages { get; internal set; }
        public uint[] UniqueIds { get; internal set; }
        public int Index { get; internal set; }
    }
}
