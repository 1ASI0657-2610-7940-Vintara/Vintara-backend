using WinesoftPlatform.API.Profiles.Application.ACL;
using WinesoftPlatform.API.Profiles.Application.Internal.CommandServices;
using WinesoftPlatform.API.Profiles.Application.Internal.QueryServices;
using WinesoftPlatform.API.Profiles.Domain.Repositories;
using WinesoftPlatform.API.Profiles.Domain.Services;
using WinesoftPlatform.API.Profiles.Infrastructure.Persistence.EFC.Repositories;
using WinesoftPlatform.API.Profiles.Interfaces.ACL;

namespace WinesoftPlatform.API.Profiles.Infrastructure.Interfaces.ASP.Configuration.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void AddProfilesContextServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
        builder.Services.AddScoped<IProfileCommandService, ProfileCommandService>();
        builder.Services.AddScoped<IProfileQueryService, ProfileQueryService>();
        builder.Services.AddScoped<IProfilesContextFacade, ProfilesContextFacade>();
    }
}