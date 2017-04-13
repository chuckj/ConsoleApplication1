﻿using SharpDX;
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
        private SharpShader shaderCandles = null;
        private SDXD3D11Buffer buffer = null;
        private SDXD3D11Buffer clrTbl = null;
        private SDXD3D11Buffer linvertbuff = null;
        private SDXD3D11Buffer linndxbuff = null;
        private SDXD3D11Buffer trivertbuff = null;
        private SDXD3D11Buffer trindxbuff = null;


        private SharpFPS fpsCounter = null;
        private DisplayUpdate dsp = null;

        private int updateds = 0;
        private MinMax[] sb;
        private float scale = 100.0f;
        private uint[] colorTable = null;
        private int count = 0;
        private IndexVertex[] lineVertices = null;
        private IndexVertex[] triVertices = null;
        private IndexVertex[] candleVertices = null;
        private short[] lineIndices = null;
        private short[] triIndices = null;
        private int[] candleIndices = null;
        private IProgress<Tuple<string, string, string>> progress;

        private Matrix WVP;
        private Matrix projection;
        private Matrix view;

        public RC1()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.MouseWheel += rc1_MouseWheel;
            this.MouseClick += rc1_MouseClick;
        }

        public void WinInit(IProgress<Tuple<string, string, string>> progress )
        {
            this.progress = progress;
        }

        private IProgress<string> clickOn;
        public void ClickOnInit(IProgress<string> progress)
        {
            this.clickOn = progress;
        }

        public void D3DInit()
        {
            // None: 0, Shift: 1, !Shift & Ctrl: 2, Alt: +3

            sb = new[] { new MinMax(-90, 5), new MinMax(-90, 90), new MinMax(40, 300),
                new MinMax(-250, 0), new MinMax(-300, 300), new MinMax(-200, 200) };

            IndexVertex[] monoGeceVertices =
                Global.Instance.LitArray.Where(lit => lit is MonoLit || lit is GECELit)
                    .Select((x) => new IndexVertex(new Vector3(x.Pt.X / scale, x.Pt.Y / scale, x.Pt.Z / scale), (uint)x.GlobalIndex))
                    .ToArray();
            int[] monoGeceIndices = Enumerable.Range(0, monoGeceVertices.Length).ToArray();

            lineVertices = Global.Instance.LineVertices.Select(pt => pt.ToIndexVertex()).ToArray();
            lineIndices = Global.Instance.LineIndices.ToArray();

            triVertices = Global.Instance.TriVertices.Select(pt => pt.ToIndexVertex()).ToArray();
            triIndices = Global.Instance.TriIndices.ToArray();

            candleVertices = Global.Instance.CandleVertices.Select(pt => pt.ToIndexVertex()).ToArray();
            candleIndices = Enumerable.Range(0, candleVertices.Length).ToArray();

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
                    new SharpShaderDescription() { VertexShaderFunction = "VSL", PixelShaderFunction = "PS" },
                    new InputElement[] {
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("TEXCOORD", 0, Format.R32_UInt, 12, 0),
                    });

            //Create Shader for Line drawing
            shaderCandles = new SharpShader(device, "../../HLSL.txt",
                    new SharpShaderDescription() { VertexShaderFunction = "VS", PixelShaderFunction = "PS", GeometryShaderFunction = "GSC" },
                    new InputElement[] {
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("TEXCOORD", 0, Format.R32_UInt, 12, 0),
                    });

            //create constant buffer
            buffer = shaderBulbs.CreateBuffer<WVPAndR>();

            colorTable = animate(0);

            clrTbl =
                new SDXD3D11Buffer(device.Device, (colorTable.Length * 16 + 15) / 16 * 16, ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);


            if (lineVertices.Length > 0)
            {
                linvertbuff = SDXD3D11Buffer.Create(device.Device, BindFlags.VertexBuffer, lineVertices);
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

            shaderCandles.VertexBuffer = SDXD3D11Buffer.Create<IndexVertex>(device.Device, BindFlags.VertexBuffer, candleVertices);
            shaderCandles.IndexBuffer = SDXD3D11Buffer.Create(device.Device, BindFlags.IndexBuffer, candleIndices);
            shaderCandles.VertexSize = SharpDX.Utilities.SizeOf<IndexVertex>();
            shaderCandles.IndexCount = candleIndices.Length;

            fpsCounter = new SharpFPS();
            fpsCounter.Reset();
        }

        public static SD.Size GetSize(Control C)
        {
            if (!C.InvokeRequired) return C.ClientSize;
            else return (SD.Size)C.Invoke(new Func<Control, SD.Size>(GetSize), C);
        }

        private int offset = 0;
        private WeakReference prevDspInfo = new WeakReference(null);
        private int prevDspInfoIndex = -1;

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
                projection = Matrix.PerspectiveFovLH(3.14F / 3.0F, ratio, 1, 2000 / scale);
                var theda = -Math.PI * Global.Instance.Settings.GetViewPort(1) / 180.0;
                var omega = Math.PI * Global.Instance.Settings.GetViewPort(0) / 180.0;
                Matrix R1 = Matrix.RotationX((float)-omega);
                Matrix R2 = Matrix.RotationY((float)-theda);

                Matrix R = R1 * R2;

                // ((float)(150 * Math.Sin(theda)), -vScrollBar1.Value, (float)(-150 * Math.Cos(theda))

                view = Matrix.LookAtLH(Vector3.TransformCoordinate(new Vector3(0 / scale, 60 / scale, -Global.Instance.Settings.GetViewPort(2) * 5 / scale), R),
                    new Vector3(-Global.Instance.Settings.GetViewPort(4) / scale, -Global.Instance.Settings.GetViewPort(3) / scale, Global.Instance.Settings.GetViewPort(5) / scale), Vector3.UnitY);
                Matrix world = Matrix.RotationY(0); // (float)Math.PI /2);
                WVP = world * view * projection;


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
                    if (Global.Instance.Song != null)
                    {
                        var song = Global.Instance.Song;
                        if (song.DisplayInfo == null)
                        {
                            colorTable = animate(offset++ / 60);
                        }
                        else
                        {
                            lock (song.DisplayInfoLock)
                            {
                                var dspInfo = song.DisplayInfo;
                                int rawNdx = song.Position * 30 / Global.pxpersec;
                                int ndx = rawNdx;
                                while ((dspInfo.Index[ndx] == 0) && (ndx > 0))
                                    ndx--;
                                if (dspInfo == null || dspInfo.Index[ndx] == 0)
                                {
                                    colorTable = animate(offset++ / 60);
                                }
                                else if (ndx != prevDspInfoIndex || dspInfo != prevDspInfo.Target)
                                {
                                    prevDspInfoIndex = ndx;
                                    prevDspInfo.Target = dspInfo;

                                    dspInfo.MMViewAccessor.ReadArray<UInt32>(dspInfo.Index[ndx], colorTable, 0, colorTable.Length);
                                }
                            }
                        }
                    }

                    Vector4[] clrs = colorTable.Select(c => new Vector4(((c>>16) & 0xff) / 256.0f, ((c >> 8) & 0xff) / 256.0f, (c & 0xff) / 256.0f, 1.0f)).ToArray();
                    device.DeviceContext.UpdateSubresource<Vector4>(clrs, clrTbl);
                }

                ////draw mesh
                ////mesh.DrawPoints(vertices.Length);
                shaderBulbs.Draw(SharpDX.Direct3D.PrimitiveTopology.PointList);

                //begin drawing text
                //device.DeviceContext.GeometryShader.Set(null);



                //apply shader
                shaderCandles.Apply();

                //apply constant buffer to geometry shader
                //device.DeviceContext.GeometryShader.SetConstantBuffer(0, buffer);

                //apply constant buffer to geometry shader
                //device.DeviceContext.GeometryShader.SetConstantBuffer(1, clrTbl);

                //update constant buffer
                //device.UpdateData<WVPAndR>(buffer, new WVPAndR(WVP, R));


                ////draw mesh
                ////mesh.DrawPoints(vertices.Length);
                shaderCandles.Draw(SharpDX.Direct3D.PrimitiveTopology.PointList);

                //begin drawing text
                device.DeviceContext.GeometryShader.Set(null);






                //apply shader
                shaderLines.Apply();

                //update constant buffer
                //device.UpdateData<Matrix>(buffer, WVP);

                //apply constant buffer to vertex shader
                device.DeviceContext.VertexShader.SetConstantBuffer(0, buffer);

                device.DeviceContext.VertexShader.SetConstantBuffer(1, clrTbl);

                if (triVertices.Length > 0)
                {
                    //  draw triangles
                    device.DeviceContext.PixelShader.Set(shaderLines.PixelShader); // areaShader);

                    device.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
                    device.DeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(trivertbuff, SharpDX.Utilities.SizeOf<IndexVertex>(), 0));
                    device.DeviceContext.InputAssembler.SetIndexBuffer(trindxbuff, Format.R16_UInt, 0);
                    device.DeviceContext.DrawIndexed(triIndices.Length, 0, 0);
                }


                if (lineVertices.Length > 0)
                {
                    //  draw lines
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

        static uint[] clrz = new[] { (uint)(Clr)Colors.Red, (uint)(Clr)Colors.Blue, (uint)(Clr)Colors.Yellow, (uint)(Clr)Colors.Green, (uint)(Clr)Colors.Orange,
                (uint)(Clr)Colors.Cyan, (uint)(Clr)Colors.Magenta };

        private uint[] animate(int ndx)
        {
            return Global.Instance.LitArray.Select((x, n) => (x is MonoLit || x is FeatureLit) ? (uint)x.InitVal : clrz[(n + ndx) % clrz.Length]).ToArray();
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


        private void rc1_MouseClick(object sender, MouseEventArgs e)
        {
            int mouseX = e.X;
            int mouseY = e.Y;

            Vector3 nearsource = new Vector3((float)mouseX, (float)mouseY, 0f);
            Vector3 farsource = new Vector3((float)mouseX, (float)mouseY, 1f);


            //Vector3 ZFarPlane = Vector3.Unproject(new Vector3(Coordinate, 0), 0, 0, ScreenSize.Width, ScreenSize.Height, -1000, 1000, View * Projection);
            //Vector3 ZNearPlane = Vector3.Unproject(new Vector3(Coordinate, 1), 0, 0, ScreenSize.Width, ScreenSize.Height, -1000, 1000, View * Projection);
            //Vector3 Direction = (ZFarPlane - ZNearPlane);
            //Direction.Normalize();
            //Ray ray = new Ray(ZNearPlane, Direction);


            Matrix vp = view * projection;

            Vector3 nearPoint;
            Vector3.Unproject(ref nearsource, 0, 0, this.Width, this.Height, -1000, 1000, ref vp, out nearPoint);

            Vector3 farPoint;
            Vector3.Unproject(ref farsource, 0, 0, this.Width, this.Height, -1000, 1000, ref vp, out farPoint);

            Vector3 direction = (farPoint - nearPoint);
            direction.Normalize();

            string report = string.Format($"({nearPoint.X},{nearPoint.Y},{nearPoint.Z}) ({farPoint.X},{farPoint.Y},{farPoint.Z})  ({direction.X},{direction.Y},{direction.Z})");
            clickOn.Report(report);
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
                dispose(shaderCandles);
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
