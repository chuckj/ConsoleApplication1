CON
		_CLKMODE = XTAL1 + PLL16X
		_CLKFREQ = 80_000_000
		
		_CLK1MS = _CLKFREQ / 1000                       '80,000
		_CLK1uS = _CLK1MS / 1000                        '80
		
		_BITTIME = _CLK1uS * 4
		
		_CLKTICKS = 100                                 '100
		_CLKCLKS = _CLK1MS / _CLKTICKS                  '800
		
		InitLock = 7


		'	DMX pins
		DmxBreakBit = 26
		DmxInBit = 27



        '	Tree Address info uses MOVS instruction - must be outa pins D8-D0
		TreeA0Bit = 0
		TreeA1Bit = 1
		TreeCS0Bit = 2
		TreeCS1Bit = 3
		TreeCS2Bit = 4
		TreeCS3Bit = 5
		
		'	Tree Data info used MOVD instruction - must be outa pins D17-D9
		TreeD0Bit = 9
		TreeD1Bit = 10
		TreeD2Bit = 11
		TreeD3Bit = 12
		TreeD4Bit = 13
		TreeD5Bit = 14
		TreeD6Bit = 15
		TreeD7Bit = 16
		
		'	Control signals use any other usused bits...
		TreeWr8255Bit = 18
		TreeZeroCrossBit = 19
		
		TreePhaseBit = 20
		TreeResetBit = 21



		GblBase = $0
		GblDMXBuffer = GblBase + 0
		GblDMXStartingSlot = 1
		GblDMXSlots = 312
		GblDMXBufferSize = GblDMXSlots
		
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

PUB Start

    lockset(InitLock)

' Start DMX Rcvr cog
    coginit(DmxRcvrCog, @DmxRcvrEntry, 0)               'USB interface service

' Start tree controller cog
    coginit(TreeCtlCog, @TreeCtlEntry, 0)                 'Buffer manager service
                                                         
    
DAT
{{
              DDDD   M   M  X   X         RRRR   EEEEE   CCC   EEEEE  IIIII  V   V  EEEEE  RRRR
              D   D  MM MM   X X          R   R  E      C   C  E        I    V   V  E      R   R
              D   D  M M M    X           RRR    EEEE   C      EEEE     I    V   V  EEEE   RRR
              D   D  M   M   X X          R  R   E      C   C  E        I     V V   E      R  R
              DDDD   M   M  X   X         R   R  EEEEE   CCC   EEEEE  IIIII    V    EEEEE  R   R
                
}}              
                        org     0
                
'
'       DMX receiver
'

DmxRcvrEntry
:init                   lockset dinitlock	wc					'wait for init to finish
              if_c      jmp     #:init
                        lockclr dinitlock

						or		dira,doutputbits				'setup i/o
						andn	dira,dinputbits					'  ports

                        mov		dslotadr,ddmxbuffer
						mov		dslotcnt,ddmxslots
:tmp
                        wrbyte  dslotadr,dslotadr
                        add     dslotadr,#1
                        djnz    dslotcnt,#:tmp
                        jmp     #$


						mov		dtimbas,cnt						'set for intital 1uS
						add		dtimbas,#_CLK1uS				'. interval

break
                        waitcnt dtimbas,#_CLK1uS				'wait 1uS

						test    ina,ddmxinmask  wz				'Break?
              if_nz     jmp     #break							'. Not zero, so not break - wait for break to start
			  						
						mov		dbrktimer,#0					'clear BREAK timer

:lup	
                        waitcnt dtimbas,#_CLK1uS				'wait 1uS

						test    ina,ddmxinmask  wz				'Break?
              if_nz     jmp     #:end							'. No, break ended

						add		dbrktimer,#1					'increment break time
						jmp		#:lup							'. & wait for end

:end
						cmp		dbrktimer,#88	wz,wc			'at least 88uS?
			  if_b		jmp		#break							'. No, try again

						or		outa,ddmxbreakbit

