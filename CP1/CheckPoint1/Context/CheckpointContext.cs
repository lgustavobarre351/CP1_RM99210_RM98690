using Microsoft.EntityFrameworkCore;
using CheckPoint1.Models;

namespace CheckPoint1;

/// <summary>
/// Contexto do Entity Framework Core para o sistema de loja
/// Responsável pela configuração do banco de dados, relacionamentos e dados iniciais
/// Utiliza SQLite como provider de banco de dados para portabilidade
/// </summary>
public class CheckpointContext : DbContext
{
    #region DbSets - Definição das Tabelas

    // ================================================================
    // DBSETS - REPRESENTAÇÃO DAS TABELAS NO BANCO DE DADOS
    // ================================================================

    /// <summary>
    /// Tabela de categorias de produtos
    /// </summary>
    public DbSet<Categoria> Categorias { get; set; }
    
    /// <summary>
    /// Tabela de produtos do sistema
    /// </summary>
    public DbSet<Produto> Produtos { get; set; }
    
    /// <summary>
    /// Tabela de clientes cadastrados
    /// </summary>
    public DbSet<Cliente> Clientes { get; set; }
    
    /// <summary>
    /// Tabela de pedidos realizados
    /// </summary>
    public DbSet<Pedido> Pedidos { get; set; }
    
    /// <summary>
    /// Tabela de itens dos pedidos (relacionamento N:N entre Pedido e Produto)
    /// </summary>
    public DbSet<PedidoItem> PedidoItens { get; set; }

    #endregion

    #region Configuração do Banco de Dados

    // ================================================================
    // CONFIGURAÇÃO - CONNECTION STRING E PROVIDER
    // ================================================================

