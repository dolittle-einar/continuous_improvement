/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using Dolittle.Domain;
using Dolittle.Runtime.Events;

namespace Domain.Improvements
{
    /// <summary>
    /// 
    /// </summary>
    public class Improvement : AggregateRoot
    {
        /// <inheritdoc/>
        Improvement(EventSourceId id) : base(id) { }

    }
}