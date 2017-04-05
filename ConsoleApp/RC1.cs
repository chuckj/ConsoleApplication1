using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SD = System.Drawing;
using Colors = System.Drawing.Color;
using SDXD3D11Buffer = SharpDX.Direct3D11.Buffer;
using SharpDX.Direct3D;
using System.Threading;
using Microsoft.ConcurrencyVisualizer.Instrumentation;

namespace ConsoleApplication1
{
    public class RC1 : SharpDX.Windows.RenderControl
    {
        // Disposable
        private SharpDevice device = null;
        private SharpMesh mesh = null;
        private SharpShader shaderBulbs = null;
        private SharpShader shaderLines = null;
        private SDXD3D11Buffer buffer = null;
        private SDXD3D11Buffer clrTbl = null;
        private SDXD3D11Buffer linvertbuff = null;
        private SDXD3D11Buffer linndxbuff = null;
        private SDXD3D11Buffer trivertbuff = null;
        private SDXD3D11Buffer trindxbuff = null;
        private PixelShader areaShader = null;


        private SharpFPS fpsCounter = null;
        private DisplayUpdate dsp = null;

        private int updateds = 0;
        private MinMax[] sb;
        private float scale = 100.0f;
        private Vector4[] colorTable = null;
        private int count = 0;
        private IndexVertex[] lineVertices = null;
        private IndexVertex[] triVertices = null;
        private short[] lineIndices = null;
        private short[] triIndices = null;
        private IProgress<Tuple<string, string, string>> progress;

        public RC1()
        {
            InitializeComponent();
        }


        private void InitializeComponent()
        {
            this.MouseWheel += rc1_MouseWheel;
        }


        public void WinInit(IProgress<Tuple<string, string, string>> progress )
        {
            this.progress = progress;
        }


