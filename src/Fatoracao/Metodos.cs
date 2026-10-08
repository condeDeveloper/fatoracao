namespace Conde.Fatoracao;

/// <summary>
/// OS METODOS DE FATORACAO, e a assimetria que define o assunto.
///
/// ACHAR os fatores de um numero e dificil. CONFERIR uma fatoracao e trivial:
/// multiplique de volta e teste se cada pedaco e primo. Essa diferenca entre achar
/// e conferir e o chao de toda a criptografia de chave publica, e ela e medida
/// neste repositorio em multiplicacoes.
///
/// Os tres metodos daqui:
///
/// - a DIVISAO POR TENTATIVA, que e a definicao e custa a raiz de n;
/// - o de FERMAT, de 1643, que procura n como diferenca de dois quadrados e e
///   instantaneo quando os fatores estao colados e inutil quando nao estao;
/// - o RHO de Pollard, de 1975, que custa a raiz QUARTA do menor fator e nao
///   parece que deveria funcionar.
/// </summary>
public static class Metodos
{
    /// <param name="Fatores">os fatores primos, com repeticao, em ordem</param>
    /// <param name="Contas">quantas multiplicacoes ou divisoes foram feitas</param>
    /// <param name="Terminou">falso quando o metodo desistiu antes do fim</param>
    public record Resultado(List<ulong> Fatores, long Contas, bool Terminou);

    /// <summary>
    /// A DIVISAO POR TENTATIVA: tente dois, tres, cinco, e assim por diante ate a
    /// raiz.
    ///
    /// E a definicao, e ela sempre funciona. O custo e a raiz do MAIOR fator primo
    /// que sobra, e e por isso que ela e rapida em quase todo numero sorteado e
    /// impossivel no produto de dois primos grandes.
    /// </summary>
    public static Resultado PorDivisao(ulong n, long limite = long.MaxValue)
    {
        var fatores = new List<ulong>();
        var contas = 0L;

        while (n % 2 == 0) { fatores.Add(2); n /= 2; contas++; }

        for (var d = 3UL; d <= n / d; d += 2)
        {
            if (++contas > limite) return new Resultado(fatores, contas, false);

            while (n % d == 0) { fatores.Add(d); n /= d; }
        }

        if (n > 1) fatores.Add(n);

        fatores.Sort();

        return new Resultado(fatores, contas, true);
    }

    /// <summary>
    /// O METODO DE FERMAT, de 1643: procure n como a diferenca de dois quadrados.
    ///
    /// Se n e igual a a ao quadrado menos b ao quadrado, entao n e (a-b) vezes
    /// (a+b), e os fatores sairam. Comece com a na raiz de n e va subindo ate que
    /// a ao quadrado menos n seja um quadrado perfeito.
    ///
    /// Quando os dois fatores estao perto da raiz, isso acontece na PRIMEIRA
    /// tentativa. Quando estao longe, o a precisa subir ate quase n sobre dois, e o
    /// metodo e pior que a divisao por tentativa.
    ///
    /// E um metodo cujo custo depende de uma propriedade do numero que ninguem sabe
    /// de antemao, e por isso ele e otimo e inutil ao mesmo tempo.
    ///
    /// Ele devolve UMA DIVISAO, e nao a fatoracao completa: dois fatores cujo
    /// produto e n, que podem ser compostos. Isso esta dito aqui porque a tabela
    /// compara contas e nao respostas, e comparar o numero de contas de um metodo
    /// que resolve menos com o de outro que resolve tudo seria desonesto sem este
    /// paragrafo.
    /// </summary>
    public static Resultado DeFermat(ulong n, long limite = 5_000_000)
    {
        if (n % 2 == 0) return new Resultado([2, n / 2], 1, true);

        var a = (ulong)Math.Ceiling(Math.Sqrt(n));
        var contas = 0L;

        while (contas < limite)
        {
            contas++;

            var quadrado = a * a - n;
            var b = (ulong)Math.Sqrt(quadrado);

            // a raiz de double erra nos ultimos bits: conferir os vizinhos custa
            // tres contas e e a diferenca entre o metodo funcionar e nao funcionar
            for (var ajuste = b == 0 ? 0UL : b - 1; ajuste <= b + 1; ajuste++)
                if (ajuste * ajuste == quadrado) return new Resultado([a - ajuste, a + ajuste], contas, true);

            a++;
        }

        return new Resultado([], contas, false);
    }

    /// <summary>
    /// O RHO DE POLLARD, de 1975, que nao parece que deveria funcionar.
    ///
    /// Ande por uma sequencia sorteada modulo n, duas velocidades, e va tirando o
    /// maior divisor comum entre a diferenca dos dois andarilhos e o proprio n.
    /// Quando os dois caem no mesmo valor MODULO UM FATOR, sem terem caido no mesmo
    /// valor modulo n, o divisor comum entrega o fator.
    ///
    /// Por que isso funciona: modulo um fator p, a sequencia tem so p valores
    /// possiveis, e pelo paradoxo do aniversario dois andarilhos se encontram em
    /// raiz de p passos. Entao achar um fator p custa raiz de p, que e a raiz QUARTA
    /// de n quando os dois fatores sao parecidos.
    ///
    /// Raiz quarta contra raiz: com um numero de quarenta bits, sao mil passos
    /// contra um milhao.
    /// </summary>
    public static Resultado Rho(ulong n, ulong semente = 7, long limite = 20_000_000)
    {
        var fatores = new List<ulong>();
        var contas = 0L;
        var pilha = new Stack<ulong>();

        pilha.Push(n);

        while (pilha.Count > 0)
        {
            var atual = pilha.Pop();

            if (atual == 1) continue;

            if (Primalidade.DeMillerRabin(atual)) { fatores.Add(atual); continue; }

            while (atual % 2 == 0) { fatores.Add(2); atual /= 2; }

            if (atual == 1) continue;

            if (Primalidade.DeMillerRabin(atual)) { fatores.Add(atual); continue; }

            var fator = UmFator(atual, semente, limite, ref contas);

            if (fator == 0) return new Resultado(fatores, contas, false);

            pilha.Push(fator);
            pilha.Push(atual / fator);
        }

        fatores.Sort();

        return new Resultado(fatores, contas, true);
    }

    /// <summary>
    /// UM fator nao trivial de n, pela caminhada de Pollard com o ciclo de Floyd.
    ///
    /// O deslocamento c muda a cada tentativa que falha. Sem isso, um n para o qual
    /// a sequencia fecha cedo trava o metodo para sempre, e trocar o c e o que
    /// transforma "nao achei" em "tento de novo com outra sequencia".
    /// </summary>
    public static ulong UmFator(ulong n, ulong semente, long limite, ref long contas)
    {
        if (n % 2 == 0) return 2;

        var sorteio = new Sorteio(semente);

        for (var tentativa = 0; tentativa < 40; tentativa++)
        {
            var c = 1 + sorteio.Ate(n - 1);
            var x = 2 + sorteio.Ate(n - 2);
            var y = x;
            var d = 1UL;

            while (d == 1)
            {
                if (++contas > limite) return 0;

                x = (Modular.Vezes(x, x, n) + c) % n;
                y = (Modular.Vezes(y, y, n) + c) % n;
                y = (Modular.Vezes(y, y, n) + c) % n;

                d = Modular.Mdc(x > y ? x - y : y - x, n);
            }

            if (d != n) return d;
        }

        return 0;
    }
}
