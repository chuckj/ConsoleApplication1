'
' USB message format (Host to prop):
'
' +----------+----------+----------+----------+
' |          |          |          |          |
' | <msgtyp> | <length> |  <seq>   | <chksum> |
' |          |          |          |          |
' +----------+----------+----------+----------+
' |          |          |          |          |
' |    <time (secs)>    |     <time (ms)>     |
' |          |          |          |          |
' +----------+----------+----------+----------+
' |          |          |          |          |
' |    00    |    00    |<portseq> |  <port>  |
' |          |          |          |          |
' +----------+----------+----------+----------+
' |          |          |          |          |
' |                  <data>                   |
' |          |          |          |          |
' +----------+----------+----------+----------+
' |          |          |          |          |
'
'  <msgtyp> - 80 - echo
'             81 - timer sync
'             82 - port data
'  <length> - number of longs in message - including header
'  <seq> - sequential number assigned to message (0-255)
'  <chksum> - xor of header should be 00
'  <time> - seconds and milliseconds (msb first)
'  <port> - output port (1-16)
'  <portseq> - port message seq #(0-255)
'  <data> - command dependent
'
'
' USB acknowledge message format (Prop to Host)
'
' +----------+----------+----------+----------+
' |          |          |          |          |
' |    80    | <length> |  <seq>   | <chksum> |
' |          |          |          |          |
' +----------+----------+----------+----------+
'
'
' USB negative acknowledge message format (Prop to Host)
'
' +----------+----------+----------+----------+
' |          |          |          |          |
' | 81/82/83 | <length> |  <seq>   | <chksum> |
' |          |          |          |          |
' +----------+----------+----------+----------+
'
'
' USB processed message format (Prop to Host)
'
' +----------+----------+----------+----------+
' |          |          |          |          |
' |    90    |    00    |<portseq> |  <port>  |
' |          |          |          |          |
' +----------+----------+----------+----------+
'
'

CON
        _CLKMODE = XTAL1 + PLL16X
        _CLKFREQ = 80_000_000

        _CLK1MS = _CLKFREQ / 1000


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


        GblTime = 0
        GblFreeChain = 4
        GblCogTbl = 6
        GblChanTbl = 8
        GblAckTbl = 10
        

        CogTblCnt = 8
        CogTblLng = 2
        CogTblSiz = CogTblLng * 4


        ChanTblCnt = 16
        ChanTblLng = 4
        ChanTblSiz = ChanTblLng * 4
        ChanTblMask = 0
        ChanTblFree = 4                                 'Free pointer must immediately preceed Busy
        ChanTblBusy = 6
        ChanTblHead = 8
        ChanTblTail = 10


        BufferLng = 68                                  'Longs
        BufferSiz = BufferLng * 4                       'Bytes

        AckTblCnt = 100

        
VAR

'       Global

        long Global[3]
'       long Timer
'       word FreeChain, @CogTab, @ChanTab, @AckTbl


'       CogTab

        long CogTbl[CogTblCnt * CogTblLng]


'       ChanTab

        long ChanTbl[ChanTblCnt * ChanTblLng]


'       AckTab

        long AckTbl[AckTblCnt]

        
        long FreeSpace

 
Pub Start | i, j


