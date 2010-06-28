using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using log4net;
using NetGore.IO;

namespace NetGore.Network
{
    /// <summary>
    /// Provides statistics tracking for <see cref="IMessageProcessor"/>s. For received data, this is implemented for you
    /// by the <see cref="StatMessageProcessorManager"/> class.
    /// </summary>
    public class MessageProcessorStatistics : IMessageProcessorStatistics
    {
        static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        readonly IMessageProcessorManager _mpm;
        readonly ProcStats[] _stats = new ProcStats[MessageProcessor.MaxProcessorID + 1];

        string _outFilePath;
        TickCount _nextDumpTime;
        int _outDumpRate;

        /// <summary>
        /// Turns off automatic file output for the statistics.
        /// </summary>
        public void DisableFileOutput()
        {
            _outFilePath = null;
        }

        /// <summary>
        /// Gets or sets the <see cref="GenericValueIOFormat"/> to use for when an instance of this class
        /// writes itself out to a new <see cref="GenericValueWriter"/>. If null, the format to use
        /// will be inherited from <see cref="GenericValueWriter.DefaultFormat"/>.
        /// Default value is <see cref="GenericValueIOFormat.Xml"/>.
        /// </summary>
        public static GenericValueIOFormat? EncodingFormat { get; set; }

        /// <summary>
        /// Gets if file output is currently enabled.
        /// </summary>
        public bool IsFileOutputEnabled { get { return _outFilePath != null; } }

        /// <summary>
        /// Turns on file output for the statistics.
        /// </summary>
        /// <param name="filePath">The output file path.</param>
        /// <param name="dumpRate">The frequency, in milliseconds, of writing the output. Default value is 10 seconds.</param>
        public void EnableFileOutput(string filePath, int dumpRate = 10000)
        {
            _outFilePath = filePath;
            _outDumpRate = dumpRate;

            _nextDumpTime = TickCount.Now;
        }

        /// <summary>
        /// Writes the stats to a file.
        /// </summary>
        /// <param name="filePath">The file path to write to.</param>
        public void Write(string filePath)
        {
            using (var writer = new GenericValueWriter(filePath, _rootNodeName, EncodingFormat))
            {
                Write(writer);
            }
        }

        /// <summary>
        /// Initializes the <see cref="StatMessageProcessorManager"/> class.
        /// </summary>
        static MessageProcessorStatistics()
        {
            EncodingFormat = GenericValueIOFormat.Xml;
        }

        const string _rootNodeName = "MessageProcessorStats";

