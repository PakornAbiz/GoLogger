using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoLogger
{
    class Stats
    {
        public int OK { get; set; }
        public int NG { get; set; }
        public int Count { get; set; }

        public int Total => OK + NG;

        public Stats()
        {
            OK = 0;
            NG = 0;
            Count = 0;
        }
        public decimal GetPerLoss() => OK / Total * 100;
    }
}