' Initialize the RAM
    Repeat i from 0 to 2
      Global[i] := 0
    Global[1] := (@CogTbl << 16) | FreeSpace
    Global[2] := (@AckTbl << 16) | @ChanTbl

    Repeat i from 0 to ChanTblCnt * ChanTblLng - 1
      ChanTbl[i] := 0
    ChanTbl[00 * ChanTblLng + ChanTblMask] := 1 << Chan01Bit
    ChanTbl[01 * ChanTblLng + ChanTblMask] := 1 << Chan02Bit
    ChanTbl[02 * ChanTblLng + ChanTblMask] := 1 << Chan03Bit
    ChanTbl[03 * ChanTblLng + ChanTblMask] := 1 << Chan04Bit

    ChanTbl[04 * ChanTblLng + ChanTblMask] := 1 << Chan05Bit
    ChanTbl[05 * ChanTblLng + ChanTblMask] := 1 << Chan06Bit
    ChanTbl[06 * ChanTblLng + ChanTblMask] := 1 << Chan07Bit
    ChanTbl[07 * ChanTblLng + ChanTblMask] := 1 << Chan08Bit

    ChanTbl[08 * ChanTblLng + ChanTblMask] := 1 << Chan09Bit
    ChanTbl[09 * ChanTblLng + ChanTblMask] := 1 << Chan10Bit
    ChanTbl[10 * ChanTblLng + ChanTblMask] := 1 << Chan11Bit
    ChanTbl[11 * ChanTblLng + ChanTblMask] := 1 << Chan12Bit

    ChanTbl[12 * ChanTblLng + ChanTblMask] := 1 << Chan13Bit
    ChanTbl[13 * ChanTblLng + ChanTblMask] := 1 << Chan14Bit
    ChanTbl[14 * ChanTblLng + ChanTblMask] := 1 << Chan15Bit
    ChanTbl[15 * ChanTblLng + ChanTblMask] := 1 << Chan16Bit

    Repeat i from 0 to CogTblCnt * CogTblLng - 1
      CogTbl[i] := 0

    CogTbl[01 * CogTblLng + 0] := (@ChanTbl[00 * ChanTblLng] << 16) | @ChanTbl[01 * ChanTblLng]
    CogTbl[01 * CogTblLng + 1] := (@ChanTbl[02 * ChanTblLng] << 16) | @ChanTbl[03 * ChanTblLng]
    CogTbl[02 * CogTblLng + 0] := (@ChanTbl[04 * ChanTblLng] << 16) | @ChanTbl[05 * ChanTblLng]
    CogTbl[02 * CogTblLng + 1] := (@ChanTbl[06 * ChanTblLng] << 16) | @ChanTbl[07 * ChanTblLng]
    CogTbl[03 * CogTblLng + 0] := (@ChanTbl[08 * ChanTblLng] << 16) | @ChanTbl[09 * ChanTblLng]
    CogTbl[03 * CogTblLng + 1] := (@ChanTbl[10 * ChanTblLng] << 16) | @ChanTbl[11 * ChanTblLng]
    CogTbl[04 * CogTblLng + 0] := (@ChanTbl[12 * ChanTblLng] << 16) | @ChanTbl[13 * ChanTblLng]
    CogTbl[04 * CogTblLng + 1] := (@ChanTbl[14 * ChanTblLng] << 16) | @ChanTbl[15 * ChanTblLng]


' Start timer cog
    coginit(7, @TimerEntry, @Global)


' Start protocol cogs
    coginit(1 ,@ProtoEntry, @Global)

    coginit(2, @ProtoEntry, @Global)

    coginit(3, @ProtoEntry, @Global)
   
    coginit(4, @ProtoEntry, @Global)


' Start buffer handler      
    coginit(5, @BfrMgrEntry, @Global)

' Start DMA cog
    coginit(6, @USBEntry, @Global)

    
DAT

                        org     0
'
'
' Entry
'
USBEntry

                        mov     afreespace,par           'Get
                        add     afreespace,#GblFreeChain '. @'Freespace pointer

                        mov     acogtab,par             'Get
                        add     acogtab,#GblCogTbl      '. @'Cog
                        rdword  acogtab,acogtab         '. . table

                        mov     achantab,par            'Get
                        add     achantab,#GblChanTbl    '. @'Chan
                        rdword  achantab,achantab       '. . table

                        mov     aacktab,par             'Get
                        add     aacktab,#GblAckTbl      '. @'Ack
                        rdword  aacktab,aacktab         '. . table

                        mov     aacktail,aacktab        'Set tail address
                        add     aackend,aacktab         '. & compute end @
                        wrlong  azero,aacktail
                        
                        or      outa,usbrdmask          'Deassert
                        or      dira,usbrdmask          '. & set as output
                        
                        andn    outa,usbwrmask          'Deassert
                        or      dira,usbwrmask          '. & set as output

