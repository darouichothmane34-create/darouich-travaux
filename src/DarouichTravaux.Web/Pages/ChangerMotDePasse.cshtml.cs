using System.ComponentModel.DataAnnotations; using DarouichTravaux.Infrastructure; using Microsoft.AspNetCore.Identity; using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Mvc.RazorPages;
namespace DarouichTravaux.Web.Pages;
public sealed class ChangePasswordModel(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn) : PageModel
{
 [BindProperty,Required,DataType(DataType.Password)] public string CurrentPassword{get;set;}=""; [BindProperty,Required,DataType(DataType.Password)] public string NewPassword{get;set;}=""; [BindProperty,Required,Compare(nameof(NewPassword)),DataType(DataType.Password)] public string Confirmation{get;set;}="";
 public async Task<IActionResult> OnPostAsync(){if(!ModelState.IsValid)return Page();var user=await users.GetUserAsync(User);if(user is null)return Challenge();var result=await users.ChangePasswordAsync(user,CurrentPassword,NewPassword);CurrentPassword=NewPassword=Confirmation="";if(!result.Succeeded){foreach(var error in result.Errors)ModelState.AddModelError("",error.Description);return Page();}user.MustChangePassword=false;await users.UpdateAsync(user);await signIn.RefreshSignInAsync(user);TempData["Success"]="Votre mot de passe a été modifié avec succès.";return RedirectToPage("/Index");}
}
