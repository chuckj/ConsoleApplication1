'
' USB message format (Host to prop):
'
' shown in 'line' byte order (lsb first)
' +----------+----------+----------+----------+
' |          |          |          |          |
' | <msgtyp> |   <seq>  | <length> | <chksum> |
' |          |          |          |          |
' +----------+----------+----------+----------+
' |          |          |          |          |
' |     <time (ms)>     |    <time (secs)>    |
' |  <lsb>   |   <msb>  |  <lsb>   |  <msb>   |
' +----------+----------+----------+----------+
' |          |          |          |          |
' |                  <data>                   |
' |          |          |          |          |
' +----------+----------+----------+----------+
' |          |          |          |          |
'
'  <msgtyp> - 80 - echo
'             81 - set timer
'             82 - run
'             83 - pause
'             84 - stop
'             ...
'             90-9f - port data
'  <seq> - sequential number assigned to message (0-255)
'  <length> - number of longs in message - including header
'  <time> - seconds and milliseconds
'  <data> - command dependent
'
'
' USB acknowledge message format (Prop to Host)
'
' +----------+----------+----------+----------+
' |          |          |          |          |
' |   <80>   |  <seq>   |<freebufs>|          |
' |          |          |          |          |
' +----------+----------+----------+----------+
'
'
' USB negative acknowledge message format (Prop to Host)
'
' +----------+----------+----------+----------+
' |          |  <last   |          |          |
' |  81-84   |   good   |<freebufs>| <length> |
' |          |   seq>   |          |          |
' +----------+----------+----------+----------+
'
'
' USB processed message format (Prop to Host)
'
' +----------+----------+----------+----------+
' |          |  <last   |          |          |
' |    90    |   good   |<freebufs>|          |
' |          |   seq>   |          |          |
' +----------+----------+----------+----------+
'
'

CON
        _CLKMODE = XTAL1 + PLL16X
        _CLKFREQ = 80_000_000

        _CLK1MS = _CLKFREQ / 1000

        _CLKTICKS = 320
        _CLKCLKS = _CLK1MS / _CLKTICKS
        
        USBRxE = 8
        USBTxF = 9
        USBRd = 10
        USBWr = 11

        
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
        CogTblLng = 4
        CogTblSiz = CogTblLng * 4
        CogTblShft = 4
        
        CogState = 0
        CogTimBas = 4
        CogChanTab = 8

        
        ProtoCog = 1
        ProtoCogCnt = 4
        USBCog = 5
        BufferCog = 6
        TimerCog = 7


        
        ChanTblCnt = 16
        ChanTblLng = 4
        ChanTblSiz = ChanTblLng * 4
        ChanTblShft = 4
        ChanTblMask = 0
        ChanTblFree = 4                                 'Free pointer must immediately preceed Busy
        ChanTblBusy = 6
        ChanTblHead = 8
        ChanTblTail = 10


        BufferLng = 68                                  'Longs
        BufferSiz = BufferLng * 4                       'Bytes

        StateInit = 1
        StateSync = 2
        StateReady = 3
        StateStop = 4
        StateRun = 5
        StatePause = 6


        QueueLock = 0                                   '0 through 3
        QueueLockCnt = 4                                '1 for each protocol cog
        InitLock = 7
        FreeLock = 6


        BfrLink = 0
        BfrLeng = 2
        BfrTimer = 4
        BfrData = 8

        
Pub Start

    lockset(InitLock)

' Start protocol cogs
    coginit(ProtoCog+0 ,@ProtoEntry, 0)                 'Cog # should correspond to ProtoCog value

    coginit(ProtoCog+1, @ProtoEntry, 0)

    coginit(ProtoCog+2, @ProtoEntry, 0)

    coginit(ProtoCog+3, @ProtoEntry, 0)

' Start USB cog
    coginit(USBCog, @USBEntry, 0)

' Start buffer handler      
    coginit(BufferCog, @BfrMgrEntry, 0)

' Start timer cog
    coginit(TimerCog, @InitEntry, 0)                    'Start timer last so init can clear memory

' Done
    cogstop(cogid)
    
DAT

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
                        add     iaddr,#CogTblSiz*CogTblCnt

