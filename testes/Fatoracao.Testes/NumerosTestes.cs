using Conde.Fatoracao;
using Xunit;

namespace Conde.Fatoracao.Testes;

public class NumerosTestes
{
    [Fact]
    public void OSorteioEhDeterminista()
    {
        var a = new Sorteio(7);
        var b = new Sorteio(7);

        for (var i = 0; i < 1000; i++) Assert.Equal(a.Proximo(), b.Proximo());
    }

    /// <summary>
    /// A fracao precisa cobrir os DOIS LADOS do meio: um gerador que so devolvesse
    /// a metade de baixo faria o rho comecar sempre perto de zero, e o custo medido
    /// deixaria de ser o custo do metodo.
    /// </summary>
    [Fact]
    public void AFracaoCobreOsDoisLadosDoMeio()
    {
        var sorteio = new Sorteio(7);
        var acima = 0;
        var maior = 0.0;

        for (var i = 0; i < 10_000; i++)
        {
            var f = sorteio.Fracao();

            Assert.InRange(f, 0.0, 1.0);
            if (f > 0.5) acima++;
            if (f > maior) maior = f;
        }

        Assert.InRange(acima, 4_700, 5_300);
        Assert.True(maior > 0.999, $"o maior sorteio foi {maior}");
    }

    /// <summary>
    /// A MULTIPLICACAO MODULAR nao estoura, e esse e o unico jeito de ela estar
    /// certa com sessenta e quatro bits.
    ///
    /// Fazer a conta com double, que e o atalho que todo mundo tenta, perde os bits
    /// de baixo e devolve um resto errado sem dar erro nenhum.
    /// </summary>
    [Fact]
    public void AMultiplicacaoModularNaoEstoura()
    {
        var grande = (1UL << 62) + 12_345;

        Assert.Equal(0UL, Modular.Vezes(grande, grande, 1));
        Assert.Equal(6UL, Modular.Vezes(2, 3, 7));

        // a conta certa, feita com cento e vinte e oito bits
        Assert.Equal((ulong)((UInt128)grande * grande % 1_000_000_007UL), Modular.Vezes(grande, grande, 1_000_000_007UL));

        // e o atalho com double erra
        var pelodouble = (ulong)((double)grande * grande % 1_000_000_007.0);

        Assert.NotEqual(Modular.Vezes(grande, grande, 1_000_000_007UL), pelodouble);
    }

    [Fact]
    public void APotenciaModularBateComAContaDireta()
    {
        for (var a = 2UL; a <= 10; a++)
            for (var e = 0UL; e <= 12; e++)
                for (var m = 2UL; m <= 15; m++)
                {
                    var direto = 1UL;

                    for (var i = 0UL; i < e; i++) direto = direto * a % m;

                    Assert.Equal(direto, Modular.Potencia(a, e, m));
                }
    }

    [Fact]
    public void OMdcEhOQueONomeDiz()
    {
        Assert.Equal(6UL, Modular.Mdc(12, 18));
        Assert.Equal(1UL, Modular.Mdc(13, 17));
        Assert.Equal(5UL, Modular.Mdc(0, 5));
        Assert.Equal(7UL, Modular.Mdc(7, 7));
    }

    /// <summary>
    /// O CRIVO acha os mesmos primos que a divisao por tentativa, ate dez mil.
    ///
    /// Sao dois caminhos diferentes para a mesma lista: um risca multiplos, o outro
    /// testa divisores. Quando duas listas independentes batem, o que esta certo
    /// nao e a implementacao.
    /// </summary>
    [Fact]
    public void OCrivoBateComADivisao()
    {
        var doCrivo = Numeros.Primos(10_000).ToHashSet();

        for (var n = 2UL; n <= 10_000; n++)
            Assert.Equal(Primalidade.PorDivisao(n), doCrivo.Contains(n));
    }

    /// <summary>
    /// O SEMIPRIMO gerado e mesmo o produto de dois primos, e os dois sao do
    /// tamanho pedido.
    ///
    /// Se o gerador devolvesse um numero com tres fatores, a tabela do rho estaria
    /// medindo outra coisa: o rho acha o MENOR fator, e com tres fatores o menor e
    /// bem menor que a raiz quarta.
    /// </summary>
    [Fact]
    public void OSemiprimoEhMesmoOProdutoDeDoisPrimos()
    {
        foreach (var bits in new[] { 8, 12, 16, 20 })
            for (ulong semente = 1; semente <= 10; semente++)
            {
                var n = Numeros.Semiprimo(bits, semente);
                var fatores = Juiz.Certa(n);

                Assert.Equal(2, fatores.Count);
                Assert.All(fatores, f => Assert.True(Primalidade.PorDivisao(f)));
                Assert.All(fatores, f => Assert.True(f >= 1UL << (bits - 1), $"{f} e pequeno demais"));
            }
    }

    /// <summary>
    /// E o SEMIPRIMO COLADO tem os dois fatores perto, que e o que faz o metodo de
    /// Fermat achar rapido.
    /// </summary>
    [Fact]
    public void OSemiprimoColadoTemOsFatoresPerto()
    {
        foreach (var bits in new[] { 12, 16, 20, 24 })
        {
            var n = Numeros.SemiprimoColado(bits, 5);
            var fatores = Juiz.Certa(n);

            Assert.Equal(2, fatores.Count);

            var distancia = fatores[1] - fatores[0];

            Assert.True(distancia < fatores[0] / 4, $"distancia {distancia} para fatores de {fatores[0]}");
        }
    }

    /// <summary>
    /// Os numeros de CARMICHAEL sao achados pelo criterio de KORSELT, que nao testa
    /// base nenhuma.
    ///
    /// Isso importa: se a lista viesse de "testei a base dois e passou", ela estaria
    /// conferindo o teste de Fermat contra ele mesmo, e a tabela nao provaria nada.
    /// </summary>
    [Fact]
    public void OsCarmichaelVemDeKorseltENaoDeTesteDeBase()
    {
        var achados = Numeros.Carmichael(10_000);

        Assert.Equal([561UL, 1105, 1729, 2465, 2821, 6601, 8911], achados);

        // todos sao COMPOSTOS, livres de quadrados, e com tres ou mais fatores
        foreach (var n in achados)
        {
            var fatores = Juiz.Certa(n);

            Assert.False(Primalidade.PorDivisao(n));
            Assert.True(fatores.Count >= 3);
            Assert.Equal(fatores.Count, fatores.Distinct().Count());
            Assert.All(fatores, p => Assert.Equal(0UL, (n - 1) % (p - 1)));
        }
    }
}
