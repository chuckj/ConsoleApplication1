CON
        _CLKMODE = XTAL1 + PLL16X
        _CLKFREQ = 80_000_000

        _CLK1MS = _CLKFREQ / 1000

        ProtoCogs = 6
        
        DMAReqBit = 16
        CSBit = 17
        RS422EnaBit = 18
        ReadyBit = 19
        
        Chan1Bit = 20
        Chan2Bit = 21
        Chan3Bit = 22
        Chan4Bit = 23
        Chan5Bit = 24
        Chan6Bit = 25
        Chan7Bit = 26
        Chan8Bit = 27
        Chan9Bit = 28
        Chan10Bit = 29
        Chan11Bit = 30
        Chan12Bit = 31
        
VAR

'       global

        long Global[3]
'       long Timer
'       word FreeChain, CogTab, ChanTab


'       CogTab

        long CogTbl[8]


'       ChanTab

        long ChanTbl[16]

        long FreeSpace

 
Pub Start | i


' Initialize the Global common area
    Repeat i from 0 to 2
      Global[i] := 0
    Global[1] := @CogTbl << 16 | @FreeSpace
    Global[2] := @ChanTbl

    Repeat i from 0 to 15
      ChanTbl[i] := 0

    Repeat i from 0 to 7
      CogTbl[i] := 0


' Start timer cog
    coginit(7, @TimerEntry, @Global)


' Start Protocol Cogs      
    if (ProtoCogs > 0)  
      ChanTbl[0] := 1 << Chan1Bit
      ChanTbl[1] := 1 << Chan2Bit
      CogTbl[1] := (@ChanTbl[1] << 16) | @ChanTbl[0]
      coginit(1, @ProtoEntry, @Global)
      
    if (ProtoCogs > 1)  
      ChanTbl[2] := 1 << Chan3Bit
      ChanTbl[3] := 1 << Chan4Bit
      CogTbl[2] := (@ChanTbl[3] << 16) | @ChanTbl[2]
      coginit(2, @ProtoEntry, @Global)
      
    if (ProtoCogs > 2)  
      ChanTbl[4] := 1 << Chan5Bit
      ChanTbl[5] := 1 << Chan6Bit
      CogTbl[3] := (@ChanTbl[5] << 16) | @ChanTbl[4]
      coginit(3, @ProtoEntry, @Global)
      
    if (ProtoCogs > 3)  
      ChanTbl[6] := 1 << Chan7Bit
      ChanTbl[7] := 1 << Chan8Bit
      CogTbl[4] := (@ChanTbl[7] << 16) | @ChanTbl[6]
      coginit(4, @ProtoEntry, @Global)
      
    if (ProtoCogs > 4)  
      ChanTbl[8] := 1 << Chan9Bit
      ChanTbl[9] := 1 << Chan10Bit
      CogTbl[5] := (@ChanTbl[9] << 16) | @ChanTbl[8]
      coginit(5, @ProtoEntry, @Global)
      
    if (ProtoCogs > 5)  
      ChanTbl[10] := 1 << Chan11Bit
      ChanTbl[11] := 1 << Chan12Bit
      CogTbl[6] := (@ChanTbl[11] << 16) | @ChanTbl[10]
      coginit(6, @ProtoEntry, @Global)


' Start DMA cog
    coginit(0, @DMAEntry, @Global)
        
DAT

                        org     0
DMAEntry

'
'       Build buffer free chain
'
BuildBuffers
                        mov     at1,par                 'Copy @'free chain
                        add     at1,#4
                        mov     afreespace,at1          'Set @'free buffer chain

                        add     at1,#2                  'Get
                        rdword  acogtab,at1             '. @'cog table
                        add     at1,#2                  'Get
                        rdword  achantab,at1            '. @'Channel table

'
'       Build the free buffer chain
'

                        mov     at1,afreespace
                        rdword  at3,at1                 'Get @'available area
:lup
                        mov     at2,at3                 'Copy @'last buffer
                        add     at3,#68*4               'Compute @'next buffer

                        test    at3,cmdmask     wz      'Have we reached end-of-memory?
              if_nz     jmp     #:dun                   '. Yep, exit loop

                        wrword  at2,at1                 'Link to this buffer from last one
                        mov     at1,at2                 '. & make this buffer now the 'last one'
                        jmp     #:lup                   'Loop 'til the memory is all allocated

