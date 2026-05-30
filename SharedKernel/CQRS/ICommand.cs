using MediatR;
using SharedKernel.Common;
namespace SharedKernel.CQRS
{
    public interface ICommand : IRequest<Result> { }

    public interface ICommand<TResponse> : IRequest<Result<TResponse>> { }
}
