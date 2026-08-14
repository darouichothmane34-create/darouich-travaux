using DarouichTravaux.Application; using DarouichTravaux.Domain; using DarouichTravaux.Infrastructure.Services;
namespace DarouichTravaux.Tests;
public sealed class CoreRulesTests
{
 [Theory][InlineData("court")][InlineData("minuscules1!")][InlineData("SANS-MINUSCULE1!")]public void Weak_password_is_rejected(string password)=>Assert.NotNull(PasswordPolicy.Validate(password));
 [Fact]public void Strong_password_is_accepted()=>Assert.Null(PasswordPolicy.Validate("Travaux#2026"));
 [Fact]public void Invoice_file_name_is_safe_and_descriptive(){var invoice=new Invoice{Number="FAC-2026-018",ClientName="Ahmed  El / Mansouri"};Assert.Equal("Facture(Ahmed_El_Mansouri)-FAC-2026-018.pdf",new InvoiceFileNameService().BuildInvoiceFileName(invoice));}
}
