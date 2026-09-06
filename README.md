# Midnight Club: Los Angeles - PC Port (Static Recompilation)

![Capa do Projeto](assets/cover.jpg)

**Midnight Club: Los Angeles - Recomp** é um projeto independente focado em trazer o clássico do Xbox 360 nativamente para o PC através de **Recompilação Estática**. Baseado na tecnologia do [ReXGlue SDK](https://github.com/florinp93/hells-gate-recomp) (usado no port do *Dante's Inferno*), este projeto traduz os binários PowerPC do jogo original diretamente para C++ (x64) nativo.

---

## O que foi feito?

Este não é um emulador tradicional. O código original do jogo foi re-compilado para rodar nativamente no Windows utilizando a API DirectX 12. 

Para tornar a experiência a mais "plug and play" possível para os jogadores, criamos um ecossistema completo:
- **Launcher WPF Customizado**: Uma interface limpa, sem a necessidade de comandos de terminal ou edição de arquivos de configuração.
- **Extração Automática (extract-xiso)**: A ferramenta extract-xiso foi embutida no projeto. O usuário apenas aponta o arquivo .iso original do jogo, e o Launcher cuida da extração do disco e da criação dos diretórios corretamente de forma visual.
- **Opções Gráficas Integradas**: Suporte imediato a renderização em 2K e Anti-Aliasing (FXAA) embutido no Launcher.
- **Portabilidade Total**: Sem instaladores complexos. Apenas um .zip com o Launcher e os binários, totalmente portátil. Sistema prático para criar atalhos no Menu Iniciar e na Área de Trabalho com 1 clique.

---

## Como Jogar

**Requisitos:** 
- O arquivo .iso original do Midnight Club: Los Angeles (Xbox 360).
- Windows 10/11.

**Passo a Passo:**
1. Vá na aba [Releases](../../releases) do GitHub e baixe o arquivo .zip da última versão.
2. Extraia o conteúdo para uma pasta no seu computador (ex: C:\Games\MCLA).
3. Abra o **MidnightClubLALauncher.exe** (dentro da pasta launcher\...\bin\Release\).
4. Vá até a aba **CONFIG**, clique em *Browse* e selecione o seu arquivo .iso do jogo.
5. Clique em **Extrair Jogo** e aguarde a barra de progresso e a mensagem de Sucesso (isso criará a pasta game/ com os arquivos).
6. *(Opcional)* Clique em **Criar Atalhos** para adicionar o jogo à sua Área de Trabalho.
7. Vá na aba **JOGAR**, selecione a resolução desejada e clique em **PLAY**!

 **Aviso de Performance:** É altamente recomendado que você utilize uma ferramenta externa (como o Painel da NVIDIA ou o RTSS) para **travar o FPS do jogo em 60**. Como o jogo original rodava a 30 FPS, isso evita instabilidades físicas no motor do jogo.

---

## Como Compilar

Se você quiser contribuir ou compilar o projeto do zero a partir do código-fonte:

**Ferramentas Necessárias:**
- **Clang 18+** (Obrigatório, MSVC/GCC não são suportados na base do código do jogo).
- **CMake 3.25+** e Ninja.
- **Visual Studio 2022** (Para compilar o Launcher em C# WPF e para a Windows SDK).

**Compilando o Jogo (ReXGlue):**
`powershell
# Gere os arquivos gerenciados pelo SDK (Requer o default.xex extraído em game/)
rexglue init --force --project_name midnight_club_la --project_root . --xex_path game\default.xex --game_root game

# Compile o binário do jogo via CMake
cmake --preset win-amd64-release -DREXSDK_DIR=thirdparty\rexglue-sdk
cmake --build out\build\win-amd64-release
`

**Compilando o Launcher:**
`powershell
# Usando o MSBuild incluído no Visual Studio
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" launcher\MidnightClubLA.sln /p:Configuration=Release /p:Platform=AnyCPU
`

---

##  Sobre o Desenvolvimento e Transparência

Gostaríamos de ser totalmente transparentes sobre a criação deste projeto:
- **Base do Código:** Este port utilizou como fundação o repositório original do projeto **Hells Gate** (Dante's Inferno). Adaptamos a infraestrutura do ReXGlue desenvolvida lá para suportar o Midnight Club LA.
- **Uso de Inteligência Artificial:** Praticamente todo o fluxo de desenvolvimento do ecossistema moderno do jogo (como o Launcher WPF, scripts de automação, refatoração de código e design da interface) foi desenvolvido com auxílio pesado de Inteligência Artificial. 
- **Engenharia Reversa:** A única exceção ao uso de IA foi a Engenharia Reversa pura. Foi necessária a intervenção e análise humana extensa para resolver problemas complexos de otimização da engine e crashes recorrentes que impediam o funcionamento estável do título na nossa base.

---
##  Ferramentas Utilizadas & Créditos

Este projeto só foi possível graças às seguintes ferramentas open-source:

- **ReXGlue SDK** - Ferramenta base de recompilação estática PowerPC -> C++.
- **[extract-xiso](https://github.com/XboxDev/extract-xiso)** - Ferramenta de extração de ISOs do Xbox.
- **[XEXLoaderWV](https://github.com/zeroKilo/XEXLoaderWV/releases)** - Pesquisa e extração de dependências.
- **[Ghidra](https://github.com/nationalsecurityagency/ghidra)** - Engenharia reversa.

*Aviso legal: Este projeto não contém nenhum código proprietário, assets originais ou arquivos .iso. Os usuários devem fornecer sua própria cópia legalmente adquirida do jogo.*

