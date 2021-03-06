using System;
using FluentValidation;
using FluentValidation.Validators;
using HappyTravel.Hiroshima.Common.Infrastructure;
using HappyTravel.Hiroshima.DirectManager.Models.Requests;
using HappyTravel.Hiroshima.DirectManager.RequestValidators.Extensions;

namespace HappyTravel.Hiroshima.DirectManager.RequestValidators
{
    public class AccommodationValidator : AbstractValidator<Accommodation>
    {
        public AccommodationValidator(IDateTimeProvider dateTimeProvider)
        {
            RuleFor(accommodation => accommodation.Name).NotNull().AnyLanguage()
                .ChildRules(validator => validator.RuleFor(name => name.Ar).NotEmpty().When(name => name.Ar != null))
                .ChildRules(validator => validator.RuleFor(name => name.En).NotEmpty().When(name => name.En != null))
                .ChildRules(validator => validator.RuleFor(name => name.Ru).NotEmpty().When(name => name.Ru != null));
            
            RuleFor(accommodation => accommodation.Address).NotNull().AnyLanguage()
                .ChildRules(validator => validator.RuleFor(address => address.Ar).NotEmpty().When(address => address.Ar != null))
                .ChildRules(validator => validator.RuleFor(address => address.En).NotEmpty().When(address => address.En != null))
                .ChildRules(validator => validator.RuleFor(address => address.Ru).NotEmpty().When(address => address.Ru != null));
            
            RuleFor(accommodation => accommodation.Description)
                .NotEmpty()
                .AnyLanguage()
                .ChildRules(validator => validator.RuleFor(textualDescription => textualDescription.Ar).SetValidator(new TextualDescriptionValidator()))
                .ChildRules(validator => validator.RuleFor(textualDescription => textualDescription.En).SetValidator(new TextualDescriptionValidator()))
                .ChildRules(validator => validator.RuleFor(textualDescription => textualDescription.Ru).SetValidator(new TextualDescriptionValidator()));
            RuleFor(accommodation => accommodation.Coordinates).NotNull();
            RuleFor(accommodation => accommodation.Rating).NotNull();
            RuleFor(accommodation => accommodation.Type).NotNull().IsInEnum();
            RuleFor(accommodation => accommodation.CheckInTime).NotEmpty().Must(IsValidTimeFormat);
            RuleFor(accommodation => accommodation.CheckOutTime).NotEmpty().Must(IsValidTimeFormat);
            RuleFor(accommodation => accommodation.ContactInfo).NotEmpty();
            RuleFor(accommodation => accommodation.ContactInfo.Emails)
                .NotEmpty()
                .ForEach(action => action.SetValidator(new AspNetCoreCompatibleEmailValidator()));
            RuleFor(accommodation => accommodation.ContactInfo.Phones)
                .NotEmpty()
                .ForEach(action => action.SetValidator(new PhoneNumberValidator()));
            RuleFor(accommodation => accommodation.ContactInfo.Websites)
                .NotEmpty()
                .ForEach(action => action.SetValidator(new UriValidator()));
            RuleFor(accommodation => accommodation.OccupancyDefinition).SetValidator(new OccupancyDefinitionValidator()).NotNull();
            RuleFor(accommodation => accommodation.Amenities).AnyLanguage().When(accommodation => accommodation.Amenities != null);
            RuleFor(accommodation => accommodation.LocationId).NotEmpty();
            RuleFor(accommodation => accommodation.Status).IsInEnum();
            RuleFor(accommodation => accommodation.BuildYear).LessThanOrEqualTo(dateTimeProvider.UtcNow().Year).When(accommodation => accommodation.BuildYear != null);
            RuleFor(accommodation => accommodation.Floors).GreaterThan(0).When(accommodation => accommodation.Floors != null);
            RuleFor(accommodation => accommodation.LeisureAndSports)
                .AnyLanguage()
                .When(accommodation => accommodation.LeisureAndSports != null);
        }
        
        
        private bool IsValidTimeFormat(string input) => TimeSpan.TryParse(input, out _);
    }
}