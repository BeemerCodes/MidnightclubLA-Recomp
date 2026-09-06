// midnight_club_la - ReXGlue Recompiled Project
//
// Customize your app by overriding virtual hooks from rex::ReXApp.

#pragma once

#include <rex/rex_app.h>
#include <rex/graphics/graphics_system.h>
#include <rex/runtime.h>
#include <atomic>

// ============================================================================
// NATIVE HOOKS (RAGE Engine Streaming Interception)
// ============================================================================
// Ponteiro global para o GraphicsSystem, preenchido em OnPostSetup().
// Permite que os hooks C (sem `this`) invalidem a memória da GPU sem gerar
// exceções VEH do Windows.
static rex::graphics::GraphicsSystem* g_mcla_graphics_system = nullptr;

// Throttle: evita chamar InvalidateGpuMemory a cada chamada de Open/OpenBulk
// (que acontece muitas vezes por segundo). Acumula chamadas e invalida a cada
// kInvalidateEveryN chamadas, reduzindo overhead sem sacrificar coerência.
static constexpr int kInvalidateEveryN = 8;
static std::atomic<int> g_open_call_counter{0};

extern "C" {

// Hook em 0x821CCEA0 (rage::fiPackfile::Open)
// Chamado ANTES da instrução original. ret=false -> fluxo continua normalmente.
void OnFiPackfileOpen(rex::arch::HostThreadContext* /*ctx*/,
                      uint64_t /*r3*/, uint64_t /*r4*/) {
  if (!g_mcla_graphics_system) return;
  // A cada kInvalidateEveryN chamadas, notifica a GPU que páginas físicas
  // podem ter mudado — sem VEH, sem kernel trap, sem custo de exceção.
  int n = g_open_call_counter.fetch_add(1, std::memory_order_relaxed);
  if ((n % kInvalidateEveryN) == 0) {
    g_mcla_graphics_system->InvalidateGpuMemory();
  }
}

// Hook em 0x821CC620 (rage::fiPackfile::OpenBulk)
// Carregamento em massa de chunks de cidade — invalida imediatamente.
void OnFiPackfileOpenBulk(rex::arch::HostThreadContext* /*ctx*/,
                          uint64_t /*r3*/, uint64_t /*r4*/) {
  if (!g_mcla_graphics_system) return;
  // OpenBulk carrega blocos grandes de assets; invalida sempre.
  g_mcla_graphics_system->InvalidateGpuMemory();
}

} // extern "C"

class MidnightClubLaApp : public rex::ReXApp {
 public:
  using rex::ReXApp::ReXApp;

  static std::unique_ptr<rex::ui::WindowedApp> Create(
      rex::ui::WindowedAppContext& ctx) {
    return std::unique_ptr<MidnightClubLaApp>(new MidnightClubLaApp(ctx, "midnight_club_la",
        PPCImageConfig));
  }

  void OnPreSetup(rex::RuntimeConfig& config) override {
    config.SetCVar("draw_resolution_scale", 2);
    config.SetCVar("draw_resolution_scale_x", 2);
    config.SetCVar("draw_resolution_scale_y", 2);
    
    // Gráficos: HD + FXAA
    config.SetCVar("swap_post_effect", std::string("fxaa"));
    config.SetCVar("use_fuzzy_alpha_epsilon", true);
    config.SetCVar("gpu_allow_invalid_fetch_constants", true);

    // Cache de texturas estritamente limitado para não estourar os 8GB reais da placa.
    // (O jogo já usa ~2GB fixos para render targets escalados + 512MB de RAM do Xbox).
    config.SetCVar("texture_cache_memory_limit_soft", 2048);
    config.SetCVar("texture_cache_memory_limit_hard", 2560);
    config.SetCVar("texture_cache_memory_limit_soft_lifetime", 15);

    // Performance
    config.SetCVar("async_shader_compilation", true);
    config.SetCVar("d3d12_host_vsync", false);
    // Usa 'off' ou 'critical'. 'fatal' é inválido no spdlog e silenciosamente ignorado,
    // mantendo o nível em 'info' e ativando backtraces caríssimos a cada Access Violation.
    config.SetCVar("log_level", std::string("off"));
  }

  void OnPostSetup() override {
    // Captura o GraphicsSystem logo após o setup completo.
    // Os hooks nativos usam esse ponteiro para invalidar páginas GPU.
    if (runtime()) {
      auto* gs = static_cast<rex::graphics::GraphicsSystem*>(
          runtime()->graphics_system());
      g_mcla_graphics_system = gs;
    }
  }

  void OnShutdown() override {
  }

  // void OnLoadXexImage(std::string& xex_image) override {}
  // void OnPostLoadXexImage() override {}
  // void OnCreateDialogs(rex::ui::ImGuiDrawer* drawer) override {}
  // void OnConfigurePaths(rex::PathConfig& paths) override {}
};

