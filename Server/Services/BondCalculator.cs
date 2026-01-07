using MathNet.Numerics.RootFinding;

namespace ArxFlow.Server.Services;

/// <summary>
/// Calculadora unificada de títulos públicos federais
/// Baseada na Metodologia ANBIMA de Precificação 2023
/// </summary>
public class BondCalculator : BondCalculatorHelper
{
    private const decimal VALOR_NOMINAL = 1000m;


    // Cupons semestrais calculados
    private static readonly decimal CUPOM_SEMESTRAL_NTNB = (decimal)(Math.Pow(1.06, 0.5) - 1); // ~0.0295630140987
    private static readonly decimal CUPOM_SEMESTRAL_NTNF = (decimal)(Math.Pow(1.10, 0.5) - 1); // ~0.0488088481701516
    private static readonly decimal CUPOM_SEMESTRAL_NTNC = (decimal)(Math.Pow(1.12, 0.5) - 1); // ~0.0583005205221815

    #region LTN - Letras do Tesouro Nacional

    /// <summary>   
    /// Calcula PU da LTN a partir da taxa
    /// Fórmula: PU = VN / [(Taxa/100 + 1)^(du/252)]
    /// </summary>
    public static decimal LTN_CalcularPU(decimal taxa, DateTime dataReferencia, DateTime dataVencimento)
    {
        var dataVencimentoAjustada = AjustarDataVencimento(dataVencimento);
        var taxaTruncada = RoundingRules.Ltn.TaxaRetorno(taxa);
        var taxaDecimal = ConverterTaxaParaDecimal(taxaTruncada);
        var diasUteis = CalcularDiasUteis(dataReferencia, dataVencimentoAjustada);
        var fracaoAno = CalcularFracaoAno(diasUteis);
        var expoente = RoundingRules.Ltn.ExponencialDias(fracaoAno);
        var fatorDesconto = CalcularFatorDesconto(taxaDecimal, expoente);
        var puCalculado = VALOR_NOMINAL / fatorDesconto;
        return RoundingRules.Ltn.PrecoUnitario(puCalculado);
    }

    /// <summary>
    /// Calcula taxa da LTN a partir do PU
    /// Fórmula: Taxa = [(VN/PU)^(252/du) - 1] x 100
    /// </summary>
    public static decimal LTN_CalcularTaxa(decimal pu, DateTime dataReferencia, DateTime dataVencimento)
    {
        var puTruncado = RoundingRules.Ltn.PrecoUnitario(pu);

        double funcaoObjetivo(double taxaPercentual)
        {
            var puCalculado = LTN_CalcularPU((decimal)taxaPercentual, dataReferencia, dataVencimento);
            return (double)(puCalculado - puTruncado);
        }

        var taxaSolucao = Brent.FindRoot(funcaoObjetivo, 0.0, 50.0, accuracy: 1e-6);
        return RoundingRules.Ltn.TaxaRetorno((decimal)taxaSolucao);
    }

    #endregion

    #region NTN-F - Notas do Tesouro Nacional, Série F

