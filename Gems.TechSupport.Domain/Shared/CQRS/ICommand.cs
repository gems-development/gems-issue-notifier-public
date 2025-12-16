using MediatR;

namespace Gems.TechSupport.Domain.Shared.CQRS;

public interface ICommand : IRequest
{
}

public interface ICommand<TResponse> : IRequest<TResponse>
{
}
