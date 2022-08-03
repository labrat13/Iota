using System;

namespace MyCodeLibrary.SystemInfo
{
    /// <summary>
    /// Processor Object
    /// </summary>
    public class ProcessorObject
    {
        /// <summary>
        /// Processor Name
        /// </summary>
        public String Name { get; internal set; }
        /// <summary>
        /// Number Of Processor Cores
        /// </summary>
        public int Cores { get; internal set; }


        public ProcessorObject()
        {
            this.Cores = 0;
            this.Name = String.Empty;
        }
    }
}