:dun
                        wrlong  azero,at2               'Null pointer in final buffer

                        mov     frqa,#1                 'Set frq for incrementing by 1
                        movs    ctra,CSBit              '. & mode
                        movi    ctra,#%01110            '.    (Neg edge detector)
                                       

'
'       Wait for all protocol cogs to signal 'Ready'
'

WaitTilReady
                        mov     at2,acogtab             'Get                        
                        add     at2,#4                  '. @'first protocol cog entry
                        mov     at3,#ProtoCogs          '. & #'protocol cogs

:lup                      
:spin                   rdlong  at1,at2         wz      'Is this cog ready?
              if_nz     jmp     #:spin                  '. No - wait a little longer
              
                        add     at2,#4                  'Bump to next cog
                        djnz    at3,#:lup               '. & continue 'til all ready

'
'       Everyone is ready - Let's go!!!
'

ReadyToGo
                        mov     at2,#ProtoCogs
                        cmp     at2,#5          wz,wc
              if_b      jmp     #:byp
                        or      outa,Rs422Enable        'Set to One
                        or      dira,Rs422Enable        'Set as output

:byp
                        andn    outa,dmareq             'Set to Zero
                        or      dira,dmareq             'Set as output
                        andn    outa,ready              'Set to Zero
                        or      dira,ready              'Set as output

                        mov     at2,acogtab             'Finally we are ready to go...
                        add     at2,#4
                        mov     at1,cnt
                        add     at1,#40
                        wrlong  at1,at2
                        add     at1,#23
                        add     at2,#4
                        wrlong  at1,at2
                        add     at1,#23
                        add     at2,#4
                        wrlong  at1,at2
                        add     at1,#23
                        add     at2,#4
                        wrlong  at1,at2
                        add     at1,#23
                        add     at2,#4
                        wrlong  at1,at2
                        add     at1,#23
                        add     at2,#4
                        wrlong  at1,at2
                        
'
'       At this point, all the protocol cogs should be running, enumerating their light
'       strings and waiting for data from the NetBurner.  This cog will now accept data
'       from the NetBurner, and push it, word by word, into channel buffers...
'
                        
DMALoop
:top
                        or      outa,ready              'Show we need a command
                        
                        call    #xferword               'Wait
          if_z_or_nc    jmp     #xferword                '. for a command


:newcmd
                        andn    outa,ready              'Show command was received
                        mov     cmd,at1                 '. & copy the command

'                        
'             get number of words to follow
'

                        call    #xferword               'Get count of words to follow
              if_z      jmp     #:top                   '. no response
              if_c      jmp     #:newcmd                '. another command

                        mov     lng,at1                 'Save #'words to follow

                        test    cmd,cuemask     wz      'Cue comand
              if_nz     jmp     #:cuecmd                '. Yes, process

                        cmp     lng,#4          wc,wz   'Valid?
              if_b      jmp     #:top                   '. No - flush the command
                        cmp     lng,#140        wc,wz   '.Valid?
              if_a      jmp     #:top                   '. No - flush the command

'
'       Looks like a valid message.  Allocate a buffer from the buffer pool
'

                        call    #allocbufr              'Grab a buffer from the free pool
                        
'
'       Now, clear the chain and put the command and length into the buffer.  Then grab the
'       remaining data
'

                        wrlong  azero,bfr               'Clear the link
                        
                        mov     at2,bfr                 'Compute
                        add     at2,#4                  '. the data pointer
                        wrword  cmd,at2                 'Save the command

                        add     at2,#2                  '. &
                        wrword  lng,at2                 '. the #'words of data
                       
:lup
                        call    #xferword               'Get nother word from the DMA
              if_z      jmp     #:errtop                '. no response
              if_c      jmp     #:errnewcmd             '. another command

                        add     at2,#2                  'Bump the pointer
                        wrword  at1,at2                 '. & save the new data word
                        
                        djnz    lng,#:lup               'Loop for all the data

'
'       All data has been moved.  Now, link the new buffer to the channel
'

                        mov     at2,cmd                 'Compute
                        ror     at2,#1                  '. the protocol cogid
                        add     at2,#1                  '. for the lock
                        
                        mov     at3,cmd                 'Compute
                        rol     at3,#2                  '. channel
                        and     at3,#$3c                '. offset
                        add     at3,achantab            '.

