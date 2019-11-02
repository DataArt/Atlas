using System.Threading;
using System.Threading.Tasks;

namespace DataArt.Atlas.Client
{
    public interface IPostRequest : IRequest<IPostRequest>
    {
        IPostRequest AddBody(object obj);

        Task PostAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<TResponse> PostAsync<TResponse>(CancellationToken cancellationToken = default(CancellationToken));
    }
}