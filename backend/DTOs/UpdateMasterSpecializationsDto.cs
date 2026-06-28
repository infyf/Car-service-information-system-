using System.Collections.Generic;

namespace AutoServ.DTOs
{
    public class UpdateMasterSpecializationsDto
    {
        public List<int> ServiceItemIds { get; set; } = new List<int>();
    }
}