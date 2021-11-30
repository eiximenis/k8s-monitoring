using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeersApi
{
    static class Telemetry
    {
        private static ActivitySource source = new ActivitySource("BeersApi");

        public static ActivitySource ActivitySource { get => source; }
    }
}
