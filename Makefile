C1541   = c1541
AS = acme
TAG = `git describe --tags --abbrev=0 || svnversion --no-newline`

# generic rules

all:	durexforth.d64

durexforth.prg: durexforth.a number.a math.a move.a disk.a lowercase.a
	@$(AS) durexforth.a

FORTHLIST=base debug vi asm gfx gfxdemo rnd sin ls turtle fractals sprite doloop sys float labels mml mmldemo sid spritedemo test testcore testcoreplus tester format

durexforth.d64: durexforth.prg forth_src/base.fs forth_src/debug.fs forth_src/vi.fs forth_src/asm.fs forth_src/gfx.fs forth_src/gfxdemo.fs forth_src/rnd.fs forth_src/sin.fs forth_src/ls.fs forth_src/turtle.fs forth_src/fractals.fs forth_src/sprite.fs forth_src/doloop.fs forth_src/sys.fs forth_src/float.fs forth_src/labels.fs forth_src/mml.fs forth_src/mmldemo.fs forth_src/sid.fs forth_src/spritedemo.fs forth_src/test.fs Makefile ext/petcom forth_src/testcore.fs forth_src/testcoreplus.fs forth_src/tester.fs forth_src/format.fs
	$(C1541) -format "durexforth$(TAG),DF"  d64 durexforth.d64 # > /dev/null
	$(C1541) -attach $@ -write durexforth.prg durexforth # > /dev/null
# $(C1541) -attach $@ -write debug.bak
	mkdir -p build
	echo -n "aa" > build/header
	@for forth in $(FORTHLIST); do\
        cat build/header forth_src/$$forth.fs | ext/petcom - > build/$$forth.pet; \
        $(C1541) -attach $@ -write build/$$forth.pet $$forth; \
    done;

clean:
	rm -f *.lbl *.prg *.d64 
	rm -rf build