'
'       Chan
'
                        mov     ichntab,iaddr
                        wrword  ichntab,#GblChanTbl
                        add     iaddr,#ChanTblSiz*ChanTblCnt


                        mov     it1,ichntab
                        mov     it2,icogtab
                        add     it2,#ProtoCog*CogTblSiz+CogChanTab
                        
'
'       ChanTbl[0]
'
                        wrword  it1,it2
                        add     it1,#ChanTblMask
                        wrlong  ichan01msk,it1
                        add     it1,#ChanTblSiz-ChanTblMask
                        add     it2,#2
                        
'
'       ChanTbl[1]
'
                        wrword  it1,it2
                        add     it1,#ChanTblMask
                        wrlong  ichan02msk,it1
                        add     it1,#ChanTblSiz-ChanTblMask
                        add     it2,#2
                        
'
'       ChanTbl[2]
'
                        wrword  it1,it2
                        add     it1,#ChanTblMask
                        wrlong  ichan03msk,it1
                        add     it1,#ChanTblSiz-ChanTblMask
                        add     it2,#2
                        
'
'       ChanTbl[3]
'
                        wrword  it1,it2
                        add     it1,#ChanTblMask
                        wrlong  ichan04msk,it1
                        add     it1,#ChanTblSiz-ChanTblMask
                        add     it2,#CogTblSiz-6

                        
'
'       ChanTbl[4]
'
                        wrword  it1,it2
                        add     it1,#ChanTblMask
                        wrlong  ichan05msk,it1
                        add     it1,#ChanTblSiz-ChanTblMask
                        add     it2,#2
                        
'
'       ChanTbl[5]
'
                        wrword  it1,it2
                        add     it1,#ChanTblMask
                        wrlong  ichan06msk,it1
                        add     it1,#ChanTblSiz-ChanTblMask
                        add     it2,#2
                        
'
'       ChanTbl[6]
'
                        wrword  it1,it2
                        add     it1,#ChanTblMask
                        wrlong  ichan07msk,it1
                        add     it1,#ChanTblSiz-ChanTblMask
                        add     it2,#2
                        
'
'       ChanTbl[7]
'
                        wrword  it1,it2
                        add     it1,#ChanTblMask
                        wrlong  ichan08msk,it1
                        add     it1,#ChanTblSiz-ChanTblMask
                        add     it2,#CogTblSiz-6

                        
'
'       ChanTbl[8]
'
                        wrword  it1,it2
                        add     it1,#ChanTblMask
                        wrlong  ichan09msk,it1
                        add     it1,#ChanTblSiz-ChanTblMask
                        add     it2,#2
                        
'
'       ChanTbl[9]
'
                        wrword  it1,it2
                        add     it1,#ChanTblMask
                        wrlong  ichan10msk,it1
                        add     it1,#ChanTblSiz-ChanTblMask
                        add     it2,#2
                        
'
'       ChanTbl[10]
'
                        wrword  it1,it2
                        add     it1,#ChanTblMask
                        wrlong  ichan11msk,it1
                        add     it1,#ChanTblSiz-ChanTblMask
                        add     it2,#2
                        
'
'       ChanTbl[11]
'
                        wrword  it1,it2
                        add     it1,#ChanTblMask
                        wrlong  ichan12msk,it1
                        add     it1,#ChanTblSiz-ChanTblMask
                        add     it2,#CogTblSiz-6

                        
'
'       ChanTbl[12]
'
                        wrword  it1,it2
                        add     it1,#ChanTblMask
                        wrlong  ichan13msk,it1
                        add     it1,#ChanTblSiz-ChanTblMask
                        add     it2,#2
                        
'
'       ChanTbl[13]
'
                        wrword  it1,it2
                        add     it1,#ChanTblMask
                        wrlong  ichan14msk,it1
                        add     it1,#ChanTblSiz-ChanTblMask
                        add     it2,#2
                        
'
'       ChanTbl[14]
'
                        wrword  it1,it2
                        add     it1,#ChanTblMask
                        wrlong  ichan15msk,it1
                        add     it1,#ChanTblSiz-ChanTblMask
                        add     it2,#2
                        
