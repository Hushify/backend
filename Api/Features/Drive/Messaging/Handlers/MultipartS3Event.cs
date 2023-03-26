using Hushify.Api.Features.Drive.Messaging.Events;
using Hushify.Api.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Hushify.Api.Features.Drive.Messaging.Handlers;

public sealed class MultipartS3Event : IConsumer<S3EventMessage>
{
    private readonly AppDbContext _ctx;

    public MultipartS3Event(AppDbContext ctx) => _ctx = ctx;

    public async Task Consume(ConsumeContext<S3EventMessage> context)
    {
        foreach (var record in context.Message.Records)
        {
            var id = record.S3.Object.Key.Split("/").LastOrDefault();
            if (id is null || !Guid.TryParse(id, out var fileId))
            {
                continue;
            }

            var file =
                await _ctx.Files.FirstOrDefaultAsync(f => f.Id == fileId,
                    context.CancellationToken);
            if (file is null)
            {
                continue;
            }

            file.EncryptedSize = record.S3.Object.Size;
            await _ctx.SaveChangesAsync(context.CancellationToken);
        }
    }
}