:spin
                        lockset at2             wc      'Lock the links
              if_c      jmp     #:spin

                        add     at3,#2                  'Get @'tail ptr
                        rdword  at1,at3         wz      'Load tail - 0?
                        
                        wrword  bfr,at3                 'Set new buffer as new tail

                        sub     at3,#2                  'Get @'head
              if_z      wrword  bfr,at3                 'If tail was 0, set new buffer in head
              if_nz     wrword  bfr,at1                 'Otherwise, set new buffer at previous tail

                        lockclr at2                     'Release the lock

                        jmp     #:top


'
'       Error handling - command seen in data area
'       return the buffer and process the command
'
:errtop
                        call    #retrnbufr
                        jmp     #:top

:errnewcmd
                        call    #retrnbufr
                        jmp     #:newcmd

'
'       Process Cue command
'                        

:cuecmd
                        cmp     lng,#2          wz      'Valid?
              if_z      jmp     #:top                   '. No - flush the command
                        
                        call    #xferword               'Get count of longs to follow
              if_z	jmp     #:top                   '. no response
              if_c      jmp     #:newcmd                '. another command

                        mov     at2,at1                 'Copy the low-order part (ms)
                        
                        call    #xferword               'Get count of longs to follow
              if_z      jmp     #:top                   '. no response
              if_c      jmp     #:newcmd                '. another command

                        ror     at1,#16                 'Combine
                        or      at1,at2                 '. w/ms
                        wrlong  at1,par                 '. & store for everyone to see
                        
                        jmp     #:top


'
'       Get a word from the DMA interface
'

xferword
                        cmp     cnt,delay       wz,wc
              if_b      jmp     #:dma
                        cmp     bfrptr,#bfrend  wz,wc
              if_a      jmp     #:dma

                        movs    :lod,bfrptr
                        add     bfrptr,#$1      wz      'Bump ptr and reset 'z' condition
:lod                    mov     at1,0-0
                        jmp     #:cmd
                        
:dma
                        mov     phsa,#0                 'Clear the phase reg (response detector)
                        xor     outa,dmareq             '. & send the DMA request
   
                        mov     at1,phsa        wz,nr   'Has response been seen?   
              if_nz     jmp     #:xfer                  '. Yes, grab the data                       
                        mov     at1,phsa        wz,nr   'Has response been seen?   
              if_nz     jmp     #:xfer                  '. Yes, grab the data                       
                        mov     at1,phsa        wz,nr   'Has response been seen?   
              if_nz     jmp     #:xfer                  '. Yes, grab the data                       
                        mov     at1,phsa        wz,nr   'Has response been seen?   
              if_nz     jmp     #:xfer                  '. Yes, grab the data                       
                        mov     at1,phsa        wz,nr   'Has response been seen?   
              if_nz     jmp     #:xfer                  '. Yes, grab the data                       
                        mov     at1,phsa        wz,nr   'Has response been seen?   
              if_nz     jmp     #:xfer                  '. Yes, grab the data                       
                        mov     at1,phsa        wz,nr   'Has response been seen?   

:xfer
                        mov     at1,ina

:cmd         
                        and     at1,wrdmask
                        test    at1,cmdmask     wc
                        
xferword_ret            ret


'
'       Allocate a buffer from the freespace chain
'

allocbufr
:spin                   lockset azero           wc
              if_c      jmp     :spin

                        rdword  bfr,afreespace  wz      'load @'next buffer
              if_z      jmp     #:unlk

                        rdword  at2,bfr                 'unchain
                        
                        wrword  at2,afreespace          '. the buffer

:unlk
                        lockclr azero                   '. & clear the lock

              if_z      jmp     :spin                   'no buffer - try again

allocbufr_ret           ret


'
'       Return a buffer to the freespace chain
'

retrnbufr
:spin                   lockset azero           wc
              if_c      jmp     :spin

                        rdword  at2,afreespace          'load @'next buffer

                        wrword  at2,bfr                 'unchain
                        
                        wrword  bfr,afreespace          '. the buffer

                        lockclr azero                   '. & clear the lock

retrnbufr_ret           ret





Rs422Enable   long    1 << Rs422EnaBit
dmareq        long    1 << DMAReqBit
ready         long    1 << ReadyBit
wrdmask       long    $0ffff
cmdmask       long    $08000
cuemask       long    $04000
at1           long    0
at2           long    0
at3           long    0
at4           long    0        
cmd           long    0
bfr           long    0
lng           long    0
achantab      long    0
acogtab       long    0
afreespace    long    0
azero         long    0

