namespace Conde.Fatoracao;

/// <summary>
/// As TABELAS que o medidor imprime e que os testes conferem.
///
/// Elas ficam aqui, e nao dentro do programa do medidor, por um motivo so: numero
/// em README que ninguem confere envelhece errado em silencio. Alguem troca uma
/// constante, a tabela muda, e o texto continua afirmando a frase antiga. Aqui o
/// teste afirma sobre o MESMO objeto que vira linha impressa.
/// </summary>
public static class Medidas
{
    public record LinhaDefinicao(
        string Numero, ulong Valor, string Fatores, long ContasParaAchar, long ContasParaConferir, bool EstaCerta);

    /// <summary>
    /// A fatoracao contra a DEFINICAO, nos numeros conhecidos.
    ///
    /// As duas colunas de contas sao o assunto do repositorio: uma e o custo de
    /// ACHAR, a outra o de CONFERIR, e a razao entre elas cresce com o tamanho.
    /// </summary>
    public static List<LinhaDefinicao> ContraADefinicao()
    {
        var linhas = new List<LinhaDefinicao>();

        foreach (var (nome, n) in Numeros.Conhecidos())
        {
            var r = Metodos.PorDivisao(n);
            var c = Juiz.Conferir(n, r.Fatores);

            linhas.Add(new LinhaDefinicao(
                nome, n,
                r.Fatores.Count == 0 ? "(nenhum)" : string.Join(" x ", r.Fatores),
                r.Contas, Juiz.CustoDeConferirRapido(r.Fatores), c.EstaCerta));
        }

        return linhas;
    }

    public record LinhaAssimetria(int Bits, ulong Numero, long Achar, long Conferir, double Razao);

    /// <summary>
    /// A ASSIMETRIA entre achar e conferir, em semiprimos de tamanho crescente.
    ///
    /// E a tabela mais importante do repositorio. O custo de achar cresce como a
    /// raiz do numero; o de conferir cresce com o numero de BITS. Com quarenta e
    /// oito bits, achar custa dezesseis milhoes de contas e conferir custa
    /// trezentas.
    /// </summary>
    public static List<LinhaAssimetria> Assimetria()
    {
        var linhas = new List<LinhaAssimetria>();

        foreach (var bits in new[] { 8, 10, 12, 14, 16, 18, 20, 22, 24 })
        {
            var n = Numeros.Semiprimo(bits, (ulong)bits);
            var r = Metodos.PorDivisao(n);
            var conferir = Juiz.CustoDeConferirRapido(r.Fatores);

            linhas.Add(new LinhaAssimetria(2 * bits, n, r.Contas, conferir, (double)r.Contas / conferir));
        }

        return linhas;
    }

    public record LinhaMetodo(
        string Caso, ulong Numero, long PorDivisao, long DeFermat, long Rho, bool FermatDesistiu, bool Batem);

    /// <summary>
    /// Os tres metodos no mesmo numero, em contas.
    ///
    /// Cada um e otimo num caso e inutil em outro, e nenhum deles sabe de antemao em
    /// qual caso esta: a propriedade que decide, que e a distancia entre os fatores,
    /// so e conhecida depois de fatorar.
    /// </summary>
    public static List<LinhaMetodo> PorMetodo()
    {
        var linhas = new List<LinhaMetodo>();

        foreach (var (caso, n) in new (string, ulong)[]
        {
            ("muitos primos pequenos", Numeros.MuitosPequenos(8, 3)),
            ("3 vezes um primo de 40 bits", 3 * Numeros.PrimoPerto(1UL << 39, 23)),
            ("5 vezes um primo de 44 bits", 5 * Numeros.PrimoPerto(1UL << 43, 29)),
            ("2^40", Numeros.PotenciaDePrimo(2, 40)),
            ("3^20", Numeros.PotenciaDePrimo(3, 20)),
            ("semiprimo colado de 16 bits", Numeros.SemiprimoColado(16, 5)),
            ("semiprimo colado de 24 bits", Numeros.SemiprimoColado(24, 7)),
            ("semiprimo de 12 bits", Numeros.Semiprimo(12, 11)),
            ("semiprimo de 16 bits", Numeros.Semiprimo(16, 13)),
            ("semiprimo de 20 bits", Numeros.Semiprimo(20, 17)),
            ("semiprimo de 24 bits", Numeros.Semiprimo(24, 19)),
        })
        {
            var divisao = Metodos.PorDivisao(n);
            var fermat = Metodos.DeFermat(n, 2_000_000);
            var rho = Metodos.Rho(n);

            linhas.Add(new LinhaMetodo(
                caso, n, divisao.Contas, fermat.Contas, rho.Contas,
                !fermat.Terminou, Juiz.Mesma(divisao.Fatores, rho.Fatores)));
        }

        return linhas;
    }