'
'       ChanTbl[15]
'
                        wrword  it1,it2
                        add     it1,#ChanTblMask
                        wrlong  ichan16msk,it1
                        add     it1,#ChanTblSiz-ChanTblMask
                        
                        wrword  it1,#GblFreeBgn         'End of allocated space - begin of buffer pool

'                       
'       Strunctures complete - let the other cogs run
'
                        lockclr iinitlock               'Init complete...


'
'       Wait for all of the protocol cogs to be ready for synchronous start
'
                        mov     it1,#ProtoCogCnt
                        mov     it2,icogtab
                        add     it2,#ProtoCog*CogTblSiz+CogState

:lup
                        rdbyte  it3,it2
                        cmp     it3,#StateSync  wz
              if_nz     jmp     #:lup

                        add     it2,#CogTblSiz
                        djnz    it1,#:lup
                        
'
'       Now, stagger start the protocol cogs...
'               
                        mov     it2,icogtab
                        add     it2,#ProtoCog*CogTblSiz+CogTimBas
                        mov     it1,cnt
                        add     it1,#100
                        wrlong  it1,it2
                        add     it1,#20
                        add     it2,#CogTblSiz
                        wrlong  it1,it2
                        add     it1,#20
                        add     it2,#CogTblSiz
                        wrlong  it1,it2
                        add     it1,#20
                        add     it2,#CogTblSiz
                        wrlong  it1,it2




'
'       Timer
'

TimerEntry
                        rdword  tcogtab,#GblCogTbl      'Get @'cog table
                        add     tcogstate,tcogtab       '. & @'our cog state

                        mov     tstate,#StateReady
                        wrbyte  tstate,tcogstate

:spin
                        rdbyte  tstate,#GblState
                        cmp     tstate,#StateReady wz,wc
              if_be     jmp     #:spin       
                        
'
'       Everyone should be running
'

Timer                                                              
                        add     timbas,cnt              'Adjust current time 

                        wrbyte  tstate,tcogstate

tNewState
                        cmp     tstate,#StateRun        wz
              if_z      jmp     #tRun
                        cmp     tstate,#StatePause      wz
              if_z      jmp     #tPause

tStop
                        wrlong  tzero,#GblTimer
                        mov     tticker,#_CLKTICKS
                        wrbyte  tstate,tcogstate

:lup
                        waitcnt timbas,#_CLKCLKS           'Wait for time

                        rdbyte  tstate,#GblState
                        cmp     tstate,#StateStop       wz
              if_z      jmp     #:lup
                        jmp     #tNewState

tRun
                        wrbyte  tstate,tcogstate

:lup
                        waitcnt timbas,#_CLKCLKS       'Wait for time

                        rdbyte  tstate,#GblState
                        cmp     tstate,#StateRun    wz
              if_nz     jmp     #tNewState
                        djnz    tticker,#:lup

                        mov     tticker,#_CLKTICKS
                        
                        rdlong  tt1,#GblTimer           'Load current time value

                        add     tt1,#1                  'Increment the lo-order (ms) part
                        mov     tt2,tt1                 '. &
                        and     tt2,tword               '.   isolate it
                        cmp     tt2,t1000           wz  '1000ms?
              if_z      add     tt1,tsecint             '. Yes, adjust time o next second
                        
                        wrlong  tt1,#GblTimer           'Store current time value
                        
                        jmp     #:lup                   'Loop

tPause
                        wrbyte  tstate,tcogstate

:lup
                        waitcnt timbas,#_CLKCLKS       'Wait for time

                        rdbyte  tstate,#GblState
                        cmp     tstate,#StatePause  wz
              if_z      jmp     #:lup
                        jmp     #tNewState



iinitlock               long    InitLock
it1                     long    StateInit
it2                     long    0
it3                     long    0
iaddr                   long    $8000
icogtab                 long    0
ichntab                 long    0
                        
ichan01msk              long    1 << Chan01Bit
ichan02msk              long    1 << Chan02Bit
ichan03msk              long    1 << Chan03Bit
ichan04msk              long    1 << Chan04Bit

