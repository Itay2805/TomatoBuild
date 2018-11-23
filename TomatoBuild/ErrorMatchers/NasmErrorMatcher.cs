using System;
using System.Collections.Generic;
using System.Text;

namespace TomatoBuild.ErrorMatchers
{
    public class NasmErrorMatcher : IErrorMatcher
    {

        public bool HasError(string output)
        {
            return output.Contains("error:");
        }

    }
}
