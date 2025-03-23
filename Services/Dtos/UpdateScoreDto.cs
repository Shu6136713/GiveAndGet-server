using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Dtos
{
    public class UpdateScoreDto
    {
        public int DealId { get; set; } // מזהה העסקה
        public int Action { get; set; } // 1 = לייק, 0 = דיסלייק
    }
}