'
'       Wait for all protocol cogs to signal 'Ready'
'

WaitTilReady
                        mov     at2,acogtab             'Get                        
                        add     at2,#CogTblSiz          '. @'first PROTOCOL cog entry
                        mov     at3,#4                  '. & #'PROTOCOL cogs

:lup                      
:spin                   rdlong  at1,at2         wz      'Is this cog ready?
              if_nz     jmp     #:spin                  '. No - wait a little longer
              
                        add     at2,#CogTblSiz          'Bump to next cog
                        djnz    at3,#:lup               '. & continue 'til all ready

'
'       Everyone is ready - Let's go!!!
'

ReadyToGo
                        mov     at2,acogtab             'Finally we are ready to go...
                        add     at2,#CogTblSiz
                        mov     at1,cnt
                        add     at1,#40
                        wrlong  at1,at2
                        add     at1,#20
                        add     at2,#CogTblSiz
                        wrlong  at1,at2
                        add     at1,#20
                        add     at2,#CogTblSiz
                        wrlong  at1,at2
                        add     at1,#20
                        add     at2,#CogTblSiz
                        wrlong  at1,at2

'
'       At this point, all the protocol cogs should be running, waiting for data from the
'       USB.  This cog will now accept data from the USB port, and push it, buffer by buffer
'       to the channels
'
'       Receiver
'                        
USBLoop
                        mov     txmquehead,#txmquebgn
                        mov     txmquetail,#txmquebgn

                        mov     txmret,#txmtop
                                                
rcvtop
                        call    #rcvlong                'Wait
              if_c      jmp     #rcvtop                 '. for a command


:newcmd
                        mov     rcvfull,rcvdata         'Isolate
                        mov     rcvcmd,#$ff             '. the parts
                        rol     rcvdata,#8              '. . of the first long
                        and     rcvcmd,rcvdata          '
                        mov     rcvlng,#$ff             '
                        rol     rcvdata,#8              '
                        and     rcvlng,rcvdata          '
                        mov     rcvseq,#$ff             '
                        rol     rcvdata,#8              '
                        and     rcvseq,rcvdata          '

                        rol     rcvdata,#8              'Isolate
                        and     rcvdata,#$ff            '. chksum
                        xor     rcvdata,rcvcmd
                        xor     rcvdata,rcvlng
                        xor     rcvdata,rcvseq  wz      'Checksum valid?
              if_nz     jmp     #sndnack_chk             '. No - back checksum

                        mov     rcvwrk,rcvcmd           'Check the
                        sub     rcvwrk,#$7f             '. command
cmd80                   djnz    rcvwrk,#cmd81           'Not 80 - maybe 81?

                        jmp     #:snd
                        
:lup
                        call    #rcvlong                 'Get next word

:snd
                        movd    :sav,txmquehead

                        add     txmquehead,#1
                        cmp     txmquehead,#txmqueend wz 'Wrap?
              if_z      mov     txmquehead,#txmquebgn                                                

:sav                    mov     0-0,rcvdata

                        djnz    rcvlng,#:lup            'Loop
                        jmp     #rcvtop

cmd81                   djnz    rcvwrk,#cmd82            'Not 81 - maybe 82?

                        cmp     rcvlng,#2       wz      'L' valid?
              if_ne     jmp     #sndnack_lng            '. No - bad L'

                        call    #rcvlong                'Get time
                        wrlong  rcvdata,par             '. & stash it

                        jmp     #sndack                 'Send ACK & loop

cmd82                   djnz    rcvwrk,#sndnack_cmd     'Not 82 - command error!

                        cmp     rcvlng,#4       wc,wz   'L' valid?
              if_le     jmp     #sndnack_lng            '. No -
                        cmp     rcvlng,#66      wz,wc   'L' valid?
              if_gt     jmp     #sndnack_lng            '. No -

'
'       allocate a buffer
'
                        mov     rcvret,#:spin
                        
:spin                   lockset azero           wc
              if_c      jmp     #txmret

                        rdword  rcvbufr,afreespace wz   'load @'next buffer
              if_z      jmp     #:unlk

                        rdword  at1,rcvbufr             'unchain
                        
                        wrword  at1,afreespace          '. the buffer

