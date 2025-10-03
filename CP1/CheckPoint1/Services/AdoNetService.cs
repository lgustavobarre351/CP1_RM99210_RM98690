using System.Data.SQLite;

namespace CheckPoint1.Services;

/// <summary>
/// Serviço responsável por operações de banco de dados usando ADO.NET.
/// Implementa consultas SQL diretas, operações em lote e análises de performance.
/// Demonstra uso de SQLiteConnection, transações e comandos parametrizados.
/// </summary>
public class AdoNetService
{
    #region Propriedades e Configuração

    private readonly string _connectionString;

    /// <summary>
    /// Construtor que inicializa a connection string para SQLite
    /// Utiliza o mesmo arquivo de banco criado pelo Entity Framework
    /// </summary>
    public AdoNetService()
    {
        _connectionString = "Data Source=loja.db";
    }

    /// <summary>
    /// Cria e retorna uma nova conexão SQLite
    /// Método auxiliar para centralizar a criação de conexões
    /// </summary>
    private SQLiteConnection GetConnection()
    {
        return new SQLiteConnection(_connectionString);
    }

    #endregion

    #region Consultas Complexas

    // ================================================================
    // CONSULTAS COMPLEXAS - JOINS DE MÚLTIPLAS TABELAS E AGREGAÇÕES
    // ================================================================

    /// <summary>
    /// Relatório completo de vendas com JOIN de 4 tabelas
    /// Demonstra uso de INNER JOINs e ordenação por data
    /// </summary>
    public void RelatorioVendasCompleto()
    {
        Console.WriteLine("=== RELATÓRIO VENDAS COMPLETO (ADO.NET) ===");
        
        using var connection = GetConnection();
        connection.Open();
        
        // Query complexa com 4 JOINs para buscar dados relacionados
        var sql = @"
            SELECT p.NumeroPedido, c.Nome as NomeCliente, pr.Nome as NomeProduto, 
                   pi.Quantidade, pi.PrecoUnitario, 
                   (pi.Quantidade * pi.PrecoUnitario - COALESCE(pi.Desconto, 0)) as Subtotal
            FROM Pedidos p
            INNER JOIN Clientes c ON p.ClienteId = c.Id
            INNER JOIN PedidoItens pi ON p.Id = pi.PedidoId
            INNER JOIN Produtos pr ON pi.ProdutoId = pr.Id
            ORDER BY p.DataPedido DESC";
        
        using var command = new SQLiteCommand(sql, connection);
        using var reader = command.ExecuteReader();
        
        // Formatação em tabela para melhor visualização
        Console.WriteLine(new string('-', 100));
        Console.WriteLine("| {0,-12} | {1,-20} | {2,-20} | {3,-4} | {4,-10} | {5,-10} |", 
            "Num. Pedido", "Cliente", "Produto", "Qtd", "Preço Unit", "Subtotal");
        Console.WriteLine(new string('-', 100));
        
        while (reader.Read())
        {
            Console.WriteLine("| {0,-12} | {1,-20} | {2,-20} | {3,-4} | {4,-10:C} | {5,-10:C} |",
                reader["NumeroPedido"],
                reader["NomeCliente"],
                reader["NomeProduto"],
                reader["Quantidade"],
                Convert.ToDecimal(reader["PrecoUnitario"]),
                Convert.ToDecimal(reader["Subtotal"]));
        }
        Console.WriteLine(new string('-', 100));
    }

