using App.Common;
using App.Data.Contracts;
using App.Data.UnitOfWork;
using App.Services;
using App.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;


namespace App.IocConfig
{
    public static class AddCustomServicesExtensions
    {
        public static IServiceCollection AddCustomServices(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork,UnitOfWork>();
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddTransient<IViewRenderService, RenderViewToString>();
            return services;
        }
    }
}
