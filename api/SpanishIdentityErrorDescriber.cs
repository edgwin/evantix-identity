using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Reflection;

namespace IdentityService
{    
    public class SpanishIdentityErrorDescriber : IdentityErrorDescriber
    {
        private readonly IStringLocalizer<SharedResource> _localizer;
        public readonly string culture;
        public SpanishIdentityErrorDescriber(IStringLocalizer<SharedResource> localizer)
        {            
            _localizer = localizer;
            culture = System.Threading.Thread.CurrentThread.CurrentCulture.ToString();
            System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
        }
        public override IdentityError DefaultError() { _localizer.WithCulture(new CultureInfo(culture)); return new IdentityError { Code = nameof(DefaultError), Description = _localizer["Ha ocurrido un error."].Value }; }
        public override IdentityError ConcurrencyFailure() { _localizer.WithCulture(new CultureInfo(culture)); return new IdentityError { Code = nameof(ConcurrencyFailure), Description = _localizer["Ha ocurrido un error, el objeto ya ha sido modificado (Optimistic concurrency failure)."].Value }; }
        public override IdentityError PasswordMismatch() { _localizer.WithCulture(new CultureInfo(culture)); return new IdentityError { Code = nameof(PasswordMismatch), Description = _localizer["Password Incorrecta."].Value }; }
        public override IdentityError InvalidToken() { _localizer.WithCulture(new CultureInfo(culture)); return new IdentityError { Code = nameof(InvalidToken), Description = _localizer["Token Invalido."].Value }; }
        public override IdentityError LoginAlreadyAssociated() { _localizer.WithCulture(new CultureInfo(culture)); return new IdentityError { Code = nameof(LoginAlreadyAssociated), Description = _localizer["Un usuario con ese nombre ya existe."].Value }; }
        public override IdentityError InvalidUserName(string userName) { _localizer.WithCulture(new CultureInfo(culture)); return new IdentityError { Code = nameof(InvalidUserName), Description = string.Format(_localizer["El nombre de usuario '{0}' es inválido. Solo puede contener letras y números." ].Value, userName) }; }
        public override IdentityError InvalidEmail(string email) { _localizer.WithCulture(new CultureInfo(culture)); return new IdentityError { Code = nameof(InvalidEmail), Description = string.Format(_localizer["La dirección de email '{0}' es incorrecta."].Value, email) }; }
        public override IdentityError DuplicateUserName(string userName) { _localizer.WithCulture(new CultureInfo(culture)); return new IdentityError { Code = nameof(DuplicateUserName), Description = string.Format(_localizer["El usuario '{0}' ya existe, por favor ingrese un nombre diferente."].Value, userName) }; }
        public override IdentityError DuplicateEmail(string email) { _localizer.WithCulture(new CultureInfo(culture)); return new IdentityError { Code = nameof(DuplicateEmail), Description = string.Format(_localizer["El Email '{0}' ya se encuentra registrada."].Value, email) }; }
        public override IdentityError InvalidRoleName(string role) { _localizer.WithCulture(new CultureInfo(culture)); return new IdentityError { Code = nameof(InvalidRoleName), Description = string.Format(_localizer["El nombre de rol '{0}' es inválido."].Value, role) }; }
        public override IdentityError DuplicateRoleName(string role) { _localizer.WithCulture(new CultureInfo(culture)); return new IdentityError { Code = nameof(DuplicateRoleName), Description = string.Format(_localizer["El nombre de rol '{0}' ya existe."].Value, role) }; }
        public override IdentityError UserAlreadyHasPassword() { _localizer.WithCulture(new CultureInfo(culture)); return new IdentityError { Code = nameof(UserAlreadyHasPassword), Description = _localizer["El usuario ya tiene contraseña."].Value }; }
        public override IdentityError UserLockoutNotEnabled() { _localizer.WithCulture(new CultureInfo(culture)); return new IdentityError { Code = nameof(UserLockoutNotEnabled), Description = _localizer["El bloqueo no esta habilitado para este usuario."].Value }; }
        public override IdentityError UserAlreadyInRole(string role) { _localizer.WithCulture(new CultureInfo(culture)); return new IdentityError { Code = nameof(UserAlreadyInRole), Description = string.Format(_localizer["El usuario ya es parte del rol '{0}'."], role) }; }
        public override IdentityError UserNotInRole(string role) { _localizer.WithCulture(new CultureInfo(culture)); return new IdentityError { Code = nameof(UserNotInRole), Description =  string.Format(_localizer["El usuario no es parte del rol '{0}'."].Value, role) }; }
        public override IdentityError PasswordTooShort(int length) { _localizer.WithCulture(new CultureInfo(culture)); return new IdentityError { Code = nameof(PasswordTooShort), Description = string.Format(_localizer["La contraseña deben tener un largo mínimo de {0} caracteres."].Value, length) }; }
        public override IdentityError PasswordRequiresNonAlphanumeric() { _localizer.WithCulture(new CultureInfo(culture)); return new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric), Description = _localizer["La contraseña debe contener al menos un caracter alfanumérico."] }; }
        public override IdentityError PasswordRequiresDigit() { _localizer.WithCulture(new CultureInfo(culture)); return new IdentityError { Code = nameof(PasswordRequiresDigit), Description = _localizer["La contraseña debe incluir al menos un dígito ('0'-'9')."].Value }; }
        public override IdentityError PasswordRequiresLower() { _localizer.WithCulture(new CultureInfo(culture)); return new IdentityError { Code = nameof(PasswordRequiresLower), Description = _localizer["La contraseña debe incluir al menos una letra minúscula ('a'-'z')."].Value }; }
        public override IdentityError PasswordRequiresUpper() { _localizer.WithCulture(new CultureInfo(culture)); return new IdentityError { Code = nameof(PasswordRequiresUpper), Description = _localizer["La contraseña debe incluir al menos una letra MAYÚSCULA ('A'-'Z')."].Value }; }
    }
}
