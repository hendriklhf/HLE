bits 64

repmovsq: ; void repmovsq(void* destination, void* source, nuint byteCount)
    mov r9, rsi
    mov r10, rdi
    mov rsi, rdx
    mov rdi, rcx
    mov rcx, r8
    rep movsq
    mov rsi, r9
    mov rdi, r10
    ret
