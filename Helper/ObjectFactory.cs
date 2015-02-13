using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            return Create(_assemblyName, _className, new Type[0], new object[0]);
        }
        public static object Create(string _assemblyName, string _className, Type[] _paramTypes, object[] _params)
        {
            if (string.IsNullOrWhiteSpace(_assemblyName)) _assemblyName = Assembly.GetExecutingAssembly().FullName;

            var _objectType = Type.GetType(_className);
            var _objectConstructor = _objectType.GetConstructor(_paramTypes);
            var _object = _objectConstructor.Invoke(_params);

            return _object;
        }
        public static object Create(string _assemblyName, string _className, params object[] _args)
        {
            if (_args == null || _args.Length == 0)
                return Create(_assemblyName, _className);

            var _types = _args.Select(_x => _x.GetType()).ToArray();

            return Create(_assemblyName, _className, _types, _args);
        }
    }
}
