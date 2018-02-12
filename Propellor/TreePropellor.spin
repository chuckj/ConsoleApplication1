'
'

CON
        _CLKMODE = XTAL1 + PLL16X
        _CLKFREQ = 80_000_000

        _CLK1MS = _CLKFREQ / 1000                       '80,000
        _CLK1uS = _CLK1MS / 1000                        '80

		_BITTIME = _CLK1uS * 4

        _CLKTICKS = 100                                 '100
        _CLKCLKS = _CLK1MS / _CLKTICKS                  '800
        

        '	Address info uses MOVS instruction - must be outa pins D8-D0
        TreeA0Bit = 0
        TreeA1Bit = 1
        TreeCS0Bit = 2
        TreeCS1Bit = 3
        TreeCS2Bit = 4
        TreeCS3Bit = 5

		'	Data info used MOVD instruction - must be pins D17-D9
        TreeD0Bit = 9
        TreeD1Bit = 10
        TreeD2Bit = 11
        TreeD3Bit = 12
        TreeD4Bit = 13
        TreeD5Bit = 14
        TreeD6Bit = 15
        TreeD7Bit = 16

		'	Control signals uses any other usused bits...
        TreeWr8255Bit = 18
		TreeZeroCrossBit = 19

        DmxInBit = 20


        GblBase = $0
        GblDMXBuffer = GblBase + 0
		GblDMXStartingSlot = 1
		GblDMXSlots = 312
		GblDMXBufferSize = 2 * GblDMXSlots

        GblTreeBuffer = GblDMXBuffer + GblDMXBufferSize
		GblTreeBufferSize = 39 * 102

        DmxRcvrCog = 1                              'DMX Receiver cog
        TreeCtlCog = 0								'Tree Control Cog                                

'        
'	8255a-5 timing rules: (all times minimums)
'		CS,A1,A0 stable before WR (leading edge) - 0ns
'		CS,A1,A0 stable after WR - 20ns
'		WR width - 300ns
'		D7-D0 valid before WR (trailing edge) - 100ns
'		D7-D0 valid adter WR - 30ns
'

Pub Start

    lockset(InitLock)

' Start DMX Rcvr cog
    coginit(DmxRcvrCog, @DmxRcvrEntry, 0)               'USB interface service

' Start tree controller cog
    coginit(TreeCtlCog, @TreeCtlEntry, 0)                 'Buffer manager service
                                                         
    
DAT
{{
              DDDD   M   M  X   X         RRRR    CCC   V   V  RRRR
              D   D  MM MM   X X          R   R  C   C  V   V  R   R
              D   D  M M M    X           RRR    C      V   V  RRR
              D   D  M   M   X X          R  R   C   C   V V   R  R
              DDDD   M   M  X   X         R   R   CCC     V    R   R
                
}}              
                        org     0
                


DmxRcvrEntry
'
'       DMX receiver
'
						mov		bufaddr,gbldmxbuffer
						mov		bufcnt,gbldmxslotcnt
:clear
						wrwork	zero,bufaddr
						add		bufaddr,#2
						djnz	bufcnt,#:clear


						'		======== set dira for dmxinbit ==============

						andn	dira,inputbits

						mov		ttimbas,cnt
						add		ttimbas,#_CLK1uS

break
                        waitcnt ttimbas,#_CLK1uS				'wait 1uS

						test    ina,dmxinmask  wz				'Break?
              if_nz     jmp     #break							'. Not zero, so not break - wait for break to start
			  						
						mov		brktimer,#0						'clear BREAK timer

:lup	
                        waitcnt ttimbas,#_CLK1uS				'wait 1uS

						test    ina,dmxinmask  wz				'Break?
              if_nz     jmp     #:end							'. No, break ended

						add		brktimer,#1						'increment break time
						jmp		#:lup							'. & wait for end

:end
						cmp		brktimer,#88	wz wc			'at least 88uS?
			  if_b		jmp		#break							'. No, try again

mab				
						mov		mabtimer,#0						.clear MAB timer
			  						
:lup
                        waitcnt ttimbas,#_CLK1uS				'wait 1uS

						test    ina,dmxinmask  wz				'MAB?
              if_z      jmp     #:end							'. No, MAB ended

						add		mabtimer,#1						'increment MAB time
						jmp		#:lup							'. & wait for end

