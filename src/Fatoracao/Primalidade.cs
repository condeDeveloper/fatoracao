namespace Conde.Fatoracao;

/// <summary>
/// A PRIMALIDADE, que e um problema completamente diferente de fatorar.
///
/// Saber SE um numero e primo e facil: ha testes rapidos e exatos. Saber QUAIS sao
/// os fatores dele e dificil, e ninguem conhece metodo rapido. Os dois parecem a
/// mesma pergunta e nao sao, e esse e um dos fatos mais uteis da computacao: e
/// dele que sai toda a criptografia de chave publica.
///
/// Aqui estao tres testes:
///
/// - por DIVISAO, que e a definicao e custa a raiz de n;
/// - de FERMAT, que e rapido e ERRADO, e os numeros de Carmichael mostram onde;
/// - de MILLER e RABIN, que e rapido e, com as bases certas, EXATO ate sessenta e
///   quatro bits.
/// </summary>
public static class Primalidade
{
    /// <summary>
    /// A DEFINICAO: n e primo quando nenhum numero entre dois e a raiz dele o
    /// divide.
    ///
    /// Custa a raiz de n, e e por isso que ela e juiz e nao algoritmo. Com um numero
    /// de sessenta e quatro bits isso e dois bilhoes de divisoes, e e exatamente por
    /// isso que os outros dois testes existem.
    /// </summary>
    public static bool PorDivisao(ulong n)
    {
        if (n < 2) return false;
        if (n % 2 == 0) return n == 2;

        for (var d = 3UL; d <= n / d; d += 2)
            if (n % d == 0) return false;

        return true;
    }

    /// <summary>
    /// O TESTE DE FERMAT: se n e primo, entao a elevado a n-1 e um, modulo n.
    ///
    /// A frase esta certa. O problema e que a volta dela nao vale: existem compostos
    /// que tambem passam. Alguns passam so em algumas bases, e os de CARMICHAEL
    /// passam em TODAS as bases coprimas com eles.
    ///
    /// Esse teste esta aqui para ser medido errando, e nao para ser usado.
    /// </summary>
    public static bool DeFermat(ulong n, ulong baseLinha)
    {
        if (n < 2) return false;
        if (n == 2) return true;
        if (n % 2 == 0) return false;
        if (Modular.Mdc(baseLinha % n, n) != 1) return false;

        return Modular.Potencia(baseLinha, n - 1, n) == 1;
    }

    /// <summary>
    /// O TESTE DE MILLER e RABIN, que conserta o de Fermat com uma observacao a
    /// mais.
    ///
    /// Se n e primo, a unica raiz quadrada de um modulo n e mais um ou menos um.
    /// Entao, ao elevar a base ate n-1 por quadrados, a sequencia tem de chegar a um
    /// passando por menos um, e nao por qualquer outro numero. E essa conferencia
    /// que os numeros de Carmichael nao sobrevivem.
    ///
    /// Com as SETE primeiras bases primas ele e EXATO para todo numero de sessenta e
    /// quatro bits, e isso nao e probabilidade: e um resultado conferido
    /// exaustivamente e publicado. As bases estao escritas aqui porque elas sao o
    /// resultado.
    /// </summary>
    public static bool DeMillerRabin(ulong n)
    {
        if (n < 2) return false;

        foreach (var p in new ulong[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37 })
        {
            if (n == p) return true;
            if (n % p == 0) return false;
        }

        var d = n - 1;
        var r = 0;

        while (d % 2 == 0) { d /= 2; r++; }

        foreach (var a in new ulong[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37 })
        {
            var x = Modular.Potencia(a, d, n);

            if (x == 1 || x == n - 1) continue;

            var passou = false;

            for (var i = 1; i < r && !passou; i++)
            {
                x = Modular.Vezes(x, x, n);

                if (x == n - 1) passou = true;
            }

            if (!passou) return false;
        }

        return true;
    }

    /// <summary>
    /// Quantas MULTIPLICACOES modulares cada teste gasta.
    ///
    /// E esta a unidade do repositorio, e nao o relogio. Segundo depende de maquina,
    /// de compilador e de quem mais esta rodando no momento; multiplicacao modular e
    /// o mesmo numero nos tres sistemas da integracao continua.
    /// </summary>
    public static long CustoPorDivisao(ulong n)
    {
        if (n < 2) return 0;
        if (n % 2 == 0) return 1;

        var contas = 1L;

        for (var d = 3UL; d <= n / d; d += 2)
        {
            contas++;

            if (n % d == 0) break;
        }

        return contas;
    }

    /// <summary>O custo de Miller e Rabin: doze bases vezes o numero de bits.</summary>
    public static long CustoDeMillerRabin(ulong n)
    {
        var bits = 0;
        var m = n;

        while (m > 0) { bits++; m >>= 1; }

        return 12L * bits;
    }
}
