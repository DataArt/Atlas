﻿//--------------------------------------------------------------------------------------------------
// Copyright 2018 DataArt, Inc.
// -------------------------------------------------------------------------------------------------
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this work except in compliance with the License.
// You may obtain a copy of the License in the LICENSE file, or at:
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//--------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Security.Claims;
using DataArt.Atlas.Messaging;

// todo
namespace DataArt.Atlas.Core.Shell
{
    internal static class StartupParameters
    {
        public static List<BusInitiator> BusInitiators { get; }

        public static IConsumerRegistrator ConsumerRegistrator { get; set; }

        public static Func<string, Claim> SecurityConfiguration { get; set; }

        static StartupParameters()
        {
            BusInitiators = new List<BusInitiator>();
        }
    }
}