    /// <summary>
    /// Análise de faturamento agrupado por cliente
    /// Utiliza LEFT JOIN e funções de agregação (COUNT, SUM, AVG)
    /// </summary>
    public void FaturamentoPorCliente()
    {
        Console.WriteLine("=== FATURAMENTO POR CLIENTE ===");
        
        using var connection = GetConnection();
        connection.Open();
        
        // Query com LEFT JOIN para incluir clientes sem pedidos
        var sql = @"
            SELECT c.Nome, 
                   COUNT(p.Id) as QuantidadePedidos,
                   SUM(p.ValorTotal) as FaturamentoTotal,
                   AVG(p.ValorTotal) as TicketMedio
            FROM Clientes c
            LEFT JOIN Pedidos p ON c.Id = p.ClienteId
            GROUP BY c.Id, c.Nome
            ORDER BY FaturamentoTotal DESC";
        
        using var command = new SQLiteCommand(sql, connection);
        using var reader = command.ExecuteReader();
        
        Console.WriteLine(new string('-', 80));
        Console.WriteLine("| {0,-20} | {1,-8} | {2,-15} | {3,-12} |", 
            "Cliente", "Pedidos", "Faturamento", "Ticket Médio");
        Console.WriteLine(new string('-', 80));
        
        while (reader.Read())
        {
            // Tratamento de valores nulos vindos do banco
            var faturamento = reader["FaturamentoTotal"] != DBNull.Value ? Convert.ToDecimal(reader["FaturamentoTotal"]) : 0;
            var ticketMedio = reader["TicketMedio"] != DBNull.Value ? Convert.ToDecimal(reader["TicketMedio"]) : 0;
            
            Console.WriteLine("| {0,-20} | {1,-8} | {2,-15:C} | {3,-12:C} |",
                reader["Nome"],
                reader["QuantidadePedidos"],
                faturamento,
                ticketMedio);
        }
        Console.WriteLine(new string('-', 80));
    }

    /// <summary>
    /// Identifica produtos que nunca foram vendidos
    /// Demonstra uso de LEFT JOIN com IS NULL para encontrar registros órfãos
    /// </summary>
    public void ProdutosSemVenda()
    {
        Console.WriteLine("=== PRODUTOS SEM VENDAS ===");
        
        using var connection = GetConnection();
        connection.Open();
        
        // LEFT JOIN com IS NULL para encontrar produtos sem vendas
        var sql = @"
            SELECT p.Nome as NomeProduto, c.Nome as Categoria, p.Preco, p.Estoque,
                   (p.Preco * p.Estoque) as ValorParadoEstoque
            FROM Produtos p
            INNER JOIN Categorias c ON p.CategoriaId = c.Id
            LEFT JOIN PedidoItens pi ON p.Id = pi.ProdutoId
            WHERE pi.ProdutoId IS NULL
            ORDER BY ValorParadoEstoque DESC";
        
        using var command = new SQLiteCommand(sql, connection);
        using var reader = command.ExecuteReader();
        
        Console.WriteLine(new string('-', 90));
        Console.WriteLine("| {0,-20} | {1,-15} | {2,-10} | {3,-7} | {4,-15} |", 
            "Produto", "Categoria", "Preço", "Estoque", "Valor Parado");
        Console.WriteLine(new string('-', 90));
        
        decimal totalParado = 0;
        while (reader.Read())
        {
            var valorParado = Convert.ToDecimal(reader["ValorParadoEstoque"]);
            totalParado += valorParado;
            
            Console.WriteLine("| {0,-20} | {1,-15} | {2,-10:C} | {3,-7} | {4,-15:C} |",
                reader["NomeProduto"],
                reader["Categoria"],
                Convert.ToDecimal(reader["Preco"]),
                reader["Estoque"],
                valorParado);
        }
        Console.WriteLine(new string('-', 90));
        Console.WriteLine($"Total parado em estoque: {totalParado:C}");
    }

    #endregion

    #region Operações de Dados

    // ================================================================
    // OPERAÇÕES DE DADOS - CRUD COM TRANSAÇÕES E VALIDAÇÕES
    // ================================================================

