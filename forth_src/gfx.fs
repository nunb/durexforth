a000 value bmpbase
8c00 value colbase

code hires 
bb lda,# d011 sta, \ enable
15 lda,# dd00 sta, \ vic bank 2
38 lda,# d018 sta,
56 lda,# 1 sta, \ no basic
;code

code lores
9b lda,# d011 sta,
17 lda,# dd00 sta,
17 lda,# d018 sta,
;code

: clrcol ( fgbgcol -- )
colbase 3e8 rot fill
bmpbase 1f40 0 fill ;

: blkcol ( col row c -- )
-rot 28 * + colbase + c! ;

header mask
80 c, 40 c, 20 c, 10 c,
8 c, 4 c, 2 c, 1 c,

variable penx variable peny
0 penx ! 0 peny !

\ blit operations for plot, line
header blitop
0 , \ doplot
0 , \ lineplot

create .blitloc
sp0 lda,x zptmp sta,
7 and,# zptmp3 sta,
sp1 lda,x zptmp 1+ sta,

zptmp lda, f8 and,# zptmp sta,

\ * 8
zptmp asl, zptmp 1+ rol,
zptmp asl, zptmp 1+ rol,
zptmp asl, zptmp 1+ rol,

zptmp lda, zptmp2 sta,
zptmp 1+ lda, zptmp2 1+ sta,

\ * 20
zptmp asl, zptmp 1+ rol,
zptmp asl, zptmp 1+ rol,

clc,
zptmp lda, zptmp2 adc, zptmp sta,
zptmp 1+ lda, zptmp2 1+ adc,
zptmp 1+ sta,
clc,
zptmp lda, zptmp3 adc, zptmp sta,
2 bcc, zptmp 1+ inc,

zptmp lda, sp0 sta,x
zptmp 1+ lda, sp1 sta,x

\ ...

' mask 100/
lda,# zptmp 1+ sta,

clc,
sp0 1+ lda,x 7 and,# ' mask adc,#
zptmp sta,
2 bcc, zptmp 1+ inc,

\ zptmp = mask
0 ldy,#
zptmp lda,(y) zptmp3 sta,

clc,
sp0 1+ lda,x f8 and,# sp0 adc,x sp0 sta,x
sp1 1+ lda,x sp1 adc,x clc, a0 adc,# sp1 sta,x
zptmp3 lda, sp0 1+ sta,x
0 lda,# sp1 1+ sta,x
rts,

code blitloc ( x y -- mask addr )
.blitloc jsr, ;code

