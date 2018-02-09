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

        
        Chan01Bit = 12
        Chan02Bit = 13
        Chan03Bit = 14
        Chan04Bit = 15
        Chan05Bit = 16
        Chan06Bit = 17
        Chan07Bit = 18
        Chan08Bit = 19
        Chan09Bit = 20
        Chan10Bit = 21
        Chan11Bit = 22
        Chan12Bit = 23
        Chan13Bit = 24
        Chan14Bit = 25
        Chan15Bit = 26
        Chan16Bit = 27

        DmxBitOn = 29
        DmxBit = 28


        GblBase = $0
        GblTimer = GblBase + 0
        GblState = GblBase + 4
        GblFreeCnt = GblBase + 5
        GblFreeChain = GblBase + 6
        GblCogTbl = GblBase + 8
        GblChanTbl = GblBase + 10
        GblFreeBgn = GblBase + 12
        GblLng = 4
        GblSiz = GblLng * 4


        
        CogTblCnt = 8
        CogTblLng = 32
        CogTblSiz = CogTblLng * 4
        CogTblShft = 7
        
        CogState = 0
        CogDbgHead = 2
        CogDbgTail = 3
        CogTimBas = 4
        CogChanTab = 8
        CogDbgBfr = 16
        CogDbgBfrSiz = CogTblSiz - CogDbgBfr

        
        ProtoCog = 4                                   'Protocol cogs should be upper range - closer to p16-p28
        ProtoCogCnt = 4                                
        USBCog = 1                                     'USB cog should be low - closer to P0-p7 for output
        BufferCog = 2
        DmxTimerCog = 3

        
        ChanTblCnt = 17
        ChanTblLng = 4
        ChanTblSiz = ChanTblLng * 4
        ChanTblShft = 4
        ChanTblMask = 0
        ChanTblFree = 4                                 'Free pointer must immediately preceed Busy
        ChanTblBusy = 6                                 'Protocol code used wrlong to set both
        ChanTblHead = 8
        ChanTblTail = 10

        ChanTimeSlice = 100                              'Ticks per Time Slice per channel
        DmxTimerTimeSlice = _CLKFREQ / 250000            'Ticks per Dmx/Timer time slice (4us)
        TimerTicksPerMs = 1000 / 4
        ChanQuietBits = 19                               'Quiet bit times between words
        
        BufferLng = 68                                  'Longs
        BufferSiz = BufferLng * 4                       'Bytes

        StateInit = 1
        StateSync = 2
        StateReady = 3
        StateStop = 4
        StateRun = 5
        StatePause = 6


        QueueLock = 0                                   '0 through 4
        QueueLockCnt = 5                                '1 for each protocol cog
        FreeLock = 6
        InitLock = 7


        BfrLink = 0
        BfrLeng = 2
        BfrTimer = 4
        BfrData = 8

        TXPIN      = 30           'customize if necessary
        BAUDRATE   = 115200
        
Pub Start

    lockset(InitLock)

' Start DMX Rcvr cog
    coginit(DmxRcvrCog, @DmxRcvrEntry, 0)               'USB interface service

' Start tree controller cog
    coginit(TreeCtlCog, @TreeCtlEntry, 0)                 'Buffer manager service

' Start init & debug code                               'Start init last so it can clear memory
    coginit(0, @InitEntry, 0)                           'Init/Serial port service
                                                         
    
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
:init                   lockset tinitlock           wc
              if_c      jmp     #:init
                        lockclr tinitlock
                        
                        rdword  tcogtab,#GblCogTbl      'Get @'Cog table



:spin
                        rdbyte  tstate,#GblState
                        cmp     tstate,#StateReady wz,wc
              if_b      jmp     #:spin 
                          

                        mov      ttimbas,cnt                     'Set
                        add      ttimbas,#DmxTimerTimeSlice      '. timer

      
                        
'
'       Everyone should be running
'

'
'       DMX receiver
'

break
                        waitcnt ttimbas,#_CLKuS					'wait 1uS

						test    outa,dmxinmask  wz				'Break?
              if_nz     jmp     #break							'. Not zero, so not break - wait for break to start
			  						
						mov		brktimer,#0						'clear BREAK timer

:lup	
                        waitcnt ttimbas,#_CLKuS					'wait 1uS

						test    outa,dmxinmask  wz				'Break?
              if_nz     jmp     #:end							'. No, break ended

						add		brktimer,#1						'increment break time
						jmp		#:lup							'. & wait for end

:end
						cmp		brktimer,#88	wc				'at least 88uS?
			  if_c		jmp		#break							'. No, try again

mab				
						mov		mabtimer,#0						.clear MAB timer
			  						
:lup
                        waitcnt ttimbas,#_CLKuS					'wait 1uS

						test    outa,dmxinmask  wz				'MAB?
              if_z      jmp     #:end							'. No, MAB ended

						add		mabtimer,#1						'increment MAB time
						jmp		#:lup							'. & wait for end

:end
						cmp		mabtimer,#8		wc				'at least 8uS?
			  if_c		jmp		#break							'. No, try again
						
						call	getbyt							'get slot 0 - 'start code'
			  if_z		jmp		#break							'Framing error - possilby BREAK

						mov		bufaddr,#buffer
						mov		bufcnt,#l'buffer

:slotlup
						waitcnt	ttimbas,_CLK1uS					'wait a bit
						test    outa,dmxinmask  wz				'start bit?  Z if start is present
			  if_nz		jmp		#slots							'not yet - wait for it

						call	getbyt
			  if_z		jmp		#break							'Framing error - may be break

			  			cmp		byt,#100	wc					'normalize
			  if_nc		mov		byt,#100						'. value to <= 100

						wrbyte	byt,bufaddr						'write word
						add		bufaddr,#1						'. & bump @
						djnz	bufcnt,#:slotlup				'no - loop
						jmp		#break							'yes - should be done

getbyt
						mov		bitptr,#1						'init bit mask
						mov		bitcnt,#8						'. & #'bits
                        waitcnt ttimbas,#_BITTIME/2				'skip to center of start bit

:lup
                        waitcnt ttimbas,#_BITTIME				'wait for next bit

						test    outa,dmxinmask  wz				'move data bit
						muxz	byt,bitctr						'. into byte
						shl		bitptr							'adjust mask
			  if_nz		djnz	bitcnt,#:lup					'. No, loop

                        waitcnt ttimbas,#_BITTIME				'wait for next bit

						test    outa,dmxinmask  wz				'stop bit?  Z if stop bit missing - possibly BREAK start
getbyt_ret				ret


