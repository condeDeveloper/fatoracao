namespace Conde.Fatoracao;

/// <summary>
/// O JUIZ, e aqui ele e barato de proposito, porque o assunto do repositorio e
/// exatamente essa diferenca.
///
/// Uma fatoracao esta certa quando duas coisas valem: o produto dos fatores e o
/// numero, e todo fator e primo. As duas sao faceis de conferir, e e por isso que
/// uma fatoracao e um CERTIFICADO: quem recebe a resposta nao precisa confiar em
/// quem a achou.
///
/// Essa assimetria entre achar e conferir e a mais util da computacao. Fatorar um
/// numero de duzentos digitos leva anos de maquina; conferir a resposta leva
/// milissegundos. Toda a criptografia de chave publica vive nessa diferenca.
/// </summary>
public static class Juiz
{
    /// <param name="OProdutoBate">multiplicar os fatores devolve o numero</param>
    /// <param name="TodosPrimos">todo fator e primo, pela divisao por tentativa</param>
    /// <param name="EmOrdem">a lista vem ordenada</param>
    public record Conferencia(bool OProdutoBate, bool TodosPrimos, bool EmOrdem)
    {
        public bool EstaCerta => OProdutoBate && TodosPrimos && EmOrdem;
    }

    /// <summary>
    /// Confere uma fatoracao pelas DUAS metades da definicao.
    ///
    /// As duas metades importam. So a primeira aceita [n] para qualquer n; so a
    /// segunda aceita qualquer lista de primos sem relacao nenhuma com o numero.
    /// </summary>
    public static Conferencia Conferir(ulong n, IReadOnlyList<ulong> fatores)
    {
        var produto = 1UL;
        var estourou = false;

        foreach (var f in fatores)
        {
            if (produto > ulong.MaxValue / Math.Max(f, 1UL)) { estourou = true; break; }

            produto *= f;
        }

        return new Conferencia(
            !estourou && produto == n,
            fatores.All(Primalidade.PorDivisao),
            fatores.SequenceEqual(fatores.Order()));
    }

    /// <summary>
    /// O custo de CONFERIR, em multiplicacoes e divisoes.
    ///
    /// E a outra metade da medida que importa: a tabela compara este numero com o
    /// custo de ACHAR, e a razao entre os dois e o assunto do repositorio.
    /// </summary>
    public static long CustoDeConferir(IReadOnlyList<ulong> fatores)
    {
        var contas = (long)fatores.Count;

        foreach (var f in fatores) contas += Primalidade.CustoPorDivisao(f);

        return contas;
    }

    /// <summary>
    /// O custo de conferir USANDO Miller e Rabin em vez da divisao.
    ///
    /// E assim que se confere de verdade, e e por isso que "conferir e barato"
    /// continua valendo com numeros grandes: a divisao por tentativa num fator de
    /// trinta e dois bits ja custa sessenta mil contas, e Miller e Rabin custa umas
    /// quatrocentas.
    /// </summary>
    public static long CustoDeConferirRapido(IReadOnlyList<ulong> fatores)
    {
        var contas = (long)fatores.Count;

        foreach (var f in fatores) contas += Primalidade.CustoDeMillerRabin(f);

        return contas;
    }

    /// <summary>
    /// A FATORACAO CERTA, pela divisao por tentativa sem limite.
    ///
    /// Ela e o juiz das outras duas: a resposta que ela da e a resposta, e o preco
    /// e ela nao chegar nos numeros grandes. Onde ela chega, ela manda.
    /// </summary>
    public static List<ulong> Certa(ulong n) => Metodos.PorDivisao(n).Fatores;

    /// <summary>Duas fatoracoes sao a mesma quando tem os mesmos fatores com as mesmas repeticoes.</summary>
    public static bool Mesma(IEnumerable<ulong> um, IEnumerable<ulong> outro) =>
        um.Order().SequenceEqual(outro.Order());
}
