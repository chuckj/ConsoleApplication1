using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Windows.Forms;
//using System.Drawing;
using SharpHelper;
using SharpDX.Windows;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

using Buffer11 = SharpDX.Direct3D11.Buffer;
using System.Xml.Linq;
using System.Runtime.InteropServices;

namespace ConsoleApplication1
{
    class Program
    {

        enum ColorEnum { Red, Yellow, Green, Blue };
        static Color[] Colors = new[] { Color.Red, Color.Yellow, Color.Green, Color.Blue };
        static void Main(string[] args)
        {
            //int? i = 10;
            //Console.WriteLine(i.GetType()); // Displays 
            //Type type = TypedReference.GetTargetType(__makeref(i));
            //Console.WriteLine((type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)).ToString());
            //Console.ReadLine();
            //return;

            //TreeTest.Run();
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //using (var frm = new Form1())
            //{
            //    Application.Run(frm);
            //}



            if (!SharpDevice.IsDirectX11Supported())
            {
                System.Windows.Forms.MessageBox.Show("DirectX11 Not Supported");
                return;
            }

            //render form
            RenderForm form = new RenderForm();
            form.Text = "Tutorial 3: Font";
            int count = 0;


            Global.doc = XDocument.Load(@".\\..\\..\\XMLfile1.xml");

            Global.dta = Global.doc.Descendants("lites").Descendants("lite").Select(n => new TreeData()
            {
                row = (int)n.Attribute("row"),
                ctr = (int)n.Attribute("cir"),
                col = (int)n.Attribute("col"),
                ndx = (int)n.Attribute("ndx"),
                color = Colors[(int)(ColorEnum)Enum.Parse(typeof(ColorEnum), (string)n.Attribute("color"))],
            }).OrderBy(t => t.ndx).ToArray();

            Global.dict = Global.dta.ToDictionary(d => Tuple.Create<int, int>(d.row, d.ctr), d => d);



            int[] indices = Enumerable.Range(0, Global.dta.Count()).ToArray();

            ColoredVertex[] vertices =
                Global.dta.Select(x => new ColoredVertex(new Vector3((x.ctr), -(x.row - 16) * 2, -5), 
                new Vector4(x.color.R / 255.0f, x.color.G / 255.0f, x.color.B / 255.0f, 1)
                )).ToArray();

            using (SharpDevice device = new SharpDevice(form))

            //Init Font
            using (SharpBatch font = new SharpBatch(device, "../../textfont.dds"))

            //Init Mesh
            using (SharpMesh mesh = SharpMesh.Create<ColoredVertex>(device, vertices, indices))

            //Create Shader From File and Create Input Layout
            using (SharpShader shader = new SharpShader(device, "../../HLSL.txt",
                    new SharpShaderDescription() { VertexShaderFunction = "VS", PixelShaderFunction = "PS", GeometryShaderFunction = "GS" },
                    new InputElement[] {
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 12, 0)
                    }))

            //create constant buffer
            using (Buffer11 buffer = shader.CreateBuffer<Matrix>())
            {
                SharpFPS fpsCounter = new SharpFPS();
                fpsCounter.Reset();

                //main loop
                RenderLoop.Run(form, () =>
                {
                    //resize if form was resized
                    if (device.MustResize)
                    {
                        device.Resize();
                        font.Resize();
                    }

                    //apply states
                    device.UpdateAllStates();

                    //clear color
                    device.Clear(Color.Black);

                    //apply shader
                    shader.Apply();


                    //apply constant buffer to shader
                    device.DeviceContext.GeometryShader.SetConstantBuffer(0, buffer);

                    //Set matrices
                    float ratio = (float)form.ClientRectangle.Width / (float)form.ClientRectangle.Height;
                    Matrix projection = Matrix.PerspectiveFovLH(3.14F / 3.0F, ratio, 1, 1000);
                    Matrix view = Matrix.LookAtLH(new Vector3(0, 0, -50), new Vector3(0, 0, 0), Vector3.UnitY);
                    Matrix world = Matrix.RotationY(0); // (float)Math.PI /2);
                    Matrix WVP = world * view * projection;

                    //update constant buffer
                    device.UpdateData<Matrix>(buffer, WVP);

                    //pass constant buffer to shader
                    //device.DeviceContext.VertexShader.SetConstantBuffer(0, buffer);

                    //draw mesh
                    mesh.DrawPoints(vertices.Length);


                    ////Set matrices
                    //world = Matrix.RotationZ( (float)Math.PI /2);
                    //WVP = world * view * projection;

                    ////update constant buffer
                    //device.UpdateData<Matrix>(buffer, WVP);

                    //////pass constant buffer to shader
                    ////device.DeviceContext.VertexShader.SetConstantBuffer(0, buffer);

                    //////apply shader
                    ////shader.Apply();

                    ////draw mesh
                    //mesh.DrawPoints(vertices.Length);


                    //begin drawing text
                    device.DeviceContext.GeometryShader.Set(null);



                    //begin drawing text
                    font.Begin();

                    //draw string
                    //font.DrawString("Hello SharpDX", 0, 0, Color.White);
                    fpsCounter.Update();
                    count++;
                    font.DrawString("FPS: " + fpsCounter.FPS + ":" + count, 0, 0, Color.White);

                    font.DrawString("Current Time " + DateTime.Now.ToString(), 0, 32, Color.White);

                    //flush text to view
                    font.End();

                    //present
                    device.Present();
                });
            }




            Console.WriteLine("Completed.");
            //Console.ReadLine();
            return;

            Dictionary<string, int> crdtypes = new Dictionary<string, int>(50);
            using (StreamReader rdr = new StreamReader("c:\\temp\\popmoney\\rmaccounts"))
            {
                while (!rdr.EndOfStream)
                {
                    string rcd = rdr.ReadLine();
                    if (rcd.Length != 78)
                    {
                    }
                    Console.WriteLine(rcd.Length.ToString());
                    string typ = rcd.Substring(53, 2);
                    int lng = rcd.Substring(30, 23).Trim().Length;
                    if (!crdtypes.ContainsKey(rcd))
                        crdtypes[typ] = 0;
                    if (crdtypes[typ] < lng)
                        crdtypes[typ] = lng;
                }
            }
            foreach (var dict in crdtypes.AsQueryable())
                Console.WriteLine(dict.Key + ":" + dict.Value.ToString());
            Console.ReadLine();
        }
    }
}
