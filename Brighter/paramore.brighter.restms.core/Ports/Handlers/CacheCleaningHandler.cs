﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using paramore.brighter.commandprocessor;
using paramore.brighter.commandprocessor.Logging;
using paramore.brighter.restms.core.Ports.Cache;
using paramore.brighter.restms.core.Ports.Commands;

namespace paramore.brighter.restms.core.Ports.Handlers
{
    public class CacheCleaningHandler : RequestHandler<InvalidateCacheCommand>
    {
        private readonly IAmACache _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestHandler{TRequest}"/> class.
        /// </summary>
        /// <param name="cache">The cache we should clear; implementors will need to wrap their cache in this</param>
        /// <param name="logger">The logger.</param>
        public CacheCleaningHandler(IAmACache cache, ILog logger) : base(logger)
        {
            _cache = cache;
        }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>TRequest.</returns>
        public override InvalidateCacheCommand Handle(InvalidateCacheCommand command)
        {
            _cache.InvalidateResource(command.ResourceToInvalidate);
            return base.Handle(command);
        }
    }
}