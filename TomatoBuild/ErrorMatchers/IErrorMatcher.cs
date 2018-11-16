using System;
using System.Collections.Generic;
using System.Text;

namespace TomatoBuild.ErrorMatchers
{
    public interface IErrorMatcher
    {

        bool HasError(string output);

    }
}
