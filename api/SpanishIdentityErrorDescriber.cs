using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace IdentityService
{
    public class SpanishIdentityErrorDescriber : IdentityErrorDescriber
    {
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly CultureInfo _culture;

        public SpanishIdentityErrorDescriber(IStringLocalizer<SharedResource> localizer)
        {
            _localizer = localizer;
            _culture = new CultureInfo("es-ES"); // Assuming "es-ES" for Spanish (Spain)
            CultureInfo.CurrentCulture = _culture;
            CultureInfo.CurrentUICulture = _culture;
        }

        public override IdentityError DefaultError()
        {
            return new IdentityError { Code = nameof(DefaultError), Description = _localizer["Ha ocurrido un error."] };
        }

        public override IdentityError ConcurrencyFailure()
        {
            return new IdentityError { Code = nameof(ConcurrencyFailure), Description = _localizer["Ha ocurrido un error, el objeto ya ha sido modificado (Optimistic concurrency failure)."] };
        }

        public override IdentityError PasswordMismatch()
        {
            return new IdentityError { Code = nameof(PasswordMismatch), Description = _localizer["Password Incorrecta."] };
        }

        public override IdentityError InvalidToken()
        {
            return new IdentityError { Code = nameof(InvalidToken), Description = _localizer["Token Invalido."] };
        }

        public override IdentityError LoginAlreadyAssociated()
        {
            return new IdentityError { Code = nameof(LoginAlreadyAssociated), Description = _localizer["Un usuario con ese nombre ya existe."] };
        }

        public override IdentityError InvalidUserName(string userName)
        {
            return new IdentityError { Code = nameof(InvalidUserName), Description = string.Format(_localizer["El nombre de usuario '{0}' es inválido. Solo puede contener letras y números."], userName) };
        }

        public override IdentityError InvalidEmail(string email)
        {
            return new IdentityError { Code = nameof(InvalidEmail), Description = string.Format(_localizer["La dirección de email '{0}' es incorrecta."], email) };
        }

        public override IdentityError DuplicateUserName(string userName)
        {
            return new IdentityError { Code = nameof(DuplicateUserName), Description = string.Format(_localizer["El usuario '{0}' ya existe, por favor ingrese un nombre diferente."], userName) };
        }

        public override IdentityError DuplicateEmail(string email)
        {
            return new IdentityError { Code = nameof(DuplicateEmail), Description = string.Format(_localizer["El Email '{0}' ya se encuentra registrada."], email) };
        }

        public override IdentityError InvalidRoleName(string role)
        {
            return new IdentityError { Code = nameof(InvalidRoleName), Description = string.Format(_localizer["El nombre de rol '{0}' es inválido."], role) };
        }

        public override IdentityError DuplicateRoleName(string role)
        {
            return new IdentityError { Code = nameof(DuplicateRoleName), Description = string.Format(_localizer["El nombre de rol '{0}' ya existe."], role) };
        }

        public override IdentityError UserAlreadyHasPassword()
        {
            return new IdentityError { Code = nameof(UserAlreadyHasPassword), Description = _localizer["El usuario ya tiene contraseña."] };
        }

        public override IdentityError UserLockoutNotEnabled()
        {
            return new IdentityError { Code = nameof(UserLockoutNotEnabled), Description = _localizer["El bloqueo no esta habilitado para este usuario."] };
        }

        public override IdentityError UserAlreadyInRole(string role)
        {
            return new IdentityError { Code = nameof(UserAlreadyInRole), Description = string.Format(_localizer["El usuario ya es parte del rol '{0}'."], role) };
        }

        public override IdentityError UserNotInRole(string role)
        {
            return new IdentityError { Code = nameof(UserNotInRole), Description = string.Format(_localizer["El usuario no es parte del rol '{0}'."], role) };
        }

        public override IdentityError PasswordTooShort(int length)
        {
            return new IdentityError { Code = nameof(PasswordTooShort), Description = string.Format(_localizer["La contraseña debe tener un largo mínimo de {0} caracteres."], length) };
        }

        public override IdentityError PasswordRequiresNonAlphanumeric()
        {
            return new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric), Description = _localizer["La contraseña debe contener al menos un caracter no alfanumérico."] };
        }

        public override IdentityError PasswordRequiresDigit()
        {
            return new IdentityError { Code = nameof(PasswordRequiresDigit), Description = _localizer["La contraseña debe incluir al menos un dígito ('0'-'9')."] };
        }

        public override IdentityError PasswordRequiresLower()
        {
            return new IdentityError { Code = nameof(PasswordRequiresLower), Description = _localizer["La contraseña debe incluir al menos una letra minúscula ('a'-'z')."] };
        }

        public override IdentityError PasswordRequiresUpper()
        {
            return new IdentityError { Code = nameof(PasswordRequiresUpper), Description = _localizer["La contraseña debe incluir al menos una letra MAYÚSCULA ('A'-'Z')."] };
        }
    }
}