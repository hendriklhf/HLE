bits 64

CHAR_A_AND_ZERO_DIFF equ 'A' - 0xA - '0'
CHAR_ZERO equ '0'
CHAR_NINE equ '9'

ParseHexColor: ; Color ParseHexColor(byte[6] hexstr)
    xor eax, eax
    xor r10, r10

    ; first char
    mov al, byte [rcx]
    cmp al, CHAR_NINE
    setg r10b
    lea r9, [r10*8]
    sub r9b, r10b
    add r9b, CHAR_ZERO
    sub al, r9b
    shl al, 4
    ; second char
    mov dl, byte [rcx+1]
    cmp dl, CHAR_NINE
    setg r10b
    lea r9, [r10*8]
    sub r9b, r10b
    add r9b, CHAR_ZERO
    sub dl, r9b
    or al, dl
    shl ax, 4

    ; third char
    mov dl, byte [rcx+2]
    cmp dl, CHAR_NINE
    setg r10b
    lea r9, [r10*8]
    sub r9b, r10b
    add r9b, CHAR_ZERO
    sub dl, r9b
    or al, dl
    shl ax, 4
    ; forth char
    mov dl, byte [rcx+3]
    cmp dl, CHAR_NINE
    setg r10b
    lea r9, [r10*8]
    sub r9b, r10b
    add r9b, CHAR_ZERO
    sub dl, r9b
    or al, dl
    shl eax, 4

    ; fifth char
    mov dl, byte [rcx+4]
    cmp dl, CHAR_NINE
    setg r10b
    lea r9, [r10*8]
    sub r9b, r10b
    add r9b, CHAR_ZERO
    sub dl, r9b
    or al, dl
    shl eax, 4
    ; sixth char
    mov dl, byte [rcx+5]
    cmp dl, CHAR_NINE
    setg r10b
    lea r9, [r10*8]
    sub r9b, r10b
    add r9b, CHAR_ZERO
    sub dl, r9b
    or al, dl

    ret
