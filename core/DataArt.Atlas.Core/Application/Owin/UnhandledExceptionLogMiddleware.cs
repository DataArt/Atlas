#region License
// =================================================================================================
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
// =================================================================================================
#endregion

#if NET452
using System;
using System.Threading.Tasks;
using DataArt.Atlas.Core.Application.Logging;
using Microsoft.Owin;
using Serilog;

namespace DataArt.Atlas.Core.Application.Owin
{
    public class UnhandledExceptionLogMiddleware : OwinMiddleware
    {
        public UnhandledExceptionLogMiddleware(OwinMiddleware next) : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            try
            {
                await Next.Invoke(context);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Unhandled exception: {@OwinUnhandledExceptionEvent}", new OwinUnhandledExceptionEvent(context.Request, context.Response));
            }
        }
    }
}
#endif