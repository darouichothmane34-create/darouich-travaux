using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DarouichTravaux.Application;
using DarouichTravaux.Maui.Services;

namespace DarouichTravaux.Maui.ViewModels;

public partial class LoginViewModel(
    IAuthenticationService authentication,
    IManagerSetupService setup,
    IPasswordRecoveryService recovery,
    INavigationService navigation) : ObservableObject
{
    [ObservableProperty] private string email = "";
    [ObservableProperty] private string password = "";
    [ObservableProperty] private string error = "";
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private bool needsInitialSetup;
    [ObservableProperty] private bool isRecovering;
    [ObservableProperty] private string recoveryCode = "";
    [ObservableProperty] private string newPassword = "";
    [ObservableProperty] private string confirmPassword = "";

    public bool IsLoggingIn => !IsRecovering;

    partial void OnIsRecoveringChanged(bool value) => OnPropertyChanged(nameof(IsLoggingIn));

    public async Task InitializeAsync() => NeedsInitialSetup = !await setup.IsConfiguredAsync();

    [RelayCommand]
    private void ShowRecovery()
    {
        Error = "";
        IsRecovering = true;
    }

    [RelayCommand]
    private void CancelRecovery()
    {
        IsRecovering = false;
        RecoveryCode = NewPassword = ConfirmPassword = "";
        Error = "";
    }

    [RelayCommand]
    private async Task RequestRecoveryCodeAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            Error = "";
            await recovery.RequestCodeAsync(Email);
            Error = "Si l'adresse correspond au gérant, un code de vérification a été envoyé. Vérifiez vos e-mails.";
        }
        catch (Exception ex) { Error = ex.Message; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task ConfirmRecoveryAsync()
    {
        if (IsBusy) return;
        if (NewPassword != ConfirmPassword)
        {
            Error = "Les nouveaux mots de passe ne correspondent pas.";
            return;
        }
        try
        {
            IsBusy = true;
            Error = "";
            await recovery.ResetPasswordAsync(Email, RecoveryCode, NewPassword);
            CancelRecovery();
            Error = "Mot de passe réinitialisé. Connectez-vous avec le nouveau mot de passe.";
        }
        catch (Exception ex) { Error = ex.Message; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            Error = "";
            var result = await authentication.LoginAsync(Email, Password);
            Password = "";
            if (!result.Succeeded)
            {
                Error = result.Error ?? "Connexion impossible.";
                return;
            }
            navigation.ShowApplication();
        }
        catch (Exception ex) { Error = ex.Message; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task CreateManagerAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            await setup.CreateManagerAsync(Email, Password);
            NeedsInitialSetup = false;
            Error = "Compte gérant créé. Connectez-vous.";
            Password = "";
        }
        catch (Exception ex) { Error = ex.Message; }
        finally { IsBusy = false; }
    }
}
