import os
import re
import glob

def patch_file(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    pattern = r'REX_FATAL\("Unresolved branch from 0x[A-Fa-f0-9]+ to 0x([A-Fa-f0-9]+)"\)'

    def replacer(match):
        target_addr = match.group(1).upper()
        label = f'loc_{target_addr}'
        if f'{label}:' in content:
            return f'goto {label}'
        else:
            return match.group(0)

    new_content = re.sub(pattern, replacer, content)

    # =========================================================================
    # 1. High-Precision Master Timer Unlock (sub_821BDA90 @ 0x821BDA90)
    # =========================================================================
    # Do NOT skip 0x821BDB08 (m_bResetTimer check), because 0x821BDB0C..0x821BDB58
    # accumulates real wall-clock delta time (f0) into m_fPrevTime (16(r3)),
    # m_fTime (20(r3)), m_fSystemTime (24(r3)), and m_dTime (48(r3)).
    # Instead, bypass the fixed-step branches (0x821BDB20, 0x821BDB2C) and
    # jump to loc_821BDC34 at 0x821BDB5C right after the cumulative clocks are
    # updated, bypassing the 30 FPS delta-time floor clamp (0x821BDB60..0x821BDB8C).
    new_content = re.sub(
        r'// bne cr6,0x821bdb90\s+if \(!ctx\.cr6\.eq\) goto loc_821BDB90;',
        r'// nop (60+ FPS Timer Unlock: bypass fixed-step clamp)\n\t(void)0;',
        new_content
    )
    new_content = re.sub(
        r'// beq cr6,0x821bdc34\s+if \(ctx\.cr6\.eq\) goto loc_821BDC34;',
        r'// b 0x821bdc34 (60+ FPS Timer Unlock: use unclamped wall-clock f0)\n\tgoto loc_821BDC34;',
        new_content
    )

    # Set guest presentation interval to 1 (60 Hz base cadence instead of 30 Hz)
    new_content = re.sub(
        r'loc_82419AA0:\s+// li r11,2\s+ctx\.r11\.s64 = 2;',
        r'loc_82419AA0:\n\t// li r11,1 (60 FPS Presentation Interval)\n\tctx.r11.s64 = 1;',
        new_content
    )

    # =========================================================================
    # 2. Traffic Light Reflection Bug Fix (RTV Path @ 0x8237B16C)
    # =========================================================================
    # In sub_8237AC60, 0x8237B11C (bl 0x822FC250 -> sub_822FB7A8) renders the
    # actual 3D traffic light corona sprites on the traffic poles, whereas
    # 0x8237B16C (bl 0x822FB508) projects offscreen traffic signal reflection
    # quads into 0x8287E3B0 (__lightglowmap reflection array), smearing solid
    # red/green across car bodies and windows at 2x resolution on the RTV path.
    new_content = re.sub(
        r'// bl 0x822fb508\s+ctx\.lr = 0x8237B170;\s+(?:ppc_)?sub_822FB508\(ctx, base\);',
        r'// nop (Eliminate traffic signal reflection quad pollution on RTV path)\n\t(void)0;',
        new_content
    )

    # =========================================================================
    # 3. Complete Motion Blur & Redundant Post-FX / Imposter Shadow Stripping
    # =========================================================================
    # Skip imposter shadow pass (0x8230C9DC)
    new_content = re.sub(
        r'// beq cr6,0x8230ca3c\s+if \(ctx\.cr6\.eq\) goto loc_8230CA3C;',
        r'// b 0x8230ca3c (Disable Imposter Shadows)\n\tgoto loc_8230CA3C;',
        new_content
    )

    # Prevent allocation of mcMotionBlurPass and fullscreen blur passes
    # in sub_8260D098 (0x8260D0B8 and 0x8260D0D4)
    new_content = re.sub(
        r'// bl 0x8218a568\s+ctx\.lr = 0x8260D0BC;\s+(?:ppc_)?sub_8218A568\(ctx, base\);',
        r'// li r3,0 (Disable Motion Blur Pass 1)\n\tctx.r3.s64 = 0;',
        new_content
    )
    new_content = re.sub(
        r'// bl 0x8218a568\s+ctx\.lr = 0x8260D0D8;\s+(?:ppc_)?sub_8218A568\(ctx, base\);',
        r'// li r3,0 (Disable Motion Blur Pass 2)\n\tctx.r3.s64 = 0;',
        new_content
    )

    # Force 1x MSAA to eliminate redundant EDRAM multisample resolve bandwidth
    new_content = re.sub(
        r'// addi r10,r11,-7944\s+ctx\.r10\.s64 = ctx\.r11\.s64 \+ -7944;\s+// lwz r11,4\(r10\)\s+ctx\.r11\.u64 = REX_LOAD_U32\(ctx\.r10\.u32 \+ 4\);',
        r'// addi r10,r11,-7944\n\tctx.r10.s64 = ctx.r11.s64 + -7944;\n\t// li r11, 1 (Disable MSAA Patch)\n\tctx.r11.s64 = 1;',
        new_content
    )

    # =========================================================================
    # 4. Bink Startup FMV Crash Bypass (sub_822CBF40 @ 0x822CBF8C)
    # =========================================================================
    # Transition startup Bink intro state machine directly to state 3/4 (boot
    # into main menu/garage) without spawning the Bink xenon decoder thread.
    # Real-time 3D RAGE campaign cutscenes do not use sub_822CBF40 and remain
    # completely intact.
    new_content = re.sub(
        r'// bne cr6,0x822cbfdc\s+if \(!ctx\.cr6\.eq\) goto loc_822CBFDC;',
        r'// b 0x822cbfdc (Bypass Startup Bink FMVs cleanly)\n\tgoto loc_822CBFDC;',
        new_content
    )

    if new_content != content:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(new_content)
        print(f"Patched {filepath}")

def main():
    cpp_files = glob.glob('generated/default/*.cpp')
    for f in cpp_files:
        patch_file(f)

if __name__ == '__main__':
    main()