    /// <summary>
    /// Atualiza estoque em lote para todos os produtos de uma categoria
    /// Demonstra uso de transações para garantir integridade dos dados
    /// Permite atualização interativa do estoque com validação e rollback automático
    /// </summary>
    public void AtualizarEstoqueLote()
    {
        Console.WriteLine("=== ATUALIZAR ESTOQUE EM LOTE ===");
        
        using var connection = GetConnection();
        connection.Open();
        
        // Listar categorias disponíveis para o usuário escolher
        var sqlCategorias = "SELECT Id, Nome FROM Categorias";
        using var commandCategorias = new SQLiteCommand(sqlCategorias, connection);
        using var readerCategorias = commandCategorias.ExecuteReader();
        
        Console.WriteLine("Categorias disponíveis:");
        while (readerCategorias.Read())
        {
            Console.WriteLine($"{readerCategorias["Id"]} - {readerCategorias["Nome"]}");
        }
        readerCategorias.Close();
        
        // Solicitar ID da categoria ao usuário
        Console.Write("Digite o ID da categoria: ");
        if (!int.TryParse(Console.ReadLine(), out int categoriaId))
        {
            Console.WriteLine("ID inválido!");
            return;
        }
        
        // Buscar produtos da categoria selecionada
        var sqlProdutos = "SELECT Id, Nome, Estoque FROM Produtos WHERE CategoriaId = @categoriaId";
        using var commandProdutos = new SQLiteCommand(sqlProdutos, connection);
        commandProdutos.Parameters.AddWithValue("@categoriaId", categoriaId);
        using var readerProdutos = commandProdutos.ExecuteReader();
        
        var produtos = new List<(int Id, string Nome, int EstoqueAtual)>();
        while (readerProdutos.Read())
        {
            produtos.Add((Convert.ToInt32(readerProdutos["Id"]), 
                         readerProdutos["Nome"]?.ToString() ?? "", 
                         Convert.ToInt32(readerProdutos["Estoque"])));
        }
        readerProdutos.Close();
        
        // Validar se existem produtos na categoria
        if (!produtos.Any())
        {
            Console.WriteLine("Nenhum produto encontrado nesta categoria!");
            return;
        }
        
        int registrosAfetados = 0;
        // Iniciar transação para garantir atomicidade das operações
        using var transaction = connection.BeginTransaction();
        
        try
        {
            // Iterar por todos os produtos da categoria para atualização
            foreach (var produto in produtos)
            {
                Console.WriteLine($"Produto: {produto.Nome} (Estoque atual: {produto.EstoqueAtual})");
                Console.Write("Nova quantidade em estoque: ");
                
                // Validar entrada do usuário e executar update se válida
                if (int.TryParse(Console.ReadLine(), out int novoEstoque))
                {
                    var sqlUpdate = "UPDATE Produtos SET Estoque = @estoque WHERE Id = @id";
                    using var commandUpdate = new SQLiteCommand(sqlUpdate, connection, transaction);
                    commandUpdate.Parameters.AddWithValue("@estoque", novoEstoque);
                    commandUpdate.Parameters.AddWithValue("@id", produto.Id);
                    
                    registrosAfetados += commandUpdate.ExecuteNonQuery();
                }
            }
            
            // Confirmar todas as alterações se tudo ocorreu bem
            transaction.Commit();
            Console.WriteLine($"Estoque atualizado com sucesso! {registrosAfetados} registros afetados.");
        }
        catch (Exception ex)
        {
            // Reverter alterações em caso de erro
            transaction.Rollback();
            Console.WriteLine($"Erro ao atualizar estoque: {ex.Message}");
        }
    }
        
