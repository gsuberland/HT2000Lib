using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HT2000Lib
{
    public class HT2000EventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the HT2000EventArgs class.
        /// </summary>
        /// <param name="state"></param>
        public HT2000EventArgs(HT2000State state)
        {
            State = state;
        }

        /// <summary>
        /// Gets the current HT2000 state.
        /// </summary>
        public HT2000State State { get; private set; }
    }
}
