
namespace TexDrawLib
{
    public class AttrLinkAtom : Atom
    {

        public static AttrLinkAtom Get(Atom baseAtom, string metaKey, bool underLink)
        {
            var atom = ObjPool<AttrLinkAtom>.Get();
            atom.BaseAtom = baseAtom;
            atom.MetaKey = metaKey;
            atom.UnderLink = underLink;
            return atom;
        }

        public Atom BaseAtom;

        public string MetaKey;

        public bool UnderLink;

        public override Box CreateBox()
        {
            if (BaseAtom == null)
                return StrutBox.Empty;
            else
            {
                if (UnderLink)
                {

                    var baseBox = BaseAtom.CreateBox();
                    var box = HorizontalBox.Get(baseBox);

                    float factor = TexContext.Scale / 2;
                    float margin = TEXConfiguration.main.NegateMargin * factor;
                    float thick = TEXConfiguration.main.LineThickness * factor;

                    box.Add(StrutBox.Get(-box.width, 0, 0, 0));
                    box.Add(StrikeBox.Get(baseBox.height, baseBox.width, baseBox.depth,
                            margin, thick, StrikeBox.StrikeMode.underline, 0, 0));

                    return AttrLinkBox.Get(box, MetaKey);

                }
                else
                    return AttrLinkBox.Get(BaseAtom.CreateBox(), MetaKey);
            }
        }

        public override void Flush()
        {
            if (BaseAtom != null)
            {
                BaseAtom.Flush();
                BaseAtom = null;
            }
            ObjPool<AttrLinkAtom>.Release(this);
        }

    }
}
