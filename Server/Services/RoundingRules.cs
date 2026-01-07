namespace ArxFlow.Server.Services;

public static class RoundingRules
{
    public static decimal Truncate(decimal value, int decimals)
    {
        decimal multiplier = (decimal)Math.Pow(10, decimals);
        return Math.Truncate(value * multiplier) / multiplier;
    }

    public static decimal Round(decimal value, int decimals)
    {
        return Math.Round(value, decimals, MidpointRounding.AwayFromZero);
    }

    public static class Ltn
    {
        public static decimal TaxaRetorno(decimal taxa) => Truncate(taxa, 4);
        public static decimal PrecoUnitario(decimal pu) => Truncate(pu, 6);
        public static decimal ExponencialDias(decimal valor) => Truncate(valor, 14);
        public static decimal ValorFinanceiro(decimal valor) => Truncate(valor, 2);
    }

    public static class NtnB
    {
        public static decimal TaxaRetorno(decimal taxa) => Truncate(taxa, 4);
        public static decimal JurosSemestrais(decimal juros) => Round(juros, 6);
        public static decimal FluxoPagamentos(decimal fluxo) => Round(fluxo, 10);
        public static decimal PrecoUnitario(decimal pu) => Truncate(pu, 6);
        public static decimal VNA(decimal vna) => Truncate(vna, 6);
        public static decimal Cotacao(decimal cotacao) => Truncate(cotacao, 4);
        public static decimal ValorFinanceiro(decimal valor) => Truncate(valor, 2);
    }

    public static class NtnF
    {
        public static decimal TaxaRetorno(decimal taxa) => Truncate(taxa, 4);
        public static decimal JurosSemestrais(decimal juros) => Round(juros, 6);
        public static decimal FluxoPagamentos(decimal fluxo) => Round(fluxo, 10);
        public static decimal PrecoUnitario(decimal pu) => Truncate(pu, 6);
        public static decimal ValorFinanceiro(decimal valor) => Truncate(valor, 2);
    }

    public static class Lft
    {
        public static decimal TaxaRetorno(decimal taxa) => Truncate(taxa, 4);
        public static decimal ExponencialDias(decimal valor) => Truncate(valor, 14);
        public static decimal Cotacao(decimal cotacao) => Truncate(cotacao, 6);
        public static decimal PrecoUnitario(decimal pu) => Truncate(pu, 6);
        public static decimal VNA(decimal vna) => Truncate(vna, 6);
        public static decimal ValorFinanceiro(decimal valor) => Truncate(valor, 2);
    }

    public static class NtnC
    {
        public static decimal TaxaRetorno(decimal taxa) => Truncate(taxa, 4);
        public static decimal JurosSemestrais(decimal juros) => Round(juros, 6);
        public static decimal FluxoPagamentos(decimal fluxo) => Round(fluxo, 10);
        public static decimal PrecoUnitario(decimal pu) => Truncate(pu, 6);
        public static decimal VNA(decimal vna) => Truncate(vna, 6);
        public static decimal Cotacao(decimal cotacao) => Truncate(cotacao, 4);
        public static decimal ValorFinanceiro(decimal valor) => Truncate(valor, 2);
    }
}
