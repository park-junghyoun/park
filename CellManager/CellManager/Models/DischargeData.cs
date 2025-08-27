using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CellManager.Models
{
    public class DischargeData
    {
        public int Id { get; set; }
        public int CellId { get; set; }
        public double Time { get; set; }
        public double Current { get; set; }
        public double Voltage { get; set; }
    }
}