dmxsrtn                 long    0
dmxrtn                  long    dmxtop
dmxbfr                  long    0
dmxptr                  long    0
dmxlng                  long    0
dmxchan                 long    0
dmxmask                 long    ChanTblMask
dmxbusy                 long    ChanTblBusy
dmxfree                 long    ChanTblFree
dmxdata                 long    0
dmxmskON                long    1 << DmxBitOn
dmxbits                 long    0

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
USBEntry
:init                   lockset uinitlock           wc
              if_c      jmp     #:init
                        lockclr uinitlock
                        
'
'       Build the free buffer chain
'
                        call    #clearbufr

                        rdword  ucogtab,#GblCogTbl				'Get @'Cog Tbl
                        add     ucogstate,ucogtab				'. & @'our cog state

                        mov     dira,#0
                        or      outa,usbrdmask					'Deassert
                        or      dira,usbrdmask					'. & set as output
                        
                        andn    outa,usbwrmask					'Deassert
                        or      dira,usbwrmask					'. & set as output

                        mov     ut0,#StateReady
                        wrbyte  ut0,#GblState
                        wrbyte  ut0,ucogstate

                        rdword  uchntab,#GblChanTbl
                                                
'
'       Wait for all cogs to signal 'Ready'
'
                        mov     utxmrtn,#0
                        call    #chkstate
                        

'
'       Everyone is ready - Let's go!!!
'

ReadyToGo
                        mov     ut1,#StateStop
                        wrbyte  ut1,#GblState
                        wrbyte  ut1,ucogstate

'	Get latest values from main RAM, then index the menory array based on value and index, and set the corresponding bit

'
'	sync to line
'
:notzeros
						test	outa,#zerocross	wc				'debounce
						rcl		debounce,#1	wz					'. zero-cross - all zeros?
			  if_nz		jmp		#:notzeros						'No - continue

compute
'
'	clear prior array values
'
						mov		arrayaddr,#array
						mov		arraycnt,#l'array_in_longs
:clear
						wrlong	zero,arrayaddr					'clear
						add		arrayaddr,#4					'. the
						djnz	arraycnt,#:clear				'.  array

'
'	compute new array
'
						mov		bufaddr,#buffer					'dsword 312
						mov		bufcnt,#l'buffer_in_words		'312	
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
						test	outa,#zerocross	wc				'debounce
						rcl		debounce,#1						'. zero-cross
						add		debounce,#1 nr wz				'all ones?
			  if_nz		jmp		#:notones

						mov		arrayaddr,#array+101





blast
						mov		arraycnt,#312/8
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
						ldbyte	byt,workadr
						or		outa,wr8255
						add		outa,#1

						andn	outa,wr8255
						movd	outa,byt
						add		workadr,@102
						ldbyte	byt,workadr
						or		outa,wr8255
						add		outa,#2

						djnz	loopcnt,#:lup

						


reset
						movs	outa,#3
						movd	outa,#$80
						mov		loopcnt,#13

:lup
						andn	outa,wr8255
						jmp		$+1
						jmp		$+1
						jmp		$+1
						jmp		$+1
						jmp		$+1
						jmp		$+1
						or		outa,wr8255























:newcmd
                        mov     urcvfull,urcvdata     
                        mov     urcvwrk,urcvdata        'Isolate
                        mov     urcvcmd,#$ff             '. the parts
                        and     urcvcmd,urcvwrk         '
                        mov     urcvseq,#$ff             '
                        ror     urcvwrk,#8              '
                        and     urcvseq,urcvwrk         's
                        mov     urcvlng,#$ff             '
                        ror     urcvwrk,#8              '
                        and     urcvlng,urcvwrk          '

                        ror     urcvwrk,#8              'Isolate
                        and     urcvwrk,#$ff            '. chksum
                        cmp     urcvwrk,#$ff   wz
              if_z      jmp     #:bypchk
                        xor     urcvwrk,urcvcmd
                        xor     urcvwrk,urcvlng
                        xor     urcvwrk,urcvseq  wz    'Checksum valid?
'              if_nz     jmp     #sndnack_chk            '. No - back checksum

:bypchk                                                        
                        mov     urcvwrk,urcvcmd         'Check the
                        sub     urcvwrk,#$7f            '. command
cmd80                   djnz    urcvwrk,#cmd81          'Not 80 (echo) - maybe 81?

                        mov     urcvecho,#1
                        mov     urcvrtn,#:snd
                        jmp     urcvrtn
                        
:lup
                        call    #urcvlong               'Get next word

:snd
                        tjnz    utxmxack,utxmrtn
                        mov     utxmxack,urcvdata

                        djnz    urcvlng,#:lup           'Loop

                        mov     urcvrtn,#$+1
                        tjnz    utxmxack,utxmrtn
                        
                        mov     urcvecho,#0
                        jmp     #urcvtop

cmd81                   djnz    urcvwrk,#cmd82          'Not 81 (Set time) - maybe 82?

                        cmp     urcvlng,#2       wz     'L' valid?
              if_ne     jmp     #sndnack_lng            '. No - bad L'

                        call    #urcvlong               'Get time
                        wrlong  urcvdata,#GblTimer      '. & stash it

                        jmp     #sndack                 'Send ACK & loop

cmd82                   djnz    urcvwrk,#cmd83          'Not 82 (run) - maybe 83?
                        mov     ut0,#StateRun           'Get new state
                        jmp     #newstate

cmd83                   djnz    urcvwrk,#cmd84          'Not 83 (pause) - maybe 84?
                        mov     ut0,#StatePause         'Get new state
                        jmp     #newstate

cmd84                   djnz    urcvwrk,#cmd85          'Not 84 (stop) - maybe 85?
                        mov     ut0,#StateStop          'Get new state

newstate
                        cmp     urcvlng,#1       wz     'L' valid?
              if_ne     jmp     #sndnack_lng            '. No - bad L'

                        wrbyte  ut0,#GblState           'Set new global state
                        wrbyte  ut0,ucogstate           '. & our cog state

                        call    #chkstate               'Wait for all cogs
                        jmp     #sndack                 'Send ACK & loop

cmd85                   djnz    urcvwrk,#cmd90          'Not 85 (Reseq) - maybe 90?

                        mov     urcvlastseq,urcvseq     'Set new 'Last Seq'
                        jmp     #sndack                 '. & send ack
                        