:unlk
                        lockclr azero                   '. & clear the lock
                        
              if_z      jmp     #txmret                 'no buffer - try again
              
'
'       receive msg
'
                        mov     rcvptr,rcvbufr          'Copy @'              
                        mov     rcvdata,rcvfull         '. & restore data
                        jmp     #:stow
                        
:lup
                        call    #rcvlong                'Get another long
                        
:stow
                        add     rcvptr,#4               'Bump @'data area
                        wrlong  rcvdata,rcvptr          '. & write command there
                        djnz    rcvlng,#:lup            'Loop thru data

'
'       queue buffer on channel
'
                        mov     rcvchan,#12             '8th byte of message + links
                        add     rcvchan,rcvbufr
                        rdbyte  rcvchan,rcvchan
                        and     rcvchan,#$f
                        shl     rcvchan,#4              '16 bytes each
                        add     rcvchan,achantab

                        mov     rcvptr,#ChanTblTail
                        add     rcvptr,rcvchan
                        rdword  at1,rcvptr      wz

                        wrword  rcvbufr,rcvptr

                        sub     rcvptr,#ChanTblTail-ChanTblHead
                        
              if_nz     wrword  rcvbufr,at1
              if_z      wrword  rcvbufr,rcvptr                                                

sndack
                        mov     sndcmd,#$80
                        jmp     #sndxack
                        
sndnack_chk
                        mov     sndcmd,#$81
                        jmp     #sndxack

sndnack_cmd
                        mov     sndcmd,#$82
                        jmp     #sndxack

sndnack_lng
                        mov     sndcmd,#$83
'                        jmp     sndxack

sndxack
                        mov     snddata,rcvcmd

                        mov     sndchks,snddata
                        rol     snddata,#16
                        or      snddata,sndcmd
                        xor     sndchks,sndcmd
                        rol     snddata,#8
                        or      snddata,#1
                        xor     sndchks,#1

                        rol     snddata,#16
                        or      snddata,sndchks
                        
snd
                        movd    :sav,txmquehead

                        add     txmquehead,#1
                        cmp     txmquehead,#txmqueend wz   'Wrap?
              if_z      mov     txmquehead,#txmquebgn                                                

:sav                    mov     0-0,snddata

                        jmp     #rcvtop


'
'       Transmitter
'
txmtop
                        mov     txmret,#:tst

:tst                        
                        movs    :pick,txmquetail

                        rdlong  txmdata,aacktail        wz
              if_nz     jmp     #:zap
:pick                   mov     txmdata,0-0             wz
              if_z      jmp     rcvret
                        
                        movd    :clear,txmquetail

                        add     txmquetail,#1
                        cmp     txmquetail,#txmqueend   wz
              if_z      mov     txmquetail,#txmquebgn
              
:clear                  mov     0-0,azero                                                
                        jmp     #:send

:zap
                        wrlong  azero,aacktail
                        
                        add     aacktail,#4
                        cmp     aacktail,aackend        wz
              if_z      mov     aacktail,aacktab

:send
                        mov     txmret,#:byt1
                        
:byt1                   test    usbtxfmask,ina          wz
              if_z      jmp     rcvret

                        or      dira,#$ff
                        or      outa,txmdata
                        or      outa,usbwrmask
                        ror     txmdata,#8
                        andn    outa,usbwrmask
                        andn    dira,#$ff

                        mov     txmret,#:byt2
                        
:byt2                   test    usbtxfmask,ina          wz
              if_nz     jmp     rcvret

                        or      dira,#$ff
                        or      outa,txmdata
                        or      outa,usbwrmask
                        ror     txmdata,#8
                        andn    outa,usbwrmask
                        andn    dira,#$ff

                        mov     txmret,#:byt3
                        
:byt3                   test    usbtxfmask,ina          wz
              if_nz     jmp     rcvret

                        or      dira,#$ff
                        or      outa,txmdata
                        or      outa,usbwrmask
                        ror     txmdata,#8
                        andn    outa,usbwrmask
                        andn    dira,#$ff

                        mov     txmret,#:byt4
                        
