#pragma once

#include <rex/rex_app.h>
#include <rex/cvar.h>
#include <rex/chrono/clock.h>
#include <rex/input/flags.h>
#include <rex/ui/keybinds.h>
#include <rex/graphics/graphics_system.h>
#include <rex/logging/macros.h>

#include <memory>
#include <string>

class MclaApp : public rex::ReXApp {
 public:
  using rex::ReXApp::ReXApp;

  static std::unique_ptr<rex::ui::WindowedApp> Create(
      rex::ui::WindowedAppContext& ctx) {
    return std::unique_ptr<MclaApp>(new MclaApp(ctx, "midnight_club_la",
        PPCImageConfig));
  }

  void OnPreSetup(rex::RuntimeConfig& config) override {
    config.gpu_plugin = "xenos";
    REXCVAR_SET(input_backend, std::string("sdl"));
  }

  void OnPostSetup() override {
    // Hooks specific to Midnight Club LA go here
  }

  void OnShutdown() override {
  }
};
