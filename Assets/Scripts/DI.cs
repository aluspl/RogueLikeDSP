using System;
using System.Reflection;
using LifeLike.Inferfaces;
using UnityEngine;

public static class DI{
    public static void Inject(MonoBehaviour mb)
    {
             Type t = mb.GetType ();
             FieldInfo[] infos = t.GetFields(BindingFlags.Instance |   
                             BindingFlags.NonPublic |        
                             BindingFlags.Public);
        for (int i = 0; i < infos.Length; i++)
        {
            var fi=infos[i];
             if(false == 
                typeof(IManager).IsAssignableFrom(fi.FieldType)) 
             { 
                continue;
             }
             if (false == 
                 fi.IsDefined(typeof(InjectAttribute), true))      
             {
                continue;
             }
             
             Attribute attr = Attribute.GetCustomAttributes(fi)[0];
             InjectAttribute ia = (attr as InjectAttribute);
 
             IManager found =   
                GameObject.Find(ia.GoName).GetComponent<IManager>();
             
             fi.SetValue(mb, found);
        }
    }
}
[AttributeUsageAttribute(AttributeTargets.Field)]
public class InjectAttribute : Attribute
{
    public string GoName {get; set;}
    public InjectAttribute(string goName)
     {
         this.GoName = goName;
     }
}