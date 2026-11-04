using DarouichTravaux.Domain;
using DarouichTravaux.Maui.ViewModels;
using Microsoft.Data.Sqlite;

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
        await LoadSafeAsync();
    }

    private async Task LoadSafeAsync()
    {
        try { await Vm.LoadAsync(); }
        catch (Exception ex) { await DisplayAlertAsync("Erreur de chargement", ex.Message, "OK"); }
    }

    private async void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        await LoadSafeAsync();
    }

    private async Task OpenEditorAsync(Client? client)
    {
        var editor = new ClientEditorPage(client, async edited => await Vm.SaveClientAsync(edited));
        await Navigation.PushModalAsync(new NavigationPage(editor));
    }

    private async void OnAddClientClicked(object sender, EventArgs e)
        => await OpenEditorAsync(null);

    private async void OnViewClientClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is not Client client) return;
        var details = string.Join(Environment.NewLine, new[]
        {
            $"Nom : {client.Name}",
            $"Entreprise : {client.CompanyName}",
            $"Téléphone : {client.Phone}",
            $"E-mail : {client.Email}",
            $"Ville : {client.City}",
            $"Adresse : {client.Address}",
            $"ICE : {client.Ice}",
            $"Identifiant fiscal : {client.TaxId}",
            $"RC : {client.Rc}",
            $"CNIE : {client.Cnie}",
            $"Notes : {client.Notes}"
        });
        await DisplayAlertAsync("Fiche client", details, "Fermer");
    }

    private async void OnEditClientClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is Client client)
            await OpenEditorAsync(client);
    }

    private async void OnDeleteClientClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is not Client client) return;
        if (!await DisplayAlertAsync("Confirmer la suppression",
            $"Supprimer définitivement « {client.Name} » ? Cette action est irréversible.",
            "Supprimer", "Annuler")) return;
        try
        {
            await Vm.DeleteClientAsync(client);
            await DisplayAlertAsync("Suppression réussie", "Le client a été supprimé.", "OK");
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            await DisplayAlertAsync("Suppression impossible",
                "Ce client est associé à un chantier ou une facture. Supprimez d'abord ses dépendances si nécessaire.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erreur", ex.Message, "OK");
        }
    }
}
