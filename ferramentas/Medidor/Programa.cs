using Conde.Fatoracao;

var pedido = args.Length > 0 ? args[0] : "tudo";
var tudo = pedido == "tudo";

if (tudo || pedido == "definicao") Definicao();
if (tudo || pedido == "assimetria") Assimetria();
if (tudo || pedido == "metodo") Metodos();
if (tudo || pedido == "rho") Rhos();
if (tudo || pedido == "carmichael") Carmichaels();
if (tudo || pedido == "primalidade") Primalidades();

static void Definicao()
{
    Console.WriteLine();
    Console.WriteLine("  A FATORACAO CONTRA A DEFINICAO");
    Console.WriteLine();
    Console.WriteLine("  numero                             valor   fatores                       contas para achar   contas para conferir   esta certa");

    foreach (var l in Medidas.ContraADefinicao())
        Console.WriteLine($"  {l.Numero,-28} {l.Valor,14:N0}   {l.Fatores,-28} {l.ContasParaAchar,17:N0} {l.ContasParaConferir,22:N0} {Marca(l.EstaCerta),12}");

    Console.WriteLine();
    Console.WriteLine("  esta certa quer dizer duas coisas: o produto dos fatores e o numero, e todo");
    Console.WriteLine("  fator e primo. So a primeira aceita [n] para qualquer n");
    Console.WriteLine();
}

static void Assimetria()
{
    Console.WriteLine();
    Console.WriteLine("  A ASSIMETRIA ENTRE ACHAR E CONFERIR, EM SEMIPRIMOS");
    Console.WriteLine();
    Console.WriteLine("  bits                numero   contas para achar   contas para conferir   quantas vezes mais");

    foreach (var l in Medidas.Assimetria())
        Console.WriteLine($"  {l.Bits,4} {l.Numero,21:N0} {l.Achar,19:N0} {l.Conferir,22:N0} {l.Razao,20:F1}");

    Console.WriteLine();
    Console.WriteLine("  achar cresce como a RAIZ do numero; conferir cresce com o numero de BITS.");
    Console.WriteLine("  E dessa diferenca que vive toda a criptografia de chave publica");
    Console.WriteLine();
}

static void Metodos()
{
    Console.WriteLine();
    Console.WriteLine("  OS TRES METODOS NO MESMO NUMERO, EM CONTAS");
    Console.WriteLine();
    Console.WriteLine("  caso                                          numero   por divisao     de Fermat          rho   Fermat desistiu   rho bate");

    foreach (var l in Medidas.PorMetodo())
        Console.WriteLine($"  {l.Caso,-29} {l.Numero,24:N0} {l.PorDivisao,13:N0} {l.DeFermat,13:N0} {l.Rho,12:N0} {Marca(l.FermatDesistiu),17} {Marca(l.Batem),10}");

    Console.WriteLine();
    Console.WriteLine("  cada um e otimo num caso e inutil em outro, e nenhum deles sabe de antemao");
    Console.WriteLine("  em qual caso esta");
    Console.WriteLine();
}

static void Rhos()
{
    Console.WriteLine();
    Console.WriteLine("  O RHO CONTRA A DIVISAO, EM SEMIPRIMOS DE TAMANHO CRESCENTE");
    Console.WriteLine();
    Console.WriteLine("  bits                numero   por divisao          rho   rho sobre a raiz quarta");

    foreach (var l in Medidas.ORho())
        Console.WriteLine($"  {l.Bits,4} {l.Numero,21:N0} {l.PorDivisao,13:N0} {l.Rho,12:N0} {l.RazaoContraRaizQuarta,25:F2}");

    Console.WriteLine();
    Console.WriteLine("  a divisao custa a RAIZ do numero e o rho custa a raiz QUARTA: a ultima");
    Console.WriteLine("  coluna fica mais ou menos parada enquanto o numero cresce mil vezes");
    Console.WriteLine();
}

static void Carmichaels()
{
    var linhas = Medidas.Carmichael();

    Console.WriteLine();
    Console.WriteLine("  OS NUMEROS DE CARMICHAEL ATE CEM MIL, CONTRA OS DOIS TESTES");
    Console.WriteLine();
    Console.WriteLine($"  quantos ha: {linhas.Count}");
    Console.WriteLine($"  quantos enganam o teste de Fermat na base 2: {linhas.Count(l => l.FermatBase2)}");
    Console.WriteLine($"  quantos enganam Miller e Rabin: {linhas.Count(l => l.MillerRabin)}");
    Console.WriteLine();
    Console.WriteLine("  numero   fatores            Fermat base 2   Fermat base 3   Miller e Rabin");

    foreach (var l in linhas.Take(12))
        Console.WriteLine($"  {l.Numero,6:N0}   {l.Fatores,-18} {Marca(l.FermatBase2),13} {Marca(l.FermatBase3),15} {Marca(l.MillerRabin),16}");

    Console.WriteLine();
    Console.WriteLine("  eles sao COMPOSTOS e passam pelo teste de Fermat em toda base coprima: e por");
    Console.WriteLine("  isso que esse teste nao serve, e e por isso que Miller e Rabin existem");
    Console.WriteLine();
}

static void Primalidades()
{
    Console.WriteLine();
    Console.WriteLine("  OS TESTES DE PRIMALIDADE CONTRA A DEFINICAO, EM TODO NUMERO ATE O LIMITE");
    Console.WriteLine();
    Console.WriteLine("     ate   primos   Fermat errou   Miller e Rabin errou   contas da divisao   contas de Miller e Rabin");

    foreach (var l in Medidas.OsTestes())
        Console.WriteLine($"  {l.Ate,6:N0} {l.Primos,8:N0} {l.FermatErrou,14:N0} {l.MillerRabinErrou,22:N0} {l.CustoDivisao,19:N0} {l.CustoMillerRabin,26:N0}");

    Console.WriteLine();
    Console.WriteLine("  nao e amostra: e TODO numero ate o limite. E a coluna de Miller e Rabin e");
    Console.WriteLine("  zero, o que nao e probabilidade: e conferencia exaustiva");
    Console.WriteLine();
}

static string Marca(bool valor) => valor ? "sim" : "NAO";
