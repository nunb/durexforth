code (do) ( limit first -- )
pla, zptmp sta,
pla, tay,

sp1 1+ lda,x pha, sp0 1+ lda,x pha,
sp1 lda,x pha, sp0 lda,x pha,
inx, inx, 

tya, pha,
zptmp lda, pha,
;code

\ leave stack
variable lstk 14 allot 
variable lsp lstk lsp !
: >l ( n -- ) lsp @ ! 2 lsp +! ;

: do
postpone (do) here dup >l ; immediate

: unloop r> r> r> 2drop >r ; no-tce

: leave
postpone unloop 
postpone branch here >l 0 , ; immediate no-tce

: resolve-leaves ( dopos -- )
begin -2 lsp +!
dup lsp @ @ < while
here lsp @ @ ! repeat drop ;

code (loop)
zptmp stx, tsx, \ x = stack pointer
103 inc,x 3 bne, 104 inc,x \ i++
103 lda,x 105 cmp,x 1 @@ beq, \ lsb
2 @:
\ not done, branch back
zptmp ldx, \ restore x
' branch jmp,
1 @:
104 lda,x 106 cmp,x 2 @@ bne, \ msb
\ loop done
\ skip branch addr
pla, clc, 3 adc,# zptmp2 sta,
pla, 0 adc,# zptmp2 1+ sta,
txa, clc, 6 adc,# tax, txs, \ sp += 6
zptmp ldx, \ restore x
zptmp2 (jmp),

: loop
postpone (loop) dup , resolve-leaves ; immediate no-tce

: (+loop) ( inc -- )
r> swap \ ret inc
r> \ ret inc i 
2dup + \ ret inc i i2
rot 0< if tuck swap else tuck then
r@ 1- -rot within 0= if
>r >r [ ' branch jmp, ] then
r> 2drop 2+ >r ;

: +loop
postpone (+loop) dup , resolve-leaves ; immediate no-tce

: i postpone r@ ; immediate
code j txa, tsx,
107 ldy,x zptmp sty, 108 ldy,x
tax, dex, 
sp1 sty,x zptmp lda, sp0 sta,x ;code
