﻿using System.Diagnostics;
using System.IO;
using System.Text;

namespace Bittrex.Net.Logging
{
    internal class TraceTextWriter : TextWriter
    {
        public override Encoding Encoding => Encoding.ASCII;

        public override void WriteLine(string line)
        {
            Trace.WriteLine(line);
        }
    }
}
