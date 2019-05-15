using DataArt.Atlas.EntityFramework.MsSql;

namespace SampleService.DataAccess
{
    public sealed class SampleEntityRepository : GenericRepository<SampleEntityContext, SampleEntity, int>
    {
        public SampleEntityRepository(SampleEntityContext context) : base(context)
        {
        }
    }
}
