using SharpDX;

namespace ConsoleApplication1
{
    public struct IndexVertex
    {
        public Vector3 Position;

        /// <summary>
        /// Textcoord
        /// </summary>
        public uint Texcoord;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="position">Position XYZ</param>
        /// <param name="texcoord">Color index</param>
        public IndexVertex(Vector3 position, uint texcoord)
        {
            Position = position;
            Texcoord = texcoord;
        }
    }


    public struct WVPAndR
    {
        Matrix WVP;
        Matrix R;

        public WVPAndR(Matrix wvp, Matrix r)
        {
            WVP = wvp;
            R = r;
        }
    }

    public struct MinMax
    {
        public int min;
        public int max;

        public MinMax(int min,int max)
        {
            this.min = min;
            this.max = max;
        }
    }
}
