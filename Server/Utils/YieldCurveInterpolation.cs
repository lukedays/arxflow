using ArxFlow.Server.DTOs.YieldCurve;

namespace ArxFlow.Server.Utils;

/// <summary>
/// Interpolacao de curvas de juros
/// </summary>
public static class YieldCurveInterpolation
{
    public enum InterpolationType
    {
        Linear,
        FlatForward
    }

    /// <summary>
    /// Interpola a taxa para um determinado DU usando Flat Forward 252
    /// </summary>
    /// <param name="du">Dias uteis do ponto a interpolar</param>
    /// <param name="duAnterior">DU do vertice anterior</param>
    /// <param name="duPosterior">DU do vertice posterior</param>
    /// <param name="taxaAnterior">Taxa do vertice anterior (em %)</param>
    /// <param name="taxaPosterior">Taxa do vertice posterior (em %)</param>
    /// <returns>Taxa interpolada (em %)</returns>
    public static decimal InterpolateFlatForward(
        int du,
        int duAnterior,
        int duPosterior,
        decimal taxaAnterior,
        decimal taxaPosterior
    )
    {
        if (du <= duAnterior)
            return taxaAnterior;
        if (du >= duPosterior)
            return taxaPosterior;
        if (duPosterior == duAnterior)
            return taxaAnterior;

        // Converte para double para calculos de potencia
        double iAnt = (double)taxaAnterior / 100.0;
        double iPos = (double)taxaPosterior / 100.0;
        double duAnt = duAnterior;
        double duPos = duPosterior;
        double duTarget = du;

        // Fator de capitalizacao anterior: (1 + i_anterior)^(DU_anterior/252)
        double fatorAnterior = Math.Pow(1.0 + iAnt, duAnt / 252.0);

        // Fator de capitalizacao posterior: (1 + i_posterior)^(DU_posterior/252)
        double fatorPosterior = Math.Pow(1.0 + iPos, duPos / 252.0);

        // Expoente da interpolacao: (DU - DU_anterior) / (DU_posterior - DU_anterior)
        double expoente = (duTarget - duAnt) / (duPos - duAnt);

        // Fator forward: (fatorPosterior / fatorAnterior)^expoente
        double fatorForward = Math.Pow(fatorPosterior / fatorAnterior, expoente);

        // Fator total no ponto DU
        double fatorTotal = fatorAnterior * fatorForward;

        // Taxa: (fatorTotal^(252/DU) - 1) * 100
        double taxa = (Math.Pow(fatorTotal, 252.0 / duTarget) - 1.0) * 100.0;

        return (decimal)taxa;
    }

    /// <summary>
    /// Interpola a taxa usando interpolacao linear
    /// </summary>
    public static decimal InterpolateLinear(
        int du,
        int duAnterior,
        int duPosterior,
        decimal taxaAnterior,
        decimal taxaPosterior
    )
    {
        if (du <= duAnterior)
            return taxaAnterior;
        if (du >= duPosterior)
            return taxaPosterior;
        if (duPosterior == duAnterior)
            return taxaAnterior;

        decimal t = (decimal)(du - duAnterior) / (duPosterior - duAnterior);
        return taxaAnterior + t * (taxaPosterior - taxaAnterior);
    }

    /// <summary>
    /// Interpola um ponto na curva
    /// </summary>
    public static decimal Interpolate(
        int du,
        int duAnterior,
        int duPosterior,
        decimal taxaAnterior,
        decimal taxaPosterior,
        InterpolationType type
    )
    {
        return type switch
        {
            InterpolationType.FlatForward => InterpolateFlatForward(
                du,
                duAnterior,
                duPosterior,
                taxaAnterior,
                taxaPosterior
            ),
            _ => InterpolateLinear(du, duAnterior, duPosterior, taxaAnterior, taxaPosterior),
        };
    }

    /// <summary>
    /// Gera curva interpolada com pontos intermediarios
    /// </summary>
    /// <param name="vertices">Vertices originais da curva (ordenados por DU)</param>
    /// <param name="dataReferencia">Data de referencia (pregao) para calculo de vencimentos</param>
    /// <param name="type">Tipo de interpolacao</param>
    /// <param name="minPointsPerSegment">Minimo de pontos intermediarios por segmento</param>
    /// <returns>Curva com pontos interpolados</returns>
    public static List<YieldCurvePoint> GenerateInterpolatedCurve(
        List<YieldCurvePoint> vertices,
        DateTime dataReferencia,
        InterpolationType type,
        int minPointsPerSegment = 5
    )
    {
        if (vertices.Count == 0)
            return [];
        if (vertices.Count == 1)
            return [vertices[0]];

        var sorted = vertices.OrderBy(v => v.Days).ToList();
        var result = new List<YieldCurvePoint>();

        // Adiciona primeiro vertice
        result.Add(sorted[0]);

        for (int i = 0; i < sorted.Count - 1; i++)
        {
            var anterior = sorted[i];
            var posterior = sorted[i + 1];

            int duDiff = posterior.Days - anterior.Days;

            // Calcula step dinamico para garantir pontos intermediarios
            // Se a diferenca e pequena, usa step de 1 DU
            // Caso contrario, divide para ter minPointsPerSegment pontos
            int step = Math.Max(1, duDiff / (minPointsPerSegment + 1));

            // Gera pontos intermediarios
            for (int du = anterior.Days + step; du < posterior.Days; du += step)
            {
                var taxa = Interpolate(
                    du,
                    anterior.Days,
                    posterior.Days,
                    anterior.Rate,
                    posterior.Rate,
                    type
                );

                // Calcula data de vencimento usando calendario B3
                var vencimento = BrazilianCalendar.AddBusinessDays(
                    dataReferencia,
                    du,
                    BrazilianCalendarType.Exchange
                );

                result.Add(
                    new YieldCurvePoint
                    {
                        Days = du,
                        Rate = taxa,
                        Maturity = vencimento,
                        IsInterpolated = true,
                    }
                );
            }

            // Adiciona proximo vertice
            result.Add(posterior);
        }

        return result;
    }

    /// <summary>
    /// Calcula taxa para um DU especifico dado uma curva de vertices
    /// </summary>
    public static decimal GetRateForDU(
        List<YieldCurvePoint> vertices,
        int du,
        InterpolationType type
    )
    {
        if (vertices.Count == 0)
            return 0;

        var sorted = vertices.OrderBy(v => v.Days).ToList();

        // Antes do primeiro vertice
        if (du <= sorted[0].Days)
            return sorted[0].Rate;

        // Depois do ultimo vertice
        if (du >= sorted[^1].Days)
            return sorted[^1].Rate;

        // Encontra vertices adjacentes
        for (int i = 0; i < sorted.Count - 1; i++)
        {
            if (du >= sorted[i].Days && du <= sorted[i + 1].Days)
            {
                return Interpolate(
                    du,
                    sorted[i].Days,
                    sorted[i + 1].Days,
                    sorted[i].Rate,
                    sorted[i + 1].Rate,
                    type
                );
            }
        }

        return sorted[^1].Rate;
    }
}