    /// <summary>
    /// Insere um pedido completo com múltiplos itens usando transação
    /// Demonstra inserção master-detail com controle de estoque automático
    /// Valida disponibilidade de produtos e atualiza estoque em tempo real
    /// </summary>
    public void InserirPedidoCompleto()
    {
        Console.WriteLine("=== INSERIR PEDIDO COMPLETO ===");
        
        using var connection = GetConnection();
        connection.Open();
        // Iniciar transação para garantir consistência entre pedido e itens
        using var transaction = connection.BeginTransaction();
        
        try
        {
            // Listar clientes ativos disponíveis para seleção
            var sqlClientes = "SELECT Id, Nome FROM Clientes WHERE Ativo = 1";
            using var commandClientes = new SQLiteCommand(sqlClientes, connection, transaction);
            using var readerClientes = commandClientes.ExecuteReader();
            
            Console.WriteLine("Clientes disponíveis:");
            while (readerClientes.Read())
            {
                Console.WriteLine($"{readerClientes["Id"]} - {readerClientes["Nome"]}");
            }
            readerClientes.Close();
            
            // Solicitar e validar ID do cliente
            Console.Write("Digite o ID do cliente: ");
            if (!int.TryParse(Console.ReadLine(), out int clienteId))
            {
                Console.WriteLine("ID inválido!");
                return;
            }
            
            // Gerar número único do pedido baseado em timestamp
            var numeroPedido = $"PED{DateTime.Now:yyyyMMddHHmmss}";
            
            // Inserir registro master do pedido com valor inicial zero
            var sqlPedido = @"
                INSERT INTO Pedidos (NumeroPedido, DataPedido, Status, ValorTotal, ClienteId)
                VALUES (@numeroPedido, @dataPedido, @status, 0, @clienteId);
                SELECT last_insert_rowid();";
            
            using var commandPedido = new SQLiteCommand(sqlPedido, connection, transaction);
            commandPedido.Parameters.AddWithValue("@numeroPedido", numeroPedido);
            commandPedido.Parameters.AddWithValue("@dataPedido", DateTime.Now);
            commandPedido.Parameters.AddWithValue("@status", (int)StatusPedido.Pendente);
            commandPedido.Parameters.AddWithValue("@clienteId", clienteId);
            
            // Obter ID do pedido recém-criado para vincular os itens
            int pedidoId = Convert.ToInt32(commandPedido.ExecuteScalar());
            
            decimal valorTotal = 0;
            bool continuarAdicionando = true;
            
            // Loop para adicionar múltiplos itens ao pedido
            while (continuarAdicionando)
            {
                // Exibir produtos disponíveis com estoque
                var sqlProdutos = "SELECT Id, Nome, Preco, Estoque FROM Produtos WHERE Ativo = 1 AND Estoque > 0";
                using var commandProdutos = new SQLiteCommand(sqlProdutos, connection, transaction);
                using var readerProdutos = commandProdutos.ExecuteReader();
                
                Console.WriteLine("\nProdutos disponíveis:");
                while (readerProdutos.Read())
                {
                    Console.WriteLine($"{readerProdutos["Id"]} - {readerProdutos["Nome"]} - {Convert.ToDecimal(readerProdutos["Preco"]):C} (Estoque: {readerProdutos["Estoque"]})");
                }
                readerProdutos.Close();
                
                // Solicitar produto (0 para finalizar)
                Console.Write("Digite o ID do produto (0 para finalizar): ");
                if (!int.TryParse(Console.ReadLine(), out int produtoId) || produtoId == 0)
                {
                    continuarAdicionando = false;
                    continue;
                }
                
                // Solicitar e validar quantidade
                Console.Write("Digite a quantidade: ");
                if (!int.TryParse(Console.ReadLine(), out int quantidade) || quantidade <= 0)
                {
                    Console.WriteLine("Quantidade inválida!");
                    continue;
                }
                
                // Verificar disponibilidade e obter preço do produto
                var sqlEstoque = "SELECT Preco, Estoque FROM Produtos WHERE Id = @produtoId";
                using var commandEstoque = new SQLiteCommand(sqlEstoque, connection, transaction);
                commandEstoque.Parameters.AddWithValue("@produtoId", produtoId);
                using var readerEstoque = commandEstoque.ExecuteReader();
                
                if (!readerEstoque.Read())
                {
                    Console.WriteLine("Produto não encontrado!");
                    readerEstoque.Close();
                    continue;
                }
                
                var preco = Convert.ToDecimal(readerEstoque["Preco"]);
                var estoqueDisponivel = Convert.ToInt32(readerEstoque["Estoque"]);
                readerEstoque.Close();
                
                // Validar se há estoque suficiente
                if (quantidade > estoqueDisponivel)
                {
                    Console.WriteLine($"Estoque insuficiente! Disponível: {estoqueDisponivel}");
                    continue;
                }
                
                // Inserir item no pedido com preço atual do produto
                var sqlItem = @"
                    INSERT INTO PedidoItens (PedidoId, ProdutoId, Quantidade, PrecoUnitario)
                    VALUES (@pedidoId, @produtoId, @quantidade, @precoUnitario)";
                
                using var commandItem = new SQLiteCommand(sqlItem, connection, transaction);
                commandItem.Parameters.AddWithValue("@pedidoId", pedidoId);
                commandItem.Parameters.AddWithValue("@produtoId", produtoId);
                commandItem.Parameters.AddWithValue("@quantidade", quantidade);
                commandItem.Parameters.AddWithValue("@precoUnitario", preco);
                commandItem.ExecuteNonQuery();
                
                // Atualizar estoque do produto subtraindo a quantidade vendida
                var sqlUpdateEstoque = "UPDATE Produtos SET Estoque = Estoque - @quantidade WHERE Id = @produtoId";
                using var commandUpdateEstoque = new SQLiteCommand(sqlUpdateEstoque, connection, transaction);
                commandUpdateEstoque.Parameters.AddWithValue("@quantidade", quantidade);
                commandUpdateEstoque.Parameters.AddWithValue("@produtoId", produtoId);
                commandUpdateEstoque.ExecuteNonQuery();
                
                // Acumular valor total do pedido
                valorTotal += preco * quantidade;
                Console.WriteLine($"Item adicionado! Subtotal: {preco * quantidade:C}");
            }
            
            // Atualizar valor total final do pedido
            var sqlUpdatePedido = "UPDATE Pedidos SET ValorTotal = @valorTotal WHERE Id = @pedidoId";
            using var commandUpdatePedido = new SQLiteCommand(sqlUpdatePedido, connection, transaction);
            commandUpdatePedido.Parameters.AddWithValue("@valorTotal", valorTotal);
            commandUpdatePedido.Parameters.AddWithValue("@pedidoId", pedidoId);
            commandUpdatePedido.ExecuteNonQuery();
            
            // Confirmar todas as operações da transação
            transaction.Commit();
            Console.WriteLine($"Pedido {numeroPedido} criado com sucesso! Valor total: {valorTotal:C}");
        }
        catch (Exception ex)
        {
            // Reverter todas as alterações em caso de erro
            transaction.Rollback();
            Console.WriteLine($"Erro ao criar pedido: {ex.Message}");
        }
    }
        
