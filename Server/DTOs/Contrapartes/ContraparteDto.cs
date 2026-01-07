namespace ArxFlow.Server.DTOs.Contrapartes;

// DTO para retorno de Contraparte
public record ContraparteDto
{
    public int Id { get; init; }
    public required string Nome { get; init; }
}

// DTO para criação de Contraparte
public record CreateContraparteRequest
{
    public required string Nome { get; init; }
}

// DTO para atualização de Contraparte
public record UpdateContraparteRequest
{
    public required string Nome { get; init; }
}
