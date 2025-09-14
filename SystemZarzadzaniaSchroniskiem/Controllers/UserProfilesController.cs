using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity.UI.Services;
using SystemZarzadzaniaSchroniskiem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace SystemZarzadzaniaSchroniskiem.Controllers
{
    public record ProfileWithRoles
    {
        public required UserProfile Profile { get; init; }
        public required List<string> Roles { get; init; }

    }

    public class LoginModel
    {
        [Display(Name = "E-Mail")]
        [Required(ErrorMessage = "Pole E-Mail jest wymagane.")]
        public string Email { get; set; } = null!;


        [Display(Name = "Hasło")]
        public string Password { get; set; } = null!;

        [Display(Name = "Zapamiętaj mnie")]
        public bool RememberMe { get; set; }
    }

    public class EditProfileModel
    {
        public int Id { get; set; }

        [Display(Name = "Imię")]
        public string FirstName { get; set; } = null!;

        [Display(Name = "Nazwisko")]
        public string LastName { get; set; } = null!;

        [Display(Name = "Obecne hasło")]
        public string? Password { get; set; }

        [Display(Name = "Nowe hasło")]
        public string? NewPassword { get; set; }

        [Display(Name = "Potwierdź nowe hasło")]
        public string? ConfirmPassword { get; set; }
    }

    public class ResetPasswordInputModel
    {
        public string UserId { get; set; } = null!;
        public string Code { get; set; } = null!;

        [Display(Name = "Nowe hasło")]
        public string Password { get; set; } = null!;

        [Display(Name = "Potwierdź nowe hasło")]
        public string ConfirmPassword { get; set; } = null!;
    }

    public class UserProfilesController(
        Areas.Identity.Data.SchroniskoDbContext context,
        IWebHostEnvironment env,
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IUserStore<IdentityUser> userStore,
        IEmailSender sender,
        ILogger<SchroniskoController> logger) : SchroniskoController(
            context, env, userManager, signInManager, userStore, sender, logger)

    {
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Index()
        {
            var profiles =
                await _dbContext.UserProfiles
                    .Include(p => p.User)
                    .Select(p => new ProfileWithRoles
                    {
                        Profile = p,
                        Roles = (from ur in _dbContext.UserRoles
                                 join role in _dbContext.Roles on ur.RoleId equals role.Id
                                 where ur.UserId == p.UserId
                                 select role.Name).ToList()
                    }).ToListAsync();
            ViewBag.Profiles = profiles;

            return View();
        }


        public IActionResult Login()
        {
            // if (User.Identity?.IsAuthenticated ?? false)
            // {
            //     AddViewMessage("Jesteś już zalogowany.", ViewMessageType.Alert, "Uwaga");
            //     return RedirectToAction("Index", "Home");
            // }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login([Bind("Email,Password,RememberMe")] LoginModel input)
        {
            if (ModelState.IsValid)
            {
                var profile = await _dbContext.UserProfiles.Include(p => p.User).FirstOrDefaultAsync(p => p.User.Email == input.Email);
                if (profile == null)
                {
                    ModelState.AddModelError(string.Empty, "Nie znaleziono użytkownika o podanych danych.");
                    return View();
                }

                if (profile.IsLocked)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Twoje konto zostało zablokowane po 3 nieudanych próbach logowania. Prosimy zresetować hasło.");
                    return View();
                }

                var result = await _signInManager.PasswordSignInAsync(
                    input.Email, input.Password, input.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    AddViewMessage("Jesteś zalogowany na swoje konto!", ViewMessageType.Success, "Logowanie udane");
                    return RedirectToAction("Index", "Home");
                }
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? Id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Forbid();
            }

            UserProfile? profile;

            if (Id == null)
            {
                var userId = _userManager.GetUserId(User);
                profile = await _dbContext.UserProfiles.Include(p => p.User).SingleOrDefaultAsync(p => p.UserId == userId);
            }
            else
            {
                profile = await _dbContext.UserProfiles.Include(p => p.User).SingleOrDefaultAsync(p => p.Id == Id);
            }


            if (profile == null)
            {
                return NotFound();
            }

            List<string> roles = ProfilesWithRoles().SingleOrDefault(p => p.Profile == profile)?.Roles.ToList() ?? [];
            ViewBag.Roles = roles;

            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Administrator");
            var isProfileOwner = currentUser == profile.User;

            if (!isAdmin && !isProfileOwner)
            {
                return Forbid();
            }

            var editObject = new EditProfileModel
            {
                Id = profile.Id,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
            };

            return View(editObject);
        }

        [HttpPost(Order = 1)]
        public async Task<IActionResult> Edit([Bind] EditProfileModel input)
        {
            var profile = await _dbContext.UserProfiles
                .Include(p => p.User)
                .SingleOrDefaultAsync(p => p.Id == input.Id);

            if (profile == null)
            {
                return NotFound();
            }

            profile.FirstName = input.FirstName;
            profile.LastName = input.LastName;
            await _dbContext.SaveChangesAsync();

            AddViewMessage("Pomyślnie zaktualizowano profil.", ViewMessageType.Success, "Sukces");



            if (!input.Password.IsNullOrEmpty())
            {
                if (input.NewPassword != input.ConfirmPassword)
                {
                    ModelState.AddModelError("NewPassword", "Podane hasła się różnią.");
                    ModelState.AddModelError("ConfirmPassword", "Podane hasła się różnią.");
                    AddViewMessage("Podczas aktualizacji profilu wystąpiły błędy.", ViewMessageType.Danger, "Błąd");
                    return View(input);
                }

                var result = await _userManager.ChangePasswordAsync(profile.User, input.Password ?? "", input.NewPassword ?? "");

                if (!result.Succeeded)
                {
                    AddViewMessage("Podczas aktualizacji profilu wystąpiły błędy.", ViewMessageType.Danger, "Błąd");
                }
            }

            return View(input);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost("/UserProfiles/ToggleRole/{Id:int}/{Role}/")]
        public async Task<IActionResult> ToggleRole(int Id, string Role)
        {
            var profile = _dbContext.UserProfiles.Include(up => up.User).SingleOrDefault(up => up.Id == Id);
            if (profile == null)
            {
                return NotFound();
            }

            var inRole = await _userManager.IsInRoleAsync(profile.User, Role);
            if (inRole)
            {
                await _userManager.RemoveFromRoleAsync(profile.User, Role);
            }
            else
            {
                await _userManager.AddToRoleAsync(profile.User, Role);
            }

            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Reset()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Reset(string Action, string Email)
        {
            var profile = await _dbContext.UserProfiles.Include(p => p.User).SingleOrDefaultAsync(p => p.User.Email == Email);


            if (profile != null)
            {
                if (Action == "activate")
                {
                    await SendConfirmationUrl(profile.User);
                    AddViewMessage("Na podany adres wysłano link aktywacyjny.", ViewMessageType.Success, "Link aktywacyjny");
                }
                else if (Action == "reset-password")
                {
                    await SendResetPasswordUrl(profile.User);
                    AddViewMessage("Na podany adres wysłano link, dzięki któremu zmienisz hasło do swojego konta.", ViewMessageType.Success, "Reset hasła");
                }
            }

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Unlock([Bind("Id")] int Id)
        {
            var profile = await _dbContext.UserProfiles.Include(p => p.User).SingleOrDefaultAsync(p => p.Id == Id);

            if (profile == null)
            {
                AddViewMessage("Nie znaleziono profilu.", ViewMessageType.Danger, "Błąd");
            }
            else
            {
                if (profile.IsLocked)
                {
                    var result = await _userManager.ResetAccessFailedCountAsync(profile.User);
                    if (result.Succeeded)
                    {

                        AddViewMessage("Pomyślnie odblokowano profil.", ViewMessageType.Success, "Sukces");
                    }
                    else
                    {
                        AddViewMessage("Nie udało się odblokować profilu.", ViewMessageType.Danger, "Błąd");

                    }
                }
                else
                {
                    AddViewMessage("Profil nie był zablokowany.", ViewMessageType.Alert, "Błąd");
                }
            }

            return RedirectToAction(nameof(Index));
        }



        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(
            [Bind("Email,Password,ConfirmPassword,FirstName,LastName")]
            UserProfile.RegisterInput input)
        {

            if (input.Password.IsNullOrEmpty())
            {
                ModelState.AddModelError(nameof(input.Password), "Hasło nie może być puste");
                return View(input);

            }

            var user = new IdentityUser();
            await _userStore.SetUserNameAsync(user, input.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, input.Email, CancellationToken.None);
            var result = await _userManager.CreateAsync(user, input.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Adopter");
                var profile = new UserProfile
                {
                    FirstName = input.FirstName,
                    LastName = input.LastName,
                    UserId = await _userManager.GetUserIdAsync(user)
                };
                await _dbContext.AddAsync(profile);
                await _dbContext.SaveChangesAsync();
                await SendConfirmationUrl(user);
                AddViewMessage("Rejestracja przebiegła pomyślnie, na podany adres e-mail została wysłana wiadomość z linkiem aktywacyjnym.", ViewMessageType.Success, "Zarejestrowano");
                return RedirectToAction("Index", "Home");
            }

            foreach (var err in result.Errors)
            {
                ModelState.AddModelError(string.Empty, err.Description);
            }

            return View(input);
        }


        [HttpPost]
        public async Task<IActionResult> ConfirmEmail([Bind("Id")] int Id)
        {
            var profile = await _dbContext.UserProfiles.Include(p => p.User).SingleOrDefaultAsync(p => p.Id == Id);
            if (profile == null)
            {
                AddViewMessage("Brak użytkownika o podanym Id.", ViewMessageType.Danger, "Błąd");
            }
            else
            {
                profile.User.EmailConfirmed = true;
                AddViewMessage($"Potwierdzono adres e-mail: {profile.User.Email}.", ViewMessageType.Success, "Sukces");

                await _dbContext.SaveChangesAsync();

            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            ViewBag.MessageType = ViewMessageType.Danger;
            ViewBag.Message = $"Coś poszło nie tak.";

            if (user == null)
            {
                return View();
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                ViewBag.MessageType = ViewMessageType.Success;
                ViewBag.Message = "Dziękujemy za potwierdzenie adresu. Teraz możesz się zalogować." + String.Join(',', result.Errors);
            }

            return View();
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int Id)
        {
            var profile = await _dbContext.UserProfiles.Include(p => p.User).Where(p => p.Id == Id).Select(p => new ProfileWithRoles
            {
                Profile = p,
                Roles = (from ur in _dbContext.UserRoles
                         join role in _dbContext.Roles on ur.RoleId equals role.Id
                         where ur.UserId == p.UserId
                         select role.Name).ToList()
            }).SingleOrDefaultAsync();
            return View(profile);
        }


        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteProfile([Bind("Id")] int Id)
        {

            var profile = await _dbContext.UserProfiles.Include(p => p.User).SingleOrDefaultAsync(p => p.Id == Id);
            var user = profile?.User;
            if (profile == null || user == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var userString = $"{profile.FirstName}{profile.LastName} ({profile.User.Email})";

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                AddViewMessage(userString, ViewMessageType.Success, "Usunięto użytkownika");
            }
            else
            {
                AddViewMessage(String.Join(',', result.Errors), ViewMessageType.Danger, "Nie udało się usunąć użytkownika");
            }

            return RedirectToAction(nameof(Index));

        }


        [HttpGet]
        public IActionResult ResetPassword(string userId, string code)
        {
            var input = new ResetPasswordInputModel
            {
                UserId = userId,
                Code = code,
                Password = "",
                ConfirmPassword = ""
            };
            return View(input);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword([Bind] ResetPasswordInputModel input)
        {

            var user = await _userManager.FindByIdAsync(input.UserId);
            AddViewMessage("Coś poszło nie tak.", ViewMessageType.Danger, "Resetowanie hasła");

            if (user == null)
            {
                return View();
            }

            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(input.Code));

            var result = await _userManager.ResetPasswordAsync(user, code, input.Password);

            if (result.Succeeded)
            {
                result = await _userManager.ResetAccessFailedCountAsync(user);
                if (result.Succeeded)
                {
                    AddViewMessage("Hasło zostało zresetowane. Teraz możesz się zalogować.", ViewMessageType.Success, "Resetowanie hasła");
                    return RedirectToAction(nameof(Login));
                }
            }

            return View(input);
        }

        private async Task SendResetPasswordUrl(IdentityUser user)
        {

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var resetPasswordUrl = Url.ActionLink("ResetPassword", "UserProfiles", new { userId, code }, protocol: Request.Scheme);
            if (user.Email != null)
            {
                await _sender.SendEmailAsync(user.Email,
                    "System zarządzania schroniskiem - Rejestracja",

                    $@"
					<html><head><style>
                        p {{
                            font-size: 1.1rem;
                            text-align: center;
                            margin: auto;
                        }}

                        a {{
                            display: block;
                            background-color: #ffc107;
                            color: black !important;
							padding: 5px 20px;
                            font-weight: bold;
                            border-radius: 10px;
                            text-decoration: none;
                            width: max-content;
                            font-size: 1.3rem;
                            margin: auto;
                            margin-top: 30px;
                        }}
                    </style></head><body>
                        <p>Kliknij w poniższy link, aby zresetować hasło w serwisie <b>System zarządzania schroniskiem</b>:</p>
                        <a href=""{resetPasswordUrl}"">Resetuj Hasło</a>
                    </body></html>
                "
                        );
            }
        }

        private async Task<IActionResult> SendConfirmationUrl(IdentityUser user)
        {
            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var confirmationUrl = Url.ActionLink("ConfirmEmail", "UserProfiles", new { userId, code }, protocol: Request.Scheme);

            if (user.Email == null)
            {
                return NotFound("Brak użytkownika o podanym adresie.");
            }


            await _sender.SendEmailAsync(user.Email,
                "System zarządzania schroniskiem - Rejestracja",

                $@"
					<html><head><style>
                        p {{
                            font-size: 1.1rem;
                            text-align: center;
                            margin: auto;
                        }}

                        a {{
                            display: block;
                            background-color: #ffc107;
                            color: black !important;
							padding: 5px 20px;
                            font-weight: bold;
                            border-radius: 10px;
                            text-decoration: none;
                            width: max-content;
                            font-size: 1.3rem;
                            margin: auto;
                            margin-top: 30px;
                        }}
                    </style></head><body>
                        <p>Kliknij w poniższy link, aby aktywować konto w serwisie <b>System zarządzania schroniskiem</b>:</p>
                        <a href=""{confirmationUrl}"">Aktywuj Konto</a>
                    </body></html>
                "
                    );


            return View();
        }
    }
}
