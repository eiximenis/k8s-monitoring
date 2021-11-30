using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beers.Data
{
    static class Telemetry
    {
        private static ActivitySource source = new ActivitySource("Beers.Data");

        public static ActivitySource ActivitySource { get => source; }
    }
}
