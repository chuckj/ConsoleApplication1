
CON
        _CLKMODE = XTAL1 + PLL16X
        _CLKFREQ = 80_000_000
        
VAR

        long Cog1bgn, Chan1bit, Chan1add, Chan2bit, Chan2add
        long Cog2bgn, Chan3bit, Chan3add, Chan4bit, Chan4add
        long Cog3bgn, Chan5bit, Chan5add, Chan6bit, Chan6add
        long Cog4bgn, Chan7bit, Chan7add, Chan8bit, Chan8add
        long Cog5bgn, Chan9bit, Chan9add, Chan10bit, Chan10add
        long Cog6bgn, Chan11bit, Chan11add, Chan12bit, Chan12add
        long Cog7bgn, Chan13bit, Chan13add, Chan14bit, Chan14add

        long Chan1xfr[64]
        long Chan2xfr[64]
        long Chan3xfr[64]
        long Chan4xfr[64]
        long Chan5xfr[64]
        long Chan6xfr[64]
        long Chan7xfr[64]
        long Chan8xfr[64]
        long Chan9xfr[64]
        long Chan10xfr[64]
        long Chan11xfr[64]
        long Chan12xfr[64]
        long Chan13xfr[64]
        long Chan14xfr[64]
        
        
Pub Start
    Cog1bgn := -1
    Chan1bit := 31
    Chan2bit := 24
    Chan1add := @Chan1xfr
    Chan2add := @Chan2xfr
    
    cognew(@ChanEntry, @Cog1bgn)

    Cog2bgn := -1
    Chan3bit := 30
    Chan4bit := 23
    Chan3add := @Chan3xfr
    Chan4add := @Chan4xfr
    
    cognew(@ChanEntry, @Cog2bgn)

    Cog3bgn := -1
    Chan5bit := 29
    Chan6bit := 22
    Chan5add := @Chan5xfr
    Chan6add := @Chan6xfr
    
    cognew(@ChanEntry, @Cog3bgn)

    Cog4bgn := -1
    Chan7bit := 28
    Chan8bit := 21
    Chan7add := @Chan7xfr
    Chan8add := @Chan8xfr
    
    cognew(@ChanEntry, @Cog4bgn)

    Cog5bgn := -1
    Chan9bit := 27
    Chan10bit := 20
    Chan9add := @Chan9xfr
    Chan10add := @Chan10xfr
    
    cognew(@ChanEntry, @Cog5bgn)

    Cog6bgn := -1
    Chan11bit := 26
    Chan12bit := 19
    Chan11add := @Chan11xfr
    Chan12add := @Chan12xfr
    
    cognew(@ChanEntry, @Cog6bgn)

    Cog7bgn := -1
    Chan13bit := 25
    Chan14bit := 18
    Chan13add := @Chan13xfr
    Chan14add := @Chan14xfr
    
    cognew(@ChanEntry, @Cog7bgn)
 
    coginit(0, @BusEntry, @Cog1bgn)
        
DAT

'***********************************
'*     Assembly language code      *
'***********************************
                        org     0
'
'
' Entry
'
BusEntry


                        mov     at2,par                 'wait for all protocol cogs
                        mov     at3,#7
waitlup                 
                        rdlong  at1,at2    wz
              if_nz     jmp     #waitlup
                        add     at2,#20
                        djnz    at3,#waitlup

                        mov     at2,par                 'Finally we are ready to go...
                        mov     at1,cnt
                        add     at1,#40
                        wrlong  at1,at2
                        add     at1,#23
                        add     at2,#20
                        wrlong  at1,at2
                        add     at1,#23
                        add     at2,#20
                        wrlong  at1,at2
                        add     at1,#23
                        add     at2,#20
                        wrlong  at1,at2
                        add     at1,#23
                        add     at2,#20
                        wrlong  at1,at2
                        add     at1,#23
                        add     at2,#20
                        wrlong  at1,at2
                        add     at1,#23
                        add     at2,#20
                        wrlong  at1,at2
                        
dun                     jmp     #dun

                        or      dira,dmareq            'Set as output