        public void D3DInit()
        {
            // None: 0, Shift: 1, !Shift & Ctrl: 2, Alt: +3

            sb = new[] { new MinMax(-90, 5), new MinMax(-90, 90), new MinMax(40, 300),
                new MinMax(-250, 0), new MinMax(-300, 300), new MinMax(-200, 200) };

            int ndx = Global.Instance.VuDict["tree"].LitArray.Count();
            //foreach (var lit in Global.Instance.LitDict.Values.OfType<GECELit>())
            //    lit.GlobalIndex = ndx++;

            //foreach (var lit in Global.Instance.LitDict.Values.OfType<DMXLit>())
            //    lit.GlobalIndex = ndx++;

            //foreach (var lit in Global.Instance.LitDict.Values.OfType<FeatureLit>())
            //    lit.GlobalIndex = ndx++;


            IndexVertex[] monoGeceVertices =
                Global.Instance.VuDict["tree"].LitArray.Select((x) => new IndexVertex(new Vector3(x.Pt.X / scale, x.Pt.Y / scale, x.Pt.Z / scale), (uint)x.Index))
                .Concat(Global.Instance.LitDict.Values.OfType<GECELit>().Select(x => new IndexVertex(new Vector3(x.Pt.X / scale, x.Pt.Y / scale, x.Pt.Z / scale), (uint)x.GlobalIndex))).ToArray();
            int[] monoGeceIndices = Enumerable.Range(0, monoGeceVertices.Length).ToArray();


            ////var test = Global.Instance.LitDict.Values.OfType<RGBLit>().Select(x => new IndexVertex(new Vector3(x.Pt.X / scale, x.Pt.Y / scale, x.Pt.Z / scale), (uint)(x.GlobalIndex)));




            //IndexVertex[] monoVertices =
            //    Global.Instance.VuDict["tree"].LitArray.Select((x) => new IndexVertex(new Vector3(x.Pt.X / scale, x.Pt.Y / scale, x.Pt.Z / scale), (uint)x.Index)).ToArray();
            //int[] monoIndices = Enumerable.Range(0, monoVertices.Length).ToArray();

            //IndexVertex[] geceVertices =
            //    Global.Instance.LitDict.Values.OfType<GECELit>().Select(x => new IndexVertex(new Vector3(x.Pt.X / scale, x.Pt.Y / scale, x.Pt.Z / scale), (uint)(x.GlobalIndex))).ToArray();
            //int[] geceIndices = Enumerable.Range(0, geceVertices.Length).ToArray();

            //IndexVertex[] dmxVertices =
            //    Global.Instance.LitDict.Values.OfType<DMXLit>().Select(x => new IndexVertex(new Vector3(x.Pt.X / scale, x.Pt.Y / scale, x.Pt.Z / scale), (uint)(x.GlobalIndex))).ToArray();

            Colors[] clrtbl = new[] { Colors.Red, Colors.Blue, Colors.Yellow, Colors.Green };
            int colorCnt = Global.Instance.LitDict.Values.OfType<GECELit>().Count() 
                + Global.Instance.LitDict.Values.OfType<DMXLit>().Count()
            //    + Global.Instance.FeatureLitDict.Count()
            ;
            colorTable =
                 Global.Instance.dta.Select((x) => new Vector4(x.Clr.R / 255.0f, x.Clr.G / 255.0f, x.Clr.B / 255.0f, 0))
                 .Concat(Enumerable.Range(0, colorCnt).Select((x, n) => (Vector4)(Clr)clrtbl[n % 4])).ToArray();



            //var cvclr = new Vector4(1f, 0f, 0f, 1);
            lineVertices = Global.Instance.LineVertices.Select(pt => new IndexVertex(new Vector3(pt.X / scale, pt.Y / scale, pt.Z / scale), (uint)pt.Ndx)).ToArray();
            lineIndices = Global.Instance.LineIndices.ToArray();

            triVertices = Global.Instance.TriVertices.Select(pt => new IndexVertex(new Vector3(pt.X / scale, pt.Y / scale, pt.Z / scale), (uint)pt.Ndx)).ToArray();
            triIndices = Global.Instance.TriIndices.ToArray();

            device = new SharpDevice(this);

            bool supportsConcurrentResources, supportsCommandLists;
            device.IsMultithreadingSupported(out supportsConcurrentResources, out supportsCommandLists);
            
            mesh = new SharpMesh(device);

            //Create Shader for Bulb generation
            shaderBulbs = new SharpShader(device, "../../HLSL.txt",
                    new SharpShaderDescription() { VertexShaderFunction = "VS", PixelShaderFunction = "PS", GeometryShaderFunction = "GS" },
                    new InputElement[] {
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("TEXCOORD", 0, Format.R32_UInt, 12, 0),
                    });

            //Create Shader for Line drawing
            shaderLines = new SharpShader(device, "../../HLSL.txt",
                    new SharpShaderDescription() { VertexShaderFunction = "VSL", PixelShaderFunction = "PSL" },
                    new InputElement[] {
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("TEXCOORD", 0, Format.R32_UInt, 12, 0),
                    });

            //create constant buffer
            buffer = shaderBulbs.CreateBuffer<WVPAndR>();

            clrTbl =
                new SDXD3D11Buffer(device.Device, (colorTable.Length * 16 + 15) / 16 * 16, ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);



            if (lineVertices.Length > 0)
            {
                //linvertbuff = SDXD3D11Buffer.Create(device.Device, BindFlags.VertexBuffer, lineVertices);
                linndxbuff = SDXD3D11Buffer.Create(device.Device, BindFlags.IndexBuffer, lineIndices);
            }
            if (triVertices.Length > 0)
            {
                trivertbuff = SDXD3D11Buffer.Create(device.Device, BindFlags.VertexBuffer, triVertices);
                trindxbuff = SDXD3D11Buffer.Create(device.Device, BindFlags.IndexBuffer, triIndices);
            }

            shaderBulbs.VertexBuffer = SDXD3D11Buffer.Create<IndexVertex>(device.Device, BindFlags.VertexBuffer, monoGeceVertices);
            shaderBulbs.IndexBuffer = SDXD3D11Buffer.Create(device.Device, BindFlags.IndexBuffer, monoGeceIndices);
            shaderBulbs.VertexSize = SharpDX.Utilities.SizeOf<IndexVertex>();
            shaderBulbs.IndexCount = monoGeceIndices.Length;

            //shaderLines.VertexBuffer = Buffer11.Create<ColoredVertex>(device.Device, BindFlags.VertexBuffer, lineVertices);
            //shaderLines.IndexBuffer = Buffer11.Create(device.Device, BindFlags.IndexBuffer, lineIndices);
            //shaderLines.VertexSize = SharpDX.Utilities.SizeOf<ColoredVertex>();
            //shaderLines.IndexCount = lineIndices.Length;

            string version = SharpDX.Direct3D11.Device.GetSupportedFeatureLevel() == SharpDX.Direct3D.FeatureLevel.Level_11_0 ? "5_0" : "4_0";
            var pixelShaderByteCode = SharpDX.D3DCompiler.ShaderBytecode.CompileFromFile("../../HLSL.txt", "PSA", "ps_" + version);
            areaShader = new PixelShader(device.Device, pixelShaderByteCode);

            fpsCounter = new SharpFPS();
            fpsCounter.Reset();
            //dsp = new DisplayUpdate();
        }