    public record LinhaRho(int Bits, ulong Numero, long PorDivisao, long Rho, double RazaoContraRaizQuarta);

    /// <summary>
    /// O RHO contra a DIVISAO, em semiprimos de tamanho crescente.
    ///
    /// A conta preve que o rho custe a raiz QUARTA do numero e a divisao custe a
    /// raiz. A ultima coluna e o custo do rho dividido pela raiz quarta, e ela
    /// precisa ficar mais ou menos parada enquanto o numero cresce.
    /// </summary>
    public static List<LinhaRho> ORho()
    {
        var linhas = new List<LinhaRho>();

        foreach (var bits in new[] { 10, 12, 14, 16, 18, 20, 22, 24, 26 })
        {
            var n = Numeros.Semiprimo(bits, (ulong)(bits * 3));
            var divisao = Metodos.PorDivisao(n);
            var rho = Metodos.Rho(n);
            var raizQuarta = Math.Pow(n, 0.25);

            linhas.Add(new LinhaRho(2 * bits, n, divisao.Contas, rho.Contas, rho.Contas / raizQuarta));
        }

        return linhas;
    }

    public record LinhaCarmichael(ulong Numero, string Fatores, bool FermatBase2, bool FermatBase3, bool MillerRabin);

    /// <summary>
    /// Os numeros de CARMICHAEL contra os dois testes de primalidade.
    ///
    /// Eles sao compostos, passam pelo teste de Fermat em toda base coprima, e nao
    /// passam por Miller e Rabin. A coluna do meio e a que mostra por que o teste de
    /// Fermat nao serve.
    /// </summary>
    public static List<LinhaCarmichael> Carmichael(ulong ate = 100_000)
    {
        var linhas = new List<LinhaCarmichael>();

        foreach (var n in Numeros.Carmichael(ate))
            linhas.Add(new LinhaCarmichael(
                n, string.Join(" x ", Juiz.Certa(n)),
                Primalidade.DeFermat(n, 2), Primalidade.DeFermat(n, 3), Primalidade.DeMillerRabin(n)));

        return linhas;
    }

    public record LinhaTestes(ulong Ate, int Primos, int FermatErrou, int MillerRabinErrou, long CustoDivisao, long CustoMillerRabin);

    /// <summary>
    /// Os testes de primalidade contra a DEFINICAO, em TODO numero ate um limite.
    ///
    /// Nao e amostra: e todo numero. Miller e Rabin com as doze bases primeiras nao
    /// erra nenhum, e isso nao e probabilidade: e um resultado conferido
    /// exaustivamente aqui ate cem mil e publicado para sessenta e quatro bits.
    /// </summary>
    public static List<LinhaTestes> OsTestes(params ulong[] limites)
    {
        if (limites.Length == 0) limites = [1_000, 10_000, 100_000];

        var linhas = new List<LinhaTestes>();

        foreach (var ate in limites)
        {
            var primos = 0;
            var fermatErrou = 0;
            var millerErrou = 0;
            var custoDivisao = 0L;
            var custoMiller = 0L;

            for (var n = 2UL; n <= ate; n++)
            {
                var certo = Primalidade.PorDivisao(n);

                if (certo) primos++;
                if (Primalidade.DeFermat(n, 2) != certo) fermatErrou++;
                if (Primalidade.DeMillerRabin(n) != certo) millerErrou++;

                custoDivisao += Primalidade.CustoPorDivisao(n);
                custoMiller += Primalidade.CustoDeMillerRabin(n);
            }

            linhas.Add(new LinhaTestes(ate, primos, fermatErrou, millerErrou, custoDivisao, custoMiller));
        }

        return linhas;
    }
}