    /// <summary>
    /// Configura a conexão com o banco de dados SQLite
    /// Define o arquivo de banco como "loja.db" na raiz do projeto
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Configurar SQLite como provider de banco de dados
        optionsBuilder.UseSqlite("Data Source=loja.db");
    }

    #endregion

    #region Configuração do Modelo

    // ================================================================
    // CONFIGURAÇÃO DO MODELO - RELACIONAMENTOS E RESTRIÇÕES
    // ================================================================

    /// <summary>
    /// Configura o modelo de dados, relacionamentos, índices e dados iniciais
    /// Define todas as regras de integridade referencial e comportamentos de exclusão
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ----------------------------------------------------------------
        // RELACIONAMENTOS - CONFIGURAÇÃO DE CHAVES ESTRANGEIRAS
        // ----------------------------------------------------------------

        // Relacionamento 1:N - Categoria possui muitos Produtos
        // Configurar cascade delete para remover produtos quando categoria é excluída
        modelBuilder.Entity<Produto>()
            .HasOne(p => p.Categoria)
            .WithMany(c => c.Produtos)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relacionamento 1:N - Cliente possui muitos Pedidos
        // Configurar cascade delete para remover pedidos quando cliente é excluído
        modelBuilder.Entity<Pedido>()
            .HasOne(p => p.Cliente)
            .WithMany(c => c.Pedidos)
            .HasForeignKey(p => p.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relacionamento 1:N - Pedido possui muitos PedidoItens
        // Configurar cascade delete para remover itens quando pedido é excluído
        modelBuilder.Entity<PedidoItem>()
            .HasOne(pi => pi.Pedido)
            .WithMany(p => p.Itens)
            .HasForeignKey(pi => pi.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relacionamento 1:N - Produto pode estar em muitos PedidoItens
        // Configurar restrict delete para proteger produtos que já foram vendidos
        modelBuilder.Entity<PedidoItem>()
            .HasOne(pi => pi.Produto)
            .WithMany(p => p.PedidoItens)
            .HasForeignKey(pi => pi.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        // ----------------------------------------------------------------
        // ÍNDICES - CONFIGURAÇÃO DE ÍNDICES ÚNICOS PARA PERFORMANCE
        // ----------------------------------------------------------------

        // Índice único no email do cliente para evitar duplicatas
        modelBuilder.Entity<Cliente>()
            .HasIndex(c => c.Email)
            .IsUnique();

        // Índice único no número do pedido para controle de duplicatas
        modelBuilder.Entity<Pedido>()
            .HasIndex(p => p.NumeroPedido)
            .IsUnique();

        // ----------------------------------------------------------------
        // DADOS INICIAIS - SEED DATA PARA TESTES E DEMONSTRAÇÃO
        // ----------------------------------------------------------------

        ConfigurarDadosIniciais(modelBuilder);
    }

    /// <summary>
    /// Configura os dados iniciais para popular o banco na primeira execução
    /// Inclui categorias, produtos, clientes, pedidos e itens para demonstração
    /// </summary>
    private void ConfigurarDadosIniciais(ModelBuilder modelBuilder)
    {
        // Seed data para Categorias - 3 categorias básicas
        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nome = "Eletrônicos", Descricao = "Produtos eletrônicos diversos", DataCriacao = DateTime.Now },
            new Categoria { Id = 2, Nome = "Roupas", Descricao = "Vestuário em geral", DataCriacao = DateTime.Now },
            new Categoria { Id = 3, Nome = "Livros", Descricao = "Livros e material educativo", DataCriacao = DateTime.Now }
        );

        // Seed data para Produtos - mix de produtos com e sem estoque para testes
        modelBuilder.Entity<Produto>().HasData(
            new Produto { Id = 1, Nome = "Smartphone", Descricao = "Smartphone Android", Preco = 899.99m, Estoque = 50, CategoriaId = 1, DataCriacao = DateTime.Now, Ativo = true },
            new Produto { Id = 2, Nome = "Notebook", Descricao = "Notebook Intel i5", Preco = 2499.99m, Estoque = 0, CategoriaId = 1, DataCriacao = DateTime.Now, Ativo = true },
            new Produto { Id = 3, Nome = "Camiseta", Descricao = "Camiseta básica algodão", Preco = 39.99m, Estoque = 100, CategoriaId = 2, DataCriacao = DateTime.Now, Ativo = true },
            new Produto { Id = 4, Nome = "Calça Jeans", Descricao = "Calça jeans masculina", Preco = 89.99m, Estoque = 10, CategoriaId = 2, DataCriacao = DateTime.Now, Ativo = true },
            new Produto { Id = 5, Nome = "C# in Action", Descricao = "Livro sobre programação C#", Preco = 79.99m, Estoque = 25, CategoriaId = 3, DataCriacao = DateTime.Now, Ativo = true },
            new Produto { Id = 6, Nome = "Clean Code", Descricao = "Livro sobre código limpo", Preco = 69.99m, Estoque = 0, CategoriaId = 3, DataCriacao = DateTime.Now, Ativo = true }
        );

        // Seed data para Clientes - 2 clientes para demonstração
        modelBuilder.Entity<Cliente>().HasData(
            new Cliente { Id = 1, Nome = "João Silva", Email = "joao@email.com", Telefone = "11999999999", CPF = "12345678901", Endereco = "Rua A, 123", Cidade = "São Paulo", Estado = "SP", CEP = "01234567", DataCadastro = DateTime.Now, Ativo = true },
            new Cliente { Id = 2, Nome = "Maria Santos", Email = "maria@email.com", Telefone = "11888888888", CPF = "98765432109", Endereco = "Rua B, 456", Cidade = "Rio de Janeiro", Estado = "RJ", CEP = "98765432", DataCadastro = DateTime.Now, Ativo = true }
        );

        // Seed data para Pedidos - 2 pedidos com status diferentes para testes
        modelBuilder.Entity<Pedido>().HasData(
            new Pedido { Id = 1, NumeroPedido = "PED001", DataPedido = DateTime.Now.AddDays(-5), Status = StatusPedido.Entregue, ValorTotal = 939.98m, ClienteId = 1 },
            new Pedido { Id = 2, NumeroPedido = "PED002", DataPedido = DateTime.Now.AddDays(-2), Status = StatusPedido.Confirmado, ValorTotal = 149.98m, ClienteId = 2 }
        );

        // Seed data para PedidoItens - itens dos pedidos acima
        modelBuilder.Entity<PedidoItem>().HasData(
            new PedidoItem { Id = 1, PedidoId = 1, ProdutoId = 1, Quantidade = 1, PrecoUnitario = 899.99m },
            new PedidoItem { Id = 2, PedidoId = 1, ProdutoId = 3, Quantidade = 1, PrecoUnitario = 39.99m },
            new PedidoItem { Id = 3, PedidoId = 2, ProdutoId = 4, Quantidade = 1, PrecoUnitario = 89.99m },
            new PedidoItem { Id = 4, PedidoId = 2, ProdutoId = 5, Quantidade = 1, PrecoUnitario = 79.99m }
        );
    }

    #endregion
}