:byt4                   test    usbtxfmask,ina          wz
              if_nz     jmp     rcvret

                        or      dira,#$ff
                        or      outa,txmdata
                        or      outa,usbwrmask
                        nop
                        andn    outa,usbwrmask
                        andn    dira,#$ff

                        jmp     #txmtop
                        


                        

rcvlong
                        mov     rcvtrys,#0              
                        mov     rcvret,#:byt1

:byt1                   test    usbrxemask,ina          wz
              if_nz     jmp     #rcvtmo

                        andn    outa,usbrdmask
                        nop
                        movi    rcvdata,ina
                        or      outa,usbrdmask
                        
                        mov     rcvret,#:byt2

:byt2                   test    usbrxemask,ina          wz
              if_nz     jmp     #rcvtmo

                        andn    outa,usbrdmask
                        shr     rcvdata,#8
                        movi    rcvdata,ina
                        or      outa,usbrdmask

                        mov     rcvret,#:byt3

:byt3                   test    usbrxemask,ina          wz
              if_nz     jmp     #rcvtmo

                        andn    outa,usbrdmask
                        shr     rcvdata,#8
                        movi    rcvdata,ina
                        or      outa,usbrdmask

                        mov     rcvret,#:byt4

:byt4                   test    usbrxemask,ina          wz
              if_nz     jmp     #rcvtmo

                        andn    outa,usbrdmask
                        shr     rcvdata,#7
                        shr     rcvdata,#1              wc
                        movi    rcvdata,ina             
                        or      outa,usbrdmask
                        rcl     rcvdata,#1
                        
rcvlong_ret             ret

rcvtmo
                        djnz    rcvtrys,txmret
                        test    $,#1                    wc                      'Set Carry
                        jmp     #rcvlong_ret                                     '. & return
                        

aacktab                 long    0
aacktail                long    0
aackend                 long    AckTblCnt * 4

               
txmret                  long    0
rcvret                  long    0
                        
rcvfull                 long    0
rcvdata                 long    0
rcvcmd                  long    0
rcvlng                  long    0
rcvseq                  long    0
rcvwrk                  long    0
rcvptr                  long    0
rcvbufr                 long    0
rcvtrys                 long    0
rcvchan                 long    0
                                
snddata                 long    0
sndcmd                  long    0
sndchks                 long    0

txmdata                 long    0

at1                     long    0
at2                     long    0
at3                     long    0
                                
achantab                long    0
acogtab                 long    0
afreespace              long    0
azero                   long    0
                                                                                 

USBRxEMask              long    1 << USBRxE
USBTxFMask              long    1 << USBTxF
USBRdMask               long    1 << USBRd
USBWrMask               long    1 << USBWr

txmquehead              long    0
txmquetail              long    0
txmquebgn               long    0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
                        long    0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
                        long    0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
txmqueend               long    0

                        fit     496

DAT
   
                        org     0
'
'
' Entry
'
BfrMgrEntry
                        mov     mfreespace,par          'Compute @'freespace
                        add     mfreespace,#GblFreeChain
                        
                        mov     mchantab,par            'Get
                        add     mchantab,#GblChanTbl    '. @'Chan
                        rdword  mchantab,mchantab       '. . table

                        mov     macktbl,par             'Get
                        add     macktbl,#GblAckTbl      '. @'Ack
                        rdword  macktbl,macktbl         '. . table

'
'       Build the free buffer chain
'

                        mov     mt1,mfreespace
                        rdword  mt3,mt1                 'Get @'available area
                        jmp     #:bgn
:lup
                        wrword  mt2,mt1
                        mov     mt1,mt2

:bgn
                        mov     mt2,mt3
                        add     mt3,#BufferSiz

                        test    mt3,mwordmask     wz
              if_z      jmp     #:lup

:dun
                        wrlong  mzero,mt2

'
'       Clear the Ack table
'                        
                        mov     mackend,macktbl
                        mov     mt1,#AckTblCnt

