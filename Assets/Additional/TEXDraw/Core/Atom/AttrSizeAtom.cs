
namespace TexDrawLib
{
    public class AttrSizeAtom : Atom
    {
        const string dotStr = ".";
        const string dotdotStr = "..";

        public static AttrSizeAtom Get(Atom baseAtom, string sizeStr)
        {
            var atom = ObjPool<AttrSizeAtom>.Get();
            atom.BaseAtom = baseAtom;
            ParseSize(sizeStr, out atom.Size, out atom.Offset);
            return atom;
        }

        public static void ParseSize(string str, out float size, out float offset)
        {
            if (str != null)
            {
                if (str.Length == 0)
                {
                    offset = 0;
                    size = float.NaN;
                }
                else if (str == dotStr)
                {
                    offset = 0;
                    size = TEXConfiguration.main.ScriptFactor;
                }
                else if (str == dotdotStr)
                {
                    offset = 0;
                    size = TEXConfiguration.main.NestedScriptFactor;
                }
                else
                {
                    int pos = str.IndexOf('-');
                    if (pos < 0)
                        pos = str.IndexOf('+');
                    if (pos < 0 || !float.TryParse(str.Substring(pos), out offset))
                        offset = 0;
                    if (pos < 1 || !float.TryParse(str.Substring(0, pos), out size))
                    {
                        if (pos == 0 || !float.TryParse(str, out size))
                            size = 1;
                    }
                }
            }
            else
            {
                size = 1;
                offset = 0;
            }
        }

        public Atom BaseAtom;

        public float Size;

        public float Offset;

        public override Box CreateBox()
        {
            // This SizeBox doesn't need start..end block, since the size metric are calculated directly
            if (BaseAtom == null)
                return StrutBox.Empty;
            else
            {
                var nan = float.IsNaN(Size);
                TexContext.Size.Push(nan ? 1f : Size);
                if (nan) TexContext.Environment.Push(TexEnvironment.Display);

                var box = BaseAtom.CreateBox();

                box.shift += Offset;
                TexContext.Size.Pop();
                if (nan) TexContext.Environment.Pop();

                return box;
            }
        }

        public override void Flush()
        {
            if (BaseAtom != null)
            {
                BaseAtom.Flush();
                BaseAtom = null;
            }
            ObjPool<AttrSizeAtom>.Release(this);
        }
    }
}

