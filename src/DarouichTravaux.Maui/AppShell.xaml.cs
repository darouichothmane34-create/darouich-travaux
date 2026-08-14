namespace DarouichTravaux.Maui; public partial class AppShell:Shell { public AppShell(){InitializeComponent();} }
cat > src/DarouichTravaux.Maui/Services/NavigationService.cs <<'EOF'
namespace DarouichTravaux.Maui.Services;
public interface INavigationService { void ShowApplication(); void ShowLogin(); }
public sealed class NavigationService(IServiceProvider services):INavigationService { public void ShowApplication(){if(Application.Current?.Windows.FirstOrDefault() is { } w)w.Page=services.GetRequiredService<AppShell>();} public void ShowLogin(){if(Application.Current?.Windows.FirstOrDefault() is { } w)w.Page=new NavigationPage(services.GetRequiredService<Views.LoginPage>());} }