ichan05msk              long    1 << Chan05Bit
ichan06msk              long    1 << Chan06Bit
ichan07msk              long    1 << Chan07Bit
ichan08msk              long    1 << Chan08Bit

ichan09msk              long    1 << Chan09Bit
ichan10msk              long    1 << Chan10Bit
ichan11msk              long    1 << Chan11Bit
ichan12msk              long    1 << Chan12Bit

ichan13msk              long    1 << Chan13Bit
ichan14msk              long    1 << Chan14Bit
ichan15msk              long    1 << Chan15Bit
ichan16msk              long    1 << Chan16Bit


                                       
tcogtab                 long    0
tcogstate               long    TimerCog*CogTblSiz+CogState
tstate                  long    StateReady

tticker                 long    0
tzero                   long    0
tt1                     long    0
tt2                     long    0
t1000                   long    1000
tword                   long    $0ffff
tsecint                 long    $10000-1000
timbas                  long    320




                        fit     496

DAT

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

                        rdword  ucogtab,#GblCogTbl      'Get @'Cog Tbl
                        add     ucogstate,ucogtab       '. & @'our cog state

                        or      outa,usbrdmask          'Deassert
                        or      dira,usbrdmask          '. & set as output
                        
                        andn    outa,usbwrmask          'Deassert
                        or      dira,usbwrmask          '. & set as output

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

'
'       At this point, all the protocol cogs should be ready & stopped, waiting for data from the
'       USB.  This cog will now accept data from the USB port, and push it, buffer by buffer
'       to the channels
'
'       Receiver
'                        
USBLoop

                        mov     utxmrtn,#utxmtop
                                                
urcvtop
                        call    #urcvlong                'Wait
              if_c      jmp     #urcvtop                 '. for a command


:newcmd
                        mov     urcvfull,urcvdata        'Isolate
                        mov     urcvcmd,#$ff             '. the parts
                        and     urcvcmd,urcvdata         '
                        mov     urcvseq,#$ff             '
                        ror     urcvdata,#8              '
                        and     urcvseq,urcvdata         '
                        mov     urcvlng,#$ff             '
                        ror     urcvdata,#8              '
                        and     urcvlng,urcvdata          '

                        ror     urcvdata,#8              'Isolate
                        and     urcvdata,#$ff            '. chksum
                        cmp     urcvdata,#$ff   wz
              if_z      jmp     #:bypchk
                        xor     urcvdata,urcvcmd
                        xor     urcvdata,urcvlng
                        xor     urcvdata,urcvseq  wz    'Checksum valid?
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

cmd84                   djnz    urcvwrk,#cmd90          'Not 84 (stop) - maybe 90?
                        mov     ut0,#StateStop          'Get new state

newstate
                        cmp     urcvlng,#1       wz     'L' valid?
              if_ne     jmp     #sndnack_lng            '. No - bad L'

                        wrbyte  ut0,#GblState           'Set new global state
                        wrbyte  ut0,ucogstate           '. & our cog state

                        call    #chkstate               'Wait for all cogs
                        jmp     #sndack                 'Send ACK & loop

cmd90
                        sub     urcvwrk,#$90-$7f-5   wc,wz
              if_b      jmp     #sndnack_cmd
                        cmp     urcvwrk,#16      wc,wz
              if_ae     jmp     #sndnack_cmd

                        mov     ut1,urcvlastseq
                        add     ut1,#1
                        and     ut1,#$ff
                        cmp     ut1,urcvseq     wz
              if_ne     jmp     #sndnack_seq
                            
                        sub     urcvlng,#2       wc,wz  'L' valid?
              if_be     jmp     #sndnack_lng            '. No -
                        cmp     urcvlng,#63      wz,wc  'L' valid?
              if_a      jmp     #sndnack_lng            '. No -
                               
'
'       allocate a buffer
'
                        mov     urcvrtn,#:spin
                        
:spin                   lockset ufreelock           wc
              if_c      jmp     #utxmrtn

                        rdword  urcvbufr,#GblFreeChain wz   'load @'next buffer
              if_z      jmp     #:unlk

                        rdword  ut1,urcvbufr             'unchain
                        
                        wrword  ut1,#GblFreeChain       '. the buffer

                        rdbyte  urcvfreecnt,#GblFreeCnt   'Decrement
                        sub     urcvfreecnt,#1            '.
                        wrbyte  urcvfreecnt,#GblFreeCnt   '. buffer count

