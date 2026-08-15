using DarouichTravaux.Application; using DarouichTravaux.Maui.Views;
namespace DarouichTravaux.Maui;
public partial class App : Microsoft.Maui.Controls.Application
{
 private readonly LoginPage _login; private readonly IDatabaseInitializer _database;
 public App(LoginPage login,IDatabaseInitializer database){InitializeComponent();_login=login;_database=database;}
 protected override Window CreateWindow(IActivationState? activationState){_database.InitializeAsync().GetAwaiter().GetResult();return new Window(new NavigationPage(_login)){Title="Darouich Travaux",MinimumWidth=1050,MinimumHeight=700};}
}
