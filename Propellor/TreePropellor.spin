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
        

        USBRxF = 8                                      'Bit 8 must be an input - USB writer uses MOVS
        USBWr = 9
        USBRd = 10
        USBTxE = 11

        '	Address info uses MOVS instruction - must be outa bits 8..0
        TreeA0Bit = 0
        TreeA1Bit = 1
        TreeCS0Bit = 2
        TreeCS1Bit = 3
        TreeCS2Bit = 4
        TreeCS3Bit = 5

		'	Data info used MOVD instruction - ,must be bits 17-9
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

        DmxBitOn = 29
        DmxInBit = 28


        GblBase = $0
        GblDMXBuffer = GblBase + 0
		GblDMXStartingSlot = 1
		GblDMXSlotCnt = 312
		GblDMXBufferSize = 2 * GblDMXSlotCnt

        GblTreeBuffer = GblDMXBuffer + GblDMXBufferSize
		GblTreeBufferSize = 39 * 102

        DmxRcvrCog = 1                              'DMX Receiver cog
        TreeCtlCog = 0								'Tree Control Cog                                

        
Pub Start

    lockset(InitLock)

' Start DMX Rcvr cog
    coginit(DmxRcvrCog, @DmxRcvrEntry, 0)               'USB interface service

' Start tree controller cog
    coginit(TreeCtlCog, @TreeCtlEntry, 0)                 'Buffer manager service
                                                         
    
DAT

{{
          IIIII  N   N  IIIII  TTTTT      /  DDDD   EEEEE  BBBB   U   U   GGGG
            I    NN  N    I      T       /   D   D  E      B   B  U   U  G   
            I    N N N    I      T      /    D   D  EEE    BBBB   U   U  G GGG
            I    N  NN    I      T     /     D   D  E      B   B  U   U  G   G
          IIIII  N   N  IIIII    T    /      DDDD   EEEEE  BBBB    UUU    GGG

}}

                        org     0
InitEntry                                        

:zap
                        sub     iaddr,#4        wz      'Clear (Initial value = $8000)
                        wrlong  it2,iaddr               '. (Initial value = 0)
              if_nz     jmp     #:zap                   '. memory

'
'       Global
'
'                       mov     it1,#StateInit          '(Initial value = StateInit)
                        wrbyte  it1,#GblState

'
'       Cog
'
                        add     iaddr,#GblBase + GblSiz
                                                                        
                        mov     icogtab,iaddr
                        wrword  icogtab,#GblCogTbl
                        add     iaddr,it3               '(it3 Initial value = CogTblSiz * CogTblCnt)

                        

                        wrword  it1,#GblFreeBgn         'End of allocated space - begin of buffer pool

'                       
'       Structures complete - let the other cogs run
'
                        lockclr iinitlock               'Init complete...


'
'       Wait for all of the protocol cogs to be ready for synchronous start
'
                        mov     it1,#ProtoCogCnt
                        mov     it2,icogtab
                        add     it2,it5

:lup
                        rdbyte  it3,it2
                        cmp     it3,#StateSync  wz
              if_nz     jmp     #:lup

                        add     it2,#CogTblSiz
                        djnz    it1,#:lup
                        

'
'       Send running indication to serial port
'
                        mov     sbuff,#"H"
                        call    #sender
                        mov     sbuff,#"i"
                        call    #sender

                        
'
'       Debug code - Serial Transmitter
'

DbgEntry
                        or      outa,dtxmask           'idle = 1
                        or      dira,dtxmask           'Pin30 = output

                        rdword  dCogTab,#GblCogTbl

:cogTop
                        mov     dCogCnt,#8
                        mov     dCogCur,dCogTab
                        jmp     #$+2

:cogLup
                        add     dCogCur,#CogTblSiz
                        mov     dDbgTail,dCogCur
                        add     dDbgTail,#CogDbgTail
                        rdbyte  dBfrPtr,dDbgTail
                        mov     dDbgHead,dCogCur
                        add     dDbgHead,#CogDbgHead
                        rdbyte  dBfrEnd,dDbgHead

                        cmp     dBfrPtr,dBfrEnd wz
              if_z      jmp     #:cogNxt                        

                        mov     dtxbuff,#"8"            'Display
                        sub     dtxbuff,dCogCnt         '. cogid
                        call    #transmit               

                        mov     dtxbuff,#":"            ':'
                        call    #transmit

                        mov     dtxbuff,#" "            '<space>'
                        call    #transmit

:charNxt
                        mov     dtxbuff,dBfrPtr
                        add     dtxbuff,dCogCur
                        add     dtxbuff,#CogDbgBfr
                        rdbyte  dtxbuff,dtxbuff
                        call    #transmit

                        add     dBfrPtr,#1
                        cmpsub  dBfrPtr,#CogDbgBfrSiz
                        wrbyte  dBfrPtr,dDbgTail
                        
                        rdbyte  dBfrEnd,dDbgHead

                        cmp     dBfrPtr,dBfrEnd wz
              if_nz     jmp     #:charNxt                        


                        mov     dtxbuff,#$0d            '<cr>'
                        call    #transmit

