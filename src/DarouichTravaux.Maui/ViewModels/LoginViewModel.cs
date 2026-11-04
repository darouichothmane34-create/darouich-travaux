using CommunityToolkit.Mvvm.ComponentModel; using CommunityToolkit.Mvvm.Input; using DarouichTravaux.Application; using DarouichTravaux.Maui.Services;
namespace DarouichTravaux.Maui.ViewModels;
public partial class LoginViewModel(IAuthenticationService authentication,IManagerSetupService setup,INavigationService navigation):ObservableObject
{
 [ObservableProperty]private string email="";[ObservableProperty]private string password="";[ObservableProperty]private string error="";[ObservableProperty]private bool isBusy;[ObservableProperty]private bool needsInitialSetup;
 public async Task InitializeAsync()=>NeedsInitialSetup=!await setup.IsConfiguredAsync();
 [RelayCommand]private async Task LoginAsync(){if(IsBusy)return;try{IsBusy=true;Error="";var result=await authentication.LoginAsync(Email,Password);Password="";if(!result.Succeeded){Error=result.Error??"Connexion impossible.";return;}navigation.ShowApplication();}catch(Exception ex){Error=ex.Message;}finally{IsBusy=false;}}
 [RelayCommand]private async Task CreateManagerAsync(){try{IsBusy=true;await setup.CreateManagerAsync(Email,Password);NeedsInitialSetup=false;Error="Compte gérant créé. Connectez-vous.";Password="";}catch(Exception ex){Error=ex.Message;}finally{IsBusy=false;}}
}