mab				
						mov		dmabtimer,#0						'clear MAB timer
			  						
:lup
                        waitcnt dtimbas,#_CLK1uS				'wait 1uS

						test    ina,ddmxinmask  wz				'MAB?
              if_z      jmp     #:end							'. No, MAB ended

						add		dmabtimer,#1					'increment MAB time
						jmp		#:lup							'. & wait for end

:end
						andn	outa,ddmxbreakbit

						cmp		dmabtimer,#8 wz,wc				'at least 8uS?
			  if_b		jmp		#break							'. No, try again
						
						call	#getbyt							'get slot 0 - 'start code'
			  if_z		jmp		#break							'Framing error - possilby BREAK

						mov		dslotadr,ddmxbuffer
						mov		dslotcnt,ddmxslots

slots
:lup
						waitcnt	dtimbas,#_CLK1uS				'wait a bit
						test    ina,ddmxinmask  wz				'start bit?  Z if start is present
			  if_nz		jmp		#:lup							'not yet - wait for it

						call	#getbyt
			  if_z		jmp		#break							'Framing error - may be break

						wrbyte	dbyt,dslotadr					'write word
						add		dslotadr,#1						'. & bump @
						djnz	dslotcnt,#:lup					'no - loop
						jmp		#break							'yes - should be done

getbyt
						mov		dbitmask,#1						'init bit mask
						mov		dbitcnt,#8						'. & #'bits
                        add		dtimbas,#_BITTIME*3/2-_CLK1uS	'skip to center of first data bit

:lup
                        waitcnt dtimbas,#_BITTIME				'wait for next bit

						test    ina,ddmxinmask  wz				'move data bit
						muxnz	dbyt,dbitmask					'. into byte
						shl		dbitmask,#1                     'adjust mask
			  	        djnz	dbitcnt,#:lup					'Decrement bit count - if non-zero, loop

                        waitcnt dtimbas,#_CLK1uS				'wait for next bit

						test    ina,ddmxinmask  wz				'stop bit?  Z if stop bit missing - possibly BREAK start
getbyt_ret				ret

dinitlock               long    InitLock

dmxinmask				long	1 << DmxInBit
ddmxbuffer				long	GblDMXBuffer
ddmxslots				long	GblDMXSlots

dinputbits				long	1 << DmxInBit
doutputbits				long	1 << DmxBreakBit 
ddmxbreakbit			long	1 << DmxBreakBit
ddmxinmask				long	1 << DmxInBit

dtimbas					long	0
ddmxslotadr				long	0
ddmxslotcnt				long	0
dbrktimer				long	0
dmabtimer				long	0
dslotadr				long	0
dslotcnt				long	0
dbitmask				long	0
dbitcnt					long	0
dbyt					long	0


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
'	Tree Controller Entry
'

TreeCtlEntry
                        or      outa,toutputson                 'setup i/o
                        andn    outa,toutputsoff                '
						or		dira,toutputbits				'
						andn	dira,tinputbits					'  ports
                        call    #reset

						mov		tslotadr,tdmxbuffer
						mov		tslotcnt,tdmxslots
:clear
						wrbyte	tzero,tslotadr
						add		tslotadr,#1
						djnz	tslotcnt,#:clear

						lockclr tinitlock						 'Init complete...
                        mov     tdebugtimer,cnt
                        add     tdebugtimer,#20

'	Get latest values from main RAM, then index the menory array based on value and index, and set the corresponding bits

'
'	sync to line
'
sync
						mov		tdebounce,tones					'init to ones

:notzeros
'						test	ina,tzerocross	wc				'debounce
'						rcl		tdebounce,#1	wz				'. zero-cross - all zeros?
'			  if_nz		jmp		#:notzeros						'No - continue

                        waitcnt tdebugtimer,tdebughalfcycle

						andn	outa,tphasebit

                        mov     ttimbas,cnt
                        add     ttimbas,tcomputeidle
                        waitcnt ttimbas,#0