    /// <summary>
    /// Calcula PU da NTN-F a partir da taxa
    /// Fórmula: PU = Σ(Fluxo_t / (1 + taxa)^(du_t/252))
    /// Cupom semestral: VN * [(1 + i)^(1/2) - 1]
    /// </summary>
    public static decimal NTNF_CalcularPU(decimal taxa, DateTime dataReferencia, DateTime dataVencimento)
    {
        var dataVencimentoAjustada = AjustarDataVencimento(dataVencimento);
        var taxaTruncada = RoundingRules.NtnF.TaxaRetorno(taxa);
        var taxaDecimal = ConverterTaxaParaDecimal(taxaTruncada);

        // Cupom semestral em valor absoluto (10% a.a.)
        var cupomSemestral = VALOR_NOMINAL * CUPOM_SEMESTRAL_NTNF;
        var cupomArredondado = RoundingRules.NtnF.JurosSemestrais(cupomSemestral);

        // Gera datas de pagamento de cupons
        var datasPagamento = GerarDatasPagamentoCupons(dataReferencia, dataVencimento);

        // Calcula valor presente dos cupons
        var vpCupons = 0m;
        foreach (var dataPagamento in datasPagamento)
        {
            var diasUteis = CalcularDiasUteis(dataReferencia, dataPagamento);
            var anos = CalcularFracaoAno(diasUteis);
            var fatorDesconto = CalcularFatorDesconto(taxaDecimal, anos);
            var vpCupom = cupomArredondado / fatorDesconto;
            vpCupom = RoundingRules.NtnF.FluxoPagamentos(vpCupom);
            vpCupons += vpCupom;
        }

        // Valor presente do principal
        var diasUteisAteVencimento = CalcularDiasUteis(dataReferencia, dataVencimentoAjustada);
        var anosAteVencimento = CalcularFracaoAno(diasUteisAteVencimento);
        var fatorDescontoVencimento = CalcularFatorDesconto(taxaDecimal, anosAteVencimento);

        // Principal + cupom na data de vencimento (se vence em data de cupom)
        var valorFuturo = VALOR_NOMINAL;
        if ((dataVencimento.Month == 1 && dataVencimento.Day == 1) ||
            (dataVencimento.Month == 7 && dataVencimento.Day == 1))
        {
            valorFuturo += cupomArredondado;
        }

        var vpPrincipal = valorFuturo / fatorDescontoVencimento;
        vpPrincipal = RoundingRules.NtnF.FluxoPagamentos(vpPrincipal);

        var pu = vpCupons + vpPrincipal;
        return RoundingRules.NtnF.PrecoUnitario(pu);
    }

    /// <summary>
    /// Calcula taxa da NTN-F a partir do PU
    /// </summary>
    public static decimal NTNF_CalcularTaxa(decimal pu, DateTime dataReferencia, DateTime dataVencimento, decimal taxaInicial = 10m)
    {
        var puTruncado = RoundingRules.NtnF.PrecoUnitario(pu);

        double funcaoObjetivo(double taxaPercentual)
        {
            var puCalculado = NTNF_CalcularPU((decimal)taxaPercentual, dataReferencia, dataVencimento);
            return (double)(puCalculado - puTruncado);
        }

        var taxaSolucao = Brent.FindRoot(funcaoObjetivo, 0.0, 50.0, accuracy: 1e-6);
        return RoundingRules.NtnF.TaxaRetorno((decimal)taxaSolucao);
    }

    #endregion

    #region LFT - Letras Financeiras do Tesouro

    /// <summary>
    /// Calcula PU da LFT a partir da taxa
    /// Fórmula: PU = VNA / [(Taxa/100 + 1)^(du/252)]
    /// Similar à LTN, mas usa VNA em vez de valor nominal fixo
    /// </summary>
    public static decimal LFT_CalcularPU(decimal taxa, decimal vna, DateTime dataReferencia, DateTime dataVencimento)
    {
        var dataVencimentoAjustada = AjustarDataVencimento(dataVencimento);
        var taxaTruncada = RoundingRules.Lft.TaxaRetorno(taxa);
        var vnaTruncado = RoundingRules.Lft.VNA(vna);
        var taxaDecimal = ConverterTaxaParaDecimal(taxaTruncada);
        var diasUteis = CalcularDiasUteis(dataReferencia, dataVencimentoAjustada);
        var fracaoAno = CalcularFracaoAno(diasUteis);
        // Não trunca o expoente antes de calcular a potência
        var fatorDesconto = CalcularFatorDesconto(taxaDecimal, fracaoAno);
        var cotacao = vnaTruncado / fatorDesconto;
        var cotacaoTruncada = RoundingRules.Lft.Cotacao(cotacao);
        return RoundingRules.Lft.PrecoUnitario(cotacaoTruncada);
    }

    /// <summary>
    /// Calcula taxa da LFT a partir do PU
    /// Fórmula: Taxa = [(VNA/PU)^(252/du) - 1] x 100
    /// </summary>
    public static decimal LFT_CalcularTaxa(decimal pu, decimal vna, DateTime dataReferencia, DateTime dataVencimento)
    {
        var puTruncado = RoundingRules.Lft.PrecoUnitario(pu);

        double funcaoObjetivo(double taxaPercentual)
        {
            var puCalculado = LFT_CalcularPU((decimal)taxaPercentual, vna, dataReferencia, dataVencimento);
            return (double)(puCalculado - puTruncado);
        }

        var taxaSolucao = Brent.FindRoot(funcaoObjetivo, 0.0, 50.0, accuracy: 1e-6);
        return RoundingRules.Lft.TaxaRetorno((decimal)taxaSolucao);
    }