:end
						cmp		mabtimer,#8		wz wc			'at least 8uS?
			  if_b		jmp		#break							'. No, try again
						
						call	getbyt							'get slot 0 - 'start code'
			  if_z		jmp		#break							'Framing error - possilby BREAK

						mov		bufaddr,gbldmxbuffer
						mov		bufcnt,gbldmxslots

slots
:lup
						waitcnt	ttimbas,#_CLK1uS				'wait a bit
						test    ina,dmxinmask  wz				'start bit?  Z if start is present
			  if_nz		jmp		#:lup							'not yet - wait for it

						call	getbyt
			  if_z		jmp		#break							'Framing error - may be break

			  			cmp		byt,#100	wc					'normalize
			  if_nc		mov		byt,#100						'. value to <= 100

						wrbyte	byt,bufaddr						'write word
						add		bufaddr,#2						'. & bump @
						djnz	bufcnt,#:lup					'no - loop
						jmp		#break							'yes - should be done

getbyt
						mov		bitmask,#1						'init bit mask
						mov		bitcnt,#8						'. & #'bits
                        add		ttimbas,#_BITTIME*3/2-_CLK1uS	'skip to center of first data bit

:lup
                        waitcnt ttimbas,#_BITTIME				'wait for next bit

						test    ina,dmxinmask  wz				'move data bit
						muxz	byt,bitmask						'. into byte
						shl		bitmask							'adjust mask
			  if_nz		djnz	bitcnt,#:lup					'. No, loop

                        waitcnt ttimbas,#_CLK1uS				'wait for next bit

						test    outa,dmxinmask  wz				'stop bit?  Z if stop bit missing - possibly BREAK start
getbyt_ret				ret


dmxinmask				long	1 << DmxInBit
gbldmxbuffer			long	GblDMXBuffer
gbldmxbuffersize		long	GblDMXBufferSize
gbldmsslots				long	GblDMXSlots

inputbits				long	1 << DmxInBit
dmxinmask				long	1 << DmxInBit

ttimbas					long	0
dmxbufaddr				long	0
dmxbufcnt				long	0
brktimer				long	0
mabtimer				long	0
bufaddr					long	0
bufcnt					long	0
bitmask					long	0
bitcnt					long	0
byt						long	0


                        fit     496

DAT

{{
              TTTTT  RRRR   EEEEE  EEEEE       DDDD   RRRR   IIIII  V   V  EEEEE  RRRR
                T    R   R  E      E           D   D  R   R    I    V   V  E      R   R
                T    RRR    EEEE   EEEE        D   D  RRR      I    V   V  EEEE   RRR
                T    R  R   E      E           D   D  R  R     I     V V   E      R  R
                T    R   R  EEEEE  EEEEE       DDDD   R   R  IIIII    V    EEEEE  R   R
}}

                        org     0
'
'
' Entry
'
TreeCtlEntry

ReadyToGo
						mov		evenoddflag,#0
						or		dira,outputbits
						andn	dira,inputbits


'	Get latest values from main RAM, then index the menory array based on value and index, and set the corresponding bits

'
'	sync to line
'
resync
						mov		debounce,ones					'init to ones

:notzeros
						test	ina,#zerocross	wc				'debounce
						rcl		debounce,#1	wz					'. zero-cross - all zeros?
			  if_nz		jmp		#:notzeros						'No - continue

compute
'
'	clear prior array values
'
						mov		arrayaddr,array
						mov		arraycnt,arraysizewords
:clear
						wrword	zero,arrayaddr					'clear
						add		arrayaddr,#2					'. the
						djnz	arraycnt,#:clear				'.  array

'
'	compute new array
'
						mov		bufaddr,buffer					'312
						mov		bufcnt,gbldmxlots				'312	
						mov		arraybase,#array+1				'dsbyte 102*40
						mov		arraymsk,#$01010101

