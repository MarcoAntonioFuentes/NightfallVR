using UnityEngine;
using UnityEngine.UI;

namespace CyberShield
{
    // Small vector glyphs stay sharp in world-space UI without texture memory.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class CyberShieldGlyph : MaskableGraphic
    {
        public int symbol;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            switch(symbol)
            {
                case 0:Path(vh,new[]{new Vector2(-.44f,-.3f),new Vector2(-.44f,.3f),new Vector2(.44f,.3f),new Vector2(.44f,-.3f),new Vector2(-.44f,-.3f)});Line(vh,new Vector2(-.44f,.3f),new Vector2(0,-.06f));Line(vh,new Vector2(0,-.06f),new Vector2(.44f,.3f));break;
                case 1:Circle(vh,new Vector2(0,.2f),.16f);Path(vh,new[]{new Vector2(-.36f,-.35f),new Vector2(-.3f,-.05f),new Vector2(0,.02f),new Vector2(.3f,-.05f),new Vector2(.36f,-.35f)});break;
                case 2:Circle(vh,new Vector2(-.2f,-.15f),.23f);Circle(vh,new Vector2(.2f,.15f),.23f);Line(vh,new Vector2(-.15f,-.13f),new Vector2(.15f,.13f));break;
                case 3:Circle(vh,Vector2.zero,.42f);Line(vh,Vector2.zero,new Vector2(0,.27f));Line(vh,Vector2.zero,new Vector2(.23f,-.1f));break;
                case 4:Box(vh,new Vector2(-.27f,-.27f),new Vector2(.27f,.27f));for(int i=0;i<3;i++){float p=-.2f+i*.2f;Line(vh,new Vector2(p,.27f),new Vector2(p,.45f));Line(vh,new Vector2(p,-.27f),new Vector2(p,-.45f));Line(vh,new Vector2(.27f,p),new Vector2(.45f,p));Line(vh,new Vector2(-.27f,p),new Vector2(-.45f,p));}break;
                case 5:Circle(vh,new Vector2(0,.3f),.12f);Circle(vh,new Vector2(-.3f,-.3f),.12f);Circle(vh,new Vector2(.3f,-.3f),.12f);Line(vh,new Vector2(0,.18f),new Vector2(-.25f,-.2f));Line(vh,new Vector2(0,.18f),new Vector2(.25f,-.2f));break;
                case 6:for(int i=0;i<3;i++){float y=-.33f+i*.27f;Box(vh,new Vector2(-.35f,y),new Vector2(.35f,y+.17f));}break;
                case 7:Path(vh,new[]{new Vector2(-.36f,0),new Vector2(-.08f,-.27f),new Vector2(.38f,.32f)});break;
                case 8:Path(vh,new[]{new Vector2(-.37f,.3f),new Vector2(0,.46f),new Vector2(.37f,.3f),new Vector2(.3f,-.15f),new Vector2(0,-.44f),new Vector2(-.3f,-.15f),new Vector2(-.37f,.3f)});Line(vh,new Vector2(-.17f,.02f),new Vector2(-.03f,-.12f));Line(vh,new Vector2(-.03f,-.12f),new Vector2(.2f,.17f));break;
                case 10:Path(vh,new[]{new Vector2(0,.45f),new Vector2(.44f,-.36f),new Vector2(-.44f,-.36f),new Vector2(0,.45f)});Line(vh,new Vector2(0,.2f),new Vector2(0,-.08f));Line(vh,new Vector2(0,-.21f),new Vector2(0,-.25f));break;
                case 11:Box(vh,new Vector2(-.33f,-.4f),new Vector2(.33f,.08f));Path(vh,new[]{new Vector2(-.21f,.08f),new Vector2(-.21f,.3f),new Vector2(-.12f,.43f),new Vector2(.12f,.43f),new Vector2(.21f,.3f),new Vector2(.21f,.08f)});Line(vh,new Vector2(0,-.1f),new Vector2(0,-.27f));break;
                case 12:var star=new Vector2[11];for(int i=0;i<11;i++){float a=Mathf.PI*.5f+i*Mathf.PI/5;star[i]=new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(i%2==0?.45f:.2f);}Path(vh,star);break;
                default:Box(vh,new Vector2(-.4f,-.25f),new Vector2(.4f,.32f));Line(vh,new Vector2(0,-.25f),new Vector2(0,-.43f));Line(vh,new Vector2(-.2f,-.43f),new Vector2(.2f,-.43f));break;
            }
        }
        void Circle(VertexHelper vh,Vector2 center,float radius){Vector2 previous=center+Vector2.right*radius;for(int i=1;i<=24;i++){float a=i*Mathf.PI*2/24;Vector2 p=center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius;Line(vh,previous,p);previous=p;}}
        void Box(VertexHelper vh,Vector2 a,Vector2 b)=>Path(vh,new[]{a,new Vector2(a.x,b.y),b,new Vector2(b.x,a.y),a});
        void Path(VertexHelper vh,Vector2[] points){for(int i=1;i<points.Length;i++)Line(vh,points[i-1],points[i]);}
        void Line(VertexHelper vh,Vector2 a,Vector2 b)
        {
            var r=rectTransform.rect;a=r.center+Vector2.Scale(a,r.size);b=r.center+Vector2.Scale(b,r.size);
            Vector2 d=(b-a).normalized,n=new Vector2(-d.y,d.x)*Mathf.Min(r.width,r.height)*.027f;
            int start=vh.currentVertCount;vh.AddVert(a-n,color,Vector2.zero);vh.AddVert(a+n,color,Vector2.zero);vh.AddVert(b+n,color,Vector2.zero);vh.AddVert(b-n,color,Vector2.zero);vh.AddTriangle(start,start+1,start+2);vh.AddTriangle(start,start+2,start+3);
        }
    }
}
