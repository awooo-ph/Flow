using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FloodMonitor.ViewModels
{
    class WaterLevel:ModelBase<WaterLevel>
    {
        protected override bool GetIsEmpty()
        {
            return false;
        }

        public long SensorId { get; set; }
        public int Level { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now;
    }
}
