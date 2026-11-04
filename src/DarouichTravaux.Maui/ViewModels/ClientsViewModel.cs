using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DarouichTravaux.Application;
using DarouichTravaux.Domain;

namespace DarouichTravaux.Maui.ViewModels;

public partial class ClientsViewModel(IClientService service) : ObservableObject
{
    public ObservableCollection<Client> Clients { get; } = [];
    [ObservableProperty] private string search = "";

    [RelayCommand]
    public async Task LoadAsync()
    {
        var clients = await service.SearchAsync(Search);
        Clients.Clear();
        foreach (var client in clients) Clients.Add(client);
    }

    public async Task SaveClientAsync(Client client)
    {
        if (string.IsNullOrWhiteSpace(client.Name))
            throw new ArgumentException("Le nom du client est obligatoire.");
        if (!string.IsNullOrWhiteSpace(client.Email) &&
            !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(client.Email))
            throw new ArgumentException("L'adresse e-mail est invalide.");
        await service.SaveAsync(client);
        await LoadAsync();
    }

    public async Task DeleteClientAsync(Client client)
    {
        await service.DeleteAsync(client.Id);
        await LoadAsync();
    }
}