compute
'
'	clear prior array values
'
						mov		tarrayaddr,tarray
						mov		tarraycnt,tarraysizelongs
:clear
						wrlong	tzero,tarrayaddr				'clear
						add		tarrayaddr,#4					'. the
						djnz	tarraycnt,#:clear				'.  array

'
'	compute new array
'
						mov		tslotadr,tdmxbuffer				'@'dmx data buffer
						mov		tslotcnt,tdmxslots				'312	
						mov		tarraybase,tarray1  			'GblTreeBuffer+1
						mov		tarraymask,tarraymaskinit

:lup
						rdbyte	tarrayaddr,tslotadr				'load value
                        max     tarrayaddr,#100
						add		tarrayaddr,tarraybase			'. + base
						
						rdbyte	twrk,tarrayaddr					'set
						or		twrk,tarraymask					'
						wrbyte	twrk,tarrayaddr					'. corresponding bit in byte

						sub		tarrayaddr,#1					'Bump @ to next sequence
						add		tslotadr,#1						'bump @

						rdbyte	twrk,tarrayaddr					'set
						or		twrk,tarraymask					'
						rol		tarraymask,#1 wc					'**Slide & save bit shifted into bit 0
						wrbyte	twrk,tarrayaddr					'. corresponding bit there too

			  if_c		add		tarraybase,#102					'If carry around, increment base
              
						djnz	tslotcnt,#:lup

'
'	sync to line
'
resync
						mov		tdebounce,#0					'set to all zeros

:lup
'						test	ina,tzerocross	wc				'debounce
'						rcl		tdebounce,#1					'. zero-cross
'						cmp		tdebounce,tones	wz				'all ones?
'			  if_nz		jmp		#:lup

                        waitcnt tdebugtimer,tdebughalfcycle

						mov		ttimbas,cnt						'get current time
                        
                        or		outa,tphasebit
						mov		tintervaladr,#tintervals		'get @'time intervals
						mov		tarrayaddr,tarray100			'init to GblTreeBuffer + 100
						xor		tevenoddflag,#1 wz
			  if_nz     add 	tarrayaddr,#1       			'. or GblTreeBuffer+101 (even/add cycles)
						mov		tblastcnt,#50					'50 steps per cycle

blast
						movs	outa,#0							'set chip select 0, A1.A0 = zeros (first port)
						mov		tchipcnt,#13					'set for all 13 chips
						mov		tworkadr,tarrayaddr				'set initial address
						jmp		#:skip

:lup
						add		outa,#2							'set to next chip select, A1.A0 to 0b00

:skip
						andn	outa,twr8255					'begin wr
						rdbyte	twrk,tworkadr					'load first byte
						movd	outa,twrk						'. & send to 8255s
						add		tworkadr,#102					'. & increment to next data row
						rdbyte	twrk,tworkadr					'load next byte
						or		outa,twr8255					'end wr

						nop
						add		outa,#1							'set A1.A0 to 0b01
						andn	outa,twr8255					'begin wr
						add		tworkadr,#102					'. & increment to next data row
						movd	outa,twrk						'. & send to 8255s
						rdbyte	twrk,tworkadr					'load next byte
						nop
                        nop
						or		outa,twr8255					'end wr

						nop
						add		outa,#1							'set A1.A0 to 0b10
						andn	outa,twr8255					'begin wr
						movd	outa,twrk						'. & send to 8255s
						add		tworkadr,#102					'. & increment to next data row
						nop
						nop
						nop
						nop
						or		outa,twr8255					'end wr

						djnz	tchipcnt,#:lup					'loop for all 13 chips
						
                        movs    outa,#$03c                      'deselect all chips
						sub		tarrayaddr,#2					'adjust array base for next blast

						movs	:indx,tintervaladr
						add		tintervaladr,#1
:indx                   add     ttimbas,$-$ wz
                        waitcnt ttimbas,#0          

                        cmp     tintervaladr,#tintervalsdone wz 'done?
			  if_nz     jmp     #blast

                        call    #reset
                        jmp     #sync                           'start new cycle