    #endregion

    #region NTN-B - Notas do Tesouro Nacional, Série B

    /// <summary>
    /// Calcula PU da NTN-B a partir da taxa real
    /// Fórmula: PU = Σ(Fluxo_t / (1 + taxa)^(du_t/252))
    /// Cupom semestral: VNA * [(1 + i)^(1/2) - 1]
    /// </summary>
    public static decimal NTNB_CalcularPU(decimal taxaReal, decimal vna, DateTime dataReferencia, DateTime dataVencimento)
    {
        var dataVencimentoAjustada = AjustarDataVencimento(dataVencimento);
        var taxaTruncada = RoundingRules.NtnB.TaxaRetorno(taxaReal);
        var vnaTruncado = RoundingRules.NtnB.VNA(vna);
        var taxa = ConverterTaxaParaDecimal(taxaTruncada);

        // Cupom semestral em PERCENTUAL (base 100) - 6% a.a.
        var cupomPercentual = CUPOM_SEMESTRAL_NTNB * 100m;
        var cupomArredondado = RoundingRules.NtnB.JurosSemestrais(cupomPercentual);

        // Gera datas de pagamento de cupons
        var datasPagamento = GerarDatasPagamentoCupons(dataReferencia, dataVencimento);

        // Calcula valor presente dos cupons (em percentual)
        var vpCupons = 0m;
        foreach (var dataPagamento in datasPagamento)
        {
            var diasUteis = CalcularDiasUteis(dataReferencia, dataPagamento);
            var anos = CalcularFracaoAno(diasUteis);
            var fatorDesconto = CalcularFatorDesconto(taxa, anos);
            var vpCupom = cupomArredondado / fatorDesconto;
            vpCupom = RoundingRules.NtnB.FluxoPagamentos(vpCupom);
            vpCupons += vpCupom;
        }

        // Valor presente do principal (base 100) no vencimento
        var diasUteisAteVencimento = CalcularDiasUteis(dataReferencia, dataVencimentoAjustada);
        var anosAteVencimento = CalcularFracaoAno(diasUteisAteVencimento);
        var fatorDescontoVencimento = CalcularFatorDesconto(taxa, anosAteVencimento);

        // Valor futuro no vencimento = principal + cupom (se vence em data de cupom)
        var valorFuturo = 100m;
        if ((dataVencimento.Month == 5 && dataVencimento.Day == 15) ||
            (dataVencimento.Month == 8 && dataVencimento.Day == 15))
        {
            valorFuturo += cupomArredondado;
        }

        var vpPrincipal = valorFuturo / fatorDescontoVencimento;
        vpPrincipal = RoundingRules.NtnB.FluxoPagamentos(vpPrincipal);

        // Cotação = VP dos cupons + VP do principal (em percentual)
        var cotacao = vpCupons + vpPrincipal;
        cotacao = RoundingRules.NtnB.Cotacao(cotacao);

        // PU = Cotação × VNA / 100
        var pu = cotacao * vnaTruncado / 100m;
        return RoundingRules.NtnB.PrecoUnitario(pu);
    }

    /// <summary>
    /// Calcula taxa da NTN-B a partir do PU
    /// </summary>
    public static decimal NTNB_CalcularTaxa(decimal pu, decimal vna, DateTime dataReferencia, DateTime dataVencimento, decimal taxaInicial = 6m)
    {
        var puTruncado = RoundingRules.NtnB.PrecoUnitario(pu);

        double funcaoObjetivo(double taxaPercentual)
        {
            var puCalculado = NTNB_CalcularPU((decimal)taxaPercentual, vna, dataReferencia, dataVencimento);
            return (double)(puCalculado - puTruncado);
        }

        var taxaSolucao = Brent.FindRoot(funcaoObjetivo, -10.0, 50.0, accuracy: 1e-6);
        return RoundingRules.NtnB.TaxaRetorno((decimal)taxaSolucao);
    }

    #endregion

    #region NTN-C - Notas do Tesouro Nacional, Série C

