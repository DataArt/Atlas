using System.Threading;
using System.Threading.Tasks;

namespace DataArt.Atlas.Client
{
    public interface IPutRequest : IRequest<IPutRequest>
    {
        IPutRequest AddBody(object obj);

        Task PutAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<TResponse> PutAsync<TResponse>(CancellationToken cancellationToken = default(CancellationToken));
    }
}