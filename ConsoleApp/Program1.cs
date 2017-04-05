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


using System.Xml.Linq;
using System.Runtime.InteropServices;
using System.Threading;

using SD = System.Drawing;

namespace ConsoleApplication1
{
    class Program
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(nameof(Program));

        [STAThread]
        static void Main(string[] args)
        {

            logger.Info("Loading settings...");

            var settings = AppSettings.LoadAppSettings();
            if (settings == null)
            {
                settings = new AppSettings()
                {
                };
            }
            Global.Instance.Settings = settings;

            logger.Info("Loading bitmaps...");
            var imgs = Directory.EnumerateFiles(Path.Combine(Directory.GetCurrentDirectory(), ".\\..\\..\\BitMapImgs"));
            foreach (var img in imgs)
            {
                using (SD.Bitmap bitmap = (SD.Bitmap)SD.Image.FromFile(img))
                {
                    Global.Instance.BitMapImgs.Add(Path.GetFileNameWithoutExtension(img), AccessBitmap.ToArray(bitmap));
                }
            }
            logger.Info($"{Global.Instance.BitMapImgs.Count()} bitmaps loaded.");



            logger.Info("Loading model...");

            XElement root = Global.Instance.Model = XDocument.Load(@".\\..\\..\\Model.xml").Element("root");

            Feature.Load(root);

            Cntrl.Load(root);

            View.Load(root);

            //  Displays

            var tree = Global.Instance.VuDict["tree"];
            Global.Instance.dta = tree.LitArray.Cast<MonoLit>().OrderBy(t => t.Index).ToArray();

            Global.Instance.tdOrder = tree.LitArray.Select(n => (short)n.Index).ToArray();

            Global.Instance.dict = Global.Instance.dta.ToDictionary(d => Tuple.Create<int, int>(d.Row, d.Circle), d => d);

            // Transitions

            Global.Instance.TreeTransitionDict = new Dictionary<string, TreeTransition>()
                { {"topdown", TreeTransition.FromEnumerable(Global.Instance.dta.Select(x => (short)x.Row)) } };

            foreach (XElement trans in root.Descendants("transitions").Descendants("transition"))
            {
                Global.Instance.TreeTransitionDict.Add((string)trans.Attribute("name"), TreeTransition.FromString((string)trans.Attribute("value")));
            }

            //  Font

            foreach (XElement felm in root.Descendants("fonts").Descendants("font"))
            {
                Global.Instance.FontDict.Add(((string)felm.Attribute("char"))[0], FontUpdate.FromString((string)felm.Attribute("value")));
            }

            foreach (XElement tran in root.Descendants("steptransitions").Descendants("transition"))
            {
                var name = (string)tran.Attribute("name");
                if (string.IsNullOrEmpty(name))
                    throw new Exception($"transition must have name.");
                var newtran = StepTransition.Factory(tran.Elements());
                Global.Instance.StepTransitionDict.Add(name, newtran);
            }

            logger.Info("Loading main window...");

            //Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var frm = new Form2();
            try
            {
                Application.Run(frm);
            }
            catch (Exception e)
            {
                logger.Info($"Unhandled exception: {e}");
            }

            return;
        }
    }
}