cmd90
                        sub     urcvwrk,#$90-$85 wc,wz  '$90 - $a0
              if_b      jmp     #sndnack_cmd1
                        cmp     urcvwrk,#$a0-$90 wc,wz
              if_a      jmp     #sndnack_cmd2

                        mov     ut1,urcvlastseq
                        add     ut1,#1
                        and     ut1,#$ff
                        cmp     ut1,urcvseq      wz
              if_ne     jmp     #sndnack_seq
                            
                        sub     urcvlng,#2       wc,wz  'L' valid?
              if_be     jmp     #sndnack_lng            '. No -
                        cmp     urcvlng,#64      wz,wc  'L' valid?
              if_a      jmp     #sndnack_lng            '. No -
                               
'
'       allocate a buffer
'
                        mov     urcvrtn,#:spin
                        
:spin                   lockset ufreelock        wc
              if_c      jmp     utxmrtn

                        rdword  urcvbufr,#GblFreeChain wz   'load @'next buffer
              if_z      jmp     #:unlk

                        rdword  ut1,urcvbufr                            'unchain
                        
                        wrword  ut1,#GblFreeChain                       '. the buffer

                        rdbyte  urcvfreecnt,#GblFreeCnt         'Decrement
                        sub     urcvfreecnt,#1                          '.
                        wrbyte  urcvfreecnt,#GblFreeCnt         '. buffer count

:unlk
                        lockclr ufreelock                                       '. & clear the lock
                        
              if_z      jmp     #sndnack_bfr                            'no buffer - try again
              
'
'       receive msg
'
                        mov     urcvptr,urcvbufr                        'Copy @'              
                        wrlong  uzero,urcvptr
                        add     urcvptr,#BfrLeng                        'Bump @'data area
                        wrword  urcvlng,urcvptr
                        add     urcvptr,#BfrTimer-BfrLeng       'Bump @'data area
                        add     urcvlng,#1                      'increment l'
                        
:lup
                        call    #urcvlong                       'Get another long
'76                        
                        wrlong  urcvdata,urcvptr                '. & write command there
                        add     urcvptr,#4                      'Bump @'data area
                        djnz    urcvlng,#:lup                   'Loop thru data

'
'       queue buffer on channel
'
                        mov     urcvchan,urcvwrk
                        shl     urcvchan,#ChanTblShft   '16 bytes each
                        add     urcvchan,uchntab

                        mov     urcvlock,urcvwrk
                        shr     urcvlock,#2

'7e                        
:spin2                  lockset urcvlock            wc  'Lock   
              if_c      jmp     #:spin2                 '. the cog (channel head/tail)

                        mov     urcvptr,#ChanTblTail
                        add     urcvptr,urcvchan
                        rdword  ut1,urcvptr      wz

                        wrword  urcvbufr,urcvptr

                        sub     urcvptr,#ChanTblTail-ChanTblHead
                        
              if_nz     wrword  urcvbufr,ut1
              if_z      wrword  urcvbufr,urcvptr                                                
                        
                        lockclr urcvlock                'Clear the lock   

                        mov     urcvlastseq,urcvseq

sndack

                        mov     usndcmd,#$80
                        jmp     #sndxack
                        
sndnack_chk
                        mov     usndcmd,#$81
                        jmp     #sndxack

sndnack_seq
                        mov     usndcmd,#$82
                        jmp     #sndxack

sndnack_cmd
                        mov     usndcmd,#$83
                        jmp     #sndxack

sndnack_lng
                        mov     usndcmd,#$84
                        jmp     #sndxack

sndnack_cmd1
                        mov     usndcmd,#$85
                        jmp     #sndxack

sndnack_cmd2
                        mov     usndcmd,#$86
                        jmp     #sndxack

sndnack_bfr
                        mov     usndcmd,#$8f

sndxack
                        mov     urcvrtn,#$+1
                        tjnz    utxmxack,utxmrtn

                        mov     utxmxack,usndcmd
                        ror     utxmxack,#8

                        or      utxmxack,urcvlastseq
                        ror     utxmxack,#8


                        rdbyte  urcvfreecnt,#GblFreeCnt
                        or      utxmxack,urcvfreecnt
                        ror     utxmxack,#16

snd
                        jmp     #urcvtop



'
'       Transmitter
'
utxmtop
                        mov     utxmrtn,#:tst

:tst                        
                        tjnz    utxmxack,#:sndxack 
                        tjnz    urcvecho,urcvrtn

                        rdbyte  ut1,#GblFreeCnt    
                        cmp     ut1,urcvfreecnt       wz,wc
              if_be     jmp     urcvrtn

                        mov     urcvfreecnt,ut1
                        
                        mov     utxmxack,#$90
                        ror     utxmxack,#8

                        or      utxmxack,urcvlastseq
                        ror     utxmxack,#8

                        or      utxmxack,urcvfreecnt
                        ror     utxmxack,#16

:sndxack
                        mov     utxmdata,utxmxack
                        mov     utxmxack,#0

                        mov     utxmrtn,#:byt1
                        
:byt1                   test    USBTxEmask,ina          wz
              if_nz     jmp     urcvrtn

                        or      dira,#$ff                                       
                        movs    outa,utxmdata                                   'bit 8 is/must be an input
                        nop
                        or      outa,usbwrmask
                        nop
                        ror     utxmdata,#8
                        nop
                        andn    outa,usbwrmask
                        nop
                        andn    dira,#$ff

                        mov     utxmrtn,#:byt2
                        
:byt2                   test    USBTxEmask,ina          wz
              if_nz     jmp     urcvrtn

                        or      dira,#$ff
                        movs    outa,utxmdata                                   'bit 8 is/must be an input
                        nop
                        or      outa,usbwrmask
                        nop
                        ror     utxmdata,#8
                        nop
                        andn    outa,usbwrmask
                        nop
                        andn    dira,#$ff

                        mov     utxmrtn,#:byt3
                        
:byt3                   test    USBTxEmask,ina          wz
              if_nz     jmp     urcvrtn

                        or      dira,#$ff
                        movs    outa,utxmdata                                   'bit 8 is/must be an input                        
                        nop
                        or      outa,usbwrmask
                        nop
                        ror     utxmdata,#8
                        nop
                        andn    outa,usbwrmask
                        nop
                        andn    dira,#$ff

                        mov     utxmrtn,#:byt4
                        
:byt4                   test    USBTxEmask,ina          wz
              if_nz     jmp     urcvrtn

                        or      dira,#$ff
                        movs    outa,utxmdata                                   'bit 8 is/must be an input
                        nop
                        or      outa,usbwrmask
                        nop
                        nop
                        nop
                        andn    outa,usbwrmask
                        nop
                        andn    dira,#$ff

                        jmp     #utxmtop                        


                        

