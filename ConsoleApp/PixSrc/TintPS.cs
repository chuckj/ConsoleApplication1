﻿using SD = System.Drawing;

namespace ConsoleApplication1
{
    public class TintPS : IPixSrc
    {
        public IPixSrc Left { get; set; }
        public IPixSrc Right { get; set; }
        public void Init(PSData psdata)
        {

        }
        public SD.Color Get(PSData psdata) => Left.Get(psdata).Cross(Right.Get(psdata));
    }
}