:lup
						rdbyte	array,bufaddr					'load value
						add		bufaddr,#1						'. & bump @

						add		array,arraybase					'base
						
						ldbyte	byt,array						'set
						or		byt,arraymask					'
						wrbyte	byt,array						'. corresponding bit in byte

						sub		array,#1						'Bump @ to next sequence

						ldbyte	byt,array						'set
						or		byt,arraymask					'
						rol		arraymask,#1	wc						'**Slide & save bit shifted into bit 0
						wrbyte	byt,array						'. corresponding bit there too

			  if_c		add		arraybase,#102					'If carry around, increment base
              
						djnz	bufcnt,#:lup

'
'	sync to line
'
						mov		debounce,#0						'set to all zeros

:lup
						test	ina,#zerocross	wc				'debounce
						rcl		debounce,#1						'. zero-cross
						cmp		debounce,ones	wz				'all ones?
			  if_nz		jmp		#:lup


						mov		arrayadr,array					'init to GblTreeBuffer + 100
						xor		evenoddflag,#1	wc
						addx	arrayadr,#0						'or array+101 (even/add cycles)


blast
						movs	outa,#0							'set chip select 0, A1.A0 = zeros (first port)
						mov		chipcnt,#13						'set for all 13 chips
						mov		workadr,arrayadr				'set initial address
						jmp		#:skip

:lup
						add		outa,#2							'set to next chip select, A1.A0 to 0b00

:skip
						andn	outa,wr8255						'begin wr
						ldbyte	byt,workadr						'load first byte
						movd	outa,byt						'. & send to 8255s
						add		workadr,@102					'. & increment to next data row
						nop
						nop
						or		outa,wr8255						'end wr

						nop
						add		outa,#1							'set A1.A0 to 0b01
						andn	outa,wr8255						'begin wr
						ldbyte	byt,workadr						'load next byte
						movd	outa,byt						'. & send to 8255s
						add		workadr,@102					'. & increment to next data row
						nop
						nop
						or		outa,wr8255						'end wr

						nop
						add		outa,#1							'set A1.A0 to 0b10
						andn	outa,wr8255						'begin wr
						ldbyte	byt,workadr						'load next byte
						movd	outa,byt						'. & send to 8255s
						add		workadr,@102					'. & increment to next data row
						nop
						nop
						or		outa,wr8255						'end wr

						djnz	chipcnt,#:lup					'loop for all 13 chips

						
						sub		arrayadr,#2

						wait a while

						djnz	???,#:lup


reset
						movs	outa,#3							'select first chip, a1&a0=control reg
						movd	outa,cmd8255init				'cmd = 0x80 - all ports - simple output
						mov		chipcnt,#13						'set loop for all 13 chips

:lup
						andn	outa,wr8255						'begin  write
						nop		'								'. wait a while (min wr is 300ns = 6 inst)
						nop		'
						nop		'
						nop		'
						nop		'
						nop		'
						or		outa,wr8255						'end write
						nop		'
						add		outa,#4							'next chip
						djnz	chipcnt,#:lup					'. & init all of them

						jmp		resync							'begin next cycle


zero					long	0
ones					long	-1

bufaddr					long	GblDMXBuffer
bufcnt					long	GblDMXSlots
wr2855					long	1 << TreeWr8255Bit
zerocross				long	1 << TreeZeroCrossBit
cmd3255init				long	$80						' all 3 ports - simple output

outputbits				long	1 << TreeA0Bit  |
								1 << TreeA1Bit 	|
								1 << TreeCS0Bit	|
								1 << TreeCS1Bit	|
								1 << TreeCS2Bit	|
								1 << TreeCS3Bit	|
								1 << TreeD0Bit 	|
								1 << TreeD1Bit 	|
								1 << TreeD2Bit 	|
								1 << TreeD3Bit 	|
								1 << TreeD4Bit 	|
								1 << TreeD5Bit 	|
								1 << TreeD6Bit 	|
								1 << TreeD7Bit 	|
								1 << TreeWr8255Bit
inputbits				long	1 << TreeZeroCrossBit


array					long	GblTreeBuffer+100
arraysizewords			long	GblTreeBufferSize/2
buffer					long	GblDMXBuffer
gbldmxslots				long	GblDMXSlots

debounce				long	0
arrayadr				long	0
chipcnt					long	0

workaddr				long	0
byt						long	0


                        fit     496
                        