using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Telemetry.Api.Application.DTOs;
using Telemetry.Api.Infrastructure.Persistence;

namespace Telemetry.Api.UnitTests.Infrastructure.Persistence;

public class ApplicationDbContextTests
{
    [Test]
    public async Task AddMcpTelemetryAsync_ShouldBindOnlySqlParameters_AndForwardCancellationToken()
    {
        var commandInterceptor = new CaptureCommandInterceptor();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=localhost;Database=TelemetryTest;Integrated Security=true;TrustServerCertificate=true")
            .AddInterceptors(new SuppressConnectionInterceptor(), commandInterceptor)
            .Options;
        await using var context = new ApplicationDbContext(options);
        using var cancellationSource = new CancellationTokenSource();
        var dto = new McpTelemetryDto
        {
            SessionId = Guid.NewGuid().ToString(),
            Timestamp = DateTimeOffset.UtcNow,
            Username = "test-user",
            Hostname = "test-host",
            McpServer = "test-server",
            Transport = "stdio",
            ToolName = "test-tool",
            ExecTimestamp = DateTimeOffset.UtcNow,
            Success = true
        };

        await context.AddMcpTelemetryAsync(dto, cancellationSource.Token);

        await Assert.That(commandInterceptor.ParameterCount).IsEqualTo(24);
        await Assert.That(commandInterceptor.SessionId).IsEqualTo(dto.SessionId);
        await Assert.That(commandInterceptor.CancellationToken).IsEqualTo(cancellationSource.Token);
    }

    private sealed class SuppressConnectionInterceptor : DbConnectionInterceptor
    {
        public override ValueTask<InterceptionResult> ConnectionOpeningAsync(
            DbConnection connection, ConnectionEventData eventData, InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(InterceptionResult.Suppress());
        }
    }

    private sealed class CaptureCommandInterceptor : DbCommandInterceptor
    {
        public int ParameterCount { get; private set; }
        public string? SessionId { get; private set; }
        public CancellationToken CancellationToken { get; private set; }

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            ParameterCount = command.Parameters.Count;
            SessionId = command.Parameters["@sessionId"].Value as string;
            CancellationToken = cancellationToken;
            return ValueTask.FromResult(InterceptionResult<int>.SuppressWithResult(1));
        }
    }
}