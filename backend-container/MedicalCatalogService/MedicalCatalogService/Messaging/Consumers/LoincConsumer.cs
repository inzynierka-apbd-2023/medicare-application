using MassTransit;
using MedicalCatalogService.Data;
using Medicare.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;

namespace MedicalCatalogService.Messaging.Consumers;

public class LoincConsumer : IConsumer<IGetLoinc>
{
    private readonly MedicalCatalogDbContext _db;

    public LoincConsumer(MedicalCatalogDbContext db)
    {
        _db = db;
    }

    public async Task Consume(ConsumeContext<IGetLoinc> context)
    {
        var query = context.Message.Query;
        var entries = await _db.Loinc
            .Where(x => x.LoincNum.Contains(query) || x.LongCommonName.Contains(query))
            .Take(50)
            .ToListAsync();

        await context.RespondAsync<ILoincResponse>(new
        {
            Items = entries.Select(x => new
            {
                x.LoincNum,
                x.LongCommonName,
                x.Component,
                x.Property,
                x.TimeAspect,
                x.System,
                x.ScaleType,
                x.MethodType,
                x.ExampleUnits
            }).ToList()
        });
    }
}