'
'       Testing
'
delay         long      10000
bfrptr        long      bfrbgn
bfrbgn        long      $8000
              long      4
              long      38
              long      0
              long      $0123
              long      $01cc

              long      $8001
              long      8
              long      40
              long      0
              long      $0fff
              long      $01cc
              long      $0fff
              long      $02cc
              long      $0fff
              long      $03cc

              long      $8000
              long      8
              long      42
              long      0
              long      $0fff
              long      $01cc
              long      $0fff
              long      $05cc
              long      $0fff
              long      $06cc
              
              long      $8001
              long      8
              long      42
              long      0
              long      $0fff
              long      $01cc
              long      $0fff
              long      $05cc
              long      $0fff
bfrend        long      $06cc


                        fit     496

DAT                        
                        org     0
ProtoEntry
                        cogid   bcogid
                        
                        mov     bt1,par                 'get structure address
                        add     bt1,#6                  'Get
                        rdword  bcogtab,bt1             '. @'cog table
                        
                        mov     bt2,bcogid              'Cogid
                        
                        add     bt1,#2                  'Get
                        rdword  bchantab,bt1            '. @'Chan table

                        rol     bt2,#2                  '. * 4
                        add     bcogtab,bt2             '. is index to cogtab

                        rdword  chan1chan,bcogtab       'Load @'first channel tab entry

                        mov     bfreespace,par                  'Compute                        
                        
                        add     bcogtab,#2              'Load                        
                        rdword  chan2chan,bcogtab       '. @'
                        sub     bcogtab,#2              '. second channel tab entry
                        
                        add     bfreespace,#4                   '. @'free space pointer

                        rdlong  chan1mask,chan1chan     'Get first bit mask
                        
                        andn    outa,chan1mask          'Set to Zero
                        or      dira,chan1mask          'Set as output
                        
                        rdlong  chan2mask,chan2chan     'Get second bit mask
                        
                        andn    outa,chan2mask          'Set to Zero
                        or      dira,chan2mask          'Set as output
                        
                        wrlong  bzero,chan1chan         'Clear to head/tail pointers

                        mov     chan1rtn,#chan1top      'Initialize
                        mov     chan1qrtn,#chan1qtop    '. the
                        
                        wrlong  bzero,chan2chan                 'Clear the head/tail pointers
                        
                        mov     chan2rtn,#chan2top      '. co-routine
                        mov     chan2qrtn,#chan2qtop    '. entry addresses

'
'       Ready-to-roll
'

                        wrlong  bzero,bcogtab           'Indicate we are ready to roll

:spin                   rdlong  btimbas,bcogtab wz      'Start?
              if_z      jmp     #:spin

'                       wrlong  bzero,bcogtab           'Acknowledge the cogtab

'                       waitcnt bzero,bzero             'Stop here - Testing

'
'       Main process loop
'

mainloop
                        waitcnt btimbas,#160            'Sync to clock
                        jmpret  btimret,chan1rtn        ' . & run channel 1
                        waitcnt btimbas,#160            'Sync to clock
                        jmpret  btimret,chan2rtn        ' . & run channel 2
                        jmp     #mainloop               'Loop forever

'
'       Channel 1 Protocol
'
'       This routine generates the data stream to the color effect string.
'       If time permits, it calls the channel1 Queue routine to check for/
'       process new data from the NetBurner
'
chan1top
                        jmpret  chan1ret,chan1qrtn      '== PROCESS CHANGES ==

chan1
                        jmpret  chan1rtn,btimret        '== PAUSE ==

                        tjnz    chan1flag1,#:fnd1       'Process
                        tjnz    chan1flag2,#:fnd2       '. pending changes (if any)
                        jmp     #chan1top               'No changes pending - loop

:fnd1
                        mov     bflag,chan1flag1        'Load flags
                        call    #findbit                '. & locate first 'set' bit
                        andn    chan1flag1,bflag        'Reset that bit
                        add     bitno,#chan1array       '. & compute @'data to xmit
                        jmp     #:xmit

:fnd2
                        mov     bflag,chan1flag2        'Load flags
                        call    #findbit                '. & locate first 'set'bit
                        andn    chan1flag2,bflag        'Reset that bit
                        add     bitno,#chan1array+32    '. & compute @'data to xmit
              
:xmit                         
                        movs    :load,bitno             'Set @'data into 'mov' instruction
                        mov     chan1bits,#27           '. & get #'bits in word
                                                
:load                   mov     chan1data,0-0           'Load data
                        rol     chan1data,#2            'Remove unused address bits
                        or      chan1data,#2            '. & set 'final' bit                        

