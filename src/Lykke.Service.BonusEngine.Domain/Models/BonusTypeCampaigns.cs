﻿using System;
using System.Collections.Generic;

namespace Lykke.Service.BonusEngine.Domain.Models
{
    public class BonusTypeCampaigns
    {
        public string TypeName { get; set; }

        public ICollection<Guid> CampaignsIds { get; set; }
    }
}
