bits 64

CHAR_A_AND_ZERO_DIFF equ 'A' - 0xA - '0'
CHAR_ZERO equ '0'
CHAR_NINE equ '9'

ParseHexColor: ; void ParseHexColor(byte[6] hexstr, byte[3] rgb)
    xor rax, rax

    ; first char
    mov r8b, byte [rcx]
    cmp r8b, CHAR_NINE
    setg al
    lea r10, [rax*8]
    sub r10b, al
    add r10b, CHAR_ZERO
    sub r8b, r10b
    shl r8b, 4
    ; second char
    mov r9b, byte [rcx+1]
    cmp r9b, CHAR_NINE
    setg al
    lea r10, [rax*8]
    sub r10b, al
    add r10b, CHAR_ZERO
    sub r9b, r10b
    or r8b, r9b
    mov byte [rdx], r8b

    ; third char
    mov r8b, byte [rcx+2]
    cmp r8b, CHAR_NINE
    setg al
    lea r10, [rax*8]
    sub r10b, al
    add r10b, CHAR_ZERO
    sub r8b, r10b
    shl r8b, 4
    ; forth char
    mov r9b, byte [rcx+3]
    cmp r9b, CHAR_NINE
    setg al
    lea r10, [rax*8]
    sub r10b, al
    add r10b, CHAR_ZERO
    sub r9b, r10b
    or r8b, r9b
    mov byte [rdx + 1], r8b

    ; fifth char
    mov r8b, byte [rcx+4]
    cmp r8b, CHAR_NINE
    setg al
    lea r10, [rax*8]
    sub r10b, al
    add r10b, CHAR_ZERO
    sub r8b, r10b
    shl r8b, 4
    ; sixth char
    mov r9b, byte [rcx+5]
    cmp r9b, CHAR_NINE
    setg al
    lea r10, [rax*8]
    sub r10b, al
    add r10b, CHAR_ZERO
    sub r9b, r10b
    or r8b, r9b
    mov byte [rdx + 2], r8b

    ret