        /// <summary>
        /// Writes the statistics to an <see cref="IValueWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="IValueWriter"/> to write to.</param>
        public void Write(IValueWriter writer)
        {
            foreach (var stat in GetAllStats())
            {
                var nodeName = "ID" + stat.Key;

                writer.WriteStartNode(nodeName);

                stat.Value.Write(writer);

                writer.WriteEndNode(nodeName);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageProcessorStatistics"/> class.
        /// </summary>
        /// <param name="mpm">The <see cref="IMessageProcessorManager"/> to track the statistics of.</param>
        public MessageProcessorStatistics(IMessageProcessorManager mpm)
        {
            _mpm = mpm;
        }

        /// <summary>
        /// Gets the <see cref="IMessageProcessorManager"/> that these statistics are for.
        /// </summary>
        public IMessageProcessorManager MessageProcessorManager
        {
            get { return _mpm; }
        }

        /// <summary>
        /// Updates the statistics for an <see cref="IMessageProcessor"/>.
        /// </summary>
        /// <param name="processorID">The ID of the <see cref="IMessageProcessor"/>.</param>
        /// <param name="bits">The number of bits read or written. Does not include the message ID.</param>
        public void HandleProcessorInvoked(byte processorID, int bits)
        {
            var ubits = (ushort)bits;

            // Check the bitsRead range
            if (bits < 0)
            {
                const string errmsg = "Invalid bitsRead value `{0}`.";
                if (log.IsErrorEnabled)
                    log.ErrorFormat(errmsg, bits);
                return;
            }

            if (bits < 0)
            {
                const string errmsg =
                    "Invalid bitsRead value `{0}` - value was too large to handle. Will use the largest possible value instead ({1}).";
                if (log.IsErrorEnabled)
                    log.ErrorFormat(errmsg, bits, ushort.MaxValue);
                ubits = ushort.MaxValue;
            }

            // Grab the stats
            var stats = _stats[processorID];

            // Create the stats if this is the first call to this processor
            if (stats == null)
            {
                stats = new ProcStats(processorID);
                _stats[processorID] = stats;
            }

            // Update the stats
            stats.NotifyCalled(ubits);

            // Check to dump the output
            if (_outFilePath != null && _nextDumpTime <= TickCount.Now)
            {
                _nextDumpTime = (TickCount)(TickCount.Now + _outDumpRate);
                Write(_outFilePath);
            }
        }

        #region IMessageProcessorStatistics Members

        /// <summary>
        /// Gets all of the <see cref="IMessageProcessorStats"/> paired with their corresponding <see cref="IMessageProcessor"/> ID.
        /// </summary>
        public IEnumerable<KeyValuePair<byte, IMessageProcessorStats>> GetAllStats()
        {
            Debug.Assert(_stats.Length - 1 <= byte.MaxValue);

            for (var i = 0; i < _stats.Length; i++)
            {
                if (_stats[i] != null)
                    yield return new KeyValuePair<byte, IMessageProcessorStats>((byte)i, _stats[i]);
            }
        }

        /// <summary>
        /// Gets the <see cref="IMessageProcessorStats"/> for a <see cref="IMessageProcessor"/> identified by the ID.
        /// </summary>
        /// <param name="msgID">The ID of the <see cref="IMessageProcessor"/> to get the stats for.</param>
        /// <returns>The <see cref="IMessageProcessorStats"/> for the given <paramref name="msgID"/>, or null if no
        /// stats exist for the given <paramref name="msgID"/>.</returns>
        public IMessageProcessorStats GetStats(byte msgID)
        {
            return _stats[msgID];
        }

        #endregion

        /// <summary>
        /// Contains the statistics for a single <see cref="IMessageProcessor"/>.
        /// </summary>
        class ProcStats : IMessageProcessorStats
        {
            readonly byte _id;

            uint _calls;
            ushort _max;
            ushort _min;
            uint _totalBits;

            /// <summary>
            /// Initializes a new instance of the <see cref="ProcStats"/> class.
            /// </summary>
            /// <param name="id">The <see cref="IMessageProcessor"/> ID.</param>
            public ProcStats(byte id)
            {
                _id = id;
            }

            /// <summary>
            /// Notifies this <see cref="ProcStats"/> that a call was made to it, and makes the appropriate updates to the statistics.
            /// </summary>
            /// <param name="bits">The length of the message in bits.</param>
            public void NotifyCalled(ushort bits)
            {
                _calls++;
                _totalBits += bits;

                if (bits < Min || Min == 0)
                    _min = bits;

                if (bits > Max)
                    _max = bits;
            }

            #region IMessageProcessorStats Members

            /// <summary>
            /// Gets the number of calls that have been made to this processor.
            /// </summary>
            public uint Calls
            {
                get { return _calls; }
            }

            /// <summary>
            /// Gets the length of the longest message read by this processor.
            /// </summary>
            public ushort Max
            {
                get { return _max; }
            }

            /// <summary>
            /// Gets length of the shortest message read by this processor.
            /// </summary>
            public ushort Min
            {
                get { return _min; }
            }

            /// <summary>
            /// Gets the ID of the <see cref="IMessageProcessor"/> that these stats are for.
            /// </summary>
            public byte ProcessorID
            {
                get { return _id; }
            }

            /// <summary>
            /// Gets the total bits read by this processor.
            /// </summary>
            public uint TotalBits
            {
                get { return _totalBits; }
            }

            /// <summary>
            /// Writes the <see cref="IMessageProcessorStats"/> to an <see cref="IValueWriter"/>.
            /// </summary>
            /// <param name="writer">The <see cref="IValueWriter"/> to write to.</param>
            public void Write(IValueWriter writer)
            {
                writer.Write("ProcessorID", ProcessorID);
                writer.Write("Calls", Calls);
                writer.Write("TotalBits", TotalBits);
                writer.Write("Min", Min);
                writer.Write("Max", Max);
            }

            #endregion
        }
    }
}