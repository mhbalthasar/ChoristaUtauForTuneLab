using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoristaUtauApi.UPhonemizer
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class PhonemeizerAttribute(string type) : Attribute
    {
        public string PhonemizerType { get; private set; } = type;
    }
}
