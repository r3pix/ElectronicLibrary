using ElectronicLibrary.Application.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ElectronicLibrary.Application.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        var assembly = typeof(ServiceCollectionExtension).Assembly;

        // An empty string (vs. no value at all) is treated as an invalid key and fails validation
        // outright instead of just logging the missing-key warning — only set it when non-empty.
        var mediatRLicenseKey = configuration["Licensing:MediatRCommunityKey"];
        var autoMapperLicenseKey = configuration["Licensing:AutoMapperCommunityKey"];

        services.AddMediatR(cfg =>
        {
            if (!string.IsNullOrWhiteSpace(mediatRLicenseKey))
                cfg.LicenseKey = mediatRLicenseKey;
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddAutoMapper(cfg =>
        {
            if (!string.IsNullOrWhiteSpace(autoMapperLicenseKey))
                cfg.LicenseKey = autoMapperLicenseKey;
        }, assembly);

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