top
                        call    xferword
              if_nz     jmp     #top

                        and     at1,#$08000     wz
              if_z      jmp     #top

                        mov     cmd,at1

                        call    #xferword
              
'             Compute data stream

'             get number of longs to follow

loop
                        xor     outa,dmareq
                        waitpne chipsel,chipsel
                        mov     at1,ina
                        and     at1,mask
                        ror     at1,#16
                        
                        xor     outa,dmareq
                        waitpne chipsel,chipsel
                        mov     at2,ina
                        and     at2,mask
                        or      at1,at2

'             store into buffer

                        djnz    count,#loop

                        mov     buffer,count


                        jmp     #top

                        
xferword
                        xor     outa,dmareq
                        mov     at1,ina         wc
              if_nc     jmp     :xfer
                        mov     at1,ina         wc
              if_nc     jmp     :xfer
                        mov     at1,ina         wc
              if_nc     jmp     :xfer
                        mov     at1,ina         wc
              if_nc     jmp     :xfer
                        mov     at1,ina         wc
              if_nc     jmp     :xfer
                        mov     at1,ina         wc
              if_nc     jmp     :xfer
                        mov     at1,ina         wc
              if_nc     jmp     :xfer
                        mov     at1,ina         wc
              if_nc     jmp     :xfer
                        mov     at1,ina         wc
              if_nc     jmp     :xfer
                        jmp     xferword_ret
                        
:xfer
                        and     at1,mask
                        test    at1,#$08000     wz
                        
xferword_ret            ret


xferlong
                        call    #xferword       ' carry - no response, zero - not a command 
              
              if_z      mov     at2,at1
              if_z      ror     at2,#16
                        
              if_z      call    #xferword

              if_z      or      at1,at2
                        
xferlong_ret            ret


                        
dmareq                  long    $40000000
mask                    long    $0ffff
at1                     long    0
at2                     long    0
at3                     long    0
count                   long    0
maskcs                  long    0
buffer                  long    0


                        fit     496

DAT                        
                        org     0
'
'
' Entry
'
ChanEntry               mov     t1,par                'get structure address
                        add     t1,#4
                        
                        rdlong  t2,t1
                        mov     chan1mask,#1              'Get
                        shl     chan1mask,t2              '. first bit mask

                        add     t1,#4
                        rdlong  chan1xfer,t1

                        add     t1,#4
                        rdlong  t2,t1
                        mov     chan2mask,#1              'Get
                        shl     chan2mask,t2              '. second bit mask

                        add     t1,#4
                        rdlong  chan2xfer,t1

                        andn    outa,chan1mask            'Set to Zero
                        andn    outa,chan2mask            'Set to Zero
                        or      dira,chan1mask            'Set as output
                        or      dira,chan2mask            '.

'
'       Main process loop
'

                        mov     chan1rtn,#chan1top      'Initialize
                        mov     chan1qrtn,#chan1qtop    ' . the
                        mov     chan2rtn,#chan2top      ' . . co-routine
                        mov     chan2qrtn,#chan2qtop    ' . . . entry addresses

                        wrlong  zero,par                'Indicate we are ready to roll

ready
                        rdlong  timbas,par      wz      'Start?
              if_z      jmp     #ready
              
mainloop
                        waitcnt timbas,#160             'Sync to clock
                        jmpret  timret,chan1rtn         ' . & run channel 1
                        waitcnt timbas,#160             'Sync to clock
                        jmpret  timret,chan2rtn         ' . & run channel 2
                        jmp     #mainloop               'Loop forever

'
'       Channel 1 Protocol
'
'       This routine generates the data stream to the color effect string.
'       If time permits, it calls the channel1 Queue routine to check for
'       new data from the NetBurner
'
chan1top
                        jmpret  chan1ret,chan1qrtn

chan1
                        jmpret  chan1rtn,timret
                        
                        tjnz    chan1flag1,#chan1fnd1
                        tjnz    chan1flag2,#chan1fnd2
                        jmp     #chan1top
              
