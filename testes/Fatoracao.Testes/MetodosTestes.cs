using Conde.Fatoracao;
using Xunit;

namespace Conde.Fatoracao.Testes;

public class MetodosTestes
{
    /// <summary>
    /// A DIVISAO POR TENTATIVA acerta TODO numero ate dez mil, conferida pela
    /// definicao de fatoracao: o produto bate e todo fator e primo.
    ///
    /// Nao e amostra: e todo numero.
    /// </summary>
    [Fact]
    public void ADivisaoAcertaTodoNumeroAteDezMil()
    {
        for (var n = 2UL; n <= 10_000; n++)
        {
            var r = Metodos.PorDivisao(n);

            Assert.True(Juiz.Conferir(n, r.Fatores).EstaCerta, $"{n}: {string.Join(" x ", r.Fatores)}");
        }
    }

    /// <summary>
    /// E o RHO acha a MESMA fatoracao que a divisao, em todo numero ate dez mil e
    /// em todo numero armado deste repositorio.
    ///
    /// O rho e sorteado por dentro, e a resposta nao pode depender do sorteio: o
    /// sorteio muda o CUSTO, nunca o resultado.
    /// </summary>
    [Fact]
    public void ORhoAchaAMesmaFatoracaoQueADivisao()
    {
        for (var n = 2UL; n <= 10_000; n++)
            Assert.True(Juiz.Mesma(Metodos.PorDivisao(n).Fatores, Metodos.Rho(n).Fatores), $"{n}");

        foreach (var (nome, n) in Numeros.Conhecidos())
        {
            if (n < 2) continue;

            Assert.True(Juiz.Mesma(Juiz.Certa(n), Metodos.Rho(n).Fatores), nome);
        }
    }

    [Fact]
    public void ORhoNaoDependeDaSemente()
    {
        var n = Numeros.Semiprimo(20, 11);
        var certo = Juiz.Certa(n);

        for (ulong semente = 1; semente <= 20; semente++)
            Assert.True(Juiz.Mesma(certo, Metodos.Rho(n, semente).Fatores), $"semente {semente}");
    }

    /// <summary>
    /// O ACHADO PRINCIPAL: o RHO custa a raiz QUARTA onde a divisao custa a RAIZ.
    ///
    /// Num semiprimo de cinquenta e dois bits, a divisao faz dezessete milhoes de
    /// contas e o rho faz sete mil. Com o numero crescendo mil vezes, a razao entre
    /// o custo do rho e a raiz quarta fica mais ou menos parada.
    /// </summary>
    [Fact]
    public void ORhoCustaARaizQuartaOndeADivisaoCustaARaiz()
    {
        foreach (var bits in new[] { 16, 20, 24 })
        {
            var n = Numeros.Semiprimo(bits, (ulong)(bits * 3));
            var divisao = Metodos.PorDivisao(n).Contas;
            var rho = Metodos.Rho(n).Contas;

            Assert.True(rho < divisao, $"{n}: rho {rho}, divisao {divisao}");
            Assert.InRange(rho / Math.Pow(n, 0.25), 0.05, 4.0);
            Assert.InRange(divisao / Math.Sqrt(n), 0.2, 1.0);
        }
    }

    /// <summary>
    /// O METODO DE FERMAT e instantaneo quando os fatores estao COLADOS e desiste
    /// quando estao LONGE.
    ///
    /// Com tres vezes um primo de quarenta bits, ele nao acha em dois milhoes de
    /// tentativas: o a dele precisaria subir ate quase n sobre dois. O rho acha o
    /// mesmo numero em UMA conta, porque o fator tres sai do primeiro divisor comum.
    ///
    /// E um metodo cujo custo depende de uma propriedade do numero que so e
    /// conhecida DEPOIS de fatorar.
    /// </summary>
    [Fact]
    public void FermatEhInstantaneoNoColadoEDesisteNoLonge()
    {
        var colado = Numeros.SemiprimoColado(24, 7);
        var longe = 3 * Numeros.PrimoPerto(1UL << 39, 23);

        var noColado = Metodos.DeFermat(colado, 2_000_000);
        var noLonge = Metodos.DeFermat(longe, 2_000_000);

        Assert.True(noColado.Terminou);
        Assert.True(noColado.Contas < Metodos.PorDivisao(colado).Contas / 10);

        Assert.False(noLonge.Terminou);
        Assert.Equal(1, Metodos.Rho(longe).Contas);
    }

