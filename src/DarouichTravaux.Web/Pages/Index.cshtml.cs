using DarouichTravaux.Application; using Microsoft.AspNetCore.Mvc.RazorPages;
namespace DarouichTravaux.Web.Pages; public sealed class IndexModel(IDashboardService dashboard) : PageModel { public DashboardDto Data { get; private set; } = new(0,0,0,0,0,0,0); public async Task OnGet(CancellationToken ct) => Data=await dashboard.GetAsync(ct); }
