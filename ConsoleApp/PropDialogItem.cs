using System;

namespace ConsoleApplication1
{
    public class PropDialogItem
    {
        public string Name;
        public string PropertyName;

        public PropDialogItem(string Name, string PropertyName)
        {
            this.Name = Name;
            this.PropertyName = PropertyName;
        }
    }

    public class PropDialogData
    {
        public Func<PropDialogItem[]> GetPropDialogItems;
        public Viz Viz;
        public Action<Viz> Handler;

        public PropDialogData(Func<PropDialogItem[]> GetPropDialogItems, Viz Viz, Action<Viz> Handler)
        {
            this.GetPropDialogItems = GetPropDialogItems;
            this.Viz = Viz;
            this.Handler = Handler;
        }
    }
}