    /// <summary>
    /// Calcula PU da NTN-C a partir da taxa real
    /// Similar à NTN-B mas com VNA atualizado pelo IGP-M
    /// </summary>
    public static decimal NTNC_CalcularPU(decimal taxaReal, decimal vna, DateTime dataReferencia, DateTime dataVencimento)
    {
        var dataVencimentoAjustada = AjustarDataVencimento(dataVencimento);
        var taxaTruncada = RoundingRules.NtnC.TaxaRetorno(taxaReal);
        var vnaTruncado = RoundingRules.NtnC.VNA(vna);
        var taxa = ConverterTaxaParaDecimal(taxaTruncada);

        // Cupom semestral em PERCENTUAL (base 100) - 12% a.a.
        var cupomPercentual = CUPOM_SEMESTRAL_NTNC * 100m;
        var cupomArredondado = RoundingRules.NtnC.JurosSemestrais(cupomPercentual);

        // Gera datas de pagamento de cupons
        var datasPagamento = GerarDatasPagamentoCupons(dataReferencia, dataVencimento);

        // Calcula valor presente dos cupons (em percentual)
        var vpCupons = 0m;
        foreach (var dataPagamento in datasPagamento)
        {
            var diasUteis = CalcularDiasUteis(dataReferencia, dataPagamento);
            var anos = CalcularFracaoAno(diasUteis);
            var fatorDesconto = CalcularFatorDesconto(taxa, anos);
            var vpCupom = cupomArredondado / fatorDesconto;
            vpCupom = RoundingRules.NtnC.FluxoPagamentos(vpCupom);
            vpCupons += vpCupom;
        }

        // Valor presente do principal (base 100) no vencimento
        var diasUteisAteVencimento = CalcularDiasUteis(dataReferencia, dataVencimentoAjustada);
        var anosAteVencimento = CalcularFracaoAno(diasUteisAteVencimento);
        var fatorDescontoVencimento = CalcularFatorDesconto(taxa, anosAteVencimento);

        // Valor futuro no vencimento = principal + cupom (se vence em data de cupom)
        var valorFuturo = 100m;
        if ((dataVencimento.Month == 1 && dataVencimento.Day == 1) ||
            (dataVencimento.Month == 7 && dataVencimento.Day == 1))
        {
            valorFuturo += cupomArredondado;
        }

        var vpPrincipal = valorFuturo / fatorDescontoVencimento;
        vpPrincipal = RoundingRules.NtnC.FluxoPagamentos(vpPrincipal);

        // Cotação = VP dos cupons + VP do principal (em percentual)
        var cotacao = vpCupons + vpPrincipal;
        cotacao = RoundingRules.NtnC.Cotacao(cotacao);

        // PU = Cotação × VNA / 100
        var pu = cotacao * vnaTruncado / 100m;
        return RoundingRules.NtnC.PrecoUnitario(pu);
    }

    /// <summary>
    /// Calcula taxa da NTN-C a partir do PU
    /// </summary>
    public static decimal NTNC_CalcularTaxa(decimal pu, decimal vna, DateTime dataReferencia, DateTime dataVencimento, decimal taxaInicial = 6m)
    {
        var puTruncado = RoundingRules.NtnC.PrecoUnitario(pu);

        double funcaoObjetivo(double taxaPercentual)
        {
            var puCalculado = NTNC_CalcularPU((decimal)taxaPercentual, vna, dataReferencia, dataVencimento);
            return (double)(puCalculado - puTruncado);
        }

        var taxaSolucao = Brent.FindRoot(funcaoObjetivo, -10.0, 50.0, accuracy: 1e-6);
        return RoundingRules.NtnC.TaxaRetorno((decimal)taxaSolucao);
    }

    #endregion

    #region Helpers para Debug - Fluxo Detalhado

    public class FluxoPagamento
    {
        public DateTime DataPagamento { get; set; }
        public int DiaUseis { get; set; }
        public decimal JurosSemestral { get; set; }
        public decimal ValorFuturo { get; set; }
        public decimal ValorPresente { get; set; }
    }

