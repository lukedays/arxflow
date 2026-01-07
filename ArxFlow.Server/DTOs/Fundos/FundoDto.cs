namespace ArxFlow.Server.DTOs.Fundos;

// DTO para retorno de Fundo
public record FundoDto
{
    public int Id { get; init; }
    public required string Nome { get; init; }
    public string? Cnpj { get; init; }
    public string? AlphaToolsId { get; init; }
}

// DTO para criação de Fundo
public record CreateFundoRequest
{
    public required string Nome { get; init; }
    public string? Cnpj { get; init; }
    public string? AlphaToolsId { get; init; }
}

// DTO para atualização de Fundo
public record UpdateFundoRequest
{
    public required string Nome { get; init; }
    public string? Cnpj { get; init; }
    public string? AlphaToolsId { get; init; }
}