urcvlong
'
'                       Load Test Data
'
                        mov     urcvrtn,#$+1
                        tjz     xrcvdtacnt,#urcvtmo

                        movs    :lod,xrcvdtaadr
                        add     xrcvdtaadr,#1
                        sub     xrcvdtacnt,#1
:lod                    mov     urcvdata,$-$
                        jmp     #urcvlong_ret
'
'                       End Test Data Load
'


                        mov     urcvtrys,#0              
                        mov     urcvrtn,#:byt1

:byt1                   test    USBRxFmask,ina          wz                           
              if_nz     jmp     #urcvtmo

                        andn    outa,usbrdmask
                        nop
                        nop
                        movi    urcvdata,ina
                        or      outa,usbrdmask
                        
                        mov     urcvrtn,#:byt2

:byt2                   test    USBRxFmask,ina          wz
              if_nz     jmp     #urcvtmo

                        andn    outa,usbrdmask
                        shr     urcvdata,#8
                        nop
                        movi    urcvdata,ina
                        or      outa,usbrdmask

                        mov     urcvrtn,#:byt3

:byt3                   test    USBRxFmask,ina          wz
              if_nz     jmp     #urcvtmo

                        andn    outa,usbrdmask
                        shr     urcvdata,#8
                        nop
                        movi    urcvdata,ina
                        or      outa,usbrdmask

                        mov     urcvrtn,#:byt4

:byt4                   test    USBRxFmask,ina          wz
              if_nz     jmp     #urcvtmo

                        andn    outa,usbrdmask
                        shr     urcvdata,#7
                        shr     urcvdata,#1             wc
                        movi    urcvdata,ina             
                        or      outa,usbrdmask
                        rcl     urcvdata,#1

                        test    $,#0                    wc                      'Clear carry                        
                                                                                
urcvlong_ret            ret

urcvtmo
                        djnz    urcvtrys,utxmrtn
                        test    $,#1                    wc                      'Set Carry
                        jmp     #urcvlong_ret                                     '. & return
                        

'
'       Build the free buffer chain
'
clearbufr
                        rdword  ut3,#GblFreeBgn                 'Get @'available area
                        mov     ut4,#0                          '. & count
                        
                        mov     ut1,#GblFreeChain
                        jmp     #:bgn
:lup
                        wrword  ut2,ut1
                        mov     ut1,ut2
                        add     ut4,#1

:bgn
                        mov     ut2,ut3
                        add     ut3,#BufferSiz

                        test    ut3,uwordmask     wz
              if_z      jmp     #:lup

:dun
                        wrlong  uzero,ut1

                        wrbyte  ut4,#GblFreeCnt

clearbufr_ret           ret


'
'       Check for all cogs in state
'
chkstate
                        mov     usc0,ut0
                        mov     usc2,ucogtab             'Get                        
                        add     usc2,#CogTblSiz+CogState '. @'first running cog entry
                        mov     usc3,#CogTblCnt-1           '. & #'running cogs

                        mov     urcvrtn,#$+1

:lup                      
                        rdbyte  usc1,usc2                 'Get cog status
                        cmp     usc1,usc0         wz      'Match?
              if_nz     tjnz    utxmrtn,utxmrtn           '. No - wait a little longer
              if_nz     jmp     #:lup

                        add     usc2,#CogTblSiz          'Bump to next cog
                        djnz    usc3,#:lup               '. & continue 'til all ready

chkstate_ret            ret



uinitlock               long    InitLock
ufreelock               long    FreeLock

               
utxmrtn                 long    0
urcvrtn                 long    0
                        
urcvlock                long    0
urcvfull                long    0
urcvdata                long    0
urcvcmd                 long    0
urcvlng                 long    0
urcvseq                 long    0
urcvwrk                 long    0
urcvptr                 long    0
urcvbufr                long    0
urcvtrys                long    0
urcvchan                long    0
urcvfreecnt             long    0
urcvecho                long    0
urcvlastseq             long    0

utxmxack                long    0

usndcmd                 long    0

utxmdata                long    0

ut0                     long    0
ut1                     long    0
ut2                     long    0
ut3                     long    0
ut4                     long    0

usc0                    long    0
usc1                    long    0
usc2                    long    0
usc3                    long    0

uchntab                 long    0
ucogtab                 long    0
ucogstate               long    USBCog*CogTblSiz+CogState
uzero                   long    0
uone                    long    1
                                                                                 
uwordmask               long    $8000


USBRxFMask              long    1 << USBRxF
USBTxEMask              long    1 << USBTxE
USBRdMask               long    1 << USBRd
USBWrMask               long    1 << USBWr



xcvh
                        mov     xs4,#8
:lup
                        rol     xs3,#4
                        mov     xsbuff,xs3
                        and     xsbuff,#$f
                        add     xsbuff,#"0"
                        cmp     xsbuff,#"9"     wz,wc
              if_a      add     xsbuff,#"a"-"9"-1
                        call    #xsender
                        djnz    xs4,#:lup
                        
xcvh_ret                ret


xsender
                        cogid   xs1
                        shl     xs1,#CogTblShft
                        rdword  xsCogTab,#GblCogTbl
                        add     xsCogTab,xs1
                        
                        mov     xsCogDbgHead,xsCogTab
                        add     xsCogDbgHead,#CogDbgHead
                        rdbyte  xsBfrPtr,xsCogDbgHead 
                        mov     xs1,xsBfrPtr
                        add     xs1,#CogDbgBfr
                        add     xs1,xsCogTab
                        wrbyte  xsbuff,xs1
                        add     xsBfrPtr,#1
                        cmpsub  xsBfrPtr,#CogDbgBfrSiz
                        wrbyte  xsBfrPtr,xsCogDbgHead
                                           
xsender_ret             ret

xs1                     long    0
xs2                     long    0
xs3                     long    0
xs4                     long    0
xsCogTab                long    0
xsBfrPtr                long    0
xsCogDbgHead            long    0

xsbuff                  long    0



xrcvdtaadr    long      xrcvdtabgn
xrcvdtacnt    long      xrcvdtaend-xrcvdtabgn

xrcvdtabgn
              long              $ff0301a0               ' DMX, Seq 1, 3 words, No chksum
              long              $0                      ' Time 0
              long              $12340678               ' Sends hi-order byte first

              long              $ff0302a0               ' DMX, Seq 1, 3 words, No chksum
              long              $1                      ' Time 0
              long              $0f0f0f0f               ' Sends hi-order byte first

              long              $ff010082               ' Runs

              
