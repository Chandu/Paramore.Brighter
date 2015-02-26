﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using paramore.brighter.commandprocessor;

namespace paramore.brighter.serviceactivator.controlbus.Ports.Commands
{
    public class HeartBeatCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public HeartBeatCommand() : base(Guid.NewGuid())
        {
        }
    }
}