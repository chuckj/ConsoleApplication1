using SD = System.Drawing;

namespace ConsoleApplication1
{
    public interface IPixSrc
    {
        void Init(PSData psdata);
        SD.Color Get(PSData psdata);
    }
}
