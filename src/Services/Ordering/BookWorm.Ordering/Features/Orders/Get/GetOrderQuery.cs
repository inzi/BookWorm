using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;
using BookWorm.Ordering.Infrastructure.Helpers;
using MediatR.Pipeline;

namespace BookWorm.Ordering.Features.Orders.Get;

public sealed record GetOrderQuery(
    [property: Description("Only 'ADMIN' role can retrieve other users' data")] Guid Id
) : IQuery<OrderDetailDto>;

public sealed class GetOrderHandler(IOrderRepository repository, ClaimsPrincipal claimsPrincipal)
    : IQueryHandler<GetOrderQuery, OrderDetailDto>
{
    public async Task<OrderDetailDto> Handle(
        GetOrderQuery request,
        CancellationToken cancellationToken
    )
    {
        Order? order;
        if (claimsPrincipal.GetRoles().Contains(Authorization.Roles.Admin))
        {
            order = await repository.GetByIdAsync(request.Id, cancellationToken);
        }
        else
        {
            var buyerId = claimsPrincipal.GetClaimValue(ClaimTypes.NameIdentifier).ToBuyerId();

            order = await repository.FirstOrDefaultAsync(
                new OrderFilterSpec(request.Id, buyerId),
                cancellationToken
            );
        }

        Guard.Against.NotFound(order, $"Order with id {request.Id} not found.");

        return order.ToOrderDetailDto();
    }
}

public sealed class PostGetOrderHandler(IBookService bookService)
    : IRequestPostProcessor<GetOrderQuery, OrderDetailDto>
{
    public async Task Process(
        GetOrderQuery request,
        OrderDetailDto response,
        CancellationToken cancellationToken
    )
    {
        var items = response.Items;

        var bookTasks = items
            .Select(item => bookService.GetBookByIdAsync(item.Id.ToString(), cancellationToken))
            .ToList();

        var bookResponses = await Task.WhenAll(bookTasks);

        List<OrderItemDto> updatedItems = [];

        for (var i = 0; i < items.Count; i++)
        {
            var bookResponse = bookResponses[i];

            Guard.Against.NotFound(bookResponse, $"Book with id {items[i].Id} not found.");

            var updatedItem = items[i] with { Name = bookResponse.Name };

            updatedItems.Add(updatedItem);
        }

        _ = response with { Items = updatedItems };
    }
}