        public static SD.Size GetSize(Control C)
        {
            if (!C.InvokeRequired) return C.ClientSize;
            else return (SD.Size)C.Invoke(new Func<Control, SD.Size>(GetSize), C);
        }

        private int lastoffset = -1;
        private int offset = 0;
        private WeakReference prevDspInfo = new WeakReference(null);
        private int prevDspInfoIndex = -1;


        private int blink = 0;

        public void D3DRender(bool updated)
        {
            if (true) //Global.Instance.Updated)
            {
#if (MARKERS)
                Span span = Markers.EnterSpan($"{nameof(RC1)} render");
#endif

                //resize if form was resized
                if (device.MustResize)
                {
                    device.Resize();
                    //font.Resize();
                }

                //apply states
                device.UpdateAllStates();

                //Set matrices
                var sz = GetSize(this);
                float ratio = (float)sz.Width / (float)sz.Height;
                Matrix projection = Matrix.PerspectiveFovLH(3.14F / 3.0F, ratio, 1, 1500 / scale);
                var theda = -Math.PI * Global.Instance.Settings.GetViewPort(1) / 180.0;
                var omega = Math.PI * Global.Instance.Settings.GetViewPort(0) / 180.0;
                Matrix R1 = Matrix.RotationX((float)-omega);
                Matrix R2 = Matrix.RotationY((float)-theda);

                Matrix R = R1 * R2;

                // ((float)(150 * Math.Sin(theda)), -vScrollBar1.Value, (float)(-150 * Math.Cos(theda))

                Matrix view = Matrix.LookAtLH(Vector3.TransformCoordinate(new Vector3(0 / scale, 60 / scale, -Global.Instance.Settings.GetViewPort(2) * 5 / scale), R),
                            new Vector3(-Global.Instance.Settings.GetViewPort(4) / scale, -Global.Instance.Settings.GetViewPort(3) / scale, Global.Instance.Settings.GetViewPort(5) / scale), Vector3.UnitY);
                Matrix world = Matrix.RotationY(0); // (float)Math.PI /2);
                Matrix WVP = world * view * projection;


                device.Clear(Color.Black);


                //apply shader
                shaderBulbs.Apply();

                //apply constant buffer to geometry shader
                device.DeviceContext.GeometryShader.SetConstantBuffer(0, buffer);

                //apply constant buffer to geometry shader
                device.DeviceContext.GeometryShader.SetConstantBuffer(1, clrTbl);

                //update constant buffer
                device.UpdateData<WVPAndR>(buffer, new WVPAndR(WVP, R));


                if (updated)
                {

                    //var curr = Global.Instance.currDisplay;

                    ////if (Global.Instance.currMarque == null)
                    ////{
                    ////    foreach (var td in Global.Instance.dta)
                    ////    {
                    ////        colorTable[td.ndx].A = (byte)(Global.Instance.currDisplay[td] ? 255 : 0);
                    ////    }
                    ////}
                    ////else
                    ////{

                    //updateds++;

                    //var mrq = Global.Instance.currMarque;

                    //foreach (var td in Global.Instance.dta)
                    //{
                    //    colorTable[td.Index] = ((curr[td] || ((td.MarqueNdx >= 0) && (mrq != null) && mrq[td])) ?
                    //        new Vector4(td.Clr.R / 255.0f, td.Clr.G / 255.0f, td.Clr.B / 255.0f, 0) : new Vector4(0, 0, 0, 0));
                    //}
                    ////}

                    Vector4[] clrs = colorTable;
                    if (Global.Instance.Song != null)
                    {
                        var song = Global.Instance.Song;
                        if (song.DisplayInfo == null)
                        {
                            if (song.LitValues != null)
                            {
                                offset++;
                                clrs = song.LitValues[(offset / 60) % 5];
                            }
                        }
                        else
                        {
                            lock (song.DisplayInfoLock)
                            {
                                //if (song.LitValues != null)
                                //{
                                //    offset++;
                                //    clrs = song.LitValues[(offset / 60) % 5];
                                //}
                                var dspInfo = song.DisplayInfo;
                                int rawNdx = song.Position * 30 / Global.pxpersec;
                                int ndx = rawNdx;
                                while ((dspInfo.Index[ndx] == 0) && (ndx > 0))
                                    ndx--;
                                if (dspInfo == null || dspInfo.Index[ndx] == 0)
                                {
                                    if (song.LitValues != null)
                                    {
                                        offset++;
                                        clrs = song.LitValues[(offset / 60) % 5];
                                    }
                                }
                                else if (ndx != prevDspInfoIndex || dspInfo != prevDspInfo.Target)
                                {
                                    prevDspInfoIndex = ndx;
                                    prevDspInfo.Target = dspInfo;

                                    dspInfo.FileStream.Position = dspInfo.Index[ndx];
                                    for (int lit = 0; lit < clrs.Length; lit++)
                                    {
                                        var clr = dspInfo.BinaryReader.ReadUInt32();
                                        clrs[lit] = new Vector4(((clr >> 16) & 0xff) / 256.0f, ((clr >> 8) & 0xff) / 256.0f, ((clr >> 0) & 0xff) / 256.0f, ((clr >> 24) & 0xff) / 256.0f);
                                    }
                                }
                            }
                        }
                    }

                    device.DeviceContext.UpdateSubresource<Vector4>(clrs, clrTbl);



                    //if (triVertices.Length > 0)
                    //{
                    //    dispose(trivertbuff);

                    //    var dmxStrand = Global.Instance.DMXStrand;
                    //    foreach (var lit in dmxStrand.lites.OfType<DMXLit>())
                    //    {
                    //        var clr = clrs[lit.GlobalIndex];
                    //        foreach (var ndx in lit.LiteNdx)
                    //            triVertices[ndx].Color = clr;
                    //        //clr.W = clr.W / 2; 
                    //        clr.X = clr.X / 2; clr.Y = clr.Y/ 2; clr.Z = clr.Z / 2;
                    //        foreach (var ndx in lit.DimNdx)
                    //            triVertices[ndx].Color = clr;
                    //    }
                    //    trivertbuff = SDXD3D11Buffer.Create(device.Device, BindFlags.VertexBuffer, triVertices);
                    //}
                }

                //if (lineVertices.Length > 0)
                //{
                //    dispose(linvertbuff);

                //    linvertbuff = SDXD3D11Buffer.Create(device.Device, BindFlags.VertexBuffer, lineVertices);
                //}

                ////draw mesh
                ////mesh.DrawPoints(vertices.Length);
                shaderBulbs.Draw(SharpDX.Direct3D.PrimitiveTopology.PointList);

                //begin drawing text
                device.DeviceContext.GeometryShader.Set(null);

                //apply shader
                shaderLines.Apply();

                //update constant buffer
                device.UpdateData<Matrix>(buffer, WVP);

                //apply constant buffer to vertex shader
                device.DeviceContext.VertexShader.SetConstantBuffer(0, buffer);

                device.DeviceContext.VertexShader.SetConstantBuffer(1, clrTbl);

                if (triVertices.Length > 0)
                {
                    //  draw
                    device.DeviceContext.PixelShader.Set(areaShader);

                    device.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
                    device.DeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(trivertbuff, SharpDX.Utilities.SizeOf<IndexVertex>(), 0));
                    device.DeviceContext.InputAssembler.SetIndexBuffer(trindxbuff, Format.R16_UInt, 0);
                    device.DeviceContext.DrawIndexed(triIndices.Length, 0, 0);
                }


                if (lineVertices.Length > 0)
                {
                    device.DeviceContext.PixelShader.Set(shaderLines.PixelShader);

                    device.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineStrip;
                    device.DeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(linvertbuff, SharpDX.Utilities.SizeOf<IndexVertex>(), 0));
                    device.DeviceContext.InputAssembler.SetIndexBuffer(linndxbuff, Format.R16_UInt, 0);
                    device.DeviceContext.DrawIndexed(lineIndices.Length, 0, 0);
                }

#if (MARKERS)
                span.Leave();
#endif