chan1fnd1
                        mov     flag,chan1flag1
                        call    #findbit
                        andn    chan1flag1,flag
                        add     bitno,#chan1array
                        jmp     #chan1xmit

chan1fnd2
                        mov     flag,chan1flag2
                        call    #findbit
                        andn    chan1flag2,flag
                        add     bitno,#chan1array+32
              
chan1xmit                         
                        movs    chan1load,bitno
                        mov     chan1bits,#27
                                                
chan1load               mov     chan1data,0-0
                        rol     chan1data,#2
                        or      chan1data,#2

chan1lup                        
                        jmpret  chan1rtn,timret

                        or      outa,chan1mask          'Start bit                               

                        jmpret  chan1ret,chan1qrtn
                        
                        jmpret  chan1rtn,timret

                        andn    outa,chan1mask          'Intermediate

                        cmp     chan1bits,#13           wz    ' here we skip the nybble                     
              if_z      rol     chan1data,#4                  ' . before the green data

                        jmpret  chan1ret,chan1qrtn
                        
                        jmpret  chan1rtn,timret
                                                   
                        rol     chan1data,#1            wc    'Data bit                  
              if_nc     or      outa,chan1mask

                        jmpret  chan1ret,chan1qrtn
                        
                        djnz    chan1bits,#chan1lup     'Loop for all data bits

                        mov     chan1bits,#20           'Set count of 'quiet' bittimes
                                                        '. needed between commands

chan1idl                                               
                        jmpret  chan1rtn,timret

                        jmpret  chan1ret,chan1qrtn
                        
                        djnz    chan1bits,#chan1idl     'Loop for all 'quiet' bits
                        jmp     #chan1                  'Loop forever

'
'       Channel 1 Queue
'
'       This routine moves channel1 data coming from the NetBurner via cog 0 into our cog
'
chan1qtop
                        jmpret  chan1qrtn,chan1ret
          
                        rdlong  chan1qcntr,chan1xfer            wz
              if_z      jmp     chan1ret

                        mov     chan1qaddr,chan1xfer
                        wrlong  zero,chan1xfer

                        jmpret  chan1qrtn,chan1ret

                        add     chan1qaddr,#4
                        rdlong  t1,chan1qaddr
                        mov     t2,t1
                        rol     t2,#8
                        and     t2,#$3f
                        
                        mov     t3,t2
                        add     t3,#chan1array
                        movd    chan1qcmp,t3
                        movd    chan1qupd,t3
chan1qcmp               cmp     0-0,t1         wz
              if_z      jmp     #chan1qbyp
              
chan1qupd               mov     0-0,t1

                        mov     t1,#1
                        rol     t1,t2
                        test    t2,#$20         wz
              if_z      or      chan1flag1,t1
              if_nz     or      chan1flag2,t1

chan1qbyp
                        djnz    chan1qcntr,chan1ret
                        jmp     #chan1qtop


'
'       Channel 2 Protocol
'
'       This routine generates the data stream to the color effect string.
'       If time permits, it calls the channel2 Queue routine to check for
'       new data from the NetBurner
'
chan2top
                        jmpret  chan2ret,chan2qrtn

chan2
                        jmpret  chan2rtn,timret
                        
                        tjnz    chan2flag1,#chan2fnd1
                        tjnz    chan2flag2,#chan2fnd2
                        jmp     #chan2top

chan2fnd1
                        mov     flag,chan2flag1
                        call    #findbit
                        andn    chan2flag1,flag
                        add     bitno,#chan2array
                        jmp     #chan2xmit

chan2fnd2
                        mov     flag,chan2flag2
                        call    #findbit
                        andn    chan2flag2,flag
                        add     bitno,#chan2array+32

chan2xmit
                        movs    chan2load,bitno
                        mov     chan2bits,#27
                       
chan2load               mov     chan2data,0-0
                        rol     chan2data,#2
                        or      chan2data,#2

