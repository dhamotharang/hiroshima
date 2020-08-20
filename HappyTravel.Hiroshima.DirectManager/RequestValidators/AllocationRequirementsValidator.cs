﻿using FluentValidation;

namespace HappyTravel.Hiroshima.DirectManager.RequestValidators
{
    public class AllocationRequirementsValidator : AbstractValidator<Models.Requests.AllocationRequirement>
    {
        public AllocationRequirementsValidator()
        {
            RuleFor(requirements => requirements.ReleaseDays).GreaterThanOrEqualTo(0);
            RuleFor(requirements => requirements.MinimumLengthOfStay).GreaterThanOrEqualTo(0)
                .When(requirements => requirements.MinimumLengthOfStay != null);
            RuleFor(requirements => requirements.Allotment).GreaterThanOrEqualTo(0);
        }
    }
}