﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceController
{
    public class ParentKeyword : KeywordBase
    {
        public List<ChildKeyword> Children { get; set; }
    }

}
