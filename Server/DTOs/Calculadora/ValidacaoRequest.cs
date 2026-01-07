namespace ArxFlow.Server.DTOs.Calculadora;

public class ValidacaoRequest
{
    public int DiasUteis { get; set; } = 5;
}

public class ValidacaoResponse
{
    public List<ResultadoValidacao> Resultados { get; set; } = [];
    public ResumoValidacao Resumo { get; set; } = new();
    public List<string> Erros { get; set; } = [];
}

public class ResultadoValidacao
{
    public string Titulo { get; set; } = string.Empty;
    public DateTime DataReferencia { get; set; }
    public DateTime DataVencimento { get; set; }
    public decimal TaxaMercado { get; set; }
    public decimal TaxaCalculada { get; set; }
    public decimal PUMercado { get; set; }
    public decimal PUCalculado { get; set; }
    public decimal DiferenciaPU { get; set; }
    public decimal DiferencaTaxa { get; set; }
    public decimal? VNA { get; set; }
    public bool OK { get; set; }
}

public class ResumoValidacao
{
    public int LtnOK { get; set; }
    public int LtnFalhou { get; set; }
    public int NtnbOK { get; set; }
    public int NtnbFalhou { get; set; }
    public int NtnfOK { get; set; }
    public int NtnfFalhou { get; set; }
    public int LftOK { get; set; }
    public int LftFalhou { get; set; }
    public int NtncOK { get; set; }
    public int NtncFalhou { get; set; }
}