:unlk
                        lockclr ufreelock                   '. & clear the lock
                        
              if_z      jmp     #utxmrtn                 'no buffer - try again
              
'
'       receive msg
'
                        mov     urcvptr,urcvbufr          'Copy @'              
                        mov     urcvdata,urcvfull         '. & restore data
                        wrlong  uzero,urcvptr
                        add     urcvptr,#BfrLeng          'Bump @'data area
                        wrword  urcvlng,urcvptr
                        add     urcvptr,#BfrTimer-BfrLeng 'Bump @'data area
                        add     urcvlng,#1                'increment l'
                        
:lup
                        call    #urcvlong                'Get another long
                        
                        wrlong  urcvdata,urcvptr         '. & write command there
                        add     urcvptr,#4               'Bump @'data area
                        djnz    urcvlng,#:lup            'Loop thru data

'
'       queue buffer on channel
'
                        mov     urcvchan,urcvwrk
                        shl     urcvchan,#ChanTblShft              '16 bytes each
                        add     urcvchan,uchntab

                        mov     urcvlock,urcvwrk
                        shr     urcvlock,#2

                        
:spin2                  lockset urcvlock            wc      'Lock   
              if_c      jmp     #:spin2                 '. the cog (channel head/tail)

                        mov     urcvptr,#ChanTblTail
                        add     urcvptr,urcvchan
                        rdword  ut1,urcvptr      wz

                        wrword  urcvbufr,urcvptr

                        sub     urcvptr,#ChanTblTail-ChanTblHead
                        
              if_nz     wrword  urcvbufr,ut1
              if_z      wrword  urcvbufr,urcvptr                                                
                        
                        lockclr urcvlock                    'Clear the lock   

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
'                       jmp     sndxack

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
                        tjz     xtxmdtacnt,#:skp

                        movd    :sav,xtxmdtaadr
                        add     xtxmdtaadr,#1
                        sub     xtxmdtacnt,#1
:sav                    mov     $-$,utxmxack

:skp
                        mov     utxmxack,#0
                        jmp     #utxmtop



                        mov     utxmdata,utxmxack
                        mov     utxmxack,#0
                        
                        mov     utxmrtn,#:byt1
                        
:byt1                   test    usbtxfmask,ina          wz
              if_z      jmp     urcvrtn

                        or      dira,#$ff
                        or      outa,utxmdata
                        or      outa,usbwrmask
                        ror     utxmdata,#8
                        andn    outa,usbwrmask
                        andn    dira,#$ff

                        mov     utxmrtn,#:byt2
                        
:byt2                   test    usbtxfmask,ina          wz
              if_nz     jmp     urcvrtn

                        or      dira,#$ff
                        or      outa,utxmdata
                        or      outa,usbwrmask
                        ror     utxmdata,#8
                        andn    outa,usbwrmask
                        andn    dira,#$ff

                        mov     utxmrtn,#:byt3
                        
:byt3                   test    usbtxfmask,ina          wz
              if_nz     jmp     urcvrtn

                        or      dira,#$ff
                        or      outa,utxmdata
                        or      outa,usbwrmask
                        ror     utxmdata,#8
                        andn    outa,usbwrmask
                        andn    dira,#$ff

                        mov     utxmrtn,#:byt4
                        
:byt4                   test    usbtxfmask,ina          wz
              if_nz     jmp     urcvrtn

                        or      dira,#$ff
                        or      outa,utxmdata
                        or      outa,usbwrmask
                        nop
                        andn    outa,usbwrmask
                        andn    dira,#$ff

                        jmp     #utxmtop                        


                        

urcvlong
                        mov     urcvrtn,#$+1
                        tjz     xrcvdtacnt,#urcvtmo

                        movs    :lod,xrcvdtaadr
                        add     xrcvdtaadr,#1
                        sub     xrcvdtacnt,#1
:lod                    mov     urcvdata,$-$
                        jmp     #urcvlong_ret




                        mov     urcvtrys,#0              
                        mov     urcvrtn,#:byt1

