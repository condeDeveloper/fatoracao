namespace Conde.Fatoracao;

/// <summary>
/// O SORTEIO, escrito aqui em vez de vir da biblioteca.
///
/// A documentacao do sorteio de biblioteca nao promete a mesma sequencia entre
/// versoes nem entre sistemas, e a integracao continua roda em tres. "O rho achou
/// o fator em 1.284 passos" com numeros diferentes em cada sistema nao e uma
/// medida: sao tres medidas que ninguem pode comparar.
///
/// E aqui o sorteio nao e so dos numeros: ele e parte do algoritmo de Pollard, que
/// comeca de um ponto sorteado. Um rho que usasse o gerador da biblioteca teria um
/// custo diferente em cada sistema e a tabela nao diria nada.
/// </summary>
public sealed class Sorteio
{
    private ulong estado;

    public Sorteio(ulong semente) => estado = semente == 0 ? 1 : semente;

    public ulong Proximo()
    {
        estado = estado * 6364136223846793005UL + 1442695040888963407UL;

        return estado >> 32 | (estado & 0xFFFFFFFF00000000UL);
    }

    public ulong Ate(ulong limite) => Proximo() % limite;

    public double Fracao() => (Proximo() >> 32) / 4294967296.0;
}

/// <summary>
/// A ARITMETICA MODULAR de sessenta e quatro bits, que e onde tudo isto acontece.
///
/// Multiplicar dois numeros de sessenta e quatro bits estoura sessenta e quatro
/// bits, e e por isso que a conta passa por cento e vinte e oito. Fazer isso com
/// double, que e o atalho que todo mundo tenta, perde os bits de baixo e devolve
/// um resto errado sem dar erro nenhum.
/// </summary>
public static class Modular
{
    /// <summary>O produto de dois numeros modulo um terceiro, sem estourar.</summary>
    public static ulong Vezes(ulong a, ulong b, ulong m) => (ulong)((UInt128)a * b % m);

    /// <summary>
    /// A POTENCIA modular por quadrados sucessivos.
    ///
    /// Ela e o que torna o teste de primalidade possivel: elevar a n-1 sem ela
    /// seria n-1 multiplicacoes, e com ela sao sessenta e quatro.
    /// </summary>
    public static ulong Potencia(ulong a, ulong expoente, ulong m)
    {
        if (m == 1) return 0;

        var resultado = 1UL;

        a %= m;

        while (expoente > 0)
        {
            if ((expoente & 1) != 0) resultado = Vezes(resultado, a, m);

            a = Vezes(a, a, m);
            expoente >>= 1;
        }

        return resultado;
    }

    /// <summary>O maior divisor comum, por Euclides.</summary>
    public static ulong Mdc(ulong a, ulong b)
    {
        while (b != 0) (a, b) = (b, a % b);

        return a;
    }
}

/// <summary>
/// Os NUMEROS que separam os metodos, e por que eles sao assim.
///
/// Numero sorteado nao separa nada: metade deles e par, e dois tercos tem um fator
/// menor que dez. O que separa sao os numeros ARMADOS, e o mais importante deles e
/// o produto de dois primos de tamanhos parecidos, que e exatamente o numero que a
/// criptografia usa.
/// </summary>
public static class Numeros
{
    /// <summary>Os primos ate um limite, pelo crivo de Eratostenes.</summary>
    public static List<ulong> Primos(int ate)
    {
        var composto = new bool[ate + 1];
        var primos = new List<ulong>();

        for (var i = 2; i <= ate; i++)
        {
            if (composto[i]) continue;

            primos.Add((ulong)i);

            for (var j = (long)i * i; j <= ate; j += i) composto[j] = true;
        }

        return primos;
    }

    /// <summary>
    /// O SEMIPRIMO: o produto de dois primos de tamanhos parecidos.
    ///
    /// E o numero mais dificil de fatorar que existe no mesmo tamanho, e e por isso
    /// que a criptografia de chave publica usa exatamente ele. Com os dois fatores
    /// grandes, a divisao por tentativa precisa chegar na raiz, e nao ha fator
    /// pequeno nenhum para encurtar o caminho.
    /// </summary>
    public static ulong Semiprimo(int bitsPorFator, ulong semente)
    {
        var p = PrimoPerto(1UL << (bitsPorFator - 1), semente);
        var q = PrimoPerto(1UL << (bitsPorFator - 1), semente + 1_000);

        return p == q ? p * PrimoPerto(1UL << (bitsPorFator - 1), semente + 2_000) : p * q;
    }

    /// <summary>
    /// Um semiprimo com os dois fatores COLADOS, que e o caso facil do metodo de
    /// Fermat.
    ///
    /// Fermat procura n como diferenca de dois quadrados, e quando os fatores estao
    /// perto da raiz ele acha na primeira tentativa. Quando estao longe, ele nunca
    /// acha.
    /// </summary>
    public static ulong SemiprimoColado(int bits, ulong semente)
    {
        var p = PrimoPerto(1UL << (bits - 1), semente);
        var q = ProximoPrimo(p + 1);

        return p * q;
    }

