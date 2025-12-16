using MediatR;

namespace Gems.TechSupport.Domain.Shared.CQRS;

public interface ICommandHandler<TCommand>
    : IRequestHandler<TCommand>
    where TCommand : ICommand
{
}

public interface ICommandHandler<TCommand, TResponse> 
    : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
    where TResponse : notnull
{
}