:byt1                   test    usbrxemask,ina          wz                           
              if_nz     jmp     #urcvtmo

                        andn    outa,usbrdmask
                        nop
                        movi    urcvdata,ina
                        or      outa,usbrdmask
                        
                        mov     urcvrtn,#:byt2

:byt2                   test    usbrxemask,ina          wz
              if_nz     jmp     #urcvtmo

                        andn    outa,usbrdmask
                        shr     urcvdata,#8
                        movi    urcvdata,ina
                        or      outa,usbrdmask

                        mov     urcvrtn,#:byt3

:byt3                   test    usbrxemask,ina          wz
              if_nz     jmp     #urcvtmo

                        andn    outa,usbrdmask
                        shr     urcvdata,#8
                        movi    urcvdata,ina
                        or      outa,usbrdmask

                        mov     urcvrtn,#:byt4

:byt4                   test    usbrxemask,ina          wz
              if_nz     jmp     #urcvtmo

                        andn    outa,usbrdmask
                        shr     urcvdata,#7
                        shr     urcvdata,#1              wc
                        movi    urcvdata,ina             
                        or      outa,usbrdmask
                        rcl     urcvdata,#1
                        
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
              if_nz     tjnz    utxmrtn,utxmrtn                  '. No - wait a little longer
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

usc0    long  0
usc1    long  0
usc2    long  0
usc3    long  0

uchntab                 long    0
ucogtab                 long    0
ucogstate               long    USBCog*CogTblSiz+CogState
uzero                   long    0
uone                    long    1
                                                                                 
uwordmask               long    $8000


USBRxEMask              long    1 << USBRxE
USBTxFMask              long    1 << USBTxF
USBRdMask               long    1 << USBRd
USBWrMask               long    1 << USBWr

xrcvdtaadr    long      xrcvdtabgn
xrcvdtacnt    long      xrcvdtaend-xrcvdtabgn

xrcvdtabgn
                        long    $ff030190
                        long    $0
                        long    $12340678
                        long    $ff030291
                        long    $0
                        long    $12340678
                        long    $ff030392
                        long    $0
                        long    $12340678
                        long    $ff030493
                        long    $0
                        long    $12340678
                        long    $ff030594
                        long    $0
                        long    $12340678
                        long    $ff030698
                        long    $0
                        long    $12340678
                        long    $ff03079c
                        long    $0
                        long    $12340678
                        long    $ff03089f
                        long    $0
                        long    $12340678
                        
                        long    $ff030990
                        long    $1
                        long    $00340678
                        long    $ff030a91
                        long    $1
                        long    $00340678
                        long    $ff030b92
                        long    $1
                        long    $00340678
                        long    $ff030c93
                        long    $1
                        long    $00340678
                        long    $ff030d94
                        long    $2
                        long    $00340678
                        long    $ff030e98
                        long    $2
                        long    $00340678
                        long    $ff030f9c
                        long    $2
                        long    $00340678
                        long    $ff03109f
                        long    $2
                        long    $00340678
                        
                        long    $ff010082
'                        long    $ff030990
'                        long    $0
'                        long    $12340678
xrcvdtaend

xtxmdtaadr    long      xtxmdtabgn
xtxmdtacnt    long      xtxmdtaend-xtxmdtabgn

xtxmdtabgn
                        long    0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
                        long    0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
                        long    0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
xtxmdtaend

                        fit     496

DAT
   
                        org     0
'
'
' Entry
'
BfrMgrEntry

:init                   lockset minitlock           wc
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
              if_nz     jmp     stopped
              
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


                        add     mbufr,#BfrTimer                'Load
                        rdlong  mt1,mbufr       wz      '. time from buffer - zero?
                        sub     mbufr,#BfrTimer                ' (restore @'buffer)
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
                       
                        org     0
'
'
' Entry
'
ProtoEntry
:init                   lockset binitlock           wc
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
'       Each 'channel' has 80 (actually, more like 68) clocks or 20 (17) instructions to
'       process whatever needs to be done before it must give up the processor to the next channel
'

