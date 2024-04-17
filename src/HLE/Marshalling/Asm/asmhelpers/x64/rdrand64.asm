bits 64

rdrand64: ; ulong rdrand64()
    rdrand rax
    ret
