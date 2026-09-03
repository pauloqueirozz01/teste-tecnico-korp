using System.Data;
using System.Globalization;
using KorpTeste.BillingService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KorpTeste.BillingService.Infrastructure.Persistence;

public sealed class PostgreSqlNumeradorNotaFiscal(BillingDbContext context) : INumeradorNotaFiscal
{
    public async Task<long> ProximoNumeroAsync(CancellationToken cancellationToken)
    {
        var connection = context.Database.GetDbConnection();
        var fecharConexao = connection.State != ConnectionState.Open;

        if (fecharConexao)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT nextval('nota_fiscal_numero_seq')";

            var resultado = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt64(resultado, CultureInfo.InvariantCulture);
        }
        finally
        {
            if (fecharConexao)
            {
                await connection.CloseAsync();
            }
        }
    }
}