: doplot ( x y -- )
blitloc tuck c@
[ here 1+ ' blitop ! ] or
swap c! ;

: chkplot ( x y -- )
over 13f > over c7 > or
if 2drop else doplot then ;

: plot ( x y -- )
2dup peny ! penx ! chkplot ;

: peek ( x y -- b )
blitloc c@ and ;

variable dy
variable sy variable sx
variable err variable 2err

variable mask variable addr

create lineplot ( -- )

\ penx @ 140 <
penx lda,# zptmp sta,
penx 100/ lda,# zptmp 1+ sta,
1 ldy,# zptmp lda,(y)
+branch beq,
1 cmp,# 1 beq, rts,
dey, zptmp lda,(y)
sec, 40 sbc,#
1 bcc, rts,
:+

\ peny @ c8 <
peny lda,# zptmp sta,
peny 100/ lda,# zptmp 1+ sta,
1 ldy,# zptmp lda,(y)
1 beq, rts,
dey, zptmp lda,(y)
sec, c8 sbc,#
1 bcc, rts,

\ addr
addr 100/
lda,# zptmp 1+ sta,
addr lda,# zptmp sta,

\ @
0 ldy,#
zptmp lda,(y) zptmp2 sta, iny,
zptmp lda,(y) zptmp2 1+ sta, dey,

\ c@ mask c@ or
zptmp2 lda,(y)
here ' blitop 2+ !
mask ora,

\ addr @ c!
zptmp2 sta,(y) rts,

variable dx2 variable dy2

create stepx
\ 2err @ dx2 @ < if
sec, 2err lda, dx2 sbc,
2err 1+ lda, dx2 1+ sbc,
3 bmi, lineplot jmp,

\ dx2 @ err +!
clc, dx2 lda, err adc, err sta,
dx2 1+ lda, err 1+ adc, err 1+ sta,
\ sy @ peny +!
clc, sy lda, peny adc, peny sta,
sy 1+ lda, peny 1+ adc, peny 1+ sta,

\ sy @ 1 = if down else up then
sy lda, 1 cmp,# +branch beq,
\ up
addr lda, 7 and,# +branch bne,
sec, addr lda, 38 sbc,# addr sta,
addr 1+ lda, 1 sbc,# addr 1+ sta,
:+ 
addr lda, 3 bne, addr 1+ dec, addr dec,
lineplot jmp,
:+ \ down
addr inc, 3 bne, addr 1+ inc,
addr lda, 7 and,# 3 beq, lineplot jmp,
clc, addr lda, 38 adc,# addr sta,
addr 1+ lda, 1 adc,# addr 1+ sta,
lineplot jmp,

create step ( 2err -- 2err )
\ err @ 2* 2err !
err lda, 2err sta,
err 1+ lda, 2err 1+ sta,
2err asl, 2err 1+ rol,

\ step up/down

\ 2err @ dy2 @ > if 
sec, dy2 lda, 2err sbc,
dy2 1+ lda, 2err 1+ sbc,
3 bmi, stepx jmp,

\ dy2 @ err +!
clc, dy2 lda, err adc, err sta,
dy2 1+ lda, err 1+ adc, err 1+ sta,
\ sx @ penx +! 
clc, sx lda, penx adc, penx sta,
sx 1+ lda, penx 1+ adc, penx 1+ sta,

\ sx @ 1 = if maskror else maskrol then
sx lda, 1 cmp,# +branch bne,
\ right
\ maskror.mask>>1,addr+8?
mask lsr, 3 bcs, stepx jmp,
80 lda,# mask sta,
clc, addr lda, 8 adc,# addr sta,
3 bcc, addr 1+ inc, stepx jmp,
:+ \ left
\ mask<<1,addr-8?
mask asl, 3 bcs, stepx jmp,
1 lda,# mask sta,
sec, addr lda, 8 sbc,# addr sta,
3 bcs, addr 1+ dec, stepx jmp,

code doline
1 @: step jsr,
peny lda, sp0 cmp,x 1 @@ bne,
penx lda, sp0 1+ cmp,x 1 @@ bne,
peny 1+ lda, sp1 cmp,x 1 @@ bne,
penx 1+ lda, sp1 1+ cmp,x 1 @@ bne,
inx, inx, ;code

: line ( x y -- )
2dup peny @ - abs dy2 !
penx @ - abs dx2 !
2dup
peny @ swap < if 1 else ffff then sy !
penx @ swap < if 1 else ffff then sx !
dx2 @ dy2 @ - err !
dy2 @ negate dy2 !

penx @ peny @ blitloc addr ! mask !

doline ;

\ --- circle

0 value cx 0 value cy

: plot4 ( x y -- x y )
over cx + over cy + chkplot
over if \ x?
over cx swap - over cy + chkplot
then
dup if \ y?
over cx + over cy swap - chkplot
then
over 0<> over 0<> and if
over cx swap - over cy swap - chkplot
then ;

: plot8 ( x y -- x y )
plot4
2dup <> if
swap plot4 swap
then ;

: circle ( cx cy r -- )
dup negate err !
swap to cy
swap to cx
0 \ x y
begin 2dup < 0= while
plot8
dup err +!
1+
dup err +!
err @ 0< 0= if
over negate err +!
swap 1- swap
over negate err +!
then
repeat 2drop ;

: erase if
4d ['] xor else
d ['] or then ['] blitop @ ! 
['] blitop 2+ @ c! ;

\ --------------------------

\ paul heckbert seed fill
\ from graphics gems
variable stk
create dopush
stk lda, zptmp sta,
stk 1+ lda, zptmp 1+ sta,

\ dy
0 ldy,# sp0 lda,x zptmp sta,(y)
\ xr
iny, sp0 1+ lda,x zptmp sta,(y)
iny, sp1 1+ lda,x zptmp sta,(y)
\ xl
iny, sp0 2 + lda,x zptmp sta,(y)
iny, sp1 2 + lda,x zptmp sta,(y)
\ y
iny, sp0 3 + lda,x zptmp sta,(y)

clc, stk lda, 6 adc,# stk sta,
3 bcc, stk 1+ inc, rts,

code spush ( y xl xr dy -- )
\ y out of bounds?
clc, sp0 lda,x sp0 3 + adc,x tay,
sp1 lda,x sp1 3 + adc,x +branch bne,
tya, sec, c8 cmp,# 3 bcs, dopush jsr,
:+
inx, inx, inx, inx, ;code

variable x1 variable x2

code spop ( -- y )
stk lda,
sec, 6 sbc,# zptmp sta, stk sta,
3 bcs, stk 1+ dec,
stk 1+ lda, zptmp 1+ sta,

\ ff = if ffff else 1 then dy !
0 ldy,# zptmp lda,(y)
dy sta, dy 1+ sta,
1 cmp,# 3 bne, dy 1+ sty,

dex,
sp1 sty,x \ msb y=0
iny, zptmp lda,(y) x2 sta,
iny, zptmp lda,(y) x2 1+ sta,
iny, zptmp lda,(y) x1 sta,
iny, zptmp lda,(y) x1 1+ sta,
iny, zptmp lda,(y) sp0 sta,x
;code

variable l

\ ---

create .bitblt ( mask addr --
                  mask addr )
sp0 lda,x zptmp sta,
sp1 lda,x zptmp 1+ sta,
0 ldy,# zptmp lda,(y)
sp0 1+ ora,x zptmp sta,(y)
\ 1 penx +! swap 2/ swap 
penx inc, 3 bne, penx 1+ inc,
sp0 1+ lsr,x rts,

create rightend
\ nip 80 swap \ mask
80 lda,# sp0 1+ sta,x 
0 lda,# sp1 1+ sta,x

:-
sp0 1+ lda,x 1 bne, rts,
sp0 lda,x zptmp sta,
sp1 lda,x zptmp 1+ sta,
0 ldy,# zptmp lda,(y)
sp0 1+ and,x 1 beq, rts,
.bitblt jsr, jmp, \ recurse

create bytewise
\ penx @ 140 < if 
penx 1+ lda, 0 cmp,# +branch beq,
3f lda,# penx cmp, 1 bcs, rts,
:+

:- \ 8 +
clc, sp0 lda,x 8 adc,# sp0 sta,x
2 bcc, sp1 inc,x
\ penx=140?
penx lda, 40 cmp,# +branch bne,
penx 1+ lda, 1 cmp,# +branch bne,
rts,
:+ :+
sp0 lda,x zptmp sta,
sp1 lda,x zptmp 1+ sta,
0 ldy,# zptmp lda,(y)
rightend -branch bne,

\ ff over c!
ff lda,# zptmp sta,(y)
\ 8 penx +!
clc, penx lda, 8 adc,# penx sta,
3 bcc, penx 1+ inc,
jmp, \ recurse

create leavel
\ 2drop nip penx @ swap 
inx, inx,
penx lda, sp0 1+ sta,x
penx 1+ lda, sp1 1+ sta,x rts,

\ this one must be fast
code fillr ( x y -- newx y )
\ over 140 >= if exit then
sp1 1+ lda,x 0 cmp,# +branch beq,
3f lda,# sp0 1+ cmp,x 1 bcs, rts,
:+

\ over penx !
sp0 1+ lda,x penx sta,
sp1 1+ lda,x penx 1+ sta,
\ 2dup blitloc \ x y mask addr
dex, dex,
sp0 2 + lda,x sp0 sta,x
sp1 2 + lda,x sp1 sta,x
sp0 3 + lda,x sp0 1+ sta,x
sp1 3 + lda,x sp1 1+ sta,x 
.blitloc jsr,

\ leftend ( x y mask addr --
\           x y mask addr more? )
:-
sp0 1+ lda,x +branch bne,
\ continue bytewise
bytewise jsr, leavel jsr, ;code
:+
sp0 lda,x zptmp sta,
sp1 lda,x zptmp 1+ sta,
0 ldy,# zptmp lda,(y)
sp0 1+ and,x +branch beq,
\ done
leavel jsr, ;code
:+
.bitblt jsr, jmp, \ recurse

code scanl
:-
\ x<0?
sp1 1+ lda,x 1 bpl, rts,

addr lda, zptmp sta,
addr 1+ lda, zptmp 1+ sta,
0 ldy,# zptmp lda,(y)
mask and, 1 beq, rts,

zptmp lda,(y)
mask ora, zptmp sta,(y)

mask asl, +branch bcc,
1 lda,# mask sta,
addr lda, sec, 8 sbc,# addr sta, 
3 bcs, addr 1+ dec,

:+ \ 1-
sp0 1+ lda,x 2 bne, sp1 1+ dec,x 
sp0 1+ dec,x
jmp, \ recurse

create .scanr
\ over l ! \ l=x
sp0 1+ lda,x l sta, 
sp1 1+ lda,x l 1+ sta,
;code

code scanr ( x y mask addr -- newx y )
sp0 lda,x addr sta,
sp1 lda,x addr 1+ sta,
sp0 1+ lda,x mask sta,
inx, inx,

:-
\ addr @ c@ mask c@ and
addr lda, zptmp sta,
addr 1+ lda, zptmp 1+ sta,
0 ldy,# zptmp lda,(y)
mask and, .scanr -branch beq,

\ x<=x2?
x2 1+ lda, sp1 1+ cmp,x .scanr -branch bcc,
+branch bne,
x2 lda, sp0 1+ cmp,x .scanr -branch bcc,
:+

mask lsr, +branch bne,
80 lda,# mask sta,
clc, addr lda, 8 adc,# addr sta,
3 bcc, addr 1+ inc,

:+ \ x++
sp0 1+ inc,x 2 bne, sp1 1+ inc,x
jmp, \ recurse

: paint ( x y -- )
2dup c8 < 0= swap 140 < 0= or
if 2drop exit then
2dup peek if 2drop exit then

here stk !
\ push y x x 1
2dup swap dup 1 spush
\ push y+1 x x -1
1+ swap dup ffff spush

begin here stk @ < while
spop dy @ + \ y

\ left line
x1 @ over \ y x y
2dup blitloc addr ! mask !
scanl
over x1 @ \ y x y x x1
< 0= if
branch [ here >r 0 , ] \ goto skip
then
\ y x y ...
over 1+ dup l ! 
\ y x y l
x1 @ < if \ l < x1?
\ push y,l,x1-1,-dy
dup l @ x1 @ 1- dy @ negate spush
then
\ y x y
nip x1 @ 1+ swap \ x=x1+1

begin
fillr
\ push y,l,x-1,dy
dup l @ 3 pick 1- dy @ spush

\ leak on right?
over x2 @ 1+ > if
\ push y,x2+1,x-1,-dy
dup x2 @ 1+ 3 pick 1- dy @ negate spush
then

\ skip: y x y
[ r> here swap ! ]

swap 1+ swap
2dup blitloc scanr 

\ y x y
over x2 @ > until

2drop drop repeat ; 

: text ( col row str strlen -- )
\ addr=dst
rot 140 * addr !
rot 8 * bmpbase + addr +!
\ disable interrupt,enable char rom
1 c@ dup >r fb and 1 sei c!
begin ?dup while
swap dup c@ 8 * d800 + \ strlen str ch
addr @ 8 move
1+ swap 8 addr +! 1- repeat
r> 1 c! cli drop ;

: drawchar ( col row srcaddr -- )
swap 140 * rot 8 * + bmpbase +
8 move ;