'             long              $ff030190
'             long              $0
'             long              $12340678
'             long              $ff030291
'             long              $0
'             long              $12340678
'             long              $ff030392
'             long              $0
'             long              $12340678
'             long              $ff030493
'             long              $0
'             long              $12340678
'             long              $ff030594
'             long              $0
'             long              $12340678
'             long              $ff030698
'             long              $0
'             long              $12340678
'             long              $ff03079c
'             long              $0
'             long              $12340678
'             long              $ff03089f
'             long              $0
'             long              $12340678
                        
'             long              $ff030990
'             long              $1
'             long              $00340678
'             long              $ff030a91
'             long              $1
'             long              $00340678
'             long              $ff030b92
'             long              $1
'             long              $00340678
'             long              $ff030c93
'             long              $1
'             long              $00340678
'             long              $ff030d94
'             long              $2
'             long              $00340678
'             long              $ff030e98
'             long              $2
'             long              $00340678
'             long              $ff030f9c
'             long              $2
'             long              $00340678
'             long              $ff03109f
'             long              $2
'             long              $00340678
                        
'             long              $ff010082
'             long              $ff030990
'             long              $0
'             long              $12340678
xrcvdtaend



                        fit     496

DAT
   
{{
              BBBB   U   U  FFFFF  FFFFF  EEEEE  RRRR       M   M   AAA   N   N   AAA    GGGG  EEEEE  RRRR
              B   B  U   U  F      F      E      R   R      MM MM  A   A  NN  N  A   A  G      E      R   R
              BBBB   U   U  FFF    FFF    EEE    RRR        M M M  AAAAA  N N N  AAAAA  G GGG  EEE    RRR
              B   B  U   U  F      F      E      R  R       M   M  A   A  N  NN  A   A  G   G  E      R  R
              BBBB    UUU   F      F      EEEEE  R   R      M   M  A   A  N   N  A   A   GGG   EEEEE  R   R

}}

                        org     0
'
'
' Entry
'
BfrMgrEntry

:init                   lockset minitlock       wc
              if_c      jmp     #:init
                        lockclr minitlock
              
                        
                        rdword  mt1,#GblCogTbl          'Get 
                        add     mcogstate,mt1           '. @'our cog state

                        mov     mt1,#StateReady

                        rdword  mchantab,#GblChanTbl    'Get @'Chan table

stopped
                        cmp     mt1,#StatePause wz
              if_nz     cmp     mt1,#StateStop  wz
              if_nz     mov     mt1,#StateReady
              
                        wrbyte  mt1,mcogstate
                        

BufrTop
                        rdbyte  mt1,#GblState
                        cmp     mt1,#StateRun   wz
              if_nz     jmp     #stopped
              
                        wrbyte  mt1,mcogstate

                        mov     mchancnt,#ChanTblCnt    'Get #'channels
                        mov     mchanptr,mchantab       ' . & @'first entry

:lup
                        mov     mptr,mchanptr
                        add     mptr,#ChanTblFree       'Get @'free pointers
                        rdword  mbufr,mptr      wz      ' . & load - 0?
              if_z      jmp     #:chkbusy               '.    . Yes - skip it

                        wrword  mzero,mptr              'Clear the pointer


'
'       Return buffer to freechain
'
:spin1                  lockset mfreelock       wc      'Grab the lock - busy
              if_c      jmp     #:spin1                 '. Yes - spin

                        rdword  mt1,#GblFreeChain       'Load @'next buffer

                        wrword  mt1,mbufr               'Chain
                        
                        wrword  mbufr,#GblFreeChain     '. the buffer

                        rdbyte  mt1,#GblFreeCnt         'Increment
                        add     mt1,#1                  '. the
                        wrbyte  mt1,#GblFreeCnt         '. . free buffer count
                        
                        lockclr mfreelock               'Clear the lock

:chkbusy
                        add     mptr,#ChanTblBusy-ChanTblFree 'Get @'busy buffer
                        rdword  mt1,mptr        wz      '. & load it - 0?
              if_nz     jmp     #:nxtchan               '.   . No - still busy - leave as is

                        add     mptr,#ChanTblHead-ChanTblBusy 'Get @'queue
                        rdword  mbufr,mptr      wz      '. & load it - 0?
              if_z      jmp     #:nxtchan               '.   . Yes - empty - skip it


                        add     mbufr,#BfrTimer         'Load
                        rdlong  mt1,mbufr       wz      '. time from buffer - zero?
                        sub     mbufr,#BfrTimer         ' (restore @'buffer)
              if_z      jmp     #:gotone                '. Yes - process the data

                        rdlong  mt2,#GblTimer           'Load current time
                        cmp     mt1,mt2         wz,wc   'Is it time for this data?
              if_a      jmp     #:nxtchan               '. No, don't busy it yet

:gotone
'
'       Remove buffer from channel queue
'
                        mov     mt2,#ChanTblCnt
                        sub     mt2,mchancnt
                        shr     mt2,#2
                        
:spin2                  lockset mt2             wc      'Lock   
              if_c      jmp     #:spin2                 '. the cog (channel head/tail)

                        rdword  mt1,mbufr       wz      'Load link from buffer - zero?

                        wrword  mt1,mptr                'Update channel head

                        add     mptr,#ChanTblTail-ChanTblHead 'Compute @'tail
                                                                     
              if_z      wrword  mt1,mptr                'Update tail (if necessary)

                        lockclr mt2                     'Release

                        sub     mptr,#ChanTblTail-ChanTblBusy 'point back to Busy buffer
                        wrword  mbufr,mptr              'Busy the buffer

:nxtchan
                        add     mchanptr,#ChanTblSiz
                        djnz    mchancnt,#:lup
                        jmp     #BufrTop


mzero                   long    0
mfreelock               long    FreeLock
minitlock               long    InitLock
mchantab                long    GblChanTbl
mcogstate               long    BufferCog*CogTblSiz+CogState

mchanptr                long    0
mchancnt                long    0

mt1                     long    0
mt2                     long    0

mptr                    long    0
mbufr                   long    0




                        fit     496


DAT

{{
               GGG   EEEEE   CCC   EEEEE         PPPP   RRRR    OOO  TTTTT  OOO    CCC    OOO   L
              G   G  E      C   C  E             P   P  R   R  O   O   T   O   O  C   C  O   O  L
              G      EEEE   C      EEEE          PPPP   RRR    O   O   T   O   O  C      O   O  L
              G  GG  E      C   C  E             P      R  R   O   O   T   O   O  C   C  O   O  L
               GGGG  EEEEE   CCC   EEEEE         P      R   R   OOO    T    OOO    CCC    OOO   LLLLL

}}                      
                        org     0
