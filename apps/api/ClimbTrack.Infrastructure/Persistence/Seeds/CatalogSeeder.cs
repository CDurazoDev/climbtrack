using ClimbTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClimbTrack.Infrastructure.Persistence.Seeds;

public static class CatalogSeeder
{
    public static async Task SeedAsync(ClimbTrackDbContext context)
    {
        if (await context.SessionTypes.AnyAsync())
        {
            return;
        }

        var alactico = new EnergySystem("alactico", "Aláctico", "0-10s");
        var lactico = new EnergySystem("lactico", "Láctico", "10-120s");
        var aerobico = new EnergySystem("aerobico", "Aeróbico", ">2min");
        context.EnergySystems.AddRange(alactico, lactico, aerobico);
        await context.SaveChangesAsync();

        context.DifficultyLevels.AddRange(
            new DifficultyLevel("novato", "Novato", 3),
            new DifficultyLevel("intermedio", "Intermedio", 4),
            new DifficultyLevel("avanzado", "Avanzado", 5));

        context.TrainingTypes.AddRange(
            new TrainingType("periodizacion", "Periodización"),
            new TrainingType("microciclo", "Microciclo"));

        context.Phases.AddRange(
            new Phase("base", "Base", 1, "Semanas 1-8 | Color #4FC3F7"),
            new Phase("fuerza", "Fuerza", 2, "Semanas 9-16 | Color #FF6B35"),
            new Phase("potencia", "Potencia", 3, "Semanas 17-22 | Color #FF4444"),
            new Phase("resistencia", "Resistencia", 4, "Semanas 23-28 | Color #66BB6A"),
            new Phase("performance", "Performance", 5, "Semanas 29-36 | Color #E8FF47"));

        var sessionTypes = new[]
        {
            new SessionType("arc", "ARC", "#4FC3F7", 1, aerobico.Id, "Aeróbico"),
            new SessionType("hangboard", "Hangboard", "#FF6B35", 4, alactico.Id, "Aláctico"),
            new SessionType("campus_hang", "Campus Hang", "#FF4444", 4, alactico.Id, "Aláctico"),
            new SessionType("campus_limit", "Campus Limit", "#FF4444", 3, alactico.Id, "Aláctico"),
            new SessionType("limit", "Limit", "#E8FF47", 3, alactico.Id, "Aláctico"),
            new SessionType("boulder", "Boulder", "#E8FF47", 3, alactico.Id, "Aláctico"),
            new SessionType("linked", "Linked", "#66BB6A", 2, lactico.Id, "Láctico"),
            new SessionType("outdoor", "Outdoor", "#66BB6A", 3, aerobico.Id, "Mixto"),
            new SessionType("rest", "Rest", "#5C5C5C", 0, aerobico.Id, "Aeróbico")
        };

        context.SessionTypes.AddRange(sessionTypes);
        await context.SaveChangesAsync();

        var blocks = new List<SessionBlock>();
        foreach (var sessionType in sessionTypes)
        {
            blocks.Add(new SessionBlock(sessionType.Id, "warmup", 1));
            blocks.Add(new SessionBlock(sessionType.Id, "main", 2));
            blocks.Add(new SessionBlock(sessionType.Id, "cooldown", 3));
        }

        context.SessionBlocks.AddRange(blocks);
        await context.SaveChangesAsync();

        var items = new List<SessionBlockItem>();
        foreach (var block in blocks)
        {
            items.Add(new SessionBlockItem(block.Id, $"Bloque {block.Name}", 1));
        }

        context.SessionBlockItems.AddRange(items);
        await context.SaveChangesAsync();
    }
}