    /// <summary>
    /// E a divisao que Fermat devolve, quando devolve, MULTIPLICA de volta.
    ///
    /// Ela nao e a fatoracao completa: sao dois fatores cujo produto e n, que podem
    /// ser compostos. O que da para exigir dela e isso, e e isso que o teste exige.
    /// </summary>
    [Fact]
    public void ADivisaoDeFermatMultiplicaDeVolta()
    {
        foreach (var bits in new[] { 12, 16, 20, 24 })
        {
            var n = Numeros.SemiprimoColado(bits, 5);
            var r = Metodos.DeFermat(n);

            Assert.True(r.Terminou);
            Assert.Equal(2, r.Fatores.Count);
            Assert.Equal(n, r.Fatores[0] * r.Fatores[1]);
            Assert.True(r.Fatores[0] > 1 && r.Fatores[1] > 1, "a divisao precisa ser nao trivial");
        }
    }

    /// <summary>
    /// A DIVISAO POR TENTATIVA desiste quando passa do limite, e desistir e devolver
    /// o que ja achou com a bandeira baixada.
    ///
    /// Devolver uma fatoracao incompleta sem dizer que ela e incompleta seria o pior
    /// erro possivel aqui: ela passaria pela metade do juiz, a do produto, so que
    /// com o produto errado.
    /// </summary>
    [Fact]
    public void ADivisaoDesisteComABandeiraBaixada()
    {
        var n = Numeros.Semiprimo(24, 19);
        var r = Metodos.PorDivisao(n, 1_000);

        Assert.False(r.Terminou);
        Assert.False(Juiz.Conferir(n, r.Fatores).EstaCerta);

        // e sem limite ela termina e acerta
        var completa = Metodos.PorDivisao(n);

        Assert.True(completa.Terminou);
        Assert.True(Juiz.Conferir(n, completa.Fatores).EstaCerta);
    }

    [Fact]
    public void OsCasosDeContornoNaoQuebramNada()
    {
        Assert.Empty(Metodos.PorDivisao(1).Fatores);
        Assert.Equal([2UL], Metodos.PorDivisao(2).Fatores);
        Assert.Equal([97UL], Metodos.PorDivisao(97).Fatores);
        Assert.Empty(Metodos.Rho(1).Fatores);
        Assert.Equal([2UL], Metodos.Rho(2).Fatores);
    }

    /// <summary>
    /// A POTENCIA DE PRIMO e o caso que quebra uma versao ingenua do rho: ele acha o
    /// mesmo fator sempre e nao progride.
    ///
    /// A versao daqui trata isso trocando o deslocamento da sequencia a cada
    /// tentativa que falha, e o teste confere que ela chega ao fim.
    /// </summary>
    [Fact]
    public void ORhoResolveAsPotenciasDePrimo()
    {
        foreach (var (p, e) in new (ulong, int)[] { (2, 40), (3, 20), (5, 15), (7, 12), (1009, 3) })
        {
            var n = Numeros.PotenciaDePrimo(p, e);
            var r = Metodos.Rho(n);

            Assert.True(r.Terminou, $"{p}^{e}");
            Assert.True(Juiz.Conferir(n, r.Fatores).EstaCerta, $"{p}^{e}");
            Assert.Equal(e, r.Fatores.Count);
            Assert.All(r.Fatores, f => Assert.Equal(p, f));
        }
    }
}
