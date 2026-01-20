using MassTransit;
using MedicalCatalogService.Data;
using Medicare.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;

namespace MedicalCatalogService.Messaging.Consumers;

public class AtcConsumer : IConsumer<IGetAtc>
{
    private readonly MedicalCatalogDbContext _db;

    public AtcConsumer(MedicalCatalogDbContext db)
    {
        _db = db;
    }

    public async Task Consume(ConsumeContext<IGetAtc> context)
    {
        var query = context.Message.Query;
        var entries = await _db.Atc
            .Where(x => x.AtcCode.Contains(query) || x.AtcName.Contains(query))
            .Take(50)
            .ToListAsync();

        await context.RespondAsync<IAtcResponse>(new
        {
            Items = entries.Select(x => new
            {
                x.AtcCode,
                x.AtcName
            }).ToList()
        });
    }
}