'
'       Channel 1 Protocol
'
'       This routine generates the data stream to the color effect string.
'
chan4ret
                        waitcnt btimbas,#80             'Sync to clock
                        jmp     chan1rtn                ' . & run channel 1

chan1top
                        jmpret  chan1rtn,#chan1ret      '== PAUSE ==

                        rdbyte  chan1stat,#GblState
                        call    #chkcogstat

                        cmp     chan1stat,#StateRun wz  'Running?
              if_nz     jmp     #chan1ret               '. No - just wait
              
                        jmpret  chan1rtn,#chan1ret      '== PAUSE ==

                        rdword  chan1bfr,chan1busy wz   'Load busy pointer - zero?   
              if_z      jmp     #chan1ret               '. Yes, == PAUSE ==

                        add     chan1bfr,#BfrLeng             'Compute @'buffer length

                        rdbyte  chan1lng,chan1bfr       'Load long count

                        sub     chan1bfr,#BfrLeng       '. @'buffer
                        mov     chan1ptr,chan1bfr       'Restore
                        
                        add     chan1ptr,#BfrData-4         'Compute @'data - 4

:xmit
                        jmpret  chan1rtn,#chan1ret      '== PAUSE ==

                        add     chan1ptr,#4             'Load
                        rdlong  chan1data,chan1ptr      '. next data word
                        
                        mov     chan1bits,#27           'Get #'bits in word
                                                
                        sub     chan1lng,#1     wz      'All data sent?
              if_z      wrlong  chan1bfr,chan1free      '. Yes - Request next buffer

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

                        mov     chan1bits,#19           'Set count of 'quiet' bittimes
                                                        '. needed between commands
:idle                                               
                        jmpret  chan1rtn,#chan1ret      '== PAUSE ==

                        djnz    chan1bits,#chan1ret     'Loop for all 'quiet' bits
                        tjnz    chan1lng,#:xmit         'Loop for all words in command
                        jmp     #chan1top               'Loop forever

chan1ret

'
'       Channel 2 Protocol
'
'       This routine generates the data stream to the color effect string.
'
                        waitcnt btimbas,#80             'Sync to clock
                        jmp     chan2rtn                ' . & run channel 2

chan2top
                        jmpret  chan2rtn,#chan2ret      '== PAUSE ==

                        rdbyte  chan2stat,#GblState
                        call    #chkcogstat

                        cmp     chan2stat,#StateRun wz  'Running?
              if_nz     jmp     #chan2ret               '. No - just wait
              
                        jmpret  chan2rtn,#chan2ret      '== PAUSE ==

                        rdword  chan2bfr,chan2busy wz   'Load busy pointer - zero?   
              if_z      jmp     #chan2ret               '. Yes, == PAUSE ==

                        add     chan2bfr,#BfrLeng             'Compute @'buffer length

                        rdword  chan2lng,chan2bfr       'Load word count

                        sub     chan2bfr,#BfrLeng             '. @'buffer
                        mov     chan2ptr,chan2bfr       'Restore

                        add     chan2ptr,#BfrData-4         'Compute @'data - 4
:xmit
                        jmpret  chan2rtn,#chan2ret      '== PAUSE ==

                        add     chan2ptr,#4             'Load
                        rdword  chan2data,chan2ptr      '. next data word
                        
                        mov     chan2bits,#27           'Get #'bits in word
                                                
                        sub     chan2lng,#1     wz      'All data sent?
              if_z      wrlong  chan2bfr,chan2free      '. Yes - Request next buffer

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

                        mov     chan2bits,#19           'Set count of 'quiet' bittimes
                                                        '. needed between commands
:idle                                               
                        jmpret  chan2rtn,#chan2ret      '== PAUSE ==

                        djnz    chan2bits,#chan2ret     'Loop for all 'quiet' bits                        
                        tjnz    chan2lng,#:xmit         'Loop for all words in command
                        jmp     #chan2top               'Loop forever

chan2ret

'
'       Channel 3 Protocol
'
'       This routine generates the data stream to the color effect string.
'
                        waitcnt btimbas,#80             'Sync to clock
                        jmp     chan3rtn                ' . & run channel 3