:clr
                        wrlong  mzero,mackend
                        add     mackend,#4
                        djnz    mt1,#:clr

                        mov     mackhead,macktbl
                        
'
'       Buffer pool ready to go - start processing
'

BufrTop
                        mov     mchancnt,#ChanTblCnt    'Get #'channels
                        mov     mchanptr,mchantab       ' . & @'first entry

:lup
                        mov     mptr,mchanptr
                        add     mptr,#ChanTblFree       'Get @'free pointers
                        rdword  mbufr,mptr      wz      ' . & load - 0?
              if_z      jmp     #:chkbusy               '.    . Yes - skip it

                        wrword  mzero,mptr              'Clear the pointer

'
'       Add port & port seq to Ack queue
'
                        mov     mt1,mbufr               'Load
                        add     mt1,#8                  '. port
                        rdlong  mt1,mt1                 '. . & port seq

                        mov     mt2,mt1                                         'Move port to chksum
                        ror     mt1,#8                                          '. & rotate

'
'       Return buffer to freechain
'
:spin1                  lockset mzero           wc      'Grab the lock - busy
              if_c      jmp     #:spin1                 '. Yes - spin


                        rdword  mt3,mfreespace          'Load @'next buffer

                        xor     mt2,mt1                                         'Add port seq to chksum
                        ror     mt1,#8                                          '. & rotate

                        wrword  mt3,mbufr               'Chain
                        
                        or      mt1,#$90                                        'Add resp type to chksum                        
                        xor     mt2,#$90                                        '. & msg

                        wrword  mbufr,mfreespace        '. the buffer

                        ror     mt1,#8                                          'Rotate one more time

                        lockclr mzero                   '. & clear the lock

                        and     mt2,#$ff                                        'Mask checksum
                        or      mt1,mt2                                         '. & add to msg

                        wrword  mt1,mackhead            'Write response long into ack queue

                        add     mackhead,#4             '. & increment it
                        cmp     mackhead,mackend wz     'Time to wrap?
              if_z      mov     mackhead,macktbl        '. Yes - do it...
              


:chkbusy
                        add     mptr,#ChanTblBusy-ChanTblFree 'Get @'busy buffer
                        rdword  mt1,mptr        wz      '. & load it - 0?
              if_nz     jmp     #:nxtchan               '.   . No - still busy - leave as is

                        add     mptr,#ChanTblHead-ChanTblBusy 'Get @'queue
                        rdword  mbufr,mptr      wz      '. & load it - 0?
              if_z      jmp     #:nxtchan               '.   . Yes - empty - skip it


                        add     mbufr,#4                'Load
                        rdlong  mt1,mbufr       wz      '. time from buffer - zero?
                        sub     mbufr,#4                ' (restore @'buffer)
              if_z      jmp     #:gotone                '. Yes - process the data

                        rdlong  mt2,par                 'Load current time
                        cmp     mt1,mt2         wz,wc   'Is it time for this data?
              if_a      jmp     #:nxtchan               '. No, don't busy it yet

:gotone
'
'       Remove buffer from channel queue
'
:spin2                  lockset mzero           wc      'Lock   
              if_c      jmp     #:spin2                  '. the cog (channel head/tail)

                        rdword  mt1,mbufr       wz      'Load link from buffer - zero?

                        wrword  mt1,mptr                'Update channel head

                        add     mptr,#ChanTblTail-ChanTblHead 'Compute @'tail
                                                                     
              if_z      wrword  mt1,mptr                'Update tail (if necessary)

                        lockclr mzero                   'Release

                        sub     mptr,#ChanTblTail-ChanTblBusy 'point back to Busy buffer
                        wrword  mbufr,mptr              'Busy the buffer

:nxtchan
                        add     mchanptr,#ChanTblSiz
                        djnz    mchancnt,#:lup
                        jmp     #BufrTop



mt1                     long    0
mt2                     long    0
mt3                     long    0

masis                   long    0
masis1                  long    0
masis2                  long    0
masis3                  long    0
masis4                  long    0
masis5                  long    0
masis6                  long    0