    /// <summary>
    /// Exclui pedidos cancelados antigos para limpeza de dados históricos
    /// Remove pedidos cancelados há mais de 6 meses para otimizar performance
    /// Demonstra uso de parâmetros com datas e operações de limpeza
    /// </summary>
    public void ExcluirDadosAntigos()
    {
        Console.WriteLine("=== EXCLUIR DADOS ANTIGOS ===");
        
        using var connection = GetConnection();
        connection.Open();
        
        // Definir data de corte (6 meses atrás)
        var dataCorte = DateTime.Now.AddMonths(-6);
        
        // Query para excluir apenas pedidos cancelados antigos
        var sql = @"
            DELETE FROM Pedidos 
            WHERE Status = @statusCancelado 
            AND DataPedido < @dataCorte";
        
        using var command = new SQLiteCommand(sql, connection);
        command.Parameters.AddWithValue("@statusCancelado", (int)StatusPedido.Cancelado);
        command.Parameters.AddWithValue("@dataCorte", dataCorte);
        
        // Executar exclusão e informar quantidade de registros afetados
        int registrosExcluidos = command.ExecuteNonQuery();
        Console.WriteLine($"Excluídos {registrosExcluidos} pedidos cancelados há mais de 6 meses.");
    }
        
    /// <summary>
    /// Processa devolução completa de um pedido com reposição de estoque
    /// Demonstra transação complexa com múltiplas validações e rollback
    /// Atualiza status do pedido e restaura quantidades no estoque
    /// </summary>
    public void ProcessarDevolucao()
    {
        Console.WriteLine("=== PROCESSAR DEVOLUÇÃO ===");
        
        using var connection = GetConnection();
        connection.Open();
        // Iniciar transação para garantir consistência de dados
        using var transaction = connection.BeginTransaction();
        
        try
        {
            // Solicitar número do pedido para devolução
            Console.Write("Digite o número do pedido para devolução: ");
            var numeroPedido = Console.ReadLine();
            
            if (string.IsNullOrEmpty(numeroPedido))
            {
                Console.WriteLine("Número do pedido inválido!");
                return;
            }
            
            // Localizar dados do pedido no banco de dados
            var sqlPedido = @"
                SELECT Id, Status, ValorTotal, NumeroPedido 
                FROM Pedidos 
                WHERE NumeroPedido = @numeroPedido";
            
            using var commandPedido = new SQLiteCommand(sqlPedido, connection, transaction);
            commandPedido.Parameters.AddWithValue("@numeroPedido", numeroPedido);
            using var readerPedido = commandPedido.ExecuteReader();
            
            // Validar se o pedido existe
            if (!readerPedido.Read())
            {
                Console.WriteLine("Pedido não encontrado!");
                readerPedido.Close();
                return;
            }
            
            // Extrair dados do pedido para validações
            int pedidoId = Convert.ToInt32(readerPedido["Id"]);
            var status = (StatusPedido)Convert.ToInt32(readerPedido["Status"]);
            var valorTotal = Convert.ToDecimal(readerPedido["ValorTotal"]);
            readerPedido.Close();
            
            // Validar se o pedido pode ser devolvido
            if (status == StatusPedido.Cancelado)
            {
                Console.WriteLine("Este pedido já foi cancelado!");
                return;
            }
            
            if (status != StatusPedido.Entregue && status != StatusPedido.Confirmado)
            {
                Console.WriteLine("Só é possível devolver pedidos entregues ou confirmados!");
                return;
            }
            
            // Buscar todos os itens do pedido para reposição de estoque
            var sqlItens = @"
                SELECT pi.ProdutoId, pi.Quantidade, p.Nome
                FROM PedidoItens pi
                INNER JOIN Produtos p ON pi.ProdutoId = p.Id
                WHERE pi.PedidoId = @pedidoId";
            
            using var commandItens = new SQLiteCommand(sqlItens, connection, transaction);
            commandItens.Parameters.AddWithValue("@pedidoId", pedidoId);
            using var readerItens = commandItens.ExecuteReader();
            
            // Armazenar itens para processamento posterior
            var itens = new List<(int ProdutoId, int Quantidade, string NomeProduto)>();
            while (readerItens.Read())
            {
                itens.Add((Convert.ToInt32(readerItens["ProdutoId"]), 
                          Convert.ToInt32(readerItens["Quantidade"]),
                          readerItens["Nome"]?.ToString() ?? ""));
            }
            readerItens.Close();
            
            // Mostrar resumo da devolução para confirmação
            Console.WriteLine($"Itens do pedido {numeroPedido}:");
            foreach (var item in itens)
            {
                Console.WriteLine($"- {item.NomeProduto}: {item.Quantidade} unidades");
            }
            
            // Confirmar devolução com o usuário
            Console.Write("Confirma a devolução? (S/N): ");
            if (Console.ReadLine()?.ToUpper() != "S")
            {
                Console.WriteLine("Devolução cancelada!");
                return;
            }
            
            // Processar devolução de estoque para cada item
            foreach (var item in itens)
            {
                var sqlDevolverEstoque = "UPDATE Produtos SET Estoque = Estoque + @quantidade WHERE Id = @produtoId";
                using var commandDevolverEstoque = new SQLiteCommand(sqlDevolverEstoque, connection, transaction);
                commandDevolverEstoque.Parameters.AddWithValue("@quantidade", item.Quantidade);
                commandDevolverEstoque.Parameters.AddWithValue("@produtoId", item.ProdutoId);
                commandDevolverEstoque.ExecuteNonQuery();
            }
            
            // Alterar status do pedido para cancelado
            var sqlUpdateStatus = "UPDATE Pedidos SET Status = @status WHERE Id = @pedidoId";
            using var commandUpdateStatus = new SQLiteCommand(sqlUpdateStatus, connection, transaction);
            commandUpdateStatus.Parameters.AddWithValue("@status", (int)StatusPedido.Cancelado);
            commandUpdateStatus.Parameters.AddWithValue("@pedidoId", pedidoId);
            commandUpdateStatus.ExecuteNonQuery();
            
            // Confirmar todas as operações
            transaction.Commit();
            Console.WriteLine($"Devolução processada com sucesso! Valor: {valorTotal:C}");
            Console.WriteLine("Estoque dos produtos foi restituído.");
        }
        catch (Exception ex)
        {
            // Reverter operações em caso de erro
            transaction.Rollback();
            Console.WriteLine($"Erro ao processar devolução: {ex.Message}");
        }
    }
        