reset
						or		outa,tresetbit

						movd	outa,tcmd8255init				'cmd = 0x80 - all ports - simple output
						movs	outa,#3							'select first chip, a1&a0=control reg
						mov		tchipcnt,#13					'set loop for all 13 chips

:lup
						andn	outa,twr8255					'begin  write
						nop		'								'. wait a while (min wr is 300ns = 6 inst)
						nop		'
						nop		'
						nop		'
						nop		'
						nop		'
						or		outa,twr8255					'end write
						nop		'
						add		outa,#4							'next chip
						djnz	tchipcnt,#:lup					'. & init all of them

                        movs    outa,#$03c                      'deselect all chips
						andn	outa,tresetbit

reset_ret               ret

tdebug                  long    $80000
tdebugtimer             long 0
tdebughalfcycle         long    80000000/120

tzero					long	0
tones					long	-1

tinitlock               long    InitLock

toutputbits				long	(1 << TreeA0Bit)|(1 << TreeA1Bit)|(1 << TreeCS0Bit)|(1 << TreeCS1Bit)|(1 << TreeCS2Bit)|(1 << TreeCS3Bit)|(1 << TreeD0Bit)|(1 << TreeD1Bit)|(1 << TreeD2Bit)|(1 << TreeD3Bit)|(1 << TreeD4Bit)|(1 << TreeD5Bit)|(1 << TreeD6Bit)|(1 << TreeD7Bit)|(1 << TreeWr8255Bit)|(1 << TreePhaseBit)|(1 << TreeResetBit)
tinputbits				long	1 << TreeZeroCrossBit
toutputson              long    (1 << TreeCS0Bit)|(1 << TreeCS1Bit)|(1 << TreeCS2Bit)|(1 << TreeCS3Bit)|(1 << TreeWr8255Bit)   'disable CS, WR off
toutputsoff             long    (1 << TreePhaseBit)|(1 << TreeResetBit)
twr8255					long	1 << TreeWr8255Bit
tzerocross				long	1 << TreeZeroCrossBit
tphasebit				long	1 << TreePhaseBit
tresetbit				long	1 << TreeResetBit

tcmd8255init			long	$80								' all 3 ports - simple output
tcomputeidle            long    666000 - 80000                  ' half-cycle (80M/120) - time to build array)

tdmxbuffer				long	GblDMXBuffer
tdmxslots				long	GblDMXSlots
tarray					long	GblTreeBuffer
tarray1                 long    GblTreeBuffer+1
tarray100				long	GblTreeBuffer+100
tarraysizelongs			long	GblTreeBufferSize/4
tarraymaskinit			long	$01010101
tarraybase				long	0
tarraycnt				long	0	

tarraymask				long	0
tdebounce				long	0
tarrayaddr				long	0
tslotadr				long	0
tslotcnt				long	0
tchipcnt				long	0
tblastcnt				long	0
tevenoddflag			long	0

tworkadr				long	0
twrk					long	0

ttimbas					long	0
tintervaladr			long	0
tintervals				long    97951
                        long    26550
                        long    19106
                        long    15563
                        long    13432
                        long    11990
                        long    10942
                        long    10144
                        long    9518
                        long    9015
                        long    8601
                        long    8255
                        long    7970
                        long    7723
                        long    7520
                        long    7343
                        long    7192
                        long    7069
                        long    6960
                        long    6875
                        long    6805
                        long    6748
                        long    6709
                        long    6682
                        long    6669
                        long    6669
                        long    6682
                        long    6709
                        long    6749
                        long    6805
                        long    6874
                        long    6962
                        long    7067
                        long    7194
                        long    7342
                        long    7519
                        long    7724
                        long    7969
                        long    8258
                        long    8600
                        long    9015
                        long    9516
                        long    10146
                        long    10941
                        long    11988
                        long    13432
                        long    15564
                        long    19108
                        long    26550
                        long    26550
tintervalsdone          long    0

                        fit     496
                        