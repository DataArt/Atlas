namespace DataArt.Atlas.Samples.Core3Samples
{
    public class AtlasOptions
    {
        public bool UseSwagger { get; set; }

        public bool RedirectToSwaggerUI { get; set; }

        public bool UseMemoryCache { get; set; }

        public bool UseServicesAutoRegistration { get; internal set; }
    }
}