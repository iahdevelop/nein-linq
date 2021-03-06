﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NeinLinq
{
    /// <summary>
    /// Proxy for query enumerator.
    /// </summary>
    public class RewriteEntityQueryEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> enumerator;

        /// <summary>
        /// Create a new enumerator proxy.
        /// </summary>
        /// <param name="enumerator">The actual enumerator.</param>
        public RewriteEntityQueryEnumerator(IEnumerator<T> enumerator)
        {
            if (enumerator == null)
                throw new ArgumentNullException(nameof(enumerator));

            this.enumerator = enumerator;
        }

        /// <inheritdoc />
        public T Current => enumerator.Current;

        /// <inheritdoc />
        public Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            return Task.FromResult(enumerator.MoveNext());
        }

        /// <summary>
        /// Releases all resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of the resources (other than memory).
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources;
        /// false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                enumerator.Dispose();
            }
        }
    }
}