    // ================================================================
    // ANÁLISES DE PERFORMANCE - RELATÓRIOS TEMPORAIS E ESTATÍSTICAS
    // ================================================================
        
    /// <summary>
    /// Analisa performance de vendas agrupadas por período temporal
    /// Calcula crescimento mensal, ticket médio e tendências de faturamento
    /// Demonstra uso de funções de data SQLite e cálculos percentuais
    /// </summary>
    public void AnalisarPerformanceVendas()
    {
        Console.WriteLine("=== ANÁLISE PERFORMANCE VENDAS ===");
        
        using var connection = GetConnection();
        connection.Open();
        
        // Query complexa com agrupamento temporal e funções de agregação
        var sql = @"
            SELECT 
                strftime('%Y-%m', DataPedido) as MesAno,
                COUNT(*) as QuantidadePedidos,
                SUM(ValorTotal) as FaturamentoMensal,
                AVG(ValorTotal) as TicketMedio
            FROM Pedidos 
            WHERE Status != @statusCancelado
            GROUP BY strftime('%Y-%m', DataPedido)
            ORDER BY MesAno DESC";
        
        using var command = new SQLiteCommand(sql, connection);
        command.Parameters.AddWithValue("@statusCancelado", (int)StatusPedido.Cancelado);
        using var reader = command.ExecuteReader();
        
        // Cabeçalho da tabela formatada para exibição
        Console.WriteLine(new string('-', 80));
        Console.WriteLine("| {0,-10} | {1,-8} | {2,-15} | {3,-12} | {4,-10} |", 
            "Mês/Ano", "Pedidos", "Faturamento", "Ticket Médio", "Crescimento");
        Console.WriteLine(new string('-', 80));
        
        decimal faturamentoAnterior = 0;
        bool primeiraLinha = true;
        
        // Processar dados calculando crescimento percentual
        while (reader.Read())
        {
            var faturamento = Convert.ToDecimal(reader["FaturamentoMensal"]);
            
            // Calcular percentual de crescimento em relação ao mês anterior
            var crescimento = primeiraLinha ? 0 : 
                faturamentoAnterior == 0 ? 0 : 
                ((faturamento - faturamentoAnterior) / faturamentoAnterior) * 100;
            
            Console.WriteLine("| {0,-10} | {1,-8} | {2,-15:C} | {3,-12:C} | {4,-10:F1}% |",
                reader["MesAno"],
                reader["QuantidadePedidos"],
                faturamento,
                Convert.ToDecimal(reader["TicketMedio"]),
                crescimento);
            
            faturamentoAnterior = faturamento;
            primeiraLinha = false;
        }
        Console.WriteLine(new string('-', 80));
    }

