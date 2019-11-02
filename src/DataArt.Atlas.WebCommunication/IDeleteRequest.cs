using System.Threading;
using System.Threading.Tasks;

namespace DataArt.Atlas.Client
{
    public interface IDeleteRequest : IRequest<IDeleteRequest>
    {
        Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<TResponse> DeleteAsync<TResponse>(CancellationToken cancellationToken = default(CancellationToken));
    }
}