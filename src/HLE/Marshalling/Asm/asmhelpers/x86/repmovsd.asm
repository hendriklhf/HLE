bits 32

repmovsd:
    push esi
    push edi
    mov esi, edx
    mov edi, ecx
    mov ecx, [esp+12]
    rep movsd
    pop edi
    pop esi
    ret 4
