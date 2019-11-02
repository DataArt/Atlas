using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DataArt.Atlas.Samples.Core3Samples
{
    public interface IAtlasExtensionModule
    {
        void ConfigureApplication(IApplicationBuilder builder);
        void ConfigureServices(IServiceCollection services);
    }

    public interface IAtlasApplication
    {

    }

    public interface IAtlasBuilder
    {
        IAtlasApplication Build();
    }

    public class AtlasBuilder : IAtlasBuilder
    {
        private readonly IServiceCollection _services;

        public AtlasBuilder(IServiceCollection services)
        {
            this._services = services;
        }

        public IAtlasBuilder UseExtension(IAtlasExtensionModule module)
        {
            //_services.RegisterModule(module);
            return this;
        }

        public IAtlasApplication Build()
        {
            return null;
        }
    }
}