# fatoracao

Fatoração e primalidade do zero em C# e .NET 8: divisão por tentativa, o método de
Fermat de 1643, o rho de Pollard de 1975, e três testes de primalidade. O juiz é a
**definição** — uma fatoração está certa quando o produto dos fatores é o número
**e** todo fator é primo.

As duas metades importam: só a primeira aceita `[n]` para qualquer n.

E o juiz ser barato é o assunto do repositório inteiro.

```
$ dotnet medidor.dll assimetria

  bits                numero   contas para achar   contas para conferir   quantas vezes mais
    16                24,287                  74                    194                  0.4
    24             6,043,907               1,136                    290                  3.9
    32         1,692,563,423              18,120                    386                 46.9
    40       312,641,893,273             262,583                    482                544.8
    48   123,097,600,545,763           5,018,720                    578               8682.9
```

**Achar** os fatores custa a raiz do número. **Conferir** custa o número de bits.
Com quarenta e oito bits, achar leva cinco milhões de contas e conferir leva 578.

É dessa diferença que vive toda a criptografia de chave pública. Uma fatoração é
um **certificado**: quem recebe a resposta não precisa confiar em quem a achou.

## Cada método é ótimo num caso e inútil em outro

```
$ dotnet medidor.dll metodo

  caso                                          numero   por divisao     de Fermat          rho   Fermat desistiu
  3 vezes um primo de 40 bits          1,855,593,555,369       393,233     2,000,000            1               sim
  5 vezes um primo de 44 bits         47,418,224,416,235     1,539,775     2,000,000            1               sim
  semiprimo colado de 24 bits        121,373,319,505,927     5,508,476             1        1,745               NAO
  semiprimo de 20 bits                   363,213,365,041       288,195           600          633               NAO
  semiprimo de 24 bits               120,441,981,604,141     4,958,855        56,315        3,256               NAO
```

O **método de Fermat** procura n como diferença de dois quadrados. Quando os
fatores estão colados, ele acha na **primeira** tentativa; quando estão longe, ele
não acha em dois milhões.

O **rho** faz o contrário: ele acha o fator pequeno em **uma** conta e paga mais
quando os dois fatores são do mesmo tamanho.

E nenhum dos dois sabe de antemão em que caso está. A propriedade que decide, que
é a distância entre os fatores, só é conhecida depois de fatorar.

## O rho custa a raiz quarta

```
$ dotnet medidor.dll rho

  bits                numero   por divisao          rho   rho sobre a raiz quarta
    24             7,382,927         1,265           18                      0.35
    32         1,878,856,513        20,241          144                      0.69
    40       484,919,501,047       331,088          300                      0.36
    48   130,160,696,902,261     5,174,885          621                      0.18
    52 1,447,793,988,118,679    17,746,086        6,814                      1.10
```

Pollard anda por uma sequência sorteada módulo n, com dois andarilhos de
velocidades diferentes, e tira o máximo divisor comum entre a diferença deles e o
próprio n. Quando os dois caem no mesmo valor **módulo um fator**, sem terem caído
no mesmo valor módulo n, o divisor comum entrega o fator.

Por que funciona: módulo um fator p, a sequência tem só p valores possíveis, e
pelo paradoxo do aniversário dois andarilhos se encontram em **raiz de p** passos.

A última coluna fica entre 0,18 e 1,10 enquanto o número cresce um milhão de
vezes. É isso que "custa a raiz quarta" quer dizer, medido.

## Os números que enganam o teste de Fermat

```
$ dotnet medidor.dll carmichael

  quantos ha: 16
  quantos enganam o teste de Fermat na base 2: 16
  quantos enganam Miller e Rabin: 0

  numero   fatores            Fermat base 2   Fermat base 3   Miller e Rabin
     561   3 x 11 x 17                  sim             NAO              NAO
   1,105   5 x 13 x 17                  sim             sim              NAO
   1,729   7 x 13 x 19                  sim             sim              NAO
  41,041   7 x 11 x 13 x 41             sim             sim              NAO
```

Os números de **Carmichael** são compostos e passam pelo teste de Fermat em
**toda** base coprima com eles. No 561, que é 3 × 11 × 17, das 559 bases
possíveis, 319 passam e as únicas que o pegam são os múltiplos de 3, de 11 e de
17 — e achar uma base assim já é fatorar.

A lista aqui vem do critério de **Korselt**, de 1899, que não testa base nenhuma:
n é de Carmichael quando é livre de quadrados e p−1 divide n−1 para todo fator
primo p. Se ela viesse de "testei a base dois e passou", estaria conferindo o
teste de Fermat contra ele mesmo.

```
$ dotnet medidor.dll primalidade

     ate   primos   Fermat errou   Miller e Rabin errou   contas da divisao   contas de Miller e Rabin
   1,000      168              3                      0               3,350                    107,832
  10,000    1,229             22                      0              65,957                  1,483,560
 100,000    9,592             78                      0           1,445,441                 18,827,340
```

Não é amostra: é **todo** número até o limite. E a coluna de Miller e Rabin é
zero, o que não é probabilidade: com as doze primeiras bases primas o teste é
determinístico até sessenta e quatro bits, e aqui ele é conferido
exaustivamente contra a definição.

## Como rodar

```
dotnet test testes/Fatoracao.Testes/Fatoracao.Testes.csproj
dotnet run --project ferramentas/Medidor/Medidor.csproj -c Release -- tudo
```

Os comandos do medidor são `definicao`, `assimetria`, `metodo`, `rho`,
`carmichael`, `primalidade` e `tudo`.

A unidade é **conta**, e não segundo: segundo depende de máquina e de quem mais
está rodando, e multiplicação modular é o mesmo número nos três sistemas da
integração contínua.

## O que as medidas me corrigiram

**Evitar os números de Carmichael não conserta o teste de Fermat.** Eu ia escrever
que eles são "o problema" do teste, e a medida diz que eles são **dezesseis** dos
**setenta e oito** erros até cem mil. Os outros sessenta e dois são compostos
comuns que passam na base dois e não são de Carmichael.

A diferença entre os dois grupos é que os de Carmichael enganam em **toda** base,
e os outros só em algumas. Mas o teste erra nos dois, e "evitar os famosos" não é
conserto nenhum.

**O gerador do semiprimo colado não colava nada.** Ele pegava o próximo primo
"perto" do primeiro, e o `PrimoPerto` sorteia dentro de meia faixa de propósito. Os
dois fatores saíam a mil de distância, e o método de Fermat gastava 356 contas em
vez de **uma**.

O teste que pegou isso foi o que afirmava que os fatores ficam perto — e ele
existia justamente porque "colado" é uma propriedade do gerador que nada mais
confere. Sem ele, a tabela mostraria Fermat ganhando por pouco, e a conclusão
seria morna em vez de nítida.

**A multiplicação modular não cabe em sessenta e quatro bits, e o atalho não
avisa.** Multiplicar dois números de sessenta e quatro bits estoura, e fazer a
conta com `double` perde os bits de baixo e devolve um resto **errado** sem dar
erro nenhum. O teste compara as duas e exige que elas sejam diferentes, porque o
jeito de garantir que a conta certa está sendo usada é mostrar que a errada daria
outra coisa.

**O método de Fermat não devolve a fatoração.** Ele devolve **uma divisão**: dois
fatores cujo produto é n, que podem ser compostos. A tabela compara contas, e
comparar o custo de um método que resolve menos com o de outro que resolve tudo
seria desonesto sem dizer isso — então está dito no código e aqui.

## Licença

MIT.
