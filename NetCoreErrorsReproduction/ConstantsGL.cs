using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreErrorsReproduction
{
    public class ConstantsGL
    {
        /// <summary>
        /// The value that numeric uninitialized ML properties will have
        /// The type is "short" so that it can be converted to any other numeric type without explicit casting to make the code shorter
        /// </summary>
        public static readonly short UninitializedNumericMLProperty;

        /// <summary>
        /// The value that string uninitialized ML properties will have
        /// </summary>
        public static readonly string UninitializedStringMLProperty;

        /// <summary>
        /// The prefix added to log values.
        /// This prefix must be some string that will never appear in the log content.
        /// </summary>
        public static readonly string LogsValuesPrefix;

        static ConstantsGL()
        {
            ConstantsGL.UninitializedNumericMLProperty = -3;
            ConstantsGL.UninitializedStringMLProperty = "-3";
            ConstantsGL.LogsValuesPrefix = "@#####:";
        }
    }
}
