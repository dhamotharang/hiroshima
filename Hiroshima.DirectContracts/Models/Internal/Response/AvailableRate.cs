﻿using System.Collections.Generic;
using Hiroshima.DbData.Models.Rooms;

namespace Hiroshima.DirectContracts.Models.Internal.Response
{
    public struct AvailableRate
    {
        public Room Room { get; set; }
        public PaymentDetails PaymentDetails { get; set; }
        public List<CancellationPolicyDetails> CancellationPolicies { get; set; }
        public List<TaxDetails> Taxes { get; set; }
        public List<string> Amenities { get; set; } 
        public string Meal { get; set; }
        public string BoardBasis { get; set; }
    }
}