using FurnitureEPR.Infrastructure.Persistence;
using FurnitureEPR.Infrastructure.Persistence.Repositories;
using FurnitureEPR.Model.Workflow;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FurnitureEPR.Model.Tests;

public sealed class WorkflowReadRepositoryIntegrationTests
{
    [Fact]
    public async Task List_and_detail_queries_return_saved_workflow_versions_stages_and_transitions()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options;
        await using var db = new ApplicationDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var definition = new WorkflowDefinition("فرآیند مبلمان", "FURNITURE");
        var version = definition.CreateVersion(1);
        var first = new WorkflowStage(version.Id, "نجاری", "CARPENTRY", 1, true);
        var second = new WorkflowStage(version.Id, "رویه‌کوبی", "UPHOLSTERY", 2);
        version.AddStage(first);
        version.AddStage(second);
        version.AddTransition(new WorkflowTransition(version.Id, first.Id, second.Id, "ارسال به رویه‌کوبی"));
        db.WorkflowDefinitions.Add(definition);
        await db.SaveChangesAsync();

        var repository = new WorkflowRepository(db);
        var page = await repository.GetPagedAsync(1, 20, "FURN", CancellationToken.None);
        var detail = await repository.GetByIdAsync(definition.Id, CancellationToken.None);

        var listed = Assert.Single(page.Items);
        Assert.Equal(definition.Id, listed.Id);
        Assert.Equal(1, listed.VersionCount);
        Assert.NotNull(detail);
        var savedVersion = Assert.Single(detail!.Versions);
        Assert.Equal(1, savedVersion.VersionNumber);
        Assert.Equal(2, savedVersion.Stages.Count);
        Assert.Single(savedVersion.Transitions);
    }
}
