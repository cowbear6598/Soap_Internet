using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Soap.Internet
{
    public class MysqlManagerScriptableObject : ScriptableObject
    {
        public List<string> domainList;
        public int timeout = 30;
    }
}
