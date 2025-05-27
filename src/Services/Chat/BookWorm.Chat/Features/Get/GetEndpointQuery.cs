using BookWorm.Chat.Domain.AggregatesModel;

namespace BookWorm.Chat.Features.Get;

public sealed record GetEndpointQuery(Guid Id) : IQuery<ConversationDto>;

public sealed class GetEndpointQueryHandler(IConversationRepository repository)
    : IQueryHandler<GetEndpointQuery, ConversationDto>
{
    public async Task<ConversationDto> Handle(
        GetEndpointQuery request,
        CancellationToken cancellationToken
    )
    {
        var conversation = await repository.GetByIdAsync(request.Id, cancellationToken);

        Guard.Against.NotFound(conversation, $"Conversation with id {request.Id} not found.");

        return conversation.ToConversationDto();
    }
}