                //present
#if (MARKERS)
                span = Markers.EnterSpan($"{nameof(RC1)} present");
#endif

                device.Present();
#if (MARKERS)
                span.Leave();
#endif
                fpsCounter.Update();
                count++;

#if (MARKERS)
                span = Markers.EnterSpan($"{nameof(RC1)} progress");
#endif
                var fps = fpsCounter.FPS;
#if (MARKERS)
                if (fps < 59)
                    Markers.WriteFlag(Importance.Critical, "fps:" + fps);
#endif
                string tssLbl1Text = $"FPS:{fps}:{Global.Instance.RealTime}  Updates:{updateds}";
                string tssLbl2Text = $"Camera: {-Global.Instance.Settings.GetViewPort(0)}° up, {-Global.Instance.Settings.GetViewPort(1)}° rt, {Global.Instance.Settings.GetViewPort(2)}";
                string tssLbl3Text = $"Viewing: ({-Global.Instance.Settings.GetViewPort(4)}, {-Global.Instance.Settings.GetViewPort(3)}, {Global.Instance.Settings.GetViewPort(5)})";
                progress.Report(Tuple.Create(tssLbl1Text, tssLbl2Text, tssLbl3Text));

#if (MARKERS)
                span.Leave();
#endif
            }
        }

        public void D3DRelease()
        {

        }

        public void rc1_MouseWheel(object sender, MouseEventArgs e)
        {
            // Update the drawing based upon the mouse wheel scrolling.

            int numberOfTextLinesToMove = e.Delta * SystemInformation.MouseWheelScrollLines / 60;

            int sbNdx = 0;
            if ((Control.ModifierKeys & Keys.Shift) != 0) sbNdx = 1;
            else if ((Control.ModifierKeys & Keys.Control) != 0) sbNdx = 2;
            if ((Control.ModifierKeys & Keys.Alt) != 0) sbNdx += 3;

            int newValue = Global.Instance.Settings.GetViewPort(sbNdx) - numberOfTextLinesToMove;
            if (newValue > sb[sbNdx].max) newValue = sb[sbNdx].max;
            if (newValue < sb[sbNdx].min) newValue = sb[sbNdx].min;
            Global.Instance.Settings.SetViewPort(sbNdx, newValue);
        }

#region IDispose
        bool disposed = false;

        public new void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected new virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.

                dispose(device);
                dispose(mesh);
                dispose(shaderBulbs);
                dispose(shaderLines);
                dispose(buffer);
                dispose(clrTbl);
                dispose(linvertbuff);
                dispose(linndxbuff);
                dispose(trivertbuff);
                dispose(trindxbuff);
                dispose(areaShader);
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }

        private void dispose(IDisposable obj)
        {
            if (obj != null) obj.Dispose();
        }
#endregion
    }
}
