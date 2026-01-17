using FluentValidation;

namespace Application.Users.Commands;

public class UserGoogleAuthenticationCommandValidator : AbstractValidator<UserGoogleAuthenticationCommand>
{
    public UserGoogleAuthenticationCommandValidator()
    {
        RuleFor(x => x.IdToken).NotEmpty().WithMessage("GoogleToken is required");
    }
}