    /// <summary>O produto de MUITOS primos pequenos: o caso facil de todo mundo.</summary>
    public static ulong MuitosPequenos(int quantos, ulong semente)
    {
        var sorteio = new Sorteio(semente);
        var primos = Primos(200);
        var n = 1UL;

        for (var i = 0; i < quantos; i++)
        {
            var p = primos[(int)sorteio.Ate((ulong)primos.Count)];

            if (n > ulong.MaxValue / p) break;

            n *= p;
        }

        return n;
    }

    /// <summary>Uma POTENCIA DE PRIMO: o caso que quebra uma versao ingenua do rho.</summary>
    public static ulong PotenciaDePrimo(ulong p, int expoente)
    {
        var n = 1UL;

        for (var i = 0; i < expoente; i++) n *= p;

        return n;
    }

    /// <summary>
    /// O PROXIMO PRIMO a partir de um numero, sem sorteio nenhum.
    ///
    /// Ele existe separado do PrimoPerto porque o semiprimo colado precisa dos dois
    /// fatores VIZINHOS, e PrimoPerto espalha o resultado por meia faixa de
    /// proposito. Usar o errado dos dois deixa os fatores a mil de distancia e o
    /// "colado" do nome deixa de ser verdade.
    /// </summary>
    public static ulong ProximoPrimo(ulong comeco)
    {
        var n = comeco;

        if (n % 2 == 0) n++;

        while (!Primalidade.PorDivisao(n)) n += 2;

        return n;
    }

    /// <summary>O primeiro primo a partir de um numero sorteado na faixa, pelo teste exato.</summary>
    public static ulong PrimoPerto(ulong comeco, ulong semente)
    {
        var sorteio = new Sorteio(semente);
        var n = comeco + sorteio.Ate(comeco / 2 + 1);

        if (n % 2 == 0) n++;

        while (!Primalidade.PorDivisao(n)) n += 2;

        return n;
    }

    /// <summary>
    /// Os numeros de CARMICHAEL, que enganam o teste de Fermat em TODA base
    /// coprima.
    ///
    /// 561, 1105, 1729, 2465... Eles sao compostos e passam pelo teste de Fermat
    /// como se fossem primos, e nao so em algumas bases: em todas as que nao
    /// compartilham fator com eles.
    ///
    /// Eles sao a razao de o teste de Fermat nao servir, e a razao de Miller e
    /// Rabin existirem.
    /// </summary>
    public static List<ulong> Carmichael(ulong ate)
    {
        var achados = new List<ulong>();

        for (var n = 3UL; n <= ate; n += 2)
        {
            if (Primalidade.PorDivisao(n)) continue;
            if (!EhCarmichael(n)) continue;

            achados.Add(n);
        }

        return achados;
    }

    /// <summary>
    /// Pelo criterio de Korselt, 1899: n e de Carmichael quando ele e livre de
    /// quadrados e p-1 divide n-1 para todo fator primo p.
    ///
    /// Esse criterio nao testa base nenhuma, e e por isso que ele serve de juiz: se
    /// a lista de Carmichael viesse de "testei a base dois e passou", ela estaria
    /// conferindo o teste de Fermat contra ele mesmo.
    /// </summary>
    public static bool EhCarmichael(ulong n)
    {
        if (n < 3 || n % 2 == 0) return false;

        var fatores = new List<ulong>();
        var resto = n;

        for (var p = 2UL; p * p <= resto; p++)
        {
            if (resto % p != 0) continue;

            var expoente = 0;

            while (resto % p == 0) { resto /= p; expoente++; }

            if (expoente > 1) return false;

            fatores.Add(p);
        }

        if (resto > 1) fatores.Add(resto);
        if (fatores.Count < 3) return false;

        return fatores.All(p => (n - 1) % (p - 1) == 0);
    }

    public static IEnumerable<(string Nome, ulong Numero)> Conhecidos()
    {
        yield return ("1", 1);
        yield return ("2 (primo)", 2);
        yield return ("97 (primo)", 97);
        yield return ("2^10", 1024);
        yield return ("3^12", PotenciaDePrimo(3, 12));
        yield return ("561 (Carmichael)", 561);
        yield return ("1729 (Carmichael)", 1729);
        yield return ("muitos primos pequenos", MuitosPequenos(8, 3));
        yield return ("semiprimo de 16 bits", Semiprimo(16, 7));
        yield return ("semiprimo de 20 bits", Semiprimo(20, 11));
        yield return ("semiprimo colado de 20 bits", SemiprimoColado(20, 13));
        yield return ("um primo de 31 bits", PrimoPerto(1UL << 30, 17));
    }
}