:cogNxt
                        djnz    dCogCnt,#:cogLup
                        jmp     #:cogTop

                                                
transmit
                        mov     dtxcnt,#10
                        or      dtxbuff,#$100          'add stoppbit
                        shl     dtxbuff,#1             'add startbit
                        mov     dtime,cnt
                        add     dtime,dbittime

:sendbit
                        shr     dtxbuff,#1    wc       'test LSB
                        muxc    outa,dtxmask             'bit=0  or
                        waitcnt dtime,dbittime         'wait 1 bit
                        djnz    dtxcnt,#:sendbit        '10 times
               
                        waitcnt dtime,dbittime         '2 stopbits
                        
transmit_ret            ret


sender
                        cogid   s1
                        shl     s1,#CogTblShft
                        rdword  sCogTab,#GblCogTbl
                        add     sCogTab,s1
                        mov     sCogDbgHead,sCogTab
                        add     sCogDbgHead,#CogDbgHead
                        rdbyte  sBfrPtr,sCogDbgHead
                        mov     s1,sBfrPtr
                        add     s1,#CogDbgBfr
                        add     s1,sCogTab
                        wrbyte  sbuff,s1
                        add     sBfrPtr,#1
                        cmpsub  sBfrPtr,#CogDbgBfrSiz
                        wrbyte  sBfrPtr,sCogDbgHead
                                           
sender_ret              ret


sCogDbgHead             long    0
sCogTab                 long    0
s1                      long    0
sbuff                   long    0
sBfrPtr                 long    0




iinitlock               long    InitLock
it1                     long    StateInit
it2                     long    0
it3                     long    CogTblSiz * CogTblCnt
it4                     long    ProtoCog * CogTblSiz + CogChanTab
it5                     long    ProtoCog * CogTblSiz + CogState
it6                     long    ProtoCog * CogTblSiz + CogTimBas
it7                     long    DmxTimerCog * CogTblSiz + CogChanTab

iaddr                   long    $8000
icogtab                 long    0
ichntab                 long    0
                        



dtime                   long    0
dbittime                long    _CLKFREQ / BAUDRATE
dtxmask                 long    1 << TXPIN
dtxcnt                  long    0
dtxbuff                 long    0

dCogTab                 long    0
dCogCnt                 long    0
dCogCur                 long    0
dDbgHead                long    0
dDbgTail                long    0
dBfrPtr                 long    0
dBfrEnd                 long    0

                        fit     496

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
						mov		bufcnt,gbldmxslotcnt

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
slotcnt					long	GblDMXBufferSize / 2

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

'	Get latest values from main RAM, then index the menory array based on value and index, and set the corresponding bits

'
'	sync to line
'
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
						mov		bufcnt,buffersizewords			'312	
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

		  if_c			add		arraybase,#102					'If carry around, increment base
              
						djnz	bufcnt,#:lup

'
'	sync to line
'
						neg		debounce,#1						'setup
:notones
						test	ina,#zerocross	wc				'debounce
						rcl		debounce,#1						'. zero-cross
						add		debounce,#1 nr wz				'all ones?
			  if_nz		jmp		#:notones


						xor		evenoddflag,#1	wc
						mov		arrayaddr,#array+100			'or array+101 (even/add cycles)
						addx	arrayadr,#0


blast
						mov		arraycnt,#39
						movs	outa,#0
						mov		loopcnt,#13
						mov		workadr,arrayadr

						ldbyte	byt,workadr

:lup
						andn	outa,wr8255
						movd	outa,byt
						add		workadr,@102
						ldbyte	byt,workadr
						or		outa,wr8255
						add		outa,#1

						andn	outa,wr8255
						movd	outa,byt
						add		workadr,@102
						nop		'							-- no cost --
						ldbyte	byt,workadr
						or		outa,wr8255
						add		outa,#1

						andn	outa,wr8255
						movd	outa,byt
						add		workadr,@102
						nop		'							-- no cost --
						ldbyte	byt,workadr
						or		outa,wr8255
						add		outa,#2

						djnz	loopcnt,#:lup

						
						sub		arrayadr,#2

						wait a while

						djnz	???,#:lup


reset
						movs	outa,#3
						movd	outa,#$80
						mov		loopcnt,#13

:lup
						andn	outa,wr8255
						nop		'
						nop		'
						nop		'
						nop		'
						nop		'
						nop		'
						or		outa,wr8255
						nop		'
						add		outa,#4
						djnz	loopcnt,#:lup












zero					long	0
ones					long	-1

zerocross				long	1 << TreeZeroCrossBit
array					long	GblTreeBuffer
arraysizewords			long	GblTreeBufferSize/2
buffer					long	GblDMXBuffer
buffersizewords			long	GblDMXBufferSize/2


debounce				long	0
arrayaddr				long	0
arraycnt				long	0
bufaddr					long	GblDMXBuffer
bufcnt					long	GblDMXBufferSize/2

work					long	0


                        fit     496
                        