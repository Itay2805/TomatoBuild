using System;
using System.Collections.Generic;
using System.Text;

namespace TomatoBuild.ErrorMatchers
{
    public class GccErrorMatcher : IErrorMatcher
    {

        public bool HasError(string output)
        {
            return output.Contains("compilation terminated.");
        }

    }
}
