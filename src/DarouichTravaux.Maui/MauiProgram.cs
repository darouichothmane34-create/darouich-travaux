using DarouichTravaux.Application; using DarouichTravaux.Infrastructure.Database; using DarouichTravaux.Infrastructure.Services; using DarouichTravaux.Maui.Services; using DarouichTravaux.Maui.ViewModels; using DarouichTravaux.Maui.Views; using Microsoft.Extensions.Logging;
namespace DarouichTravaux.Maui;
public static class MauiProgram
{
 public static MauiApp CreateMauiApp(){SQLitePCL.Batteries_V2.Init();var builder=MauiApp.CreateBuilder();builder.UseMauiApp<App>();
 var data=Path.Combine(FileSystem.AppDataDirectory,"darouich-travaux.db");var backups=Path.Combine(FileSystem.AppDataDirectory,"Sauvegardes");builder.Services.AddSingleton(new SqliteDatabase(data));builder.Services.AddSingleton<IDatabaseInitializer>(p=>p.GetRequiredService<SqliteDatabase>());builder.Services.AddSingleton<AuthenticationService>();builder.Services.AddSingleton<IAuthenticationService>(p=>p.GetRequiredService<AuthenticationService>());builder.Services.AddSingleton<IManagerSetupService>(p=>p.GetRequiredService<AuthenticationService>());builder.Services.AddSingleton<IBackupService>(p=>new BackupService(p.GetRequiredService<SqliteDatabase>(),backups));builder.Services.AddSingleton<IClientService,ClientService>();builder.Services.AddSingleton<IDashboardService,DashboardService>();builder.Services.AddSingleton<IInvoiceFileNameService,InvoiceFileNameService>();builder.Services.AddSingleton<INavigationService,NavigationService>();builder.Services.AddTransient<LoginViewModel>();builder.Services.AddTransient<DashboardViewModel>();builder.Services.AddTransient<ClientsViewModel>();builder.Services.AddTransient<SettingsViewModel>();builder.Services.AddTransient<LoginPage>();builder.Services.AddTransient<DashboardPage>();builder.Services.AddTransient<ClientsPage>();builder.Services.AddTransient<SettingsPage>();builder.Services.AddSingleton<AppShell>();
 #if DEBUG
 builder.Logging.AddDebug();
 #endif
 return builder.Build();}
}
