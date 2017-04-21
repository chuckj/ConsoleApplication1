using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;

namespace ConsoleApplication1
{
    public class VizRes
    {
        public VizRes()
        {
            VizResList.Add(this);
            this.DevIndepAcquire();
        }

        ~VizRes()
        {
            this.DevIndepRelease();
            this.DevDepRelease();
            VizResList.Remove(this);
        }

        public static Dictionary<string, object> DevIndepResources = new Dictionary<string, object>();
        public static Dictionary<string, object> DevDepResources = new Dictionary<string, object>();
        public static List<VizRes> VizResList = new List<VizRes>();

        public static void DevDepReacquireAll(RenderTarget target)
        {
            DevDepReleaseAll();
            DevDepAcquireAll(target);
        }

        public static void DevDepReleaseAll()
        {
            foreach (var viz in VizResList)
                viz.DevDepRelease();
            foreach (IDisposable res in DevDepResources.Values)
                res.Dispose();
            DevDepResources.Clear();
        }

        public static void DevDepAcquireAll(RenderTarget target)
        {
            foreach (var viz in VizResList)
                viz.DevDepAcquire(target);
        }

        public static void DevIndepAcquireAll()
        {
            foreach (var viz in VizResList)
                viz.DevIndepAcquire();
        }

        public virtual void DevIndepAcquire() { }

        public virtual void DevIndepRelease() { }

        public virtual void DevDepAcquire(RenderTarget target) { }

        public virtual void DevDepRelease() { }

        #region Device Independent Resources
        public StrokeStyle StrokeStyle(StrokeStyleProperties props) => (StrokeStyle)addDevIndepResource($"StrokeStyle:{props.DashStyle}",
    () => new StrokeStyle(Global.Instance.factory2D1, props));

        public StrokeStyle StrokeStyle(StrokeStyleProperties props, float[] dashes) => (StrokeStyle)addDevIndepResource($"StrokeStyle:{props.DashStyle}::{string.Join(":", dashes)}",
    () => new StrokeStyle(Global.Instance.factory2D1, props, dashes));

        public TextFormat TextFormat(string family, float size) => (TextFormat)addDevIndepResource($"TextFormat:{family}:{size}",
    () => new TextFormat(Global.Instance.factoryWrite, family, size));

        private object addDevIndepResource(string name, Func<object> createRes)
        {
            object res;
            if (!DevIndepResources.TryGetValue(name, out res))
                DevIndepResources[name] = res = createRes();
            return res;
        }
        #endregion

        #region Device Dependent Resources

        public Brush SolidColorBrush(RenderTarget target, Color color) => (SolidColorBrush)addDevDepResource($"SolidBrush:{color.R}/{color.G}/{color.B}",
    () => new SolidColorBrush(target, color));

        private object addDevDepResource(string name, Func<object> createRes)
        {
            object res;
            if (!DevDepResources.TryGetValue(name, out res))
                DevDepResources[name] = res = createRes();
            return res;
        }
        #endregion
    }
}
