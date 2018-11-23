using System;
using System.Collections.Generic;
using System.Text;

namespace TomatoBuild.ErrorMatchers
{
    public class ErrorMatcherManager
    {

        public static Dictionary<string, IErrorMatcher> matchers = new Dictionary<string, IErrorMatcher>();

        static ErrorMatcherManager()
        {
            matchers["gcc_error_matcher"] = new GccErrorMatcher();
            matchers["nasm_error_matcher"] = new NasmErrorMatcher();
        }

    }
}
