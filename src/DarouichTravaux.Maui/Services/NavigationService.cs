namespace DarouichTravaux.Maui.Services;
public interface INavigationService
{
    void ShowApplication();
    void ShowLogin();
}
public sealed class NavigationService(IServiceProvider services) : INavigationService
{
    public void ShowApplication()
    {
        if (Application.Current?.Windows.FirstOrDefault() is { } window)
            window.Page = services.GetRequiredService<AppShell>();
    }
    public void ShowLogin()
    {
        if (Application.Current?.Windows.FirstOrDefault() is { } window)
            window.Page = new NavigationPage(services.GetRequiredService<Views.LoginPage>());
    }
}
