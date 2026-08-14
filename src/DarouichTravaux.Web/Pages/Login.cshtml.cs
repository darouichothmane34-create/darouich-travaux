using System.ComponentModel.DataAnnotations; using DarouichTravaux.Infrastructure; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Identity; using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Mvc.RazorPages;
namespace DarouichTravaux.Web.Pages;
[AllowAnonymous] public sealed class LoginModel(SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> users) : PageModel
{
 [BindProperty,Required,EmailAddress] public string Email { get;set; }=""; [BindProperty,Required,DataType(DataType.Password)] public string Password { get;set; }="";
 public async Task<IActionResult> OnPostAsync(string? returnUrl=null) { if(!ModelState.IsValid)return Page(); var user=await users.FindByEmailAsync(Email); if(user is null){ModelState.AddModelError("","Identifiants invalides.");return Page();} var result=await signIn.PasswordSignInAsync(user,Password,true,true); Password=""; if(!result.Succeeded){ModelState.AddModelError("",result.IsLockedOut?"Compte temporairement verrouillé.":"Identifiants invalides.");return Page();} return LocalRedirect(user.MustChangePassword?"/changer-mot-de-passe":Url.IsLocalUrl(returnUrl)?returnUrl!:"/"); }
}