:lup                        
                        jmpret  chan1rtn,btimret        '== PAUSE ==

                        or      outa,chan1mask          'Start bit                               

                        jmpret  chan1ret,chan1qrtn      '== PROCESS CHANGES ==
                        
                        jmpret  chan1rtn,btimret        '== PAUSE ==

                        andn    outa,chan1mask          'Intermediate

                        cmp     chan1bits,#13   wz      ' here we skip the nybble                           
              if_z      rol     chan1data,#4            ' . before the green data

                        jmpret  chan1ret,chan1qrtn      '== PROCESS CHANGES ==
                        
                        jmpret  chan1rtn,btimret        '== PAUSE ==
                                                   
                        rol     chan1data,#1    wc      'Data bit                        
              if_nc     or      outa,chan1mask

                        jmpret  chan1ret,chan1qrtn      '== PROCESS CHANGES ==
                        
                        djnz    chan1bits,#:lup         'Loop for all data bits

                        mov     chan1bits,#20           'Set count of 'quiet' bittimes
                                                        '. needed between commands

:idle                                               
                        jmpret  chan1rtn,btimret        '== PAUSE ==

                        jmpret  chan1ret,chan1qrtn      '== PROCESS CHANGES ==
                        
                        djnz    chan1bits,#:idle        'Loop for all 'quiet' bits
                        jmp     #chan1                  'Loop forever

'
'       Channel 1 Queue
'
'       This routine moves channel1 data coming from the NetBurner via cog 0 into our cog
'
chan1qtop
                        jmpret  chan1qrtn,chan1ret      '== PAUSE ==

                        rdword  chan1bfr,chan1chan wz   'Load head pointer - zero?   
              if_z      jmp     chan1ret                '. Yes, == PAUSE ==

                        add     chan1bfr,#8             'Load
                        rdlong  bt1,chan1bfr    wz      '. time from buffer - zero?
                        
                        sub     chan1bfr,#2                     'Compute @'word cout
                        
              if_z      jmp     #:gotone                '. Yes - process the data

                        rdlong  bt2,par                 'Load current time
                        cmp     bt1,bt2         wz,wc   'Is it time for this data?
              if_a      jmp     chan1ret                '. No, == PAUSE ==

:gotone
'                        waitcnt bzero,bzero

                        rdword  chan1lng,chan1bfr       'Load word count
                        
                        sub     chan1bfr,#6             'Restore @'buffer
                        mov     chan1ptr,chan1bfr       'Compute
                        add     chan1ptr,#8             '. @'data longs
                        
                        shr     chan1lng,#1             'Compute
                        sub     chan1lng,#1             '. #'data longs
                        
                        jmpret  chan1qrtn,chan1ret      '== PAUSE ==

:lup
                        add     chan1ptr,#4             'Load 
                        rdlong  bt1,chan1ptr            '. next data word
                        mov     bt2,bt1                 'Isolate
                        rol     bt2,#8                  '.
                        and     bt2,#$3f                '. lite number
                        
                        mov     bt3,bt2                 'Compute
                        add     bt3,#chan1array         '. @'data word
                        movd    :qcmp,bt3               'Stash in 'compare'
                        movd    :qupd,bt3               '. & again in 'mov'
:qcmp                   cmp     0-0,bt1         wz      'Has data changed?
              if_z      jmp     #:qbyp                  '. No, bypass the update
              
:qupd                   mov     0-0,bt1                 'Save the new data

                        mov     bt1,#1                  'Set
                        rol     bt1,bt2                 '.
                        test    bt2,#$20        wz      '.
              if_z      or      chan1flag1,bt1          '.
              if_nz     or      chan1flag2,bt1          '. the bit in the flag word

:qbyp
                        djnz    chan1lng,chan1ret       'Continue for all longs in data
                        
                        jmpret  chan1qrtn,chan1ret      '== PAUSE ==

chan1unchain
                        lockset bcogid          wc      'Lock   
              if_c      jmp     chan1ret                '. the cog (channel head/tail)

                        rdword  bt1,chan1bfr    wz      'Load link from buffer - zero?

                        mov     bt2,chan1chan           'Compute
                        add     bt2,#2                  '.  @'channel tail
                        
                        wrword  bt1,chan1chan           'Update channel head
                                                                     '
              if_z      wrword  bt1,bt2                 '. & tail (if necessary)

                        lockclr bcogid                  'Release the lock
                        
                        jmpret  chan1qrtn,chan1ret      '== PAUSE ==