mwordmask               long    $08000

mchantab                long    0
mchanptr                long    0
mchancnt                long    0
macktbl                 long    0
mackhead                long    0
mackend                 long    0
mptr                    long    0
mbufr                   long    0
mfreespace              long    0

mzero                   long    0



                        fit     496


DAT
                       
                        org     0
'
'
' Entry
'
ProtoEntry
                        mov     bcogtab,par             'Get
                        add     bcogtab,#GblCogTbl      '. @'Cog
                        rdword  bcogtab,bcogtab         '. . table
                        
                        cogid   bt1                     'Cogid
                        shl     bt1,#3                  '. * 8
                        add     bcogtab,bt1             '. is index to cogtab

                        mov     bchantab,par            'Get
                        add     bchantab,#GblChanTbl    '. @'Chan
                        rdword  bchantab,bchantab       '. . table

                        mov     bt2,bcogtab             'Copy @'cog tbl
                        rdword  chan1chan,bt2           'Load Cog tbl entry
                        add     bt2,#2

                        rdlong  chan1mask,chan1chan     'Get first bit mask
                        
                        andn    outa,chan1mask          'Set to Zero
                        or      dira,chan1mask          'Set as output

                        mov     chan1busy,chan1chan     'Compute
                        add     chan1busy,#ChanTblBusy  ' @'busy buffer
                        mov     chan1free,chan1chan     'Compute
                        add     chan1free,#ChanTblFree  ' @'free buffer

                        rdword  chan2chan,bt2           'Load Cog tbl entry
                        add     bt2,#2

                        rdlong  chan2mask,chan2chan     'Get second bit mask
                        
                        andn    outa,chan2mask          'Set to Zero
                        or      dira,chan2mask          'Set as output
                        
                        mov     chan2busy,chan2chan     'Compute
                        add     chan2busy,#ChanTblBusy  ' @'busy buffer
                        mov     chan2free,chan2chan     'Compute
                        add     chan2free,#ChanTblFree  ' @'free buffer

                        rdword  chan3chan,bt2           'Load Cog tbl entry
                        add     bt2,#2

                        rdlong  chan3mask,chan3chan     'Get third bit mask
                        
                        andn    outa,chan3mask          'Set to Zero
                        or      dira,chan3mask          'Set as output
                        
                        mov     chan3busy,chan3chan     'Compute
                        add     chan3busy,#ChanTblBusy  ' @'busy buffer
                        mov     chan3free,chan3chan     'Compute
                        add     chan3free,#ChanTblFree  ' @'free buffer

                        rdword  chan4chan,bt2           'Load Cog tbl entry

                        rdlong  chan4mask,chan4chan     'Get fourth bit mask
                        
                        andn    outa,chan4mask          'Set to Zero
                        or      dira,chan4mask          'Set as output
                        
                        mov     chan4busy,chan4chan     'Compute
                        add     chan4busy,#ChanTblBusy  ' @'busy buffer
                        mov     chan4free,chan4chan     'Compute
                        add     chan4free,#ChanTblFree  ' @'free buffer

                        mov     chan1rtn,#chan1top      'Initialize co-routine                        
                        mov     chan2rtn,#chan2top      'Initialize co-routine
                        mov     chan3rtn,#chan3top      'Initialize co-routine                      
                        mov     chan4rtn,#chan4top      'Initialize co-routine

'
'       Ready-to-roll
'

                        wrlong  bzero,bcogtab           'Indicate we are ready to roll

