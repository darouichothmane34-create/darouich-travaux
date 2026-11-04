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

    public async Task AddClientAsync(string name, string? phone, string? city)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Le nom du client est obligatoire.");
        await service.SaveAsync(new Client
        {
            Name = name.Trim(),
            Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
            City = string.IsNullOrWhiteSpace(city) ? null : city.Trim()
        });
        await LoadAsync();
    }
}
