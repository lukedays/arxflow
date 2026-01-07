namespace ArxFlow.Server.DTOs.Emissores;

// DTO para retorno de Emissor
public record EmissorDto
{
    public int Id { get; init; }
    public required string Nome { get; init; }
    public string? Documento { get; init; }
    public string? AlphaToolsId { get; init; }
}

// DTO para criação de Emissor
public record CreateEmissorRequest
{
    public required string Nome { get; init; }
    public string? Documento { get; init; }
    public string? AlphaToolsId { get; init; }
}

// DTO para atualização de Emissor
public record UpdateEmissorRequest
{
    public required string Nome { get; init; }
    public string? Documento { get; init; }
    public string? AlphaToolsId { get; init; }
}
