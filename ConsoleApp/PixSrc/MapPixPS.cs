using SD = System.Drawing;

namespace ConsoleApplication1
{
    public class MapPixPS : IPixSrc
    {
        public byte[,] Map { get; set; }
        public IPixSrc Src { get; set; }
        public SD.Point BeginOffset { get; set; }
        public SD.Point FinalOffset { get; set; }
        private SD.Point diff { get; set; }
        public void Init(PSData psdata)
        {
            Src.Init(psdata);
        }
        public SD.Color Get(PSData psdata)
        {
            int x = BeginOffset.X + (int)(diff.X * psdata.Factor);
            int y = BeginOffset.Y + (int)(diff.Y * psdata.Factor);
            psdata.Index = Map[x, y];
            return SD.Color.Black;
        }
    }
}