    #endregion

    #region Utilidades e Testes

    // ================================================================
    // UTILIDADES - TESTES DE CONECTIVIDADE E INFORMAÇÕES DO BANCO
    // ================================================================

    /// <summary>
    /// Testa a conectividade com o banco de dados SQLite
    /// Executa queries simples para validar o funcionamento
    /// Exibe informações detalhadas sobre o banco de dados
    /// </summary>
    public void TestarConexao()
    {
        Console.WriteLine("=== TESTE DE CONEXÃO ===");
        
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            Console.WriteLine("✓ Conexão estabelecida com sucesso!");
            
            // Query simples para testar funcionalidade básica
            var sql = "SELECT COUNT(*) as TotalTabelas FROM sqlite_master WHERE type='table'";
            using var command = new SQLiteCommand(sql, connection);
            var totalTabelas = command.ExecuteScalar();
            
            Console.WriteLine($"✓ Query executada com sucesso!");
            Console.WriteLine($"✓ Banco de dados contém {totalTabelas} tabelas");
            
            // Informações detalhadas sobre o conteúdo do banco
            var sqlInfo = @"
                SELECT 
                    (SELECT COUNT(*) FROM Categorias) as TotalCategorias,
                    (SELECT COUNT(*) FROM Produtos) as TotalProdutos,
                    (SELECT COUNT(*) FROM Clientes) as TotalClientes,
                    (SELECT COUNT(*) FROM Pedidos) as TotalPedidos,
                    (SELECT COUNT(*) FROM PedidoItens) as TotalItens";
            
            using var commandInfo = new SQLiteCommand(sqlInfo, connection);
            using var reader = commandInfo.ExecuteReader();
            
            if (reader.Read())
            {
                Console.WriteLine("\nInformações do banco:");
                Console.WriteLine($"- Categorias: {reader["TotalCategorias"]}");
                Console.WriteLine($"- Produtos: {reader["TotalProdutos"]}");
                Console.WriteLine($"- Clientes: {reader["TotalClientes"]}");
                Console.WriteLine($"- Pedidos: {reader["TotalPedidos"]}");
                Console.WriteLine($"- Itens de pedido: {reader["TotalItens"]}");
            }
            
            Console.WriteLine("✓ Teste de conexão concluído com sucesso!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Erro na conexão: {ex.Message}");
        }
    }

    #endregion
}