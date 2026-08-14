using CommunityToolkit.Mvvm.ComponentModel; using DarouichTravaux.Application;
namespace DarouichTravaux.Maui.ViewModels; public partial class DashboardViewModel(IDashboardService service):ObservableObject { [ObservableProperty]private DashboardDto data=new(0,0,0,0,0,0,0);public async Task LoadAsync()=>Data=await service.GetAsync(); }
