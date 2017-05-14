using UnityEngine;
using System;

namespace Utils
{
    public static class MathUtils
    {     
         public static double CalculateAzimuth(Vector2 vec1, Vector2 vec2) {
            double A=0;
            var dY=vec2.y - vec1.y;
            var dX=vec2.x - vec1.x;    
            if (dX==0)
            {
                if (dY>0) A=0.5*Mathf.PI;
                else A=1.5*Mathf.PI;                
            }
            else
            {
                A=Mathf.Atan2(dY, dX);
                if (dX<0)
                {  A=A+Mathf.PI;}
                else
                { 
                     if (dY<0) A=A+(2*Mathf.PI); 
                }            
            }
            return A;
        }
        public static float Round(double number)
        {
            if (number>0) return 1f;
            if (number <0) return -1f;
            return 0f;

        }
        public static double AngleInDeg(Vector2 vec1, Vector2 vec2) {
             var A=CalculateAzimuth(vec1, vec2);
             var RG= (180 / Mathf.PI);
             Debug.Log(string.Format("Azimuth is {0}, Vector 1: x: {1} y: {2}, Vector 2: x: {3} y: {4} Angle is {5}", A, vec1.x,vec1.y,vec2.x,vec2.y,A*RG));

            return A*RG;
        }
        public static void RoundMoves(ref Vector2 moveVector)
        {
            if (Math.Abs(moveVector.x) > Math.Abs(moveVector.y))
            {
                moveVector.x = moveVector.x > 0 ? -1 : 1;
                moveVector.y = 0;
            }
            else if (Math.Abs(moveVector.x) < Math.Abs(moveVector.y))
            {
                moveVector.y= moveVector.y >0 ?  -1: 1;
                moveVector.x = 0;
            } 
            else
            {
                moveVector.x = moveVector.x > 0 ? -1 : 1;
                moveVector.y= moveVector.y >0 ?  -1: 1;
            }
        }
        public static Vector3 SetRotation(Vector2 moveVector)
        {
            if (moveVector.y>0)
            {
                return new Vector3(0,0,0);
            }
            if (moveVector.y<0)
            {
                return new Vector3(0,0,180);
            }
            if (moveVector.x>0)
            {
                return new Vector3(0,0,270);
            }
            if (moveVector.x<0)
            {
                return new Vector3(0,0,90);
            }
            return Vector3.zero;
        }
    }
}