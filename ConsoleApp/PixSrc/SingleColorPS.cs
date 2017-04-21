using SD = System.Drawing;


namespace ConsoleApplication1
{
    public class SingleColorPS : IPixSrc
    {
        public SD.Color Color { get; set; }

        public void Init(PSData psdata)
        {

        }
        public SD.Color Get(PSData PSD)
        {
            return Color;
        }
    }
}