chan1return
                        lockset bzero           wc      'Lock
              if_c      jmp     chan1ret                '. the free chain

                        rdword  bt2,bfreespace          'Load @'free buffer

                        wrword  bt2,chan1bfr            'Return the buffer
                        
                        wrword  chan1bfr,bfreespace     '. to the free chain

                        lockclr bzero                   'Release the lock

                        jmp     #chan1qtop


'
'       Channel 2 Protocol
'
'       This routine generates the data stream to the color effect string.
'       If time permits, it calls the channel2 Queue routine to check for/
'       process new data from the NetBurner
'
chan2top
                        jmpret  chan2ret,chan2qrtn      '== PROCESS CHANGES ==

chan2
                        jmpret  chan2rtn,btimret        '== PAUSE ==

                        tjnz    chan2flag1,#:fnd1       'Process
                        tjnz    chan2flag2,#:fnd2       '. pending changes (if any)
                        jmp     #chan2top               'No changes pending - loop

:fnd1
                        mov     bflag,chan2flag1        'Load flags
                        call    #findbit                '. & locate first 'set' bit
                        andn    chan2flag1,bflag        'Reset that bit
                        add     bitno,#chan2array       '. & compute @'data to xmit
                        jmp     #:xmit

:fnd2
                        mov     bflag,chan2flag2        'Load flags
                        call    #findbit                '. & locate first 'set'bit
                        andn    chan2flag2,bflag        'Reset that bit
                        add     bitno,#chan2array+32    '. & compute @'data to xmit
              
:xmit                         
                        movs    :load,bitno             'Set @'data into 'mov' instruction
                        mov     chan2bits,#27           '. & get #'bits in word
                                                
:load                   mov     chan2data,0-0           'Load data
                        rol     chan2data,#2            'Remove unused address bits
                        or      chan2data,#2            '. & set 'final' bit                        

:lup                        
                        jmpret  chan2rtn,btimret        '== PAUSE ==

                        or      outa,chan2mask          'Start bit                               

                        jmpret  chan2ret,chan2qrtn      '== PROCESS CHANGES ==
                        
                        jmpret  chan2rtn,btimret        '== PAUSE ==

                        andn    outa,chan2mask          'Intermediate

                        cmp     chan2bits,#13   wz      ' here we skip the nybble                           
              if_z      rol     chan2data,#4            ' . before the green data

                        jmpret  chan2ret,chan2qrtn      '== PROCESS CHANGES ==
                        
                        jmpret  chan2rtn,btimret        '== PAUSE ==
                                                   
                        rol     chan2data,#1    wc      'Data bit                        
              if_nc     or      outa,chan2mask

                        jmpret  chan2ret,chan2qrtn      '== PROCESS CHANGES ==
                        
                        djnz    chan2bits,#:lup         'Loop for all data bits

                        mov     chan2bits,#20           'Set count of 'quiet' bittimes
                                                        '. needed between commands

:idle                                               
                        jmpret  chan2rtn,btimret        '== PAUSE ==

                        jmpret  chan2ret,chan2qrtn      '== PROCESS CHANGES ==
                        
                        djnz    chan2bits,#:idle        'Loop for all 'quiet' bits
                        jmp     #chan2                  'Loop forever

'
'       Channel 2 Queue
'
'       This routine moves channel1 data coming from the NetBurner via cog 0 into our cog
'
chan2qtop
                        jmpret  chan2qrtn,chan2ret      '== PAUSE ==

                        rdword  chan2bfr,chan2chan wz   'Load head pointer - zero?   
              if_z      jmp     chan2ret                '. Yes, == PAUSE ==

                        add     chan2bfr,#8             'Load
                        rdlong  bt1,chan2bfr    wz      '. time from buffer - zero?

                        sub     chan2bfr,#2                     'Compute @'word cout
                        
              if_z      jmp     #:gotone                '. Yes - process the data

                        rdlong  bt2,par                 'Load current time
                        cmp     bt1,bt2         wz,wc   'Is it time for this data?
              if_a      jmp     chan2ret                '. No, == PAUSE ==

:gotone
                        rdword  chan2lng,chan2bfr       'Load word count
                        
                        sub     chan2bfr,#6             'Restore @'buffer
                        mov     chan2ptr,chan2bfr       'Compute
                        add     chan2ptr,#8             '. @'data longs
                        
                        shr     chan2lng,#1             'Compute
                        sub     chan2lng,#1             '. #'data longs
                        
                        jmpret  chan2qrtn,chan2ret      '== PAUSE ==