'
'
' Entry
'
ProtoEntry
:init                   lockset binitlock       wc
              if_c      jmp     #:init
                        lockclr binitlock


                        rdword  bcogtab,#GblCogTbl      'Get @'Cog table

                        cogid   bt1                     'Cogid
                        shl     bt1,#CogTblShft            '. * 8
                        add     bcogtab,bt1             '. is index to cogtab

                        add     bcogstate,bcogtab       'Compute @'cog state
                        mov     bt1,#StateInit          'Set
                        wrbyte  bt1,bcogstate           '. cog state
                        
                        mov     bt2,bcogtab             'Copy @'cog tbl
                        add     bt2,#CogChanTab
                        
                        rdword  chan1chan,bt2           'Load Cog tbl entry
                        add     bt2,#2

                        add     chan1mask,chan1chan
                        rdlong  chan1mask,chan1mask     'Get first bit mask
                        
                        andn    outa,chan1mask          'Set to Zero
                        or      dira,chan1mask          'Set as output

                        add     chan1busy,chan1chan     'Compute @'busy buffer
                        add     chan1free,chan1chan     'Compute @'free buffer

                        rdword  chan2chan,bt2           'Load Cog tbl entry
                        add     bt2,#2

                        add     chan2mask,chan2chan
                        rdlong  chan2mask,chan2mask     'Get second bit mask
                        
                        andn    outa,chan2mask          'Set to Zero
                        or      dira,chan2mask          'Set as output
                        
                        add     chan2busy,chan2chan     'Compute @'busy buffer
                        add     chan2free,chan2chan     'Compute @'free buffer

                        rdword  chan3chan,bt2           'Load Cog tbl entry
                        add     bt2,#2

                        add     chan3mask,chan3chan
                        rdlong  chan3mask,chan3mask     'Get third bit mask
                        
                        andn    outa,chan3mask          'Set to Zero
                        or      dira,chan3mask          'Set as output
                        
                        add     chan3busy,chan3chan     'Compute @'busy buffer
                        add     chan3free,chan3chan     'Compute @'free buffer

                        rdword  chan4chan,bt2           'Load Cog tbl entry

                        add     chan4mask,chan4chan
                        rdlong  chan4mask,chan4mask     'Get fourth bit mask
                        
                        andn    outa,chan4mask          'Set to Zero
                        or      dira,chan4mask          'Set as output
                        
                        add     chan4busy,chan4chan     'Compute @'busy buffer
                        add     chan4free,chan4chan     'Compute @'free buffer

'
'       Ready-to-roll
'
                        
                        mov     bt1,bcogtab
                        add     bt1,#CogTimBas

                        mov     bt2,#StateSync
                        wrbyte  bt2,bcogstate

:spin                   rdlong  btimbas,bt1     wz      'Start?
              if_z      jmp     #:spin

                        mov     bt1,#StateReady
                        wrbyte  bt1,bcogstate


'
'       Main process loop
'
'       Each 'channel' has ChanTimeSlice (actually, more like ChanTimeSlice - 12) clocks to
'       process whatever needs to be done before it must give up the processor to the next channel
'

'
'       Channel 1 Protocol
'
'       This routine generates the data stream to the color effect string.
'
chan4ret
                        waitcnt btimbas,#ChanTimeSlice  'Sync to clock
                        jmp     chan1rtn                ' . & run channel 1

chan1top
                        jmpret  chan1rtn,#chan1ret      '== PAUSE ==

                        rdbyte  chan1stat,#GblState     'Get current state
                        call    #chkcogstat             '. and check if all chans match

                        cmp     chan1stat,#StateRun wz  'Running?
              if_nz     jmp     #chan1ret               '. No - just wait
              
                        jmpret  chan1rtn,#chan1ret      '== PAUSE ==

                        rdword  chan1bfr,chan1busy  wz  'Load busy pointer - zero?   
              if_z      jmp     #chan1top               '. Yes, == PAUSE ==

                        add     chan1bfr,#BfrLeng       'Compute @'buffer length

                        rdbyte  chan1lng,chan1bfr       'Load long count

                        sub     chan1bfr,#BfrLeng       '. @'buffer
                        mov     chan1ptr,chan1bfr       'Restore
                        
                        add     chan1ptr,#BfrData       'Compute @'data

:xmit
                        mov     chan1ecnt,#1            'Assume not enumeration

                        jmpret  chan1rtn,#chan1ret      '== PAUSE ==

                        rdlong  chan1data,chan1ptr      'Load
                        add     chan1ptr,#4             '.  next data word

                        sub     chan1lng,#1         wz  'All data sent?
              if_z      wrlong  chan1bfr,chan1free      '. Yes - Request next buffer

                        test    chan1data,enummask  wz  'Enumeration?
              if_z      jmp     #:enumnot               '. No, start xmit

                        mov     chan1ecnt,enummask1     'Get lamp count
                        and     chan1ecnt,chan1data            
                        mov     chan1einc,enummask2     '. and isolate
                        and     chan1einc,chan1data     '. . levels

                        jmp     #chan1ret
                        
:enumlup
                        add     chan1edta,chan1einc     'Increment lamp #
                        mov     chan1data,chan1edta     'Copy enumeration data

:enumnot                        
                        mov     chan1edta,chan1data

                        mov     chan1bits,#27           'Get #'bits in word
                                                
                        rol     chan1data,#2            'Remove unused address bits
                        or      chan1data,#2            '. & set 'final' bit
                        
:lup                        
                        jmpret  chan1rtn,#chan1ret      '== PAUSE ==

                        or      outa,chan1mask          'Start bit                               

                        jmpret  chan1rtn,#chan1ret      '== PAUSE ==

                        andn    outa,chan1mask          'Intermediate

                        cmp     chan1bits,#13   wz      ' here we skip the nybble                           
              if_z      rol     chan1data,#4            ' . before the green data

                        jmpret  chan1rtn,#chan1ret      '== PAUSE ==
                                                   
                        rol     chan1data,#1    wc      'Data bit                        
              if_nc     or      outa,chan1mask

                        djnz    chan1bits,#:lup         'Loop for all data bits

                        mov     chan1bits,#ChanQuietBits 'Set count of 'quiet' bittimes
                                                        '. needed between commands
:idle                                               
                        jmpret  chan1rtn,#chan1ret      '== PAUSE ==

                        djnz    chan1bits,#chan1ret     'Loop for all 'quiet' bits
                        djnz    chan1ecnt,#:enumlup     'Loop for enumeration 
                        tjnz    chan1lng,#:xmit         'Loop for all words in command
                        jmp     #chan1top               'Loop forever



