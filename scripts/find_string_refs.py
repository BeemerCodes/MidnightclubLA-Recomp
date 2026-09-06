import struct
import sys

def find_ppc_address_loads(filepath, target_address):
    # Target address is 0x82012c74
    # high = 0x8201, low = 0x2C74
    high_val = (target_address >> 16) & 0xFFFF
    low_val = target_address & 0xFFFF
    
    # Adjust for PowerPC sign extension in addi if low_val >= 0x8000
    if low_val >= 0x8000:
        high_val = (high_val + 1) & 0xFFFF
        
    print(f"Searching for lis ..., 0x{high_val:04X} followed by addi ..., 0x{low_val:04X}")

    with open(filepath, 'rb') as f:
        data = f.read()

    # XEX base address mapping
    # Usually the code section starts at 0x82000000.
    # We will search the whole file.
    
    for i in range(0, len(data) - 8, 4):
        instr = struct.unpack('>I', data[i:i+4])[0]
        if (instr >> 26) == 15: # lis opcode is 15
            imm16 = instr & 0xFFFF
            if imm16 == high_val:
                reg_d = (instr >> 21) & 0x1F
                
                # Look ahead a few instructions for the addi (opcode 14)
                for j in range(i + 4, min(i + 40, len(data)), 4):
                    next_instr = struct.unpack('>I', data[j:j+4])[0]
                    if (next_instr >> 26) == 14: # addi
                        next_imm16 = next_instr & 0xFFFF
                        if next_imm16 == low_val:
                            print(f"Found match at file offset 0x{i:X}")
                            print(f"lis instruction: 0x{instr:08X}")
                            print(f"addi instruction at offset 0x{j:X}: 0x{next_instr:08X}")

if __name__ == '__main__':
    find_ppc_address_loads(sys.argv[1], 0x82012c74) # fiPackfile::Open
    print("---")
    find_ppc_address_loads(sys.argv[1], 0x82012ae8) # fiPackfile::OpenBulk
