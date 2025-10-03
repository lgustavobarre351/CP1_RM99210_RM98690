# 🛒 Sistema de Loja - CheckPoint 1 - 3ESPY

![.NET](https://img.shields.io/badge/.NET-9.0-blue?style=for-the-badge&logo=dotnet)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework-Core-purple?style=for-the-badge&logo=microsoft)
![SQLite](https://img.shields.io/badge/SQLite-Database-green?style=for-the-badge&logo=sqlite)
![ADO.NET](https://img.shields.io/badge/ADO.NET-Data%20Access-orange?style=for-the-badge&logo=microsoft)


## 👥 Integrantes | 3ESPY

- **Julia Azevedo Lins**: RM98690
- **Luís Gustavo Barreto Garrido**: RM99210

---

## 📋 Sobre o Projeto

Sistema completo de gerenciamento de loja desenvolvido em **C# .NET 9.0** utilizando **Entity Framework Core** e **ADO.NET** para demonstrar diferentes abordagens de acesso a dados. O projeto implementa um sistema robusto de e-commerce com funcionalidades completas de CRUD, relatórios avançados e análises de performance.

## 🏗️ Arquitetura

```
📁 CheckPoint1/
├── 📁 Models/           # Entidades do domínio
├── 📁 Context/          # Configuração do Entity Framework
├── 📁 Services/         # Camada de serviços
├── 📁 Enums/           # Enumerações
├── 📁 Migrations/      # Migrações do banco
└── 📄 Program.cs       # Ponto de entrada da aplicação
```

## 🎯 Funcionalidades Principais

### 🔧 Entity Framework Service
- ✅ **CRUD Completo**: Categorias, Produtos, Clientes e Pedidos
- ✅ **Validações Avançadas**: Email único, CPF, estoque disponível
- ✅ **Relacionamentos**: Include para carregar entidades relacionadas
- ✅ **Gestão de Pedidos**: Criação completa, mudança de status, cancelamento
- ✅ **Consultas LINQ**: Produtos mais vendidos, análise de vendas, relatórios gerenciais
- ✅ **Dashboard Executivo**: Métricas de negócio em tempo real

### ⚡ ADO.NET Service
- ✅ **Consultas Complexas**: JOINs de múltiplas tabelas
- ✅ **Operações em Lote**: Atualização de estoque por categoria
- ✅ **Transações**: Inserção de pedidos completos com rollback
- ✅ **Análise de Performance**: Vendas mensais com crescimento percentual
- ✅ **Manutenção**: Exclusão de dados antigos, processamento de devoluções

### 🗄️ Banco de Dados
- ✅ **SQLite**: Banco local `loja.db`
- ✅ **Relacionamentos**: Configurados com cascade e restrict
- ✅ **Índices Únicos**: Email de cliente e número de pedido
- ✅ **Seed Data**: Dados iniciais para teste e demonstração

## 🚀 Como Executar

### Pré-requisitos
- .NET 9.0 SDK
- Visual Studio 2022 ou VS Code

### Passos
1. **Clone o repositório**
   ```bash
   git clone <seu-repositorio>
   cd CheckPoint1
   ```

2. **Restaurar dependências**
   ```bash
   dotnet restore
   ```

3. **Executar migrações**
   ```bash
   dotnet ef database update
   ```

4. **Executar aplicação**
   ```bash
   dotnet run
   ```

## 📊 Demonstração de Funcionalidades

### Entity Framework - Consultas LINQ Avançadas
```csharp
// Produtos mais vendidos com categoria
var produtosMaisVendidos = _context.PedidoItens
    .Include(pi => pi.Produto)
        .ThenInclude(p => p.Categoria)
    .GroupBy(pi => new { pi.ProdutoId, pi.Produto.Nome, CategoriaNome = pi.Produto.Categoria.Nome })
    .Select(g => new
    {
        ProdutoId = g.Key.ProdutoId,
        NomeProduto = g.Key.Nome,
        Categoria = g.Key.CategoriaNome,
        QuantidadeVendida = g.Sum(pi => pi.Quantidade),
        ValorTotal = g.Sum(pi => pi.Quantidade * pi.PrecoUnitario)
    })
    .OrderByDescending(x => x.QuantidadeVendida)
    .Take(10);
```

### ADO.NET - Query Complexa com JOINs
```sql
SELECT p.NumeroPedido, c.Nome as NomeCliente, pr.Nome as NomeProduto, 
       pi.Quantidade, pi.PrecoUnitario, 
       (pi.Quantidade * pi.PrecoUnitario) as Subtotal
FROM Pedidos p
INNER JOIN Clientes c ON p.ClienteId = c.Id
INNER JOIN PedidoItens pi ON p.Id = pi.PedidoId
INNER JOIN Produtos pr ON pi.ProdutoId = pr.Id
ORDER BY p.DataPedido DESC
```

## 🎨 Interface do Console

O sistema possui uma interface de console bem estruturada com:
- 📋 **Menus organizados** por funcionalidade
- 📊 **Tabelas formatadas** para exibição de dados
- ✅ **Validações em tempo real**
- 🎯 **Feedback claro** para o usuário

## 🧪 Dados de Teste

O sistema inclui dados iniciais para demonstração:
- **3 Categorias**: Eletrônicos, Roupas, Livros
- **6 Produtos**: Incluindo produtos com estoque zero
- **2 Clientes**: Com dados completos
- **2 Pedidos**: Em diferentes status
- **4 Itens**: Distribuídos entre os pedidos

## 🏆 Destaques Técnicos

### 🔒 Segurança e Integridade
- **Transações** para operações críticas
- **Validação de dados** em múltiplas camadas
- **Relacionamentos** com políticas adequadas de exclusão
- **Parameterização** de queries para prevenir SQL injection

### ⚡ Performance
- **Índices únicos** em campos críticos
- **Include otimizado** para carregamento de relacionamentos
- **Queries eficientes** com agregações no banco
- **Paginação** em listagens extensas

### 🧪 Qualidade de Código
- **Separation of Concerns** com services especializados
- **Exception handling** robusto
- **Código limpo** e bem documentado
- **Padrões consistentes** em toda aplicação

## 📚 Requisitos Originais do Professor

### 🎯 Instruções Gerais
- ✅ Projeto entregue **sem erros de compilação**
- ✅ **Não alteração** das classes na pasta Models
- ✅ **Implementação completa** das classes EntityFrameworkService.cs e AdoNetService.cs
- ✅ **Seguimento** de todas as instruções marcadas com "TODO:"
- ✅ **Entrega via GitHub** conforme solicitado

### 📋 TODO Lists Implementados

<details>
<summary>🔍 EntityFrameworkService - Requisitos Completos</summary>

#### 📂 Categorias
- ✅ Implementar criação de categoria
- ✅ Implementar listagem com contagem de produtos

#### 📦 Produtos
- ✅ Implementar criação de produto
  - ✅ Mostrar categorias disponíveis para o usuário escolher
  - ✅ Validar se a categoria existe
- ✅ Implementar listagem com categoria
  - ✅ Usar Include para carregar categoria
- ✅ Implementar atualização de produto

#### 👤 Clientes
- ✅ Implementar criação de cliente
  - ✅ Validar email único
  - ✅ Validar CPF (formato - apenas números)
- ✅ Implementar listagem com contagem de pedidos
- ✅ Implementar atualização de cliente

#### 🛒 Pedidos
- ✅ Implementar criação de pedido completo
  - ✅ Pedir o ID do cliente
  - ✅ Gerar número do pedido automático
  - ✅ Permitir adicionar múltiplos itens
  - ✅ Calcular valor total automaticamente
  - ✅ Validar estoque disponível
- ✅ Implementar listagem com cliente e itens
  - ✅ Usar Include para carregar Cliente e Itens
  - ✅ Incluir Produtos nos itens
- ✅ Implementar mudança de status
  - ✅ Mostrar status disponíveis
  - ✅ Validar transições válidas
- ✅ Implementar cancelamento de pedido
  - ✅ Pedir ID do pedido
  - ✅ Validar se o pedido existe
  - ✅ Só permitir cancelar se status = **Pendente** ou **Confirmado**
  - ✅ Devolver estoque dos produtos

#### 🔍 Consultas LINQ Avançadas
- ✅ ProdutosMaisVendidos
  - ✅ Agrupar por produto
  - ✅ Somar quantidades vendidas
  - ✅ Ordenar por quantidade decrescente
  - ✅ Incluir nome do produto e categoria
- ✅ ClientesComMaisPedidos
  - ✅ Agrupar por cliente
  - ✅ Contar pedidos
  - ✅ Ordenar por quantidade decrescente
- ✅ FaturamentoPorCategoria
  - ✅ Agrupar por categoria
  - ✅ Calcular valor total vendido
  - ✅ Contar produtos vendidos
  - ✅ Calcular ticket médio
- ✅ PedidosPorPeriodo
  - ✅ Solicitar data início e fim
  - ✅ Filtrar pedidos no período
  - ✅ Agrupar por data
  - ✅ Calcular totais
- ✅ ProdutosEstoqueBaixo
  - ✅ Filtrar produtos com estoque < 20
  - ✅ Incluir categoria
  - ✅ Ordenar por estoque crescente
- ✅ AnaliseVendasMensal
  - ✅ Agrupar vendas por mês/ano
  - ✅ Calcular quantidade vendida e faturamento
  - ✅ Comparar com mês anterior
- ✅ TopClientesPorValor
  - ✅ Somar valor total de pedidos por cliente
  - ✅ Ordenar por valor decrescente
  - ✅ Mostrar top 10

#### 📊 Relatórios Gerais
- ✅ DashboardExecutivo
  - ✅ Quantidade de pedidos
  - ✅ Ticket médio
  - ✅ Produtos em estoque
  - ✅ Clientes ativos
  - ✅ Faturamento mensal
- ✅ RelatorioEstoque
  - ✅ Produtos por categoria
  - ✅ Valor total em estoque
  - ✅ Produtos zerados
  - ✅ Produtos em estoque baixo
- ✅ AnaliseClientes
  - ✅ Clientes por estado
  - ✅ Valor médio por cliente
</details>

<details>
<summary>⚡ AdoNetService - Requisitos Completos</summary>

#### 🔗 Conexão
- ✅ Implementar connection string para SQLite
  - ✅ Usar o mesmo arquivo **"loja.db"** criado pelo EF

#### 📊 Consultas Complexas
- ✅ RelatorioVendasCompleto
  - ✅ SELECT com JOIN de 4 tabelas: Pedido, Cliente, PedidoItem, Produto
  - ✅ Mostrar: NumeroPedido, NomeCliente, NomeProduto, Quantidade, PrecoUnitario, Subtotal
  - ✅ Agrupar por pedido
  - ✅ Ordenar por data do pedido
- ✅ FaturamentoPorCliente
  - ✅ Agrupar por cliente
  - ✅ Calcular valor total de pedidos
  - ✅ Contar quantidade de pedidos
  - ✅ Calcular ticket médio
  - ✅ Ordenar por faturamento decrescente
- ✅ ProdutosSemVenda
  - ✅ Consulta com LEFT JOIN e IS NULL
  - ✅ Identificar produtos que nunca foram vendidos
  - ✅ Mostrar categoria, nome, preço e estoque
  - ✅ Calcular valor parado em estoque

#### 🛠️ Operações de Dados
- ✅ AtualizarEstoqueLote
  - ✅ Solicitar categoria e percentual de ajuste
  - ✅ Atualizar estoque de todos produtos da categoria
  - ✅ Para cada produto, perguntar nova quantidade
  - ✅ Exibir quantos registros foram afetados
- ✅ InserirPedidoCompleto
  - ✅ Inserir pedido master (tabela Pedidos)
  - ✅ Inserir múltiplos itens no pedido
  - ✅ Atualizar estoque dos produtos
  - ✅ Validar estoque antes de inserir item
- ✅ ExcluirDadosAntigos
  - ✅ Excluir pedidos cancelados há mais de 6 meses
- ✅ ProcessarDevolucao
  - ✅ Localizar pedido e itens
  - ✅ Validar se pode devolver
  - ✅ Devolver estoque (aumentar estoque dos produtos conforme itens devolvidos)

#### 📈 Análises de Performance
- ✅ AnalisarPerformanceVendas
  - ✅ Calcular vendas mensais
  - ✅ Calcular crescimento percentual

#### 🧪 Utilidades
- ✅ TestarConexao
  - ✅ Tentar conectar ao banco
  - ✅ Executar query simples
  - ✅ Mostrar informações do banco
</details>

<details>
<summary>🗄️ CheckpointContext - Requisitos Completos</summary>

#### 🗂️ DbSets
- ✅ Implementar todos os DbSets
  - ✅ DbSet<Categoria> Categorias
  - ✅ DbSet<Produto> Produtos
  - ✅ DbSet<Cliente> Clientes
  - ✅ DbSet<Pedido> Pedidos
  - ✅ DbSet<PedidoItem> PedidoItens

#### 🔗 Configuração de Conexão
- ✅ Configurar conexão com SQLite
  - ✅ Usar o arquivo **"loja.db"**

#### ⚙️ Relacionamentos
- ✅ Categoria -> Produtos
  - ✅ Produto pertence a uma categoria (required)
  - ✅ Categoria pode ter muitos produtos
  - ✅ Configurar cascade delete
- ✅ Cliente -> Pedidos
  - ✅ Pedido pertence a um cliente (required)
  - ✅ Cliente pode ter muitos pedidos
  - ✅ Configurar cascade delete
- ✅ Pedido -> PedidoItens
  - ✅ Item pertence a um pedido (required)
  - ✅ Pedido pode ter muitos itens
  - ✅ Configurar cascade delete
- ✅ Produto -> PedidoItens
  - ✅ Item referencia um produto (required)
  - ✅ Produto pode estar em muitos itens
  - ✅ Configurar restrict delete (não pode excluir produto que tem vendas)

#### 🗃️ Índices
- ✅ Configurar índices únicos
  - ✅ Cliente.Email deve ser único
  - ✅ Pedido.NumeroPedido deve ser único

#### 🧪 Dados Iniciais (Seed)
- ✅ Adicionar dados iniciais
  - ✅ 3 categorias
  - ✅ 6 produtos (2 por categoria, incluindo produtos com estoque zero)
  - ✅ 2 clientes
  - ✅ 2 pedidos (1 por cliente)
  - ✅ 4 itens de pedido (2 por pedido)
</details>

---

## 📜 Licença

Este projeto foi desenvolvido para fins educacionais como parte do CheckPoint 1 da disciplina de C#

---