chan1ret

'
'       Channel 2 Protocol
'
'       This routine generates the data stream to the color effect string.
'
                        waitcnt btimbas,#ChanTimeSlice  'Sync to clock
                        jmp     chan2rtn                ' . & run channel 1

chan2top
                        jmpret  chan2rtn,#chan2ret      '== PAUSE ==

                        rdbyte  chan2stat,#GblState     'Get current state
                        call    #chkcogstat             '. and check if all chans match

                        cmp     chan2stat,#StateRun wz  'Running?
              if_nz     jmp     #chan2ret               '. No - just wait
              
                        jmpret  chan2rtn,#chan2ret      '== PAUSE ==

                        rdword  chan2bfr,chan2busy  wz  'Load busy pointer - zero?   
              if_z      jmp     #chan2top               '. Yes, == PAUSE ==

                        add     chan2bfr,#BfrLeng       'Compute @'buffer length

                        rdbyte  chan2lng,chan2bfr       'Load long count

                        sub     chan2bfr,#BfrLeng       '. @'buffer
                        mov     chan2ptr,chan2bfr       'Restore
                        
                        add     chan2ptr,#BfrData       'Compute @'data

:xmit
                        mov     chan2ecnt,#1            'Assume not enumeration

                        jmpret  chan2rtn,#chan2ret      '== PAUSE ==

                        rdlong  chan2data,chan2ptr      'Load
                        add     chan2ptr,#4             '.  next data word

                        sub     chan2lng,#1         wz  'All data sent?
              if_z      wrlong  chan2bfr,chan2free      '. Yes - Request next buffer

                        test    chan2data,enummask  wz  'Enumeration?
              if_z      jmp     #:enumnot               '. No, start xmit

                        mov     chan2ecnt,enummask1      'Get lamp count
                        and     chan2ecnt,chan2data
                        mov     chan2einc,enummask2     '. and isolate
                        and     chan2einc,chan2data     '. . levels

                        jmp     #chan2ret
                        
:enumlup
                        add     chan2edta,chan2einc     'Increment lamp #
                        mov     chan2data,chan2edta     'Copy enumeration data

:enumnot                        
                        mov     chan2edta,chan2data

                        mov     chan2bits,#27           'Get #'bits in word
                                                
                        rol     chan2data,#2            'Remove unused address bits
                        or      chan2data,#2            '. & set 'final' bit
                        
:lup                        
                        jmpret  chan2rtn,#chan2ret      '== PAUSE ==

                        or      outa,chan2mask          'Start bit                               

                        jmpret  chan2rtn,#chan2ret      '== PAUSE ==

                        andn    outa,chan2mask          'Intermediate

                        cmp     chan2bits,#13   wz      ' here we skip the nybble                           
              if_z      rol     chan2data,#4            ' . before the green data

                        jmpret  chan2rtn,#chan2ret      '== PAUSE ==
                                                   
                        rol     chan2data,#1    wc      'Data bit                        
              if_nc     or      outa,chan2mask

                        djnz    chan2bits,#:lup         'Loop for all data bits

                        mov     chan2bits,#ChanQuietBits 'Set count of 'quiet' bittimes
                                                        '. needed between commands
:idle                                               
                        jmpret  chan2rtn,#chan2ret      '== PAUSE ==

                        djnz    chan2bits,#chan2ret     'Loop for all 'quiet' bits
                        djnz    chan2ecnt,#:enumlup     'Loop for enumeration 
                        tjnz    chan2lng,#:xmit         'Loop for all words in command
                        jmp     #chan2top               'Loop forever



chan2ret

'
'       Channel 3 Protocol
'
'       This routine generates the data stream to the color effect string.
'
                        waitcnt btimbas,#ChanTimeSlice  'Sync to clock
                        jmp     chan3rtn                ' . & run channel 1

chan3top
                        jmpret  chan3rtn,#chan3ret      '== PAUSE ==

                        rdbyte  chan3stat,#GblState     'Get current state
                        call    #chkcogstat             '. and check if all chans match

                        cmp     chan3stat,#StateRun wz  'Running?
              if_nz     jmp     #chan3ret               '. No - just wait
              
                        jmpret  chan3rtn,#chan3ret      '== PAUSE ==

                        rdword  chan3bfr,chan3busy  wz  'Load busy pointer - zero?   
              if_z      jmp     #chan3top               '. Yes, == PAUSE ==

                        add     chan3bfr,#BfrLeng       'Compute @'buffer length

                        rdbyte  chan3lng,chan3bfr       'Load long count

                        sub     chan3bfr,#BfrLeng       '. @'buffer
                        mov     chan3ptr,chan3bfr       'Restore
                        
                        add     chan3ptr,#BfrData       'Compute @'data

:xmit
                        mov     chan3ecnt,#1            'Assume not enumeration

                        jmpret  chan3rtn,#chan3ret      '== PAUSE ==

                        rdlong  chan3data,chan3ptr      'Load
                        add     chan3ptr,#4             '.  next data word

                        sub     chan3lng,#1         wz  'All data sent?
              if_z      wrlong  chan3bfr,chan3free      '. Yes - Request next buffer

                        test    chan3data,enummask  wz  'Enumeration?
              if_z      jmp     #:enumnot               '. No, start xmit

                        mov     chan3ecnt,enummask1      'Get lamp count
                        and     chan3ecnt,chan3data
                        mov     chan3einc,enummask2     '. and isolate
                        and     chan3einc,chan3data     '. . levels

                        jmp     #chan3ret
                        
:enumlup
                        add     chan3edta,chan3einc     'Increment lamp #
                        mov     chan3data,chan3edta     'Copy enumeration data

:enumnot                        
                        mov     chan3edta,chan3data

                        mov     chan3bits,#27           'Get #'bits in word
                                                
                        rol     chan3data,#2            'Remove unused address bits
                        or      chan3data,#2            '. & set 'final' bit
                        
:lup                        
                        jmpret  chan3rtn,#chan3ret      '== PAUSE ==

                        or      outa,chan3mask          'Start bit                               

                        jmpret  chan3rtn,#chan3ret      '== PAUSE ==

                        andn    outa,chan3mask          'Intermediate

                        cmp     chan3bits,#13   wz      ' here we skip the nybble                           
              if_z      rol     chan3data,#4            ' . before the green data

                        jmpret  chan3rtn,#chan3ret      '== PAUSE ==
                                                   
                        rol     chan3data,#1    wc      'Data bit                        
              if_nc     or      outa,chan3mask

                        djnz    chan3bits,#:lup         'Loop for all data bits

                        mov     chan3bits,#ChanQuietBits 'Set count of 'quiet' bittimes
                                                        '. needed between commands
