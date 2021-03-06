using FluentValidation;
using HappyTravel.Hiroshima.DirectManager.Models.Requests;

namespace HappyTravel.Hiroshima.DirectManager.RequestValidators
{
    public class PermissionsRequestValidator : AbstractValidator<Permissions>
    {
        public PermissionsRequestValidator()
        {
            RuleFor(permissions => permissions.ManagerPermissions).NotEmpty();
        }
    }
}
