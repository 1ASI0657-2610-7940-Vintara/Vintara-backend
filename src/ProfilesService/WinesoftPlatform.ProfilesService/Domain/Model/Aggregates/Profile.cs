using WinesoftPlatform.API.Profiles.Domain.Model.Commands;
using WinesoftPlatform.API.Profiles.Domain.Model.ValueObjects;

namespace WinesoftPlatform.API.Profiles.Domain.Model.Aggregates;

/// <summary>
///  Profile Aggregate Root
/// </summary>
/// <remarks>
///  This class represents the Profile aggregate root.
///  It contains the properties and methods to manage the profile information.
/// </remarks>
public partial class Profile
{
    public Profile()
    {
        Name = new CompanyName();
        Address = new FiscalAddress();
        Phone = new ContactPhone();
        LegalId = new TaxIdentity();
    }

    public Profile(string businessName, string branch, string address, string phone, string legalId)
    {
        Name = new CompanyName(businessName, branch);
        Address = new FiscalAddress(address);
        Phone = new ContactPhone(phone);
        LegalId = new TaxIdentity(legalId);
    }

    public Profile(CreateProfileCommand command)
    {
        Name = new CompanyName(command.BusinessName, command.Branch);
        Address = new FiscalAddress(command.Street, command.Number, command.City, command.PostalCode, command.Country);
        Phone = new ContactPhone(command.Phone);
        LegalId = new TaxIdentity(command.LegalId);
    }

    public int Id { get; }
    public CompanyName Name { get; }
    public FiscalAddress Address { get; }
    public ContactPhone Phone { get; }
    public TaxIdentity LegalId { get; }

    public string FullName => Name.FullName;
    public string FiscalAddress => Address.FullAddress;
    public string ContactPhone => Phone.Number;
    public string TaxIdentity => LegalId.Number;
}