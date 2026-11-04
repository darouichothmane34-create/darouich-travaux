using DarouichTravaux.Domain;

namespace DarouichTravaux.Maui.Views;

/// <summary>Formulaire de création et de modification des fiches clients.</summary>
public sealed class ClientEditorPage : ContentPage
{
    private readonly Client _client;
    private readonly Func<Client, Task> _save;
    private readonly Dictionary<string, Entry> _fields = new();
    private readonly Editor _notes = new() { AutoSize = EditorAutoSizeOption.TextChanges, HeightRequest = 85 };
    private readonly Button _saveButton = new() { Text = "Enregistrer", HorizontalOptions = LayoutOptions.Fill };

    public ClientEditorPage(Client? original, Func<Client, Task> save)
    {
        _client = original is null ? new Client { Name = "" } : Copy(original);
        _save = save;
        Title = original is null ? "Nouveau client" : "Modifier le client";
        BackgroundColor = Color.FromArgb("#F5F7F8");

        var form = new VerticalStackLayout { Padding = new Thickness(24), Spacing = 10 };
        form.Children.Add(new Label
        {
            Text = Title, FontSize = 27, FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#18372E")
        });
        AddField(form, "Nom du client *", "Name", _client.Name);
        AddField(form, "Entreprise", "CompanyName", _client.CompanyName);
        AddField(form, "Téléphone", "Phone", _client.Phone, Keyboard.Telephone);
        AddField(form, "E-mail", "Email", _client.Email, Keyboard.Email);
        AddField(form, "Adresse", "Address", _client.Address);
        AddField(form, "Ville", "City", _client.City);
        AddField(form, "ICE", "Ice", _client.Ice);
        AddField(form, "Identifiant fiscal", "TaxId", _client.TaxId);
        AddField(form, "Registre de commerce (RC)", "Rc", _client.Rc);
        AddField(form, "CNIE", "Cnie", _client.Cnie);
        form.Children.Add(new Label { Text = "Notes", FontAttributes = FontAttributes.Bold });
        _notes.Text = _client.Notes;
        form.Children.Add(_notes);
        _saveButton.Clicked += OnSaveClicked;
        form.Children.Add(_saveButton);
        var cancel = new Button { Text = "Annuler", BackgroundColor = Color.FromArgb("#647970") };
        cancel.Clicked += async (_, _) => await Navigation.PopModalAsync();
        form.Children.Add(cancel);
        Content = new ScrollView { Content = form, MaximumWidthRequest = 750, HorizontalOptions = LayoutOptions.Center };
    }

    private void AddField(VerticalStackLayout form, string title, string key, string? value, Keyboard? keyboard = null)
    {
        form.Children.Add(new Label { Text = title, FontAttributes = FontAttributes.Bold });
        var entry = new Entry { Text = value ?? "", Placeholder = title, BackgroundColor = Colors.White };
        if (keyboard is not null) entry.Keyboard = keyboard;
        _fields.Add(key, entry);
        form.Children.Add(entry);
    }

    private static Client Copy(Client c) => new()
    {
        Id = c.Id, CreatedAtUtc = c.CreatedAtUtc, UpdatedAtUtc = c.UpdatedAtUtc,
        Name = c.Name, CompanyName = c.CompanyName, Phone = c.Phone, Email = c.Email,
        Address = c.Address, City = c.City, Ice = c.Ice, TaxId = c.TaxId,
        Rc = c.Rc, Cnie = c.Cnie, Notes = c.Notes
    };

    private string? Optional(string key)
    {
        var value = _fields[key].Text?.Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (!_saveButton.IsEnabled) return;
        _saveButton.IsEnabled = false;
        try
        {
            _client.Name = Optional("Name") ?? "";
            _client.CompanyName = Optional("CompanyName");
            _client.Phone = Optional("Phone");
            _client.Email = Optional("Email");
            _client.Address = Optional("Address");
            _client.City = Optional("City");
            _client.Ice = Optional("Ice");
            _client.TaxId = Optional("TaxId");
            _client.Rc = Optional("Rc");
            _client.Cnie = Optional("Cnie");
            _client.Notes = string.IsNullOrWhiteSpace(_notes.Text) ? null : _notes.Text.Trim();
            await _save(_client);
            await Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Enregistrement impossible", ex.Message, "OK");
        }
        finally { _saveButton.IsEnabled = true; }
    }
}