:lup
                        add     chan2ptr,#4             'Load 
                        rdlong  bt1,chan2ptr            '. next data word
                        mov     bt2,bt1                 'Isolate
                        rol     bt2,#8                  '.
                        and     bt2,#$3f                '. lite number
                        
                        mov     bt3,bt2                 'Compute
                        add     bt3,#chan2array         '. @'data word
                        movd    :qcmp,bt3               'Stash in 'compare'
                        movd    :qupd,bt3               '. & again in 'mov'
:qcmp                   cmp     0-0,bt1         wz      'Has data changed?
              if_z      jmp     #:qbyp                  '. No, bypass the update
              
:qupd                   mov     0-0,bt1                 'Save the new data

                        mov     bt1,#1                  'Set
                        rol     bt1,bt2                 '.
                        test    bt2,#$20        wz      '.
              if_z      or      chan2flag1,bt1          '.
              if_nz     or      chan2flag2,bt1          '. the bit in the flag word

:qbyp
                        djnz    chan2lng,chan2ret       'Continue for all longs in data
                        
                        jmpret  chan2qrtn,chan2ret      '== PAUSE ==

chan2unchain
                        lockset bcogid          wc      'Lock   
              if_c      jmp     chan2ret                '. the cog (channel head/tail)

                        rdword  bt1,chan2bfr    wz      'Load link from buffer - zero?

                        mov     bt2,chan2chan           'Compute
                        add     bt2,#2                  '.  @'channel tail
                        
                        wrword  bt1,chan2chan           'Update channel head
                                                                     '
              if_z      wrword  bt1,bt2                 '. & tail (if necessary)

                        lockclr bcogid                  'Release
                        
                        jmpret  chan2qrtn,chan2ret      '== PAUSE ==

chan2return
                        lockset bzero           wc      'Lock
              if_c      jmp     chan2ret                '. the free chain

                        rdword  bt2,bfreespace          'Load @'free buffer

                        wrword  bt2,chan2bfr            'Return the buffer
                        
                        wrword  chan2bfr,bfreespace     '. to the free chain

                        lockclr bzero                   'Release the lock

                        jmp     #chan2qtop



'
'       This routine locates the first 1 bit in a register.  This is used to locate
'       bits in the 'bflag' registers in channel1 and 2 which indicate lights whose
'       values have changed and, so, need to be updated.
'
'       19 instruction times (76 clocks)
'
findbit
                        mov     bitno,#0                'Assume bit 0                          
                        
                        test    bflag,findbit1  wz      'Is it bit 0-15?
              if_nz     and     bflag,findbit1          '. Yes, clear 16-31
              if_z      add     bitno,#16               '. No, bit must 16-31
              
                        test    bflag,findbit2  wz      'Is it bit 0-7 or 16-23
              if_nz     and     bflag,findbit2          '. Yes, clear 8-15, 24-31
              if_z      add     bitno,#8                '. No, bit must be 8-15, 24-31

                        test    bflag,findbit3  wz      'Is it bit 0-3, 8-11, 16-19, 24-27?
              if_nz     and     bflag,findbit3          '. Yes, clear 4-7, ...
              if_z      add     bitno,#4                '. No, bit must be 4-7...

                        test    bflag,findbit4  wz      'Is it bits 0-1 in each nybble
              if_nz     and     bflag,findbit4          '. Yes, clear bits 2-3 in each nybble
              if_z      add     bitno,#2                '. No, bit must be 2-3,...
                                                         
                        test    bflag,findbit5  wz      'Is it n even numbered bit? 
              if_nz     and     bflag,findbit5          '. Yes, clear odd numbered bits
              if_z      add     bitno,#1                '. No, bit must be odd
                                                        
                        mov     bflag,#1                'Generate
                        rol     bflag,bitno             '. bit mask for the found bit
                                
findbit_ret             ret                             'Retrun
                                            
bflag         long    0
bitno         long    0
findbit1      long    $0000ffff
findbit2      long    $00ff00ff
findbit3      long    $0f0f0f0f
findbit4      long    $33333333
findbit5      long    $55555555


bzero         long    0

'
'
' Uninitialized data
'
bt1           long    0
bt2           long    0
bt3           long    0

bcogid        long    0
bcogtab       long    0
bchantab      long    0
bfreespace    long    0
btimbas       long    0
btimret       long    0

'       Information for the first channel

