using System.Threading;
using System.Threading.Tasks;

namespace DataArt.Atlas.Client
{
    public interface IGetRequest : IRequest<IGetRequest>
    {
        Task GetAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<TResponse> GetAsync<TResponse>(CancellationToken cancellationToken = default(CancellationToken));
    }
}