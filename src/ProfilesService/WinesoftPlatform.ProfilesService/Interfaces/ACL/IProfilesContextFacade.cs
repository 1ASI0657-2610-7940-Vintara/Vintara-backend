namespace WinesoftPlatform.API.Profiles.Interfaces.ACL;

public interface IProfilesContextFacade
{
    Task<int> CreateProfile(
        string businessName,
        string branch,
        string street, 
        string number,
        string city,
        string postalCode,
        string country,
        string phone,
        string legalId);
    
    Task<int> FetchProfileByTaxIdentity(string legalId);
}