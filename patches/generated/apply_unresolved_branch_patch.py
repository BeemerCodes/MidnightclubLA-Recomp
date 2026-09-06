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

    # 60 FPS Patch - Part 1 (Branch override)
    new_content = re.sub(
        r'// bne cr6,0x821bdbc8\s+if \(!ctx\.cr6\.eq\) goto loc_821BDBC8;',
        r'// b 0x821bdc34 (60 FPS Patch)\n\tgoto loc_821BDC34;',
        new_content
    )

    # 60 FPS Patch - Part 2 (Vsync Target)
    new_content = re.sub(
        r'loc_82419AA0:\s+// li r11,2\s+ctx\.r11\.s64 = 2;',
        r'loc_82419AA0:\n\t// li r11,1 (60 FPS Patch)\n\tctx.r11.s64 = 1;',
        new_content
    )

    # Disable Imposter Shadows Patch (Performance Boost)
    new_content = re.sub(
        r'// beq cr6,0x8230ca3c\s+if \(ctx\.cr6\.eq\) goto loc_8230CA3C;',
        r'// b 0x8230ca3c (Disable Imposter Shadows)\n\tgoto loc_8230CA3C;',
        new_content
    )

    # Disable Motion Blur Patch
    new_content = re.sub(
        r'// bl 0x8218a568\s+ctx\.lr = 0x8260D0BC;\s+sub_8218A568\(ctx, base\);',
        r'// li r3,0 (Disable Motion Blur Patch 1)\n\tctx.r3.s64 = 0;',
        new_content
    )
    new_content = re.sub(
        r'// bl 0x8218a568\s+ctx\.lr = 0x8260D0D8;\s+sub_8218A568\(ctx, base\);',
        r'// li r3,0 (Disable Motion Blur Patch 2)\n\tctx.r3.s64 = 0;',
        new_content
    )

    # Disable MSAA Patch
    new_content = re.sub(
        r'// addi r10,r11,-7944\s+ctx\.r10\.s64 = ctx\.r11\.s64 \+ -7944;\s+// lwz r11,4\(r10\)\s+ctx\.r11\.u64 = REX_LOAD_U32\(ctx\.r10\.u32 \+ 4\);',
        r'// addi r10,r11,-7944\n\tctx.r10.s64 = ctx.r11.s64 + -7944;\n\t// li r11, 1 (Disable MSAA Patch)\n\tctx.r11.s64 = 1;',
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
