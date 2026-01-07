using ArxFlow.Server.Utils;

namespace ArxFlow.Server.Services;

public abstract class BondCalculatorHelper
{
    protected const int DIAS_UTEIS_ANO = 252;

    protected static DateTime AjustarDataVencimento(DateTime dataVencimento)
    {
        return BrazilianCalendar.AdjustToBusinessDay(dataVencimento, BrazilianCalendarType.Settlement);
    }

    protected static int CalcularDiasUteis(DateTime dataInicio, DateTime dataFim)
    {
        return BrazilianCalendar.BusinessDaysBetween(dataInicio, dataFim, BrazilianCalendarType.Settlement);
    }

    protected static decimal CalcularFracaoAno(int diasUteis)
    {
        return diasUteis / (decimal)DIAS_UTEIS_ANO;
    }

    protected static decimal ConverterTaxaParaDecimal(decimal taxaPercentual)
    {
        return taxaPercentual / 100m;
    }

    protected static decimal CalcularFatorDesconto(decimal taxaDecimal, decimal expoente)
    {
        return (decimal)Math.Pow((double)(1 + taxaDecimal), (double)expoente);
    }

    protected static decimal CalcularValorPresente(decimal valorFuturo, decimal taxaDecimal, decimal expoente)
    {
        var fatorDesconto = CalcularFatorDesconto(taxaDecimal, expoente);
        return valorFuturo / fatorDesconto;
    }

    /// <summary>
    /// Gera datas de pagamento de cupons semestrais
    /// Cupons são calculados RETROATIVAMENTE a partir da data de vencimento
    /// As datas são sempre ajustadas para o próximo dia útil
    /// </summary>
    protected static List<DateTime> GerarDatasPagamentoCupons(DateTime dataReferencia, DateTime dataVencimento)
    {
        var datasPagamento = new List<DateTime>();

        // Começa 6 meses antes do vencimento e vai retroativamente
        var dataCupom = dataVencimento.AddMonths(-6);

        // Gera cupons até passar a data de referência
        while (dataCupom > dataReferencia)
        {
            // Ajusta para o próximo dia útil
            var dataAjustada = AjustarDataVencimento(dataCupom);
            datasPagamento.Add(dataAjustada);

            // Retrocede mais 6 meses
            dataCupom = dataCupom.AddMonths(-6);
        }

        // Inverte a lista para ficar em ordem cronológica
        datasPagamento.Reverse();

        return datasPagamento;
    }
}
