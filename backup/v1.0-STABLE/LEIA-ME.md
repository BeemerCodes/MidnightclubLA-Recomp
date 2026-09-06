# Backup v1.0-STABLE — Midnight Club LA Recomp

**Data:** 06/09/2026  
**Commit:** e2de583 — "v1.0-STABLE: RC3 restored + all unresolved branches fixed + VEH disabled for smooth 60fps"

## Estado do jogo neste backup
- ✅ Roda estável a 60 FPS, inclusive no cockpit
- ✅ Áudio limpo, sem chiado
- ✅ HUD funcionando normalmente
- ✅ Resolução 2x (1440p/4K) + FXAA
- ✅ Sem stutters (VEH page traps desabilitados, D3D12 Tiled Tier 4)

---

## Como restaurar (Ctrl+C / Ctrl+V)

### 1. Binários do jogo
Copie estes arquivos para `out\build\win-amd64-release\`:

| Arquivo desta pasta | Destino |
|---------------------|---------|
| `midnight_club_la.exe` | `out\build\win-amd64-release\` |
| `rexruntime.dll` | `out\build\win-amd64-release\` |
| `rexgpu-xenos.dll` | `out\build\win-amd64-release\` |

### 2. Código-fonte (necessário só se for recompilar)
| Arquivo desta pasta | Destino |
|---------------------|---------|
| `mcla_app.h` | `src\` |
| `midnight_club_la_app.h` | `src\` |
| `main.cpp` | `src\` |
| `midnight_club_la_manifest.toml` | raiz do projeto |
| `CMakeLists.txt` | raiz do projeto |
| `run_mcla_60fps_hd.bat` | raiz do projeto |
| `build_fix.bat` | raiz do projeto |

### 3. Shader cache (evita stutters na primeira execução)
Copie a pasta `user_data\` completa para dentro de `Documentos\midnight_club_la\`:
```
user_data\cache\               →  Documentos\midnight_club_la\cache\
user_data\B13EBABEBABEBABE\    →  Documentos\midnight_club_la\B13EBABEBABEBABE\
```
> **Nota:** A pasta `B13EBABEBABEBABE` contém o save do jogo (`mc4.sav`).

---

## Para apenas rodar o jogo (sem recompilar)
1. Copie os 3 binários (`midnight_club_la.exe`, `rexruntime.dll`, `rexgpu-xenos.dll`) → `out\build\win-amd64-release\`
2. Copie `user_data\cache\` → `Documentos\midnight_club_la\cache\`
3. Execute `run_mcla_60fps_hd.bat`

---

## Para recompilar do zero após restaurar os fontes
```powershell
.\build_fix.bat
Copy-Item thirdparty\rexglue-sdk\out\win-amd64\rexgpu-xenos.dll out\build\win-amd64-release\rexgpu-xenos.dll -Force
```

---

## Flags de performance que resolveram os travamentos
```
--clear_memory_page_state=false  → desativa VEH page traps, usa D3D12 Tiled Tier 4 (RTX 4060 Ti)
--log_level=off                  → elimina overhead de log em disco durante o jogo
```
