using Conde.Fatoracao;
using Xunit;

namespace Conde.Fatoracao.Testes;

public class PrimalidadeTestes
{
    /// <summary>
    /// MILLER e RABIN com as doze primeiras bases primas nao erra NENHUM numero ate
    /// cem mil.
    ///
    /// Nao e amostra: e todo numero. E nao e probabilidade: com essas bases o teste
    /// e DETERMINISTICO ate sessenta e quatro bits, e isso e um resultado conferido
    /// exaustivamente e publicado. Aqui ele e conferido ate cem mil contra a
    /// definicao.
    /// </summary>
    [Fact]
    public void MillerRabinNaoErraNenhumNumeroAteCemMil()
    {
        for (var n = 2UL; n <= 100_000; n++)
            Assert.Equal(Primalidade.PorDivisao(n), Primalidade.DeMillerRabin(n));
    }

    /// <summary>
    /// E ele tambem acerta os primos GRANDES, onde a definicao ainda chega.
    /// </summary>
    [Fact]
    public void MillerRabinAcertaOsPrimosGrandes()
    {
        foreach (var n in new ulong[]
        {
            2_147_483_647,          // o primo de Mersenne de 31 bits
            1_000_000_007,
            1_000_000_009,
            4_294_967_291,
            999_999_000_001,
        })
            Assert.True(Primalidade.DeMillerRabin(n), $"{n} e primo");

        foreach (var n in new ulong[]
        {
            2_147_483_647UL * 3,
            1_000_000_007UL * 1_000_000_009,
            4_294_967_291UL * 2,
        })
            Assert.False(Primalidade.DeMillerRabin(n), $"{n} e composto");
    }

    /// <summary>
    /// O ACHADO PRINCIPAL: o teste de FERMAT erra, e os numeros de CARMICHAEL sao
    /// onde ele erra sempre.
    ///
    /// Eles sao compostos e passam pelo teste em TODA base coprima com eles. O
    /// primeiro e 561, que e 3 vezes 11 vezes 17, e ate cem mil ha dezesseis deles.
    ///
    /// E nao e um erro que mais bases consertam: a base so ajuda quando ela
    /// compartilha fator com o numero, e achar uma base assim ja e fatorar.
    /// </summary>
    [Fact]
    public void OTesteDeFermatErraEmTodoCarmichael()
    {
        var carmichael = Numeros.Carmichael(100_000);

        Assert.Equal(16, carmichael.Count);

        foreach (var n in carmichael)
        {
            Assert.False(Primalidade.PorDivisao(n), $"{n} e composto");
            Assert.True(Primalidade.DeFermat(n, 2), $"{n} engana Fermat na base 2");
            Assert.False(Primalidade.DeMillerRabin(n), $"{n} nao engana Miller e Rabin");
        }
    }

    /// <summary>
    /// E ele engana em QUASE TODA base: as unicas que o pegam sao as que
    /// compartilham fator com ele.
    ///
    /// No 561, que e 3 vezes 11 vezes 17, as bases que o pegam sao os multiplos de
    /// tres, de onze e de dezessete. Todas as outras passam.
    /// </summary>
    [Fact]
    public void OCarmichaelEnganaFermatEmQuaseTodaBase()
    {
        const ulong n = 561;
        var enganou = 0;
        var pegou = 0;

        for (var a = 2UL; a < n; a++)
        {
            if (Modular.Mdc(a, n) != 1) { pegou++; continue; }

            if (Primalidade.DeFermat(n, a)) enganou++;
        }

        // toda base coprima passa, sem excecao
        Assert.Equal(320 - 1, enganou);
        Assert.Equal(n - 2 - (ulong)enganou, (ulong)pegou);
    }

    /// <summary>
    /// E o teste de Fermat erra tambem FORA dos Carmichael: ha compostos que passam
    /// na base dois e nao sao de Carmichael.
    ///
    /// Esse teste existe para ninguem achar que "evitar Carmichael" conserta o
    /// teste. Ate cem mil sao setenta e oito erros na base dois, e so dezesseis
    /// deles sao de Carmichael.
    /// </summary>
    [Fact]
    public void OTesteDeFermatErraForaDosCarmichaelTambem()
    {
        var errou = 0;

        for (var n = 2UL; n <= 100_000; n++)
            if (Primalidade.DeFermat(n, 2) != Primalidade.PorDivisao(n)) errou++;

        Assert.Equal(78, errou);
        Assert.True(errou > Numeros.Carmichael(100_000).Count);
    }

    /// <summary>
    /// A DEFINICAO acerta os casos de contorno, que e onde um teste de primalidade
    /// costuma errar sem dar sinal.
    /// </summary>
    [Fact]
    public void ADefinicaoAcertaOsCasosDeContorno()
    {
        Assert.False(Primalidade.PorDivisao(0));
        Assert.False(Primalidade.PorDivisao(1));
        Assert.True(Primalidade.PorDivisao(2));
        Assert.True(Primalidade.PorDivisao(3));
        Assert.False(Primalidade.PorDivisao(4));
        Assert.False(Primalidade.PorDivisao(9));
        Assert.True(Primalidade.PorDivisao(25 - 2));

        Assert.False(Primalidade.DeMillerRabin(0));
        Assert.False(Primalidade.DeMillerRabin(1));
        Assert.True(Primalidade.DeMillerRabin(2));
        Assert.False(Primalidade.DeMillerRabin(4));
        Assert.False(Primalidade.DeMillerRabin(9));
    }

    /// <summary>
    /// O CUSTO: a definicao custa a raiz do numero e Miller e Rabin custa os bits.
    ///
    /// Com um numero de trinta e dois bits, sao sessenta e cinco mil divisoes contra
    /// quatrocentas multiplicacoes.
    /// </summary>
    [Fact]
    public void OCustoDaDefinicaoCresceComARaizEODeMillerRabinComOsBits()
    {
        var primo = Numeros.PrimoPerto(1UL << 31, 7);

        var divisao = Primalidade.CustoPorDivisao(primo);
        var miller = Primalidade.CustoDeMillerRabin(primo);

        Assert.True(divisao > 20_000, $"a divisao custou {divisao}");
        Assert.True(miller < 500, $"Miller e Rabin custou {miller}");
        Assert.True(divisao > 50 * miller);

        // e o custo da divisao fica na casa da raiz
        Assert.InRange(divisao / Math.Sqrt(primo), 0.3, 0.7);
    }
}