:idle                                               
                        jmpret  chan3rtn,#chan3ret      '== PAUSE ==

                        djnz    chan3bits,#chan3ret     'Loop for all 'quiet' bits
                        djnz    chan3ecnt,#:enumlup     'Loop for enumeration 
                        tjnz    chan3lng,#:xmit         'Loop for all words in command
                        jmp     #chan3top               'Loop forever


chan3ret

'
'       Channel 4 Protocol
'
'       This routine generates the data stream to the color effect string.
'
                        waitcnt btimbas,#ChanTimeSlice  'Sync to clock
                        jmp     chan4rtn                ' . & run channel 1

chan4activ
                                                tjz             chan4mask,#chan4ret

chan4top
                        jmpret  chan4rtn,#chan4ret      '== PAUSE ==

                        rdbyte  chan4stat,#GblState     'Get current state
                        call    #chkcogstat             '. and check if all chans match

                        cmp     chan4stat,#StateRun wz  'Running?
              if_nz     jmp     #chan4ret               '. No - just wait
              
                        jmpret  chan4rtn,#chan4ret      '== PAUSE ==

                        rdword  chan4bfr,chan4busy  wz  'Load busy pointer - zero?   
              if_z      jmp     #chan4top               '. Yes, == PAUSE ==

                        add     chan4bfr,#BfrLeng       'Compute @'buffer length

                        rdbyte  chan4lng,chan4bfr       'Load long count

                        sub     chan4bfr,#BfrLeng       '. @'buffer
                        mov     chan4ptr,chan4bfr       'Restore
                        
                        add     chan4ptr,#BfrData       'Compute @'data

:xmit
                        mov     chan4ecnt,#1            'Assume not enumeration

                        jmpret  chan4rtn,#chan4ret      '== PAUSE ==

                        rdlong  chan4data,chan4ptr      'Load
                        add     chan4ptr,#4             '.  next data word

                        sub     chan4lng,#1         wz  'All data sent?
              if_z      wrlong  chan4bfr,chan4free      '. Yes - Request next buffer

                        test    chan4data,enummask  wz  'Enumeration?
              if_z      jmp     #:enumnot               '. No, start xmit

                        mov     chan4ecnt,enummask1     'Get lamp count
                        and     chan4ecnt,chan4data
                        mov     chan4einc,enummask2     '. and isolate
                        and     chan4einc,chan4data     '. . levels

                        jmp     #chan4ret
                        
:enumlup
                        add     chan4edta,chan4einc     'Increment lamp #
                        mov     chan4data,chan4edta     'Copy enumeration data

:enumnot                        
                        mov     chan4edta,chan4data

                        mov     chan4bits,#27           'Get #'bits in word
                                                
                        rol     chan4data,#2            'Remove unused address bits
                        or      chan4data,#2            '. & set 'final' bit
                        
:lup                        
                        jmpret  chan4rtn,#chan4ret      '== PAUSE ==

                        or      outa,chan4mask          'Start bit                               

                        jmpret  chan4rtn,#chan4ret      '== PAUSE ==

                        andn    outa,chan4mask          'Intermediate

                        cmp     chan4bits,#13   wz      ' here we skip the nybble                           
              if_z      rol     chan4data,#4            ' . before the green data

                        jmpret  chan4rtn,#chan4ret      '== PAUSE ==
                                                   
                        rol     chan4data,#1    wc      'Data bit                        
              if_nc     or      outa,chan4mask

                        djnz    chan4bits,#:lup         'Loop for all data bits

                        mov     chan4bits,#ChanQuietBits 'Set count of 'quiet' bittimes
                                                        '. needed between commands
:idle                                               
                        jmpret  chan4rtn,#chan4ret      '== PAUSE ==

                        djnz    chan4bits,#chan4ret     'Loop for all 'quiet' bits
                        djnz    chan4ecnt,#:enumlup     'Loop for enumeration 
                        tjnz    chan4lng,#:xmit         'Loop for all words in command
                        jmp     #chan4top               'Loop forever


'
'       check for all channel state matching.  If so, set cog state
'
chkcogstat
                        cmp     chan1stat,chan2stat wz  'All
              if_z      cmp     chan2stat,chan3stat wz  '. the
              if_z      cmp     chan3stat,chan4stat wz  '. . same?
              if_z      wrbyte  chan1stat,bcogstate     '. Yes - set cog status
chkcogstat_ret          ret


binitlock               long    InitLock
bzero                   long    0
bcogtab                 long    0
bcogstate               long    CogState

bt1                     long    0
bt2                     long    0

btimbas                 long    0

enummask                long    $80000000
enummask1               long    $000000ff
enummask2               long    $7fff0000

chan1chan               long    0
chan1stat               long    StateStop
chan1busy               long    ChanTblBusy
chan1free               long    ChanTblFree
chan1mask               long    ChanTblMask
chan1rtn                long    chan1top
chan1data               long    0
chan1bits               long    0
chan1bfr                long    0
chan1lng                long    0
chan1ptr                long    0
chan1ecnt               long    0
chan1edta               long    0
chan1einc               long    0

chan2chan               long    0
chan2stat               long    StateStop
chan2busy               long    ChanTblBusy
chan2free               long    ChanTblFree
chan2mask               long    ChanTblMask
chan2rtn                long    chan2top
chan2data               long    0
chan2bits               long    0
chan2bfr                long    0
chan2lng                long    0
chan2ptr                long    0
chan2ecnt               long    0
chan2edta               long    0
chan2einc               long    0

chan3chan               long    0
chan3stat               long    StateStop
chan3busy               long    ChanTblBusy
chan3free               long    ChanTblFree
chan3mask               long    ChanTblMask
chan3rtn                long    chan3top
chan3data               long    0
chan3bits               long    0
chan3bfr                long    0
chan3lng                long    0
chan3ptr                long    0
chan3ecnt               long    0
chan3edta               long    0
chan3einc               long    0

chan4chan               long    0
chan4stat               long    StateStop
chan4busy               long    ChanTblBusy
chan4free               long    ChanTblFree
chan4mask               long    ChanTblMask
chan4rtn                long    chan4activ
chan4data               long    0
chan4bits               long    0
chan4bfr                long    0
chan4lng                long    0
chan4ptr                long    0
chan4ecnt               long    0
chan4edta               long    0
chan4einc               long    0


                        fit     496
                        