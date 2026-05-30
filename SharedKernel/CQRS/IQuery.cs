using MediatR;
using SharedKernel.Common;
namespace SharedKernel.CQRS
{
    public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }
}
