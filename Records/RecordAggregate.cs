using System;

namespace Code.Records
{
    /// <summary>
    ///     Interface for calculating aggregates of a record
    /// </summary>
    public interface IRecordAggregate : IDisposable
    {
        /// <summary>
        /// Sums the values of the field names 
        /// </summary>
        /// <param name="start">The starting point</param>
        /// <param name="fields">Array of fields that will be updated.</param>
        /// <returns></returns>
        double SumValues(double start, params string[] fields);
    }

    /// <summary>
    ///     Provides the ability to calculate aggregates of a record
    /// </summary>
    public class RecordAggregate : IRecordAggregate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecordAggregate"/> class.
        /// </summary>
        /// <param name="record">The record.</param>
        public RecordAggregate(IRecord record)
        {
            Record = record;
        }

        /// <summary>
        /// Gets the record.
        /// </summary>
        /// <value>
        /// The record.
        /// </value>
        protected IRecord Record { get; private set; }

        /// <summary>
        /// Sums the values of the field names
        /// </summary>
        /// <param name="start">The starting point</param>
        /// <param name="fields">Array of fields that will be updated.</param>
        /// <returns></returns>
        public double SumValues(double start, params string[] fields)
        {
            double result = start;
            foreach (string fieldName in fields)
            {
                result += Record.GetFieldValue<double>(fieldName, 0.0D);
            }
            return result;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Record = null;
        }
    }
}