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

        /// <summary>
        /// Gets the ratio from the field values
        /// </summary>
        /// <param name="numeratorField">The numerator field.</param>
        /// <param name="denominatorField">The denominator field.</param>
        /// <returns></returns>
        double GetRatio(string numeratorField, string denominatorField);

        /// <summary>
        /// Gets the ratio from the field and a double value
        /// </summary>
        /// <param name="numeratorField">The numerator field.</param>
        /// <param name="denominator">The denominator.</param>
        /// <returns></returns>
        double GetRatio(string numeratorField, double denominator);
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
        /// Gets the ratio from the field values
        /// </summary>
        /// <param name="numeratorField">The numerator field.</param>
        /// <param name="denominatorField">The denominator field.</param>
        /// <returns></returns>
        public double GetRatio(string numeratorField, string denominatorField)
        {
            double numerator = Record.GetFieldValue<double>(numeratorField, 0);
            double denominator = Record.GetFieldValue<double>(denominatorField, 0);
            return numerator/denominator;
        }

        /// <summary>
        /// Gets the ratio from the field and a double value
        /// </summary>
        /// <param name="numeratorField">The numerator field.</param>
        /// <param name="denominator">The denominator.</param>
        /// <returns></returns>
        public double GetRatio(string numeratorField, double denominator)
        {
            double numerator = Record.GetFieldValue<double>(numeratorField, 0);
            return numerator / denominator;
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