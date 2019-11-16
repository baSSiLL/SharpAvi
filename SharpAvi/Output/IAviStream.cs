using System;
#if !NET35
using System.Diagnostics.Contracts;
#endif

namespace SharpAvi.Output
{
    /// <summary>
    /// A stream of AVI files.
    /// </summary>
#if !NET35
    [ContractClass(typeof(Contracts.AviStreamContract))]
#endif
    public interface IAviStream
    {
        /// <summary>
        /// Serial number of this stream in AVI file.
        /// </summary>
        int Index { get; }

        /// <summary>Name of the stream.</summary>
        /// <remarks>May be used by some players when displaying the list of available streams.</remarks>
        string Name { get; set; }
    }


#if !NET35
    namespace Contracts
    {
        [ContractClassFor(typeof(IAviStream))]
        internal abstract class AviStreamContract : IAviStream
        {
            public int Index
            {
                get 
                {
                    Contract.Ensures(Contract.Result<int>() >= 0);
                    throw new NotImplementedException(); 
                }
            }

            public string Name
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                }
            }
        }
    }
#endif
}