:spin                   rdlong  btimbas,bcogtab wz      'Start?
              if_z      jmp     #:spin

                        wrlong  bzero,bcogtab           'Acknowledge the cogtab

                        waitcnt bzero,bzero

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

                        rdword  chan1ptr,chan1busy wz   'Load busy pointer - zero?   
              if_z      jmp     #chan1ret               '. Yes, == PAUSE ==

                        add     chan1ptr,#2             'Compute @'buffer length

                        rdbyte  chan1lng,chan1ptr       'Load long count

                        mov     chan1bfr,chan1ptr       'Restore
                        sub     chan1bfr,#2             '. @'buffer
                        
                        add     chan1ptr,#8-2-4         'Compute @'data - 4
                        sub     chan1lng,#2             'Compute #'data longs

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

                        rdword  chan2ptr,chan2busy wz   'Load busy pointer - zero?   
              if_z      jmp     #chan2ret               '. Yes, == PAUSE ==

                        add     chan2ptr,#2             'Compute @'buffer length

                        rdword  chan2lng,chan2ptr       'Load word count

                        mov     chan2bfr,chan2ptr       'Restore
                        sub     chan2bfr,#2             '. @'buffer

                        add     chan2ptr,#8-2-4         'Compute @'data - 4
                        sub     chan2lng,#1             'Compute #'data longs

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

                        rdword  chan3ptr,chan3busy wz   'Load busy pointer - zero?   
              if_z      jmp     #chan3ret               '. Yes, == PAUSE ==

                        add     chan3ptr,#2             'Compute @'buffer length

                        rdword  chan3lng,chan3ptr       'Load word count

                        mov     chan3bfr,chan3ptr       'Restore
                        sub     chan3bfr,#2             '. @'buffer

                        add     chan1ptr,#8-2-4         'Compute @'data - 4
                        sub     chan3lng,#1             'Compute #'data longs

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

                        rdword  chan4ptr,chan4busy wz   'Load busy pointer - zero?   
              if_z      jmp     #chan4ret               '. Yes, == PAUSE ==

                        add     chan4ptr,#2             'Compute @'buffer length

                        rdword  chan4lng,chan4ptr       'Load word count

                        mov     chan4bfr,chan4ptr       'Restore
                        sub     chan4bfr,#2             '. @'buffer

                        add     chan4ptr,#8-2-4         'Compute @'data - 4
                        sub     chan4lng,#1             'Compute #'data longs

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


bzero                   long    0

bt1                     long    0
bt2                     long    0

basis                   long    0
basis1                  long    0
basis2                  long    0
basis3                  long    0
basis4                  long    0
basis5                  long    0
basis6                  long    0

bcogtab                 long    0
bchantab                long    0

btimbas                 long    0


chan1chan               long    0
chan1busy               long    0
chan1free               long    0
chan1mask               long    0
chan1rtn                long    0
chan1data               long    0
chan1bits               long    0
chan1bfr                long    0
chan1lng                long    0
chan1ptr                long    0

chan2chan               long    0
chan2busy               long    0
chan2free               long    0
chan2mask               long    0
chan2rtn                long    0
chan2data               long    0
chan2bits               long    0
chan2bfr                long    0
chan2lng                long    0
chan2ptr                long    0

chan3chan               long    0
chan3busy               long    0
chan3free               long    0
chan3mask               long    0
chan3rtn                long    0
chan3data               long    0
chan3bits               long    0
chan3bfr                long    0
chan3lng                long    0
chan3ptr                long    0

chan4chan               long    0
chan4busy               long    0
chan4free               long    0
chan4mask               long    0
chan4rtn                long    0
chan4data               long    0
chan4bits               long    0
chan4bfr                long    0
chan4lng                long    0
chan4ptr                long    0


                        fit     496

              
DAT

                        org     0

TimerEntry
                        mov     timbas,cnt              'Adjust
                        add     timbas,#40              '. current time

:lup
                        waitcnt timbas,timint           'Wait for time, then add 1ms.

                        rdlong  t1,par                  'Load current time value

                        add     t1,#1                   'Increment the lo-order (ms) part
                        mov     t2,t1                   '. &
                        and     t2,tword                '.   isolate it
                        cmp     t2,t1000        wz      '1000ms?
              if_z      add     t1,tsecint              '. Yes, adjust time o next second
                        
                        wrlong  t1,par                  'Store current time value
                        
                        jmp     #:lup                   'Loop

                        

t1                      long    0
t2                      long    0
t1000                   long    1000
timint                  long    _CLK1MS
tword                   long    $0ffff
tsecint                 long    $10000-1000
timbas                  long    0


                        fit     496