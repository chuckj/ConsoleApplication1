using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
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
            var settings = Global.Instance.Settings = AppSettings.LoadAppSettings() ?? new AppSettings();

            logger.Info("Loading bitmaps...");
            var imgs = Directory.EnumerateFiles(Path.Combine(Directory.GetCurrentDirectory(), ".\\..\\..\\BitMapImgs"));
            foreach (var img in imgs)
            {
                using (var bitmap = (SD.Bitmap)SD.Image.FromFile(img))
                {
                    Global.Instance.BitMapImgs.Add(Path.GetFileNameWithoutExtension(img), AccessBitmap.ToArray(bitmap));
                }
            }
            logger.Info($"Bitmaps loaded: {Global.Instance.BitMapImgs.Count()}");


            logger.Info("Loading model...");

            XElement root = Global.Instance.Model = XDocument.Load(@".\\..\\..\\Model.xml").Element("root");

            Feature.Load(root);

            Cntrl.Load(root);

            View.Load(root);

            Global.Instance.LitArray = Global.Instance.LitDict.Values.OrderBy(lit => lit.GlobalIndex).ToArray();

            //  Displays

            var tree = Global.Instance.VuDict["tree"];
            Global.Instance.dta = tree.LitArray.Cast<MonoLit>().OrderBy(t => t.Index).ToArray();

            Global.Instance.tdOrder = tree.LitArray.Select(n => (short)n.Index).ToArray();

            //Global.Instance.dict = Global.Instance.dta.ToDictionary(d => Tuple.Create<int, int>(d.Row, d.Circle), d => d);

            // Transitions

            Global.Instance.TreeTransitionDict = new Dictionary<string, TreeTransition>()
                { {"topdown", TreeTransition.FromEnumerable(Global.Instance.dta.Select(x => (short)x.Row)) } };

            foreach (var trans in root.Descendants("transitions").Descendants("transition"))
            {
                Global.Instance.TreeTransitionDict.Add((string)trans.Attribute("name"), TreeTransition.FromString((string)trans.Attribute("value")));
            }

            //  Font

            foreach (var felm in root.Descendants("fonts").Descendants("font"))
            {
                Global.Instance.FontDict.Add(((string)felm.Attribute("char"))[0], FontUpdate.FromString((string)felm.Attribute("value")));
            }

            //  Step Transitions

            foreach (var tran in root.Descendants("steptransitions").Descendants("transition"))
            {
                var name = (string)tran.Attribute("name");
                if (string.IsNullOrEmpty(name))
                    throw new Exception($"transition must have name.");
                Global.Instance.StepTransitionDict.Add(name, StepTransition.Factory(tran.Elements()));
            }

            logger.Info("Loading main window...");

            //Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var frm = new Form2();
            //try
            //{
               Application.Run(frm);
            //}
            //catch (Exception e)
            //{
            //    logger.Info($"Unhandled exception: {e}");
            //}

            return;
        }
    }
}
