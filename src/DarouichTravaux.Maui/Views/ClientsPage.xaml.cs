using DarouichTravaux.Maui.ViewModels;

namespace DarouichTravaux.Maui.Views;

public partial class ClientsPage : ContentPage
{
    private ClientsViewModel Vm => (ClientsViewModel)BindingContext;

    public ClientsPage()
    {
        InitializeComponent();
        BindingContext = IPlatformApplication.Current!.Services.GetRequiredService<ClientsViewModel>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Vm.LoadCommand.ExecuteAsync(null);
    }

    private async void OnAddClientClicked(object sender, EventArgs e)
    {
        var name = await DisplayPromptAsync("Nouveau client", "Nom du client :", "Suivant", "Annuler");
        if (string.IsNullOrWhiteSpace(name)) return;
        var phone = await DisplayPromptAsync("Nouveau client", "Téléphone (facultatif) :", "Suivant", "Ignorer", keyboard: Keyboard.Telephone);
        var city = await DisplayPromptAsync("Nouveau client", "Ville (facultatif) :", "Enregistrer", "Ignorer");
        try
        {
            await Vm.AddClientAsync(name, phone, city);
            await DisplayAlertAsync("Client enregistré", "Le client a été ajouté.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erreur", ex.Message, "OK");
        }
    }
}
