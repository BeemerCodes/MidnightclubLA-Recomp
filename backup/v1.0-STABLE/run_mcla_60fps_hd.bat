@echo off
echo Iniciando Midnight Club: Los Angeles (60 FPS + HD)
echo ===================================================
echo.
echo NOTA IMPORTANTE PARA OS 60 FPS:
echo O patch de 60 FPS esta embutido no .exe! Porem, se o jogo
echo estiver muito acelerado (passando de 60 FPS no Afterburner),
echo voce PRECISA travar o framerate do midnight_club_la.exe 
echo em exatos 60 FPS usando o Painel de Controle da NVIDIA ou o RTSS.
echo.
echo Opcoes ativadas:
echo - Resolucao 2x (1440p/4K)
echo - FXAA (Anti-aliasing)
echo - Compilacao assincrona de shaders
echo - Cache de texturas equilibrado: 1.5GB soft / 2.5GB hard (5s lifetime)
echo.

.\out\build\win-amd64-release\midnight_club_la.exe --game_data_root=game --async_shader_compilation=true --draw_resolution_scale_x=2 --draw_resolution_scale_y=2 --swap_post_effect=fxaa --texture_cache_memory_limit_soft=1536 --texture_cache_memory_limit_hard=2560 --texture_cache_memory_limit_soft_lifetime=5 --d3d12_host_vsync=false --clear_memory_page_state=false --log_level=off
