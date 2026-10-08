using Conde.Fatoracao;
using Xunit;

namespace Conde.Fatoracao.Testes;

/// <summary>
/// Os testes das TABELAS que o medidor imprime.
///
/// Eles existem porque numero em README que ninguem confere envelhece errado em
/// silencio: alguem troca uma constante, a tabela muda e o texto continua dizendo
/// a frase antiga. Aqui o teste afirma sobre o mesmo objeto que vira linha
/// impressa.
/// </summary>
public class MedidasTestes
{
    [Fact]
    public void TodaFatoracaoDaTabelaEstaCerta()
    {
        var linhas = Medidas.ContraADefinicao();

        Assert.Equal(12, linhas.Count);
        Assert.All(linhas.Where(l => l.Valor > 1), l => Assert.True(l.EstaCerta, l.Numero));
    }

    /// <summary>
    /// A frase do texto: o custo de ACHAR cresce como a raiz e o de CONFERIR cresce
    /// com os bits, e a razao entre os dois explode.
    /// </summary>
    [Fact]
    public void AAssimetriaEntreAcharEConferirExplode()
    {
        var linhas = Medidas.Assimetria();

        Assert.Equal(9, linhas.Count);

        // conferir cresce devagarinho, e de forma linear nos bits
        var primeiro = linhas.First();
        var ultimo = linhas.Last();

        Assert.True(ultimo.Conferir < 4 * primeiro.Conferir, "conferir deveria crescer com os bits");

        // e achar cresce como a raiz: dobrar os bits multiplica por mil
        Assert.True(ultimo.Achar > 10_000 * primeiro.Achar);
        Assert.True(ultimo.Razao > 1_000, $"razao final {ultimo.Razao:F1}");

        // e a razao so cresce
        for (var i = 1; i < linhas.Count; i++)
            Assert.True(linhas[i].Razao > linhas[i - 1].Razao, $"{linhas[i].Bits} bits");
    }

    /// <summary>
    /// A frase do texto: cada metodo e otimo num caso e inutil em outro.
    /// </summary>
    [Fact]
    public void CadaMetodoEhOtimoNumCasoEInutilEmOutro()
    {
        var linhas = Medidas.PorMetodo();

        Assert.Equal(11, linhas.Count);
        Assert.All(linhas, l => Assert.True(l.Batem, l.Caso));

        // Fermat desiste exatamente nos dois numeros com fatores desequilibrados
        Assert.Equal(["3 vezes um primo de 40 bits", "5 vezes um primo de 44 bits"],
            linhas.Where(l => l.FermatDesistiu).Select(l => l.Caso));

        // e no colado ele acha em UMA conta, onde a divisao faz cinco milhoes
        var colado = linhas.Single(l => l.Caso == "semiprimo colado de 24 bits");

        Assert.Equal(1, colado.DeFermat);
        Assert.True(colado.PorDivisao > 1_000_000);

        // e no desequilibrado o rho acha em UMA conta
        var desequilibrado = linhas.Single(l => l.Caso == "3 vezes um primo de 40 bits");

        Assert.Equal(1, desequilibrado.Rho);
        Assert.True(desequilibrado.FermatDesistiu);
    }

    /// <summary>
    /// A frase do texto: o rho custa a raiz quarta, e a razao contra ela fica
    /// parada enquanto o numero cresce mil vezes.
    /// </summary>
    [Fact]
    public void ORhoFicaNaCasaDaRaizQuarta()
    {
        var linhas = Medidas.ORho();

        Assert.Equal(9, linhas.Count);
        Assert.All(linhas, l => Assert.True(l.Rho < l.PorDivisao, $"{l.Bits} bits"));
        Assert.All(linhas, l => Assert.InRange(l.RazaoContraRaizQuarta, 0.05, 4.0));

        // e o numero cresce muito mais que a razao
        Assert.True(linhas.Last().Numero > 1_000_000 * linhas.First().Numero);
        Assert.True(linhas.Max(l => l.RazaoContraRaizQuarta) < 20 * linhas.Min(l => l.RazaoContraRaizQuarta));
    }

    /// <summary>
    /// A frase do texto: os dezesseis numeros de Carmichael ate cem mil enganam o
    /// teste de Fermat e nenhum engana Miller e Rabin.
    /// </summary>
    [Fact]
    public void TodosOsCarmichaelEnganamFermatENenhumEnganaMillerRabin()
    {
        var linhas = Medidas.Carmichael();

        Assert.Equal(16, linhas.Count);
        Assert.All(linhas, l => Assert.True(l.FermatBase2, $"{l.Numero}"));
        Assert.All(linhas, l => Assert.False(l.MillerRabin, $"{l.Numero}"));

        var primeiro = linhas.First();

        Assert.Equal(561UL, primeiro.Numero);
        Assert.Equal("3 x 11 x 17", primeiro.Fatores);

        // o 561 e pego pela base 3 porque 3 divide ele, e por mais nada
        Assert.False(primeiro.FermatBase3);
        Assert.True(linhas.Count(l => l.FermatBase3) >= 14);
    }

    /// <summary>
    /// A frase do texto: Miller e Rabin nao erra NENHUM numero ate cem mil, e o de
    /// Fermat erra setenta e oito.
    /// </summary>
    [Fact]
    public void MillerRabinNaoErraNadaEFermatErraSetentaEOito()
    {
        var linhas = Medidas.OsTestes();

        Assert.Equal(3, linhas.Count);
        Assert.All(linhas, l => Assert.Equal(0, l.MillerRabinErrou));

        var cemMil = linhas.Single(l => l.Ate == 100_000);

        Assert.Equal(9_592, cemMil.Primos);
        Assert.Equal(78, cemMil.FermatErrou);

        // e os erros de Fermat so crescem com o limite
        for (var i = 1; i < linhas.Count; i++)
            Assert.True(linhas[i].FermatErrou > linhas[i - 1].FermatErrou);
    }
}