chan2lup                        
                        jmpret  chan2rtn,timret

                        or      outa,chan2mask

                        jmpret  chan2ret,chan2qrtn
                        
                        jmpret  chan2rtn,timret

                        andn    outa,chan2mask

                        cmp     chan2bits,#13           wz
              if_z      rol     chan2data,#4

                        jmpret  chan2ret,chan2qrtn
                        
                        jmpret  chan2rtn,timret

                        rol     chan2data,#1            wc
              if_nc     or      outa,chan2mask

                        jmpret  chan2ret,chan2qrtn
                        
                        djnz    chan2bits,#chan2lup

                        mov     chan2bits,#20

chan2idl                                               
                        jmpret  chan2rtn,timret

                        jmpret  chan2ret,chan2qrtn
                        
                        djnz    chan2bits,#chan2idl
                        jmp     #chan2
                        

'
'       Channel 2 Queue
'
'       This routine moves channel2 data coming from the NetBurner via cog 0 into our cog
'
chan2qtop
                        jmpret  chan2qrtn,chan2ret
          
                        rdlong  chan2qcntr,chan2xfer            wz
              if_z      jmp     chan2ret

                        mov     chan2qaddr,chan2xfer
                        wrlong  zero,chan2xfer

                        jmpret  chan2qrtn,chan2ret

                        add     chan2qaddr,#4
                        rdlong  t1,chan2qaddr
                        mov     t2,t1
                        rol     t2,#8
                        and     t2,#$3f
                        
                        mov     t3,t2
                        add     t3,#chan2array
                        movd    chan2qcmp,t3
                        movd    chan2qupd,t3
chan2qcmp               cmp     0-0,t1          wz
              if_z      jmp     #chan2qbyp
              
chan2qupd               mov     0-0,t1

                        mov     t1,#1
                        rol     t1,t2
                        test    t2,#$20         wz
              if_z      or      chan2flag1,t1
              if_nz     or      chan2flag2,t1
              
chan2qbyp
                        djnz    chan2qcntr,chan2ret
                        jmp     #chan2qtop


'
'       This routine locates the first 1 bit in a register.  This is used to locate
'       bits in the 'flag' registers in channel1 and 2 which indicate lights whose
'       values have changed and, so, need to be updated.
'
'       19 instructions
'
findbit
                        mov     bitno,#0
                        
                        test    flag,findbit1           wz
              if_nz     and     flag,findbit1
              if_z      add     bitno,#16
              
                        test    flag,findbit2           wz
              if_nz     and     flag,findbit2
              if_z      add     bitno,#8

                        test    flag,findbit3           wz
              if_nz     and     flag,findbit3
              if_z      add     bitno,#4

                        test    flag,findbit4           wz
              if_nz     and     flag,findbit4
              if_z      add     bitno,#2

                        test    flag,findbit5           wz
              if_nz     and     flag,findbit5
              if_z      add     bitno,#1

                        mov     flag,#1
                        rol     flag,bitno
                        
findbit_ret             ret
                                            
findbit1                long    $0000ffff
findbit2                long    $00ff00ff
findbit3                long    $0f0f0f0f
findbit4                long    $33333333
findbit5                long    $55555555


zero                    long    0
minus1                  long    -1

'
'
' Uninitialized data
'
t1                      long    0
t2                      long    0
t3                      long    0

basis                   long    0
basis1                  long    0
basis2                  long    0
basis3                  long    0
basis4                  long    0
basis5                  long    0
basis6                  long    0
timbas                  long    0
timret                  long    0
flag                    long    0
bitno                   long    0

chan1flag1              long    -1
chan1flag2              long    0

chan1xfer               long    0
chan1mask               long    0
chan1rtn                long    0
chan1data               long    0
chan1bits               long    0

chan1qcntr              long    0
chan1qaddr              long    0
chan1ret                long    0
chan1qrtn               long    0

chan2flag1              long    -1
chan2flag2              long    0

chan2xfer               long    0
chan2mask               long    0
chan2rtn                long    0
chan2data               long    0
chan2bits               long    0

chan2qcntr              long    0
chan2qaddr              long    0
chan2ret                long    0
chan2qrtn               long    0

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