    /// <summary>
    /// Retorna o fluxo detalhado de pagamentos da NTN-B para debug
    /// </summary>
    public static List<FluxoPagamento> NTNB_ObterFluxoDetalhado(decimal taxaReal, decimal vna, DateTime dataReferencia, DateTime dataVencimento)
    {
        var fluxos = new List<FluxoPagamento>();
        var dataVencimentoAjustada = AjustarDataVencimento(dataVencimento);
        var taxaTruncada = RoundingRules.NtnB.TaxaRetorno(taxaReal);
        var taxa = ConverterTaxaParaDecimal(taxaTruncada);

        // Cupom semestral em PERCENTUAL (base 100) - 6% a.a.
        var cupomPercentual = CUPOM_SEMESTRAL_NTNB * 100m;
        var cupomArredondado = RoundingRules.NtnB.JurosSemestrais(cupomPercentual);

        // Gera datas de pagamento de cupons
        var datasPagamento = GerarDatasPagamentoCupons(dataReferencia, dataVencimento);

        // Cupons intermediários
        foreach (var dataPagamento in datasPagamento)
        {
            var diasUteis = CalcularDiasUteis(dataReferencia, dataPagamento);
            var anos = CalcularFracaoAno(diasUteis);
            var fatorDesconto = CalcularFatorDesconto(taxa, anos);
            var vpCupom = cupomArredondado / fatorDesconto;
            vpCupom = RoundingRules.NtnB.FluxoPagamentos(vpCupom);

            fluxos.Add(new FluxoPagamento
            {
                DataPagamento = dataPagamento,
                DiaUseis = diasUteis,
                JurosSemestral = cupomArredondado,
                ValorFuturo = cupomArredondado,
                ValorPresente = vpCupom
            });
        }

        // Vencimento (principal + cupom se for data de cupom)
        var diasUteisAteVencimento = CalcularDiasUteis(dataReferencia, dataVencimentoAjustada);
        var anosAteVencimento = CalcularFracaoAno(diasUteisAteVencimento);
        var fatorDescontoVencimento = CalcularFatorDesconto(taxa, anosAteVencimento);

        var valorFuturo = 100m;
        if ((dataVencimento.Month == 5 && dataVencimento.Day == 15) ||
            (dataVencimento.Month == 8 && dataVencimento.Day == 15))
        {
            valorFuturo += cupomArredondado;
        }

        var vpPrincipal = valorFuturo / fatorDescontoVencimento;
        vpPrincipal = RoundingRules.NtnB.FluxoPagamentos(vpPrincipal);

        fluxos.Add(new FluxoPagamento
        {
            DataPagamento = dataVencimentoAjustada,
            DiaUseis = diasUteisAteVencimento,
            JurosSemestral = cupomArredondado,
            ValorFuturo = valorFuturo,
            ValorPresente = vpPrincipal
        });

        return fluxos;
    }

    /// <summary>
    /// Retorna o fluxo detalhado de pagamentos da NTN-F para debug
    /// </summary>
    public static List<FluxoPagamento> NTNF_ObterFluxoDetalhado(decimal taxa, DateTime dataReferencia, DateTime dataVencimento)
    {
        var fluxos = new List<FluxoPagamento>();
        var dataVencimentoAjustada = AjustarDataVencimento(dataVencimento);
        var taxaTruncada = RoundingRules.NtnF.TaxaRetorno(taxa);
        var taxaDecimal = ConverterTaxaParaDecimal(taxaTruncada);

        // Cupom semestral em valor absoluto (10% a.a.)
        var cupomSemestral = VALOR_NOMINAL * CUPOM_SEMESTRAL_NTNF;
        var cupomArredondado = RoundingRules.NtnF.JurosSemestrais(cupomSemestral);

        // Gera datas de pagamento de cupons
        var datasPagamento = GerarDatasPagamentoCupons(dataReferencia, dataVencimento);

        // Cupons intermediários
        foreach (var dataPagamento in datasPagamento)
        {
            var diasUteis = CalcularDiasUteis(dataReferencia, dataPagamento);
            var anos = CalcularFracaoAno(diasUteis);
            var fatorDesconto = CalcularFatorDesconto(taxaDecimal, anos);
            var vpCupom = cupomArredondado / fatorDesconto;
            vpCupom = RoundingRules.NtnF.FluxoPagamentos(vpCupom);

            fluxos.Add(new FluxoPagamento
            {
                DataPagamento = dataPagamento,
                DiaUseis = diasUteis,
                JurosSemestral = cupomArredondado,
                ValorFuturo = cupomArredondado,
                ValorPresente = vpCupom
            });
        }

        // Vencimento (principal + cupom se for data de cupom)
        var diasUteisAteVencimento = CalcularDiasUteis(dataReferencia, dataVencimentoAjustada);
        var anosAteVencimento = CalcularFracaoAno(diasUteisAteVencimento);
        var fatorDescontoVencimento = CalcularFatorDesconto(taxaDecimal, anosAteVencimento);

        var valorFuturo = VALOR_NOMINAL;
        if ((dataVencimento.Month == 1 && dataVencimento.Day == 1) ||
            (dataVencimento.Month == 7 && dataVencimento.Day == 1))
        {
            valorFuturo += cupomArredondado;
        }

        var vpPrincipal = valorFuturo / fatorDescontoVencimento;
        vpPrincipal = RoundingRules.NtnF.FluxoPagamentos(vpPrincipal);

        fluxos.Add(new FluxoPagamento
        {
            DataPagamento = dataVencimentoAjustada,
            DiaUseis = diasUteisAteVencimento,
            JurosSemestral = cupomArredondado,
            ValorFuturo = valorFuturo,
            ValorPresente = vpPrincipal
        });

        return fluxos;
    }

