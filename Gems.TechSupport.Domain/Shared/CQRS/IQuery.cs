using MediatR;

namespace Gems.TechSupport.Domain.Shared.CQRS;

public interface IQuery<TResponse> : IRequest<TResponse>
{
}
