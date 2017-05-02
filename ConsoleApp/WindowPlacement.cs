using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace WindowPlacementExample
{
    // RECT structure required by WINDOWPLACEMENT structure
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
        }
    }

    // POINT structure required by WINDOWPLACEMENT structure
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    // WINDOWPLACEMENT stores the position, size, and state of a window

    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPLACEMENT
    {
        public int length;
        public int flags;
        public int showCmd;
        public POINT minPosition;
        public POINT maxPosition;
        public RECT normalPosition;

        public string ToXml()
        {
            var xml = new XElement("WindowPlacement",
                new XAttribute("length", length),
                new XAttribute("flags", flags),
                new XAttribute("showCmd", showCmd),
                new XElement("minPosition",
                    new XAttribute("x", minPosition.X),
                    new XAttribute("y", minPosition.Y)),
                new XElement("maxPosition",
                    new XAttribute("x", maxPosition.X),
                    new XAttribute("y", maxPosition.Y)),
                new XElement("normalPosition",
                    new XAttribute("left", normalPosition.Left),
                    new XAttribute("top", normalPosition.Top),
                    new XAttribute("right", normalPosition.Right),
                    new XAttribute("bottom", normalPosition.Bottom)));
            return xml.ToString();
        }

        public static bool TryParse(string placementXml, out WINDOWPLACEMENT placement)
        {
            var xml = XElement.Parse(placementXml);
            var minPosition = xml.Element("minPosition");
            var maxPosition = xml.Element("maxPosition");
            var normalPosition = xml.Element("normalPosition");

            placement = new WINDOWPLACEMENT()
            {
                length = (int)xml.Attribute("length"),
                flags = (int)xml.Attribute("flags"),
                showCmd = (int)xml.Attribute("showCmd"),
                minPosition = new POINT((int)minPosition.Attribute("x"), (int)minPosition.Attribute("y")),
                maxPosition = new POINT((int)maxPosition.Attribute("x"), (int)maxPosition.Attribute("y")),
                normalPosition = new RECT((int)normalPosition.Attribute("left"), (int)normalPosition.Attribute("top"), (int)normalPosition.Attribute("right"), (int)normalPosition.Attribute("bottom"))
            };
            return true;
        }
    }

    public static class WindowPlacement
    {
        private static Encoding encoding = new UTF8Encoding();
        private static XmlSerializer serializer = new XmlSerializer(typeof(WINDOWPLACEMENT));

        [DllImport("user32.dll")]
        private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        private static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;

        public static void SetPlacement(IntPtr windowHandle, string placementXml)
        {
            if (string.IsNullOrEmpty(placementXml))
            {
                return;
            }

            WINDOWPLACEMENT placement;

            try
            {
                WINDOWPLACEMENT.TryParse(placementXml, out placement);

                placement.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
                placement.flags = 0;
                placement.showCmd = (placement.showCmd == SW_SHOWMINIMIZED ? SW_SHOWNORMAL : placement.showCmd);
                SetWindowPlacement(windowHandle, ref placement);
            }
            catch (InvalidOperationException)
            {
                // Parsing placement XML failed. Fail silently.
            }
        }

        public static string GetPlacement(IntPtr windowHandle)
        {
            WINDOWPLACEMENT placement;
            GetWindowPlacement(windowHandle, out placement);
            return placement.ToXml();
        }
    }
}