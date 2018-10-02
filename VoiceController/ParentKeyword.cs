using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceController
{
    public class ParentKeyword
    {
        public string Keyword { get; set; }
        public List<ChildKeyword> Children { get; set; }
    }

}
