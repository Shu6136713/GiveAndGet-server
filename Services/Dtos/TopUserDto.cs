using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Dtos
{
    public class TopUserDto
    {
        public string UserName { get; set; }
        public int Score { get; set; }
        public string Desc { get; set; }
        public string? ProfileImageUrl { get; set; }
    }

}