chan3top
                        jmpret  chan3rtn,#chan3ret      '== PAUSE ==

                        rdbyte  chan3stat,#GblState
                        call    #chkcogstat

                        cmp     chan3stat,#StateRun wz  'Running?
              if_nz     jmp     #chan3ret               '. No - just wait
              
                        jmpret  chan3rtn,#chan3ret      '== PAUSE ==

                        rdword  chan3bfr,chan3busy wz   'Load busy pointer - zero?   
              if_z      jmp     #chan3ret               '. Yes, == PAUSE ==

                        add     chan3bfr,#BfrLeng             'Compute @'buffer length

                        rdword  chan3lng,chan3bfr       'Load word count

                        sub     chan3bfr,#BfrLeng             '. @'buffer
                        mov     chan3ptr,chan3bfr       'Restore

                        add     chan3ptr,#BfrData-4         'Compute @'data - 4

:xmit
                        jmpret  chan3rtn,#chan3ret      '== PAUSE ==

                        add     chan3ptr,#4             'Load
                        rdword  chan3data,chan3ptr      '. next data word
                        
                        mov     chan3bits,#27           'Get #'bits in word
                                                
                        sub     chan3lng,#1     wz      'All data sent?
              if_z      wrlong  chan3bfr,chan3free      '. Yes - Request next buffer

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

                        mov     chan3bits,#19           'Set count of 'quiet' bittimes
                                                        '. needed between commands
:idle                                               
                        jmpret  chan3rtn,#chan3ret      '== PAUSE ==

                        djnz    chan3bits,#chan3ret     'Loop for all 'quiet' bits                        
                        tjnz    chan3lng,#:xmit         'Loop for all words in command
                        jmp     #chan3top               'Loop forever

chan3ret

'
'       Channel 4 Protocol
'
'       This routine generates the data stream to the color effect string.
'
                        waitcnt btimbas,#80             'Sync to clock
                        jmp     chan4rtn                ' . & run channel 4

chan4top
                        jmpret  chan4rtn,#chan4ret      '== PAUSE ==

                        rdbyte  chan4stat,#GblState
                        call    #chkcogstat

                        cmp     chan4stat,#StateRun wz  'Running?
              if_nz     jmp     #chan4ret               '. No - just wait
              
                        jmpret  chan4rtn,#chan4ret      '== PAUSE ==

                        rdword  chan4bfr,chan4busy wz   'Load busy pointer - zero?   
              if_z      jmp     #chan4ret               '. Yes, == PAUSE ==

                        add     chan4bfr,#BfrLeng             'Compute @'buffer length

                        rdword  chan4lng,chan4bfr       'Load word count

                        sub     chan4bfr,#BfrLeng             '. @'buffer
                        mov     chan4ptr,chan4bfr       'Restore

                        add     chan4ptr,#BfrData-4         'Compute @'data - 4

:xmit
                        jmpret  chan4rtn,#chan4ret      '== PAUSE ==

                        add     chan4ptr,#4             'Load
                        rdword  chan4data,chan4ptr      '. next data word
                        
                        mov     chan4bits,#27           'Get #'bits in word
                                                
                        sub     chan4lng,#1     wz      'All data sent?
              if_z      wrlong  chan4bfr,chan4free      '. Yes - Request next buffer

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

                        mov     chan4bits,#19           'Set count of 'quiet' bittimes
                                                        '. needed between commands
:idle                                               
                        jmpret  chan4rtn,#chan4ret      '== PAUSE ==

                        djnz    chan4bits,#chan4ret     'Loop for all 'quiet' bits                        
                        tjnz    chan4lng,#:xmit         'Loop for all words in command
                        jmp     #chan4top               'Loop forever


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

basis                   long    0
basis1                  long    0
basis2                  long    0
basis3                  long    0
basis4                  long    0
basis5                  long    0
basis6                  long    0


btimbas                 long    0


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

chan4chan               long    0
chan4stat               long    StateStop
chan4busy               long    ChanTblBusy
chan4free               long    ChanTblFree
chan4mask               long    ChanTblMask
chan4rtn                long    chan4top
chan4data               long    0
chan4bits               long    0
chan4bfr                long    0
chan4lng                long    0
chan4ptr                long    0


                        fit     496

              