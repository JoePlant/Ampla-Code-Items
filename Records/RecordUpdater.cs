using System;
using System.Diagnostics;
using Citect.Ampla.Framework;
using Citect.Common;

namespace Code.Records
{
    /// <summary>
    ///     Interface to handle updating records
    /// </summary>
    public interface IRecordUpdater
    {
        void Update(IRecord record);
    }

    /// <summary>
    ///     Record Updater based class
    /// </summary>
    public abstract class RecordUpdater : IRecordUpdater
    {
        /// <summary>
        /// Called when the record is a new record.
        /// </summary>
        /// <param name="record">The record.</param>
        protected abstract void OnUpdateNewRecord(IRecord record);

        /// <summary>
        /// Called when the record is to be updated
        /// </summary>
        /// <param name="record">The record.</param>
        protected abstract void OnUpdateRecord(IRecord record);

        /// <summary>
        /// Updates the specified record.
        /// </summary>
        /// <param name="record">The record.</param>
        public void Update(IRecord record)
        {
            TraceInfo("{0} - {1}", record, this);
            if (record.IsNew)
            {
                OnUpdateNewRecord(record);
            }
            OnUpdateRecord(record);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return GetType().Name;
        }

        /// <summary>
        /// Handles the record changing event
        /// </summary>
        /// <param name="eventItem">The event item.</param>
        /// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void HandleRecordChanging(Item eventItem, EventArgs eventArgs)
        {
            new RecordChanging(this).Handle(eventItem, eventArgs);
        }

        /// <summary>
        /// Handles the record changed event
        /// </summary>
        /// <param name="eventItem">The event item.</param>
        /// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void HandleRecordChanged(Item eventItem, EventArgs eventArgs)
        {
            new RecordChanged(this).Handle(eventItem, eventArgs);
        }

        /// <summary>
        /// Writes a message to the trace  
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        protected void TraceInfo(string format, params object[] args)
        {
            Diagnostics.Write(TraceLevel.Info, format, args);
        }

        /// <summary>
        /// Writes an error to the trace
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        protected void TraceError(string format, params object[] args)
        {
            Diagnostics.Write(TraceLevel.Error, format, args);
        }
    }
}