    /// <summary>
    /// Retorna o fluxo detalhado de pagamentos da NTN-C para debug
    /// </summary>
    public static List<FluxoPagamento> NTNC_ObterFluxoDetalhado(decimal taxaReal, decimal vna, DateTime dataReferencia, DateTime dataVencimento)
    {
        var fluxos = new List<FluxoPagamento>();
        var dataVencimentoAjustada = AjustarDataVencimento(dataVencimento);
        var taxaTruncada = RoundingRules.NtnC.TaxaRetorno(taxaReal);
        var taxa = ConverterTaxaParaDecimal(taxaTruncada);

        // Cupom semestral em PERCENTUAL (base 100)
        var cupomPercentual = CUPOM_SEMESTRAL_NTNC * 100m;
        var cupomArredondado = RoundingRules.NtnC.JurosSemestrais(cupomPercentual);

        // Gera datas de pagamento de cupons
        var datasPagamento = GerarDatasPagamentoCupons(dataReferencia, dataVencimento);

        // Cupons intermediários
        foreach (var dataPagamento in datasPagamento)
        {
            var diasUteis = CalcularDiasUteis(dataReferencia, dataPagamento);
            var anos = CalcularFracaoAno(diasUteis);
            var fatorDesconto = CalcularFatorDesconto(taxa, anos);
            var vpCupom = cupomArredondado / fatorDesconto;
            vpCupom = RoundingRules.NtnC.FluxoPagamentos(vpCupom);

            fluxos.Add(new FluxoPagamento
            {
                DataPagamento = dataPagamento,
                DiaUseis = diasUteis,
                JurosSemestral = cupomArredondado,
                ValorFuturo = cupomArredondado,
                ValorPresente = vpCupom
            });
        }

        // Vencimento (principal + cupom se for data de cupom)
        var diasUteisAteVencimento = CalcularDiasUteis(dataReferencia, dataVencimentoAjustada);
        var anosAteVencimento = CalcularFracaoAno(diasUteisAteVencimento);
        var fatorDescontoVencimento = CalcularFatorDesconto(taxa, anosAteVencimento);

        var valorFuturo = 100m;
        if ((dataVencimento.Month == 1 && dataVencimento.Day == 1) ||
            (dataVencimento.Month == 7 && dataVencimento.Day == 1))
        {
            valorFuturo += cupomArredondado;
        }

        var vpPrincipal = valorFuturo / fatorDescontoVencimento;
        vpPrincipal = RoundingRules.NtnC.FluxoPagamentos(vpPrincipal);

        fluxos.Add(new FluxoPagamento
        {
            DataPagamento = dataVencimentoAjustada,
            DiaUseis = diasUteisAteVencimento,
            JurosSemestral = cupomArredondado,
            ValorFuturo = valorFuturo,
            ValorPresente = vpPrincipal
        });

        return fluxos;
    }

    #endregion
}
