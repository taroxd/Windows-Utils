extern Sleep:PROC
includelib kernel32.lib
.code
main proc
    mov  ecx,0FFFFFFFFh  ;INFINITY
    call Sleep
    ret
main endp
end
