using System;
using System.Diagnostics.Contracts;

namespace SharpAvi.Output
{
    /// <summary>
    /// A stream of AVI files.
    /// </summary>
    [ContractClass(typeof(Contracts.AviStreamContract))]
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
}
