using UnityEngine;
using UnityEngine.UI;

namespace CyberShield
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class CyberShieldPanelArt : MaskableGraphic
    {
        public bool radar;
        public float phase,progress;
        public int state;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;Vector2 c=r.center;
            if(!radar)
            {
                float x=r.width*.5f-4,y=r.height*.5f-4,k=Mathf.Min(35,Mathf.Min(x,y)*.25f);
                for(int sx=-1;sx<=1;sx+=2)for(int sy=-1;sy<=1;sy+=2)
                {
                    Line(vh,c+new Vector2(sx*(x-k*2),sy*y),c+new Vector2(sx*(x-k),sy*y),3,color);
                    Line(vh,c+new Vector2(sx*(x-k),sy*y),c+new Vector2(sx*x,sy*(y-k)),3,color);
                    Line(vh,c+new Vector2(sx*x,sy*(y-k)),c+new Vector2(sx*x,sy*(y-k*2)),3,color);
                }
                for(int i=-4;i<=4;i++)Line(vh,c+new Vector2(i*12,-y),c+new Vector2(i*12+5,-y),2,color);
                return;
            }
            float radius=Mathf.Min(r.width,r.height)*.39f;
            Ring(vh,c,radius,1,color*.9f);Ring(vh,c,radius*.65f,1,color*.65f);Ring(vh,c,radius*.3f,1,color*.65f);
            for(int i=0;i<32;i++){float a=i*Mathf.PI*2/32;var d=new Vector2(Mathf.Cos(a),Mathf.Sin(a));Line(vh,c+d*(radius+7),c+d*(radius+14),2,color*.7f);}
            Line(vh,c+Vector2.left*radius,c+Vector2.right*radius,1,color*.2f);Line(vh,c+Vector2.up*radius,c+Vector2.down*radius,1,color*.2f);
            if(state<2)
            {
                float a=phase*Mathf.PI*2;var d=new Vector2(Mathf.Cos(a),Mathf.Sin(a));Line(vh,c,c+d*radius,3,color);
                for(int i=0;i<8;i++){float angle=i*2.399f;var p=c+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius*(.25f+(i%3)*.23f);Ring(vh,p,i==2?9:4,1,color);}
            }
            else
            {
                Ring(vh,c,radius*.88f,Mathf.Max(.02f,progress),color);
                Line(vh,c+new Vector2(-radius*.32f,0),c+new Vector2(-radius*.06f,-radius*.24f),7,color);
                Line(vh,c+new Vector2(-radius*.06f,-radius*.24f),c+new Vector2(radius*.38f,radius*.28f),7,color);
            }
        }
        static void Ring(VertexHelper vh,Vector2 center,float radius,float portion,Color color){Vector2 last=center+Vector2.right*radius;for(int i=1;i<=64;i++){float a=i/64f*Mathf.PI*2*portion;var p=center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius;Line(vh,last,p,4,color);last=p;}}
        static void Line(VertexHelper vh,Vector2 a,Vector2 b,float width,Color color){var d=(b-a).normalized;var n=new Vector2(-d.y,d.x)*width*.5f;int k=vh.currentVertCount;vh.AddVert(a-n,color,Vector2.zero);vh.AddVert(a+n,color,Vector2.zero);vh.AddVert(b+n,color,Vector2.zero);vh.AddVert(b-n,color,Vector2.zero);vh.AddTriangle(k,k+1,k+2);vh.AddTriangle(k,k+2,k+3);}
    }
}