chan1chan     long    0

chan1flag1    long    $ffffffff
chan1flag2    long    $7fffffff

chan1xfer     long    0
chan1mask     long    0
chan1rtn      long    0
chan1data     long    0
chan1bits     long    0

chan1bfr      long    0
chan1lng      long    0
chan1ptr      long    0
chan1ret      long    0
chan1qrtn     long    0


'       Information for the second channel

chan2chan     long    0

chan2flag1    long    $ffffffff
chan2flag2    long    $7fffffff

chan2xfer     long    0
chan2mask     long    0
chan2rtn      long    0
chan2data     long    0
chan2bits     long    0

chan2bfr      long    0
chan2lng      long    0
chan2ptr      long    0
chan2ret      long    0
chan2qrtn     long    0

'       First channel current data array

chan1array
              long     0*$1000000
              long     1*$1000000
              long     2*$1000000
              long     3*$1000000
              long     4*$1000000
              long     5*$1000000
              long     6*$1000000
              long     7*$1000000
              long     8*$1000000
              long     9*$1000000
              long     10*$1000000
              long     11*$1000000
              long     12*$1000000
              long     13*$1000000
              long     14*$1000000
              long     15*$1000000
              long     16*$1000000
              long     17*$1000000
              long     18*$1000000
              long     19*$1000000
              long     20*$1000000
              long     21*$1000000
              long     22*$1000000
              long     23*$1000000
              long     24*$1000000
              long     25*$1000000
              long     26*$1000000
              long     27*$1000000
              long     28*$1000000
              long     29*$1000000
              long     30*$1000000
              long     31*$1000000
              long     32*$1000000
              long     33*$1000000
              long     34*$1000000
              long     35*$1000000
              long     36*$1000000
              long     37*$1000000
              long     38*$1000000
              long     39*$1000000
              long     40*$1000000
              long     41*$1000000
              long     42*$1000000
              long     43*$1000000
              long     44*$1000000
              long     45*$1000000
              long     46*$1000000
              long     47*$1000000
              long     48*$1000000
              long     49*$1000000
              long     50*$1000000
              long     51*$1000000
              long     52*$1000000
              long     53*$1000000
              long     54*$1000000
              long     55*$1000000
              long     56*$1000000
              long     57*$1000000
              long     58*$1000000
              long     59*$1000000
              long     60*$1000000
              long     61*$1000000
              long     62*$1000000
              long     63*$1000000

'       Second channel current data array

chan2array
              long     0*$1000000
              long     1*$1000000
              long     2*$1000000
              long     3*$1000000
              long     4*$1000000
              long     5*$1000000
              long     6*$1000000
              long     7*$1000000
              long     8*$1000000
              long     9*$1000000
              long     10*$1000000
              long     11*$1000000
              long     12*$1000000
              long     13*$1000000
              long     14*$1000000
              long     15*$1000000
              long     16*$1000000
              long     17*$1000000
              long     18*$1000000
              long     19*$1000000
              long     20*$1000000
              long     21*$1000000
              long     22*$1000000
              long     23*$1000000
              long     24*$1000000
              long     25*$1000000
              long     26*$1000000
              long     27*$1000000
              long     28*$1000000
              long     29*$1000000
              long     30*$1000000
              long     31*$1000000
              long     32*$1000000
              long     33*$1000000
              long     34*$1000000
              long     35*$1000000
              long     36*$1000000
              long     37*$1000000
              long     38*$1000000
              long     39*$1000000
              long     40*$1000000
              long     41*$1000000
              long     42*$1000000
              long     43*$1000000
              long     44*$1000000
              long     45*$1000000
              long     46*$1000000
              long     47*$1000000
              long     48*$1000000
              long     49*$1000000
              long     50*$1000000
              long     51*$1000000
              long     52*$1000000
              long     53*$1000000
              long     54*$1000000
              long     55*$1000000
              long     56*$1000000
              long     57*$1000000
              long     58*$1000000
              long     59*$1000000
              long     60*$1000000
              long     61*$1000000
              long     62*$1000000
              long     63*$1000000

              fit       496
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
              if_z      add     t1,tsecint              '. Yes, adjust time to next second
                        
                        wrlong  t1,par                  'Store current time value
                        
                        jmp     #:lup                   'Loop

                        

t1            long      0
t2            long      0
t1000         long      1000
timint        long      _CLK1MS
tword         long      $0ffff
tsecint       long      $10000-1000
timbas        long      0


              fit       496