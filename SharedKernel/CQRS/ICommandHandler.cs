using MediatR;
using SharedKernel.Common;
namespace SharedKernel.CQRS
{
    public interface ICommandHandler<TCommand>
     : IRequestHandler<TCommand, Result>
     where TCommand : ICommand
    { }

    public interface ICommandHandler<TCommand, TResponse>
        : IRequestHandler<TCommand, Result<TResponse>>
        where TCommand : ICommand<TResponse>
    { }

    public interface IQueryHandler<TQuery, TResponse>
        : IRequestHandler<TQuery, Result<TResponse>>
        where TQuery : IQuery<TResponse>
    { }
}
