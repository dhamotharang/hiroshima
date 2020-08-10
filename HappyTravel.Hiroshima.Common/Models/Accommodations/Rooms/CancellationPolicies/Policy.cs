﻿namespace HappyTravel.Hiroshima.Common.Models.Accommodations.Rooms.CancellationPolicies
{
    /// <summary>
    /// Represents a cancellation policy during days interval 
    /// Example: 28-14 days prior to arrival: penalty 4 night
    /// </summary>
    public class CancellationPolicyItem
    {
        /// <summary>
        /// Days prior to arrival
        /// E.g. 28-14 days prior to arrival
        /// </summary>
        public DayInterval DayPriorToArrival { get; set; } 
        
        /// <summary>
        /// Type of the cancellation penalty
        /// </summary>
        public CancellationPenaltyTypes PenaltyType { get; set; }
        
        /// <summary>
        /// In nights number or percentage
        /// </summary>
        public double PenaltyCharge { get; set; }
    }
}