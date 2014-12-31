using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper
{
    public class ObjectFactory
    {
        public static object Create(string _className) 
        {
            return Create(null, _className);
        }
        public static object Create(string _assemblyName, string _className)
        {
            return Create(_assemblyName, _className, new object[0]);
        }
        public static object Create(string _assemblyName, string _className, params object[] _args)
        {
            return null;
        }
    }
}
