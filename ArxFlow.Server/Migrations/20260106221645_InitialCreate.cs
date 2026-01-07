using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArxFlow.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnbimaTPFs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DataReferencia = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CodigoSelic = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    DataBase = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DataVencimento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TaxaCompra = table.Column<decimal>(type: "TEXT", nullable: true),
                    TaxaVenda = table.Column<decimal>(type: "TEXT", nullable: true),
                    TaxaIndicativa = table.Column<decimal>(type: "TEXT", nullable: true),
                    PU = table.Column<decimal>(type: "TEXT", nullable: true),
                    DesvioPadrao = table.Column<decimal>(type: "TEXT", nullable: true),
                    IntervaloIndMinD0 = table.Column<decimal>(type: "TEXT", nullable: true),
                    IntervaloIndMaxD0 = table.Column<decimal>(type: "TEXT", nullable: true),
                    IntervaloIndMinD1 = table.Column<decimal>(type: "TEXT", nullable: true),
                    IntervaloIndMaxD1 = table.Column<decimal>(type: "TEXT", nullable: true),
                    Criterio = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ImportedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnbimaTPFs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AnbimaVNAs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CodigoSelic = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    DataReferencia = table.Column<DateTime>(type: "TEXT", nullable: false),
                    VNA = table.Column<decimal>(type: "TEXT", nullable: false),
                    Indice = table.Column<decimal>(type: "TEXT", nullable: true),
                    Referencia = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    ValidoAPartirDe = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ImportedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnbimaVNAs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "B3InstrumentosDerivativos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DataReferencia = table.Column<DateTime>(type: "TEXT", nullable: false),
                    InstrumentoFinanceiro = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Ativo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DescricaoAtivo = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Segmento = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Mercado = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Categoria = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DataVencimento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CodigoExpiracao = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    DataInicioNegocio = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DataFimNegocio = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CodigoBase = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    CriterioConversao = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DataMaturidadeAlvo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    IndicadorConversao = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    CodigoIsin = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    CodigoCfi = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    DataInicioAvisoEntrega = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DataFimAvisoEntrega = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TipoOpcao = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    MultiplicadorContrato = table.Column<decimal>(type: "TEXT", nullable: true),
                    QuantidadeAtivos = table.Column<decimal>(type: "TEXT", nullable: true),
                    TamanhoLoteAlocacao = table.Column<int>(type: "INTEGER", nullable: true),
                    MoedaNegociada = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    TipoEntrega = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DiasSaque = table.Column<int>(type: "INTEGER", nullable: true),
                    DiasUteis = table.Column<int>(type: "INTEGER", nullable: true),
                    DiasCorridos = table.Column<int>(type: "INTEGER", nullable: true),
                    PrecoBaseEstrategia = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DiasPosicaoFutura = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CodigoTipoEstrategia1 = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    SimboloSubjacente1 = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CodigoTipoEstrategia2 = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    SimboloSubjacente2 = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    PrecoExercicio = table.Column<decimal>(type: "TEXT", nullable: true),
                    EstiloOpcao = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    TipoValor = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    IndicadorPremioAntecipado = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    DataLimitePosicoesAbertas = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_B3InstrumentosDerivativos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "B3InstrumentosRendaFixa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DataReferencia = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CodigoIF = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CodigoIsin = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Emissor = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    InstrumentoFinanceiro = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Incentivada = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    NumeroSerie = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    NumeroEmissao = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Indexador = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    PercentualIndexador = table.Column<decimal>(type: "TEXT", nullable: true),
                    TaxaAdicional = table.Column<decimal>(type: "TEXT", nullable: true),
                    BaseCalculo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Vencimento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    QuantidadeEmitida = table.Column<long>(type: "INTEGER", nullable: true),
                    PrecoUnitarioEmissao = table.Column<decimal>(type: "TEXT", nullable: true),
                    EsforcoRestrito = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    TipoEmissao = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    IndicadorOferta = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_B3InstrumentosRendaFixa", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "B3PrecosDerivativos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ticker = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DataReferencia = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ContratosEmAberto = table.Column<long>(type: "INTEGER", nullable: true),
                    PrecoInicial = table.Column<decimal>(type: "TEXT", nullable: true),
                    PrecoMinimo = table.Column<decimal>(type: "TEXT", nullable: true),
                    PrecoMaximo = table.Column<decimal>(type: "TEXT", nullable: true),
                    PrecoMedioPonderado = table.Column<decimal>(type: "TEXT", nullable: true),
                    UltimoPreco = table.Column<decimal>(type: "TEXT", nullable: true),
                    QuantidadeNegociosRegulares = table.Column<long>(type: "INTEGER", nullable: true),
                    PrecoAjuste = table.Column<decimal>(type: "TEXT", nullable: true),
                    TaxaAjuste = table.Column<decimal>(type: "TEXT", nullable: true),
                    SituacaoPrecoAjuste = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    PrecoAjusteAnterior = table.Column<decimal>(type: "TEXT", nullable: true),
                    TaxaAjusteAnterior = table.Column<decimal>(type: "TEXT", nullable: true),
                    SituacaoPrecoAjusteAnterior = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Moeda = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    ImportedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_B3PrecosDerivativos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BcbExpectativas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Indicador = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Data = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataReferencia = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TipoCalculo = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Media = table.Column<decimal>(type: "TEXT", nullable: true),
                    Mediana = table.Column<decimal>(type: "TEXT", nullable: true),
                    DesvioPadrao = table.Column<decimal>(type: "TEXT", nullable: true),
                    Minimo = table.Column<decimal>(type: "TEXT", nullable: true),
                    Maximo = table.Column<decimal>(type: "TEXT", nullable: true),
                    ImportedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BcbExpectativas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contrapartes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contrapartes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Emissores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Documento = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    AlphaToolsId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emissores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fundos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Cnpj = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    AlphaToolsId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fundos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ativos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CodAtivo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TipoAtivo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    EmissorId = table.Column<int>(type: "INTEGER", nullable: true),
                    AlphaToolsId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DataVencimento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ativos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ativos_Emissores_EmissorId",
                        column: x => x.EmissorId,
                        principalTable: "Emissores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Boletas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BoletaPrincipalId = table.Column<int>(type: "INTEGER", nullable: true),
                    AtivoId = table.Column<int>(type: "INTEGER", nullable: true),
                    ContraparteId = table.Column<int>(type: "INTEGER", nullable: true),
                    FundoId = table.Column<int>(type: "INTEGER", nullable: true),
                    Ticker = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    TipoOperacao = table.Column<string>(type: "TEXT", maxLength: 1, nullable: false),
                    Volume = table.Column<decimal>(type: "TEXT", nullable: false),
                    Quantidade = table.Column<decimal>(type: "TEXT", nullable: false),
                    TipoPrecificacao = table.Column<int>(type: "INTEGER", nullable: false),
                    NtnbReferencia = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    SpreadValor = table.Column<decimal>(type: "TEXT", nullable: true),
                    DataFixing = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TaxaNominal = table.Column<decimal>(type: "TEXT", nullable: true),
                    PU = table.Column<decimal>(type: "TEXT", nullable: true),
                    Alocacao = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Usuario = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Observacao = table.Column<string>(type: "TEXT", nullable: false),
                    DataLiquidacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Boletas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Boletas_Ativos_AtivoId",
                        column: x => x.AtivoId,
                        principalTable: "Ativos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Boletas_Boletas_BoletaPrincipalId",
                        column: x => x.BoletaPrincipalId,
                        principalTable: "Boletas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Boletas_Contrapartes_ContraparteId",
                        column: x => x.ContraparteId,
                        principalTable: "Contrapartes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Boletas_Fundos_FundoId",
                        column: x => x.FundoId,
                        principalTable: "Fundos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnbimaTPF_DataReferencia",
                table: "AnbimaTPFs",
                column: "DataReferencia");

            migrationBuilder.CreateIndex(
                name: "IX_AnbimaTPF_DataReferencia_Titulo_DataVencimento",
                table: "AnbimaTPFs",
                columns: new[] { "DataReferencia", "Titulo", "DataVencimento" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnbimaTPF_DataVencimento",
                table: "AnbimaTPFs",
                column: "DataVencimento");

            migrationBuilder.CreateIndex(
                name: "IX_AnbimaTPF_Titulo",
                table: "AnbimaTPFs",
                column: "Titulo");

            migrationBuilder.CreateIndex(
                name: "IX_AnbimaVNA_DataReferencia",
                table: "AnbimaVNAs",
                column: "DataReferencia");

            migrationBuilder.CreateIndex(
                name: "IX_AnbimaVNA_DataReferencia_Titulo",
                table: "AnbimaVNAs",
                columns: new[] { "DataReferencia", "Titulo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnbimaVNA_Titulo",
                table: "AnbimaVNAs",
                column: "Titulo");

            migrationBuilder.CreateIndex(
                name: "IX_Ativo_AlphaToolsId",
                table: "Ativos",
                column: "AlphaToolsId");

            migrationBuilder.CreateIndex(
                name: "IX_Ativo_CodAtivo",
                table: "Ativos",
                column: "CodAtivo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ativo_EmissorId",
                table: "Ativos",
                column: "EmissorId");

            migrationBuilder.CreateIndex(
                name: "IX_B3InstrumentoDerivativo_Ativo",
                table: "B3InstrumentosDerivativos",
                column: "Ativo");

            migrationBuilder.CreateIndex(
                name: "IX_B3InstrumentoDerivativo_DataVencimento",
                table: "B3InstrumentosDerivativos",
                column: "DataVencimento");

            migrationBuilder.CreateIndex(
                name: "IX_B3InstrumentoDerivativo_InstrumentoFinanceiro_DataReferencia",
                table: "B3InstrumentosDerivativos",
                columns: new[] { "InstrumentoFinanceiro", "DataReferencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_B3InstrumentoRendaFixa_CodigoIF_DataReferencia",
                table: "B3InstrumentosRendaFixa",
                columns: new[] { "CodigoIF", "DataReferencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_B3InstrumentoRendaFixa_Emissor",
                table: "B3InstrumentosRendaFixa",
                column: "Emissor");

            migrationBuilder.CreateIndex(
                name: "IX_B3InstrumentoRendaFixa_InstrumentoFinanceiro",
                table: "B3InstrumentosRendaFixa",
                column: "InstrumentoFinanceiro");

            migrationBuilder.CreateIndex(
                name: "IX_B3InstrumentoRendaFixa_Vencimento",
                table: "B3InstrumentosRendaFixa",
                column: "Vencimento");

            migrationBuilder.CreateIndex(
                name: "IX_B3PrecoDerivativo_DataReferencia",
                table: "B3PrecosDerivativos",
                column: "DataReferencia");

            migrationBuilder.CreateIndex(
                name: "IX_B3PrecoDerivativo_Ticker_DataReferencia",
                table: "B3PrecosDerivativos",
                columns: new[] { "Ticker", "DataReferencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BcbExpectativa_Data",
                table: "BcbExpectativas",
                column: "Data");

            migrationBuilder.CreateIndex(
                name: "IX_BcbExpectativa_Data_Indicador_DataReferencia",
                table: "BcbExpectativas",
                columns: new[] { "Data", "Indicador", "DataReferencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BcbExpectativa_Indicador",
                table: "BcbExpectativas",
                column: "Indicador");

            migrationBuilder.CreateIndex(
                name: "IX_Boleta_AtivoId",
                table: "Boletas",
                column: "AtivoId");

            migrationBuilder.CreateIndex(
                name: "IX_Boleta_ContraparteId",
                table: "Boletas",
                column: "ContraparteId");

            migrationBuilder.CreateIndex(
                name: "IX_Boleta_CriadoEm",
                table: "Boletas",
                column: "CriadoEm");

            migrationBuilder.CreateIndex(
                name: "IX_Boleta_FundoId",
                table: "Boletas",
                column: "FundoId");

            migrationBuilder.CreateIndex(
                name: "IX_Boleta_Ticker",
                table: "Boletas",
                column: "Ticker");

            migrationBuilder.CreateIndex(
                name: "IX_Boletas_BoletaPrincipalId",
                table: "Boletas",
                column: "BoletaPrincipalId");

            migrationBuilder.CreateIndex(
                name: "IX_Emissor_AlphaToolsId",
                table: "Emissores",
                column: "AlphaToolsId");

            migrationBuilder.CreateIndex(
                name: "IX_Emissor_Nome",
                table: "Emissores",
                column: "Nome");

            migrationBuilder.CreateIndex(
                name: "IX_Fundo_AlphaToolsId",
                table: "Fundos",
                column: "AlphaToolsId");

            migrationBuilder.CreateIndex(
                name: "IX_Fundo_Nome",
                table: "Fundos",
                column: "Nome");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnbimaTPFs");

            migrationBuilder.DropTable(
                name: "AnbimaVNAs");

            migrationBuilder.DropTable(
                name: "B3InstrumentosDerivativos");

            migrationBuilder.DropTable(
                name: "B3InstrumentosRendaFixa");

            migrationBuilder.DropTable(
                name: "B3PrecosDerivativos");

            migrationBuilder.DropTable(
                name: "BcbExpectativas");

            migrationBuilder.DropTable(
                name: "Boletas");

            migrationBuilder.DropTable(
                name: "Ativos");

            migrationBuilder.DropTable(
                name: "Contrapartes");

            migrationBuilder.DropTable(
                name: "Fundos");

            migrationBuilder.DropTable(
                name: "Emissores");
        }
    }
}
